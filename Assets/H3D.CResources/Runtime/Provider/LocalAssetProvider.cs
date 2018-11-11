using System.Collections.Generic;
using UnityEngine;

namespace H3D.CResources
{
#if UNITY_EDITOR
    internal class LocalAssetProvider : CResourceProvider
    {

        public override CResourceRequest<T> Provide<T>(IResourceLocation location, List<CResourceRequest<object>> loadDependencyOperation)
        {
            CResourceRequest<T> request = new CResourceRequest<T>();
            T asset = UnityEditor.AssetDatabase.LoadAssetAtPath(location.InternalId, typeof(T)) as T;
            request.SetResult(asset);
            return request;
        }

        public override CResourceRequest<T> ProvideAsync<T>(IResourceLocation location, List<CResourceRequest<object>> loadDependencyOperation)
        {
            return Provide<T>(location, loadDependencyOperation);
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
}
