using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
namespace H3D.EditorCResources
{
    [BundleNameBuilder]
    public class CResourcesBundleNameBuilder : Operation, IBundleNameBuilder
    {
        void IBundleNameBuilder.Hanlde(List<AssetFile> input, out List<AssetFileGroup> output)
        {
            LogUtility.m_LogTag = LogUtility.LogTag.BundleNamBuidler;

            RecordTime();

            Dictionary<string, List<string>> sharedAssets = new Dictionary<string, List<string>>();

            CollectLoadAndSharedAssets(input,  ref sharedAssets);

            Dictionary<string, AssetFileGroup> sharedGroups = new Dictionary<string, AssetFileGroup>();

            GenerateSharedAssetGroups(sharedAssets,ref sharedGroups);

            output = sharedGroups.Values.ToList();

            Statistics(input.Count, output.Count);

            StatisticsUseTime();
        }

        void CollectLoadAndSharedAssets(List<AssetFile> input,ref Dictionary<string, List<string>> sharedAssets)
        {

            Dictionary<string, HashSet<string>> temp = new Dictionary<string, HashSet<string>>();

            foreach (var assetFile in input)
            {
                if (EditorCRUtlity.CyclicReferenceCheck(assetFile.m_FilePath))
                {
                    throw new CResourcesException("Have Cyclic Reference ：" + assetFile.m_FilePath);
                }
                string[] deps = AssetDatabase.GetDependencies(assetFile.m_FilePath);
                foreach (var aPath in deps)
                {
                    string extension = System.IO.Path.GetExtension(aPath);
                    if (extension.Equals(".cs") || extension.Equals(".js") )
                    {
                        continue;
                    }

                    HashSet<string> refs;
                    if (!temp.ContainsKey(aPath))
                    {
                        refs = new HashSet<string>();
                        temp.Add(aPath,refs);
                    }
                    else
                    {
                        refs = temp[aPath];
                    }

                    if(!refs.Contains(assetFile.m_AssetDatabaseId))
                    {
                        refs.Add(assetFile.m_AssetDatabaseId);
                    }
                }

            }
            foreach(var item in temp)
            {
                sharedAssets.Add(item.Key, new List<string>(item.Value));
            }
        }

      
        void GenerateSharedAssetGroups(Dictionary<string, List<string>> sharedAssets, ref Dictionary<string, AssetFileGroup> sharedGroups)
        {
            foreach (var sAsset in sharedAssets)
            {

                string bundleGUID = sAsset.Value[0];
             
                 
                if(sAsset.Value.Count>1)
                {
                    for (int i = 1; i < sAsset.Value.Count; i++)
                    {
                        bundleGUID = EditorCRUtlity.StringOrOperation(bundleGUID, sAsset.Value[i]);
                    }
                    bundleGUID = EditorCRUtlity.CalauateMD5Code(bundleGUID);
                }
                bool isLoadAsset = false;
                if(sAsset.Value.Count==1)
                {
                    isLoadAsset = true;
                    bundleGUID = "load/" + bundleGUID;
                }
                else
                {
                    isLoadAsset = false;
                    bundleGUID = "sharedassets/" + bundleGUID;
                }
              

                AssetFileGroup group = null;
                if (!sharedGroups.ContainsKey(bundleGUID))
                {
                    group = new AssetFileGroup();
                    group.m_IsLoadAsset = isLoadAsset;
                    group.m_BundleName = bundleGUID;
                    sharedGroups.Add(bundleGUID, group);
                }
                group = sharedGroups[bundleGUID];
                group.m_AssetFiles.Add(new AssetFile()
                {
                    m_AssetDatabaseId = AssetDatabase.AssetPathToGUID(sAsset.Key),
                    m_FilePath = sAsset.Key,
                    m_FileLowrPath = sAsset.Key.ToLower()
                });

            }
        }
    }

}
