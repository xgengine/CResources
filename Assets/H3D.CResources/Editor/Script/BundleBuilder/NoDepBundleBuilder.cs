using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
namespace H3D.EditorCResources
{
    [BundleBuidler]
    public class NoDepBundleBuilder : BundleBuilder, IBundleBuidler
    {
      
        public void Hanlde(List<AssetFileGroup> input, out List<BundleFile> output)
        {
            output = new List<BundleFile>();
            foreach (var assetFileGroup in input)
            {
                AssetBundleBuild[] assetBundleBuilds = new AssetBundleBuild[1];
                assetBundleBuilds[0].assetBundleName = assetFileGroup.m_BundleName;
                assetBundleBuilds[0].assetBundleVariant = assetFileGroup.m_BundleVarintsName;
                string[] assetNames = new string[assetFileGroup.m_AssetFiles.Count];
                for (int i = 0; i < assetFileGroup.m_AssetFiles.Count; i++)
                {
                    assetNames[i] = assetFileGroup.m_AssetFiles[i].m_FilePath;
                }
                assetBundleBuilds[0].assetNames = assetNames;


                if(!System.IO.Directory.Exists(m_BundleCachePath))
                {
                    System.IO.Directory.CreateDirectory(m_BundleCachePath);
                }

                AssetBundleManifest manifest =  BuildPipeline.BuildAssetBundles(m_BundleCachePath, assetBundleBuilds, m_Options, EditorUserBuildSettings.activeBuildTarget);

                if(manifest !=null)
                {
                    foreach (var bundlePath in manifest.GetAllAssetBundles())
                    {
                        BundleFile bundleFile = new BundleFile()
                        {
                            m_BundleName = bundlePath,
                            m_Path = System.IO.Path.Combine(m_BundleCachePath, bundlePath),
                            m_BFType = BundleFile.BFType.Bundle
                        };
                        output.Add(bundleFile);
                    }
                }

            }
        }

       
    }

}
