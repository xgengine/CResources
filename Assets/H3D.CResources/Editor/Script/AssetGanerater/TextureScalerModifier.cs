using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
namespace H3D.EditorCResources
{
    [AssetGanerater]
    public class TextureScalerModifier : Operation, IAssetGanerater
    {
        public enum TextureOutputType
        {
            PNG,
            JPG,
        }

        public enum TextureFilterType
        {
            Point,
            Bilinear,
        }

        public string m_NameAddedSuffix = "_mat";

        public TextureFilterType m_TextureFilterType = TextureFilterType.Bilinear;

        public TextureOutputType m_TextureOutputType = TextureOutputType.PNG;

        public float m_Scale = 0.2f;

        void IAssetGanerater.Hanlde(List<AssetFile> input, out List<AssetFile> output)
        {
            output = new List<AssetFile>();

            foreach (var assetFile in input)
            {
                TextureImporter texImproter = AssetImporter.GetAtPath(assetFile.m_FilePath) as TextureImporter;
                if (texImproter != null)
                {
                    bool isRaadable = texImproter.isReadable;
                    if (!texImproter.isReadable)
                    {
                        texImproter.isReadable = true;
                        texImproter.SaveAndReimport();
                    }
                    Texture2D outputTex = null;
                    Texture2D tex = AssetDatabase.LoadMainAssetAtPath(assetFile.m_FilePath) as Texture2D;
                    
                    if (tex != null)
                    {
                        switch (m_TextureFilterType)
                        {
                            case TextureFilterType.Bilinear:
                                outputTex = CreateScaledTextureBL(tex);
                                break;
                            case TextureFilterType.Point:
                                outputTex = CreateScaledTexturePT(tex);
                                break;
                        }
                        Resources.UnloadAsset(tex);
                    }

                    if (isRaadable == false)
                    {
                        texImproter.isReadable = isRaadable;
                        texImproter.SaveAndReimport();
                    }

                    if (outputTex != null)
                    {
                        byte[] bytes = null;
                        string pathExtension = "";
                        switch (m_TextureOutputType)
                        {
                            case TextureOutputType.JPG:
                                bytes = outputTex.EncodeToJPG();
                                pathExtension = ".jpg";
                                break;
                            case TextureOutputType.PNG:
                                bytes = outputTex.EncodeToPNG();
                                pathExtension = ".png";
                                break;
                        }
                        string fullPath = Path.GetDirectoryName(assetFile.m_FilePath) + "/" + Path.GetFileNameWithoutExtension(assetFile.m_FilePath) + "_mat" + pathExtension;
                        File.WriteAllBytes(fullPath, bytes);
                        Object.DestroyImmediate(outputTex);
                        AssetDatabase.ImportAsset(fullPath);
                        AssetFile outAssetFile = new AssetFile()
                        {
                            m_AssetDatabaseId = AssetDatabase.AssetPathToGUID(fullPath),
                            m_FilePath = fullPath,
                            m_FileLowrPath = fullPath.ToLower()
                        };
                        output.Add(outAssetFile);
                    }
                }
            }
        }

        private Texture2D CreateScaledTexturePT(Texture2D src)
        {
            var dst = CreateDstTexture(src);
            var dstPix = new Color[dst.width * dst.height];
            int y = 0;
            while (y < dst.height)
            {
                int x = 0;
                while (x < dst.width)
                {
                    int srcX = Mathf.FloorToInt(x / m_Scale);
                    int srcY = Mathf.FloorToInt(y / m_Scale);
                    dstPix[y * dst.width + x] = src.GetPixel((int)srcX, (int)srcY);
                    ++x;
                }
                ++y;
            }
            dst.SetPixels(dstPix);
            dst.Apply();

            return dst;
        }

        private Texture2D CreateScaledTextureBL(Texture2D src)
        {
            var dst = CreateDstTexture(src);
            var dstPix = new Color[dst.width * dst.height];
            int y = 0;
            while (y < dst.height)
            {
                int x = 0;
                while (x < dst.width)
                {
                    float xFrac = x * 1.0F / (dst.width - 1);
                    float yFrac = y * 1.0F / (dst.height - 1);
                    dstPix[y * dst.width + x] = src.GetPixelBilinear(xFrac, yFrac);
                    ++x;
                }
                ++y;
            }
            dst.SetPixels(dstPix);
            dst.Apply();

            return dst;
        }

        private Texture2D CreateDstTexture(Texture2D src)
        {
            int width = (int)(src.width * m_Scale);
            int height = (int)(src.height * m_Scale);
            return new Texture2D(width, height);
        }

    }
}
