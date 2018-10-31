using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace H3D.EditorCResources
{
    public interface IBundleExporter
    {
        void Hanlde(List<BundleFile> input);
    }
    public class BundleExporterAttribute : System.Attribute
    {

    }
}
