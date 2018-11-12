using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
namespace H3D.EditorCResources
{
    [BundleExporter]
    public class BundleExporter : Operation, IBundleExporter
    {
        public bool   m_DeleteExporterFolder = true;
        public string m_ExproterPath;
        public bool   m_IsCopyManifest = false;
        void IBundleExporter.Hanlde(List<BundleFile> input)
        {
            string exproterPath = Path.GetFullPath(m_ExproterPath);

            EditorCRUtlity.DeleteDirectory(exproterPath);

           
            foreach(var bundleFile in input)
            {

                string outPath =bundleFile.m_BFType== BundleFile.BFType.Manifest? Path.Combine(exproterPath, "manifest"):  Path.Combine(exproterPath, bundleFile.m_BundleName);
                string outManifestPath = outPath+".manifest";

                EditorCRUtlity.CopyFile(bundleFile.m_Path, outPath);

                if(bundleFile.m_BFType != BundleFile.BFType.Location && m_IsCopyManifest == true)
                {
                    EditorCRUtlity.CopyFile(bundleFile.m_Path + ".manifest", outManifestPath);
                }
               
            }
            System.Diagnostics.Process.Start(exproterPath);

        }

        void ModyfiyFile(string path)
        {
           
        }

    }

}

