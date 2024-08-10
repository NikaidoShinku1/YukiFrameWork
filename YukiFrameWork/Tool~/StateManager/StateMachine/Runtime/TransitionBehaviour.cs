namespace YukiFrameWork.ActionStates
{
    using System;

    /// <summary>
    /// 连接行为--用户可以继承此类添加组件 
    /// </summary>
    [Serializable]
    public class TransitionBehaviour : BehaviourBase
    {
        [UnityEngine.HideInInspector]
        public int transitionID;
        public Transition Transition => state.transitions[transitionID];
        public virtual void OnUpdate(ref bool isEnterNextState) { }
        public virtual void OnFixedUpdate(ref bool isEnterNextState) { }
        public virtual void OnLateUpdate(ref bool isEnterNextState) { }
    }
}