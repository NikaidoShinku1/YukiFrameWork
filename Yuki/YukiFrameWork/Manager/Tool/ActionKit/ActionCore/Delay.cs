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
               }, null, 10);

        private float maxTime;
        private float currentTime = 0;
        private Action callBack;      
        public Delay(float maxTime, Action callBack)
        {          
            OnReset(maxTime, callBack);
        }

        public Delay()
        {
            
        }

        public static Delay Get(float maxTime,Action callBack)
        {         
            var delay = simpleObjectPools.Get();
            delay.OnReset(maxTime, callBack);
            return delay;
        }

        public void OnReset(float maxTime, Action callBack)
        {
            this.maxTime = maxTime;
            this.callBack = callBack;
            AddNode(this);
        }

        public override bool OnExecute(float delta)
        {
            if (IsPaused) return false;

            if (Time.time - currentTime > maxTime)
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
           
            
            callBack = null;
            currentTime = 0;
            simpleObjectPools.Release(this);
        }

        public override void OnInit()
        {
            IsInit = true;
            currentTime = Time.time;
            IsCompleted = false;
        }

        public override IEnumerator ToCoroutine()
        {
            if (!IsInit) OnInit();
            yield return new WaitForSeconds(maxTime);

            callBack?.Invoke();
            OnFinish();
        }
    }

}