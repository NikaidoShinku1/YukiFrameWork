///=====================================================
/// - FileName:      ListenerTool.cs
/// - NameSpace:     YukiFrameWork.Machine
/// - Description:   高级定制脚本生成
/// - Creation Time: 2025/3/9 21:41:16
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
    public class ListenerTool 
    {

        [InitializeOnLoadMethod]
        private static void InitOnLoad()
        {
            //Debug.Log("Init");
            Selection.selectionChanged += OnSelectionChange;


            EditorApplication.quitting += OnEditorApplicationQuitting;
        }

        private static void OnSelectionChange()
        {
            RuntimeStateMachineCore controller = Selection.activeObject as RuntimeStateMachineCore;
            if (controller != null)
            {
                
                Global.Instance.StateManagerInstanceID = 0;
                string assetPath = AssetDatabase.GetAssetPath(controller);
                string guid = AssetDatabase.AssetPathToGUID(assetPath);
                Global.Instance.RuntimeStateMachineCoreGUID = guid;
            }
            else
            {
                GameObject gameObj = Selection.activeGameObject;

                if (gameObj == null) return;

                if (gameObj.activeInHierarchy == false)
                {
                    string path = AssetDatabase.GetAssetPath(gameObj);
                    if (string.IsNullOrEmpty(path)) 
                        return;
                }
                // 判断有没有状态机组件
                var stateManager = gameObj.GetComponent<StateManager>();
                if (StateManagerIsEmpty(stateManager))
                    return;

                for (int i = 0; i < stateManager.Editor_All_StateMachineCores.Count; i++)
                {
                    if (stateManager.Editor_All_StateMachineCores[i] != null)
                    {
                        Global.Instance.StateManagerIndex = i;
                        break;
                    }
                }

                Global.Instance.StateManagerInstanceID = gameObj.GetInstanceID();
            }

            // 刷新
            Global.Instance.RefreshRuntimeStateMachineCoreGUID();
        }


        private static bool StateManagerIsEmpty(StateManager stateManager)
        {
            if (stateManager == null)
                return true;

            if (stateManager.Editor_All_StateMachineCores == null)
                return true;

            if (stateManager.Editor_All_StateMachineCores.Count == 0)
                return true;

            for (int i = 0; i < stateManager.Editor_All_StateMachineCores.Count; i++)
            {
                if (stateManager.Editor_All_StateMachineCores[i] != null)
                    return false;
            }

            return true;
        }

        private static void OnEditorApplicationQuitting()
        {
            Global.Instance.StateManagerIndex = 0;
            Global.Instance.StateManagerInstanceID = 0;
        }

        [UnityEditor.Callbacks.OnOpenAsset(0)]
        private static bool OnOpenAsset(int insId, int line)
        {
            RuntimeStateMachineCore obj = EditorUtility.InstanceIDToObject(insId) as RuntimeStateMachineCore;
            if (obj != null)
            {
                EditorWindow.GetWindow<StateMachineEditorWindow>().Show();
            }
            return obj != null;
        }

    }
}
#endif