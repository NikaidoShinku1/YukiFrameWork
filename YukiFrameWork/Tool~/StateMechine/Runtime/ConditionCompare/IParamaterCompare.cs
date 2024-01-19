using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YukiFrameWork.States
{
    public interface IParamaterCompare
    {
        bool IsMeetCondition(StateParameterData data, float v);
    }  
}
