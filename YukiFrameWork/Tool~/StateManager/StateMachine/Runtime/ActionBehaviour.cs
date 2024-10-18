namespace YukiFrameWork.ActionStates
{
    using System;

    /// <summary>
    /// 动作行为--用户添加的组件
    /// </summary>
    [Serializable]
    [DisableRuntimeArchitectureWarning]
    public class ActionBehaviour : BehaviourBase
    {
        /// <summary>
        /// 当进入状态
        /// </summary>
        /// <param name="action">当前动作</param>
        public virtual void OnEnter(StateAction action) { }
      
        public virtual void OnUpdate(StateAction action) { }

        public virtual void OnFixedUpdate(StateAction action) { }

        public virtual void OnLateUpdate(StateAction action) { }
        

        /// <summary>
        /// 当退出状态
        /// </summary>
        /// <param name="action">当前动作</param>
        public virtual void OnExit(StateAction action) { }

        /// <summary>
        /// 当停止动作 : 当动作不使用动画循环时, 动画时间到达100%后调用
        /// </summary>
        /// <param name="action"></param>
        public virtual void OnStop(StateAction action) { }       
    }
}