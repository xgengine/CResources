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
        string m_id;
        string m_providerId;
        List<IResourceLocation> m_dependencies;
        public string InternalId { get { return m_id; } }
        public string ProviderId { get { return m_providerId; } }
        public IList<IResourceLocation> Dependencies { get { return m_dependencies; } }
        public bool HasDependencies { get { return m_dependencies != null && m_dependencies.Count > 0; } }

        public CResourceLocation(string id, string providerId, params IResourceLocation[] dependencies)
        {
            if (string.IsNullOrEmpty(id))
                throw new System.ArgumentNullException(id);
            if (string.IsNullOrEmpty(providerId))
                throw new System.ArgumentNullException(providerId);
            m_id = id;
            m_providerId = providerId;
            m_dependencies = new List<IResourceLocation>(dependencies);
        }
        public void AddDependencies(IResourceLocation[] dependencies)
        {
            m_dependencies = new List<IResourceLocation>(dependencies);
        }
    }

    public class CResourceLocator : IResourceLocator
    {

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
                    Dictionary<int, int[]> deps = new Dictionary<int, int[]>();
                    for(int i =0;i<count;i++)
                    {
                        int hashCode = br.ReadInt32();
                        string bundleName = br.ReadString();
                        int depCount = br.ReadInt32();
                        int[] depLocations = new int[depCount];
                        for (int k =0;k<depCount;i++)
                        {
                            depLocations[k] = br.ReadInt32();
                        }
                        deps.Add(hashCode,depLocations);
                        CResourceLocation cLocation = new CResourceLocation(bundleName, "",null);
                        m_Lcoations.Add(hashCode, cLocation);
                    }
                    foreach(var data in m_Lcoations)
                    {
                        CResourceLocation[] dependencies = new CResourceLocation[deps.Values.Count];
                        for(int i =0;i<dependencies.Length;i++)
                        {
                            dependencies[i] = m_Lcoations[deps[data.Key][i]];
                        }
                        data.Value.AddDependencies(dependencies);
                    }

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
