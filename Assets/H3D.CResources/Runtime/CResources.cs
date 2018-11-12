using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine.SceneManagement;

namespace H3D.CResources
{
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
            m_Locators.Add(new BundleAssetLocator());
#if UNIEY_EDITOR
            m_Locators.Add(new LocalAssetLocator());
#endif
            m_Locators.Add(new DefulatLocator());

            m_ResouceProviders = new List<IResourceProvider>();
            m_ResouceProviders.Add(new BundleAssetProvider());
            m_ResouceProviders.Add(new CPoolProvider(new LocalBundleProvider()));
#if UNIEY_EDITOR
            m_ResouceProviders.Add(new LocalAssetProvider());
#endif
            m_ResouceProviders.Add(new LegacyResourcesProvider());

            SceneManager.sceneUnloaded+= OnSceneUnloaded;
        }

        private static void OnSceneUnloaded(Scene scene)
        {
            LogUtility.Log("undload Load scene {0} ", scene.name);
            if(scene.name !="Preview Scene")
            {
                UnloadUnusedAssets();
            }

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

        public static T Load<T>(string requestID) where T : class
        {
            IResourceLocation location = Locate<T>(requestID);
            CResourceRequest<T> request = Load<T>(location);
            return request.Result;       
        }
        public static CResourceRequest<T> LoadAsync<T>(string requestID) where T:class
        {
            IResourceLocation location = Locate<T>(requestID);
            return LoadAsync<T>(location);
        }

        public static T CreateInstance<T>(string requestID) where T:class
        {
            CResourceRequest<T> requestInstance = new CResourceRequest<T>();
            IResourceLocation location = Locate<T>(requestID);
            IResourceProvider provider = GetResourceProvider<T>(location);
            CResourceRequest<T> request = provider.Provide<T>(location, LoadDependencies(location));

            Object instance = null;
            if(request.Status == AsyncOperationStatus.Succeeded)
            {
                instance = Object.Instantiate(request.Result as Object);
                m_MapLocator.RecordInstance(instance, location);
            }
    
            requestInstance.SetResult(instance);   
            return requestInstance.Result;
        }

        public static CResourceRequest<T> CreateInstanceAsync<T>(string requestID) where T : class
        {
            CResourceRequest<T> requestInstance = new CResourceRequest<T>();
            IResourceLocation location = Locate<T>(requestID);
            IResourceProvider provider = GetResourceProvider<T>(location);
            CResourceRequest<T> request = provider.ProvideAsync<T>(location, LoadDependenciesAsync(location));
            request.Completed += (p) =>
            {
                T asset = p.Result as T;

                Object instance = Object.Instantiate(request.Result as Object);
                m_MapLocator.RecordInstance(instance, location);
                requestInstance.SetResult(instance);
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
            if(location.HasDependencies)
            {
                foreach (var depLocation in location.Dependencies)
                {
                    requests.Add(Load<object>(depLocation));
                }
            }          
            return requests;
        }

        internal static List<CResourceRequest<object>> LoadDependenciesAsync(IResourceLocation location)
        {
            List<CResourceRequest<object>> requests = new List<CResourceRequest<object>>();
            if(location.HasDependencies)
            {
                foreach (var depLocation in location.Dependencies)
                {
                    requests.Add(LoadAsync<object>(depLocation));
                }
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
                    //LogUtility.Log("Record Asset"+location.InternalId+" "+d.Value);
                }
            }

            internal void RecordAsset<T>(IResourceLocation location, CResourceRequest<T> request) where T:class
            {
                if(!request.IsDone)
                {
                    request.Completed += (op) =>
                    {
                        RecordAsset(location, op.Result);
                    };
                }
                else
                {
                    RecordAsset(location, request.Result);
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

            internal void RecordInstance( object instance,IResourceLocation location)
            {
                if (location == null || instance == null)
                {
                    LogUtility.LogError("RecordInstance Error ");
                    return;
                }
                int instanceID = GetInstanceID(instance);

                m_InstanceID2LocationMap.Add(instanceID,location);

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
                //LogUtility.Log(info);
                //LogUtility.Log("++++++++++++++++++++++++++++++");
                LogUtility.Log("[{0}][{1}][{2}][{3}]", m_AssetID2LocationMap.Count, m_AssetID2RefrenceMap.Count, m_InstanceID2LocationMap.Count, m_InstanceID2RefrenceMap.Count);
                //foreach(var item in m_AssetID2RefrenceMap)
                //{
                //    LogUtility.Log("[{0}][{1}][{2}]", m_AssetID2LocationMap[item.Key].InternalId, item.Value.Key.IsAlive, item.Value.Value);
                //}
                //LogUtility.Log("__________________________________");
                //foreach (var item in m_InstanceID2RefrenceMap)
                //{
                //    LogUtility.Log("[{0}][{1}][{2}]", m_InstanceID2LocationMap[item.Key].InternalId, item.Value.IsAlive,item.Value.Target);
                //}
                //LogUtility.Log("++++++++++++++++++++++++++++++");
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
}

