using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
namespace H3D.EditorCResources
{
    [AssetModifier]
    public class AssetRefrenceChecker : Operation,IAssetModifier
    {
        void IAssetModifier.Hanlde(List<AssetFile> input, out List<AssetFile> output)
        {
            output = new List<AssetFile>();
            foreach(var assetFile in input)
            {
                if(!AssetChecker.IsReferenceMissing(assetFile.m_FilePath))
                {
                    output.Add(assetFile);
                }
            }
        }
    }

    public class AssetChecker
    {

        /// <summary>
        /// 资源检查是否有资源错误
        /// </summary>
        /// <param name="paths"></param>
        /// <returns></returns>
        public static bool IsHaveError(string[] paths)
        {
            bool isHaveError = false;
            if (IsReferenceMissing(paths))
            {
                isHaveError = true;
            }

            return isHaveError;
        }

        /// <summary>
        /// 检查材质个数是否大于submesh个数
        /// </summary>
        /// <param name="paths"></param>
        /// <returns></returns>
        public static bool isMaterialMuchThanSubMesh(string[] paths)
        {

            bool isHaveEror = false;

            foreach (var item in paths)
            {
                if (item.EndsWith(".prefab"))
                {
                    GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(item);
                    SkinnedMeshRenderer[] srenders = obj.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                    foreach (SkinnedMeshRenderer sr in srenders)
                    {
                        if (sr.sharedMesh != null && sr.sharedMaterials != null)
                        {
                            if (sr.sharedMesh.subMeshCount < sr.sharedMaterials.Length)
                            {
                                //ABToolLogWriter.LogError("【制作规范有问题】, 材质球的个数大于submesh的个数，这会造成额外的开销，不被允许，请修改 " + item);
                                isHaveEror = true;
                            }
                        }
                    }
                    MeshRenderer[] mrenders = obj.GetComponentsInChildren<MeshRenderer>(true);
                    foreach (MeshRenderer sr in mrenders)
                    {
                        MeshFilter filter = sr.GetComponent<MeshFilter>();
                        if (filter != null)
                        {
                            if (filter.sharedMesh != null && sr.sharedMaterial != null)
                            {
                                if (filter.sharedMesh.subMeshCount < sr.sharedMaterials.Length)
                                {
                                    //ABToolLogWriter.LogError("【制作规范有问题】, 材质球的个数大于submesh的个数，这会造成额外的开销，不被允许，请修改 " + item + " [" + sr.name + "]");
                                    isHaveEror = true;
                                }
                            }
                        }
                        else
                        {
                            // ABToolLogWriter.LogError("检测到一个物体上拥用MeshRenderer组件，但是没有MeshFilter组件 " + item);
                            isHaveEror = true;
                        }
                    }
                }
            }
            return isHaveEror;
        }

        /// <summary>
        /// 检查引用丢失
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool IsReferenceMissing(string path)
        {
            UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
            string[] result = AssetDatabase.GetDependencies(AssetDatabase.GetAssetPath(obj));
            foreach (var item in result)
            {
                if (!item.EndsWith(".cs") && !item.EndsWith(".js"))
                {
                    if (IsObjectReferenceMissing(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(item)))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool IsReferenceMissing(string[] paths)
        {
            for (int i = 0; i < paths.Length; i++)
            {
                if (IsReferenceMissing(paths[i]))
                {
                    return true;
                }
            }
            return false;
        }

        static bool IsObjectReferenceMissing(Object obj)
        {
            bool isHaveError = false;
            List<Object> checkSos = new List<Object>();
            if (obj is GameObject)
            {
                GameObject go = obj as GameObject;
                Component[] cps = go.GetComponentsInChildren<Component>(true);
                checkSos.AddRange(cps);
            }
            else
            {
                checkSos.Add(obj);
            }
            foreach (var o in checkSos)
            {
                if (o is Material)
                {
                    if (IsMaterialReferenceMissing(o as Material))
                    {
                        return true;
                    }
                }
                else
                {

                    if (o == null)
                    {
                        //ABToolLogWriter.LogError("【引用丢失】[脚本引用丢失] " + AssetDatabase.GetAssetPath(obj));
                        isHaveError = true;
                        continue;
                    }

                    SerializedObject so = new SerializedObject(o);
                    SerializedProperty iter = so.GetIterator();
                    while (iter.NextVisible(true))
                    {
                        if (iter.propertyType == SerializedPropertyType.ObjectReference)
                        {
                            if (iter.objectReferenceValue == null && iter.objectReferenceInstanceIDValue != 0)
                            {
                                ParticleSystem p = iter.serializedObject.targetObject as ParticleSystem;
                                if (p != null)
                                {
                                    if (iter.propertyPath == "ShapeModule.m_Mesh")
                                    {

                                        if (p.shape.shapeType != ParticleSystemShapeType.Mesh)
                                        {
                                            continue;
                                        }
                                    }
                                }
                                ParticleSystemRenderer np = iter.serializedObject.targetObject as ParticleSystemRenderer;
                                if (np != null)
                                {
                                    if (iter.propertyPath == "m_Mesh")
                                    {
                                        if (np.renderMode != ParticleSystemRenderMode.Mesh)
                                        {
                                            continue;
                                        }
                                    }
                                }
                                //ABToolLogWriter.LogError("【引用丢失】[" + o + " " + iter.propertyPath + "] " + AssetDatabase.GetAssetPath(obj));
                                isHaveError = true;
                            }
                        }
                    }
                }
            }
            return isHaveError;
        }
        /// <summary>
        /// 材质球的引用单独提出来，因为多次切换shader SerializedObject会记录以前的引用
        /// </summary>
        /// <param name="mat"></param>
        /// <returns></returns>
        static bool IsMaterialReferenceMissing(Material mat)
        {
            bool isHaveError = false;
            Dictionary<string, bool> dic = new Dictionary<string, bool>();

            SerializedObject so = new SerializedObject(mat);
            SerializedProperty iter = so.GetIterator();
            while (iter.NextVisible(true))
            {
                if (iter.propertyPath.Contains("m_TexEnvs"))
                {
                    if (iter.propertyPath.Contains("m_TexEnvs"))
                    {
                        for (int i = 0; i < iter.FindPropertyRelative("Array").FindPropertyRelative("size").intValue; i++)
                        {
                            SerializedProperty p = iter.FindPropertyRelative("Array").FindPropertyRelative("data[" + i + "]").FindPropertyRelative("second").FindPropertyRelative("m_Texture");
                            dic.Add(iter.FindPropertyRelative("Array").FindPropertyRelative("data[" + i + "]").FindPropertyRelative("first").FindPropertyRelative("name").stringValue, p.objectReferenceValue == null && p.objectReferenceInstanceIDValue != 0);
                        }
                        break;
                    }
                }
            }
            List<string> shaderTexNames = new List<string>();
            List<string> pds = new List<string>();
            Shader s = mat.shader;
            if (!s.name.Contains("Hidden/InternalErrorShader"))
            {
                for (int i = 0; i < ShaderUtil.GetPropertyCount(s); i++)
                {
                    if (ShaderUtil.GetPropertyType(s, i) == ShaderUtil.ShaderPropertyType.TexEnv)
                    {
                        shaderTexNames.Add(ShaderUtil.GetPropertyName(s, i));
                        pds.Add(ShaderUtil.GetPropertyDescription(s, i));
                    }
                }
                foreach (var item in dic)
                {
                    if (shaderTexNames.Contains(item.Key) && item.Value)
                    {
                        string pd = pds[shaderTexNames.FindIndex(p => item.Key == p)];
                        //ABToolLogWriter.LogError("【贴图引用丢失】 <b><font color=\"red\">[" + pd + "]</font></b> " + AssetDatabase.GetAssetPath(mat));
                        isHaveError = true;
                    }
                }
            }
            else
            {
                isHaveError = true;
                //ABToolLogWriter.LogError("【Shader引用丢失】" + AssetDatabase.GetAssetPath(mat));
            }
            return isHaveError;

        }

    }


}

