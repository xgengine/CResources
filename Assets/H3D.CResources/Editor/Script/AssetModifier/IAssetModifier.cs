using System.Collections.Generic;

namespace H3D.EditorCResources
{
    public interface IAssetModifier
    {
        void Hanlde(List<AssetFile> input, out List<AssetFile> output);
    }
    public class AssetModifierAttribute : System.Attribute
    {

    }
}