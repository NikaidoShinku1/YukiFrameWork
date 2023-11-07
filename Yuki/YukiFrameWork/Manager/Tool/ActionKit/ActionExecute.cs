///=====================================================
/// - FileName:      ActionExecute.cs
/// - NameSpace:     YukiFrameWork
/// - Created:       Yuki
/// - Email:         Yuki@qq.com
/// - Description:   ActionKit执行
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================

using UnityEngine;
using System;
using System.Collections.Generic;

namespace YukiFrameWork
{   
    public class OnGameObjectTrigger : MonoBehaviour
    {
        private readonly List<IActionNode> actionNodes = new List<IActionNode>();

        private readonly Stack<Action> onFinishEvents = new Stack<Action>();

        public void AddAction(IActionNode action)
        {
            actionNodes.Add(action);
        }

        public void RemoveAction(IActionNode action) 
        {
            actionNodes.Remove(action);
        }

        public void PushFinishEvent(Action onFinish)
        {
            onFinishEvents.Push(onFinish);
        }

        public Action PopFinishEvent()
        {
            return onFinishEvents.Pop();
        }

        private void Update()
        {
            for (int i = 0; i < actionNodes.Count; i++)
            {
                if (!actionNodes[i].IsInit) actionNodes[i].OnInit();

                if (actionNodes[i].OnExecute(Time.deltaTime) || actionNodes[i].IsCompleted)
                {
                    actionNodes[i].OnFinish();
                    actionNodes.RemoveAt(i);
                }
            }
        }

        private void OnDestroy()
        {
            foreach (var action in actionNodes)
            {
                action.OnFinish();
            }

            while (onFinishEvents.Count > 0)
            {
                PopFinishEvent()?.Invoke();
            }

            actionNodes.Clear();
        }
    }

    public class MonoUpdateExecute : MonoBehaviour
    {
        private readonly List<IActionUpdateNode> actionUpdateNodes = new List<IActionUpdateNode>();
        private readonly Stack<Action> onFinishEvents = new Stack<Action>();
        public void AddUpdate(IActionUpdateNode node)
        {
            actionUpdateNodes.Add(node);
        }

        public void RemoveUpdate(IActionUpdateNode node)
        {
            actionUpdateNodes.Remove(node);
        }

        public void PushFinishEvent(Action onFinish)
        {
            onFinishEvents.Push(onFinish);
        }

        public Action PopFinishEvent()
        {
            return onFinishEvents.Pop();
        }

        private void Update()
        {
            CheckOrUpdate(UpdateStatus.OnUpdate);
        }

        private void FixedUpdate()
        {
            CheckOrUpdate(UpdateStatus.OnFixedUpdate);
        }

        private void LateUpdate()
        {
            CheckOrUpdate(UpdateStatus.OnLateUpdate);
        }

        private void CheckOrUpdate(UpdateStatus updateStatus)
        {
            for (int i = 0; i < actionUpdateNodes.Count; i++)
            {
                if (actionUpdateNodes[i].UpdateStatus == updateStatus)
                {
                    if (actionUpdateNodes[i].OnExecute(Time.deltaTime))
                    {
                        actionUpdateNodes[i].OnFinish();
                        actionUpdateNodes.RemoveAt(i);
                    }
                }
            }
        }

        private void OnDestroy()
        {            
            foreach (var node in actionUpdateNodes)
            {
                node.OnFinish();
            }

            while (onFinishEvents.Count > 0)
            {
                PopFinishEvent()?.Invoke();
            }

            actionUpdateNodes.Clear();          
        }

    }
}