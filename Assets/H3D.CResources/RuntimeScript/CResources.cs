using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace H3D.CResources
{
    public class CResources
    {

        static List<IResourceLocator> m_Locators;
        static List<IResourceProvider> m_ResouceProviders;

        static CResources()
        {
            m_Locators = new List<IResourceLocator>();

            //m_Locators.Add(new LocalAssetCResourceLocator());
            m_Locators.Add( new BundleAssetCResourceLocator());

            m_ResouceProviders = new List<IResourceProvider>();

            m_ResouceProviders.Add(new CResourcePoolProvider( new BundleAssetCResourceProvider()));
            m_ResouceProviders.Add(new LocalBundleCResourceProvider());

           // m_ResouceProviders.Add(new LocalAssetCResourceProvider());
   
        }

        public static void UnloadUnusedAssets()
        {
            Resources.UnloadUnusedAssets();
        }

        public static TObject Load<TObject>(string requestID) where TObject : Object
        {
            try
            {
                IResourceLocation location = Locate<TObject>(requestID);

                return ProvideResource<TObject>(location);
            }
            catch (CResourcesException e)
            {
                LogUtility.LogError(e.Message);
            }
            return null;
        }

        internal static TObject ProvideResource<TObject>(IResourceLocation location) where TObject : Object
        {
            try
            {
                IResourceProvider provider = GetResourceProvider<TObject>(location);
                return provider.Provide<TObject>(location);
            }
            catch (CResourcesException e)
            {
                LogUtility.LogError(e.Message);
            }
            return null;
        }


        private static IResourceLocation Locate<TObject>(string requestID) where TObject : Object
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

        private static IResourceProvider GetResourceProvider<TObject>(IResourceLocation location)where TObject : Object
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

        public static void Destroy(Object obj)
        {

        }
    }

    public interface IResourceLocator
    {
        IResourceLocation Locate<TObject>(string requestID)
        where TObject : UnityEngine.Object;
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
        where TObject : Object;


        bool CanProvide<TObject>(IResourceLocation location)
        where TObject : Object;


        bool Release(IResourceLocation location, object asset);
    }


    public abstract class CResourceProvider : IResourceProvider
    {
        protected CResourceProvider() { }

        public virtual string ProviderId
        {
            get { return GetType().FullName; }
        }

        public virtual bool CanProvide<TObject>(IResourceLocation location)where TObject : Object
        {
            if (location == null)
                throw new System.ArgumentException("IResourceLocation location cannot be null.");
            return ProviderId.Equals(location.ProviderId, System.StringComparison.Ordinal);
        }

        public override string ToString()
        {
            return string.Format("[{0}]", ProviderId);
        }

        public abstract TObject Provide<TObject>(IResourceLocation location)
        where TObject : Object;

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

        public class CResource
        {
            protected System.WeakReference m_Reference;

            public Object Content
            {
                get
                {
                    return m_Reference.Target as Object;
                }
            }

            public bool IsAlive
            {
                get
                {
                    return m_Reference.IsAlive;
                }
            }

            public CResource( Object obj)
            {
                m_Reference =new  System.WeakReference(obj);
            }
        }


        protected Dictionary<string, CResource> m_Cache = new Dictionary<string, CResource>();

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
            string key = location.InternalId;
            if(m_Cache.ContainsKey(key))
            {
                CResource res = m_Cache[key];
                if(res.IsAlive)
                {
                    return res.Content as TObject;
                }
                else
                {
                    m_Cache.Remove(key);
                }
            }

            TObject result = m_Provider.Provide<TObject>(location);
            m_Cache.Add(key, new CResource(result));
            return result;
        }

        public bool Release(IResourceLocation location, object asset)
        {
            return true ;
        }
    }


    public class LocalBundleCResourceProvider  : CResourceProvider
    {
        static Dictionary<int, AssetBundle> cache = new Dictionary<int, AssetBundle>();

        public override TObject Provide<TObject>(IResourceLocation location)
        {
            int key = location.GetHashCode();
            if (cache.ContainsKey(key))
            {
                return cache[key] as TObject;
            }

            if(location.HasDependencies)
            {
                for(int i = 0;i<location.Dependencies.Count;i++)
                {
                    CResources.ProvideResource<AssetBundle>(location.Dependencies[i]);
                }
            }
            
            AssetBundle bundle = AssetBundle.LoadFromFile(Path.Combine( Path.GetDirectoryName( Application.dataPath),"assetbundles/cresources/" +location.InternalId));
           
            cache.Add(key,bundle);
            return bundle as TObject;
        }
        public override bool Release(IResourceLocation location, object asset)
        {
            throw new System.NotImplementedException();
        }
    }

#if UNITY_EDITOR
    public class LocalAssetCResourceProvider : CResourceProvider
    {

        public override TObject Provide<TObject>(IResourceLocation location)
        {
            TObject asset = UnityEditor.AssetDatabase.LoadAssetAtPath<TObject>(location.InternalId);
            if (asset == null)
            {
                throw new ResourceProviderFailedException(this, location);
            }
            return asset;
        }

        public override bool Release(IResourceLocation location, object asset)
        {
            throw new System.NotImplementedException();
        }
    }
#endif
    public class BundleAssetCResourceProvider : CResourceProvider
    {
        public override TObject Provide<TObject>(IResourceLocation location)
        {
            if(location.HasDependencies)
            {
                IResourceLocation bundleLocation = location.Dependencies[0];
                AssetBundle bundle = CResources.ProvideResource<AssetBundle>(bundleLocation);
                LogUtility.Log("Load asset from bundle "+location.InternalId);
                return  bundle.LoadAsset<TObject>(Path.GetFileName(location.InternalId));
         
            }
            else
            {
                throw new CResourcesException(" Can not find bundle Location "+location.InternalId);
            }
             
        }

        public override bool Release(IResourceLocation location, object asset)
        {
            throw new System.NotImplementedException();
        }
    }


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
        public abstract IResourceLocation Locate<TObject>(string requestID)
        where TObject : Object;

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