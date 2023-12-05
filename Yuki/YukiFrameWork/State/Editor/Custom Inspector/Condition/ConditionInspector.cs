using System.Collections;
using System.Collections.Generic;
using UnityEngine; 
namespace YukiFrameWork.States
{
    public abstract class ConditionInspector 
    {
        public abstract void OnGUI(Rect rect, StateConditionData condition, StateMechine stateMechine);
    }
}