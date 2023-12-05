using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YukiFrameWork.States
{
    public enum CompareType
    {
        Greater = 0,
        Less,
        Equal,
        NotEqual,
    }

    [System.Serializable]
    public class StateConditionData
    {
        public float targetValue;

        public string parameterName;

        public CompareType compareType;
    }
}
