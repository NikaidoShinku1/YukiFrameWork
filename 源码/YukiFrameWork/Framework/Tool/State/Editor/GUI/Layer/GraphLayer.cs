﻿using System.Collections;
using System.Collections.Generic;
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
        public StateMechineEditor EditorWindow { get; private set; } = null;

        protected Context Context => EditorWindow.Context;
        #endregion

        #region 方法

        public GraphLayer(StateMechineEditor editorWindow)
            => this.EditorWindow = editorWindow;

        public virtual void OnGUI(Rect rect)
        {
            position = rect;
            UpdateTransformationMatrix();         
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
                foreach (var item in this.Context.StateMechine.states)
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

        #endregion
    }
}
