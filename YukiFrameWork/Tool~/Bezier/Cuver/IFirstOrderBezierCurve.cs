using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YukiFrameWork
{
    public interface IFirstOrderBezierCurve : IBezierCore
    {
        Vector3 GetStartPoint();
        Vector3 GetEndPoint();
    }
}
