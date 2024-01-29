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
    public interface IBezier
    {
        public event Action<float> OnCompleted;
        public event Action OnUpdate;
    }
    [Serializable]
    public class Bezier : IBezier
    {
        public int index { get; set; }
        public float currentLength { get; set; }
        public float totalLength { get; set; }
        public float InitalTime { get; private set; }        
        public float t { get; set; }
        public List<Vector3> paths { get;private set; }

        private static SimpleObjectPools<Bezier> objectPools = new SimpleObjectPools<Bezier>(() => new Bezier(),core =>
        {           
            core.Reset();
        },50);

        public static Bezier Get(BezierRuntimeMode Mode ,List<Vector3> paths)
        {
            var core = objectPools.Get();
            core.OnInit(Mode,paths);
            return core;
        }

        public static void Release(Bezier core)
        {            
            core.OnCompleted?.Invoke(Time.time - core.InitalTime);
            objectPools.Release(core);
        }

        public BezierRuntimeMode Mode { get; private set; } = BezierRuntimeMode.OnUpdate;

        private void OnInit(BezierRuntimeMode Mode, List<Vector3> paths)
        {
            InitalTime = Time.time;
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
            Condition = null;
            OnUpdate = null;
        }

        public event Action<float> OnCompleted = null;
        public event Action OnUpdate = null;

        public void UpdateInvoke()
            => OnUpdate?.Invoke();

        public Func<bool> Condition = null;     
              
    }
}