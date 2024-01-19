using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
namespace YukiFrameWork.States
{
    public class TransitionInspectorHelper : ScriptableObjectSingleton<TransitionInspectorHelper>
    {
        public StateTransitionData transition;

        public StateMechine stateMechine;

        public void Inspect(StateMechine stateMechine, StateTransitionData transition)
        {
            this.transition = transition;
            this.stateMechine = stateMechine;
            Selection.activeObject = this;
        }
    }
}
#endif