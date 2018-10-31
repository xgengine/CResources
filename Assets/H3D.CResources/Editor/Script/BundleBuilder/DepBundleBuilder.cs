using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
namespace H3D.EditorCResources
{
    [BundleBuidler]
    public class DepBundleBuilder : BundleBuilder, IBundleBuidler
    {
        public void Hanlde(List<AssetFileGroup> input, out List<BundleFile> output)
        {
            string cachePath = System.IO.Path.GetFullPath(m_BundleCachePath);
            AssetBundleBuild[] assetBundleBuilds = new AssetBundleBuild[input.Count];
            output = new List<BundleFile>();
            for (int k =0;k<input.Count;k++ )
            {
                AssetFileGroup assetFileGroup = input[k];
               
                assetBundleBuilds[k].assetBundleName = assetFileGroup.m_BundleName;
                assetBundleBuilds[k].assetBundleVariant = assetFileGroup.m_BundleVarintsName;
               
                List<string> assetsPaths = new List<string>();
                for (int i = 0; i < assetFileGroup.m_AssetFiles.Count; i++)
                {
                    assetsPaths.Add(assetFileGroup.m_AssetFiles[i].m_FilePath);
                }
                assetsPaths.Sort();
                assetBundleBuilds[k].assetNames = assetsPaths.ToArray();
            }

            if (!System.IO.Directory.Exists(cachePath))
            {
                System.IO.Directory.CreateDirectory(cachePath);
            }

            AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(cachePath, assetBundleBuilds, m_Options, EditorUserBuildSettings.activeBuildTarget);

            if (manifest != null)
            {
                foreach (var bundlePath in manifest.GetAllAssetBundles())
                {
                    BundleFile bundleFile = new BundleFile()
                    {
                        m_BundleName = bundlePath,
                        m_Path = System.IO.Path.Combine(cachePath, bundlePath),
                        m_BFType = BundleFile.BFType.Bundle
                    };
                    output.Add(bundleFile);
                }


                BundleFile manifestFile = new BundleFile()
                {
                    m_BundleName = System.IO.Path.GetFileName(cachePath),
                    m_Path = System.IO.Path.Combine(cachePath ,System.IO.Path.GetFileName(cachePath)),
                    m_BFType = BundleFile.BFType.Manifest
                };
                output.Add(manifestFile);


                H3D.CResources.CResourceLocator.WriteLocationData(System.IO.Path.Combine(cachePath,"loactions"));

                BundleFile loactions = new BundleFile()
                {
                    m_BundleName = "loactions",
                    m_Path = System.IO.Path.Combine(cachePath, "loactions"),
                    m_BFType = BundleFile.BFType.Location
                };

                output.Add(loactions);

            }

           
        }

    }
}
