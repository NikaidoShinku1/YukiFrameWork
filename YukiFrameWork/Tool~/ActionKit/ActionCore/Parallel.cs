///=====================================================
/// - FileName:      Parallel.cs
/// - NameSpace:     YukiFrameWork
/// - Created:       Yuki
/// - Email:         Yuki@qq.com
/// - Description:   并行时序
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================

using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using YukiFrameWork.Pools;

namespace YukiFrameWork
{
    public class Parallel : ActionNode, IParallel
    {
        private readonly List<IActionNode> parallelConditions = new List<IActionNode>();

        private static SimpleObjectPools<Parallel> simpleObjectPools
            = new SimpleObjectPools<Parallel>(() => new Parallel(),x => x.actions.Clear(),10);

        private readonly List<IActionNode> realeaseParallel = new List<IActionNode>();
        public Parallel()
        {
            
        }

        public static Parallel Get()
        {
            var parallel = simpleObjectPools.Get();
            parallel.OnReset();
            return parallel;
        }

        public void OnReset()
        {
            AddNode(this);
        }

        public IParallel AddParallel(IActionNode node)
        {
            parallelConditions.Add(node);
            return this;
        }

        public override bool OnExecute(float delta)
        {
            if (parallelConditions.Count > 0)
            {
                if (IsPaused) return false;
                for (int i = 0; i < parallelConditions.Count; i++)
                {
                    if (!parallelConditions[i].IsInit)
                        parallelConditions[i].OnInit();
                    if (parallelConditions[i].OnExecute(delta))              
                        realeaseParallel.Add(parallelConditions[i]);                 
                }
                if (realeaseParallel.Count > 0)
                {
                    for (int i = 0; i < realeaseParallel.Count; i++)
                    {
                        parallelConditions.Remove(realeaseParallel[i]);
                    }
                    realeaseParallel.Clear();
                }
                return false;
            }
            return true;
        }

        public override void OnFinish()
        {
            IsCompleted = true;
            IsInit = false;
           
            
            parallelConditions.Clear();
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
            while (parallelConditions.Count > 0)
            {
                for (int i = 0; i < parallelConditions.Count; i++)
                {
                    if (!parallelConditions[i].IsInit)
                        parallelConditions[i].OnInit();
                    if (parallelConditions[i].OnExecute(Time.deltaTime))
                        realeaseParallel.Add(parallelConditions[i]);
                }
                if (realeaseParallel.Count > 0)
                {
                    for (int i = 0; i < realeaseParallel.Count; i++)
                    {
                        parallelConditions.Remove(realeaseParallel[i]);
                    }
                    realeaseParallel.Clear();
                }
                yield return CoroutineTool.WaitForFrame();
            }
            OnFinish();
        }

    }
}