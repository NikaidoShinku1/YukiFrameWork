///=====================================================
/// - FileName:      StateTransitionInspectorHelper.cs
/// - NameSpace:     YukiFrameWork.Machine
/// - Description:   高级定制脚本生成
/// - Creation Time: 2025/3/13 14:56:40
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
    public class StateTransitionInspectorHelper : ScriptableObjectSingleton<StateTransitionInspectorHelper>
    {
        public StateTransitionData transition;
        public RuntimeStateMachineCore runtimeStateMachineCore;

        public void Inspect(RuntimeStateMachineCore runtimeStateMachineCore, StateTransitionData transition)
        {
            if (transition == null)
            {
                Selection.activeObject = null;
                return;
            }

            this.transition = transition;
            this.runtimeStateMachineCore = runtimeStateMachineCore;

            Selection.activeObject = this;
        }


    }
}
#endif