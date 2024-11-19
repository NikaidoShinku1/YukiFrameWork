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
using YukiFrameWork.Events;

namespace YukiFrameWork
{   
    public class OnGameObjectTrigger : MonoBehaviour
    {
        private readonly List<IActionNode> actionNodes = new List<IActionNode>();

        private List<KeyValuePair<IActionNode, IActionNodeController>> queueActionNodes = new List<KeyValuePair<IActionNode, IActionNodeController>>();

        private readonly Dictionary<IActionNode, IActionNodeController> executeNodeDict = new Dictionary<IActionNode, IActionNodeController>();

        private readonly FastList<Action> onFinishEvents = new FastList<Action>();

        private readonly Stack<IUnRegister> unRegisters  = new Stack<IUnRegister>();

        private readonly Stack<IUnRegister> unDisableRegisters = new Stack<IUnRegister>();

        public readonly EasyEvent OnUpdate = new EasyEvent();
        public readonly EasyEvent OnFixedUpdate = new EasyEvent();
        public readonly EasyEvent OnLateUpdate = new EasyEvent();

        public void AddAction(IActionNode node,IActionNodeController controller)
        {                     
            queueActionNodes.Add(new KeyValuePair<IActionNode, IActionNodeController>(node, controller));
        }

        public void AddUnRegister(IUnRegister register)
            => unRegisters.Push(register);

        public void AddUnRegisterByDisable(IUnRegister register)
            => unDisableRegisters.Push(register);

        public IUnRegister PopRegister()
            => unRegisters.Pop();
       
        public void PushFinishEvent(Action onFinish)
        {           
            onFinishEvents.Add(onFinish);
        }

        public void RemoveFinishEvent(Action onFinish)
        {
            onFinishEvents.Remove(onFinish);
        }

        private void Update()
        {
            OnUpdate.SendEvent();

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

        private void FixedUpdate()
        {
            OnFixedUpdate.SendEvent();
        }

        private void LateUpdate()
        {
            OnLateUpdate.SendEvent();
        }

        private void OnDisable()
        {
            foreach (var action in unDisableRegisters)
            {
                action.UnRegisterAllEvent();
            }

            unDisableRegisters.Clear();
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
#if UNITY_2022_1_OR_NEWER
                EventManager.Root.RemoveEvent(register);
#else
                register.UnRegisterAllEvent();
#endif
            }

            foreach (var finish in onFinishEvents)
            {
                finish?.Invoke();
            }

            onFinishEvents.Clear();

            unRegisters.Clear();

            executeNodeDict.Clear();

            actionNodes.Clear();
        }
    }

}