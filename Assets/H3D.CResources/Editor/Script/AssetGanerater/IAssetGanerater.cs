using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace H3D.EditorCResources
{
    public interface IAssetGanerater
    {
        void Hanlde(List<AssetFile> input, out List<AssetFile> output);
    }
    public class AssetGaneraterAttribute : System.Attribute
    {

    }
}
