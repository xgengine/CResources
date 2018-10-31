using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
namespace H3D.EditorCResources
{
    [CreateAssetMenu(menuName = "CResources/BundleBuildPipeline ")]
    public class BundleBuildPipeline : ScriptableObject
    {
        public List<Operation> m_AssetCollector = new List<Operation>();
        public List<Operation> m_AssetGaneraters = new List<Operation>();
        public List<Operation> m_AssetModifiers = new List<Operation>();
        public List<Operation> m_BundleNameBuilder = new List<Operation>();
        public List<Operation> m_BundleBuidler = new List<Operation>();
        public List<Operation> m_BundleExporter = new List<Operation>();

        protected IAssetCollector m_IAssetCollector;
        protected List<IAssetGanerater> m_IAssetGaneraters;
        protected List<IAssetModifier> m_IAssetModifiers;
        protected IBundleNameBuilder m_IBundleNameBuilder;
        protected IBundleBuidler m_IBundleBuidler;
        protected IBundleExporter m_IBundleExporter;

        private void InitPipline()
        {
            if(m_AssetCollector.Count>0)
            {
                m_IAssetCollector = m_AssetCollector[0] as IAssetCollector;
            }

            m_IAssetGaneraters = new List<IAssetGanerater>();
            foreach (var item in m_AssetGaneraters)
            {
                m_IAssetGaneraters.Add(item as IAssetGanerater);
            }

            m_IAssetModifiers = new List<IAssetModifier>();
            foreach (var item in m_AssetModifiers)
            {
                m_IAssetModifiers.Add(item as IAssetModifier);
            }

            if (m_BundleNameBuilder.Count>0)
            {
                m_IBundleNameBuilder = m_BundleNameBuilder[0] as IBundleNameBuilder;
            }

            if(m_BundleBuidler.Count>0)
            {
                m_IBundleBuidler = m_BundleBuidler[0] as IBundleBuidler;
            }
            if(m_BundleExporter.Count>0)
            {

                m_IBundleExporter = m_BundleExporter[0] as IBundleExporter;
            }

        }
       
        public void DeleteOperation(List<Operation> list,Operation so)
        {
            if (so != null)
            {
                list.Remove(so);
                DestroyImmediate(so,true);
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(this));
            }
        }
        public void AddOperation(List<Operation> list, Operation so,bool isMult = false)
        {
            if (so != null)
            {
                if(isMult == false)
                {
                    for(int i =list.Count-1;i>=0;i--)
                    {
                        DeleteOperation(list, list[i]);
                    }            
                }
                list.Add(so);
                AssetDatabase.AddObjectToAsset(so, this);
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(this));
            }
        }


        public void Build()
        {
            try
            {

                InitPipline();

                List<AssetFile> assetsNeedBuild = null;
                if (m_IAssetCollector != null)
                {
                    m_IAssetCollector.Hanlde(out assetsNeedBuild);
                }
                else
                {
                    return;
                }


                List<AssetFile> ganeraterAssets = null;
                m_IAssetGaneraters.ForEach(p =>
                {
                    p.Hanlde(assetsNeedBuild, out ganeraterAssets);
                    assetsNeedBuild = ganeraterAssets;
                });


                List<AssetFile> modifyAssets = null;

                m_IAssetModifiers.ForEach(p =>
                {
                    p.Hanlde(assetsNeedBuild, out modifyAssets);
                    assetsNeedBuild = modifyAssets;
                });

                List<AssetFileGroup> groups = null;
                if (m_IBundleBuidler != null)
                {
                    m_IBundleNameBuilder.Hanlde(assetsNeedBuild, out groups);
                }
                else
                {
                    return;
                }

                List<BundleFile> bundleFiles = null;
                if (m_IBundleBuidler != null)
                {
                    m_IBundleBuidler.Hanlde(groups, out bundleFiles);
                }

                if (m_IBundleExporter != null)
                {
                    m_IBundleExporter.Hanlde(bundleFiles);
                }
                else
                {
                    return;
                }
            }
            catch (CResourcesException e)
            {
                LogUtlity.LogError("{0} {1}",e.Message, e.StackTrace);
            }
        }

        private void OnDestroy()
        {
            Debug.LogError("onDestroy");
            EditorUtility.SetDirty(this);
        }
    }
}
