///=====================================================
/// - FileName:      StateTransitionInspector.cs
/// - NameSpace:     YukiFrameWork.Machine
/// - Description:   高级定制脚本生成
/// - Creation Time: 2025/3/13 14:56:25
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;





#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
namespace YukiFrameWork.Machine
{
    [CustomEditor(typeof(StateTransitionInspectorHelper))]
    public class StateTransitionInspector : Editor
    {
        private ReorderableList reorderableList;

        private Rect condition_left_rect;
        private Rect condition_right_rect;
        private Rect popRect;

        private GUIContent add_a_sets_of_conditions = null;

        private GUIContent auto_switch = null;

        private static Dictionary<ParameterType, StateConditionInspector> conditionInspector = new Dictionary<ParameterType, StateConditionInspector>();

        private Dictionary<int, ReorderableList> reorderables = new Dictionary<int, ReorderableList>();

        private bool autoSwitch;

        private void OnEnable()
        {
            StateTransitionInspectorHelper helper = target as StateTransitionInspectorHelper;
            if (helper == null) { return; }

            reorderableList = new ReorderableList(helper.transition.conditions, typeof(StateConditionData), true, true, true, true);
            reorderableList.drawHeaderCallback += OnDrawHeaderCallback;
            reorderableList.onAddCallback += this.OnAddCallback;
            reorderableList.onRemoveCallback += this.OnRemoveCallback;
            reorderableList.drawElementCallback += this.DrawItem;

            autoSwitch = helper.transition.autoSwitch;
        }

        public override void OnInspectorGUI()
        {
            StateTransitionInspectorHelper helper = target as StateTransitionInspectorHelper;
            if (helper == null) { return; }

            EditorGUI.BeginDisabledGroup(EditorApplication.isPlaying);

            if (auto_switch == null)
                auto_switch = new GUIContent("AutoSwitch");

            EditorGUI.BeginDisabledGroup(!helper.transition.Empty);
            Rect rect = GUILayoutUtility.GetRect(0f, 20, GUILayout.ExpandWidth(expand: true));
            EditorGUI.HelpBox(rect, "当条件为空时,是否自动切换?当前过渡的条件为空时可用!", MessageType.Info);
            rect.Set(rect.x, rect.y + 20, rect.width, rect.height);
            GUI.Label(rect, auto_switch);
         
            rect.Set(rect.width - 20, rect.y, 20, 20);
            
            if (helper.transition.Empty)
            {             
                helper.transition.autoSwitch = GUI.Toggle(rect, helper.transition.autoSwitch, string.Empty);
            }
            else
            {
                GUI.Toggle(rect, false, string.Empty);
            }

            if (autoSwitch != helper.transition.autoSwitch)
            {
                helper.runtimeStateMachineCore.Save();
                autoSwitch = helper.transition.autoSwitch;
            }
            GUILayout.Space(30);
            EditorGUI.EndDisabledGroup();

            reorderableList.list = helper.transition.conditions;
            reorderableList.DoLayoutList();

            GUILayout.Space(10);

            for (int i = 0; i < helper.transition.conditionGroups.Count; i++)
            {
                var condition = helper.transition.conditionGroups[i];
                if (condition == null) continue;
                ReorderableList list = GetReorderableList(condition);
                if (list != null)
                {
                    list.list = condition;
                    list.DoLayoutList();
                    GUILayout.Space(10);
                }
            }




            if (add_a_sets_of_conditions == null)
                add_a_sets_of_conditions = new GUIContent("添加一组新的条件");

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (helper.transition.conditionGroups.Count > 0)
            {
                GUILayout.Label("注:当有多组条件时,其中一组满足,状态就会切换!", "CN StatusWarn");
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(add_a_sets_of_conditions, GUILayout.Width(260), GUILayout.Height(25)))
            {
                helper.transition.conditionGroups.Add(new List<StateConditionData>());
                helper.runtimeStateMachineCore.Save();
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            EditorGUI.EndDisabledGroup();

        }

        protected override void OnHeaderGUI()
        {
            //base.OnHeaderGUI();
            StateTransitionInspectorHelper helper = target as StateTransitionInspectorHelper;
            if (helper == null) { return; }

            GUILayout.BeginHorizontal();

            GUILayout.Label(EditorGUIUtility.IconContent("icons/processed/unityeditor/animations/animatorstatetransition icon.asset"), GUILayout.Width(30), GUILayout.Height(30));
            GUILayout.Label(string.Format("{0} -> {1}", helper.transition.fromStateName, helper.transition.toStateName));
            GUILayout.EndHorizontal();

            // 画一条 分隔的线 
            var rect = EditorGUILayout.BeginHorizontal();

            Handles.color = Color.black;
            Handles.DrawLine(new Vector2(rect.x, rect.y), new Vector3(rect.x + rect.width, rect.y));

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

        }


        private static void InitConditionInspectors()
        {

            if (conditionInspector.Count == 0)
            {
                conditionInspector.Add(ParameterType.Bool, new StateBoolConditionInspector());
                conditionInspector.Add(ParameterType.Float, new StateFloatConditionInspector());
                conditionInspector.Add(ParameterType.Int, new StateIntConditionInspector());
                conditionInspector.Add(ParameterType.Trigger, new StateConditionInspector()); // 不需要做任何绘制 
            }

        }

        private void OnAddCallback(ReorderableList list)
        {

            StateTransitionInspectorHelper helper = target as StateTransitionInspectorHelper;
            if (helper == null) { return; }
            //Debug.Log("添加条件");
            StateConditionFactory.CreateCondition(helper.runtimeStateMachineCore, helper.transition);
        }

        private void OnRemoveCallback(ReorderableList list)
        {

            StateTransitionInspectorHelper helper = target as StateTransitionInspectorHelper;
            if (helper == null) return;

            StateConditionFactory.DeleteCondition(helper.runtimeStateMachineCore, helper.transition, list.index);
        }

        private void DrawItem(Rect rect, int index, bool isActive, bool isFocused)
        {

            StateTransitionInspectorHelper helper = target as StateTransitionInspectorHelper;
            if (helper == null) { return; }

            var conditon = helper.transition.conditions[index];

            DrawItemExcute(rect, conditon);
        }

        private void DrawItemExcute(Rect rect, StateConditionData condition)
        {
            StateTransitionInspectorHelper helper = target as StateTransitionInspectorHelper;
            if (helper == null) return;

            condition_left_rect.Set(rect.x, rect.y, rect.width / 2, rect.height);
            condition_right_rect.Set(rect.x + rect.width / 2, rect.y, rect.width / 2, rect.height);

            if (helper.runtimeStateMachineCore.all_runtime_parameters.Count > 0)
            {
                //tempContent.text = conditon.parameterName;
                if (EditorGUI.DropdownButton(condition_left_rect, new GUIContent(condition.parameterName), FocusType.Keyboard))
                {
                    // 弹出选择参数的弹框 TODO
                    popRect.Set(rect.x, rect.y + 2, rect.width / 2, rect.height);
                    PopupWindow.Show(popRect, new StateSelectParamWindow(rect.width / 2, condition, helper.runtimeStateMachineCore));
                }
            }

            InitConditionInspectors();

            StateParameterData parameter = helper.runtimeStateMachineCore.GetParameterData(condition.parameterName);

            if (parameter == null)
            {
                EditorGUI.LabelField(condition_right_rect, "缺少参数!");
            }
            else
            {

                // 根据不同的参数类型绘制不同的内容
                if (conditionInspector.ContainsKey(parameter.parameterType))
                    // 进行绘制
                    if (conditionInspector[parameter.parameterType] != null)
                        conditionInspector[parameter.parameterType].OnGUI(condition_right_rect, condition, helper.runtimeStateMachineCore);
                    else
                        Debug.LogErrorFormat("未查询到对应的绘制方式:{0}", parameter.parameterType);
            }

        }

        private void OnDrawHeaderCallback(Rect rect)
        {
            GUI.Label(rect, "条件集合");
        }

        public ReorderableList GetReorderableList(List<StateConditionData> conditions)
        {
            if (conditions == null)
                return null;

            StateTransitionInspectorHelper helper = target as StateTransitionInspectorHelper;
            if (helper == null)
                return null;

            int key = conditions.GetHashCode();
            if (!reorderables.ContainsKey(key))
            {

                ReorderableList list = new ReorderableList(conditions, typeof(StateConditionData), true, true, true, true);
                list.drawHeaderCallback += (rect) => {

                    GUI.Label(rect, "Conditions");

                    rect.Set(rect.width - 10, rect.y + 1, 25, 25);

                    if (GUI.Button(rect, EditorGUIUtility.IconContent("d__Menu"), "IconButton"))
                    {
                        ShowConditionMenu(conditions);
                    }


                };
                list.onAddCallback += (a) => {
                    StateConditionData data = StateConditionFactory.CreateCondition(helper.runtimeStateMachineCore);
                    conditions.Add(data);
                    helper.runtimeStateMachineCore.Save();
                };

                list.onRemoveCallback += (a) =>
                {
                    if (a.index >= 0 && a.index < conditions.Count)
                        conditions.RemoveAt(a.index);
                    helper.runtimeStateMachineCore.Save();
                };

                list.drawElementCallback += (rect, index, c, d) =>
                {
                    DrawItemExcute(rect, conditions[index]);
                };
                reorderables.Add(key, list);
            }

            return reorderables[key];
        }


        private void ShowConditionMenu(List<StateConditionData> condition)
        {
            StateTransitionInspectorHelper helper = target as StateTransitionInspectorHelper;
            if (helper == null) { return; }

            var genricMenu = new GenericMenu();

            genricMenu.AddItem(new GUIContent("移除"), false, () =>
            {
                // 移除一组条件
                helper.transition.conditionGroups.Remove(condition);
                helper.runtimeStateMachineCore.Save();
            });

            genricMenu.ShowAsContext();
        }

    }


}

#endif