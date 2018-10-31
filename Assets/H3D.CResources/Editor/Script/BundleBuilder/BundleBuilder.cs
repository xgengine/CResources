using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace H3D.EditorCResources
{
    public class BundleBuilder : Operation, IBundleBuidler
    {
        [BuildBundleOption]
        public BuildAssetBundleOptions m_Options;

        public string m_BundleCachePath = "BundleCache/bundles";
        void IBundleBuidler.Hanlde(List<AssetFileGroup> input, out List<BundleFile> output)
        {
            throw new NotImplementedException();
        }
    }

    public class BuildBundleOptionAttribute : PropertyAttribute
    {
        public BuildBundleOptionAttribute()
        {
        }
    }

    [CustomPropertyDrawer(typeof(BuildBundleOptionAttribute))]
    public class MyRangeAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            DepBundleBuilder builder = property.serializedObject.targetObject as DepBundleBuilder;
            builder.m_Options = (BuildAssetBundleOptions)EditorGUI.EnumMaskField(position, "Options", builder.m_Options);
        }
    }
}
