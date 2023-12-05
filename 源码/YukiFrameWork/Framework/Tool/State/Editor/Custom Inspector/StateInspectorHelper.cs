using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace YukiFrameWork.States
{
    public class StateInspectorHelper : ScriptableObjectSingleton<StateInspectorHelper>
    {     
        public StateBase node;
        public StateMechine StateMechine;

        public void Inspect(StateMechine stateMechine, StateBase state)
        {
            this.node = state;
            this.StateMechine = stateMechine;
            Selection.activeObject = this;
        }
    }
}
