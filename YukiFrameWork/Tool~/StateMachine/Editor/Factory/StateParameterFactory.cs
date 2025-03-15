///=====================================================
/// - FileName:      StateParameterFactory.cs
/// - NameSpace:     YukiFrameWork.Machine
/// - Description:   高级定制脚本生成
/// - Creation Time: 2025/3/9 21:01:39
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Text;
#if UNITY_EDITOR
namespace YukiFrameWork.Machine
{
    public class StateParameterFactory 
    {
        // 创建参数
        public static void CreateParamter(RuntimeStateMachineCore controller, ParameterType type)
        {

            StateParameterData parameter = new StateParameterData();
            parameter.parameterName = GetDefualtName(controller, type);
            parameter.parameterType = type;
            parameter.Parameter.Value = 0;

            controller.AddParameters(parameter);
        }

        private static string GetDefualtName(RuntimeStateMachineCore controller, ParameterType type)
        {
            string name = string.Format("New {0}", type.ToString());
            string tempName = name;

            int i = 1;
            while (controller.GetParameterData(tempName) != null)
            {
                tempName = string.Format("{0}{1}", name, i);
                i++;
            }

            return tempName;
        }

        // 删除参数
        public static void RemoveParamter(RuntimeStateMachineCore controller, int index)
        {

            if (Application.isPlaying) return;

            if (controller == null) return;

            StateParameterData parameter = controller.all_runtime_parameters[index];

            List<StateTransitionData> transitions = new List<StateTransitionData>();

            // 查询引用了这个参数的过渡
            foreach (var item in controller.all_runtime_Transitions)
            {
                foreach (var transition in item.Value)
                {
                    foreach (var condition in transition.conditions)
                    {
                        if (condition.parameterName != null && condition.parameterName.Equals(parameter.parameterName))
                        {
                            transitions.Add(transition);
                            break;
                        }
                    }

                }
            }

            if (transitions.Count == 0)
            {
                controller.RemoveParameters(parameter);
            }
            else
            {

                StringBuilder content = new StringBuilder();
                content.Append("确定删除参数:").Append(parameter.parameterName).Append("吗？").Append("\n");
                content.Append("有以下过渡引用此参数!\n");

                foreach (var item in transitions)
                {
                    content.Append(item.fromStateName).Append(" -> ").Append(item.toStateName);
                }

                if (UnityEditor.EditorUtility.DisplayDialog("删除参数", content.ToString(), "确定", "取消"))
                {
                    controller.RemoveParameters(parameter);

                    foreach (var item in transitions)
                    {
                        for (int i = item.conditions.Count - 1; i >= 0; i--)
                        {
                            StateConditionData condition = item.conditions[i];
                            if (condition.parameterName != null && condition.parameterName.Equals(parameter.parameterName))
                            {
                                item.conditions.RemoveAt(i);
                            }
                        }

                    }

                }
            }

        }


        // 重命名参数
        public static void RenameParamter(RuntimeStateMachineCore controller, StateParameterData parameter, string newName)
        {

            // 判断名称是否为空 
            if (string.IsNullOrEmpty(newName))
            {
                StateMachineEditorWindow.ShowNotification("参数名称不能为空!");
                Debug.LogError("参数名称不能为空!");
                return;
            }
            // 判断新的名称是否已经存在 
            if (controller.GetParameterData(newName) != null)
            {
                StateMachineEditorWindow.ShowNotification("参数名称已经存在!");
                Debug.LogError("参数名称已经存在!");
                return;
            }

            // 查找到所有引用此参数的过渡 修改名称 
            foreach (var item in controller.all_runtime_Transitions.Values)
            {
                foreach (var transition in item)
                {
                    // 遍历所有的条件
                    foreach (var condition in transition.conditions)
                    {
                        if (condition.parameterName != null && condition.parameterName.Equals(parameter.parameterName))
                        {
                            condition.parameterName = newName;
                        }
                    }

                    // 遍历其他组的条件
                    foreach (var group in transition.conditionGroups)
                    {
                        foreach (var condition in group)
                        {
                            if (condition.parameterName != null && condition.parameterName.Equals(parameter.parameterName))
                            {
                                condition.parameterName = newName;
                            }
                        }
                    }
                }              

            }



            // 修改参数名称
            parameter.parameterName = newName;
            controller.ClearCache();
            controller.Save();
        }


    }
}
#endif