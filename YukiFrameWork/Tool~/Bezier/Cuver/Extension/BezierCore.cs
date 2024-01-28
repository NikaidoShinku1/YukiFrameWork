using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using YukiFrameWork.Pools;


namespace YukiFrameWork
{
    public enum BezierRuntimeMode
    {
        OnUpdate,
        OnFixedUpdate,
        OnLateUpdate
    }
    [Serializable]
    public class BezierCore
    {
        public int index { get; set; }
        public float currentLength { get; set; }
        public float totalLength { get; set; }

        public Transform user { get; private set; }     
        public float t { get; set; }
        public List<Vector3> paths { get;private set; }

        private static SimpleObjectPools<BezierCore> objectPools = new SimpleObjectPools<BezierCore>(() => new BezierCore(),core =>
        {
            core.OnCompleted?.Invoke(core.user);
            core.Reset();
        },50);

        public static BezierCore Get(BezierRuntimeMode Mode,Transform User, List<Vector3> paths)
        {
            var core = objectPools.Get();
            core.OnInit(Mode,User,paths);
            return core;
        }

        public static void Release(BezierCore core)
            => objectPools.Release(core);

        public BezierRuntimeMode Mode { get; private set; } = BezierRuntimeMode.OnUpdate;

        private void OnInit(BezierRuntimeMode Mode, Transform User, List<Vector3> paths)
        {
            this.user = User;
            this.Mode = Mode;
            this.paths = paths;
        }

        public void Reset()
        {
            index = 0;
            t = 0;
            paths?.Clear();
            currentLength = 0;
            OnCompleted = null;
            OnUpdate = null;
        }

        public event Action<Transform> OnCompleted = null;

        public Func<bool> OnUpdate = null;
              
    }
}