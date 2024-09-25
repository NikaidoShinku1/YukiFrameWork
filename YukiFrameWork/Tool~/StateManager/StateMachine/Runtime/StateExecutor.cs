using UnityEngine;
using UnityEngine.Playables;
#if SHADER_ANIMATED
using FSG.MeshAnimator.ShaderAnimated;
#endif

namespace YukiFrameWork.ActionStates
{
    [DisableViewWarning]
    public sealed class StateExecutor : MonoBehaviour
    {
        public StateMachineView support = null;
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
            if (support.GetComponentInParent<StateExecutor>() == null)//当使用本地公用状态机时
            {
                var sm = Instantiate(support, transform);
                sm.name = support.name;
                sm.transform.localPosition = Vector3.zero;
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