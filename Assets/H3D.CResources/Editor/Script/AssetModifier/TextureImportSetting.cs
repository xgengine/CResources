using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
namespace H3D.EditorCResources
{
    [AssetModifier]
    [AssetGanerater]
    public class TextureImportSetting : AssetSetting,IAssetModifier ,IAssetGanerater
    {
        public void Hanlde(List<AssetFile> input, out List<AssetFile> output)
        {
            output = input;
            Apply<TextureImporter>(input,output);
        }

        // ref copy some code from https://github.com/unity3d-jp/AssetGraph
        protected override void  OverwriteImportSettings(AssetImporter importer, AssetImporter templeteImporter)
        {
            TextureImporter reference = templeteImporter as TextureImporter;
            TextureImporter target = importer as TextureImporter;

            target.textureType = reference.textureType;

            TextureImporterSettings dstSettings = new TextureImporterSettings();
            TextureImporterSettings srcSettings = new TextureImporterSettings();

            target.ReadTextureSettings(srcSettings);
            reference.ReadTextureSettings(dstSettings);

            target.SetTextureSettings(dstSettings);

            // some unity version do not properly copy properties via TextureSettings,
            // so also perform manual copy

            target.anisoLevel = reference.anisoLevel;
            target.borderMipmap = reference.borderMipmap;
            target.compressionQuality = reference.compressionQuality;
            target.convertToNormalmap = reference.convertToNormalmap;
            target.fadeout = reference.fadeout;
            target.filterMode = reference.filterMode;
            target.generateCubemap = reference.generateCubemap;
            target.heightmapScale = reference.heightmapScale;

            target.isReadable = reference.isReadable;
            target.maxTextureSize = reference.maxTextureSize;
            target.mipMapBias = reference.mipMapBias;
            target.mipmapEnabled = reference.mipmapEnabled;
            target.mipmapFadeDistanceEnd = reference.mipmapFadeDistanceEnd;
            target.mipmapFadeDistanceStart = reference.mipmapFadeDistanceStart;
            target.mipmapFilter = reference.mipmapFilter;

            target.normalmapFilter = reference.normalmapFilter;
            target.npotScale = reference.npotScale;

            target.wrapMode = reference.wrapMode;

            /* read only */
            // target.qualifiesForSpritePacking

#if !UNITY_5_5_OR_NEWER
            // obsolete features
            target.generateMipsInLinearSpace = reference.generateMipsInLinearSpace;
            target.grayscaleToAlpha = reference.grayscaleToAlpha;
            target.lightmap = reference.lightmap;
            target.linearTexture = reference.linearTexture;
            target.normalmap = reference.normalmap;
            target.textureFormat = reference.textureFormat;

            foreach (var platformName in  new string[] { "Android", "iPhone" })
            {            
                int maxTextureSize;
                TextureImporterFormat format;
                int compressionQuality;
                if (reference.GetPlatformTextureSettings(platformName, out maxTextureSize, out format, out compressionQuality))
                {
                    target.SetPlatformTextureSettings(platformName, maxTextureSize, format, compressionQuality, false);
                }
                else
                {
                    target.ClearPlatformTextureSettings(platformName);
                }
            }
#else
            target.allowAlphaSplitting = reference.allowAlphaSplitting;
            target.alphaIsTransparency = reference.alphaIsTransparency;
            target.textureShape = reference.textureShape;

            target.alphaSource = reference.alphaSource;
            target.sRGBTexture = reference.sRGBTexture;
            target.textureCompression = reference.textureCompression;
            target.crunchedCompression = reference.crunchedCompression;

            foreach (var platformName in  new string[] { "Android", "iOS" })
            {
                var impSet = reference.GetPlatformTextureSettings(platformName);
                target.SetPlatformTextureSettings(impSet);
            }
#endif

#if UNITY_2017_1_OR_NEWER
			target.alphaTestReferenceValue = reference.alphaTestReferenceValue;
			target.mipMapsPreserveCoverage = reference.mipMapsPreserveCoverage;
			target.wrapModeU = reference.wrapModeU;
			target.wrapModeV = reference.wrapModeV;
			target.wrapModeW = reference.wrapModeW;
#endif
        }

        protected override bool IsEqual(AssetImporter importer, AssetImporter templeteImporter)
        {
            TextureImporter reference = templeteImporter as TextureImporter;
            TextureImporter target = importer as TextureImporter;
     
            if (target.textureType != reference.textureType) return false;

            TextureImporterSettings targetSetting = new TextureImporterSettings();
            TextureImporterSettings referenceSetting = new TextureImporterSettings();

            target.ReadTextureSettings(targetSetting);
            reference.ReadTextureSettings(referenceSetting);

            if (!TextureImporterSettings.Equal(targetSetting, referenceSetting))
            {
                return false;
            }

            if (target.textureType == TextureImporterType.Sprite)
            {
                throw new NoSupportException(string.Format("[TextureImportSeting] not support {0}", target.textureType.ToString()));
            }

            if (target.wrapMode != reference.wrapMode) return false;
            if (target.anisoLevel != reference.anisoLevel) return false;
            if (target.borderMipmap != reference.borderMipmap) return false;
            if (target.compressionQuality != reference.compressionQuality) return false;
            if (target.convertToNormalmap != reference.convertToNormalmap) return false;
            if (target.fadeout != reference.fadeout) return false;
            if (target.filterMode != reference.filterMode) return false;
            if (target.generateCubemap != reference.generateCubemap) return false;
            if (target.heightmapScale != reference.heightmapScale) return false;
            if (target.isReadable != reference.isReadable) return false;
            if (target.maxTextureSize != reference.maxTextureSize) return false;
            if (target.mipMapBias != reference.mipMapBias) return false;
            if (target.mipmapEnabled != reference.mipmapEnabled) return false;
            if (target.mipmapFadeDistanceEnd != reference.mipmapFadeDistanceEnd) return false;
            if (target.mipmapFadeDistanceStart != reference.mipmapFadeDistanceStart) return false;
            if (target.mipmapFilter != reference.mipmapFilter) return false;
            if (target.normalmapFilter != reference.normalmapFilter) return false;
            if (target.npotScale != reference.npotScale) return false;

            /* read only properties */
            // target.qualifiesForSpritePacking

#if !UNITY_5_5_OR_NEWER
            // obsolete features
            if (target.normalmap != reference.normalmap) return false;
            if (target.linearTexture != reference.linearTexture) return false;
            if (target.lightmap != reference.lightmap) return false;
            if (target.grayscaleToAlpha != reference.grayscaleToAlpha) return false;
            if (target.generateMipsInLinearSpace != reference.generateMipsInLinearSpace) return false;
            if (target.textureFormat != reference.textureFormat) return false;

            foreach (var platformName in new string[] {"Android","iOS" })
            {              
                int srcMaxTextureSize;
                TextureImporterFormat srcFormat;
                int srcCompressionQuality;

                int dstMaxTextureSize;
                TextureImporterFormat dstFormat;
                int dstCompressionQuality;

                var srcHasSetting = target.GetPlatformTextureSettings(platformName, out srcMaxTextureSize, out srcFormat, out srcCompressionQuality);
                var dstHasSetting = reference.GetPlatformTextureSettings(platformName, out dstMaxTextureSize, out dstFormat, out dstCompressionQuality);

                if (srcHasSetting != dstHasSetting) return false;
                if (srcMaxTextureSize != dstMaxTextureSize) return false;
                if (srcFormat != dstFormat) return false;
                if (srcCompressionQuality != dstCompressionQuality) return false;
            }
#else
            if (target.allowAlphaSplitting != reference.allowAlphaSplitting) return false;
            if (target.alphaIsTransparency != reference.alphaIsTransparency) return false;
            if (target.textureShape != reference.textureShape) return false;

            if (target.alphaSource != reference.alphaSource) return false;
            if (target.sRGBTexture != reference.sRGBTexture) return false;
            if (target.textureCompression != reference.textureCompression) return false;
            if (target.crunchedCompression != reference.crunchedCompression) return false;

            foreach (var platformName in new string[] { "Android", "iPhone" })
            {
                
                var impSet = reference.GetPlatformTextureSettings(platformName);
                var targetImpSet = target.GetPlatformTextureSettings(platformName);
                if (!CompareImporterPlatformSettings(impSet, targetImpSet)) return false;
            }
#endif

#if UNITY_2017_1_OR_NEWER
			if (target.alphaTestReferenceValue != reference.alphaTestReferenceValue) return false;
			if (target.mipMapsPreserveCoverage != reference.mipMapsPreserveCoverage) return false;
			if (target.wrapModeU != reference.wrapModeU) return false;
			if (target.wrapModeV != reference.wrapModeV) return false;
			if (target.wrapModeW != reference.wrapModeW) return false;
#endif
            return true;
        }
#if UNITY_5_5_OR_NEWER
        bool CompareImporterPlatformSettings(TextureImporterPlatformSettings c1, TextureImporterPlatformSettings c2)
        {
            if (c1.allowsAlphaSplitting != c2.allowsAlphaSplitting) return false;
            if (c1.compressionQuality != c2.compressionQuality) return false;
            if (c1.crunchedCompression != c2.crunchedCompression) return false;
            if (c1.format != c2.format) return false;
            if (c1.maxTextureSize != c2.maxTextureSize) return false;
            if (c1.name != c2.name) return false;
            if (c1.overridden != c2.overridden) return false;
            if (c1.textureCompression != c2.textureCompression) return false;

            return true;
        }
#endif


    }
}


