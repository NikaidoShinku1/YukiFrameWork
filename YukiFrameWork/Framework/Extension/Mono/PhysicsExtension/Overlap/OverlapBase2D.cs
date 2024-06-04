using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace YukiFrameWork.Overlap {

    [Serializable]
    public class OverlapEvent2D : UnityEvent<OverlapHit<Collider2D>> { }

    public abstract class OverlapBase2D : OverlapBase<Collider2D, OverlapEvent2D>
    { 
    }
}

