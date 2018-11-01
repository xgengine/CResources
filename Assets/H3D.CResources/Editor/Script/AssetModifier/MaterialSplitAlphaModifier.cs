using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using H3D.CResources;
namespace H3D.EditorCResources
{
    [AssetModifier]
    public class MaterialSplitAlphaModifier : Operation, IAssetModifier
    {

        public string m_AddedSuffix = "_Alpha_ETC1";
        public string m_ShaderPropertyAlphaAddedSuffix = "_ALPHA";

        void IAssetModifier.Hanlde(List<AssetFile> input, out List<AssetFile> output)
        {
            output = input;
            LogUtility.m_LogTag = LogUtility.LogTag.AssetModifier;
            RecordTime();

            List<string> needOperateObjects = new List<string>();
            foreach (var assetFile in input)
            {
                string[] depAssets = AssetDatabase.GetDependencies(assetFile.m_FilePath);
                foreach (var assetPath in depAssets)
                {
                    if (!needOperateObjects.Contains(assetPath))
                    {
                        if (assetPath.EndsWith(".mat", System.StringComparison.Ordinal) && !needOperateObjects.Contains(assetPath))
                        {
                            needOperateObjects.Add(assetPath);
                        }
                    }
                }
            }

            MaterialSplit(needOperateObjects);

            Statistics(input.Count, output.Count);

            StatisticsUseTime();
        }

        private void  MaterialSplit(List<string> matPaths)
        {
            HashSet<string> texCache = new HashSet<string>(); 
            foreach(var matPath in matPaths)
            {
                Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);

                Shader shader = mat.shader;

                if (shader == null)
                {
                    LogUtility.Log("[{0}]{1} Shader is Null ", "MaterialSplitAlphaModifier", matPath);
                    continue;
                }

                string shaderPath = AssetDatabase.GetAssetPath(shader);

                if(shaderPath =="Resources/unity_builtin_extra")
                {
                    LogUtility.LogError("[{0}]{1} Have No Alpha ETC1 Shader ", "MaterialSplitAlphaModifier", matPath);
                    continue;
                }

                if (!shaderPath.EndsWith(m_AddedSuffix + ".shader", System.StringComparison.Ordinal))
                {
                    shaderPath = CRUtlity.DeleteExtension(shaderPath) + m_AddedSuffix + ".shader";
                    if (System.IO.File.Exists(shaderPath))
                    {
                        shader = mat.shader = AssetDatabase.LoadAssetAtPath<Shader>(shaderPath);
                    }
                    else
                    {
                        LogUtility.LogError("[{0}]{1} Have No Alpha ETC1 Shader ", "MaterialSplitAlphaModifier", matPath);
                        continue;
                    }
                }

                for (int i = 0; i < ShaderUtil.GetPropertyCount(shader); ++i)
                {
                    if (ShaderUtil.GetPropertyType(shader, i) == ShaderUtil.ShaderPropertyType.TexEnv)
                    {
                        string propertyName = ShaderUtil.GetPropertyName(shader, i);

                        if (propertyName.EndsWith(m_ShaderPropertyAlphaAddedSuffix, System.StringComparison.Ordinal))
                        {
                            string mainPropertyName = propertyName.Replace(m_ShaderPropertyAlphaAddedSuffix, "");
                            Texture2D tex = (UnityEngine.Texture2D)mat.GetTexture(mainPropertyName);

                            if (tex == null)
                            {
                                LogUtility.LogError("{0} {1} Texture is Null", matPath, mainPropertyName);
                                continue;
                            }
                            string texPath = AssetDatabase.GetAssetPath(tex);
                            string alphaTexPath = CRUtlity.DeleteExtension(texPath) + m_AddedSuffix + ".png";

                            if(!texCache.Contains(texPath))
                            {
                                SplitAlphaTexture(texPath, alphaTexPath);
                                texCache.Add(texPath);
                            }
                            Texture2D alpahTex = AssetDatabase.LoadAssetAtPath<Texture2D>(alphaTexPath);
                            mat.SetTexture(propertyName,alpahTex);

                            Resources.UnloadAsset(alpahTex);
                            Resources.UnloadAsset(tex);
                        }
                    }
                }
            }
        }

        public static void SplitAlphaTexture(string texPath,string alphaTexPath)
        {
            
            TextureImporter texImproter = AssetImporter.GetAtPath(texPath) as TextureImporter;

            if (texImproter != null)
            {
                bool isRaadable = texImproter.isReadable;
                if (!texImproter.isReadable)
                {
                    texImproter.isReadable = true;
                    texImproter.SaveAndReimport();
                }

                Texture2D tex = AssetDatabase.LoadMainAssetAtPath(texPath) as Texture2D;
                CreateAlphaTexture(tex, alphaTexPath);

                Resources.UnloadAsset(tex);
                if (isRaadable == false)
                {
                    texImproter.isReadable = isRaadable;
                    texImproter.SaveAndReimport();
                }
            }
        }

        private static void CreateAlphaTexture(Texture2D texture, string path)
        {
            var texw = texture.width;
            var texh = texture.height;

            Texture2D texAlpha = new Texture2D(texw, texh);
            for (var y = 0; y < texh; y++)
            {
                for (var x = 0; x < texw; x++)
                {
                    Color c = texture.GetPixel(x, y);
                    Color alpha = new Color(c.a, c.a, c.a);
                    texAlpha.SetPixel(x, y, alpha);
                }
            }
            byte[] alphaBuf = texAlpha.EncodeToPNG();
            if (System.IO.File.Exists(path))
            {
                FileUtil.DeleteFileOrDirectory(path);
            }
            System.IO.File.WriteAllBytes(path, alphaBuf);
         
            AssetDatabase.ImportAsset(path);
            DestroyImmediate(texAlpha);

            LogUtility.Log("{0}", path);
        }

        private static bool HasTransparentPixels(Texture2D texture)
        {
            bool hasTrans = false;
            var pixels = texture.GetPixels32();

            foreach (var p in pixels)
            {
                if (p.a < 255)
                {
                    hasTrans = true;
                    break;
                }
            }

            return hasTrans;
        }

    }
}
