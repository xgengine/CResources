using UnityEngine;
using System.Collections;
using System;
using System.IO;
namespace H3D.CResources
{
    public class LogUtility
    {

        public static ILogger logger{
            get{
#if UNITY_2017_1_OR_NEWER
                return Debug.unityLogger;
#else
                return Debug.logger;
#endif
            }
        }

        static string path = Application.persistentDataPath + "/log.txt";

        public static string m_LogTag = LogTag.CResources;

        public static void Log(string format, params object[] args)
        {
            logger.Log(m_LogTag,string.Format(format, args));
            File.AppendAllText(path,string.Format(format,args)+"\r\n");
        }
        
        public static void Log(object message)
        {         
            logger.Log(m_LogTag, message);
            File.AppendAllText(path,message.ToString() + "\r\n");
        }
     
        public static void LogError(object message)
        {
            logger.LogError(m_LogTag, message);
        }
        public static void LogError(string format, params object[] args)
        {
            logger.LogError(m_LogTag, string.Format(format, args));
        }

        public static void LogWarning(object message)
        {
            logger.LogWarning(m_LogTag,message);
        }
        public static void LogWarning(string format, params object[] args)
        {
            logger.LogWarning( m_LogTag, string.Format(format, args));
        }


        //ref https://docs.unity3d.com/Manual/StyledText.html
        public static class LogTag
        {
            public static string CResources = "<color=teal><size=10><b>[CResources]</b></size></color>";
            public static string AssetCollector = string.Concat(CResources, " <color=brown><size=10><b>[AssetCollector]</b></size></color>");
            public static string AssetGanerater = string.Concat(CResources, " <color=maroon><size=10><b>[AssetGanerater]</b></size></color>");
            public static string AssetModifier = string.Concat(CResources, " <color=olive><size=10><b>[AssetModifier]</b></size></color>");
            public static string BundleNamBuidler = string.Concat(CResources, " <color=purple><size=10><b>[BundleNamBuidler]</b></size></color>");
            public static string BundleBuilder = string.Concat(CResources, " <color=green><size=10><b>[BundleBuilder]</b></size></color>");
            public static string BundleExporter = string.Concat(CResources, " <color=aqua ><size=10><b>[BundleExporter]</b></size></color>");
        }
    }
}
