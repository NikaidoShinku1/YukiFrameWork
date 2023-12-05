using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using UnityEditor;

namespace YukiFrameWork.States
{
    [System.Serializable]
    public class StateMechine : MonoBehaviour
    {       
        public List<StateBase> states = new List<StateBase>();

        public List<StateParameterData> parameters = new List<StateParameterData>();

        public List<StateTransitionData> transitions = new List<StateTransitionData>();


#if UNITY_EDITOR
        public void SaveToMechine()
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
#endif
    }
}
