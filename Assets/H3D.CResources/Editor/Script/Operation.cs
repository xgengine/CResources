using UnityEngine;
using System.Collections;
using UnityEditor;
namespace H3D.EditorCResources
{
    public class Operation : ScriptableObject
    {
        [HideInInspector]
        public int m_InputCount;

        [HideInInspector]
        public int m_OutputCount;

        [HideInInspector]
        public int m_NeedOperateCount;

        [HideInInspector]
        public int m_RealOperateCount;

        [HideInInspector]
        public float m_Progresss;

        private float m_RunUseTime = 0;

        public void Statistics(int inputCount,int outputCount,int needOperateCount,int realOperateCount)
        {
            m_InputCount = inputCount;
            m_OutputCount = outputCount;
            m_NeedOperateCount = needOperateCount;
            m_RealOperateCount = realOperateCount;
            LogUtility.Log("Input Count :{0} * Output Count :{1}  * Need Operate Count :{2} * Real Operate Count :{3}", m_InputCount,m_OutputCount,m_NeedOperateCount,m_RealOperateCount);
        }
        public void Statistics(int inputCount, int outputCount)
        {
            m_InputCount = inputCount;
            m_OutputCount = outputCount;
            LogUtility.Log("Input Count :{0} * Output Count :{1}  ", m_InputCount, m_OutputCount);
        }
 
        public void RecordTime()
        {
            m_RunUseTime = Time.realtimeSinceStartup;
        }
        public void StatisticsUseTime()
        {
            LogUtility.Log(" Use Time {0}" ,Time.realtimeSinceStartup-m_RunUseTime);
        }

    }
}


