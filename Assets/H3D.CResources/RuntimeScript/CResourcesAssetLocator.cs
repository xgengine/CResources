using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
namespace H3D.CResources
{
    public class CResourcesAssetLocator : ICResourcesLocator
    {
#if UNITY_EDITOR

        static CResourcesAssetLocator()
        {

        }
        public static void WriteLocationData(string dataPath)
        {
            Dictionary<int, string> maps = new Dictionary<int, string>();

            IEnumerable<string> paths = Directory.GetFiles(ConstValue.m_PackPath, "*.*", SearchOption.AllDirectories).Where(
                    p => p.EndsWith(".meta") == false && p.EndsWith(".cs") == false
                );

            foreach (var wholeFilePath in paths)
            {              
                string assetPath = CRUtlity.FullPathToAssetPath(wholeFilePath);
                string guid = UnityEditor.AssetDatabase.AssetPathToGUID(assetPath);
                System.Type type = UnityEditor.AssetDatabase.GetMainAssetTypeAtPath(assetPath);
                maps.Add(Lcation(AssetPathToLoadPath(assetPath),type),guid);
            }

            using (Stream stream = new FileStream(dataPath, FileMode.Create))
            {
                using (var bw = new BinaryWriter(stream))
                {
                    bw.Write(maps.Count);
                    foreach (var item in maps)
                    {
                        bw.Write(item.Key);
                        bw.Write(item.Value);
                    }
                }
            }    
        }
#endif

        public static void ReadLoactionData(string dataPath)
        {
            using (Stream stream = new FileStream(dataPath, FileMode.Open))
            {
                using (var br = new BinaryReader(stream))
                {
                    int count = br.ReadInt32();
                    m_AssetMaps = new Dictionary<int, string>(count);
                    for (int i = 0; i < count; i++)
                    {
                        m_AssetMaps.Add(br.ReadInt32(), br.ReadString());
                    }
                }
            }   
        }

        public static string AssetPathToLoadPath(string assetPath)
        {
            return assetPath.ToLower().Replace(ConstValue.m_PackPath, "");
        }

        public static int Lcation(string loadPath, System.Type type)
        {
            return string.Concat(loadPath.ToLower(), type.Name).GetHashCode();

        }

        protected static Dictionary<int, string> m_AssetMaps;

        public ICResourcesLocation GetLocation(string requestID)
        {
            throw new System.NotImplementedException();
        }
    }
}

