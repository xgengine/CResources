using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine.SceneManagement;

namespace H3D.CResources
{

    public class CResource : CustomYieldInstruction
    {
        List<AsyncOperation> m_AsyncOperations=new List<AsyncOperation>();

        public List<AsyncOperation> AsyncOperations
        {
            get
            {
                return m_AsyncOperations;
            }
        }
        public void AddAsyncOperation(AsyncOperation op)
        {
            m_AsyncOperations.Add(op);
        }


        protected object m_Content;

        protected System.WeakReference m_Reference;

        protected IResourceLocation m_Location;

        public object Content
        {
            get
            {
                return m_Reference.Target;
            }
        }

        public IResourceLocation Location
        {
            get
            {
                return m_Location;
            }
        }

        public bool IsAlive
        {
            get
            {
                return m_Reference.IsAlive;
            }
        }

        protected bool m_KepWaiting = true;

        public override bool keepWaiting
        {
            get
            {

                for(int i =m_AsyncOperations.Count-1;i>=0;i--)
                {
                    if(m_AsyncOperations[i].isDone)
                    {
                        m_AsyncOperations.RemoveAt(i);
                    }
                }
                if(m_AsyncOperations.Count == 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        public CResource(object obj, IResourceLocation location)
        {
            m_Location = location;
            m_Reference = new System.WeakReference(obj);

        }

        public CResource Retain()
        {
            m_RefCount++;
            return this;
        }

        public void Release()
        {
            m_RefCount--;
        }

        public event System.Action<CResource> Completed;

        internal int m_RefCount = 0;
    }


    public class CResources
    {
        static CResourcesLoadRecorder m_LoadRecorder;

        static List<IResourceLocator> m_Locators;

        static List<IResourceProvider> m_ResouceProviders;

        static CResources()
        {    
        }

        [RuntimeInitializeOnLoadMethod]
        private static void RuntimeInitialization()
        {
            LogUtility.Log("Runtime Initializtion");
            m_LoadRecorder = new CResourcesLoadRecorder();

            m_Locators = new List<IResourceLocator>();

            //m_Locators.Add(new LocalAssetCResourceLocator());
            m_Locators.Add(new BundleAssetCResourceLocator());

            m_ResouceProviders = new List<IResourceProvider>();

            m_ResouceProviders.Add(new CResourcePoolProvider(new BundleAssetCResourceProvider()));
            m_ResouceProviders.Add(new CResourcePoolProvider(new LocalBundleCResourceProvider()));

            // m_ResouceProviders.Add(new LocalAssetCResourceProvider());
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        static void OnSceneUnloaded(Scene scene)
        {
            UnloadUnusedAssets();
        }

        public static AsyncOperation UnloadUnusedAssets()
        {
            AsyncOperation operation = Resources.UnloadUnusedAssets();
            operation.completed += (p) =>
            {
                UnloadUnusedAssetsInternal();
            };
            return operation;
        }

        internal static void UnloadUnusedAssetsInternal()
        {
            float t = Time.realtimeSinceStartup;
            for (int i = 0; i < m_ResouceProviders.Count; i++)
            {
                System.GC.Collect();
                IResourceProvider provider = m_ResouceProviders[i];
                provider.UnloadUnusedAssets();
            }
            LogUtility.Log("[UnloadUnusedAssetsInternal ] is Done use time {0}", (Time.realtimeSinceStartup - t));
        }

        public static TObject Load<TObject>(string requestID) where TObject : class
        {
            try
            {
                IResourceLocation location = Locate<TObject>(requestID);
                return Load<TObject>(location);
            }
            catch (CResourcesException e)
            {
                LogUtility.LogError(e.Message);
            }
            return null;
        }

        public static CResource LoadAsync<TObject>(string requestID) where TObject:class
        {
            IResourceLocation location = Locate<TObject>(requestID);
            return LoadAsync<TObject>(location);
        }

        public static TObject Instantiate<TObject>(TObject obj) where TObject :Object
        {
            IResourceLocation location = m_LoadRecorder.LocateResource(obj);
            if(location !=null)
            {
                TObject asset = Load<TObject>(location);        
                Object instance = Object.Instantiate(obj);
                m_LoadRecorder.RecordInstance(asset, instance);
                return instance as TObject;
            }
            return null;
        }

        public static void Destroy(object obj)
        {
            IResourceLocation location = m_LoadRecorder.LocateResource(obj);
            if (location != null)
            {
                Release<object>(location, obj);
                if (m_LoadRecorder.IsInstance(obj))
                {
                    m_LoadRecorder.RemoveInstance(obj);
                    Object.Destroy(obj as Object);
                }
            }
            else
            {
                LogUtility.LogError(" CResource.Destory only can destory the asset and instance Load or Instantiate by CResources API ");
            }
        }

        internal static TObject Load<TObject>(IResourceLocation location) where TObject : class
        {
            try
            {
                IResourceProvider provider = GetResourceProvider<TObject>(location);
                TObject result = provider.Provide<TObject>(location);
                m_LoadRecorder.RecordResource(location,result);
                return result;
            }
            catch (CResourcesException e)
            {
                LogUtility.LogError(e.Message);
            }
            return null;
        }

        internal static CResource LoadAsync<TObject>(IResourceLocation location) where TObject : class
        {
            try
            {
                IResourceProvider provider = GetResourceProvider<TObject>(location);
                CResource result = provider.ProvideAsync<TObject>(location);
                m_LoadRecorder.RecordResource(location, result);
                return result;
            }
            catch (CResourcesException e)
            {
                LogUtility.LogError(e.Message);
            }
            return null;
        }


        internal static void Release<TObject>(IResourceLocation location,object asset) where TObject:class
        {
            IResourceProvider provider = GetResourceProvider<TObject>(location);
            int key = CResourcesLoadRecorder.GetInstanceID(asset);
            if(provider.Release(location, asset))
            {
                m_LoadRecorder.RemoveResource(key);
                if(location.HasDependencies)
                {
                    for(int i =0;i<location.Dependencies.Count;i++)
                    {
                        Release<object>(location.Dependencies[i], null);
                    }
                    
                }
            }
        }

        private static IResourceLocation Locate<TObject>(string requestID) where TObject : class
        {
            IResourceLocation location;
            for (int i = 0; i < m_Locators.Count; i++)
            {
                IResourceLocator locator = m_Locators[i];
                location = locator.Locate<TObject>(requestID);
                if (location != null)
                {
                    return location;
                }
            }
            throw new CanNotLocateExcption (requestID);
        }

        private static IResourceProvider GetResourceProvider<TObject>(IResourceLocation location)where TObject :class
        {       
            foreach (IResourceProvider provider in m_ResouceProviders)
            {
                if (provider.CanProvide<TObject>(location))
                {
                    return provider;
                }
            }
            throw  new UnknownResourceProviderException(location);
        }

        internal class CResourcesLoadRecorder
        {
            internal Dictionary<int, IResourceLocation> m_LoadedCResources = new Dictionary<int, IResourceLocation>();

            internal Dictionary<int, int> m_InstanceResources = new Dictionary<int, int>();

            internal void RecordResource(IResourceLocation location, object asset)
            {
                if (asset == null || location == null)
                {
                    LogUtility.LogError("RecordResource Error ");
                    return;
                }
                int key = GetInstanceID(asset);
                if (key != -1)
                {
                    if (!m_LoadedCResources.ContainsKey(key))
                    {
                        m_LoadedCResources.Add(key, location);
                    }
                }
            }

            internal IResourceLocation LocateResource(object asset)
            {
                int key = GetInstanceID(asset);
                if(m_LoadedCResources.ContainsKey(key))
                {
                    return m_LoadedCResources[key];
                }
                else if(m_InstanceResources.ContainsKey(key))
                {
                    return m_LoadedCResources[m_InstanceResources[key]];
                }
                return null;
            }

            internal void RecordInstance(object asset, object instance)
            {
                if (asset == null || instance == null)
                {
                    LogUtility.LogError("RecordInstance Error ");
                    return;
                }
                m_InstanceResources.Add(GetInstanceID(instance),GetInstanceID(asset));
            }

            internal bool IsInstance(object asset)
            {
                return m_InstanceResources.ContainsKey(GetInstanceID(asset));
            }

            internal bool IsAsset(object asset)
            {
                return m_LoadedCResources.ContainsKey(GetInstanceID(asset));
            }

            internal bool RemoveResource(int key)
            {
                if (key != -1)
                {
                    if (m_LoadedCResources.ContainsKey(key))
                    {
                        m_LoadedCResources.Remove(key);
                        return true;
                    }
                  
                }
                return false;
            }

            internal bool RemoveInstance(object asset)
            {
                int key = GetInstanceID(asset);
                if (m_InstanceResources.ContainsKey(key))
                {
                    m_InstanceResources.Remove(key);
                    return true;
                }
                return false;
            }

            internal static int GetInstanceID(object asset)
            {
                if (asset == null)
                {
                    return -1;
                }
                Object unityObject = asset as Object;
                if (unityObject != null)
                {
                    if (unityObject is AssetBundle)
                    {
                        return -1;
                    }
                    return unityObject.GetInstanceID();
                }
                else
                {
                    return asset.GetHashCode();
                }
            }
        }
    }

    public interface IResourceLocator
    {
        IResourceLocation Locate<TObject>(string requestI)
        where TObject :class;
    }

    public interface IResourceLocation
    {

        string InternalId { get; }

        string ProviderId { get; }

        IList<IResourceLocation> Dependencies { get; }

        bool HasDependencies { get; }
    }

    public interface IResourceProvider
    {

        string ProviderId { get; }

        TObject Provide<TObject>(IResourceLocation location)
        where TObject : class;


        CResource ProvideAsync<TObject>(IResourceLocation location)
        where TObject : class;

        bool CanProvide<TObject>(IResourceLocation location)
        where TObject : class;

        void UnloadUnusedAssets();

        bool Release(IResourceLocation location, object asset);
    }


    public abstract class CResourceProvider : IResourceProvider
    {
        protected CResourceProvider() { }

        public virtual string ProviderId
        {
            get { return GetType().FullName; }
        }

        public virtual bool CanProvide<TObject>(IResourceLocation location) where TObject : class
        {
            if (location == null)
                throw new System.ArgumentException("IResourceLocation location cannot be null.");
            return ProviderId.Equals(location.ProviderId, System.StringComparison.Ordinal);
        }

        public override string ToString()
        {
            return string.Format("[{0}]", ProviderId);
        }

        public abstract TObject Provide<TObject>(IResourceLocation location) where TObject : class;
        public virtual CResource ProvideAsync<TObject>(IResourceLocation location) where TObject : class
        {
               return null;
        }
        public virtual void UnloadUnusedAssets()
        {

        }

        public virtual bool Release(IResourceLocation location, object asset)
        {
            if (location == null)
                throw new System.ArgumentNullException("location");
            if (asset == null)
                throw new System.ArgumentNullException("asset");

            return true;
        }
    }

    

    public class CResourcePoolProvider : CResourceProvider
    {
        protected  Dictionary<int, CResource> m_Cache = new Dictionary<int, CResource>();

        protected IResourceProvider m_Provider;

        public CResourcePoolProvider(IResourceProvider provider,int maxSize =0)
        {
            m_Provider = provider;
        }

        public override bool CanProvide<TObject>(IResourceLocation location) 
        {
            return m_Provider.CanProvide<TObject>(location);
        }

        public override TObject Provide<TObject>(IResourceLocation location) 
        {
            TObject result = null;
            int key = location.GetHashCode();
            if(m_Cache.ContainsKey(key))
            {
                CResource res = m_Cache[key];
                if(res.IsAlive)
                {
                    result = res.Retain().Content as TObject;

                    LogUtility.Log(location.InternalId + " " + location.ProviderId + " " + result);
                    return result;
                }
                else
                {
                    LogUtility.LogError(location.InternalId +"  not alive");
                    m_Cache.Remove(key);
                }                  
            }

            result = m_Provider.Provide<TObject>(location);
            m_Cache.Add(key, new CResource(result,location).Retain());
                
            LogUtility.Log(location.InternalId + " " + location.ProviderId+" "+result);
            return result;
        }

        public override bool Release(IResourceLocation location, object asset)
        {
            int key = location.GetHashCode();
          
            if (m_Cache.ContainsKey(key))
            {
                CResource res = m_Cache[key];
                res.Release();
                if(res.m_RefCount == 0)
                {
                    m_Cache.Remove(key);
                    m_Provider.Release(location, res.Content);           
                    return true;
                }        
            }
            return false ;
        }

        public override void UnloadUnusedAssets()
        {

            List<int> assetsNotAlive = new List<int>();

            foreach (var item in m_Cache)
            {
                CResource res = item.Value;
                LogUtility.Log("[CResources.UnloadUnusedAssets]{0}  {1}", item.Value.Location.InternalId, item.Value.IsAlive);
                if(res.IsAlive == false)
                {
                    IResourceLocation location = res.Location;
                    if(location.HasDependencies)
                    {
                        for (int i = 0; i < location.Dependencies.Count; i++)
                        {
                            CResources.Release<object>(location.Dependencies[i], null);
                        }
                    }
                    assetsNotAlive.Add(item.Key);
                }          
            }
            for(int i =0;i<assetsNotAlive.Count;i++)
            {
                m_Cache.Remove(assetsNotAlive[i]);
            }
        }
    }

    public class LocalBundleCResourceProvider  : CResourceProvider
    {
        public override TObject Provide<TObject>(IResourceLocation location)
        {
            if(location.HasDependencies)
            {
                for(int i = 0;i<location.Dependencies.Count;i++)
                {
                    CResources.Load<AssetBundle>(location.Dependencies[i]);
                }
            }
            LogUtility.Log("[Load bundle] " + location.InternalId);
            AssetBundle bundle = AssetBundle.LoadFromFile(Path.Combine( Path.GetDirectoryName( Application.dataPath),"assetbundles/cresources/" +location.InternalId));
           
            return bundle as TObject;
        }

        public override CResource ProvideAsync<TObject>(IResourceLocation location)
        {
            if (location.HasDependencies)
            {
                for (int i = 0; i < location.Dependencies.Count; i++)
                {
                    CResources.LoadAsync<AssetBundle>(location.Dependencies[i]);
                }
            }
            LogUtility.Log("[Load bundle] " + location.InternalId);

            AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(Path.Combine(Path.GetDirectoryName(Application.dataPath), "assetbundles/cresources/" + location.InternalId));

            return null;
        }


        public override bool Release(IResourceLocation location, object asset)
        {
            if (location == null)
                throw new System.ArgumentNullException("location");
            if (asset == null)
                throw new System.ArgumentNullException("asset");
            var bundle = asset as AssetBundle;
            if (bundle != null)
            {
                LogUtility.Log("Release bundle "+location.InternalId);
                bundle.Unload(true);
                return true;
            }

            return false;
        }
    }

    public class BundleAssetCResourceProvider : CResourceProvider
    {
        public override TObject Provide<TObject>(IResourceLocation location)
        {
            if (location.HasDependencies)
            {
                IResourceLocation bundleLocation = location.Dependencies[0];
                AssetBundle bundle = CResources.Load<AssetBundle>(bundleLocation);
                LogUtility.Log("Load asset from bundle " + location.InternalId);
                return bundle.LoadAsset(Path.GetFileName(location.InternalId), typeof(TObject)) as TObject;
            }
            else
            {
                throw new CResourcesException(" Can not find bundle Location " + location.InternalId);
            }

        }
        public override bool Release(IResourceLocation location, object asset)
        {
            if (location == null)
                throw new System.ArgumentNullException("location");
            return true;
        }
    }

#if UNITY_EDITOR
    public class LocalAssetCResourceProvider : CResourceProvider
    {

        public override TObject Provide<TObject>(IResourceLocation location)
        {
            TObject asset = UnityEditor.AssetDatabase.LoadAssetAtPath(location.InternalId,typeof(TObject)) as TObject;
            if (asset == null)
            {
                throw new ResourceProviderFailedException(this, location);
            }
            return asset;
        }

        public override bool Release(IResourceLocation location, object asset)
        {

            if (location == null)
                throw new System.ArgumentNullException("location");
            var go = asset as GameObject;
            if (go != null)
            {
                //GameObjects cannot be resleased via Object.Destroy because they are considered an asset
                //but they can't be unloaded via Resources.UnloadAsset since they are NOT an asset?
                return true;
            }
            var obj = asset as Object;
            if (obj != null)
            {
                Resources.UnloadAsset(obj);
                return true;
            }
            return true;
        }
    }
#endif

    public class BundleAssetCResourceLocation : IResourceLocation
    {
        string m_id;
        string m_providerId;
        List<IResourceLocation> m_dependencies;
        public string InternalId { get { return m_id; } }
        public string ProviderId { get { return m_providerId; } }
        public IList<IResourceLocation> Dependencies { get { return m_dependencies; } }
        public bool HasDependencies { get { return m_dependencies != null && m_dependencies.Count > 0; } }
        public BundleAssetCResourceLocation(string id, string providerId, params IResourceLocation[] dependencies)
        {
            if (string.IsNullOrEmpty(id))
                throw new System.ArgumentNullException(id);
            if (string.IsNullOrEmpty(providerId))
                throw new System.ArgumentNullException(providerId);
            m_id = id;
            m_providerId = providerId;
            m_dependencies = new List<IResourceLocation>(dependencies);
        }
        public void AddDependencies(IResourceLocation[] dependencies)
        {
            m_dependencies = new List<IResourceLocation>(dependencies);
        }
        public override int GetHashCode()
        {
            return m_providerId.GetHashCode();
        }
    }

    public class BundleCResourceLocation:IResourceLocation
    {
        string m_id;
        string m_providerId;
        List<IResourceLocation> m_dependencies;
        public string InternalId { get { return m_id; } }
        public string ProviderId { get { return m_providerId; } }
        public IList<IResourceLocation> Dependencies { get { return m_dependencies; } }
        public bool HasDependencies { get { return m_dependencies != null && m_dependencies.Count > 0; } }
        public BundleCResourceLocation(string id, string providerId, params IResourceLocation[] dependencies)
        {
            if (string.IsNullOrEmpty(id))
                throw new System.ArgumentNullException(id);
            if (string.IsNullOrEmpty(providerId))
                throw new System.ArgumentNullException(providerId);
            m_id = id;
            m_providerId = providerId;
            m_dependencies = new List<IResourceLocation>(dependencies);
        }
        public void AddDependencies(IResourceLocation[] dependencies)
        {
            m_dependencies = new List<IResourceLocation>(dependencies);
        }
    }

#if UNITY_EDITOR
    public class LocalAssetCResourceLocation : IResourceLocation
    {
        string m_InternalId;
        string m_ProviderId;
        public string InternalId { get { return m_InternalId; } }
        public string ProviderId { get { return m_ProviderId; } }
        public IList<IResourceLocation> Dependencies { get { return null; } }
        public bool HasDependencies { get { return false; } }

        public LocalAssetCResourceLocation(string internalId, string providerId)
        {
            if (string.IsNullOrEmpty(internalId))
                throw new System.ArgumentNullException(internalId);
            if (string.IsNullOrEmpty(providerId))
                throw new System.ArgumentNullException(providerId);
            m_InternalId = internalId;
            m_ProviderId = providerId;
        }
    }
#endif

    public abstract class CResourceLocator : IResourceLocator
    {
        protected static Dictionary<int, IResourceLocation> m_Locations;

        public abstract IResourceLocation Locate<TObject>(string requestID) where TObject : class;

        public static string AssetPathToLoadPath(string assetPath)
        {
            string noExtPath = CRUtlity.DeleteExtension(assetPath);
            return noExtPath.ToLower().Replace(ConstValue.m_PackPath + "/", "");
        }

        public static int Lcation(string loadPath, System.Type type)
        {
            string internalLoadPath = string.Concat(loadPath.ToLower(), type.Name);
            return internalLoadPath.GetHashCode();

        }
    }

#if UNITY_EDITOR
    public class LocalAssetCResourceLocator : CResourceLocator
    {
      
        public LocalAssetCResourceLocator()
        {
            m_Locations = new Dictionary<int, IResourceLocation>();
            IEnumerable<string> paths = Directory.GetFiles(ConstValue.m_PackPath, "*.*", SearchOption.AllDirectories).Where(
                   p => p.EndsWith(".meta") == false && p.EndsWith(".cs") == false
               );
            foreach (var wholeFilePath in paths)
            {
                string assetPath = CRUtlity.FullPathToAssetPath(wholeFilePath);
                string guid =  UnityEditor.AssetDatabase.AssetPathToGUID(assetPath);
                System.Type type = UnityEditor.AssetDatabase.GetMainAssetTypeAtPath(assetPath);
                int location = Lcation(AssetPathToLoadPath(assetPath), type);

                m_Locations.Add(location,new LocalAssetCResourceLocation(assetPath,(typeof(LocalAssetCResourceProvider)).FullName));
            }
        }

        public override IResourceLocation Locate<TObject>(string requestID)
        {
            IResourceLocation assetLocation = null;
            int hashCode = Lcation(requestID, typeof(TObject));
            if (m_Locations.ContainsKey(hashCode))
            {
                assetLocation = m_Locations[hashCode];
            }
            return assetLocation;
        }   
    }
#endif

    public class BundleAssetCResourceLocator : CResourceLocator
    {

        public BundleAssetCResourceLocator()
        {

            ReadLoactionData(Path.Combine(Path.GetDirectoryName(Application.dataPath), ConstValue.m_BundlePath + "/" + ConstValue.m_LocationName));
        }

        public void ReadLoactionData(string dataPath)
        {
            using (Stream stream = new FileStream(dataPath, FileMode.Open))
            {
                using (var br = new BinaryReader(stream))
                {
                    int count = br.ReadInt32();
                    m_Locations = new Dictionary<int, IResourceLocation>(count);
                    List<BundleCResourceLocation> tempLocations = new List<BundleCResourceLocation>(count);
                    for (int i = 0; i < count; i++)
                    {
                        int hashCode = br.ReadInt32();
                        string bundleName = br.ReadString();
                        BundleCResourceLocation cLocation = new BundleCResourceLocation(bundleName, typeof(LocalBundleCResourceProvider).FullName );
                        m_Locations.Add(hashCode, cLocation);
                        tempLocations.Add(cLocation);
                    }

                    for (int i = 0; i < count; i++)
                    {
                        int depCount = br.ReadInt32();
                        BundleCResourceLocation[] dependencies = new BundleCResourceLocation[depCount];
                        for (int k = 0; k < depCount; k++)
                        {
                            dependencies[k] = m_Locations[br.ReadInt32()] as BundleCResourceLocation;
                        }
                        tempLocations[i].AddDependencies(dependencies);
                    }
                    tempLocations.Clear();
                }
            }
        }

        public override IResourceLocation Locate<TObject>(string requestID)
        {
            IResourceLocation assetLocation = null;

            int hashCode = Lcation(requestID, typeof(TObject));

            if (m_Locations.ContainsKey(hashCode))
            {
                assetLocation = new BundleAssetCResourceLocation(requestID, typeof(BundleAssetCResourceProvider).FullName, new IResourceLocation[] { m_Locations[hashCode] });                 
            }

            return assetLocation;
        }

    }
}