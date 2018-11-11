
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
namespace H3D.CResources
{
#if UNITY_EDITOR
    internal class LocalAssetLocator : CResourceLocator
    {

        public LocalAssetLocator()
        {
            m_Locations = new Dictionary<int, IResourceLocation>();
            IEnumerable<string> paths = Directory.GetFiles(CResourceConst.m_PackPath, "*.*", SearchOption.AllDirectories).Where(
                   p => p.EndsWith(".meta") == false && p.EndsWith(".cs") == false
               );
            foreach (var wholeFilePath in paths)
            {
                string assetPath = CRUtlity.FullPathToAssetPath(wholeFilePath);
                System.Type type = UnityEditor.AssetDatabase.GetMainAssetTypeAtPath(assetPath);
                int location = Lcation(AssetPathToLoadPath(assetPath), type);

                m_Locations.Add(location, new LocalAssetLocation(assetPath, (typeof(LocalAssetProvider)).FullName));
            }
        }

        public override IResourceLocation Locate<T>(object requestID)
        {
            IResourceLocation assetLocation = null;
            int hashCode = Lcation(requestID.ToString(), typeof(T));
            if (m_Locations.ContainsKey(hashCode))
            {
                assetLocation = m_Locations[hashCode];
            }
            return assetLocation;
        }
    }
#endif
}
