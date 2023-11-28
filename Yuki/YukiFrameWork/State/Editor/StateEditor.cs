///=====================================================
/// - FileName:      StateEditor.cs
/// - NameSpace:     YukiFrameWork.States
/// - Created:       Yuki
/// - Email:         Yuki@qq.com
/// - Description:   这是一个框架工具创建的状态机编辑器脚本
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
#if UNITY_EDITOR
namespace YukiFrameWork.States
{
    [CustomEditor(typeof(StateManager))]
    public class StateEditor : Editor
    {
        private StateManager stateManager;

        private bool isRecomposeScript = false;

        private readonly static List<string> animClipsName = new List<string>();

        private static SerializedProperty initType;
        private static SerializedProperty isDebugLog;
        private static SerializedProperty stateMechine;
        private new static SerializedObject serializedObject;

        private void OnEnable()
        {          
            stateManager = (StateManager)target;
            serializedObject = new SerializedObject(stateManager);
            initType = serializedObject.FindProperty("initType");
            isDebugLog = serializedObject.FindProperty("IsDebugLog");
            stateMechine = serializedObject.FindProperty("stateMechine");

        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            SetField();          

            OpenStateGraphEditor();

            Statusdetails();

            EditorUtility.SetDirty(target);
        }

        private void SetField()
        {
            GUILayout.BeginVertical();
            EditorGUILayout.Space(10);
            GUILayout.BeginHorizontal();
            GUILayout.Label("状态机初始化方式");
            EditorGUILayout.PropertyField(initType,GUIContent.none,GUILayout.Width(400));
            GUILayout.EndHorizontal();
            EditorGUILayout.Space(10);
            GUILayout.BeginHorizontal();
            GUILayout.Label("状态机调试");
            EditorGUILayout.PropertyField(isDebugLog,GUIContent.none, GUILayout.Width(400));
            GUILayout.EndHorizontal();
            EditorGUILayout.Space(10);
            GUILayout.BeginHorizontal(); 
            GUILayout.Label("状态编辑器");
            stateManager.stateMechine = (StateMechine)EditorGUILayout.ObjectField(stateManager.stateMechine,typeof(StateMechine),true,GUILayout.Width(400));
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            serializedObject.ApplyModifiedProperties();
        }

        private void OpenStateGraphEditor()
        {
            GUILayout.Space(20);
            if (GUILayout.Button("打开状态机编辑器"))
            {
                StateMechineEditor.StateMechineEditorShow(stateManager);
            }
        }

        private void Statusdetails()
        {
            if (stateManager.stateIndexs.Count > 0 && stateManager.stateMechine != null)
                foreach (var item in stateManager.stateMechine.states)
                {
                    if (stateManager.GetStateByIndex(item.index))
                    {
                        GUILayout.Space(20);
                        GUILayout.Label("状态：" + $"{item.name}");
                        item.name = EditorGUILayout.TextField("状态名称：", item.name);
                        item.index = EditorGUILayout.IntField("状态标识：", item.index);
                        EditorGUILayout.BeginHorizontal();
                        string animType = "";
                        switch (item.type)
                        {
                            case AnimType.None:
                                break;
                            case AnimType.Animation:
                                {
                                    if (item.animation == null)
                                        item.animation = stateManager.GetComponent<Animation>();
                                    if (item.animation == null)
                                    {
                                        item.animation = stateManager.GetComponentInChildren<Animation>();
                                    }
                                    item.animation = (Animation)EditorGUILayout.ObjectField(item.animation, typeof(Animation), true);
                                    animType = "旧版";
                                    item.animator = null;
                                }
                                break;
                            case AnimType.Animator:
                                {
                                    if (item.animator == null)
                                        item.animator = stateManager.GetComponent<Animator>();
                                    if (item.animator == null)
                                    {
                                        item.animator = stateManager.GetComponentInChildren<Animator>();
                                    }
                                    item.animator = (Animator)EditorGUILayout.ObjectField(item.animator, typeof(Animator), true);
                                    animType = "新版";
                                    item.animation = null;
                                }
                                break;
                        }
                        GUILayout.Label($"动画类型：{animType}");

                        item.type = (AnimType)EditorGUILayout.EnumPopup(item.type, GUILayout.Width(180));
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.Space(10);

                        animClipsName.Clear();

                        switch (item.type)
                        {
                            case AnimType.None:
                                item.stateIndex = 0;
                                item.currentStateIndex = -1;
                                break;
                            case AnimType.Animation:
                                {
                                    if (item.animation != null)
                                    {
                                        foreach (AnimationState clip in item.animation)
                                        {
                                            animClipsName.Add(clip.clip.name);
                                        }
                                    }
                                }
                                break;
                            case AnimType.Animator:
                                {
                                    if (item.animator != null && item.animator.runtimeAnimatorController != null)
                                    {
                                        var animState = item.animator.runtimeAnimatorController;
                                        var animClips = animState.animationClips;

                                        foreach (var clip in animClips)
                                        {
                                            animClipsName.Add(clip.name);
                                        }
                                    }
                                }
                                break;
                        }
                        if (item.type != AnimType.None)
                            item.isActiveNormalAnim = EditorGUILayout.ToggleLeft("是否需要使该状态拥有默认动画", item.isActiveNormalAnim);
                        if (item.isActiveNormalAnim && animClipsName.Count > 0)
                        {
                            item.currentStateIndex = EditorGUILayout.Popup("默认动画选择", item.currentStateIndex, animClipsName.ToArray());

                            if (item.currentStateIndex != -1)
                                item.normalAnimClipName = animClipsName[item.currentStateIndex];
                            if (item.type == AnimType.Animator)
                                item.stateIndex = EditorGUILayout.IntField("默认动画状态图层", item.stateIndex);
                            else
                                item.stateIndex = 0;
                            item.animSpeed = EditorGUILayout.FloatField("默认动画速度", item.animSpeed);
                            item.animLength = EditorGUILayout.FloatField("默认动画长度", item.animLength);
                        }
                        else if (item.isActiveNormalAnim)
                        {
                            switch (item.type)
                            {
                                case AnimType.Animation:
                                    GUILayout.Label("当前没有添加动画剪辑在animation内");
                                    break;
                                case AnimType.Animator:
                                    GUILayout.Label("当前没有在Animator的状态内添加至少一个动画剪辑！");
                                    break;
                            }
                        }
                        else
                        {
                            item.currentStateIndex = -1;
                            item.normalAnimClipName = string.Empty;
                        }
                        EditorGUILayout.Space(10);
                        item.isNextState = EditorGUILayout.Toggle("是否自动进入下一个状态", item.isNextState);

                        if (item.isNextState)
                        {
                            item.nextStateID = EditorGUILayout.IntField("下一个状态的标识", item.nextStateID);
                        }
                        else item.nextStateID = -1;

                        GUILayout.Space(20);
                        if (item.stateBehaviours.Count > 0)
                        {
                            for (int i = 0; i < item.stateBehaviours.Count; i++)
                            {
                                EditorGUILayout.Space(10);
                                EditorGUILayout.BeginHorizontal();
                                item.stateBehaviours[i].IsActive = EditorGUILayout.ToggleLeft(item.stateBehaviours[i].name, item.stateBehaviours[i].IsActive);
                                if (GUILayout.Button("删除脚本"))
                                {
                                    item.stateBehaviours.RemoveAt(i);
                                    continue;
                                }
                                EditorGUILayout.EndHorizontal();

                            }
                        }
                        RecomposeBehaviour(item);
                    }

                }
            Repaint();
        }

        /// <summary>
        /// 增删状态
        /// </summary>
        private void RecomposeBehaviour(State state)
        {
            if (!isRecomposeScript)
            {
                if (GUILayout.Button("添加状态脚本"))
                {
                    isRecomposeScript = true;
                    Repaint();
                    AssetDatabase.Refresh();
                }
            }
            else
            {
                Type type = typeof(StateBehaviour);
                Type[] types = Assembly.GetAssembly(type).GetTypes();

                foreach (var infoType in types)
                {
                    if (infoType.BaseType == type)
                    {
                        if (GUILayout.Button(infoType.Name))
                        {
                            StateBehaviour obj = Activator.CreateInstance(infoType) as StateBehaviour;

                            if (state.stateBehaviours.Count > 0)
                            {
                                if (state.stateBehaviours.Find(x => x.GetType() == infoType) != null)
                                {
                                    isRecomposeScript = false;
                                    return;
                                }
                            }

                            obj.name = infoType.ToString();
                            obj.index = state.index;
                            state.AddBehaviours(obj);
                            isRecomposeScript = false;
                            Repaint();
                            AssetDatabase.Refresh();

                        }
                    }
                }

                if (GUILayout.Button("取消"))
                {
                    isRecomposeScript = false;
                }

            }

        }


    }
}
#endif

