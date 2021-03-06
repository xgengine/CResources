﻿using UnityEngine;
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
        void IBundleExporter.Hanlde(List<BundleFile> input)
        {
            string exproterPath = Path.GetFullPath(m_ExproterPath);

            EditorCRUtlity.DeleteDirectory(exproterPath);

            List<string> bundleMd5CodeInfo = new List<string>();

            foreach(var bundleFile in input)
            {

                string outPath =bundleFile.m_BFType== BundleFile.BFType.Manifest? Path.Combine(exproterPath, "manifest"):  Path.Combine(exproterPath, bundleFile.m_BundleName);
                string outManifestPath = outPath+".manifest";

                EditorCRUtlity.CopyFile(bundleFile.m_Path, outPath);

                if(bundleFile.m_BFType != BundleFile.BFType.Location)
                {
                    EditorCRUtlity.CopyFile(bundleFile.m_Path + ".manifest", outManifestPath);
                }
                bundleMd5CodeInfo.Add(bundleFile.m_BundleName + " " + EditorCRUtlity.CalauateMD5CodeFile(outPath));
            }

            bundleMd5CodeInfo.Sort();
            File.WriteAllLines(Path.Combine(Path.GetDirectoryName(exproterPath),System.DateTime.Now.ToString("yy-MM-dd-HH-mm-ss")+"bunldeBuild.txt"),bundleMd5CodeInfo.ToArray());
            System.Diagnostics.Process.Start(exproterPath);
        }

        void ModyfiyFile(string path)
        {
           
        }

    }

}

