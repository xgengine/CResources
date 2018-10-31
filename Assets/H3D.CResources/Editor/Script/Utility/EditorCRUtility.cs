using UnityEngine;
using System.Collections;
using System.IO;
using UnityEditor;
using System.Collections.Generic;
using System.Security.Cryptography;
namespace H3D.EditorCResources
{
    public static class EditorCRUtlity
    {
        public static List<System.Type> GetTypes(System.Type attributeType)
        {
            List<System.Type> typeList = new List<System.Type>();
            foreach (System.Type type in System.Reflection.Assembly.GetExecutingAssembly().GetTypes())
            {
                object[] atrributes = type.GetCustomAttributes(attributeType, false);
                if (atrributes != null && atrributes.Length > 0)
                {
                    typeList.Add(type);
                }
            }
            return typeList;
        }

        public static string GetClassName(System.Type type)
        {
            string typeString = type.ToString();
            return typeString.Substring(typeString.LastIndexOf(".") + 1);
        }

        public static void DeleteDirectory(string directory)
        {
            if (Directory.Exists(directory))
            {
                foreach (var filePath in Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories))
                {
                    FileAttributes attributes = File.GetAttributes(filePath);
                    if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                    {
                        File.SetAttributes(filePath, FileAttributes.Normal);
                    }
                }

                Directory.Delete(directory, true);
            }
        }

        public static void CopyFile(string sourceFileName, string destFileName)
        {
            if (File.Exists(sourceFileName))
            {
                string folder = Path.GetDirectoryName(destFileName);
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                DeletFile(destFileName);
                File.Copy(sourceFileName, destFileName);
            }
        }

        public static void DeletFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                FileAttributes attributes = File.GetAttributes(filePath);
                if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {
                    File.SetAttributes(filePath, FileAttributes.Normal);
                }
                File.Delete(filePath);
            }
        }

        public static string CalauateMD5Code(string info)
        {
            byte[] data = System.Text.Encoding.Default.GetBytes(info);
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] targetData = md5.ComputeHash(data);
            return System.BitConverter.ToString(targetData).Replace("-", "").ToLower();
        }

        public static string CalauateMD5CodeFile(string filePath)
        {
            byte[] data = File.ReadAllBytes(filePath);
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] targetData = md5.ComputeHash(data);
            return System.BitConverter.ToString(targetData).Replace("-", "");
        }

        public static string StringOrOperation(string a, string b)
        {
            if (a.Length != b.Length)
            {
                throw new CResourcesException("String Or peration input Length is not same");
            }

            byte[] byteArrayA = System.Text.Encoding.Default.GetBytes(a);
            byte[] byteArrayB = System.Text.Encoding.Default.GetBytes(b);
            byte[] outArray = new byte[byteArrayA.Length];
            for (int i = 0; i < byteArrayA.Length; i++)
            {
                outArray[i] = (byte)((uint)byteArrayA[i] | (uint)byteArrayB[i]);
            }
            return System.Text.Encoding.Default.GetString(outArray);
        }

        public static bool CyclicReferenceCheck(string path)
        {
            return CyclicReference(path, new string[] { path });
        }

        private static bool CyclicReference(string path, string[] paths)
        {
            var deps = AssetDatabase.GetDependencies(path);
            if (deps.Length == 1)
            {
                return false;
            }

            foreach (var item in deps)
            {
                if (item == path)
                {
                    continue;
                }
                string extension = System.IO.Path.GetExtension(item);
                if (extension.Equals(".cs") == false)
                {
                    var cDeps = AssetDatabase.GetDependencies(item);
                    if (cDeps.Length == 1)
                    {
                        continue;
                    }
                    List<string> cDepsList = new List<string>(cDeps);

                    List<string> prePaths = new List<string>(paths);
                    if (IsIntersection(cDepsList, prePaths))
                    {

                        return true;
                    }
                    else
                    {

                        prePaths.Add(item);
                        if (CyclicReference(item, prePaths.ToArray()))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private static bool IsIntersection(List<string> A, List<string> B)
        {
            HashSet<string> sets = new HashSet<string>(B);

            foreach (var itemA in A)
            {
                if (sets.Contains(itemA))
                {
                    Debug.LogError(itemA);
                    return true;
                }
            }

            return false;
        }
    }
}
