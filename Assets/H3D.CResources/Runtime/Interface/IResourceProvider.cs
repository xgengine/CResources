using System.Collections.Generic;
namespace H3D.CResources
{
    public interface IResourceProvider
    {

        string ProviderId { get; }

        CResourceRequest<T> Provide<T>(IResourceLocation location, List<CResourceRequest<object>> loadDependencyOperation)
        where T : class;


        CResourceRequest<T> ProvideAsync<T>(IResourceLocation location, List<CResourceRequest<object>> loadDependencyOperation)
        where T : class;

        bool CanProvide<T>(IResourceLocation location)
        where T : class;


        bool Release(IResourceLocation location, object asset);
    }
}


