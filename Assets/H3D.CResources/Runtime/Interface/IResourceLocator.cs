using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace H3D.CResources
{
    public interface IResourceLocator
    {
        IResourceLocation Locate<T>(object requestI)
        where T : class;
    }
}

