﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace YukiFrameWork.States
{
    public class ParamLayer : GraphLayer
    {

        #region 字段
        private ReorderableList reorderableList;

        private Vector2 scrollView;

        //参数区域
        private Rect paramLabelRect;
        private Rect paramValueRect;
        private const float param_Value_Width = 50f;
        private const float param_Value_Space = 2f;

        //是否在重命名
        private bool isRenaming = false;
        private string newName;
        #endregion

        #region 重写方法

        public override void OnGUI(Rect rect)
        {
            base.OnGUI(rect);

            DrawBackGround(rect);

            DrawToParamsData(rect);

        }

        private void DrawBackGround(Rect rect)
        {
            EditorGUI.DrawRect(rect, ColorConst.ParamBackGround);
        }

        public override void ProcessEvents()
        {
            base.ProcessEvents();

            switch (Event.current.type)
            {
                case EventType.MouseDown:
                    {
                        if (Event.current.button == 0 && position.Contains(Event.current.mousePosition))
                        {
                            this.Context.ClearSelections();
                        }
                    }
                    break;
            }
        }

        public override void OnLostFocus()
        {
            base.OnLostFocus();

            isRenaming = false;
        }
        #endregion

        #region 方法

        public ParamLayer(StateMechineEditor editorWindow) : base(editorWindow)
        {
        }

        private void DrawToParamsData(Rect rect)
        {
            GUI.Box(rect, string.Empty, GUI.skin.GetStyle("CN Box"));

            if (this.Context.StateMechine == null) return;

            if (reorderableList == null)
            {
                reorderableList = new ReorderableList(this.Context.StateMechine.parameters, typeof(StateParameterData), true, true, true, true);
                reorderableList.headerHeight = 0;

                reorderableList.onAddDropdownCallback += OnAddDropDownCallBack;

                reorderableList.onRemoveCallback += OnRemoveCallBack;

                reorderableList.drawElementCallback += DrawElementCallback;

                reorderableList.onCanRemoveCallback += OnCanRemoveCallback;

                reorderableList.onCanAddCallback += OnCanRemoveCallback;
            }

            reorderableList.list = this.Context.StateMechine.parameters;

            GUILayout.BeginArea(rect);
            scrollView = GUILayout.BeginScrollView(scrollView);
            //绘制
            reorderableList.DoLayoutList();
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private void DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (index < 0 || index >= this.Context.StateMechine.parameters.Count) return;

            StateParameterData parameterData = this.Context.StateMechine.parameters[index];

            if (parameterData == null) return;

            paramLabelRect.Set(rect.x, rect.y, rect.width - param_Value_Width, rect.height);
            paramValueRect.Set(rect.x + rect.width - param_Value_Width + param_Value_Space, rect.y + param_Value_Space, param_Value_Width - param_Value_Space, rect.height - param_Value_Space);

            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && paramLabelRect.Contains(Event.current.mousePosition)
                && isFocused)
            {
                isRenaming = true;
            }

            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && !paramLabelRect.Contains(Event.current.mousePosition)
                && index == reorderableList.index)
            {
                isRenaming = false;
            }

            //按下回车也保存重命名
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
            {
                EditorApplication.delayCall += () => isRenaming = false;
            }

            if (isRenaming && index == reorderableList.index)
            {
                EditorGUI.BeginChangeCheck();
                {
                    newName = EditorGUI.DelayedTextField(paramLabelRect, parameterData.name);
                }

                if (EditorGUI.EndChangeCheck())
                {
                    isRenaming = false;                  
                    this.Context.StateMechine.RenameParamter(parameterData,newName);
                }
            }

            else
                GUI.Label(paramLabelRect, parameterData.name);

            switch (parameterData.parameterType)
            {
                case ParameterType.Float:
                    parameterData.Value = EditorGUI.FloatField(paramValueRect, parameterData.Value);
                    break;
                case ParameterType.Int:
                    parameterData.Value = EditorGUI.IntField(paramValueRect, (int)parameterData.Value);
                    break;
                case ParameterType.Bool:
                    parameterData.Value = EditorGUI.Toggle(paramValueRect, parameterData.Value == 1) ? 1 : 0;
                    break;           
            }
        }

        private void OnAddDropDownCallBack(Rect buttonRect, ReorderableList list)
        {
            GenericMenu menu = new GenericMenu();

            var paramterTypeValues = System.Enum.GetValues(typeof(ParameterType));

            for (int i = 0; i < paramterTypeValues.Length; i++)
            {
                ParameterType v = (ParameterType)paramterTypeValues.GetValue(i);
                menu.AddItem(new GUIContent(v.ToString()), false, () => 
                {
                    this.Context.StateMechine.CreateParamter(v);

                });

                menu.ShowAsContext();
            }
        }

        private void OnRemoveCallBack(ReorderableList list)
        {
            this.Context.StateMechine?.RemoveParamter(list.index);
        }

        /// <summary>
        /// 非运行才可以增删条件判断
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private bool OnCanRemoveCallback(ReorderableList list)
        {
            return !Application.isPlaying;
        }      

        #endregion


    }
}
