using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;
namespace H3D.EditorCResources
{
    [PlayerBuilder]
    public class PlayerBuilder : Operation,IPlayerBuilder
    {
        public bool m_isDevelopment = false;
        public bool m_isScriptDebugging = true;
        public string m_version = "0.0.0";
        public string m_outPath = "PlayerOutput";
        void IPlayerBuilder.Hanlde()
        { 
            if(!Directory.Exists(m_outPath))
            {
                Directory.CreateDirectory(m_outPath);
            }

            List<string> buildScenes = new List<string>();
            foreach (var scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled)
                {
                    buildScenes.Add(scene.path);
                }
            }

            BuildOptions option = BuildOptions.None;
            if (m_isDevelopment)
            {
                option = option | BuildOptions.Development;
                if (m_isScriptDebugging)
                {
                    option = option | BuildOptions.AllowDebugging;
                }
            }
            WindowsBuild(buildScenes, option);
           // AndroidBuild(buildScenes, option);
        }
        void WindowsBuild(List<string> buildScenes, BuildOptions option)
        {
            string folder= System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-s-ff");
            string outPath = m_outPath + "/" + folder+"/runer.exe";
  
            
            BuildPipeline.BuildPlayer(buildScenes.ToArray(),outPath , BuildTarget.StandaloneWindows64, option);
        }

        void AndroidBuild(List<string> buildScenes, BuildOptions option)
        {
            string apkName = System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-s-ff") + ".apk";
            
            BuildPipeline.BuildPlayer(buildScenes.ToArray(), m_outPath + "/" + apkName, BuildTarget.Android, option);
        }
    }

}
