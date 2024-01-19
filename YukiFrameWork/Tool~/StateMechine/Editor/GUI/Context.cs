using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
namespace YukiFrameWork.States
{
    public class Context 
    {     
        private StateMechine stateMechine;
        private StateManager stateManager;
        public List<StateBase> SelectNodes = new List<StateBase>();

        public StateTransitionData selectTransition = null;
        #region 箭头的预览
        public bool isPreviewTransition = false;
        public StateBase fromState = null;
        public StateBase hoverState = null;
        #endregion

        #region 属性
        public float ZoomFactor { get; set; } = 0.3f;

        public Vector2 DragOffset { get; set; }
        public StateMechine StateMechine
        {
            get
            {
                StateMechine stateMechine = GetMechine();
                if (stateMechine != null && stateMechine != this.stateMechine)
                {
                    Reset();
                    this.stateMechine = stateMechine;
                }
                return this.stateMechine;
            }
            set => this.stateMechine = value;
        }

        public StateManager StateManager
        {
            get
            {
                if (Application.isPlaying)
                {
                    GameObject obj = Selection.activeObject as GameObject;

                    if (obj != null && obj.GetComponent<StateManager>() != null)
                    {
                        stateManager = obj.GetComponent<StateManager>();
                    }
                }
                return stateManager;
            }
            set
            {
                stateManager = value;
            }
        }

        #endregion


        #region 方法
        public void Reset()
        {
            //重置缩放和偏移
            this.ZoomFactor = 0.3f;
            this.DragOffset = Vector2.zero;            
        }

        public StateMechine GetMechine()
        {
            StateMechine stateMechine = Selection.activeObject as StateMechine;

            if (stateMechine != null)
                return stateMechine;

            if (StateManager != null)
            {
                return StateManager.stateMechine;
            }

            GameObject obj = Selection.activeObject as GameObject;
            if (obj != null && obj.GetComponent<StateManager>())
                stateMechine = obj.GetComponent<StateManager>().stateMechine;

            return stateMechine;
        }

        public void ClearSelections()
        {
            this.SelectNodes.Clear();
            selectTransition = null;
        }

        public void StartPreviewTransition(StateBase state)
        {
            this.fromState = state;
            this.isPreviewTransition = true;
        }

        public void CancelPreviewTransition()
        {
            this.fromState = null;
            this.isPreviewTransition = false;
        }
        #endregion
    }
}
#endif