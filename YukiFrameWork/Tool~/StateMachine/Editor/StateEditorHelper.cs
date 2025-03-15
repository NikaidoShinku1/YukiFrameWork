///=====================================================
/// - FileName:      StateEditorHelper.cs
/// - NameSpace:     YukiFrameWork.Machine
/// - Description:   高级定制脚本生成
/// - Creation Time: 2025/3/9 14:16:54
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
namespace YukiFrameWork.Machine
{
    public class StateEditorHelper 
    {

        [MenuItem("Assets/Create/YukiFrameWork/Yuki有限状态机")]
        static void CreateRuntimeStateMachineCore()
        {
            StateMachineCoreCreator stateMachineCoreCreator = ScriptableObject.CreateInstance<StateMachineCoreCreator>();

            string name = GetName();
#if UNITY_2018
            GUIContent content = EditorGUIUtility.IconContent("icons/processed/unityengine/billboardasset icon.asset");
#elif UNITY_2019_1_OR_NEWER
            GUIContent content = EditorGUIUtility.IconContent("d_ScriptableObject Icon");
#endif
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(stateMachineCoreCreator.GetInstanceID(),stateMachineCoreCreator,name,(Texture2D)content.image,default);
        }

        public class StateMachineCoreCreator : EndNameEditAction
        {
            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                RuntimeStateMachineCore runtimeStateMachineCore = ScriptableObject.CreateInstance<RuntimeStateMachineCore>();
              
                AssetDatabase.CreateAsset(runtimeStateMachineCore, pathName);
                Selection.activeObject = runtimeStateMachineCore;
                runtimeStateMachineCore.Init();

                Rect rect = new Rect(0, 100,StateMachineConst.StateNodeWith, StateMachineConst.StateNodeHeight);
                StateNodeFactory.CreateStateNode(runtimeStateMachineCore, StateMachineConst.anyState,StateMachineConst.BaseLayer, rect, false, false, true, StateMachineConst.anyState);
                rect = new Rect(0, 300, StateMachineConst.StateNodeWith, StateMachineConst.StateNodeHeight);
                StateNodeFactory.CreateStateNode(runtimeStateMachineCore, StateMachineConst.entryState, StateMachineConst.BaseLayer, rect, false, false, true, StateMachineConst.entryState);

            }
        }

        internal static string GetName(string templateName = "New StateMachine", string suffix = "asset")
        {

            string name;
            string[] files;

            int i = 0;

            do
            {
                name = string.Format("{0}{1}", templateName, i == 0 ? string.Empty : i.ToString());
                files = AssetDatabase.FindAssets(name);

                i++;
            } while (files != null && files.Length != 0);

            return string.Format("{0}.{1}", name, suffix);
        }
    }
}
#endif