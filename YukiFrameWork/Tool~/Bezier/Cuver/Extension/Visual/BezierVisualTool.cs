using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YukiFrameWork
{
    public enum PointType
    {
        Vector = 0,
        Transform
    }

    public enum BezierStage
    {
        一阶 = 0,
        二阶,
        三阶,
    }

    [Serializable]
    public partial class BezierVisualTool : MonoBehaviour
    {       
#if UNITY_EDITOR      
        [Header("在场景中检视"),SerializeField]
        private bool isScene;      

        public bool IsScene => isScene;        
#endif

        [HideInInspector]
        public List<Vector3> paths = new List<Vector3>();

        public Vector3 StartValue
            => pointType == PointType.Vector ? start : (IsLocal ? startPos.localPosition : startPos.position);

        public Vector3 EndValue
            => pointType == PointType.Vector ? end : (IsLocal ? endPos.localPosition : endPos.position);

        public Vector3 SecondOrderControl
            => pointType == PointType.Vector ? control1 : (IsLocal ? control1Pos.localPosition :control1Pos.position);

        public Vector3 ThirdOrderControl
            => pointType == PointType.Vector ? control2 : (IsLocal ? control2Pos.localPosition:control2Pos.position);

        public BezierRuntimeMode Mode { get; set; } = BezierRuntimeMode.OnFixedUpdate;

        public void SetFirstOrderBezier(IFirstOrderBezierCurve curve)
        {
            stage = BezierStage.一阶;
            Set(curve);
            paths = BezierUtility.GetBezierList(start, end, curve.GetPathLangth());
        }

        public void SetSecondOrderBezier(ISecondOrderBezierCurve curve)
        {
            stage = BezierStage.二阶;
            Set(curve);
            paths = BezierUtility.GetBezierList(start, end,curve.GetControlPointAtSecondOrder(), curve.GetPathLangth());
        }

        public void SetThirdOrderBezier(IThirdOrderBezierCurve curve)
        {
            stage = BezierStage.三阶;
            Set(curve);
            paths = BezierUtility.GetBezierList(start, end, curve.GetControlPointAtSecondOrder(),curve.GetControlPointAtThirdOrder(), curve.GetPathLangth());
        }

        private void Set(IFirstOrderBezierCurve curve)
        {
            pointType = PointType.Vector;          
            start = curve.GetStartPoint();
            end = curve.GetEndPoint();
            count = curve.GetPathLangth();
        }               
    }
}
