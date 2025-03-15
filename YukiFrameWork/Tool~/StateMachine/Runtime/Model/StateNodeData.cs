///=====================================================
/// - FileName:      State.cs
/// - NameSpace:     YukiFrameWork.Example
/// - Description:   高级定制脚本生成
/// - Creation Time: 2025/3/7 14:09:01
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
namespace YukiFrameWork.Machine
{
    [Serializable]
    public class StateDependStateBehaviourScriptInfo
    {
        public string typeName;
        public string guid;   
    }
    [Serializable]
    public class StateNodeData 
    {
        public string parentStateMachineName;
        public string name;
        
        public List<StateDependStateBehaviourScriptInfo> behaviourInfos = new List<StateDependStateBehaviourScriptInfo>();

        [LabelText("是否是初始化的状态")]
        public bool isBuildInitState;
        [LabelText("初始化构建的状态名称")]
        public string buildStateName;
        [HideInInspector]
        public bool IsAnyState => name.Equals(StateMachineConst.anyState) || (isBuildInitState && buildStateName.Equals(StateMachineConst.anyState));
        [HideInInspector]
        public bool IsEntryState => name.Equals(StateMachineConst.entryState) || (isBuildInitState && buildStateName.Equals(StateMachineConst.entryState));
        [HideInInspector]
        public bool IsUpState => isBuildInitState && buildStateName.Equals(StateMachineConst.up);

        [SerializeField]
        public bool IsSubStateMachine;

        [SerializeField]
        public bool IsDefaultState;

        /// <summary>
        /// 返回状态的返回层级名称
        /// </summary>
        [SerializeField]
        public string upMachineName;
        [ShowInInspector,LabelText("状态显示名称")]
        public string DisPlayName
        {
            get
            {
                if (IsAnyState)
                    return StateMachineConst.anyState;
                if(IsEntryState)
                    return StateMachineConst.entryState;
                if (IsUpState)
                    return $"({StateMachineConst.up}) {upMachineName}";
                return name;
            }
        }

#if UNITY_EDITOR
        public Rect position;


        /// <summary>
        /// 根据状态脚本全名称获取脚本guid(仅编辑器有效)
        /// </summary>
        /// <returns></returns>
        public static string GetGUIDByStateClassFullName(string fullName)
        {
            MonoScript[] scripts = YukiAssetDataBase.FindAssets<MonoScript>();

            foreach (var item in scripts)
            {
                if (item.GetClass() != null && item.GetClass().FullName.Equals(fullName))
                {
                    return AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(item));
                }
            }

            return string.Empty;
        }
        public void RemoveStateScript(MonoScript script)
        {
            Type type = script.GetClass();
            if (type == null) return;

            string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(script));
            if (string.IsNullOrEmpty(guid))
                throw new System.Exception(string.Format("脚本添加失败,获取guid失败:{0}", script.name));

            StateDependStateBehaviourScriptInfo info = null;

            foreach (var item in behaviourInfos)
            {
                if (item.guid.Equals(guid))
                {
                    info = item;
                    break;
                }
            }

            if (info == null) return;

            behaviourInfos.Remove(info);
        }
        public void AddStateScript(MonoScript script)
        {
            string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(script));
            if (string.IsNullOrEmpty(guid))
                throw new System.Exception(string.Format("脚本添加失败,获取guid失败:{0}", script.name));
            foreach (var item in behaviourInfos)
            {
                if (item.guid.Equals(guid))
                    throw new System.Exception("脚本重复添加!");
            }
            StateDependStateBehaviourScriptInfo info = new StateDependStateBehaviourScriptInfo();
            info.guid = guid;
            Type type = script.GetClass();
            if (type != null)
            {
                info.typeName = type.FullName;
            }
            behaviourInfos.Add(info);
        }

        public void RefreshStateScripts(RuntimeStateMachineCore runtimeStateMachineCore)
        {          
            List<StateDependStateBehaviourScriptInfo> invalid = new List<StateDependStateBehaviourScriptInfo>();

            foreach (var item in behaviourInfos)
            {
                if (string.IsNullOrEmpty(item.guid) && !string.IsNullOrEmpty(item.typeName))
                    item.guid = GetGUIDByStateClassFullName(item.typeName);

                string path = AssetDatabase.GUIDToAssetPath(item.guid);
                if (string.IsNullOrEmpty(path))
                {
                    invalid.Add(item);
                    continue;
                }
                MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                if (script == null)
                {                  
                    continue;
                }
                Type type = script.GetClass();             
                if (type == null)
                {
                    continue;
                }

                item.typeName = type.FullName;
            }
            // 移除无效的脚本
            foreach (var item in invalid)
            {
                behaviourInfos.Remove(item);
            }

            if (runtimeStateMachineCore)
                runtimeStateMachineCore.Save();
        }      

#endif

    }
}
