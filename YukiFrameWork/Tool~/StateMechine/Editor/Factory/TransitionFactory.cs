using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
namespace YukiFrameWork.States
{
    public class TransitionFactory 
    {
        public static void CreateTransition(StateMechine stateMechine, string fromStateName, string toStateName)
        {          
            if (toStateName.Equals(StateConst.entryState))
                return;

            if (toStateName.StartsWith(StateConst.upState)) return;
           
            if (fromStateName.Equals(toStateName))
                return;

            if (!stateMechine.IsSubLayerAndContainsName())
            {
                foreach (var item in stateMechine.transitions)
                {
                    if (item.fromStateName.Equals(fromStateName) && item.toStateName.Equals(toStateName))
                    {
                        string message = $"过渡{fromStateName} -> {toStateName}已经存在！";
                        Debug.LogError(message);
                        StateMechineEditorWindow.OnShowNotification(message);
                        return;
                    }
                }

                StateTransitionData transition = new StateTransitionData()
                {
                    fromStateName = fromStateName,
                    toStateName = toStateName,
                    layerName = stateMechine.layerName
                };

                stateMechine.transitions.Add(transition);
            }
            else
            {
                if (!stateMechine.subTransitions.ContainsKey(stateMechine.layerName))
                {
                    stateMechine.subTransitions.Add(stateMechine.layerName, new List<StateTransitionData>());
                }

                foreach (var item in stateMechine.subTransitions[stateMechine.layerName])
                {
                    if (item.fromStateName.Equals(fromStateName) && item.toStateName.Equals(toStateName))
                    {
                        string message = $"子状态机{stateMechine.layerName} 过渡{fromStateName} -> {toStateName}已经存在！";
                        Debug.LogError(message);
                        StateMechineEditorWindow.OnShowNotification(message);
                        return;
                    }
                }
                StateTransitionData transition = new StateTransitionData()
                {
                    fromStateName = fromStateName,
                    toStateName = toStateName,
                    layerName = stateMechine.layerName
                };
                stateMechine.subTransitions[stateMechine.layerName].Add(transition);
            }
            EditorUtility.SetDirty(stateMechine);
            AssetDatabase.SaveAssets();
        }

        public static void DeleteTransition(StateMechine stateMechine, StateTransitionData transition)
        {        
            stateMechine.transitions.Remove(transition);
            foreach (var item in stateMechine.subTransitions.Values)
            {            
                item.Remove(transition);
            }
            EditorUtility.SetDirty(stateMechine);
            AssetDatabase.SaveAssets();
        }                   
    }
}
#endif