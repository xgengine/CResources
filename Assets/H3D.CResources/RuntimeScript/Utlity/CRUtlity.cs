using UnityEngine;
using System.IO;
namespace H3D.CResources
{
    public static class CRUtlity
    {
        public static string FullPathToAssetPath(string path)
        {
            return UnifyPathSeparator(path).Replace(Application.dataPath, "");
        }

        public static string UnifyPathSeparator(string path)
        {
            return path.Replace("\\", "/");
        }

        public static string ReplaceExtension(string filePath, string newExtension)
        {
            return filePath.Substring(0, filePath.LastIndexOf(".")) + newExtension;
        }

        public static string DeleteExtension(string filePath)
        {
            return filePath.Substring(0, filePath.LastIndexOf("."));
        }

        public static string GetAddedName(string path)
        {
            string addedName = path;
            int index = 0;
            while (File.Exists(addedName))
            {
                index++;
                string extension = Path.GetExtension(path);
                string noExtName = DeleteExtension(path);
                addedName = string.Format("{0} {1}{2}", noExtName, index, extension);
            }
            return addedName;
        }
    }
}
