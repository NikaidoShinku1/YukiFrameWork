﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif
using UnityEngine;

#if UNITY_EDITOR
namespace YukiFrameWork.States
{
    [CustomEditor(typeof(TransitionInspectorHelper))]
    public class TransitionInspector : UnityEditor.Editor
    {
        private ReorderableList reorderableList;
        private Dictionary<int,ReorderableList> reorderableLists = new Dictionary<int,ReorderableList>();
        private Rect condition_left_rect;
        private Rect condition_right_rect;
        private Rect popRect;

        private GUIContent tempContent = new GUIContent();

        private static Dictionary<ParameterType, ConditionInspector> conditionInspectorDict = new Dictionary<ParameterType, ConditionInspector>();
        private static void InitConditionInspectors()
        {
            if (conditionInspectorDict.Count == 0)
            {
                conditionInspectorDict.Add(ParameterType.Bool, new StateBoolCondition());
                conditionInspectorDict.Add(ParameterType.Trigger, new StateTriggerCondition());
                conditionInspectorDict.Add(ParameterType.Int, new StateIntCondition());
                conditionInspectorDict.Add(ParameterType.Float, new StateFloatCondition());
            }
        }
        private void OnEnable()
        {
            TransitionInspectorHelper helper = target as TransitionInspectorHelper;
            if (helper == null) return;

            reorderableList = new ReorderableList(helper.transition.conditions, typeof(StateConditionData), true, true, true, true);
            reorderableList.drawHeaderCallback += DrawHeaderCallBack;
            reorderableList.onAddCallback += OnAddCallBack;
            reorderableList.onRemoveCallback += OnRemoveCallBack;
            reorderableList.drawElementCallback += DrawItem;          
        }

        private void DrawHeaderCallBack(Rect rect)
        {
            EditorGUI.LabelField(rect, new GUIContent("参数条件设置"));          
        }

        public override void OnInspectorGUI()
        {
            TransitionInspectorHelper helper = target as TransitionInspectorHelper;
            if (helper == null) return;

            reorderableList.list = helper.transition.conditions;
            //绘制类似动画状态机的参数设置
            reorderableList.DoLayoutList();

            EditorGUILayout.Space(20);

            if (helper.transition.conditionDatas.Count > 0)
            {
                EditorGUILayout.HelpBox("当具备多组条件时，只要满足任意一组条件即可自动切换!",MessageType.Info);
            }

            for (int i = 0; i < helper.transition.conditionDatas.Count; i++)
            {
                GroupConditionDatas condition = helper.transition.conditionDatas[i];
                if (condition == null) continue;
                ReorderableList list = GetReorderableList(condition);
                if (list != null)
                {
                    list.list = condition.conditions;
                    list.DoLayoutList();
                    GUILayout.Space(10);
                }
            }

            if (GUILayout.Button("添加新的条件集合", GUILayout.Height(30)))
            {
                helper.transition.conditionDatas.Add(new GroupConditionDatas());
                helper.stateMechine.Save();
            }
        }

        public ReorderableList GetReorderableList(GroupConditionDatas condition)
        {
            if (condition == null)
                return null;

            TransitionInspectorHelper helper = target as TransitionInspectorHelper;
            if (helper == null)
                return null;

            int key = condition.GetHashCode();
            if (!reorderableLists.ContainsKey(key))
            {

                ReorderableList list = new ReorderableList(condition.conditions, typeof(StateConditionData), true, true, true, true);
                list.drawHeaderCallback += (rect) => {

                    GUI.Label(rect, "Conditions");

                    rect.Set(rect.width, rect.y + 1, 25, 25);

                    if (GUI.Button(rect, EditorGUIUtility.IconContent("d__Menu"), "IconButton"))
                    {
                        ShowConditionMenu(condition);
                    }


                };
                list.onAddCallback += (a) => {
                    StateConditionData data = StateConditionDataFactory.CreateCondition(helper.stateMechine);
                    condition.conditions.Add(data);
                    helper.stateMechine.Save();
                };

                list.onRemoveCallback += (a) =>
                {
                    if (a.index >= 0 && a.index < condition.conditions.Count)
                        condition.conditions.RemoveAt(a.index);
                    helper.stateMechine.Save();
                };

                list.drawElementCallback += (rect, index, c, d) =>
                {
                    DrawItemExcute(rect, condition.conditions[index]);
                };
                reorderableLists.Add(key, list);
            }

            return reorderableLists[key];
        }

        private void ShowConditionMenu(GroupConditionDatas condition)
        {
            TransitionInspectorHelper helper = target as TransitionInspectorHelper;
            if (helper == null) { return; }

            var genricMenu = new GenericMenu();

            genricMenu.AddItem(new GUIContent("Remove"), false, () =>
            {
                // 移除一组条件
                helper.transition.conditionDatas.Remove(condition);
                helper.stateMechine.Save();
            });

            genricMenu.ShowAsContext();
        }

        protected override void OnHeaderGUI()
        {
            TransitionInspectorHelper helper = target as TransitionInspectorHelper;
            if (helper == null) return;

            GUILayout.BeginHorizontal();

            GUILayout.Label(EditorGUIUtility.IconContent("icons/processed/unityeditor/animations/animatorstate icon.asset"), GUILayout.Width(30), GUILayout.Height(30));
            GUILayout.Label($"{helper.transition.fromStateName} -> {helper.transition.toStateName}");

            GUILayout.EndHorizontal();

            //画一条分割线

            var rect = EditorGUILayout.BeginHorizontal();

            Handles.color = Color.black;
            Handles.DrawLine(new Vector2(rect.x, rect.y), new Vector2(rect.x + rect.width, rect.y));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }

      
        private void OnAddCallBack(ReorderableList list)
        {
            TransitionInspectorHelper helper = target as TransitionInspectorHelper;
            if (helper == null) return;

            StateConditionDataFactory.CreateCondition(helper.stateMechine, helper.transition);
        }
        private void OnRemoveCallBack(ReorderableList list)
        {
            TransitionInspectorHelper helper = target as TransitionInspectorHelper;
            if (helper == null) return;

            StateConditionDataFactory.RemoveCondition(helper.stateMechine, helper.transition, list.index);
        }        

        private void DrawItem(Rect rect, int index, bool isActive, bool isFocused)
        {
            TransitionInspectorHelper helper = target as TransitionInspectorHelper;
            if (helper == null) return;

            var condition = helper.transition.conditions[index];

            condition_left_rect.Set(rect.x, rect.y, rect.width / 2, rect.height);
            condition_right_rect.Set(rect.x + rect.width / 2, rect.y, rect.width / 2, rect.height);

            if (helper.stateMechine.parameters.Count > 0)
            {
                tempContent.text = condition.parameterName;
                if (EditorGUI.DropdownButton(condition_left_rect, tempContent, FocusType.Keyboard))
                {
                    popRect.Set(rect.x, rect.y + 2, rect.width / 2, rect.height);
                    PopupWindow.Show(popRect,new StateSelectParamEditor(rect.width / 2,condition,helper.stateMechine));
                }
            }

            InitConditionInspectors();

            var parameter = helper.stateMechine.parameters.Where(x => x.name.Equals(condition.parameterName)).FirstOrDefault();
            if (parameter == null)
            {
                EditorGUI.LabelField(condition_right_rect, "缺少参数！");
            }
            else
            {
                //根据不同的参数类型绘制不同的内容

                if (conditionInspectorDict.ContainsKey(parameter.parameterType))
                {
                    conditionInspectorDict[parameter.parameterType].OnGUI(condition_right_rect, condition, helper.stateMechine);
                }
                else
                {
                    Debug.LogError(string.Format("未查询到对应绘制方式！-> {0}",parameter.parameterType));
                }
            }
        }

        private void DrawItemExcute(Rect rect, StateConditionData condition)
        {
            TransitionInspectorHelper helper = target as TransitionInspectorHelper;
            if (helper == null) return;

            condition_left_rect.Set(rect.x, rect.y, rect.width / 2, rect.height);
            condition_right_rect.Set(rect.x + rect.width / 2, rect.y, rect.width / 2, rect.height);

            if (helper.stateMechine.parameters.Count > 0)
            {
                //tempContent.text = conditon.parameterName;
                if (EditorGUI.DropdownButton(condition_left_rect, new GUIContent(condition.parameterName), FocusType.Keyboard))
                {
                    // 弹出选择参数的弹框 TODO
                    popRect.Set(rect.x, rect.y + 2, rect.width / 2, rect.height);
                    PopupWindow.Show(popRect, new StateSelectParamEditor(rect.width / 2, condition, helper.stateMechine));
                }
            }
            InitConditionInspectors();

            StateParameterData parameter = helper.stateMechine.parameters.Where(x => x.name.Equals(condition.parameterName)).FirstOrDefault();

            if (parameter == null)
            {
                EditorGUI.LabelField(condition_right_rect, "缺少参数!");
            }
            else
            {

                // 根据不同的参数类型绘制不同的内容
                if (conditionInspectorDict.ContainsKey(parameter.parameterType))
                    // 进行绘制
                    if (conditionInspectorDict[parameter.parameterType] != null)
                        conditionInspectorDict[parameter.parameterType].OnGUI(condition_right_rect, condition, helper.stateMechine);
                    else
                        Debug.LogErrorFormat("未查询到对应的绘制方式:{0}", parameter.parameterType);
            }

        }
    }
}
#endif