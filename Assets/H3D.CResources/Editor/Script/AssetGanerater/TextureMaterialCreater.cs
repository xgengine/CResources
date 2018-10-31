using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using H3D.CResources;
namespace H3D.EditorCResources
{
    [AssetGanerater]
    public class TextureMaterialCreater : Operation, IAssetGanerater
    {
        public Material m_SoruceMaterial = null;

        void IAssetGanerater.Hanlde(List<AssetFile> input, out List<AssetFile> output)
        {
            output = new List<AssetFile>();
            foreach (var assetFile in input)
            {
                if (assetFile.m_MainAssetType == typeof(Texture2D) || AssetImporter.GetAtPath(assetFile.m_FilePath) is TextureImporter)
                {
                    Material material = new Material(m_SoruceMaterial);
                    Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(assetFile.m_FilePath);
                    material.mainTexture = tex;
                    string matPath = CRUtlity.ReplaceExtension(assetFile.m_FilePath, ".mat");
                    AssetDatabase.CreateAsset(material, matPath);
                    Resources.UnloadAsset(tex);
                    output.Add(new AssetFile()
                    {
                        m_FilePath = matPath,
                        m_FileLowrPath = matPath.ToLower()
                    }
                    );
                }
            }

        }
    }
}

