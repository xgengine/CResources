using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using H3D.CResources;
namespace H3D.EditorCResources
{
    [BundleNameBuilder]
    public class BundleNameBuilder : Operation, IBundleNameBuilder
    {
        public string m_DeletePrefix = "assets/cresources";

        public enum StrategyType
        {
            FileName
        }
        public StrategyType m_StrategyType = StrategyType.FileName;
        void IBundleNameBuilder.Hanlde(List<AssetFile> input, out List<AssetFileGroup> output)
        {
            Dictionary<string, AssetFileGroup> groups = new Dictionary<string, AssetFileGroup>();
            foreach (var assetFile in input)
            {
                string bundleName = CRUtlity.DeleteExtension(assetFile.m_FileLowrPath).Replace("assets/cresources/", "");
                AssetFileGroup aGroup = null;
                if (groups.ContainsKey(bundleName) == false)
                {
                    aGroup = groups[bundleName] = new AssetFileGroup();
                    aGroup.m_AssetFiles = new List<AssetFile>();
                }
                else
                {
                    aGroup = groups[bundleName];
                }
                aGroup.m_BundleName = bundleName;
                aGroup.m_AssetFiles.Add(assetFile);
            }
            output = groups.Values.ToList<AssetFileGroup>();
        }
    }

}
