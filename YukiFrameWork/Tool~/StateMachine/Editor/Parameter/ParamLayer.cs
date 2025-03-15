///=====================================================
/// - FileName:      ParamLayer.cs
/// - NameSpace:     YukiFrameWork.Machine
/// - Description:   高级定制脚本生成
/// - Creation Time: 2025/3/9 16:14:46
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using UnityEditorInternal;
using UnityEditor;
namespace YukiFrameWork.Machine
{
    public class ParamLayer 
    {
        private ReorderableList reorderableList;
        private ReorderableList reorderableListStates;
        private Vector2 scrollView;

        private bool isRenaming = false;
        private string newName;
        private List<StateParameterData> EmptyList = new List<StateParameterData>();

        private Rect headerRect = new Rect();

        private string[] toolbars = new string[] { "状态机集合", "参数列表" };
        private int select = 1;

        private int index = 0;


        protected Rect position;

        public EditorWindow EditorWindow { get; private set; }

        public ParamLayer(EditorWindow editorWindow)
        {
            this.EditorWindow = editorWindow;
        }
        public void OnGUI(Rect rect)
        {
            rect.Set(rect.x + 2, rect.y, rect.width, rect.height);

            headerRect.Set(rect.x, rect.y, rect.width, 20);

            //EditorGUI.DrawRect(headerRect, ColorConst.ParaBackground);

            headerRect.Set(headerRect.x + 5, headerRect.y, 150, headerRect.height);
            select = GUI.Toolbar(headerRect, select, toolbars, "toolbarbuttonLeft");

            //Handles.color = Color.black;
            //Handles.DrawLine(new Vector2(rect.x, rect.y + headerRect.height), new Vector2(rect.x + rect.width, rect.y + headerRect.height));
            //Handles.DrawLine(new Vector2(rect.x + rect.width, rect.y), new Vector2(rect.x + rect.width, rect.y + rect.height));
            // 偏移20, 用来绘制 States 和 Parmaters
            rect.Set(rect.x, rect.y + 20, rect.width, rect.height);

            //EditorGUI.DrawRect(rect, ColorConst.ParaBackground);
            //GUI.Box(rect, string.Empty, GUI.skin.GetStyle("CN Box"));

            if (select == 1)
            {
                DrawParamters(rect);
            }
            else
            {
                DrawStates(rect);
            }


        }

        private void DrawParamters(Rect rect)
        {
            if (reorderableList == null)
            {
                if (Global.Instance.RuntimeStateMachineCore != null)
                {
                    reorderableList = new ReorderableList(Global.Instance.RuntimeStateMachineCore.all_runtime_parameters, typeof(StateParameterData), true, true, true, true);
                }
                else
                {
                    reorderableList = new ReorderableList(EmptyList, typeof(StateParameterData), true, true, true, true);
                }

                reorderableList.drawHeaderCallback += HeaderCallbackDelegate;
                reorderableList.onAddDropdownCallback += OnAddDropdownCallback;
                reorderableList.onRemoveCallback += OnRemoveCallback;
                reorderableList.drawElementCallback += DrawElementCallback;

                reorderableList.onCanRemoveCallback += onCanRemoveCallback;
                reorderableList.onCanAddCallback += onCanRemoveCallback;
            }

            if (Global.Instance.RuntimeStateMachineCore != null)
                reorderableList.list = Global.Instance.RuntimeStateMachineCore.all_runtime_parameters;
            else
                reorderableList.list = EmptyList;

            EditorGUI.BeginDisabledGroup(Global.Instance.RuntimeStateMachineCore == null);

            GUILayout.BeginArea(rect);
            scrollView = GUILayout.BeginScrollView(scrollView);
            reorderableList.DoLayoutList();
            GUILayout.EndScrollView();
            GUILayout.EndArea();

            EditorGUI.EndDisabledGroup();
        }

        private void DrawStates(Rect rect)
        {
            if (reorderableListStates == null)
            {

                reorderableListStates = new ReorderableList(Global.Instance.RuntimeStateMachineCores, typeof(RuntimeStateMachineCore), false, true, false, false);

                reorderableListStates.headerHeight = 0;
                reorderableListStates.drawElementCallback += DrawStateElementCallback;
                reorderableListStates.onSelectCallback += OnStateChanged;
            }

            for (int i = 0; i < Global.Instance.RuntimeStateMachineCores.Count; i++)
            {
                if (Global.Instance.RuntimeStateMachineCores[i] == null)
                {
                    Global.Instance.RuntimeStateMachineCores.RemoveAt(i);
                    i--;
                }
            }

            reorderableListStates.list = Global.Instance.RuntimeStateMachineCores;

            GUILayout.BeginArea(rect);
            scrollView = GUILayout.BeginScrollView(scrollView);
            reorderableListStates.DoLayoutList();
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        public void ProcessEvents()
        {
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 &&
                position.Contains(Event.current.mousePosition)
                )
            {
                Global.Instance.ClearSelections();
            }
        }     

        // 添加
        private void OnAddDropdownCallback(Rect buttonRect, ReorderableList list)
        {
            GenericMenu menu = new GenericMenu();

            for (int i = 0; i < Enum.GetValues(typeof(ParameterType)).Length; i++)
            {
                ParameterType v = (ParameterType)Enum.GetValues(typeof(ParameterType)).GetValue(i);
                menu.AddItem(new GUIContent(v.ToString()), false, () =>
                {
                    StateParameterFactory.CreateParamter(Global.Instance.RuntimeStateMachineCore, v);
                });
            }

            menu.ShowAsContext();
        }

        // 移除 
        private void OnRemoveCallback(ReorderableList list)
        {
            StateParameterFactory.RemoveParamter(Global.Instance.RuntimeStateMachineCore, list.index);
        }

        // 绘制每一条数据
        private void DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (Global.Instance.RuntimeStateMachineCore == null) return;
            if (index < 0 || index >= Global.Instance.RuntimeStateMachineCore.all_runtime_parameters.Count) return;

            StateParameterData parameter = Global.Instance.RuntimeStateMachineCore.all_runtime_parameters[index];

            if (parameter == null) return;

            rect.width *= 0.6f;

            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && rect.Contains(Event.current.mousePosition) && isFocused)
            {
                isRenaming = true;
                this.index = index;
            }

            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 &&
                !rect.Contains(Event.current.mousePosition) && index == reorderableList.index)
            {
                EditorApplication.delayCall += () =>
                {
                    isRenaming = false;
                };

                GUI.FocusControl(null);
            }

            // 按下回车键的时候 也需要取消重命名
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
            {
                EditorApplication.delayCall += () =>
                {
                    isRenaming = false;
                };

            }

            if (isRenaming && index == reorderableList.index)
            {
                // 绘制输入框
                EditorGUI.BeginChangeCheck();
                newName = EditorGUI.DelayedTextField(rect, parameter.parameterName);
                if (EditorGUI.EndChangeCheck() && this.index == index)
                {
                    isRenaming = false;
                    if (parameter.parameterName.Equals(newName)) return;
                    StateParameterFactory.RenameParamter(Global.Instance.RuntimeStateMachineCore, parameter, newName);
                }
            }
            else
            {
                GUI.Label(rect, parameter.parameterName);
            }

            rect.x += rect.width;
            rect.width /= 3;
            EditorGUI.BeginDisabledGroup(true);
            GUI.Label(rect, GetParameterType(parameter));
            EditorGUI.EndDisabledGroup();
            rect.x += rect.width;

            switch (parameter.parameterType)
            {
                case ParameterType.Float:
                    parameter.Parameter.Value = EditorGUI.FloatField(rect, parameter.Parameter.Value);
                    break;
                case ParameterType.Int:
                    parameter.Parameter.Value = EditorGUI.IntField(rect, (int)parameter.Parameter.Value);
                    break;
                case ParameterType.Bool:
                    parameter.Parameter.Value = EditorGUI.Toggle(rect, parameter.Parameter.Value == 1) ? 1 : 0;
                    break;
                case ParameterType.Trigger:
                    parameter.Parameter.Value = EditorGUI.Toggle(rect, parameter.Parameter.Value == 1, GUI.skin.GetStyle("Radio")) ? 1 : 0;
                    break;
            }

        }

        private string GetParameterType(StateParameterData parameter)
        {            
            return parameter.parameterType.ToString();
        }

        private void DrawStateElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (index < 0 || index >= Global.Instance.RuntimeStateMachineCores.Count) return;

            RuntimeStateMachineCore controller = Global.Instance.RuntimeStateMachineCores[index];

            //if (controller == null) return;

            try
            {
                GUIContent content = EditorGUIUtility.IconContent("AnimatorController Icon");

                if (controller == Global.Instance.RuntimeStateMachineCore)
                    GUI.Label(new Rect(rect.x, rect.y, rect.height, rect.height), "✓");

                GUI.Label(new Rect(rect.x + rect.height, rect.y, rect.height, rect.height), content);
                rect.Set(rect.x + rect.height * 2, rect.y, rect.width - rect.height, rect.height);
                GUI.Label(rect, controller == null ? "None" : controller.name);
            }
            catch (Exception)
            {
            }


        }

        private bool onCanRemoveCallback(ReorderableList list)
        {
            return Application.isPlaying == false;
        }


        private void OnStateChanged(ReorderableList list)
        {
            if (Global.Instance.RuntimeStateMachineCores == null || Global.Instance.RuntimeStateMachineCores.Count == 0) return;

            if (Global.Instance.RuntimeStateMachineCore == Global.Instance.RuntimeStateMachineCores[list.index]) return;

            Global.Instance.StateManagerIndex = list.index;
        }


        private void HeaderCallbackDelegate(Rect rect)
        {
            rect.width *= 0.6f;
            rect.x += 15;
            GUI.Label(rect, "Name");
            rect.x += rect.width;
            rect.width /= 3;
            rect.x -= 10;
            GUI.Label(rect, "Type");
            rect.x += rect.width;
            rect.x -= 5;
            GUI.Label(rect, "Value");
        }



    }
}
#endif