using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
namespace H3D.EditorCResources
{
    [AssetModifier]
    public class AudioImportSetting : AssetSetting, IAssetModifier
    {
        void IAssetModifier.Hanlde(List<AssetFile> input, out List<AssetFile> output)
        {
            output = input;

            Apply<AudioImporter>(input,output);
        }
        // ref copy some code from https://github.com/unity3d-jp/AssetGraph
        private bool IsEqualAudioSampleSetting(AudioImporterSampleSettings target, AudioImporterSampleSettings reference)
        {
            // defaultSampleSettings
            if (target.compressionFormat != reference.compressionFormat) return false;
            if (target.loadType != reference.loadType) return false;
            if (target.quality != reference.quality) return false;
            if (target.sampleRateOverride != reference.sampleRateOverride) return false;
            if (target.sampleRateSetting != reference.sampleRateSetting) return false;

            return true;
        }

        protected override bool IsEqual(AssetImporter importer, AssetImporter templeteImproter)
        {
            AudioImporter reference = templeteImproter as AudioImporter;

            AudioImporter target = importer as AudioImporter;

            UnityEngine.Assertions.Assert.IsNotNull(reference);

            if (!IsEqualAudioSampleSetting(target.defaultSampleSettings, reference.defaultSampleSettings))
            {
                return false;
            }

            foreach (var platformName in new string[] { "Android", "iOS" })
            {
               
                if (target.ContainsSampleSettingsOverride(platformName) !=
                   reference.ContainsSampleSettingsOverride(platformName))
                {
                    return false;
                }
                if (target.ContainsSampleSettingsOverride(platformName))
                {
                    var t = target.GetOverrideSampleSettings(platformName);
                    var r = reference.GetOverrideSampleSettings(platformName);
                    if (!IsEqualAudioSampleSetting(t, r))
                    {
                        return false;
                    }
                }
            }

            if (target.forceToMono != reference.forceToMono)
                return false;
            // using "!UNITY_5_6_OR_NEWER" instead of "Unity_5_6" because loadInBackground became obsolete after Unity 5.6b3.
#if !UNITY_5_6_OR_NEWER
            if (target.loadInBackground != reference.loadInBackground)
                return false;
#endif

#if UNITY_2017_1_OR_NEWER
            if (target.ambisonic != reference.ambisonic)
                return false;
#endif
            if (target.preloadAudioData != reference.preloadAudioData)
                return false;

            return true;

        }

        protected override void  OverwriteImportSettings(AssetImporter importer, AssetImporter templeteImproter)
        {
            var reference = templeteImproter as AudioImporter;
            var target = importer as AudioImporter;
            UnityEngine.Assertions.Assert.IsNotNull(reference);

            target.defaultSampleSettings = reference.defaultSampleSettings;
            target.forceToMono = reference.forceToMono;
            target.preloadAudioData = reference.preloadAudioData;

            foreach (var platformName in new string[] { "Android", "iOS" })
            {
               
                if (reference.ContainsSampleSettingsOverride(platformName))
                {
                    var setting = reference.GetOverrideSampleSettings(platformName);
                    if (!target.SetOverrideSampleSettings(platformName, setting))
                    {
                        Debug.LogError("AudioImporter "+string.Format("Failed to set override setting for {0}: {1}", platformName, target.assetPath));
                    }
                }
                else
                {
                    target.ClearSampleSettingOverride(platformName);
                }
            }

            // using "!UNITY_5_6_OR_NEWER" instead of "Unity_5_6" because loadInBackground became obsolete after Unity 5.6b3.
#if !UNITY_5_6_OR_NEWER
            target.loadInBackground = reference.loadInBackground;
#endif

#if UNITY_2017_1_OR_NEWER
            target.ambisonic = reference.ambisonic;
#endif
        }

    }

}
