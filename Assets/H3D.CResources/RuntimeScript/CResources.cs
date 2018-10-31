using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
namespace H3D.CResources
{
    public class CResources
    {
        static IResourceLocator m_CResourceLocator;

        static List<IResourceProvider> m_ResouceProviders;

        static CResources()
        {
            m_CResourceLocator = new CResourceLocator();
   
        }

        public static TObject Load<TObject>(string requestID) where TObject : Object
        {
            IResourceLocation location = m_CResourceLocator.GetLocation<TObject>(requestID);

            IResourceProvider resProvider = null;

            foreach(IResourceProvider provider in m_ResouceProviders)
            {
                if(provider.CanProvide<TObject>(location))
                {
                    resProvider = provider;
                }
            }

            if(resProvider ==null)
            {
                throw new System.Exception("could not find support provider");
            }

            return resProvider.Provide<TObject>(location);
        }

        public static void Destroy(Object obj)
        {

        }
    }

    public interface IResourceLocator
    {
        IResourceLocation GetLocation<TObject>(string requestID)
        where TObject : UnityEngine.Object;
    }

    public interface IResourceLocation
    {

        string InternalId { get; }


        string ProviderId { get; }

        IList<IResourceLocation> Dependencies { get; }

        bool HasDependencies { get; }
    }

    public interface IResourceProvider
    {

        string ProviderId { get; }

        TObject Provide<TObject>(IResourceLocation location)
        where TObject : UnityEngine.Object;


        bool CanProvide<TObject>(IResourceLocation location)
        where TObject : class;


        bool Release(IResourceLocation location, object asset);
    }

    public class CResourceLocation:IResourceLocation
    {
        string m_name;
        string m_id;
        string m_providerId;
        List<IResourceLocation> m_dependencies;
        public string InternalId { get { return m_id; } }
        public string ProviderId { get { return m_providerId; } }
        public IList<IResourceLocation> Dependencies { get { return m_dependencies; } }
        public bool HasDependencies { get { return m_dependencies != null && m_dependencies.Count > 0; } }
        public override string ToString()
        {
            return m_name;
        }
        public CResourceLocation(string name, string id, string providerId, params IResourceLocation[] dependencies)
        {
            if (string.IsNullOrEmpty(id))
                throw new System.ArgumentNullException(id);
            if (string.IsNullOrEmpty(providerId))
                throw new System.ArgumentNullException(providerId);
            m_name = name;
            m_id = id;
            m_providerId = providerId;
            m_dependencies = new List<IResourceLocation>(dependencies);
        }
    }

    public class CResourceLocator : IResourceLocator
    {
#if UNITY_EDITOR
        public static void WriteLocationData(string dataPath)
        {
            Dictionary<int, string> maps = new Dictionary<int, string>();

            IEnumerable<string> paths = Directory.GetFiles(ConstValue.m_PackPath, "*.*", SearchOption.AllDirectories).Where(
                    p => p.EndsWith(".meta") == false && p.EndsWith(".cs") == false
                );

            foreach (var wholeFilePath in paths)
            {
                string assetPath = CRUtlity.FullPathToAssetPath(wholeFilePath);
                string guid = UnityEditor.AssetDatabase.AssetPathToGUID(assetPath);
                System.Type type = UnityEditor.AssetDatabase.GetMainAssetTypeAtPath(assetPath);
                maps.Add(Lcation(AssetPathToLoadPath(assetPath), type), guid);
            }

            using (Stream stream = new FileStream(dataPath, FileMode.Create))
            {
                using (var bw = new BinaryWriter(stream))
                {
                    bw.Write(maps.Count);
                    foreach (var item in maps)
                    {
                        bw.Write(item.Key);
                        bw.Write(item.Value);
                    }
                }
            }
        }
#endif
        public CResourceLocator()
        {
            ReadLoactionData(Path.Combine( Path.GetDirectoryName( Application.dataPath),ConstValue.m_BundlePath+"/"+ConstValue.m_LocationName));
        }

        public void ReadLoactionData(string dataPath)
        {
            using (Stream stream = new FileStream(dataPath, FileMode.Open))
            {
                using (var br = new BinaryReader(stream))
                {
                    int count = br.ReadInt32();
                    m_Lcoations = new Dictionary<int, CResourceLocation>(count);
                  
                }
            }
        }

        protected static Dictionary<int, CResourceLocation> m_Lcoations;

        IResourceLocation IResourceLocator.GetLocation<TObject>(string requestID)
        {
            IResourceLocation assetLocation = null;

            int hashCode = Lcation(requestID, typeof(TObject));

            if(m_Lcoations.ContainsKey(hashCode))
            {
                assetLocation = m_Lcoations[hashCode];
            }

            return assetLocation;
        }

        public static string AssetPathToLoadPath(string assetPath)
        {
            return assetPath.ToLower().Replace(ConstValue.m_PackPath, "");
        }

        public static int Lcation(string loadPath, System.Type type)
        {
            return string.Concat(loadPath.ToLower(), type.Name).GetHashCode();

        }
    }

  
}
