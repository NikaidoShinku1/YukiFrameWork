using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Playables;

namespace YukiFrameWork.ActionStates
{
    public enum RuntimeInitMode
    {
        Awake, Start,
    }
    /// <summary>
    /// 插件语言
    /// </summary>
    public enum PluginLanguage
    {
        /// <summary>
        /// 英文
        /// </summary>
        English,
        /// <summary>
        /// 中文
        /// </summary>
        Chinese
    }
    /// <summary>
    /// 状态执行管理类   
    /// </summary>
    public sealed class StateManager : MonoBehaviour
    {
        public StateMachineMono support = null;
        public RuntimeInitMode initMode = RuntimeInitMode.Awake;

        void Awake()
        {
            if (initMode == RuntimeInitMode.Awake)
                Init();
        }

        void Start()
        {
            if (initMode == RuntimeInitMode.Start)
                Init();
            support.Init(); //解决awake初始化其他组件还没被初始化导致获取失效
        }

        private void Init()
        {
            if (support == null)
            {
                enabled = false;
                return;
            }
            if (support.GetComponentInParent<StateManager>() == null)//当使用本地公用状态机时
            {
                var sm = Instantiate(support, transform);
                sm.name = support.name;
                sm.transform.localPosition = Vector3.zero;
                if (sm.animation == null)
                    sm.animation = GetComponentInChildren<Animation>();
                else if (!sm.animation.gameObject.scene.isLoaded)
                    sm.animation = GetComponentInChildren<Animation>();
                if (sm.animator == null)
                    sm.animator = GetComponentInChildren<Animator>();
                else if (!sm.animator.gameObject.scene.isLoaded)
                    sm.animator = GetComponentInChildren<Animator>();
                if (sm.director == null)
                    sm.director = GetComponentInChildren<PlayableDirector>();
                else if (!sm.director.gameObject.scene.isLoaded)
                    sm.director = GetComponentInChildren<PlayableDirector>();
                support = sm;
            }
        }

        /// <summary>
        /// 当进入下一个状态, 你也可以立即进入当前播放的状态, 如果不想进入当前播放的状态, 使用StatusEntry方法
        /// </summary>
        /// <param name="nextStateIndex">下一个状态的ID</param>
		public void EnterNextState(int nextStateIndex, int actionId = 0) => support.stateMachine.EnterNextState(nextStateIndex, actionId);

        /// <summary>
        /// 进入下一个状态, 如果状态正在播放就不做任何处理, 如果想让动作立即播放可以使用 OnEnterNextState 方法
        /// </summary>
        /// <param name="stateID"></param>
        public void StatusEntry(int stateID, int actionId = 0) => support.stateMachine.StatusEntry(stateID, actionId);

        /// <summary>
        /// 切换状态
        /// </summary>
        /// <param name="stateId"></param>
        /// <param name="force"></param>
        public void ChangeState(int stateId, int actionId = 0, bool force = false) => support.stateMachine.ChangeState(stateId, actionId, force);

#if UNITY_EDITOR
        public void OnValidate()
        {
            if (support == null)
                return;
            support.OnScriptReload();
        }
#endif

        private void OnEnable()
        {
            StateMachineSystem.AddStateMachine(support.stateMachine);
        }

        private void OnDisable()
        {
            StateMachineSystem.RemoveStateMachine(support.stateMachine);
        }
    }
}