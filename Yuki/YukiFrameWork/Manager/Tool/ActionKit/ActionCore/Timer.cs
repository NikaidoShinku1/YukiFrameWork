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
            = new SimpleObjectPools<Timer>(() => new Timer(), null, 10);

        public float MaxTime { get; private set; } = 0;
        public bool IsConstranit { get; private set; } = false;

        private float currentTime;
        private Action<float> callTemp;
        private Action callBack;

        public Timer(float maxTime, Action<float> callTemp, Action callBack, bool isConstranit)
        {
            OnReset(maxTime, callTemp, callBack, isConstranit);
        }

        public Timer()
        {
            
        }

        public static Timer Get(float maxTime, Action<float> callTemp, Action callBack, bool isConstranit)
        {           
            var timer = simpleObjectPools.Get();
            timer.OnReset(maxTime,callTemp,callBack,isConstranit);         
            return timer;
        }

        public void OnReset(float maxTime, Action<float> callTemp, Action callBack, bool isConstranit)
        {
            currentTime = Time.time;
            this.MaxTime = maxTime;
            this.callTemp = callTemp;
            this.callBack = callBack;
            this.IsConstranit = isConstranit;
            AddNode(this);
        }


        public override bool OnExecute(float delta)
        {
            if (IsPaused) return false;
            callTemp?.Invoke(IsConstranit ? Time.time - currentTime / MaxTime : Time.time - currentTime);
            if (Time.time - currentTime > MaxTime)
            {
                callBack?.Invoke();
                return true;
            }
            return false;
        }

        public override void OnFinish()
        {
           
            
            IsCompleted = true;
            IsInit = false;
            currentTime = 0;
            IsConstranit = false;
            callTemp = null;
            callBack = null;
            simpleObjectPools.Release(this);
        }

        public override IEnumerator ToCoroutine()
        {
            if (!IsInit) OnInit();
            while (currentTime < MaxTime)
            {
                callTemp?.Invoke(IsConstranit ? Time.time - currentTime : Time.time - currentTime / MaxTime);
                yield return null;
                yield return new WaitUntil(() => !IsPaused);
            }
            callBack?.Invoke();
            OnFinish();

        }

        public override void OnInit()
        {
            IsInit = true;
            currentTime = Time.time;
            IsCompleted = false;
        }
    }
}