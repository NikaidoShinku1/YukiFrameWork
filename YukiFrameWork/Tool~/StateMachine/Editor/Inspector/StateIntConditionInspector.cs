///=====================================================
/// - FileName:      StateIntConditionInspector.cs
/// - NameSpace:     YukiFrameWork.Machine
/// - Description:   高级定制脚本生成
/// - Creation Time: 2025/3/13 14:16:09
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
    public class StateIntConditionInspector : StateConditionInspector 
    {
        private Rect leftRect;
        private Rect rightRect;
        public override void OnGUI(Rect rect, StateConditionData condition, RuntimeStateMachineCore runtimeStateMachineCore)
        {        
            leftRect.Set(rect.x, rect.y, rect.width / 2, rect.height);
            rightRect.Set(rect.x + rect.width / 2, rect.y, rect.width / 2, rect.height);

            //tempContent.text = condition.compareType.ToString();
            if (EditorGUI.DropdownButton(leftRect, new GUIContent(condition.compareType.ToString()), FocusType.Keyboard))
            {

                GenericMenu menu = new GenericMenu();

                for (int i = 0; i < Enum.GetValues(typeof(CompareType)).Length; i++)
                {
                    CompareType v = (CompareType)Enum.GetValues(typeof(CompareType)).GetValue(i);

                    //if (v == CompareType.Equal || v == CompareType.NotEqual) continue;

                    //tempContent.text = v.ToString();
                    menu.AddItem(new GUIContent(v.ToString()), condition.compareType == v, () => {
                        condition.compareType = v;
                        runtimeStateMachineCore.Save();
                    });

                }

                menu.ShowAsContext();

            }

            condition.targetValue = EditorGUI.IntField(rightRect, (int)condition.targetValue);
            EditorUtility.SetDirty(runtimeStateMachineCore);
        }

    }
}
#endif