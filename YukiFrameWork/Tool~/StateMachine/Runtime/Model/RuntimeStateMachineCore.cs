///=====================================================
/// - FileName:      RuntimeStateMachineCore.cs
/// - NameSpace:     YukiFrameWork.Machine
/// - Description:   高级定制脚本生成
/// - Creation Time: 2025/3/7 19:17:07
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
using Sirenix.OdinInspector;
using YukiFrameWork.Extension;

namespace YukiFrameWork.Machine
{
    /// <summary>
    /// 状态机集合体的运行时配置
    /// </summary>
    public class RuntimeStateMachineCore : ScriptableObject
    {
        /// <summary>
        /// 背景线的中心点坐标
        /// </summary>
        [HideInInspector]
        public Vector3 viewPosition = new Vector3(100, 100, 0);
        [HideInInspector]
        public Vector3 viewScale = Vector3.one * 0.8f;

        /// <summary>
        /// <summary>
        /// 运行时实例化该配置的guid
        /// </summary>
        [SerializeField,HideInInspector]
        public string runtime_GUID;
        /// <summary>
        /// 状态机的所有图层与其所在的所有状态
        /// </summary>
        [SerializeField,HideInInspector]
        public YDictionary<string,List<StateNodeData>> all_runtime_States = new YDictionary<string, List<StateNodeData>>();

        /// <summary>
        /// 状态机的所有图层分别的条件过渡
        /// </summary>
        [SerializeField,HideInInspector]
        public YDictionary<string, List<StateTransitionData>> all_runtime_Transitions = new YDictionary<string, List<StateTransitionData>>();

        [SerializeField,HideInInspector]
        public List<StateParameterData> all_runtime_parameters = new List<StateParameterData>();
        /// <summary>
        /// 选择的状态机层级
        /// </summary>
        [SerializeField,HideInInspector]
        public List<string> SelectLayers = new List<string>();

        private Dictionary<string, StateParameterData> parameters_dic = new Dictionary<string, StateParameterData>();

        /// 检查名称是否唯一
        /// </summary>
        /// <returns></returns>
        public bool CheckStateNameOnly(string check,bool isBuildState)
        {
            foreach (var item in all_runtime_States.Values)
            {
                foreach (var data in item)
                {
                    if(data.name == check && !isBuildState)
                        return true;    
                }
            }
            return false;
        }

        public void Init()
        {
            all_runtime_States.Add(StateMachineConst.BaseLayer, new List<StateNodeData>());
            all_runtime_Transitions.Add(StateMachineConst.BaseLayer, new List<StateTransitionData>());
        }

        /// <summary>
        /// 检测参数名称是否唯一
        /// </summary>
        /// <param name="check"></param>
        /// <returns></returns>
        public bool CheckParameterNameOnly(string check)
        {
            foreach (var item in all_runtime_parameters)
            {
                if (item.parameterName == check) return true;
            }

            return false;
        }
        public StateParameterData GetParameterData(string name)
        {
            if (name == null)
                return null;

            if (parameters_dic.Count == 0)
            {
                foreach (var item in all_runtime_parameters)
                {
                    if (parameters_dic.ContainsKey(item.parameterName))
                        throw new System.Exception(string.Format("参数名称重复:{0}", item.parameterName));
                    parameters_dic.Add(item.parameterName, item);
                }
            }

            if (parameters_dic.ContainsKey(name))
                return parameters_dic[name];

            return null;
        }
        public StateNodeData GetCurrentNodeData(string layer,string name)
        {
            if (name.IsNullOrEmpty()) return null;
           
            if (all_runtime_States.TryGetValue(layer, out var list))
            {
                foreach (var item in list)
                {
                    if (item.name == name)
                        return item;
                }
            }

            return null;

        }

        public StateTransitionData GetCurrentTransitionData(string layer,string from,string to) 
        {           
            if (all_runtime_Transitions.TryGetValue(layer, out var list))
            {
                foreach (var item in list)
                {
                    if(item.ToString() == $"{from}:{to}")
                        return item;
                }
            }

            return null;
        }

        public StateNodeData GetCurrentDefaultNodeData(string layer)
        {
            if (all_runtime_States.TryGetValue(layer, out var list))
            {              
                foreach (var item in list)
                {
                    if (item.IsDefaultState)
                        return item;
                }
            }

            return null;
        }

#if UNITY_EDITOR
        private void Awake()
        {
            var item = FrameworkConfigInfo.GetFrameworkConfig();
            if (!Application.isPlaying && item)
            {
                nameSpace = item.nameSpace;
            }
        }
        [PropertySpace(10)]
        [LabelText("打开状态类生成器")]       
        public bool openGenerator;
        [ShowIf(nameof(openGenerator))]
        [BoxGroup("状态类生成设置")]
        public string nameSpace;
        [ShowIf(nameof(openGenerator))]
        [BoxGroup("状态类生成设置")]
        public string className;
        [FolderPath]
        [ShowIf(nameof(openGenerator))]
        [BoxGroup("状态类生成设置")]
        public string path = "Assets/Scripts";
        [PropertySpace(10)]
        [Button("创建脚本",ButtonHeight = 35),BoxGroup("状态类生成设置"),ShowIf(nameof(openGenerator))]
        void CreateScript()
        {           
            if (className.IsNullOrEmpty())
                throw new Exception("类名为空");
            if (nameSpace.IsNullOrEmpty())
                throw new Exception("命名空间为空");
            var builder = new StateBehaviourGenerator().BuildFile();
            builder = builder
                .Replace("用于替换提示类",className)
                .Replace("框架配置没有设置命名空间字符串已生成需要自己替换的默认命名空间",nameSpace);
            builder.CreateFileStream(path,className,".cs");
        }

        private UnityEditor.MonoScript monoScript => UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEditor.MonoScript>(path + "/" + className + ".cs");
        private bool IsShow => monoScript && openGenerator;
        [ShowIf(nameof(IsShow))]
        [Button("打开脚本",ButtonHeight = 35)]
        [PropertySpace(10)]
        [BoxGroup("状态类生成设置")]
        void OpenScript()
        {
            monoScript.Open();
        }
        public void Save()
        {
            UnityEditor.EditorUtility.SetDirty(this);
        }
        public List<StateNodeData> GetCurrentShowStateNodeData(string layer)
        {

            if (layer == null)
                return null;

            if (all_runtime_States.TryGetValue(layer, out var list))
                return list;
            ClearLayer();

            return null;
        }

        public List<StateTransitionData> GetCurrentShowTransitionData(string parent)
        {

            if (parent == null)
                return null;

            if (all_runtime_Transitions.TryGetValue(parent, out var list))
                return list;
            return null;
        }

        private Dictionary<string, List<StateNodeData>> current_show_states = new Dictionary<string, List<StateNodeData>>();
        private Dictionary<string, List<StateTransitionData>> current_show_transitions = new Dictionary<string, List<StateTransitionData>>();

        public IEnumerable Array => throw new NotImplementedException();

        public Type ImportType => typeof(int);

        public void AddState(string layer,StateNodeData state)
        {
            ClearCache();
            if (all_runtime_States.ContainsKey(layer))
                all_runtime_States[layer].Add(state);
            else all_runtime_States.Add(layer, new List<StateNodeData>() { state });
            this.Save();
        }

        public void RemoveState(string layer,StateNodeData state)
        {
            ClearCache();
            if (all_runtime_States.ContainsKey(layer))
                all_runtime_States[layer].Remove(state);           
            this.Save();
        }

        public void AddParameters(StateParameterData data)
        {
            ClearCache();
            all_runtime_parameters.Add(data);
            this.Save();
        }

        public void RemoveParameters(StateParameterData data)
        {
            ClearCache();
            all_runtime_parameters.Remove(data);
            this.Save();
        }

        public void AddTransition(string layer,StateTransitionData data)
        {
            ClearCache();
            if (all_runtime_Transitions.ContainsKey(layer))
                all_runtime_Transitions[layer].Add(data);
            else all_runtime_Transitions.Add(layer, new List<StateTransitionData>() { data });
            this.Save();
        }

        public void RemoveTransition(string layer,StateTransitionData data)
        {
            ClearCache();
            if (all_runtime_Transitions.ContainsKey(layer))
                all_runtime_Transitions[layer].Remove(data);
            this.Save();
        }
        public void ClearCache()
        {            
            parameters_dic.Clear();
            current_show_states.Clear();
            current_show_transitions.Clear();
           
        }
        /// <summary>
        /// 清空当前用户选择的状态层级
        /// </summary>
        public void ClearLayer()
        {
            SelectLayers.Clear();
            this.Save();
        }
#endif
    }
}
