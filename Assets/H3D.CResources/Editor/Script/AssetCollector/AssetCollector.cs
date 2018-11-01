using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using H3D.CResources;
namespace H3D.EditorCResources
{
    [AssetCollector]
    public class AssetCollector : Operation, IAssetCollector
    {
        public string m_AssetsPath = "Assets/CResources";
        public string m_UnityFilterText;
        public string m_RegularExpression = "_mat.*";
        public bool m_RegularMatchedInculde = false;
        void IAssetCollector.Hanlde(out List<AssetFile> output)
        {
            LogUtility.m_LogTag = LogUtility.LogTag.AssetCollector;
            output = new List<AssetFile>();

            if (!string.IsNullOrEmpty(m_UnityFilterText))
            {
                string[] assetGuids = AssetDatabase.FindAssets(m_UnityFilterText, new string[] { m_AssetsPath });
                foreach (var guid in assetGuids)
                {
                    string filePath = AssetDatabase.GUIDToAssetPath(guid);
                    AssetFile assetFile = new AssetFile()
                    {
                        m_AssetDatabaseId = guid,
                        m_FilePath = filePath,
                        m_FileLowrPath = filePath.ToLower()
                    };
                    output.Add(assetFile);
                }
            }
            else
            {
                IEnumerable<string> paths = Directory.GetFiles(m_AssetsPath, "*.*", SearchOption.AllDirectories).Where(
                    p => p.EndsWith(".meta") == false && p.EndsWith(".cs") == false
                );
                foreach (var wholeFilePath in paths)
                {
                    string filePath = CRUtlity.UnifyPathSeparator(wholeFilePath).Replace(Application.dataPath, "");
                    string guid = AssetDatabase.AssetPathToGUID(filePath);
                    AssetFile assetFile = new AssetFile()
                    {
                        m_AssetDatabaseId = guid,
                        m_FilePath = filePath,
                        m_FileLowrPath = filePath.ToLower()
                    };
                    output.Add(assetFile);
                }

            }

            if (!string.IsNullOrEmpty(m_RegularExpression))
            {
                Regex regex = new Regex(m_RegularExpression);
                for (int i = output.Count - 1; i >= 0; i--)
                {
                    AssetFile assetFile = output[i];
                    bool isMatched = (regex.IsMatch(assetFile.m_FilePath) || regex.IsMatch(assetFile.m_FileLowrPath));
                    if (m_RegularMatchedInculde != isMatched)
                    {
                        output.RemoveAt(i);
                    }
                }
            }

            Statistics(0, output.Count);
        }
    }

}
