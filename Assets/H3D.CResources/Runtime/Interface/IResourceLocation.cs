using System.Collections;
using System.Collections.Generic;
namespace H3D.CResources
{
    public interface IResourceLocation
    {

        string InternalId { get; }

        string ProviderId { get; }

        IList<IResourceLocation> Dependencies { get; }

        bool HasDependencies { get; }


    }
}

