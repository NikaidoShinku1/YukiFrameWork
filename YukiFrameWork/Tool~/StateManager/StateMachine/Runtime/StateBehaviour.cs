using System;

namespace YukiFrameWork.ActionStates
{
    /// <summary>
    /// 状态行为脚本
    /// </summary>
    [Serializable]
    [DisableViewWarning]
    [DisableRuntimeArchitectureWarning]
    public class StateBehaviour : BehaviourBase
    {       
        /// <summary>
        /// 当状态进入时
        /// </summary>
        public virtual void OnEnter() { }
       
        public virtual void OnUpdate() { }

        public virtual void OnFixedUpdate() { }

        public virtual void OnLateUpdate() { }

        /// <summary>
        /// 当状态退出后
        /// </summary>
        public virtual void OnExit() { }

        /// <summary>
        /// 当停止动作 : 当动作不使用动画循环时, 动画时间到达100%后调用
        /// </summary>
        public virtual void OnStop() { }

        /// <summary>
        /// 当动作处于循环模式时, 子动作动画每次结束都会调用一次
        /// </summary>
        public virtual void OnActionExit() { }
    }
}