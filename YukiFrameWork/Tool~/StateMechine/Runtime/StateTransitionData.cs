using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YukiFrameWork.States
{   
    [Serializable]
    public class StateTransitionData
    {
        public string fromStateName;
        public string toStateName;
        public string layerName;
        public List<StateConditionData> conditions = new List<StateConditionData>();
    }
}
