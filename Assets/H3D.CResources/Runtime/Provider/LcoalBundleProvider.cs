using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace H3D.CResources
{
    internal class LocalBundleProvider : CResourceProvider
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
                    string bundleFilePath = "";
#if UNITY_EDITOR
                    bundleFilePath = Path.Combine(Path.GetDirectoryName(Application.dataPath), "assetbundles/cresources/" + m_location.InternalId);

#else
                    bundleFilePath = Path.Combine(Application.streamingAssetsPath, "cresources/" + m_location.InternalId);
#endif
                    AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(bundleFilePath);
                    yield return request;
                    //LogUtility.Log("[Load bundle async][{0}][{1}] [{2}]", m_Location.InternalId, request.assetBundle == null ? "NULL" : request.assetBundle.ToString(), Time.frameCount);
                    SetResult(request.assetBundle);
                }
            }
            protected override void Load()
            {
                AssetBundle bundle = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Application.dataPath), "assetbundles/cresources/" + m_location.InternalId));
                //LogUtility.Log("[Load bundle sync] " + m_Location.InternalId);
                SetResult(bundle);
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
            if (asset == null)
                throw new System.ArgumentNullException("asset");
            var bundle = asset as AssetBundle;
            if (bundle != null)
            {
                LogUtility.Log("Release bundle " + location.InternalId);
                bundle.Unload(true);
                return true;
            }

            return false;
        }
    }
}
