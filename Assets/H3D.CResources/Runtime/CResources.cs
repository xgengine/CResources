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
            //LogUtility.Log("[RefCount] [{0}] [Retain] [{1}]", m_RefCount,m_Location.InternalId );
            return this;
        }

        public void Release()
        {
            m_RefCount--;
           // LogUtility.Log("[RefCount] [{0}] [Release]  [{1}]", m_RefCount, m_Location.InternalId); ;
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

        protected virtual void LoadingInternal()
        {  
            
        }
        public void LoadImmediate()
        {
            LoadingInternal();
        
        }
        protected List<CResourceRequest<object>> m_DepOperations;
    }

    public class CResourceRequest<T> : CResourceRequestBase where T : class
    {
        List<System.Action<CResourceRequest<T>>> m_CompletedAction;
        public event System.Action<CResourceRequest<T>> Completed
        {
            add
            {
                if(IsDone)
                {
                   value(this);
                }
                else
                {
                    if (m_CompletedAction == null)
                        m_CompletedAction = new List<System.Action<CResourceRequest<T>>>();
                    m_CompletedAction.Add(value);
                }
            }
            remove
            {
                m_CompletedAction.Remove(value);
            }
        }

        public void InvokeCompletionEvent()
        {
            if (m_CompletedAction != null)
            {
                for (int i = 0; i < m_CompletedAction.Count; i++)
                {
                    try
                    {
                        m_CompletedAction[i](this);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogException(e);
                      
                    }
                }
                m_CompletedAction.Clear();
            }
        }

        new public T Content
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
        public CResourceRequest<T> SendRequestAsync(IResourceLocation location, List<CResourceRequest<object>> loadDependencyOperation)
        {
            m_Location = location;
            m_DepOperations = loadDependencyOperation;
            TaskManager.Instance.StartTask(LoadingAsyncInternal());
            return this;
        }

        public CResourceRequest<T> SendRequest(IResourceLocation location, List<CResourceRequest<object>> loadDependencyOperation)
        {
            m_Location = location;
            m_DepOperations = loadDependencyOperation;
            LoadingInternal();
            return this;
        }

        protected IEnumerator LoadingAsyncInternal()
        {
            yield return LoadingAsync();
            m_IsDone = true;
            m_KeepWaiting = false;
            InvokeCompletionEvent();
            yield break;           
        }
        protected virtual IEnumerator LoadingAsync()
        {
            yield break;
        }
        protected virtual void Loading()
        {
          
        }
        protected  override void LoadingInternal()
        {
            Loading();
            m_IsDone = true;
            m_KeepWaiting = false;
        }
    
    }

    public class CResourceBundleCreateRequest<T> : CResourceRequest<T> where T : class
    {
        protected override IEnumerator LoadingAsync()
        {
            foreach (var dp in m_DepOperations)
            {
                if(dp.IsDone ==false)
                {
                    yield return dp;
                }
            }
            if (IsDone==false)
            {
            
                AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(Path.Combine(Path.GetDirectoryName(Application.dataPath), "assetbundles/cresources/" + m_Location.InternalId));
                yield return request;
                //LogUtility.Log("[Load bundle async][{0}][{1}] [{2}]", m_Location.InternalId, request.assetBundle == null ? "NULL" : request.assetBundle.ToString(), Time.frameCount);
                SetContent(request.assetBundle as T);              
            }       
        }

        protected override void Loading()
        {
            AssetBundle bundle = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Application.dataPath), "assetbundles/cresources/" + m_Location.InternalId));
            //LogUtility.Log("[Load bundle sync] " + m_Location.InternalId);
            SetContent(bundle as T);
        }
    }

    public class CResouceBundleAssetRequest<T> : CResourceRequest<T> where T : class
    {
        protected override IEnumerator LoadingAsync()
        {
            foreach(var dp in m_DepOperations)
            {
                if(dp.IsDone == false)
                {
                    yield return dp;
                }
            }
            if(IsDone == false)
            {
                AssetBundle bundle = m_DepOperations[0].Content as AssetBundle;
                AssetBundleRequest assetRequest = bundle.LoadAssetAsync(Path.GetFileName(m_Location.InternalId), typeof(T));
                yield return assetRequest;
                LogUtility.Log("Load asset from bundle async " + m_Location.InternalId);
                SetContent(assetRequest.asset as T);
            }                
        }

        protected override void Loading()
        {
            LogUtility.Log("Load asset from bundle sync " + m_Location.InternalId);
            AssetBundle bundle = m_DepOperations[0].Content as AssetBundle;
            Object asset = bundle.LoadAsset(Path.GetFileName(m_Location.InternalId), typeof(T));
            SetContent(asset as T);
        }
    
    }

    public class CResources
    {
        static MapLocator m_MapLocator;

        static List<IResourceLocator> m_Locators;

        static List<IResourceProvider> m_ResouceProviders;

        static CResources()
        {    
        }

        [RuntimeInitializeOnLoadMethod]
        private static void RuntimeInitialization()
        {
            m_MapLocator = new MapLocator();

            m_Locators = new List<IResourceLocator>();

            //m_Locators.Add(new LocalAssetCResourceLocator());
            m_Locators.Add(new BundleAssetCResourceLocator());

            m_ResouceProviders = new List<IResourceProvider>();

            m_ResouceProviders.Add(new CResourcePoolProvider(new BundleAssetCResourceProvider()));
            m_ResouceProviders.Add(new CResourcePoolProvider(new LocalBundleCResourceProvider()));

            // m_ResouceProviders.Add(new LocalAssetCResourceProvider());
            SceneManager.sceneUnloaded+= OnSceneUnloaded;
        }

        static void OnSceneUnloaded(Scene scene)
        {
            LogUtility.Log("undload Load scene {0} ", scene.name);
            UnloadUnusedAssets();
        }

        public static AsyncOperation UnloadUnusedAssets()
        {
            System.GC.Collect();
            AsyncOperation operation = Resources.UnloadUnusedAssets();
            operation.completed += (p) =>
            {
                UnloadUnusedAssetsInternal();
            };
            return operation;
        }

        internal static void UnloadUnusedAssetsInternal()
        {
            System.GC.Collect();
            float t = Time.realtimeSinceStartup;
           
            m_MapLocator.UnloadUnusedAssets();
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
            CResourceRequest<T> requestInstance = new CResourceRequest<T>();
            IResourceLocation location = Locate<T>(requestID);
            IResourceProvider provider = GetResourceProvider<T>(location);
            CResourceRequest<T> request = provider.Provide<T>(location, LoadDependenciesAsync(location));     
            Object instance = Object.Instantiate(request.Content as Object);
            m_MapLocator.RecordInstance(request.Content, instance);
            requestInstance.SetContent(instance);   
            return requestInstance;
        }

        public static CResourceRequest<T> CreateInstanceAsync<T>(string requestID) where T : class
        {
            CResourceRequest<T> requestInstance = new CResourceRequest<T>();
            IResourceLocation location = Locate<T>(requestID);
            IResourceProvider provider = GetResourceProvider<T>(location);
            CResourceRequest<T> request = provider.ProvideAsync<T>(location, LoadDependenciesAsync(location));
            request.Completed += (p) =>
            {
                T asset = p.Content as T;

                Object instance = Object.Instantiate(request.Content as Object);
                m_MapLocator.RecordInstance(request.Content, instance);
                requestInstance.SetContent(instance);
            };          
            return requestInstance;
        }

        internal static CResourceRequest<T> Load<T>(IResourceLocation location) where T : class
        {
            IResourceProvider provider = GetResourceProvider<T>(location);
            CResourceRequest<T> request= provider.Provide<T>(location, LoadDependencies(location));
            m_MapLocator.RecordAsset(location,request);
            return request;
        }

        internal static List<CResourceRequest<object>> LoadDependencies(IResourceLocation location)
        {
            List<CResourceRequest<object>> requests = new List<CResourceRequest<object>>();
            foreach(var depLocation in location.Dependencies)
            {
                 requests.Add(Load<object>(depLocation));
            }     
            return requests;
        }

        internal static List<CResourceRequest<object>> LoadDependenciesAsync(IResourceLocation location)
        {
            List<CResourceRequest<object>> requests = new List<CResourceRequest<object>>();
            foreach (var depLocation in location.Dependencies)
            {
                requests.Add(LoadAsync<object>(depLocation));
            }
            return requests;
        }

        internal static CResourceRequest<T> LoadAsync<T>(IResourceLocation location) where T : class
        { 
            IResourceProvider provider = GetResourceProvider<T>(location);
            CResourceRequest<T> request = provider.ProvideAsync<T>(location,LoadDependenciesAsync(location));
            m_MapLocator.RecordAsset(location,request);
            return request;        
        }

        internal static void ReleaseResource<T>(IResourceLocation location,object asset) where T:class
        {

            IResourceProvider provider = GetResourceProvider<T>(location);
            provider.Release(location, asset);
            if (location.HasDependencies)
            {
                for (int i = 0; i < location.Dependencies.Count; i++)
                {
                    ReleaseResource<object>(location.Dependencies[i], null);
                }
            }
            
        }
        public static void Destroy(object obj)
        {
            IResourceLocation location = m_MapLocator.Locate(obj);
            if (location != null)
            {
                
                if (m_MapLocator.IsInstance(obj))
                {
                    m_MapLocator.RemoveInstance(obj);
                    Object.Destroy(obj as Object);
                    ReleaseResource<object>(location, null);
                }
                else
                {
                    ReleaseResource<object>(location, null);
                    m_MapLocator.RemoveAsset(location);
                }
            }
            else
            {
                LogUtility.LogError(" CResource.Destory only can destory the asset and instance Load or Instantiate by CResources API ");
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

        internal class MapLocator
        {
            internal Dictionary<int, IResourceLocation> m_AssetID2LocationMap = new Dictionary<int, IResourceLocation>();

            internal Dictionary<int, IResourceLocation> m_InstanceID2LocationMap = new Dictionary<int, IResourceLocation>();



            internal Dictionary<int, KeyValuePair<System.WeakReference, int>> m_AssetID2RefrenceMap = new Dictionary<int, KeyValuePair<System.WeakReference, int>>();

            internal Dictionary<int, System.WeakReference> m_InstanceID2RefrenceMap = new Dictionary<int, System.WeakReference>();

            internal void RecordAsset(IResourceLocation location, object asset)
            {
                if (asset == null || location == null)
                {
                    LogUtility.LogError("RecordResource Error ");
                    return;
                }
                int key = GetInstanceID(asset);
                if (key != -1)
                {
                    if (!m_AssetID2LocationMap.ContainsKey(key))
                    {
                        m_AssetID2LocationMap.Add(key, location);      
                    }
    
                    if (!m_AssetID2RefrenceMap.ContainsKey(key))
                    {
                        m_AssetID2RefrenceMap.Add(key, new KeyValuePair<System.WeakReference, int>(new System.WeakReference(asset), 1));
                       
                    }
                    else
                    {
                        var keyValue = m_AssetID2RefrenceMap[key];
                        m_AssetID2RefrenceMap[key] = new KeyValuePair<System.WeakReference, int>(keyValue.Key, keyValue.Value + 1);
                    }
                    var d = m_AssetID2RefrenceMap[key];
                    //LogUtility.Log("Record Asset"+location.InternalId+" "+d.Value);
                }
            }

            internal void RecordAsset<T>(IResourceLocation location, CResourceRequest<T> request) where T:class
            {
                if(!request.IsDone)
                {
                    request.Completed += (op) =>
                    {
                        RecordAsset(location, op.Content);
                    };
                }
                else
                {
                    RecordAsset(location, request.Content);
                }               
            }

            internal IResourceLocation Locate(object asset)
            {
                int key = GetInstanceID(asset);
                if(m_AssetID2LocationMap.ContainsKey(key))
                {
                    return m_AssetID2LocationMap[key];
                }
                else if(m_InstanceID2LocationMap.ContainsKey(key))
                {
                    return m_InstanceID2LocationMap[key];
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

                m_InstanceID2LocationMap.Add(instanceID,Locate(asset));

                m_InstanceID2RefrenceMap.Add(instanceID, new System.WeakReference(instance));
            }

            internal bool IsInstance(object asset)
            {
                return m_InstanceID2LocationMap.ContainsKey(GetInstanceID(asset));
            }

            internal bool IsAsset(object asset)
            {
                return m_AssetID2LocationMap.ContainsKey(GetInstanceID(asset));
            }

            internal bool RemoveAsset(IResourceLocation location)
            {
                int key = -1;
                foreach(var item in m_AssetID2LocationMap)
                {
                    if(item.Value.InternalId == location.InternalId)
                    {
                        key = item.Key;
                    }
                }
                if (m_AssetID2RefrenceMap.ContainsKey(key))
                {
                    var keyValue = m_AssetID2RefrenceMap[key];

                    if(keyValue.Value==1)
                    {
                        m_AssetID2RefrenceMap.Remove(key);
                        m_AssetID2LocationMap.Remove(key);
                    }
                    else
                    {
                        m_AssetID2RefrenceMap[key] = new KeyValuePair<System.WeakReference, int>(keyValue.Key, keyValue.Value - 1);
                    }
                }            
                return false;
            }

            internal bool RemoveInstance(object asset)
            {
                int key = GetInstanceID(asset);
                if (m_InstanceID2LocationMap.ContainsKey(key))
                {
                    m_InstanceID2LocationMap.Remove(key);
                    m_InstanceID2RefrenceMap.Remove(key);
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


            void LogInfo(string info)
            {
                LogUtility.Log(info);
                LogUtility.Log("++++++++++++++++++++++++++++++");
                LogUtility.Log("[{0}][{1}][{2}][{3}]", m_AssetID2LocationMap.Count, m_AssetID2RefrenceMap.Count, m_InstanceID2LocationMap.Count, m_InstanceID2RefrenceMap.Count);
                foreach(var item in m_AssetID2RefrenceMap)
                {
                    LogUtility.Log("[{0}][{1}][{2}]", m_AssetID2LocationMap[item.Key].InternalId, item.Value.Key.IsAlive, item.Value.Value);
                }
                LogUtility.Log("__________________________________");
                foreach (var item in m_InstanceID2RefrenceMap)
                {
                    LogUtility.Log("[{0}][{1}][{2}]", m_InstanceID2LocationMap[item.Key].InternalId, item.Value.IsAlive,item.Value.Target);
                }
                LogUtility.Log("++++++++++++++++++++++++++++++");
            }
            public void UnloadUnusedAssets()
            {
                LogInfo("begin");

                List<int> needReleaseInstances = new List<int>();

                foreach(var instanceRefrence in m_InstanceID2RefrenceMap)
                {
                    if(instanceRefrence.Value.IsAlive == false||instanceRefrence.Value.Target.ToString() =="null")
                    {
                        needReleaseInstances.Add(instanceRefrence.Key);
                    }
                }
                for(int i =0;i<needReleaseInstances.Count;i++)
                {
                    int key =needReleaseInstances[i]; 
                    ReleaseResource<object>(m_InstanceID2LocationMap[key], null);
                    m_InstanceID2RefrenceMap.Remove(key);
                    m_InstanceID2LocationMap.Remove(key);
                }

                Dictionary<IResourceLocation, int> needReleaseAssets = new Dictionary<IResourceLocation, int>();
                foreach(var assetRefrence in m_AssetID2RefrenceMap)
                {
                    if(assetRefrence.Value.Key.IsAlive==false)
                    {
                        needReleaseAssets.Add(m_AssetID2LocationMap[ assetRefrence.Key], assetRefrence.Value.Value);
                    }
                }
                foreach (var item in needReleaseAssets)
                {
                    for (int i = 0; i < item.Value; i++)
                    {
                        ReleaseResource<object>(item.Key, null);
                        m_MapLocator.RemoveAsset(item.Key);
                    }
                }

                LogInfo("end");
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

        CResourceRequest<T> Provide<T>(IResourceLocation location, List<CResourceRequest<object>> loadDependencyOperation)
        where T : class;


        CResourceRequest<T> ProvideAsync<T>(IResourceLocation location, List<CResourceRequest<object>> loadDependencyOperation)
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

        public abstract CResourceRequest<T> Provide<T>(IResourceLocation location, List<CResourceRequest<object>> loadDependencyOperation) where T : class;

        public abstract CResourceRequest<T> ProvideAsync<T>(IResourceLocation location, List<CResourceRequest<object>> loadDependencyOperation) where T : class;
    
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

        public override CResourceRequest<T> Provide<T>(IResourceLocation location , List<CResourceRequest<object>> loadDependencyOperation) 
        {
     
            int key = location.GetHashCode();
            if(m_Cache.ContainsKey(key))
            {
                CResourceRequestBase  res = m_Cache[key];
                if(res.IsAlive )
                {
                    res.Retain();
                    if (res.IsDone ==false)
                    {
                       
                        res.LoadImmediate();
                    }
                    return res as CResourceRequest<T>;
                }
                else
                {
                    LogUtility.LogError(location.InternalId +"  not alive");
                    m_Cache.Remove(key);
                }                  
            }
            CResourceRequest<T> request = m_Provider.Provide<T>(location,loadDependencyOperation).Retain() as CResourceRequest<T>;
            m_Cache.Add(key, request);               
            return request;
        }

        public override CResourceRequest<T> ProvideAsync<T>(IResourceLocation location, List<CResourceRequest<object>> loadDependencyOperation)
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
            CResourceRequest<T> request = m_Provider.ProvideAsync<T>(location,loadDependencyOperation).Retain() as CResourceRequest<T>;
            m_Cache.Add(key, request);
            return request; ;
        }

        public override bool Release(IResourceLocation location, object asset)
        {
            int key = location.GetHashCode();   
            if (m_Cache.ContainsKey(key))
            {
                CResourceRequestBase request = m_Cache[key];
                request.Release();
                if (request.m_RefCount == 0)
                {           
                    m_Provider.Release(location, request.Content);
                    m_Cache.Remove(key);                         
                    return true;
                }        
            }
            return false ;
        }

        public override void UnloadUnusedAssets()
        {
            //List<int> assetsNotAlive = new List<int>();
            //List<CResourceRequestBase> assetsNeedRemove = new List<CResourceRequestBase>();
            //foreach (var item in m_Cache)
            //{
            //    CResourceRequestBase request = item.Value;
            //    if (request != null)
            //    {
            //        LogUtility.Log("[CResources.UnloadUnusedAssets]{0}  {1}", request.Location.InternalId, request.IsAlive);
            //        if (request.IsAlive == false)
            //        {
            //            request.m_RefCount = 1;
            //            assetsNeedRemove.Add(request);
            //            assetsNotAlive.Add(item.Key);
            //        }
            //    }
            //}

            //for (int i = 0; i < assetsNeedRemove.Count; i++)
            //{
            //    CResources.Release<object>(assetsNeedRemove[i].Location, null);
            //}
        }
    }

    public class LocalBundleCResourceProvider  : CResourceProvider
    {
        public override CResourceRequest<T> Provide<T>(IResourceLocation location, List<CResourceRequest<object>> loadDependencyOperation)
        {
            CResourceBundleCreateRequest<T> request = new CResourceBundleCreateRequest<T>();
            return request.SendRequest(location,loadDependencyOperation);
        }

        public override CResourceRequest<T> ProvideAsync<T>(IResourceLocation location, List<CResourceRequest<object>> loadDependencyOperation)
        {
            CResourceBundleCreateRequest<T> request = new CResourceBundleCreateRequest<T>();
            return request.SendRequestAsync(location, loadDependencyOperation);
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
        public override CResourceRequest<T> Provide<T>(IResourceLocation location, List<CResourceRequest<object>> loadDependencyOperation)
        {
            CResouceBundleAssetRequest<T> request = new CResouceBundleAssetRequest<T>();

            return request.SendRequest(location, loadDependencyOperation);
        }

        public override CResourceRequest<T> ProvideAsync<T>(IResourceLocation location, List<CResourceRequest<object>> loadDependencyOperation)
        {
            CResouceBundleAssetRequest<T> request = new CResouceBundleAssetRequest<T>();
            return request.SendRequestAsync(location, loadDependencyOperation);
        }

        public override bool Release(IResourceLocation location, object asset)
        {
            if (location == null)
                throw new System.ArgumentNullException("location");
            return true;
        }
    }

#if UNITY_EDITOR
    //public class LocalAssetCResourceProvider : CResourceProvider
    //{
    //    public override CResourceRequest<T> Provide<T>(IResourceLocation location)
    //    {
    //        CResourceRequest<T> request = new CResourceRequest<T>();
    //        T asset = UnityEditor.AssetDatabase.LoadAssetAtPath(location.InternalId,typeof(T)) as T;
    //        request.SetContent(asset);
    //        return request;
    //    }

    //    public override CResourceRequest<T> ProvideAsync<T>(IResourceLocation location)
    //    {
    //        return Provide<T>(location);
    //    }

    //    public override bool Release(IResourceLocation location, object asset)
    //    {

    //        if (location == null)
    //            throw new System.ArgumentNullException("location");
    //        var go = asset as GameObject;
    //        if (go != null)
    //        {
    //            //GameObjects cannot be resleased via Object.Destroy because they are considered an asset
    //            //but they can't be unloaded via Resources.UnloadAsset since they are NOT an asset?
    //            return true;
    //        }
    //        var obj = asset as Object;
    //        if (obj != null)
    //        {
    //            Resources.UnloadAsset(obj);
    //            return true;
    //        }
    //        return true;
    //    }
    //}
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
    //public class LocalAssetCResourceLocator : CResourceLocator
    //{
      
    //    public LocalAssetCResourceLocator()
    //    {
    //        m_Locations = new Dictionary<int, IResourceLocation>();
    //        IEnumerable<string> paths = Directory.GetFiles(ConstValue.m_PackPath, "*.*", SearchOption.AllDirectories).Where(
    //               p => p.EndsWith(".meta") == false && p.EndsWith(".cs") == false
    //           );
    //        foreach (var wholeFilePath in paths)
    //        {
    //            string assetPath = CRUtlity.FullPathToAssetPath(wholeFilePath);
    //            string guid =  UnityEditor.AssetDatabase.AssetPathToGUID(assetPath);
    //            System.Type type = UnityEditor.AssetDatabase.GetMainAssetTypeAtPath(assetPath);
    //            int location = Lcation(AssetPathToLoadPath(assetPath), type);

    //            m_Locations.Add(location,new LocalAssetCResourceLocation(assetPath,(typeof(LocalAssetCResourceProvider)).FullName));
    //        }
    //    }

    //    public override IResourceLocation Locate<T>(string requestID)
    //    {
    //        IResourceLocation assetLocation = null;
    //        int hashCode = Lcation(requestID, typeof(T));
    //        if (m_Locations.ContainsKey(hashCode))
    //        {
    //            assetLocation = m_Locations[hashCode];
    //        }
    //        return assetLocation;
    //    }   
    //}
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

