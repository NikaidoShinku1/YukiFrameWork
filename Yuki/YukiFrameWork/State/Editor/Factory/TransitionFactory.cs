using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace YukiFrameWork.States
{
    public class TransitionFactory 
    {
        public static void CreateTransition(StateMechine stateMechine, string fromStateName, string toStateName)
        {          
            if (toStateName.Equals(StateConst.entryState))
                return;
           
            if (fromStateName.Equals(toStateName))
                return;
           
            foreach (var item in stateMechine.transitions)
            {
                if (item.fromStateName.Equals(fromStateName) && item.toStateName.Equals(toStateName))
                {
                    Debug.LogError($"过渡{fromStateName} -> {toStateName}已经存在！");
                    return;
                }             
            }

            StateTransitionData transition = new StateTransitionData()
            {
                fromStateName = fromStateName,
                toStateName = toStateName,
            };

            stateMechine.transitions.Add(transition);
            EditorUtility.SetDirty(stateMechine);
            AssetDatabase.SaveAssets();
        }

        public static void DeleteTransition(StateMechine stateMechine, StateTransitionData transition)
        {
          
            stateMechine.transitions.Remove(transition);
            EditorUtility.SetDirty(stateMechine);
            AssetDatabase.SaveAssets();
        }                   
    }
}