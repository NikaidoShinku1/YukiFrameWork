using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YukiFrameWork
{
    public interface IThirdOrderBezierCurve : ISecondOrderBezierCurve
    {
        Vector3 GetControlPointAtThirdOrder();
    } 
}