using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using H3D.CResources;
using System.IO;
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
                        m_Path = Path.Combine(cachePath, bundlePath),
                        m_BFType = BundleFile.BFType.Bundle
                    };
                    output.Add(bundleFile);
                }

                //BundleFile manifestFile = new BundleFile()
                //{
                //    m_BundleName = Path.GetFileName(cachePath),
                //    m_Path = Path.Combine(cachePath ,System.IO.Path.GetFileName(cachePath)),
                //    m_BFType = BundleFile.BFType.Manifest
                //};
                //output.Add(manifestFile);

                string locationPath = Path.Combine(cachePath, "locations");
                WriteLocation(input, manifest, locationPath);

                BundleFile loactions = new BundleFile()
                {
                    m_BundleName = "locations",
                    m_Path = locationPath,
                    m_BFType = BundleFile.BFType.Location
                };

                output.Add(loactions);

            }        
        }

        class LocationData
        {
            public string m_LoadPath;
            public string m_BundleName;
            public int[]  m_dependencies;
        }
        void WriteLocation(List<AssetFileGroup> assetGroups, AssetBundleManifest manifest,string dataPath)
        {
            Dictionary<int, LocationData> maps = new Dictionary<int, LocationData>();
            Dictionary<string, int> bundleNameToLocaiton = new Dictionary<string, int>();

            foreach (var assetGroup in assetGroups)
            {

                string bundleName = assetGroup.m_BundleName ;
                string assetPath = assetGroup.m_AssetFiles[0].m_FilePath;
                System.Type type = UnityEditor.AssetDatabase.GetMainAssetTypeAtPath(assetPath);
                int location;
                if(assetGroup.m_IsLoadAsset)
                {      
                    location= CResourceLocator.Lcation(CResourceLocator.AssetPathToLoadPath(assetPath), type);
                }
                else
                {
                    location = bundleName.GetHashCode();
                }
                maps.Add(location,
                    new LocationData()
                    {
                        m_BundleName = bundleName,
                        m_dependencies = null
                    }
                );
                bundleNameToLocaiton.Add(bundleName,location);        
            }
            foreach(var bundleData in maps)
            {
                string[] deps = manifest.GetDirectDependencies(bundleData.Value.m_BundleName);

                bundleData.Value.m_dependencies = new int[deps.Length];

                for(int i=0;i<deps.Length;i++)
                {
                    bundleData.Value.m_dependencies[i] = bundleNameToLocaiton[deps[i]];
                }
            }

            using (Stream stream = new FileStream(dataPath, FileMode.Create))
            {
                using (var bw = new BinaryWriter(stream))
                {
                    bw.Write(maps.Count);
                    foreach (var item in maps)
                    {
                        bw.Write(item.Key);
                        bw.Write(item.Value.m_BundleName);               
                    }
                    foreach (var item in maps)
                    {
                        bw.Write(item.Value.m_dependencies.Length);
                        for (int i = 0; i < item.Value.m_dependencies.Length; i++)
                        {
                            bw.Write(item.Value.m_dependencies[i]);
                        }
                    }
                }
            }
        }

    }
}
