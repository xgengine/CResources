using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
namespace H3D.EditorCResources
{
    [PlayerBuilder]
    public class PlayerBuilder : Operation,IPlayerBuilder
    {
        public bool m_isDevelopment = true;
        public bool m_isScriptDebugging = true;
        public string m_version = "0.0.0";
        public string m_outPath = "player_output";

        private static string m_EnableKey = "_PlayerBuilder_";

        void IPlayerBuilder.Hanlde()
        {
            EditorPrefs.SetBool(m_EnableKey, true);

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
                    option = option | BuildOptions.AllowDebugging|BuildOptions.AutoRunPlayer|BuildOptions.ConnectWithProfiler;
                }
            }
            
            

            switch(EditorUserBuildSettings.activeBuildTarget)
            {
                case BuildTarget.StandaloneWindows64:
                    WindowsBuild(buildScenes,option);
                    break;
                case BuildTarget.Android:
                    AndroidBuild(buildScenes,option);
                    break;
            }

        }
        static string m_windowsPlayerPath;
        void WindowsBuild(List<string> buildScenes, BuildOptions option)
        {
            string folder= System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-s-ff");
            string outPath = m_outPath + "/" + folder+"/"+Application.productName+".exe";
            m_windowsPlayerPath = outPath;
            BuildPipeline.BuildPlayer(buildScenes.ToArray(),outPath , BuildTarget.StandaloneWindows64, option);
        }

        void AndroidBuild(List<string> buildScenes, BuildOptions option)
        {
            string apkName = System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-s-ff") + ".apk";
            
            BuildPipeline.BuildPlayer(buildScenes.ToArray(), m_outPath + "/" + apkName, BuildTarget.Android, option);
        }

        //[PostProcessBuild(1)]
        //public static void RecordBuildEnd(BuildTarget target, string pathToBuiltProject)
        //{
        //    if (EditorPrefs.GetBool(m_EnableKey, false))
        //    {
        //        switch (EditorUserBuildSettings.activeBuildTarget)
        //        {
        //            case BuildTarget.StandaloneWindows64:
        //                System.Diagnostics.Process.Start(Path.GetFullPath(m_windowsPlayerPath));
        //                break;
        //        }
        //        EditorPrefs.SetBool(m_EnableKey, false);
        //    }

        //}


    }

}
