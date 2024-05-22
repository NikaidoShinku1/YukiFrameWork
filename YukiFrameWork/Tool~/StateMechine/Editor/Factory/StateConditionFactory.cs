using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
namespace YukiFrameWork.States
{
    public class StateConditionDataFactory 
    {
        public static void CreateCondition(StateMechine stateMechine,StateTransitionData transition)
        {
            StateConditionData condition = new StateConditionData();

            string parameterName = string.Empty;

            StateParameterData parameter = null;

            if (stateMechine.parameters.Count > 0)
            {
                parameter = stateMechine.parameters[0];
                parameterName = parameter.name;
            }

            if (parameter != null)
            {
                switch (parameter.parameterType)
                {
                    case ParameterType.Float:
                        condition.compareType = CompareType.Greater;
                        break;
                    case ParameterType.Int:
                        condition.compareType = CompareType.Greater;
                        break;
                    case ParameterType.Bool:
                        condition.compareType = CompareType.Equal;
                        break;                 
                }
            }
            else
            {
                condition.compareType = CompareType.Greater;
            }
          
            condition.targetValue = 0;
            condition.parameterName = parameterName;

            transition.conditions.Add(condition);

            EditorUtility.SetDirty(stateMechine);
            AssetDatabase.SaveAssets();
        }

        public static StateConditionData CreateCondition(StateMechine stateMechine)
        {
            StateConditionData condition = new StateConditionData();

            string parameterName = string.Empty;

            StateParameterData parameter = null;

            if (stateMechine.parameters.Count > 0)
            {
                parameter = stateMechine.parameters[0];
                parameterName = parameter.name;
            }

            if (parameter != null)
            {
                switch (parameter.parameterType)
                {
                    case ParameterType.Float:
                        condition.compareType = CompareType.Greater;
                        break;
                    case ParameterType.Int:
                        condition.compareType = CompareType.Greater;
                        break;
                    case ParameterType.Bool:
                        condition.compareType = CompareType.Equal;
                        break;                  
                }
            }
            else
            {
                condition.compareType = CompareType.Greater;
            }

            condition.targetValue = 0;
            condition.parameterName = parameterName;

            EditorUtility.SetDirty(stateMechine);
            AssetDatabase.SaveAssets();

            return condition;
        }

        public static void RemoveCondition(StateMechine stateMechine, StateTransitionData transition, int index)
        {
            if (index <= 0 || index >= transition.conditions.Count)
                return;
            transition.conditions.RemoveAt(index);
            EditorUtility.SetDirty(stateMechine);
            AssetDatabase.SaveAssets();
        }
 
    }
}
#endif