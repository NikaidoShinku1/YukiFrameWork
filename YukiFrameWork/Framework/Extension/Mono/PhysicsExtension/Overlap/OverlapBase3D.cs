using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace YukiFrameWork.Overlap 
{

    [Serializable]
    public class OverlapEvent3D : UnityEvent<OverlapHit<Collider>> { }

    public abstract class OverlapBase3D : OverlapBase<Collider, OverlapEvent3D>
    {
    }
}
