using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YukiFrameWork
{
    public interface ISecondOrderBezierCurve : IFirstOrderBezierCurve
    {
        Vector3 GetControlPointAtSecondOrder();
    }
}