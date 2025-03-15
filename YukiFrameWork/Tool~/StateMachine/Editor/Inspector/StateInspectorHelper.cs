///=====================================================
/// - FileName:      StateInspectorHelper.cs
/// - NameSpace:     YukiFrameWork.Machine
/// - Description:   高级定制脚本生成
/// - Creation Time: 2025/3/13 14:55:54
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
namespace YukiFrameWork.Machine
{
    public class StateInspectorHelper : ScriptableObjectSingleton<StateInspectorHelper>
    {

        public StateNodeData node;
        public RuntimeStateMachineCore runtimeStateMachineCore;

        public StateGraphView grap;

        public void Inspect(RuntimeStateMachineCore runtimeStateMachineCore, StateNodeData node, StateGraphView grap)
        {

            if (node == null)
            {
                Selection.activeObject = null;
                return;
            }

            this.node = node;
            this.runtimeStateMachineCore = runtimeStateMachineCore;
            this.grap = grap;
            Selection.activeObject = this;

        }

    }
}
#endif