using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;


namespace YukiFrameWork.States
{
    public class GraphLayer
    {
        #region 字段

        protected Rect position;

        private Matrix4x4 transormMatrix = Matrix4x4.identity;

        #endregion

        #region 属性
        public StateMechineEditorWindow EditorWindow { get; private set; } = null;

        protected Context Context => EditorWindow.Context;
        #endregion

        #region 方法

        public GraphLayer(StateMechineEditorWindow editorWindow)
        {
            this.EditorWindow = editorWindow;

            EditorApplication.playModeStateChanged += OnModeChange;
        }

        public virtual void OnGUI(Rect rect)
        {
            position = rect;
            UpdateTransformationMatrix();                        
        }

        public virtual void OnModeChange(PlayModeStateChange mode)
        {
            
        }

        public virtual void ProcessEvents() { }

        public virtual void Update() { }

        private void UpdateTransformationMatrix()
        {
            var centerMat = Matrix4x4.Translate(-position.center);
            var translationMat = Matrix4x4.Translate(this.Context.DragOffset / this.Context.ZoomFactor);
            var scaleMat = Matrix4x4.Scale(Vector3.one * this.Context.ZoomFactor);

            this.transormMatrix = centerMat.inverse * scaleMat * translationMat * centerMat;

        }
        /// <summary>
        /// 鼠标是否处于选择的状态之下
        /// </summary>
        /// <returns>如果鼠标不在任一状态下返回False</returns>
        protected bool IsSelectActive()
        {
            bool isSelectClearTive = false;

            if (this.Context.StateMechine != null)
            {
                List<StateBase> stateBases = null;
                if (this.Context.StateMechine.IsSubLayerAndContainsName())
                {
                    stateBases = this.Context.StateMechine.subStatesPair[this.Context.StateMechine.layerName].stateBases;
                }
                else stateBases = this.Context.StateMechine.states;
                foreach (var item in stateBases)
                {
                    Rect rect = GetTransformedRect(item.rect);
                    if (position.Overlaps(rect) && rect.Contains(Event.current.mousePosition))
                    {
                        isSelectClearTive = true;
                        break;
                    }
                }
            }
            return isSelectClearTive;
        }

        /// <summary>
        /// 位置转换矩形区域
        /// </summary>
        /// <param name="rect">传进来的Rect位置</param>
        /// <returns>返回转换后的坐标</returns>
        public Rect GetTransformedRect(Rect rect)
        {
            Rect result = new Rect();
            result.position = transormMatrix.MultiplyPoint(rect.position);
            result.size = transormMatrix.MultiplyVector(rect.size);
            return result;
        }

        public virtual void OnLostFocus()
        {
            if (UnityEditor.EditorWindow.mouseOverWindow != null && UnityEditor.EditorWindow.mouseOverWindow.GetType().ToString().Equals("UnityEditor.InspectorWindow")) return;
            this.Context.ClearSelections();
        }

        protected List<StateBase> GetLayerStates()
        {
            if (this.Context.StateMechine == null)
                return null;

            List<StateBase> stateBases = null;

            if (this.Context.StateMechine.IsSubLayerAndContainsName())
            {
                stateBases = this.Context.StateMechine.subStatesPair[this.Context.StateMechine.layerName].stateBases;
            }
            else
            {
                stateBases = this.Context.StateMechine.states;
            }
            return stateBases;
        }

        protected List<StateTransitionData> GetLayerTransitions()
        {
            if (this.Context.StateMechine == null)
                return null;

            if (this.Context.StateMechine.IsSubLayerAndContainsName())
            {
                this.Context.StateMechine.subTransitions.TryGetValue(this.Context.StateMechine.layerName, out var value);
                return value;
            }
            return this.Context.StateMechine.transitions;
        }


        #endregion
    }
}
#endif