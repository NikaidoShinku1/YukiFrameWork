using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
namespace YukiFrameWork.States
{
    public class StateBoolCondition : ConditionInspector
    {       
        public override void OnGUI(Rect rect, StateConditionData condition, StateMechine stateMechine)
        {
            if (EditorGUI.DropdownButton(rect, new GUIContent(condition.targetValue == 1 ? "True" : "False"), FocusType.Keyboard))
            {
                GenericMenu menu = new GenericMenu();
            
                menu.AddItem(new GUIContent("True"), condition.targetValue == 1, () =>
                {
                    condition.targetValue = 1;
                    stateMechine.SaveToMechine();
                });              

                menu.AddItem(new GUIContent("False"), condition.targetValue == 0, () => 
                {
                    condition.targetValue = 0;
                    stateMechine.SaveToMechine();
                });

                menu.ShowAsContext();
            }
        }
    }

    public class StateTriggerCondition : ConditionInspector
    {
        public override void OnGUI(Rect rect, StateConditionData condition, StateMechine stateMechine)
        {
            
        }
    }
}
#endif