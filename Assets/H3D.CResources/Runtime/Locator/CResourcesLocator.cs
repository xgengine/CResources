using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace H3D.CResources
{
    public abstract class CResourceLocator : IResourceLocator
    {
        protected Dictionary<int, IResourceLocation> m_Locations;

        public abstract IResourceLocation Locate<T>(object requestID) where T : class;

        public static string AssetPathToLoadPath(string assetPath)
        {
            string noExtPath = CRUtlity.DeleteExtension(assetPath);
            return noExtPath.ToLower().Replace(CResourceConst.m_PackPath + "/", "");
        }

        public static int Lcation(string loadPath, System.Type type)
        {
            string internalLoadPath = string.Concat(loadPath.ToLower(), type.Name);
            return internalLoadPath.GetHashCode();

        }
    }
}
