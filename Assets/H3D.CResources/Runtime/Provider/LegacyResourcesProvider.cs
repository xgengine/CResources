using System.Collections.Generic;
using UnityEngine;
using System.Collections;
namespace H3D.CResources
{
    internal class LegacyResourcesProvider : CResourceProvider
    {
        internal class InternalRequest<T> : CResourceRequest<T> where T : class
        {
            protected override IEnumerator LoadAsync()
            {
                ResourceRequest request = Resources.LoadAsync(m_location.InternalId, typeof(T));
                yield return request;
                SetResult(request.asset);

            }
            protected override void Load()
            {
                Object asset = Resources.Load(m_location.InternalId, typeof(T));
                SetResult(asset);
            }
        }
        public override CResourceRequest<T> Provide<T>(IResourceLocation location, List<CResourceRequest<object>> loadDependencyOperation)
        {
            InternalRequest<T> request = new InternalRequest<T>();
            return request.Send(location, null, false);
        }

        public override CResourceRequest<T> ProvideAsync<T>(IResourceLocation location, List<CResourceRequest<object>> loadDependencyOperation)
        {
            InternalRequest<T> request = new InternalRequest<T>();
            return request.Send(location, null, true);
        }

        public override bool Release(IResourceLocation location, object asset)
        {
            if (location == null)
                throw new System.ArgumentNullException("location");
            return true;
        }
    }
}
