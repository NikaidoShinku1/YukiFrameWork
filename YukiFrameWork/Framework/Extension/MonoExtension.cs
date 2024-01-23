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

        private readonly Dictionary<IActionNode, IActionNodeController> executeNodeDict = new Dictionary<IActionNode, IActionNodeController>();

        private readonly Stack<Action> onFinishEvents = new Stack<Action>();

        private readonly Stack<IUnRegister> unRegisters = new Stack<IUnRegister>();

        public void AddAction(IActionNode node,IActionNodeController controller)
        {          
            if (executeNodeDict.ContainsKey(node))
                executeNodeDict[node] = controller;
            else
                executeNodeDict.Add(node, controller);
        }

        public void AddUnRegister(IUnRegister register)
            => unRegisters.Push(register);

        public IUnRegister PopRegister()
            => unRegisters.Pop();
       
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
            foreach (var node in executeNodeDict.Keys)
            {
                if (!node.IsInit) node.OnInit();

                if (node.OnExecute(Time.deltaTime))
                {
                    actionNodes.Add(node);
                }
            }

            if (actionNodes.Count > 0)
            {
                for (int i = 0; i < actionNodes.Count; i++)
                {
                    actionNodes[i].OnFinish();
                    executeNodeDict.Remove(actionNodes[i]);
                }
            }

            actionNodes.Clear();
        }              

        private void OnDestroy()
        {
            foreach (var action in executeNodeDict.Keys)
            {
                action.OnFinish();
            }

            foreach (var register in unRegisters)
            {
                register.UnRegisterAllEvent();
            }

            while (onFinishEvents.Count > 0)
            {
                PopFinishEvent()?.Invoke();
            }

            unRegisters.Clear();

            executeNodeDict.Clear();

            actionNodes.Clear();
        }
    }

    public class MonoUpdateExecute : MonoBehaviour
    {
        private readonly List<IActionUpdateNode> actionUpdateNodes = new List<IActionUpdateNode>();

        private readonly Dictionary<IActionUpdateNode, IActionUpdateNodeController> updateExecuteDict = new Dictionary<IActionUpdateNode, IActionUpdateNodeController>();

        private readonly Stack<Action> onFinishEvents = new Stack<Action>();
        public void AddUpdate(IActionUpdateNode node,IActionUpdateNodeController controller)
        {          
            if (updateExecuteDict.ContainsKey(node))            
                updateExecuteDict[node] = controller;            
            else
                updateExecuteDict.Add(node, controller);
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

            foreach (var update in updateExecuteDict.Keys)
            {
                float deltaTime = 0;
                switch (updateStatus)
                {
                    case UpdateStatus.OnUpdate:
                        deltaTime = Time.deltaTime;
                        break;
                    case UpdateStatus.OnFixedUpdate:
                        deltaTime = Time.fixedDeltaTime;
                        break;
                    case UpdateStatus.OnLateUpdate:
                        deltaTime = Time.deltaTime;
                        break;

                };

                if (update.UpdateStatus == updateStatus)
                {
                    if (update.OnExecute(deltaTime))
                    {
                        actionUpdateNodes.Add(update);
                    }
                }
            }

            if (actionUpdateNodes.Count > 0)
            {
                for (int i = 0; i < actionUpdateNodes.Count; i++)
                {
                    actionUpdateNodes[i].OnFinish();
                    updateExecuteDict.Remove(actionUpdateNodes[i]);
                }
            }

            actionUpdateNodes.Clear();
        }

        private void OnDestroy()
        {            
            foreach (var node in updateExecuteDict.Keys)
            {
                node.OnFinish();
            }

            while (onFinishEvents.Count > 0)
            {
                PopFinishEvent()?.Invoke();
            }

            updateExecuteDict.Clear();

            actionUpdateNodes.Clear();          
        }

    }
}