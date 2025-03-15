///=====================================================
/// - FileName:      MachineRectangleSelector.cs
/// - NameSpace:     YukiFrameWork.Machine
/// - Description:   高级定制脚本生成
/// - Creation Time: 2025/3/9 23:52:33
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor.Graphs;
using UnityEngine.UIElements;

namespace YukiFrameWork.Machine
{
    public class MachineRectangleSelector : ImmediateModeElement
    {
        private Vector3 dragStartPoint;

        public bool Dragging { get; private set; } = false;

        private Vector3 currentMousePosition;

        private VisualElement m_Container;

        private StateGraphView graphView;

        // Fix编码 
        internal static Rect FromToRect(Vector2 start, Vector2 end)
        {
            Rect result = new Rect(start.x, start.y, end.x - start.x, end.y - start.y);
            if (result.width < 0f)
            {
                result.x += result.width;
                result.width = 0f - result.width;
            }

            if (result.height < 0f)
            {
                result.y += result.height;
                result.height = 0f - result.height;
            }

            return result;
        }

        private Dictionary<string, Vector3> node_postion_offset = new Dictionary<string, Vector3>();

        protected override void ImmediateRepaint()
        {

            VisualElement visualElement = base.parent;
            graphView = visualElement as StateGraphView;
            if (graphView == null)
            {
                throw new InvalidOperationException("FSMRectangleSelector can only be added to a GraphView");
            }

            currentMousePosition = Event.current.mousePosition;
            if (Event.current.type == EventType.Repaint && Dragging)
                Styles.selectionRect.Draw(FromToRect(dragStartPoint, Event.current.mousePosition), isHover: false, isActive: false, on: false, hasKeyboardFocus: false);

            m_Container = graphView.contentViewContainer;
            Matrix4x4 inverse = m_Container.transform.matrix.inverse;

            if (Dragging)
            {
                Vector3 start = inverse.MultiplyPoint(dragStartPoint);
                Vector3 end = inverse.MultiplyPoint(currentMousePosition);
                Rect dragRect = FromToRect(start, end);

                graphView.graphElements.ForEach((e) =>
                {
                    //Debug.LogFormat("name:{0} position:{1}", e.name, e.GetPosition());
                    if (dragRect.Overlaps(e.GetPosition()))
                    {
                        graphView.AddToSelection(e);
                    }
                    else
                    {
                        graphView.RemoveFromSelection(e);
                    }
                });

            }

            UpdateHoverState();

        }


        public void ExecuteAction(EventBase evt)
        {
            VisualElement visualElement = base.parent;
            graphView = visualElement as StateGraphView;
            if (graphView == null)
            {
                throw new InvalidOperationException("MachineRectangleSelector can only be added to a GraphView");
            }

            MouseDownEvent mouseDownEvent = evt as MouseDownEvent;

            if (mouseDownEvent != null && mouseDownEvent.button == 0 && graphView.HoverNode == null)
            {
                dragStartPoint = currentMousePosition;
                Dragging = true;
              
            }

            MouseUpEvent mouseUpEvent = evt as MouseUpEvent;
            if (mouseUpEvent != null && Event.current.button == 0)
            {
                Dragging = false;
            }

            MouseLeaveWindowEvent windowEvent = evt as MouseLeaveWindowEvent;
            if (windowEvent != null && windowEvent.button == 0)
            {
                Dragging = false;
            }

            //DragStateNode(evt);

        }

        public void CancelDrag()
        {
            Dragging = false;
        }

        private void UpdateHoverState()
        {
            if (graphView == null)
                return;

            m_Container = graphView.contentViewContainer;
            Matrix4x4 inverse = m_Container.transform.matrix.inverse;
            Vector3 point = inverse.MultiplyPoint(currentMousePosition);

            bool isHover = false;

            foreach (var item in graphView.StateNodes.Values)
            {
                if (item.GetPosition().Contains(point))
                {
                    //graphView.transition.CloseTransition = null;
                    graphView.HoverNode = item;
                    isHover = true;
                    break;
                }
            }

            if (!isHover)
            {
                graphView.HoverNode = null;
            }
        }



    }
}

#endif
