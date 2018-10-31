using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

namespace H3D.EditorCResources
{
    [AssetModifier]
    [AssetGanerater]
    public class AnimationCompressor : Operation, IAssetModifier,IAssetGanerater
    {
        [Range(9,1)]
        public uint  m_FloatAccuracy = 3;

        [Range(0.0001f,0.1f)]
        public float m_DuplicateFramePrecision = 0.009f;

        public void Hanlde(List<AssetFile> input, out List<AssetFile> output)
        {
            output = input;
            foreach (var assetFile in input)
            {
                AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(assetFile.m_FilePath);
                if (clip != null)
                {
                    RemoveDuplicateFrameData(clip);
                    FloatAccuracyCompress(clip);
                    assetFile.m_MainAsset = clip;
                }
            }
        }
        /// <summary>
        /// 浮点压缩
        /// </summary>
        /// <param name="clip"></param>
        /// <param name="精度"></param>
        private void FloatAccuracyCompress(AnimationClip clip)
        {
            uint accuracy = m_FloatAccuracy;
            EditorCurveBinding[] curveBindings = AnimationUtility.GetCurveBindings(clip);
            Keyframe key;
            Keyframe[] keyFrames;
            Dictionary<uint, string> _FLOAT_FORMAT;
            _FLOAT_FORMAT = new Dictionary<uint, string>();
            for (uint i = 1; i < 6; i++)
            {
                _FLOAT_FORMAT.Add(i, "f" + i.ToString());
            }
            string floatFormat;
            if (_FLOAT_FORMAT.TryGetValue(accuracy, out floatFormat))
            {
                if (curveBindings != null && curveBindings.Length > 0)
                {
                    for (int i = 0; i < curveBindings.Length; i++)
                    {
                        AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, curveBindings[i]);
                        if (curve == null || curve.keys == null)
                        {
                            continue;
                        }
                        keyFrames = curve.keys;
                        for (int j = 0; j < keyFrames.Length; j++)
                        {
                            key = keyFrames[j];
                            key.value = float.Parse(key.value.ToString(floatFormat));
                            key.inTangent = float.Parse(key.inTangent.ToString(floatFormat));
                            key.outTangent = float.Parse(key.outTangent.ToString(floatFormat));
                            keyFrames[j] = key;
                        }
                        curve.keys = keyFrames;
                        // _clip.SetCurve(curveBindings[i].path, curveBindings[i].type, curveBindings[i].propertyName, curve); // 删除编辑器曲线
                        AnimationUtility.SetEditorCurve(clip, curveBindings[i], curve);
                    }
                }
            }
            EditorUtility.SetDirty(clip);

        }
        /// <summary>
        /// 去掉重复帧数据
        /// </summary>
        /// <param name="clip"></param>
        private void RemoveDuplicateFrameData(AnimationClip clip)
        {
            float floatPrecision = 0.009f;
            EditorCurveBinding[] ecbs = AnimationUtility.GetCurveBindings(clip);
            for (int i = 0; i < ecbs.Length; ++i)
            {
                EditorCurveBinding ecb = ecbs[i];
                AnimationCurve acurve = AnimationUtility.GetEditorCurve(clip, ecb);
                Keyframe[] kfs = acurve.keys;
                bool bDel = false;
                if (kfs.Length > 2
                    && Mathf.Abs(kfs[0].value - kfs[kfs.Length - 1].value) <= floatPrecision
                    && Mathf.Abs(kfs[kfs.Length - 1].time - clip.length) <= floatPrecision
                    && Mathf.Abs(kfs[0].inTangent - kfs[kfs.Length - 1].inTangent) <= floatPrecision
                    && Mathf.Abs(kfs[0].outTangent - kfs[kfs.Length - 1].outTangent) <= floatPrecision
                    )
                {
                    bDel = true;
                    float val = kfs[0].value;
                    for (int j = 1; j < kfs.Length - 1; ++j)
                    {
                        if (Mathf.Abs(val - kfs[j].value) > floatPrecision
                                || Mathf.Abs(kfs[0].inTangent - kfs[j].inTangent) > floatPrecision
                                || Mathf.Abs(kfs[0].outTangent - kfs[j].outTangent) > floatPrecision
                            )
                        {
                            bDel = false;
                            break;
                        }
                    }

                    if (bDel)
                    {
                        for (int k = 1; k < acurve.length - 1;)
                            acurve.RemoveKey(1);
                    }
                }

                if (bDel)
                    AnimationUtility.SetEditorCurve(clip, ecb, acurve);
            }
            EditorUtility.SetDirty(clip);
        }


    }


}
