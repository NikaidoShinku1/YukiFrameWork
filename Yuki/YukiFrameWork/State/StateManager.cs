using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YukiFrameWork
{
    public enum InitType
    {
        Awake,
        OnEnable,
        Start
    }

    public enum DeBugLog
    {
        �ر�,
        ����      
    }

}
namespace YukiFrameWork.States
{
    public class StateManager : MonoBehaviour
    {
        [HideInInspector]
        [SerializeField]
        public InitType initType;

        [HideInInspector]
        [SerializeField]
        public DeBugLog IsDebugLog;

        [HideInInspector]
        [SerializeField]
        public StateMechine stateMechine;
               
        [HideInInspector]
        public int normalID;    

        [HideInInspector]
        public Stack<int> stateIndexs = new Stack<int>();

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
            if (stateMechine == null)
            {
                Debug.LogError("StateMechine is not added��");
                return;
            }
            foreach (var state in stateMechine.states)
            {              
                state.Init(this);
            }
            if (IsDebugLog == DeBugLog.����)
            {
                Debug.Log($"״̬����ʼ����ɣ�״̬��������{gameObject.name},��ʼ���������ڣ�{initType}");
            }
            stateMechine.OnChangeState(normalID);
            
        }

        public bool GetStateByIndex(int index)
        {
            return stateIndexs.Contains(index);
        }
    }

}