using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace H3D.EditorCResources
{
    public interface IAssetCollector
    {
        void Hanlde(out List<AssetFile> output);
    }

    public class AssetCollectorAttribute:System.Attribute
    {
    
    }
}
