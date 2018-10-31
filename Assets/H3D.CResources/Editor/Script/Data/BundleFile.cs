using UnityEngine;
using System.Collections;
using System.IO;
namespace H3D.EditorCResources
{
    public class BundleFile
    {   
        public enum BFType
        {
            Bundle,
            Manifest,
            Location
        }
        public string m_BundleName;
        public string m_Path;
        public BFType m_BFType= BFType.Bundle;
    }

}