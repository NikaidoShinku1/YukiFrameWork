using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
namespace YukiFrameWork.States
{
    public class StateInspectorHelper : ScriptableObjectSingleton<StateInspectorHelper>
    {     
        public StateBase node;
        public StateMechine StateMechine;

        internal bool IsFileInit;
        public void Inspect(StateMechine stateMechine, StateBase state)
        {
            this.node = state;           
            this.StateMechine = stateMechine;
            Selection.activeObject = this;
            IsFileInit = false;
        }
    }
}
#endif