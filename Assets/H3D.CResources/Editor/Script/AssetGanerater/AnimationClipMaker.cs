using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using H3D.CResources;
namespace H3D.EditorCResources
{
    [AssetGanerater]
    public class AnimationClipMaker : Operation, IAssetGanerater
    {
        public enum MakerType
        {
            Fbx2Clip,
            Clip2Clip
        }
        public MakerType m_MakerType = MakerType.Fbx2Clip;


        public ModelImporterAnimationType m_AnimationType = ModelImporterAnimationType.Legacy;

        void IAssetGanerater.Hanlde(List<AssetFile> input, out List<AssetFile> output)
        {
            output = new List<AssetFile>();
            foreach (var assetFile in input)
            {
                switch(m_MakerType)
                {
                    case MakerType.Fbx2Clip:
                        Fbx2AnimatonClip(assetFile, ref output);
                        break;
                    case MakerType.Clip2Clip:
                        Clip2AnimationClip(assetFile, ref output);
                        break;
                }
                         
            }
        }
        private void Fbx2AnimatonClip(AssetFile assetFile ,ref List<AssetFile> output)
        {
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(assetFile.m_FilePath);
            if (clip != null)
            {
                ModelImporter modelImporter = AssetImporter.GetAtPath(assetFile.m_FilePath) as ModelImporter;
                if (modelImporter != null)
                {
                    if (modelImporter.animationType != m_AnimationType)
                    {
                        Resources.UnloadAsset(clip);
                        modelImporter.animationType = m_AnimationType;
                        modelImporter.SaveAndReimport();
                        clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(assetFile.m_FilePath);
                    }                   
                    AnimationClip outClip = new AnimationClip();
                    outClip.name = name;
                    EditorUtility.CopySerialized(clip, outClip);
                    string clipPath = CRUtlity.DeleteExtension(assetFile.m_FilePath) + ".anim";
                    AssetDatabase.CreateAsset(outClip, clipPath);
                    Resources.UnloadAsset(clip);
                    Resources.UnloadAsset(outClip);
                    AssetFile newFile = new AssetFile()
                    {
                        m_FilePath = clipPath,
                        m_FileLowrPath = clipPath.ToLower()

                    };
                    output.Add(newFile);
                }

            }
        }
        private void Clip2AnimationClip(AssetFile assetFile, ref List<AssetFile> output)
        {
            if(assetFile.m_MainAsset !=null )
            {
                AnimationClip clip = assetFile.m_MainAsset as AnimationClip;
                if(clip!=null)
                {
                    AnimationClip outClip = new AnimationClip();
                    outClip.name = name;
                    EditorUtility.CopySerialized(clip, outClip);
                    string clipPath = CRUtlity.DeleteExtension(assetFile.m_FilePath) + ".anim";

                    AssetDatabase.CreateAsset(outClip, clipPath);

                    Resources.UnloadAsset(clip);
                    Resources.UnloadAsset(outClip);
                    AssetFile newFile = new AssetFile()
                    {
                        m_FilePath = clipPath,
                        m_FileLowrPath = clipPath.ToLower()

                    };
                    output.Add(newFile);
                }
            }
        }
    }
}

