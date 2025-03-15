///=====================================================
/// - FileName:      Global.cs
/// - NameSpace:     YukiFrameWork.Machine
/// - Description:   高级定制脚本生成
/// - Creation Time: 2025/3/9 20:05:23
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;

namespace YukiFrameWork.Machine
{
    public class Global : Singleton<Global>
    {
        #region 字段

        private RuntimeStateMachineCore _runtimeMachineCore;

        public List<StateNodeData> SelectNodes = new List<StateNodeData>();

        public StateTransitionData selectTransition;

        public bool isPreviewTransition = false;
        public StateNodeData fromState = null;
        public StateNodeData hoverState = null;

        private StateManager _StateManager;
  

        #endregion

        #region Layers

        private List<string> empty_layers = new List<string>();

        public List<string> Layers
        {
            get
            {

                if (RuntimeStateMachineCore != null)
                    return RuntimeStateMachineCore.SelectLayers;

                return empty_layers;
            }
        }

        public string LayerParent
        {
            get
            {
                if (Layers.Count == 0)
                    return StateMachineConst.BaseLayer;
                return Layers[Layers.Count - 1];
            }
        }

        public void AddLayer(string layer)
        {
            if (RuntimeStateMachineCore == null) return;
            RuntimeStateMachineCore.SelectLayers.Add(layer);
        }

        public void RemoveLayer(int index, int count)
        {
            if (RuntimeStateMachineCore == null) return;
            RuntimeStateMachineCore.SelectLayers.RemoveRange(index, count);
        }

        public void RemoveLast(string name)
        {
            if (RuntimeStateMachineCore == null) return;
            int index = RuntimeStateMachineCore.SelectLayers.Count - 1;
            if (index >= 0 && index < RuntimeStateMachineCore.SelectLayers.Count)
            {
                string last = RuntimeStateMachineCore.SelectLayers[index];
                if (!last.Equals(name))
                    return;
                RuntimeStateMachineCore.SelectLayers.RemoveRange(index, 1);
            }
        }

        #endregion

        #region 属性

        // 这个逻辑修改为如果只有一个,逻辑不变,
        // 如果有多个,判断当前显示的是不是在多个中,如果在不做处理,如果不在设置为第一个
        public RuntimeStateMachineCore RuntimeStateMachineCore
        {
            get
            {
                if (!IsEmpty(RuntimeStateMachineCores))
                {                 
                    if (StateManagerIndex < 0 || StateManagerIndex >= RuntimeStateMachineCores.Count)
                        StateManagerIndex = 0;

                    RuntimeStateMachineCore controller = RuntimeStateMachineCores[StateManagerIndex];
                   
                    if (controller != null && !string.IsNullOrEmpty(controller.runtime_GUID) && _runtimeStateManagerGUID != controller.runtime_GUID)
                    {                        
                        PlayerPrefs.SetString("RuntimeStateMachineCoreGUID", controller.runtime_GUID);
                        _runtimeStateManagerGUID = controller.runtime_GUID;                       
                    }

                    return controller;
                }

                if (_runtimeMachineCore == null)
                {
                    string path = AssetDatabase.GUIDToAssetPath(RuntimeStateMachineCoreGUID);
                    _runtimeMachineCore = AssetDatabase.LoadAssetAtPath<RuntimeStateMachineCore>(path);
                }

                return _runtimeMachineCore;
            }
            internal set
            {
                if (_runtimeMachineCore == value) return;
                _runtimeMachineCore = value;
            }
        }

        public StateManager StateManager
        {
            get
            {

                if (_StateManager == null)
                {
                    GameObject gameObj = EditorUtility.InstanceIDToObject(StateManagerInstanceID) as GameObject;
                    if (gameObj == null) return null;
                    _StateManager = gameObj.GetComponent<StateManager>();
                }

                return _StateManager;
            }
        }


        private List<RuntimeStateMachineCore> currentStateManagers = new List<RuntimeStateMachineCore>();
     
        public List<RuntimeStateMachineCore> RuntimeStateMachineCores
        {
            get
            {
                if (StateManager != null)
                {
                    return StateManager.Editor_All_StateMachineCores;
                }
                currentStateManagers.Clear();

                if (_runtimeMachineCore == null)
                {
                    string path = AssetDatabase.GUIDToAssetPath(RuntimeStateMachineCoreGUID);
                    _runtimeMachineCore = AssetDatabase.LoadAssetAtPath<RuntimeStateMachineCore>(path);
                }

                if (_runtimeMachineCore != null)
                    currentStateManagers.Add(_runtimeMachineCore);
                return currentStateManagers;
            }
        }

        internal int StateManagerInstanceID
        {
            get
            {
                return PlayerPrefs.GetInt("StateManagerInstanceID", 0);
            }
            set
            {
                PlayerPrefs.SetInt("StateManagerInstanceID", value);
                UnityEngine.Object obj = EditorUtility.InstanceIDToObject(value);
                GameObject gameObj = obj as GameObject;
                if (gameObj != null && gameObj.GetComponent<StateManager>() != null)
                {
                    _StateManager = gameObj.GetComponent<StateManager>();
                }
                else
                    _StateManager = null;

            }
        }

        private string _runtimeStateManagerGUID;

        internal string RuntimeStateMachineCoreGUID
        {
            get
            {
                return PlayerPrefs.GetString("RuntimeStateMachineCoreGUID", string.Empty);
            }
            set
            {
                //Debug.LogFormat("修改GUID:{0}",value);
                PlayerPrefs.SetString("RuntimeStateMachineCoreGUID", value);
                string path = AssetDatabase.GUIDToAssetPath(value);
                RuntimeStateMachineCore = AssetDatabase.LoadAssetAtPath<RuntimeStateMachineCore>(path);
            }
        }

        internal int StateManagerIndex
        {
            get
            {
                return PlayerPrefs.GetInt("StateManagerIndex", 0);
            }
            set
            {
                if (StateManagerIndex == value) return;
                PlayerPrefs.SetInt("StateManagerIndex", value);

                // 刷新RuntimeGUID
                RefreshRuntimeStateMachineCoreGUID();
            }
        }

        #endregion

        #region 方法

        private Global()
        {
        }

        public void ClearSelections()
        {
            // 如果不是 InspectorWindow 此时清空 Inspector
            this.SelectNodes.Clear();
            selectTransition = null;
            Selection.activeObject = null;
            //Selection.activeObject = this.RuntimeStateMachineCore;
        }

        /// <summary>
        /// 获取当前显示状态节点
        /// </summary>
        /// <returns></returns>
        public List<StateNodeData> GetCurrentShowStateNodeData()
        {

            if (RuntimeStateMachineCore != null)
                return RuntimeStateMachineCore.GetCurrentShowStateNodeData(LayerParent);

            return null;
        }

        public List<StateTransitionData> GetCurrentShowTransitionData()
        {
            if (RuntimeStateMachineCore != null)
                return RuntimeStateMachineCore.GetCurrentShowTransitionData(LayerParent);

            return null;
        }


        public bool IsEmpty(List<RuntimeStateMachineCore> controllers)
        {
            if (controllers == null || controllers.Count == 0)
                return true;

            foreach (var item in controllers)
            {
                if (item) return false;
            }

            return true;
        }




        #endregion

        #region 运行时刷新选中的GUID

        // 运行时选中某一个StateManager 组件，当取消运行，这个游戏物体被销毁时
        // 仍选中这个状态配置

        private Dictionary<string, RuntimeStateMachineCore> tempLayers = new Dictionary<string,RuntimeStateMachineCore>();

        public void RefreshRuntimeStateMachineCoreGUID()
        {
            if (!Application.isPlaying) return;
            if (StateManager == null) return;
            if (IsEmpty(StateManager.Editor_All_StateMachineCores)) return;

            // 通过反射拿到源配置 
            Type type = typeof(StateManager);
            FieldInfo fieldInfo = type.GetField("runtime_StateMachineCores", BindingFlags.NonPublic | BindingFlags.Instance);
            Dictionary<string, RuntimeStateMachineCore> layers = fieldInfo.GetValue(StateManager) as Dictionary<string, RuntimeStateMachineCore>;

            if (layers == null || layers.Count == 0) return;

            tempLayers.Clear();
            foreach (var layer in layers)
            {
                if (layer.Value == null) continue;
                tempLayers.Add(layer.Key,layer.Value);
            }

            if (StateManagerIndex < 0 || StateManagerIndex >= tempLayers.Count) return;

            string asset_path = AssetDatabase.GetAssetPath(Array.IndexOf(tempLayers.Values.ToArray(),StateManagerIndex));
            string guid = AssetDatabase.AssetPathToGUID(asset_path);
            if (string.IsNullOrEmpty(guid)) return;
            RuntimeStateMachineCoreGUID = guid;

        }

        #endregion


    }
}
#endif