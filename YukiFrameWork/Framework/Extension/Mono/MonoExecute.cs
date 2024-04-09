using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YukiFrameWork
{
    public class MonoExecute : MonoBehaviour
    {
        private readonly List<IActionUpdateNode> actionUpdateNodes = new List<IActionUpdateNode>();
        private List<KeyValuePair<IActionUpdateNode, IActionUpdateNodeController>> queueActionNodes = new List<KeyValuePair<IActionUpdateNode, IActionUpdateNodeController>>();
        private readonly Dictionary<IActionUpdateNode, IActionUpdateNodeController> updateExecuteDict = new Dictionary<IActionUpdateNode, IActionUpdateNodeController>();
        
        private readonly Stack<Action> onFinishEvents = new Stack<Action>();
        public void AddUpdate(IActionUpdateNode node, IActionUpdateNodeController controller)
        {
            queueActionNodes.Add(new KeyValuePair<IActionUpdateNode, IActionUpdateNodeController>(node, controller));
        }

        public readonly EasyEvent<bool> applicationFocus = new EasyEvent<bool>();
        public readonly EasyEvent<bool> applicationPause = new EasyEvent<bool>();
        public readonly EasyEvent applicationQuit = new EasyEvent();
        public readonly EasyEvent onGUI = new EasyEvent();
        public readonly EasyEvent onDrawGizmos = new EasyEvent();
        public readonly EasyEvent onCanvasGroupChange = new EasyEvent();

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
                    updateExecuteDict[queueNode.Key] = queueNode.Value;
                }
                queueActionNodes.Clear();
            }

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

        private void OnApplicationFocus(bool focus)
        {
            applicationFocus?.SendEvent(focus);
        }

        private void OnApplicationPause(bool pause)
        {
            applicationPause?.SendEvent(pause);
        }

        private void OnApplicationQuit()
        {
            applicationQuit?.SendEvent();
        }

        private void OnCanvasGroupChanged()
        {
            onCanvasGroupChange?.SendEvent();
        }

        private void OnGUI()
        {
            onGUI?.SendEvent();
        }

        private void OnDrawGizmos()
        {
            onDrawGizmos?.SendEvent();
        }

        private void OnDestroy()
        {
            queueActionNodes.Clear();
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