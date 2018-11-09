using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
public class EditorBuildPlayer {

    private string outFolder = "PlayerOutput";

    public void Build()
    {
        List<string> buildScenes = new List<string>();
        foreach (var scene in EditorBuildSettings.scenes)
        {
            if (scene.enabled)
            {
                buildScenes.Add(scene.path);
            }
        }
        BuildOptions option = BuildOptions.None;
       
        if (!System.IO.Directory.Exists(outFolder))
        {
            System.IO.Directory.CreateDirectory(outFolder);
        }
        string apkName = System.DateTime.Now.ToString() + ".apk";
        string outPath = outFolder + "/" + apkName;

        BuildPipeline.BuildPlayer(buildScenes.ToArray(), outPath, BuildTarget.Android, option);

    }

}
