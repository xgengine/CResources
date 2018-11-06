using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine.SceneManagement;

namespace H3D.CResources
{




    public class CResourceRequestBase : CustomYieldInstruction
    {
        protected bool m_KeepWaiting = true;

        public override bool keepWaiting
        {
            get
            {
                return m_KeepWaiting;
            }
        }

        public CResourceRequestBase Retain()
        {
           
            m_RefCount++;
            LogUtility.Log(m_Location.InternalId + "  +1 " + m_RefCount);
            return this;
        }

        public void Release()
        {
            m_RefCount--;
            LogUtility.Log(m_Location.InternalId + " -1 " + m_RefCount);
        }


        protected bool m_IsDone= false;

        public bool IsDone
        {
            get
            {
                return m_IsDone;
            }
        }

        public bool IsAlive
        {
            get
            {
                if (m_IsDone)
                {
                    return m_Reference.IsAlive;
                }
                else
                {
                    return true;
                }
               
            }
        }

        protected System.WeakReference m_Reference;

        internal int m_RefCount = 0;

        public object Content
        {
            get
            {
                return m_Reference.Target;
            }
        }

        public void SetContent(object obj)
        {
            m_IsDone = true;
            if(m_Reference != null && m_Reference.Target !=null)
            {
                return;
            }
            m_Reference = new System.WeakReference(obj);

        }

        protected IResourceLocation m_Location;

        public IResourceLocation Location
        {
            get
            {
                return m_Location;
            }
        }

        public event System.Action<CResourceRequestBase> Completed;

        public void Notify()
        {
            if(Completed !=null)
            {
                Completed(this);
            }

        }
    
        protected virtual void LoadingInternal()
        {  
            
        }
        public void LoadImmediate()
        {
            LoadingInternal();
        
        }
    }

    public class CResourceRequest<T> : CResourceRequestBase where T : class
    {
        public T Content
        {
            get
            {
                if(m_Reference !=null)
                {
                    return m_Reference.Target as T;
                }
                else
                {
                    return null;
                }
               
            }
        }
        public CResourceRequest<T> SendRequestAsync(IResourceLocation location)
        {
            m_Location = location;
            TaskManager.Instance.StartTask(LoadingAsyncInternal());
            return this;
        }

        public CResourceRequest<T> SendRequest(IResourceLocation location)
        {
            m_Location = location;
            LoadingInternal();
            return this;
        }

        protected virtual IEnumerator LoadingAsyncInternal()
        {
            Notify();
            m_IsDone = true;
            m_KeepWaiting = false;
  
            yield break;           
        }

        protected  override void LoadingInternal()
        {
            Notify();
            m_IsDone = true;
            m_KeepWaiting = false;
        }
    
    }

    public class CResourceBundleCreateRequest<T> : CResourceRequest<T> where T : class
    {
        protected override IEnumerator LoadingAsyncInternal()
        {
            List<CResourceRequest<AssetBundle>> depRequests = new List<CResourceRequest<AssetBundle>>();

            if (m_Location.HasDependencies)
            {
                for (int i = 0; i < m_Location.Dependencies.Count; i++)
                {
                    depRequests.Add(CResources.LoadAsync<AssetBundle>(m_Location.Dependencies[i]));
                }
            }

            for (int i = 0; i < depRequests.Count; i++)
            {
                yield return depRequests[i];
            }

            LogUtility.Log("[Load bundle] " + m_Location.InternalId);

            AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(Path.Combine(Path.GetDirectoryName(Application.dataPath), "assetbundles/cresources/" + m_Location.InternalId));

            yield return request;

 
            SetContent(request.assetBundle as T);

            yield return base.LoadingAsyncInternal();
        }

        protected override void LoadingInternal()
        {
            if (m_Location.HasDependencies)
            {
                for (int i = 0; i < m_Location.Dependencies.Count; i++)
                {
                    CResources.Load<AssetBundle>(m_Location.Dependencies[i]);
                }
            }
            AssetBundle bundle = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Application.dataPath), "assetbundles/cresources/" + m_Location.InternalId));
            LogUtility.Log("[Load bundle] " + m_Location.InternalId);
            SetContent(bundle as T);

            base.LoadingInternal();
        }
    }

    public class CResouceBundleAssetRequest<T> : CResourceRequest<T> where T : class
    {
        protected override IEnumerator LoadingAsyncInternal()
        {
            if (m_Location.HasDependencies)
            {
                IResourceLocation bundleLocation = m_Location.Dependencies[0];
                CResourceRequest<AssetBundle> request = CResources.LoadAsync<AssetBundle>(bundleLocation);

               

                yield return request;

                AssetBundle bundle = request.Content as AssetBundle;
                LogUtility.Log("Load asset from bundle " + m_Location.InternalId);
                AssetBundleRequest assetRequest = bundle.LoadAssetAsync(Path.GetFileName(m_Location.InternalId), typeof(T));

                yield return assetRequest;

                SetContent(assetRequest.asset as T);
            }
            yield return base.LoadingAsyncInternal();
        }
        protected override void LoadingInternal()
        {
            if (m_Location.HasDependencies)
            {
                IResourceLocation bundleLocation = m_Location.Dependencies[0];
                CResourceRequest<AssetBundle> request = CResources.Load<AssetBundle>(bundleLocation);

                LogUtility.Log("Load asset from bundle " + m_Location.InternalId);

                AssetBundle bundle = request.Content as AssetBundle;

                Object asset = bundle.LoadAsset(Path.GetFileName(m_Location.InternalId), typeof(T));

                SetContent(asset as T);
            }
            base.LoadingInternal();
        }

    
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
            System.GC.Collect();
            for (int i = 0; i < m_ResouceProviders.Count; i++)
            {
                IResourceProvider provider = m_ResouceProviders[i];
                provider.UnloadUnusedAssets();
            }
            m_LoadRecorder.UnloadUnusedAssets();
            LogUtility.Log("[UnloadUnusedAssetsInternal ] is Done use time {0}", (Time.realtimeSinceStartup - t));
        }

        public static CResourceRequest<T> Load<T>(string requestID) where T : class
        {
            IResourceLocation location = Locate<T>(requestID);
            CResourceRequest<T> request = Load<T>(location);
            return request;       
        }

        public static CResourceRequest<T> LoadAsync<T>(string requestID) where T:class
        {
            IResourceLocation location = Locate<T>(requestID);
            return LoadAsync<T>(location);
        }

        public static CResourceRequest<T> CreateInstance<T>(string requestID) where T:class
        {
            IResourceLocation location = Locate<T>(requestID);
            CResourceRequest<T> request = Load<T>(location);
            T asset = request.Content as T;
            T instance = Instantiate<T>(asset);
            Release<T>(location, asset);
            return request;
        }
        public static CResourceRequest<T> CreateInstanceAsync<T>(string requestID) where T : class
        {
            CResourceRequest<T> requestInstance = new CResourceRequest<T>();
            IResourceLocation location = Locate<T>(requestID);
            CResourceRequest<T> request = LoadAsync<T>(location);
            request.Completed += (p) =>
            {
                Debug.Log(p.Content);
                T asset = p.Content as T;
                T instance = Instantiate<T>(asset);
                Release<T>(location, asset);
                requestInstance.SetContent(instance);
            };          
            return requestInstance;
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

        internal static T Instantiate<T>(T obj) where T :class
        {
            IResourceLocation location = m_LoadRecorder.LocateResource(obj);
            if (location != null)
            {
                CResourceRequest<T> request = Load<T>(location);
                Object instance = Object.Instantiate(request.Content as Object);
                m_LoadRecorder.RecordInstance(request.Content, instance);
                return instance as T;
            }
            return null;
        }
 
        internal static CResourceRequest<T> Load<T>(IResourceLocation location) where T : class
        {
            IResourceProvider provider = GetResourceProvider<T>(location);
            CResourceRequest<T> request= provider.Provide<T>(location);
            m_LoadRecorder.RecordResource(location,request);
            return request;
        }

        internal static CResourceRequest<T> LoadAsync<T>(IResourceLocation location) where T : class
        { 
            IResourceProvider provider = GetResourceProvider<T>(location);
            CResourceRequest<T> request = provider.ProvideAsync<T>(location);
            m_LoadRecorder.RecordResource(location,request);
            return request;        
        }

        internal static void Release<T>(IResourceLocation location,object asset) where T:class
        {
            IResourceProvider provider = GetResourceProvider<T>(location);
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

        private static IResourceLocation Locate<T>(string requestID) where T : class
        {
            IResourceLocation location;
            for (int i = 0; i < m_Locators.Count; i++)
            {
                IResourceLocator locator = m_Locators[i];
                location = locator.Locate<T>(requestID);
                if (location != null)
                {
                    return location;
                }
            }
            throw new CanNotLocateExcption (requestID);
        }

        private static IResourceProvider GetResourceProvider<T>(IResourceLocation location)where T :class
        {       
            foreach (IResourceProvider provider in m_ResouceProviders)
            {
                if (provider.CanProvide<T>(location))
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

            internal Dictionary<int,System.WeakReference> m_Instances = new Dictionary<int ,System.WeakReference> ();

            internal Dictionary<int, System.WeakReference> m_Resouces = new Dictionary<int, System.WeakReference>();

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
                    if (!m_Resouces.ContainsKey(key))
                    {
                         m_Resouces.Add(key,new System.WeakReference(asset));
                    }
                }
            }

            internal void RecordResource<T>(IResourceLocation location, CResourceRequest<T> request) where T:class
            {
                if(request.keepWaiting)
                {
                    request.Completed += (re) =>
                    {
                        RecordResource(location, re.Content);
                    };
                }
                else
                {
                    RecordResource(location, request.Content);
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
                int instanceID = GetInstanceID(instance);
                m_Instances.Add(instanceID,new System.WeakReference(asset));               
                m_InstanceResources.Add(instanceID, GetInstanceID(asset));
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

            public void UnloadUnusedAssets()
            {
                List<int> needMoves = new List<int>();
                foreach(var item in m_Instances)
                {
                    if(item.Value.IsAlive ==false)
                    {
                        needMoves.Add(item.Key);
                    }
                }
                for(int i =0;i<needMoves.Count;i++)
                {
                    m_InstanceResources.Remove(needMoves[i]);
                    m_Instances.Remove(needMoves[i]);
                }
                List<int> needreMoves = new List<int>();
                foreach (var item in m_Resouces)
                {
                    if (item.Value.IsAlive == false)
                    {
                        needreMoves.Add(item.Key);
                    }
                }
                for (int i = 0; i < needreMoves.Count; i++)
                {
                    m_LoadedCResources.Remove(needreMoves[i]);
                    m_Resouces.Remove(needreMoves[i]);
                }   
            }
        }
    }

    public interface IResourceLocator
    {
        IResourceLocation Locate<T>(string requestI)
        where T :class;
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

        CResourceRequest<T> Provide<T>(IResourceLocation location)
        where T : class;


        CResourceRequest<T> ProvideAsync<T>(IResourceLocation location)
        where T : class;

        bool CanProvide<T>(IResourceLocation location)
        where T : class;

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

        public virtual bool CanProvide<T>(IResourceLocation location) where T : class
        {
            if (location == null)
                throw new System.ArgumentException("IResourceLocation location cannot be null.");
            return ProviderId.Equals(location.ProviderId, System.StringComparison.Ordinal);
        }

        public override string ToString()
        {
            return string.Format("[{0}]", ProviderId);
        }

        public abstract CResourceRequest<T> Provide<T>(IResourceLocation location) where T : class;

        public abstract CResourceRequest<T> ProvideAsync<T>(IResourceLocation location) where T : class;
    
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
        protected  Dictionary<int,CResourceRequestBase> m_Cache = new Dictionary<int, CResourceRequestBase>();

        protected IResourceProvider m_Provider;

        public CResourcePoolProvider(IResourceProvider provider,int maxSize =0)
        {
            m_Provider = provider;
        }

        public override bool CanProvide<T>(IResourceLocation location) 
        {
            return m_Provider.CanProvide<T>(location);
        }

        public override CResourceRequest<T> Provide<T>(IResourceLocation location) 
        {
            int key = location.GetHashCode();
            if(m_Cache.ContainsKey(key))
            {
                CResourceRequestBase  res = m_Cache[key];
                if(res.IsAlive )
                {   
                    return res.Retain() as CResourceRequest<T>;

                }
                else
                {
                    LogUtility.LogError(location.InternalId +"  not alive");
                    m_Cache.Remove(key);
                }                  
            }
            CResourceRequest<T> request = m_Provider.Provide<T>(location).Retain() as CResourceRequest<T>;
            m_Cache.Add(key, request);               
            return request;
        }

        public override CResourceRequest<T> ProvideAsync<T>(IResourceLocation location)
        {
            int key = location.GetHashCode();
            if (m_Cache.ContainsKey(key))
            {
                CResourceRequestBase res = m_Cache[key];
                if (res.IsAlive)
                {
                    return res.Retain() as CResourceRequest<T>;
                }
                else
                {
                    LogUtility.LogError(location.InternalId + "  not alive");
                    m_Cache.Remove(key);
                }
            }
            CResourceRequest<T> request = m_Provider.ProvideAsync<T>(location).Retain() as CResourceRequest<T>;
            m_Cache.Add(key, request);
            return request; ;
        }

        public override bool Release(IResourceLocation location, object asset)
        {
            int key = location.GetHashCode();
          
            if (m_Cache.ContainsKey(key))
            {
                CResourceRequestBase  request = m_Cache[key];
                request.Release();
                if(request.m_RefCount == 0)
                {
                    m_Cache.Remove(key);
                    m_Provider.Release(location, request.Content);           
                    return true;
                }        
            }
            return false ;
        }

        public override void UnloadUnusedAssets()
        {
            List<int> assetsNotAlive = new List<int>();
            List<CResourceRequestBase> assetsNeedRemove = new List<CResourceRequestBase>();
            foreach (var item in m_Cache)
            {
                CResourceRequestBase request = item.Value;
                if(request !=null)
                {
                    LogUtility.Log("[CResources.UnloadUnusedAssets]{0}  {1}", request.Location.InternalId, request.IsAlive);
                    if (request.IsAlive == false)
                    {
                        request.m_RefCount =1;
                        assetsNeedRemove.Add(request);
                        assetsNotAlive.Add(item.Key);
                    }
                }       
            }

            for(int i =0;i<assetsNeedRemove.Count;i++)
            {
                CResources.Release<object>(assetsNeedRemove[i].Location, null);
            }
        }
    }

    public class LocalBundleCResourceProvider  : CResourceProvider
    {
        public override CResourceRequest<T> Provide<T>(IResourceLocation location)
        {
            CResourceBundleCreateRequest<T> request = new CResourceBundleCreateRequest<T>();
            return request.SendRequest(location);
        }

        public override CResourceRequest<T> ProvideAsync<T>(IResourceLocation location)
        {
            CResourceBundleCreateRequest<T> request = new CResourceBundleCreateRequest<T>();
            return request.SendRequestAsync(location);
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
        public override CResourceRequest<T> Provide<T>(IResourceLocation location)
        {
            CResouceBundleAssetRequest<T> request = new CResouceBundleAssetRequest<T>();
            return request.SendRequest(location);
        }

        public override CResourceRequest<T> ProvideAsync<T>(IResourceLocation location)
        {
            CResouceBundleAssetRequest<T> request = new CResouceBundleAssetRequest<T>();
            return request.SendRequestAsync(location);
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
        public override CResourceRequest<T> Provide<T>(IResourceLocation location)
        {
            CResourceRequest<T> request = new CResourceRequest<T>();
            T asset = UnityEditor.AssetDatabase.LoadAssetAtPath(location.InternalId,typeof(T)) as T;
            request.SetContent(asset);
            return request;
        }

        public override CResourceRequest<T> ProvideAsync<T>(IResourceLocation location)
        {
            return Provide<T>(location);
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

        public abstract IResourceLocation Locate<T>(string requestID) where T : class;

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

        public override IResourceLocation Locate<T>(string requestID)
        {
            IResourceLocation assetLocation = null;
            int hashCode = Lcation(requestID, typeof(T));
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

        public override IResourceLocation Locate<T>(string requestID)
        {
            IResourceLocation assetLocation = null;

            int hashCode = Lcation(requestID, typeof(T));

            if (m_Locations.ContainsKey(hashCode))
            {
                assetLocation = new BundleAssetCResourceLocation(requestID, typeof(BundleAssetCResourceProvider).FullName, new IResourceLocation[] { m_Locations[hashCode] });                 
            }

            return assetLocation;
        }

    }
}

