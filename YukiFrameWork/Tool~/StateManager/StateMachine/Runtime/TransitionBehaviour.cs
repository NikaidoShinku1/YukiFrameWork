namespace YukiFrameWork.ActionStates
{
    using System;

    /// <summary>
    /// 连接行为--用户可以继承此类添加组件 2017年12月6日(星期三)
    /// </summary>
    [Serializable]
    public class TransitionBehaviour : BehaviourBase
    {
        [HideField]
        public int transitionID;
        public Transition Transition => state.transitions[transitionID];
        public virtual void OnUpdate(ref bool isEnterNextState) { }
        public virtual void OnFixedUpdate(ref bool isEnterNextState) { }
        public virtual void OnLateUpdate(ref bool isEnterNextState) { }
    }
}