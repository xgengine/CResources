using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ResourceManagement
{
    public interface IResourceLocator
    {
        bool Locate<T>(object key, out IEnumerable<IResourceLocation> locations)
        where T : class;
    }

}

