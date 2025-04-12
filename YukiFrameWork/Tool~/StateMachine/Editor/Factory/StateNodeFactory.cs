///=====================================================
/// - FileName:      StateNodeFectory.cs
/// - NameSpace:     YukiFrameWork.Machine
/// - Description:   高级定制脚本生成
/// - Creation Time: 2025/3/8 19:46:22
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEditor;
using System.Linq;
#if UNITY_EDITOR
namespace YukiFrameWork.Machine
{
    public class StateNodeFactory 
    {
        public static StateNodeData CreateStateNode(RuntimeStateMachineCore runtimeStateMachineCore, string name,string machineLayerName, Rect rect, bool defaultState, bool isSubStateMachine = false, bool isBuildInState = false, string buildInStateName = "")
        {

            // 判断名称是否重复 
            if (runtimeStateMachineCore.CheckStateNameOnly(name,isBuildInState))
            {
                string message = string.Format("创建状态节点失败,名称:{0}重复!", name);
                Debug.LogError(message);
                StateMachineEditorWindow.ShowNotification(message);
                return null;
            }

            StateNodeData node = new StateNodeData();
            node.name = name;
            node.position = rect;
            node.IsSubStateMachine = isSubStateMachine;
            List<StateNodeData> items = null;
            if(runtimeStateMachineCore.all_runtime_States.ContainsKey(machineLayerName))            
                items = runtimeStateMachineCore.all_runtime_States[machineLayerName];
            else items = new List<StateNodeData>();
            node.isBuildInitState = isBuildInState;
            node.buildStateName = buildInStateName;          
            node.parentStateMachineName = machineLayerName;
            if(node.IsUpState)
                node.upMachineName = Global.Instance.LayerParent;
            items.Add(node);
          
            if (defaultState)
            {
                foreach (var item in items)
                {
                    item.IsDefaultState = false;
                }               
            }
            node.IsDefaultState = defaultState;
            runtimeStateMachineCore.all_runtime_States[machineLayerName] = items;
            AssetDatabase.SaveAssets();
            return node;
        }

        public static StateNodeData CreateStateNode(RuntimeStateMachineCore runtimeStateMachineCore, string machineLayerName, Rect rect, bool defaultState, bool isSubStateMachine = false, bool isBuildInState = false, string buildInStateName = "")
        {
            return CreateStateNode(runtimeStateMachineCore, GetStateNodeName(runtimeStateMachineCore), machineLayerName, rect, defaultState, isSubStateMachine, isBuildInState, buildInStateName);
        }

        private static string GetStateNodeName(RuntimeStateMachineCore runtimeStateMachineCore)
        {

            string name = null;
            int i = 1;
            do
            {
                name = string.Format("New State{0}", i);
                i++;
            } while (runtimeStateMachineCore.CheckStateNameOnly(name,false));



            return name;
        }

        public static void DeleteState(RuntimeStateMachineCore runtimeStateMachineCore, StateNodeData state,string machineLayerName = StateMachineConst.BaseLayer)
        {

            // 判断 删除的是不是 entry  或者 any 
            if (state.IsAnyState || state.IsEntryState || state.IsUpState)
            {             
                    string message = string.Format("状态:{0}不能删除!", state.DisPlayName);
                    Debug.LogWarning(message);
                   //FSMEditorWindow.ShowNotification(message);
                    return;
                
            }

            // 删除相关的过渡 
            var transitions = runtimeStateMachineCore.all_runtime_Transitions.ToArray();
             for (int i = 0; i < transitions.Length; i++)
             {
                for (int j = 0; j < transitions[i].Value.Count; j++)
                {
                    StateTransitionData transitionData = transitions[i].Value[j];
                    if (transitionData.fromStateName.Equals(state.name) || transitionData.toStateName.Equals(state.name))
                    {
                        runtimeStateMachineCore.RemoveTransition(machineLayerName,transitionData);
                        j--;
                    }
                    if (transitions[i].Value.Count == 0)
                        runtimeStateMachineCore.all_runtime_Transitions.Remove(transitions[i].Key);
                }
                 
             }
            var items = runtimeStateMachineCore.all_runtime_States[machineLayerName];
            items.Remove(state);
            // 判断是不是默认状态 
            if (state.IsDefaultState)
            {
                foreach (var item in items)
                {
                    if (item.IsAnyState || item.IsEntryState || item.IsUpState)
                        continue;
                  
                    item.IsDefaultState = true;
                    break;
                }
            }

            // 判断是不是子状态机
            if (state.IsSubStateMachine)
            {
                runtimeStateMachineCore.all_runtime_States.Remove(state.name);
            }
            runtimeStateMachineCore.all_runtime_States[machineLayerName] = items;

        }

        public static bool Rename(RuntimeStateMachineCore runtimeStateMachineCore, StateNodeData node, string newName)
        {

            if (node.name.Equals(StateMachineConst.entryState) || node.name.Equals(StateMachineConst.anyState))
            {
                return false;
            }

            if (string.IsNullOrEmpty(newName))
            {
                StateMachineEditorWindow.ShowNotification("名称不能为空!");
                return false;
            }

            if (runtimeStateMachineCore.CheckStateNameOnly(newName,false))
            {
                string message = string.Format("状态重命名失败,名称:{0}已经存在,请使用其他的名称!", newName);
                StateMachineEditorWindow.ShowNotification(message);
                return false;
            }

            // 找到 相关的过渡 进行修改 
            foreach (var transitions in runtimeStateMachineCore.all_runtime_Transitions.Values)
            {
                foreach (var item in transitions)
                {
                    if (item.fromStateName.Equals(node.name))
                    {
                        item.fromStateName = newName;
                    }

                    if (item.toStateName.Equals(node.name))
                    {
                        item.toStateName = newName;
                    }
                }
            }

            string oldName = node.name;

            node.name = newName;

            // 修改子状态层级
            if (node.IsSubStateMachine)
            {
                var items = runtimeStateMachineCore.all_runtime_States[oldName];

                runtimeStateMachineCore.all_runtime_States.Remove(oldName);
                runtimeStateMachineCore.all_runtime_States.Add(newName, items);

                foreach (var item in items)
                {
                    item.parentStateMachineName = newName;
                }
            }         
            EditorUtility.SetDirty(runtimeStateMachineCore);

            return true;
        }

    }


}

#endif