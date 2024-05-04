///=====================================================
/// - FileName:      DiaLogGraphLayer.cs
/// - NameSpace:     YukiFrameWork.DiaLog
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/4/30 0:44:14
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEditor;
namespace YukiFrameWork.DiaLog
{
	public class DiaLogGraphLayer
	{
        #region 字段

        protected Rect position;

        private Matrix4x4 transormMatrix = Matrix4x4.identity;

        #endregion

        #region 属性
        public DiaLogEditorWindow EditorWindow { get; private set; } = null;

        protected Context Context => EditorWindow.Context;
        #endregion

        #region 方法

        public DiaLogGraphLayer(DiaLogEditorWindow editorWindow)
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

            if (this.Context.NodeTree != null)
            {
                List<Node> stateBases = this.Context.NodeTree.nodes;             
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
            try
            {
                if (UnityEditor.EditorWindow.mouseOverWindow != null && UnityEditor.EditorWindow.mouseOverWindow.GetType()?.ToString()?.Equals("UnityEditor.InspectorWindow") == true) return;
                
            }
            catch { }
            this.Context.ClearSelections();
        }

        #endregion
    }
}
