﻿using System;
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
    [DisableViewWarning]
    public partial class BezierVisualTool : MonoBehaviour
    {       

        public Vector3 StartValue
            => pointType == PointType.Vector ? start : startPos.position;

        public Vector3 EndValue
            => pointType == PointType.Vector ? end : endPos.position;

        public Vector3 SecondOrderControl
            => pointType == PointType.Vector ? control1 : control1Pos.position;

        public Vector3 ThirdOrderControl
            => pointType == PointType.Vector ? control2 :control2Pos.position;

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
