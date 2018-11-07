using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
public class EditorLog : MonoBehaviour {

    [PostProcessBuild(100)]
    public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
    {
        if (target == BuildTarget.Android)
        {
            Debug.Log(System.DateTime.Now.ToString());
        }
    }
    [PostProcessBuild(10000)]
    public static void OnPostProcessBuilds(BuildTarget target, string pathToBuiltProject)
    {
        Debug.Log("结束打包");
    }
    [PostProcessBuild(100000)]
    public static void OnPostProcessBuilds2(BuildTarget target, string pathToBuiltProject)
    {
        Debug.Log("结束打包1");
    }

}
