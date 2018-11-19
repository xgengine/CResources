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
            
                assetBundleBuilds[k].assetNames = assetsPaths.ToArray();
            }

            if (!System.IO.Directory.Exists(cachePath))
            {
                System.IO.Directory.CreateDirectory(cachePath);
            }

            AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(cachePath, assetBundleBuilds, m_Options, EditorUserBuildSettings.activeBuildTarget);

           
          

            if (manifest != null)
            {
                List<string> bundleMd5CodeInfo = new List<string>();
                foreach (var bundlePath in manifest.GetAllAssetBundles())
                {
                    BundleFile bundleFile = new BundleFile()
                    {
                        m_BundleName = bundlePath,
                        m_Path = Path.Combine(cachePath, bundlePath),
                        m_BFType = BundleFile.BFType.Bundle
                    };
                    bundleMd5CodeInfo.Add(bundleFile.m_BundleName + " " + EditorCRUtlity.CalauateMD5CodeFile(bundleFile.m_Path));
                    output.Add(bundleFile);
                }         
                bundleMd5CodeInfo.Sort();
                string logFolder =Path.GetDirectoryName( m_BundleCachePath) + "/log";
                if(!Directory.Exists(logFolder))
                {
                    Directory.CreateDirectory(logFolder);
                }
                File.WriteAllLines(Path.Combine(logFolder, System.DateTime.Now.ToString("yy-MM-dd-HH-mm-ss") + "bunldeBuild.txt"), bundleMd5CodeInfo.ToArray());



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
            public string m_BundleName;
            public int[]  m_dependencies;
        }
        void WriteLocation(List<AssetFileGroup> assetGroups, AssetBundleManifest manifest,string dataPath)
        {
            Dictionary<int, LocationData> maps = new Dictionary<int, LocationData>();
            Dictionary<string, int> bundleNameToLocaiton = new Dictionary<string, int>();
            int location = 0;
            int loadCount = 0;
            foreach (var assetGroup in assetGroups)
            {

                string bundleName = assetGroup.m_BundleName ;
                //List<int> locations = new List<int>();
                //if(assetGroup.m_LoadPaths.Count>0)
                //{

                //    foreach(var assetpath in assetGroup.m_LoadPaths)
                //    {
                //        System.Type type = UnityEditor.AssetDatabase.GetMainAssetTypeAtPath(assetpath);

                //        locations.Add(CResourceLocator.Lcation(CResourceLocator.AssetPathToLoadPath(assetpath), type));

                //    }
                  
                //}
                //else
                //{
                //    locations.Add(bundleName.GetHashCode());
                    
                //}
 
                loadCount += assetGroup.m_LoadPaths.Count;
                
                maps.Add(location,
                    new LocationData()
                    {
                        m_BundleName = bundleName,
                        m_dependencies = null
                    }
                );               
                bundleNameToLocaiton.Add(bundleName, location);
                location += 1;
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
                    bw.Write(loadCount);

                    for(int i =0;i< assetGroups.Count;i++)
                    {
                        foreach(var loadPath in assetGroups[i].m_LoadPaths)
                        {
                            System.Type type = UnityEditor.AssetDatabase.GetMainAssetTypeAtPath(loadPath);
                            bw.Write(CResourceLocator.Lcation(CResourceLocator.AssetPathToLoadPath(loadPath), type));
                            bw.Write(i);
                        }
                    }


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
