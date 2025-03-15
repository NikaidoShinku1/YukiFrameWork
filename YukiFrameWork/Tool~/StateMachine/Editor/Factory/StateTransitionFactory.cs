///=====================================================
/// - FileName:      StateTransitionFectory.cs
/// - NameSpace:     YukiFrameWork.Machine
/// - Description:   高级定制脚本生成
/// - Creation Time: 2025/3/13 14:39:21
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
#if UNITY_EDITOR
namespace YukiFrameWork.Machine
{
    public class StateTransitionFactory 
    {

        public static void CreateTransition(RuntimeStateMachineCore runtimeStateMachineCore, string fromStateName, string toStateName)
        {
            string layerName = Global.Instance.LayerParent;
            StateNodeData toNode = runtimeStateMachineCore.GetCurrentNodeData(layerName,toStateName);
          
            if (toNode.IsAnyState || toNode.IsEntryState || toNode.IsUpState)
            {
                string message = string.Format("状态:{0}不能添加过渡!", toNode.DisPlayName);
                Debug.LogWarning(message);
                StateMachineEditorWindow.ShowNotification(message);
                return;
            }

            if (fromStateName.Equals(toStateName))
            {
                return;
            }

            foreach (var dict in runtimeStateMachineCore.all_runtime_Transitions)
            {
                foreach (var item in dict.Value)
                {
                    if (item.fromStateName.Equals(fromStateName) && item.toStateName.Equals(toStateName))
                    {
                        string message = string.Format("过渡 {0} -> {1} 已经存在,请勿重复添加!", fromStateName, toStateName);

                        Debug.LogError(message);
                        StateMachineEditorWindow.ShowNotification(message);
                        return;
                    }
                }
            }

            StateTransitionData transition = new StateTransitionData();
            transition.fromStateName = fromStateName;
            transition.toStateName = toStateName;
            runtimeStateMachineCore.AddTransition(layerName,transition);
            UnityEditor.EditorUtility.SetDirty(runtimeStateMachineCore);
            UnityEditor.AssetDatabase.SaveAssets();

        }

        public static void DeleteTransition(RuntimeStateMachineCore controller, StateTransitionData transition)
        {
            controller.RemoveTransition(Global.Instance.LayerParent,transition);
            UnityEditor.EditorUtility.SetDirty(controller);
            UnityEditor.AssetDatabase.SaveAssets();
        }

    }

}

#endif