///=====================================================
/// - FileName:      ExecuteFrame.cs
/// - NameSpace:     YukiFrameWork
/// - Created:       Yuki
/// - Email:         Yuki@qq.com
/// - Description:   事件检测回调
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================

using UnityEngine;
using System;
using System.Collections;
using YukiFrameWork.Pools;

namespace YukiFrameWork
{
    public class ExecuteFrame : ActionNode
    {
        private static SimpleObjectPools<ExecuteFrame> simpleObjectPools
            = new SimpleObjectPools<ExecuteFrame>(() => new ExecuteFrame(), x => x.actions.Clear(), 10);
        private Func<bool> predicate;
        private Action callBack;
        public ExecuteFrame(Func<bool> predicate, Action callBack)
        {
            OnReset(predicate, callBack);
        }

        public ExecuteFrame() { }

        public static ExecuteFrame Get(Func<bool> predicate, Action callBack)
        {
            var executeFrame = simpleObjectPools.Get();
            executeFrame.OnReset(predicate,callBack);
            return executeFrame;
        }

        public void OnReset(Func<bool> predicate, Action callBack)
        {
            this.predicate = predicate;
            this.callBack = callBack;
            AddNode(this);
        }

        public override bool OnExecute(float delta)
        {
            if (IsPaused) return false;
            if (predicate?.Invoke() == true)
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
            predicate = null;
            callBack = null;
            simpleObjectPools.Release(this);
        }

        public override IEnumerator ToCoroutine()
        {
            if (!IsInit) OnInit();
            yield return CoroutineTool.WaitUntil(predicate);
            callBack?.Invoke();
            OnFinish();
        }

        public override void OnInit()
        {
            IsCompleted = false;
            IsInit = true;
        }
    }
}