///=====================================================
/// - FileName:      Delay.cs
/// - NameSpace:     YukiFrameWork
/// - Created:       Yuki
/// - Email:         Yuki@qq.com
/// - Description:   Delay定时回调
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================

using UnityEngine;
using System;
using System.Collections;
using YukiFrameWork.Pools;

namespace YukiFrameWork
{
    public class Delay : ActionNode
    {
        public static SimpleObjectPools<Delay> simpleObjectPools
            = new SimpleObjectPools<Delay>
               (() =>
               {
                   return new Delay();
               }, x => x.actions.Clear(), 10);

        private float maxTime;
        private float currentTime = 0;

        private float pauseTime;
        private float pauseStartTime;
        private float pauseCurrentTime;

        private Action callBack;
        private bool isRealTime = false;       
        public override bool IsPaused
        {
            get => base.IsPaused;
            set
            {
                base.IsPaused = value;
                if (!value)
                    pauseTime += pauseCurrentTime;
                else pauseStartTime = (isRealTime ? Time.realtimeSinceStartup : Time.time);
            }
        }

        public Delay(float maxTime, Action callBack,bool isRealTime = false)
        {          
            OnReset(maxTime, callBack,isRealTime);
        }

        public Delay()
        {
            
        }

        public static Delay Get(float maxTime,Action callBack, bool isRealTime = false)
        {         
            var delay = simpleObjectPools.Get();
            delay.OnReset(maxTime, callBack,isRealTime);
            return delay;
        }

        public void OnReset(float maxTime, Action callBack, bool isRealTime = false)
        {
            this.maxTime = maxTime;
            this.callBack = callBack;
            this.isRealTime = isRealTime;
            AddNode(this);
        }

        public override bool OnExecute(float delta)
        {
            if (IsPaused)
            {
                pauseCurrentTime = (isRealTime ? Time.realtimeSinceStartup : Time.time) - pauseStartTime;
                return false;
            }

            if ((isRealTime ? Time.realtimeSinceStartup : Time.time) - (currentTime + pauseTime) > maxTime)
            {
                callBack?.Invoke();              
                return true;
            }
            return false;
        }

        public override void OnFinish()
        {
            IsInit = false;
            IsCompleted = true;

            pauseCurrentTime = 0;
            pauseStartTime = 0;
            pauseTime = 0;
            
            callBack = null;
            currentTime = 0;          
            simpleObjectPools.Release(this);
        }

        public override void OnInit()
        {
            IsInit = true;
            currentTime = isRealTime ? Time.realtimeSinceStartup : Time.time;
            IsCompleted = false;
        }
        [DisableEnumeratorWarning]
        public override IEnumerator ToCoroutine()
        {
            if (!IsInit) OnInit();
            if (isRealTime)
                yield return CoroutineTool.WaitForSecondsRealtime(maxTime);
            else yield return CoroutineTool.WaitForSeconds(maxTime);

            callBack?.Invoke();            
            OnFinish();
        }       

    }

}