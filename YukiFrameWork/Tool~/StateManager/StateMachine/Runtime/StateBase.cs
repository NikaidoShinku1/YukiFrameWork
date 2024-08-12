using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using YukiFrameWork.Extension;

namespace YukiFrameWork.ActionStates
{
    public interface IAnimationHandler
    {
        void OnInit();
        /// <summary>
        /// 当播放动画方法
        /// </summary>
        /// <param name="state">当前状态</param>
        /// <param name="stateAction">当前动作</param>
        void OnPlayAnimation(State state, StateAction stateAction);
        /// <summary>
        /// 当动画每帧更新
        /// </summary>
        /// <param name="state">当前状态</param>
        /// <param name="stateAction">当前动作</param>
        /// <returns>是否播放完成</returns>
        bool OnAnimationUpdate(State state, StateAction stateAction,UpdateStatus updateStatus);
    }

    public interface IStateMachine
    {
        int Id { get; set; }
        string name { get; set; }
        StateMachineView View { get; set; }
        Transform transform { get; }
        State[] States { get; set; }
#if UNITY_EDITOR
        State SelectState { get; set; }
        List<int> SelectStates { get; set; }    
#endif
        State DefaultState { get; set; }
        int StateId { get; set; }
        int NextId { get; set; }
        IStateMachine Parent { get; set; }
        IAnimationHandler Handler { get; set; }
        UpdateStatus UpdateStatus { get; set; }
        /// <summary>
        /// 状态机执行
        /// </summary>
        void Execute(UpdateStatus updateStatus);     
        /// <summary>
        /// 当进入下一个状态, 你也可以立即进入当前播放的状态, 如果不想进入当前播放的状态, 使用StatusEntry方法
        /// </summary>
        /// <param name="nextStateIndex">下一个状态的ID</param>
        void EnterNextState(int nextStateIndex, int actionId = 0);
        /// <summary>
        /// 进入下一个状态, 如果状态正在播放就不做任何处理, 如果想让动作立即播放可以使用 OnEnterNextState 方法
        /// </summary>
        /// <param name="stateID"></param>
        void StatusEntry(int stateID, int actionId = 0);
        /// <summary>
        /// 切换状态
        /// </summary>
        /// <param name="stateId"></param>
        /// <param name="force"></param>
        void ChangeState(int stateId, int actionId = 0, bool force = false);
        void ChangeChildState(int stateId, int actionId);
        void UpdateStates();
    }   

    /// <summary>
    /// 状态基类
    /// </summary>
    [System.Serializable]
    public class StateBase
    {
        public string name;
        public int ID, perID;
#if UNITY_2020_1_OR_NEWER
        [NonReorderable]
#endif
        public BehaviourBase[] behaviours;
        public IStateMachine stateMachine;
#if UNITY_EDITOR
        [HideInInspector]
        public bool foldout = true;
        public Rect rect;
#endif

        /// <summary>
        /// 添加状态行为组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T AddComponent<T>() where T : BehaviourBase, new()
        {
            return AddComponent(new T());
        }

        /// <summary>
        /// 添加状态行为组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="component"></param>
        /// <returns></returns>
        public T AddComponent<T>(T component) where T : BehaviourBase
        {
            return (T)AddComponentInternal(component);
        }

        public void AddComponent(params BehaviourBase[] behaviours)
        {
            if (behaviours == null)
                return;
            foreach (var component in behaviours)
                AddComponentInternal(component);
        }

        private BehaviourBase AddComponentInternal(BehaviourBase component)
        {
            System.Type type = component.GetType();
            component.name = type.ToString();
            component.ID = ID;
            component.stateMachine = stateMachine;
#if UNITY_EDITOR
            component.InitMetadatas();
#endif            
            InitUpdateMode(type);
            component.OnInit();          
            ArrayExtend.Add(ref behaviours, component);
            return component;
        }

        protected void InitUpdateMode(System.Type type)
        {
            var lateUpdateMethod = type.GetMethod("OnLateUpdate");
            var fixedUpdateMethod = type.GetMethod("OnFixedUpdate");
            stateMachine.UpdateStatus |= (lateUpdateMethod.DeclaringType == type) ? UpdateStatus.OnLateUpdate : UpdateStatus.OnUpdate;
            stateMachine.UpdateStatus |= (fixedUpdateMethod.DeclaringType == type) ? UpdateStatus.OnFixedUpdate : UpdateStatus.OnUpdate;
        }

        /// <summary>
        /// 获取状态组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetComponent<T>() where T : BehaviourBase
        {
            for (int i = 0; i < behaviours.Length; i++)
                if (behaviours[i] is T component)
                    return component;
            return null;
        }

        /// <summary>
        /// 获取多个状态组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T[] GetComponents<T>() where T : BehaviourBase
        {
            var components = new List<T>();
            for (int i = 0; i < behaviours.Length; i++)
                if (behaviours[i] is T component)
                    components.Add(component);
            return components.ToArray();
        }
    }
}