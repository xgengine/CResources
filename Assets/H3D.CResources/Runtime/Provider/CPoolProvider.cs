using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace  H3D.CResources
{
    internal class CPoolProvider : CResourceProvider
    {
        protected Dictionary<int, CResourceRequest> m_Cache = new Dictionary<int, CResourceRequest>();

        protected IResourceProvider m_Provider;

        public CPoolProvider(IResourceProvider provider, int maxSize = 0)
        {
            m_Provider = provider;
        }

        public override bool CanProvide<T>(IResourceLocation location)
        {
            return m_Provider.CanProvide<T>(location);
        }

        public override CResourceRequest<T> Provide<T>(IResourceLocation location, List<CResourceRequest<object>> loadDependencyOperation)
        {
            CResourceRequest<T> request = null;
            int key = location.GetHashCode();
            if (m_Cache.ContainsKey(key))
            {
                request = m_Cache[key] as CResourceRequest<T>;
                request.Retain();
                if (request.IsDone == false)
                {
                    request.LoadImmediate();
                }
                return request;
            }
            request = m_Provider.Provide<T>(location, loadDependencyOperation).Retain();
            m_Cache.Add(key, request);
            return request;
        }

        public override CResourceRequest<T> ProvideAsync<T>(IResourceLocation location, List<CResourceRequest<object>> loadDependencyOperation)
        {
            CResourceRequest<T> request = null;
            int key = location.GetHashCode();
            if (m_Cache.ContainsKey(key))
            {
                request = m_Cache[key] as CResourceRequest<T>;
                return request.Retain();
            }
            request = m_Provider.ProvideAsync<T>(location, loadDependencyOperation).Retain();
            m_Cache.Add(key, request);
            return request; ;
        }

        public override bool Release(IResourceLocation location, object asset)
        {
            int key = location.GetHashCode();
            if (m_Cache.ContainsKey(key))
            {
                CResourceRequest request = m_Cache[key];
                if (request.Release())
                {
                    m_Provider.Release(location, request.Result);
                    m_Cache.Remove(key);
                    return true;
                }
            }
            return false;
        }
    }

}
