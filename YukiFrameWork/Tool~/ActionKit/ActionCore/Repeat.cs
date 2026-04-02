///=====================================================
/// - FileName:      Repeat.cs
/// - NameSpace:     YukiFrameWork
/// - Created:       Yuki
/// - Email:         Yuki@qq.com
/// - Description:   循环检测
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================

using UnityEngine;
using System;
using System.Collections;
using YukiFrameWork.Pools;

namespace YukiFrameWork
{
    public class Repeat : ActionNode, IRepeat
    {      
        private static SimpleObjectPools<Repeat> simpleObjectPools
            = new SimpleObjectPools<Repeat>(() => new Repeat(), x => x.actions.Clear(), 10);
        public Repeat(int count)
        {
            OnReset(count);
        }

        public Repeat() { }

        public static Repeat Get(int count)
        {
            var repeat = simpleObjectPools.Get();
            repeat.OnReset(count);
            return repeat;
        }

        public IActionNode ActionNode { get; set; }

        public void OnReset(int count)
        {
            this.CurrentCount = count;
            AddNode(this);
        }

        public int CurrentCount { get; private set; }

        public override bool OnExecute(float delta)
        {
            if (!IsInit) OnInit();
            if (ActionNode == null)
                return true;
            if (CurrentCount == -1)
            {
                if (ActionNode.OnExecute(delta))
                {
                    ActionNode.OnInit();
                }
            }
            else
            {
                if (CurrentCount == 0)
                    return true;

                if (ActionNode.OnExecute(delta))
                {
                    CurrentCount--;
                    if (CurrentCount > 0)
                    {
                        ActionNode.OnInit();
                    }
                }
            }

            return false;
        }

        public override void OnFinish()
        {
            CurrentCount = 0;
            IsInit = false;
            IsCompleted = true;
            ActionNode = null;
            simpleObjectPools.Release(this);
        }

        public override void OnInit()
        {
            if (ActionNode == null)
                throw new Exception("ActionNode丢失，请检查调用ActionKit.Repeat是否有继续链式调用Action API！例如ActionKit.Repeat().Delay() //ToDo");
            ActionNode.OnInit();
            IsInit = true;
            IsCompleted = false;
        }

        public override IEnumerator ToCoroutine()
        {
            if (!IsInit) OnInit();
            yield return CoroutineTool.WaitUntil(() => OnExecute(Time.deltaTime));
            OnFinish();
        }
    }
}