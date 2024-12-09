///=====================================================
/// - FileName:      Sequence.cs
/// - NameSpace:     YukiFrameWork
/// - Created:       Yuki
/// - Email:         Yuki@qq.com
/// - Description:   队列时序
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================

using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using YukiFrameWork.Pools;

namespace YukiFrameWork
{
    public class Sequence : ActionNode, ISequence
    {
        private static SimpleObjectPools<Sequence> simpleObjectPools
            = new SimpleObjectPools<Sequence>(() => new Sequence(), x => x.actions.Clear(), 10);
        private readonly Queue<IActionNode> sequenes = new Queue<IActionNode>();

        public Sequence()
        {
            
        }

        public static Sequence Get()
        {
            var sequence = simpleObjectPools.Get();
            sequence.OnReset();
            return sequence;
        }

        public void OnReset()
        {
            AddNode(this);
        }

        public ISequence AddSequence(IActionNode action)
        {
            sequenes.Enqueue(action);
            return this;
        }

        public override bool OnExecute(float delta)
        {
            if (IsPaused) return false;
            if (sequenes.Count > 0)
            {
                if (!sequenes.Peek().IsInit)
                    sequenes.Peek().OnInit();
                if (sequenes.Peek().OnExecute(delta))
                    sequenes.Dequeue().OnFinish();
                return false;
            }
            return true;
        }

        public override void OnFinish()
        {
            IsInit = false;
            IsCompleted = true;
           
            
            sequenes.Clear();
            simpleObjectPools.Release(this);
        }

        public override void OnInit()
        {
            IsInit = true;
            IsCompleted = false;
        }

        public override IEnumerator ToCoroutine()
        {
            if (!IsInit) OnInit();
            while (sequenes.Count > 0)
            {
                if (!sequenes.Peek().IsInit)
                    sequenes.Peek().OnInit();
                if (sequenes.Peek().OnExecute(Time.deltaTime))
                    sequenes.Dequeue().OnFinish();
                yield return CoroutineTool.WaitForFrame();
            }
            OnFinish();
        }
    }
}