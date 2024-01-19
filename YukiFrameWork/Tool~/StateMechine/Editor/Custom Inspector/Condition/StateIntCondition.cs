using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
namespace YukiFrameWork.States
{
    public class StateIntCondition : ConditionInspector
    {
        private Rect leftRect;
        private Rect rightRect;

        public override void OnGUI(Rect rect, StateConditionData condition, StateMechine stateMechine)
        {
            leftRect.Set(rect.x, rect.y, rect.width / 2, rect.height);
            rightRect.Set(rect.x + rect.width / 2, rect.y, rect.width / 2, rect.height);

        
            if (EditorGUI.DropdownButton(leftRect, new GUIContent(condition.compareType.ToString()), FocusType.Keyboard))
            {
                GenericMenu menu = new GenericMenu();
                for (int i = 0; i < Enum.GetValues(typeof(CompareType)).Length; i++)
                {
                    CompareType type = (CompareType)Enum.GetValues(typeof(CompareType)).GetValue(i);                               

                    menu.AddItem(new GUIContent(type.ToString()), condition.compareType == type, () =>
                    {
                        condition.compareType = type;
                        stateMechine.SaveToMechine();
                    });

                }

                menu.ShowAsContext();
            }

            condition.targetValue = EditorGUI.IntField(rightRect, (int)condition.targetValue);
            EditorUtility.SetDirty(stateMechine);
        }
    }
}
#endif