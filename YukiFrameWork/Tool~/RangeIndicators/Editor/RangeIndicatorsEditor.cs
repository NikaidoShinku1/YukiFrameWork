using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
#if UNITY_EDITOR

namespace YukiFrameWork.Editorial
{
    [CustomEditor(typeof(RangeIndicators))]
    [System.Obsolete]
    public class RangeIndicatorsEditor : Editor
    {
        private RangeIndicators m_RangeIndicators;
        private void OnEnable()
        {
            m_RangeIndicators = (RangeIndicators)target;
        }
        public void OnSceneGUI()
        {
            Handles.color = new Color(0.7f, 1, 0.9f, 0.6f);
            Handles.DrawSolidArc(m_RangeIndicators.transform.position + m_RangeIndicators.offect, m_RangeIndicators.transform.up, m_RangeIndicators.transform.forward, m_RangeIndicators.angle, m_RangeIndicators.radius);
            Handles.DrawSolidArc(m_RangeIndicators.transform.position + m_RangeIndicators.offect, m_RangeIndicators.transform.up, m_RangeIndicators.transform.forward, -m_RangeIndicators.angle, m_RangeIndicators.radius);
        }


    }
}
#endif