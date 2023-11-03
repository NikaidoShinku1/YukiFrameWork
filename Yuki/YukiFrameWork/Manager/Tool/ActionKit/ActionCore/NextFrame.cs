///=====================================================
/// - FileName:      NextFrame.cs
/// - NameSpace:     YukiFrameWork
/// - Created:       Yuki
/// - Email:         Yuki@qq.com
/// - Description:   定时帧回调
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================

using UnityEngine;
using System;
using System.Collections;
using YukiFrameWork.Pools;

namespace YukiFrameWork
{
    public class NextFrame : ActionNode
    {
        private static SimpleObjectPools<NextFrame> simpleObjectPools
            = new SimpleObjectPools<NextFrame>(() => new NextFrame(), null, 10);
        private int frameCount;
        private Action callBack;
        public NextFrame(int frameCount, Action callBack)
        {
            OnReset(frameCount, callBack);
        }

        public NextFrame() { }

        public static NextFrame Get(int frameCount, Action callBack)
        {
            var nextFrame = simpleObjectPools.Get();
            nextFrame.OnReset(frameCount,callBack);
            return nextFrame;
        }

        public void OnReset(int frameCount, Action callBack)
        {
            this.frameCount = frameCount;
            this.callBack = callBack;
            AddNode(this);
        }

        public override bool OnExecute(float delta)
        {
            if (IsPaused) return false;
            if (frameCount == 0)
            {
                callBack?.Invoke();
                OnFinish();
                return true;
            }
            frameCount--;
            return false;
        }

        public override void OnFinish()
        {
           
            
            IsInit = false;
            IsFinish = true;
            frameCount = 0;
            callBack = null;
            simpleObjectPools.Release(this);
        }

        public override void OnInit()
        {
            IsInit = true;
            IsFinish = false;
        }

        public override IEnumerator ToCoroutine()
        {
            if (!IsInit) OnInit();
            while (frameCount > 0)
            {
                frameCount--;
                yield return null;
            }
            callBack?.Invoke();
            OnFinish();
        }
    }

}