using System.Collections.Generic;

namespace H3D.EditorCResources
{
    public interface IBundleBuidler
    {
        void Hanlde(List<AssetFileGroup> input, out List<BundleFile> output);
    }
    public class BundleBuidlerAttribute : System.Attribute
    {

    }
}

