using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace H3D.EditorCResources
{
    public interface IBundleNameBuilder
    {
        void Hanlde(List<AssetFile> input, out List<AssetFileGroup> output);
    }
    public class BundleNameBuilderAttribute : System.Attribute
    {

    }
}
