using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;
namespace H3D.CResources
{
    internal class BundleAssetLocator : CResourceLocator
    {

        public BundleAssetLocator()
        {
            string locationDataPath;


            locationDataPath = Path.Combine(Path.GetDirectoryName(Application.dataPath), CResourceConst.m_BundlePath + "/" + CResourceConst.m_LocationName);
            if(!File.Exists(locationDataPath))
            {
                locationDataPath = Path.Combine(Application.streamingAssetsPath, "cresources/" + CResourceConst.m_LocationName);
            }
            ReadLoactionData(locationDataPath);
        }

        public void ReadLoactionData(string dataPath)
        {
            using (Stream stream = new FileStream(dataPath, FileMode.Open))
            {
                using (var br = new BinaryReader(stream))
                {
                    int count = br.ReadInt32();
                    m_Locations = new Dictionary<int, IResourceLocation>(count);
                    List<BundleLocation> tempLocations = new List<BundleLocation>(count);
                    for (int i = 0; i < count; i++)
                    {
                        int hashCode = br.ReadInt32();
                        string bundleName = br.ReadString();
                        BundleLocation cLocation = new BundleLocation(bundleName, typeof(LocalBundleProvider).FullName);
                        m_Locations.Add(hashCode, cLocation);
                        tempLocations.Add(cLocation);
                    }

                    for (int i = 0; i < count; i++)
                    {
                        int depCount = br.ReadInt32();
                        BundleLocation[] dependencies = new BundleLocation[depCount];
                        for (int k = 0; k < depCount; k++)
                        {
                            dependencies[k] = m_Locations[br.ReadInt32()] as BundleLocation;
                        }
                        tempLocations[i].AddDependencies(dependencies);
                    }
                    tempLocations.Clear();
                }
            }
        }

        public override IResourceLocation Locate<T>(object requestID)
        {
            IResourceLocation assetLocation = null;

            int hashCode = Lcation(requestID as string, typeof(T));

            if (m_Locations.ContainsKey(hashCode))
            {
                assetLocation = new BundleAssetLocation(requestID as string, typeof(BundleAssetProvider).FullName, new IResourceLocation[] { m_Locations[hashCode] });
            }

            return assetLocation;
        }

    }
}
