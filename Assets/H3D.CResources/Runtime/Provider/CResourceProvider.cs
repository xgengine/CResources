using System.Collections.Generic;
namespace H3D.CResources
{
    internal abstract class CResourceProvider : IResourceProvider
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

        public virtual bool Release(IResourceLocation location, object asset)
        {
            if (location == null)
                throw new System.ArgumentNullException("location");
            if (asset == null)
                throw new System.ArgumentNullException("asset");

            return true;
        }
    }

}
