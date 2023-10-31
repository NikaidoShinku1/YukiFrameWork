using System.Collections;
using UnityEngine;

namespace YukiFrameWork.States
{
    public enum InitType
    {
        Awake,
        OnEnable,
        Start
    }

    public class StateManager : MonoBehaviour
    {
        [SerializeField]
        [Header("状态机初始化方式")]
        private InitType initType;

        [Header("状态机调试打印")]
        public bool IsDebugLog;
      
        public StateMechine stateMechine;
               
        [HideInInspector]
        public int normalID;  

        /// <summary>
        /// 当前控制id
        /// </summary>
        [HideInInspector]
        public int controllerID;

        [HideInInspector]
        public bool isController;

        private void Awake()
        {
            if (initType == InitType.Awake) Init();
        }
        private void OnEnable()
        {
            if (initType == InitType.OnEnable) Init();
        }

        private void Start()
        {
            if (initType == InitType.Start)
            {               
                Init();
            }
        }


        private void Init()
        {
            InitState();               
        }

        /// <summary>
        /// 初始化所有的状态
        /// </summary>
        /// <param name="stateManager"></param>
        public void InitState()
        {
            foreach (var state in stateMechine.states)
            {
                state.Init(this);
            }
            if (IsDebugLog)
            {
                Debug.Log($"状态机初始化完成，状态机归属：{gameObject.name},初始化生命周期：{initType}");
            }
            stateMechine.OnChangeState(normalID);
            
        }
    }

}