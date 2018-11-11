using System.Collections.Generic;
using System.Collections;
using System.IO;
using UnityEngine;
namespace H3D.CResources
{
    internal class BundleAssetProvider : CResourceProvider
    {
        internal class InternalRequest<T> : CResourceRequest<T> where T : class
        {
            protected override IEnumerator LoadAsync()
            {
                foreach (var dp in m_dependencyOperations)
                {
                    if (dp.IsDone == false)
                    {
                        yield return dp;
                    }
                }
                if (IsDone == false)
                {
                    AssetBundle bundle = m_dependencyOperations[0].Result as AssetBundle;
                    AssetBundleRequest assetRequest = bundle.LoadAssetAsync(Path.GetFileName(m_location.InternalId), typeof(T));
                    yield return assetRequest;
                    LogUtility.Log("Load asset from bundle async " + m_location.InternalId);
                    SetResult(assetRequest.asset);
                }
            }

            protected override void Load()
            {
                LogUtility.Log("Load asset from bundle sync " + m_location.InternalId);
                AssetBundle bundle = m_dependencyOperations[0].Result as AssetBundle;
                Object asset = bundle.LoadAsset(Path.GetFileName(m_location.InternalId), typeof(T));
                SetResult(asset);
            }

        }

        public override CResourceRequest<T> Provide<T>(IResourceLocation location, List<CResourceRequest<object>> loadDependencyOperation)
        {
            InternalRequest<T> request = new InternalRequest<T>();
            return request.Send(location, loadDependencyOperation, false);
        }

        public override CResourceRequest<T> ProvideAsync<T>(IResourceLocation location, List<CResourceRequest<object>> loadDependencyOperation)
        {
            InternalRequest<T> request = new InternalRequest<T>();

            return request.Send(location, loadDependencyOperation, true);
        }

        public override bool Release(IResourceLocation location, object asset)
        {
            if (location == null)
                throw new System.ArgumentNullException("location");
            return true;
        }
    }
}
