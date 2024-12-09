///=====================================================
/// - FileName:      Timer.cs
/// - NameSpace:     YukiFrameWork
/// - Created:       Yuki
/// - Email:         Yuki@qq.com
/// - Description:   计时器
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================

using UnityEngine;
using System;
using System.Collections;
using YukiFrameWork.Pools;

namespace YukiFrameWork
{
    public class Timer : ActionNode
    {
        private static SimpleObjectPools<Timer> simpleObjectPools
            = new SimpleObjectPools<Timer>(() => new Timer(), x => x.actions.Clear(), 10);

        public float MaxTime { get; private set; } = 0;
        private bool IsConstranit = false;
        private bool IsRealTime = false;//是否是真实时间

        private float currentTime;

        private float pauseTime;
        private float pauseStartTime;
        private float pauseCurrentTime;       

        private Action<float> callTemp;
        private Action callBack;

        public override bool IsPaused
        { 
            get => base.IsPaused; 
            set 
            {
                base.IsPaused = value;
                if (!value)
                    pauseTime += pauseCurrentTime;
                else pauseStartTime = (IsRealTime ? Time.realtimeSinceStartup : Time.time);
            }
        }

        public Timer(float maxTime, Action<float> callTemp, Action callBack, bool isConstranit,bool IsRealTime)
        {
            OnReset(maxTime, callTemp, callBack, isConstranit,IsRealTime);
        }

        public Timer()
        {
            
        }

        public static Timer Get(float maxTime, Action<float> callTemp, Action callBack, bool isConstranit,bool IsRealTime)
        {           
            var timer = simpleObjectPools.Get();
            timer.OnReset(maxTime,callTemp,callBack,isConstranit,IsRealTime);         
            return timer;
        }

        public void OnReset(float maxTime, Action<float> callTemp, Action callBack, bool isConstranit,bool IsRealTime)
        {                
            this.IsRealTime = IsRealTime;
            this.MaxTime = maxTime;
            this.callTemp = callTemp;
            this.callBack = callBack;
            this.IsConstranit = isConstranit;
            AddNode(this);
        }


        public override bool OnExecute(float delta)
        {
            if (IsPaused)
            {
                pauseCurrentTime = (IsRealTime ? Time.realtimeSinceStartup : Time.time) - pauseStartTime;               
                return false;
            }
            callTemp?.Invoke(IsConstranit ? (((IsRealTime ? Time.realtimeSinceStartup : Time.time ) - (currentTime + pauseTime)) / MaxTime) : (IsRealTime ? Time.realtimeSinceStartup : Time.time) - (currentTime + pauseTime));
            if ((IsRealTime ? Time.realtimeSinceStartup : Time.time) - (currentTime + pauseTime) > MaxTime)
            {
                callBack?.Invoke();
                return true;
            }
            return false;
        }

        public override void OnFinish()
        {                      
            IsCompleted = true;
            IsRealTime = false;
            IsInit = false;
            currentTime = 0;
            IsConstranit = false;
            pauseTime = 0;
            pauseStartTime = 0;
            pauseCurrentTime = 0;
            callTemp = null;
            callBack = null;
            simpleObjectPools.Release(this);
        }

        public override IEnumerator ToCoroutine()
        {
            if (!IsInit) OnInit();
            while ((IsRealTime ? Time.realtimeSinceStartup : Time.time) - currentTime < MaxTime)
            {
                callTemp?.Invoke(IsConstranit ? (((IsRealTime ? Time.realtimeSinceStartup : Time.time) - currentTime) / MaxTime) : (IsRealTime ? Time.realtimeSinceStartup : Time.time) - currentTime);
                yield return CoroutineTool.WaitForFrame();             
            }
            callBack?.Invoke();
            OnFinish();

        }

        public override void OnInit()
        {
            IsInit = true;
            currentTime = IsRealTime ? Time.realtimeSinceStartup : Time.time;
            IsCompleted = false;
        }
    }
}