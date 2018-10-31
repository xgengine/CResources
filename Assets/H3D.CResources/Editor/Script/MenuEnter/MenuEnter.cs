using UnityEngine;
using System.Collections;
using UnityEditor;
using H3D.CResources;
namespace H3D.EditorCResources
{
    public class MenuEnter
    {
        [MenuItem("Assets/Create/CResources/TempleteAssetSettring/Model")]
        static void CreateModelSettingTemplate()
        {
            CreateTemplateAsset<GameObject>("setting.fbx");
        }
     
        [MenuItem("Assets/Create/CResources/TempleteAssetSettring/Texture")]
        static void CreateTextureSettingTemplate()
        {
            CreateTemplateAsset<Texture2D>("setting.png");
        }

        [MenuItem("Assets/Create/CResources/TempleteAssetSettring/Audio")]
        static void CreateAudioSettingTemplate()
        {
            CreateTemplateAsset<AudioClip>("setting.wav");
        }

        private static void CreateTemplateAsset<T>(string name) where T : Object
        {
            if (Selection.assetGUIDs.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[0]);
                if (!AssetDatabase.IsValidFolder(path))
                {
                    path = System.IO.Path.GetDirectoryName(path);
                }

                string createPath = CRUtlity.GetAddedName( path + "/" + name);
                FileUtil.CopyFileOrDirectory("Assets/H3D.CResources/SettingTemplate/" + name, createPath);
                AssetDatabase.ImportAsset(createPath);
                Object obj = AssetDatabase.LoadAssetAtPath<T>(createPath);
                Selection.activeObject = obj;
                EditorGUIUtility.PingObject(obj);
            }
        }
    }
}

