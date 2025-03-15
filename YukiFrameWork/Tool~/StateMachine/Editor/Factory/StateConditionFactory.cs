///=====================================================
/// - FileName:      StateConditionFectory.cs
/// - NameSpace:     YukiFrameWork.Machine
/// - Description:   高级定制脚本生成
/// - Creation Time: 2025/3/13 14:39:37
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
#if UNITY_EDITOR
namespace YukiFrameWork.Machine
{
    public class StateConditionFactory 
    {

        public static StateConditionData CreateCondition(RuntimeStateMachineCore runtimeStateMachineCore)
        {
            StateConditionData condition = new StateConditionData();

            string parameterName = string.Empty;

            StateParameterData parameter = null;

            if (runtimeStateMachineCore.all_runtime_parameters.Count > 0)
            {
                parameter = runtimeStateMachineCore.all_runtime_parameters[0];
                parameterName = parameter.parameterName;
            }

            if (parameter != null)
            {
                switch (parameter.parameterType)
                {
                    case ParameterType.Float:
                        condition.compareType = CompareType.Greater;
                        break;
                    case ParameterType.Int:
                        condition.compareType = CompareType.Greater;
                        break;
                    case ParameterType.Bool:
                        condition.compareType = CompareType.Equal;
                        break;

                }
            }
            else
            {
                condition.compareType = CompareType.Greater;
            }


            condition.targetValue = 0;
            condition.parameterName = parameterName;
            return condition;
        }

        // 创建条件
        public static void CreateCondition(RuntimeStateMachineCore controller, StateTransitionData transition)
        {
            StateConditionData condition = CreateCondition(controller);
            transition.conditions.Add(condition);

            UnityEditor.EditorUtility.SetDirty(controller);
            UnityEditor.AssetDatabase.SaveAssets();
        }

        public static void DeleteCondition(RuntimeStateMachineCore controller, StateTransitionData transition, int index)
        {
            if (index < 0 || index >= transition.conditions.Count)
            {
                return;
            }

            transition.conditions.RemoveAt(index);


            UnityEditor.EditorUtility.SetDirty(controller);
            UnityEditor.AssetDatabase.SaveAssets();
        }


    }
}
#endif