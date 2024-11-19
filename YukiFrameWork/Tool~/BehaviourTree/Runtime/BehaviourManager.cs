///=====================================================
/// - FileName:      BehaviourManager.cs
/// - NameSpace:     YukiFrameWork.Behaviours
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/11/13 14:01:26
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using YukiFrameWork.Extension;
using System.Collections.Generic;
using System.Linq;
namespace YukiFrameWork.Behaviours
{
    public enum RuntimeInitMode
    {
        Awake, Start,
    }   
	public class BehaviourManager : SingletonMono<BehaviourManager>
	{
        private FastList<BehaviourTree> behaviourTrees = new FastList<BehaviourTree>();

        public IReadOnlyList<BehaviourTree> BehaviourTrees => behaviourTrees;

        public void AddBehaviourTree(BehaviourTree behaviourTree)
        {
            behaviourTrees.Add(behaviourTree);
        }

        public void RemoveBehaviourTree(BehaviourTree behaviourTree)
        {
            behaviourTrees.Remove(behaviourTree);
        }

        private void Update()
        {
            for (int i = 0; i < behaviourTrees.Count; i++)
            {
                BehaviourTree behaviourTree = behaviourTrees[i];               
                if (CheckBehaviourRoot(behaviourTree))
                    behaviourTree.rootBehaviour.Update();
            }
        }

        private void FixedUpdate()
        {
            for (int i = 0; i < behaviourTrees.Count; i++)
            {
                BehaviourTree behaviourTree = behaviourTrees[i];
                if (CheckBehaviourRoot(behaviourTree))
                    behaviourTree.rootBehaviour.OnFixedUpdate();
            }
        }

        private void LateUpdate()
        {
            for (int i = 0; i < behaviourTrees.Count; i++)
            {
                BehaviourTree behaviourTree = behaviourTrees[i];
                if (CheckBehaviourRoot(behaviourTree))
                    behaviourTree.rootBehaviour.OnLateUpdate();
            }
        }

        private bool CheckBehaviourRoot(BehaviourTree behaviourTree)
        {
            return behaviourTree.rootBehaviour != null && behaviourTree.rootBehaviour.Status == BehaviourStatus.Running && !behaviourTree.IsPaused;
        }
    }
}
