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

        private List<KeyValuePair<IActionNode, IActionNodeController>> queueActionNodes = new List<KeyValuePair<IActionNode, IActionNodeController>>();

        private readonly Dictionary<IActionNode, IActionNodeController> executeNodeDict = new Dictionary<IActionNode, IActionNodeController>();

        private readonly Stack<Action> onFinishEvents = new Stack<Action>();

        private readonly Stack<IUnRegister> unRegisters = new Stack<IUnRegister>();

        public void AddAction(IActionNode node,IActionNodeController controller)
        {                     
            queueActionNodes.Add(new KeyValuePair<IActionNode, IActionNodeController>(node, controller));
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
            if (queueActionNodes.Count > 0)
            {
                foreach (var queueNode in queueActionNodes)
                {                  
                    executeNodeDict[queueNode.Key] = queueNode.Value;
                }
                queueActionNodes.Clear();
            }

            foreach (var node in executeNodeDict.Keys)
            {                
                if (!node.IsInit) node.OnInit();

                if (node.OnExecute(Time.deltaTime) || node.IsCompleted)
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
            queueActionNodes.Clear();
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

}