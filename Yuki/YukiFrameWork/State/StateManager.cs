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
        [Header("״̬����ʼ����ʽ")]
        private InitType initType;

        [Header("״̬�����Դ�ӡ")]
        public bool IsDebugLog;
      
        public StateMechine stateMechine;
               
        [HideInInspector]
        public int normalID;  

        /// <summary>
        /// ��ǰ����id
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
        /// ��ʼ�����е�״̬
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
                Debug.Log($"״̬����ʼ����ɣ�״̬��������{gameObject.name},��ʼ���������ڣ�{initType}");
            }
            stateMechine.OnChangeState(normalID);
            
        }
    }

}