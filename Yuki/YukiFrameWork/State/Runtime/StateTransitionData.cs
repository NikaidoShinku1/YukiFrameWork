using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YukiFrameWork.States
{
    public enum TransitionMode
    {
        动画剪辑模式 = 0,
        定时模式,
        有限条件模式
    }
    [Serializable]
    public class StateTransitionData
    {
        public string fromStateName;
        public string toStateName;

        public float stateLoadTime = 0;

        public TransitionMode transitionMode = TransitionMode.有限条件模式;

        public List<StateConditionData> conditions = new List<StateConditionData>();
    }
}
