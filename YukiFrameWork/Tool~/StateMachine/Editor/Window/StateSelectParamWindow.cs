///=====================================================
/// - FileName:      StateSelectParamWindow.cs
/// - NameSpace:     YukiFrameWork.Machine
/// - Description:   高级定制脚本生成
/// - Creation Time: 2025/3/13 19:29:42
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;


#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.IMGUI.Controls;
namespace YukiFrameWork.Machine
{
    public class StateSelectParamWindow : PopupWindowContent
    {

        #region 字段

        private float width;
        private StateConditionData condition;
        private RuntimeStateMachineCore runtimeStateMachineCore;

        // 搜索框
        private SearchField searchField;
        private Rect searchRect;
        const float searchHeight = 25f;

        // 标签 
        private Rect labelRect;
        const float labelHeight = 30f;

        // 参数列表
        private StateSelectParamListTree paramTree;
        private TreeViewState paramState;
        private Rect paramRect;

        #endregion


        public StateSelectParamWindow(float width, StateConditionData condition, RuntimeStateMachineCore runtimeStateMachineCore)
        {
            this.width = width;
            this.condition = condition;
            this.runtimeStateMachineCore = runtimeStateMachineCore;
        }


        public override Vector2 GetWindowSize()
        {
            return new Vector2(this.width, 120);
        }

        public override void OnGUI(Rect rect)
        {

            if (paramTree == null)
            {
                if (paramState == null)
                {
                    paramState = new TreeViewState();
                }

                paramTree = new StateSelectParamListTree(paramState, runtimeStateMachineCore, condition);
                paramTree.Reload();
            }

            // 搜索框
            if (searchField == null)
            {
                searchField = new SearchField();
            }
            searchRect.Set(rect.x + 5, rect.y + 5, rect.width - 10, searchHeight);
            paramTree.searchString = searchField.OnGUI(searchRect, paramTree.searchString);

            // 标签
            labelRect.Set(rect.x, rect.y + searchHeight, rect.width, labelHeight);
            EditorGUI.LabelField(labelRect, condition.parameterName, GUI.skin.GetStyle("AC BoldHeader"));

            // 参数列表 


            paramRect.Set(rect.x, rect.y + searchHeight + labelHeight - 5, rect.width, rect.height - searchHeight - labelHeight);
            paramTree.OnGUI(paramRect);

        }



    }
}
#endif