///=====================================================
/// - FileName:      StateBoolConditionInspector.cs
/// - NameSpace:     YukiFrameWork.Machine
/// - Description:   高级定制脚本生成
/// - Creation Time: 2025/3/13 14:15:53
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
    public class StateBoolConditionInspector : StateConditionInspector
    {
        public override void OnGUI(Rect rect, StateConditionData condition, RuntimeStateMachineCore runtimeStateMachineCore)
        {
          
            string text = condition.targetValue == 1 ? "True" : "False";
            if (EditorGUI.DropdownButton(rect, new GUIContent(text), FocusType.Keyboard))
            {

                GenericMenu menu = new GenericMenu();

                menu.AddItem(new GUIContent("True"), condition.targetValue == 1, () => {
                    condition.targetValue = 1;
                    runtimeStateMachineCore.Save();
                });

                //tempContent.text = "False";
                menu.AddItem(new GUIContent("False"), condition.targetValue == 0, () => {
                    condition.targetValue = 0;
                    runtimeStateMachineCore.Save();
                });

                menu.ShowAsContext();
            }
        }


    }
}
#endif