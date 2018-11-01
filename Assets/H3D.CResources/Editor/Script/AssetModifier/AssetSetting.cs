using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
namespace H3D.EditorCResources
{
    public class AssetSetting : Operation
    {
        public UnityEngine.Object m_TempleteObject;

        protected  void Apply<T>(List<AssetFile> input , List<AssetFile> output) where T : AssetImporter
        {
            LogUtility.m_LogTag = LogUtility.LogTag.AssetModifier;

            RecordTime();

            T templeteImproter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(m_TempleteObject)) as T;

            if (templeteImproter == null)
            {
                throw new NullOperationParam(string.Format("[{0}] Template object is null or Type is wrong",GetType()));
            }

            Dictionary<string, AssetImporter> needOperateObjects = new Dictionary<string, AssetImporter>();

            foreach (var assetFile in input)
            {
                string[] depAssets = AssetDatabase.GetDependencies(assetFile.m_FilePath);
                foreach (var assetPath in depAssets)
                {
                    if (!needOperateObjects.ContainsKey(assetPath))
                    {
                        T importer = AssetImporter.GetAtPath(assetPath) as T;
                        if (importer != null)
                        {
                            needOperateObjects.Add(assetPath, importer);
                        }
                    }
                }
            }

            int realOperateCount = 0;
            foreach (var item in needOperateObjects)
            {
                T importer = item.Value as T;
                if (!IsEqual(importer, templeteImproter))
                {
                    OverwriteImportSettings(importer, templeteImproter);
                    realOperateCount++;
                    importer.SaveAndReimport();
                    LogUtility.Log(" Overwrite import seting {0}", item.Key);
                }
            }

            Statistics(input.Count, output.Count, needOperateObjects.Count, realOperateCount);

            StatisticsUseTime();
        }

        protected virtual bool IsEqual( AssetImporter importer, AssetImporter templeteImproter)
        {
            throw new System.NotImplementedException();
        }

        protected virtual void  OverwriteImportSettings(AssetImporter importer, AssetImporter templeteImproter)
        {
            throw new System.NotImplementedException();
        }
    }
}
