using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;
using System.Linq;
namespace H3D.EditorCResources
{
    [CustomEditor(typeof(CBuildPipeline))]
    public class CBuildPipelineEditor : Editor
    {

        private OperatonUINode m_AssetCollectorUI;
        private OperatonUINode m_AssetGaneratersUI;
        private OperatonUINode m_AssetModifiersUI;
        private OperatonUINode m_BundleNameBuilderUI;
        private OperatonUINode m_BundleBuidlerUI;
        private OperatonUINode m_BundleExporterUI;
        private OperatonUINode m_PlayerBuilderUI;
        private CBuildPipeline pipeline;
        private void OnEnable()
        {
            pipeline = target as CBuildPipeline;
            m_AssetCollectorUI = new OperatonUINode(pipeline, pipeline.m_AssetCollector, "AssetCollector", typeof(AssetCollectorAttribute), false);
            m_AssetGaneratersUI = new OperatonUINode(pipeline, pipeline.m_AssetGaneraters, "AssetGanerater", typeof(AssetGaneraterAttribute), true);
            m_AssetModifiersUI = new OperatonUINode(pipeline, pipeline.m_AssetModifiers, "AssetModifier", typeof(AssetModifierAttribute), true);
            m_BundleNameBuilderUI = new OperatonUINode(pipeline, pipeline.m_BundleNameBuilder, "BundleNameBuilder", typeof(BundleNameBuilderAttribute), false);
            m_BundleBuidlerUI = new OperatonUINode(pipeline, pipeline.m_BundleBuidler, "BundleBuidler", typeof(BundleBuidlerAttribute), false);
            m_BundleExporterUI = new OperatonUINode(pipeline, pipeline.m_BundleExporter, "BundleExporter", typeof(BundleExporterAttribute), false);
            m_BundleExporterUI = new OperatonUINode(pipeline, pipeline.m_BundleExporter, "BundleExporter", typeof(BundleExporterAttribute), false);
            m_PlayerBuilderUI = new OperatonUINode(pipeline, pipeline.m_PlayerBuilder, "PlayerBuilder", typeof(PlayerBuilderAttribute), false);
        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            m_AssetCollectorUI.DoLayoutList();
            m_AssetGaneratersUI.DoLayoutList();
            m_AssetModifiersUI.DoLayoutList();
            m_BundleNameBuilderUI.DoLayoutList();
            m_BundleBuidlerUI.DoLayoutList();
            m_BundleExporterUI.DoLayoutList();
            m_PlayerBuilderUI.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
            BuildButton();
        }

        private void BuildButton()
        {
            GUILayout.FlexibleSpace();
            Color c = GUI.color;
            GUI.color = Color.green;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(true));
            bool isClick = GUILayout.Button("Build", new GUIStyle("PreButton"), GUILayout.Width(60));
 
            EditorGUILayout.EndHorizontal();
            GUI.color = c;
            if(isClick)
            {
                if (pipeline != null)
                {
                    pipeline.Build();
                }
            }
           

        }


    }
    public class OperatonUINode
    {
        private ReorderableList m_OperationListUI;
        private List<Operation> m_Operations;
        private bool m_IsMultple;
        private Operation m_ActiveObject = null;
        public OperatonUINode(CBuildPipeline pipeline, List<Operation> operations, string headName, System.Type attributeType, bool isMultple)
        {
            m_Operations = operations;
            m_IsMultple = isMultple;

            m_OperationListUI = new ReorderableList(m_Operations, typeof(ScriptableObject), true, true, true, true);
            m_OperationListUI.elementHeight = 30;
            m_OperationListUI.drawHeaderCallback = (Rect rect) => {
                EditorGUI.LabelField(rect, headName, EditorStyles.boldLabel);
            };

            m_OperationListUI.drawElementCallback = (Rect rect, int index, bool selected, bool focused) =>
            {
                GUI.DrawTexture(new Rect(rect.x, rect.y + 4, 16, 16), EditorGUIUtility.IconContent("ScriptableObject Icon").image as Texture2D);

                EditorGUI.LabelField(new Rect(rect.x + 20, rect.y + 4, rect.width - 20, 16), m_Operations[index].name, EditorStyles.miniBoldLabel);
                if (selected)
                {
                    m_ActiveObject = m_Operations[index];
                }
            };

            m_OperationListUI.onMouseUpCallback = (ReorderableList list) =>
            {
                EditorGUIUtility.PingObject(m_ActiveObject);

            };

            m_OperationListUI.onAddDropdownCallback = (Rect rect, ReorderableList list) =>
            {
                var menu = new GenericMenu();
                List<System.Type> types = EditorCRUtlity.GetTypes(attributeType);

                for (int i = 0; i < types.Count; i++)
                {
                    string itemName = EditorCRUtlity.GetClassName(types[i]);
                    menu.AddItem(new GUIContent(itemName), false,
                    (object obj) =>
                    {
                        System.Type operatorType = obj as System.Type;
                        Operation noperator = ScriptableObject.CreateInstance(operatorType) as Operation;
                        noperator.name = EditorCRUtlity.GetClassName(operatorType);
                        pipeline.AddOperation(m_Operations, noperator , m_IsMultple);                   
                    },
                    types[i]);
                }
                menu.ShowAsContext();
            };

            m_OperationListUI.onRemoveCallback = (ReorderableList lis) =>
            {
                pipeline.DeleteOperation(m_Operations, m_ActiveObject );
            };
        }
        public void DoLayoutList()
        {
            m_OperationListUI.DoLayoutList();
        }
    }
}

