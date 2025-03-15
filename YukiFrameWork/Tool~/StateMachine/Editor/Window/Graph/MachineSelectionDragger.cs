///=====================================================
/// - FileName:      MachineSelectionDragger.cs
/// - NameSpace:     YukiFrameWork.Machine
/// - Description:   高级定制脚本生成
/// - Creation Time: 2025/3/9 23:55:40
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace YukiFrameWork.Machine
{
    public class MachineSelectionDragger : Dragger
    {
        private class OriginalPos
        {
            public Rect pos;

            public Scope scope;

            public StackNode stack;

            public int stackIndex;

            public bool dragStarted;
        }

        private IDropTarget m_PrevDropTarget;

        private GraphViewChange m_GraphViewChange;

        private List<GraphElement> m_MovedElements;

        private List<VisualElement> m_DropTargetPickList = new List<VisualElement>();

        private GraphView m_GraphView;

        private Dictionary<GraphElement, OriginalPos> m_OriginalPos;

        private Vector2 m_originalMouse;

        internal const int k_PanAreaWidth = 100;

        internal const int k_PanSpeed = 4;

        internal const int k_PanInterval = 10;

        internal const float k_MinSpeedFactor = 0.5f;

        internal const float k_MaxSpeedFactor = 2.5f;

        internal const float k_MaxPanSpeed = 10f;

        private IVisualElementScheduledItem m_PanSchedule;

        private Vector3 m_PanDiff = Vector3.zero;

        private Vector3 m_ItemPanDiff = Vector3.zero;

        private Vector2 m_MouseDiff = Vector2.zero;

        private GraphElement selectedElement { get; set; }

        private GraphElement clickedElement { get; set; }

        private IDropTarget GetDropTargetAt(Vector2 mousePosition, IEnumerable<VisualElement> exclusionList)
        {
            List<VisualElement> dropTargetPickList = m_DropTargetPickList;
            dropTargetPickList.Clear();
            base.target.panel.PickAll(mousePosition, dropTargetPickList);
            IDropTarget dropTarget = null;
            for (int i = 0; i < dropTargetPickList.Count; i++)
            {
                if (dropTargetPickList[i] == base.target && base.target != m_GraphView)
                {
                    continue;
                }

                VisualElement visualElement = dropTargetPickList[i];
                dropTarget = visualElement as IDropTarget;
                if (dropTarget != null)
                {
                    if (!exclusionList.Contains(visualElement))
                    {
                        break;
                    }

                    dropTarget = null;
                }
            }

            return dropTarget;
        }

        //
        // 摘要:
        //     SelectionDragger's constructor.
        public MachineSelectionDragger()
        {
            base.activators.Add(new ManipulatorActivationFilter
            {
                button = MouseButton.LeftMouse
            });
            base.activators.Add(new ManipulatorActivationFilter
            {
                button = MouseButton.LeftMouse,
                modifiers = EventModifiers.Shift
            });
            if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer)
            {
                base.activators.Add(new ManipulatorActivationFilter
                {
                    button = MouseButton.LeftMouse,
                    modifiers = EventModifiers.Command
                });
            }
            else
            {
                base.activators.Add(new ManipulatorActivationFilter
                {
                    button = MouseButton.LeftMouse,
                    modifiers = EventModifiers.Control
                });
            }

            base.panSpeed = new Vector2(1f, 1f);
            base.clampToParentEdges = false;
            m_MovedElements = new List<GraphElement>();
            m_GraphViewChange.movedElements = m_MovedElements;
        }

        //
        // 摘要:
        //     Called to register click event callbacks on the target element.
        protected override void RegisterCallbacksOnTarget()
        {
            ISelection selection = base.target as ISelection;
            if (selection == null)
            {
                throw new InvalidOperationException("Manipulator can only be added to a control that supports selection");
            }

            base.target.RegisterCallback<MouseDownEvent>(OnMouseDown);
            base.target.RegisterCallback<MouseMoveEvent>(OnMouseMove);
            base.target.RegisterCallback<MouseUpEvent>(OnMouseUp);
            base.target.RegisterCallback<KeyDownEvent>(OnKeyDown);
            base.target.RegisterCallback<MouseCaptureOutEvent>(OnMouseCaptureOutEvent);
        }

        //
        // 摘要:
        //     Called to unregister event callbacks from the target element.
        protected override void UnregisterCallbacksFromTarget()
        {
            base.target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
            base.target.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
            base.target.UnregisterCallback<MouseUpEvent>(OnMouseUp);
            base.target.UnregisterCallback<KeyDownEvent>(OnKeyDown);
            base.target.UnregisterCallback<MouseCaptureOutEvent>(OnMouseCaptureOutEvent);
        }

        private static void SendDragAndDropEvent(IDragAndDropEvent evt, List<ISelectable> selection, IDropTarget dropTarget, ISelection dragSource)
        {
            if (dropTarget == null)
            {
                return;
            }

            EventBase eventBase = evt as EventBase;
            if (eventBase.eventTypeId == EventBase<DragExitedEvent>.TypeId())
            {
                dropTarget.DragExited();
            }
            else if (eventBase.eventTypeId == EventBase<DragEnterEvent>.TypeId())
            {
                dropTarget.DragEnter(evt as DragEnterEvent, selection, dropTarget, dragSource);
            }
            else if (eventBase.eventTypeId == EventBase<DragLeaveEvent>.TypeId())
            {
                dropTarget.DragLeave(evt as DragLeaveEvent, selection, dropTarget, dragSource);
            }

            if (dropTarget.CanAcceptDrop(selection))
            {
                if (eventBase.eventTypeId == EventBase<DragPerformEvent>.TypeId())
                {
                    dropTarget.DragPerform(evt as DragPerformEvent, selection, dropTarget, dragSource);
                }
                else if (eventBase.eventTypeId == EventBase<DragUpdatedEvent>.TypeId())
                {
                    dropTarget.DragUpdated(evt as DragUpdatedEvent, selection, dropTarget, dragSource);
                }
            }
        }

        private void OnMouseCaptureOutEvent(MouseCaptureOutEvent e)
        {
            if (m_Active)
            {
                if (m_PrevDropTarget != null && m_GraphView != null && m_PrevDropTarget.CanAcceptDrop(m_GraphView.selection))
                {
                    m_PrevDropTarget.DragExited();
                }

                selectedElement = null;
                m_PrevDropTarget = null;
                m_Active = false;
            }
        }

        //
        // 摘要:
        //     Called on mouse down event.
        //
        // 参数:
        //   e:
        //     The event.
        protected new void OnMouseDown(MouseDownEvent e)
        {
            if (m_Active)
            {
                e.StopImmediatePropagation();
            }
            else
            {
                if (!CanStartManipulation(e))
                {
                    return;
                }

                m_GraphView = base.target as GraphView;
                if (m_GraphView == null)
                {
                    return;
                }

                selectedElement = null;
                clickedElement = e.target as GraphElement;
                if (clickedElement == null)
                {
                    VisualElement visualElement = e.target as VisualElement;
                    clickedElement = visualElement.GetFirstAncestorOfType<GraphElement>();
                    if (clickedElement == null)
                    {
                        return;
                    }
                }

                if (!clickedElement.IsMovable() || !clickedElement.HitTest(clickedElement.WorldToLocal(e.mousePosition)))
                {
                    return;
                }

                if (!m_GraphView.selection.Contains(clickedElement))
                {
                    e.StopImmediatePropagation();
                    return;
                }

                selectedElement = clickedElement;
                m_OriginalPos = new Dictionary<GraphElement, OriginalPos>();
                foreach (ISelectable item in m_GraphView.selection)
                {
                    GraphElement graphElement = item as GraphElement;
                    if (graphElement == null || !graphElement.IsMovable())
                    {
                        continue;
                    }

                    StackNode stackNode = null;
                    if (graphElement.parent is StackNode)
                    {
                        stackNode = graphElement.parent as StackNode;
                        if (stackNode.IsSelected(m_GraphView))
                        {
                            continue;
                        }
                    }

                    Rect position = graphElement.GetPosition();
                    Rect pos = graphElement.hierarchy.parent.ChangeCoordinatesTo(m_GraphView.contentViewContainer, position);
                    m_OriginalPos[graphElement] = new OriginalPos
                    {
                        pos = pos,
                        scope = graphElement.GetContainingScope(),
                        stack = stackNode,
                        stackIndex = (stackNode?.IndexOf(graphElement) ?? (-1))
                    };
                }

                m_originalMouse = e.mousePosition;
                m_ItemPanDiff = Vector3.zero;
                if (m_PanSchedule == null)
                {
                    m_PanSchedule = m_GraphView.schedule.Execute(Pan).Every(10L).StartingIn(10L);
                    m_PanSchedule.Pause();
                }

                m_Active = true;
                base.target.CaptureMouse();
                e.StopImmediatePropagation();
            }
        }

        internal Vector2 GetEffectivePanSpeed(Vector2 mousePos)
        {
            Vector2 vector = Vector2.zero;
            if (mousePos.x <= 100f)
            {
                vector.x = (0f - ((100f - mousePos.x) / 100f + 0.5f)) * 4f;
            }
            else if (mousePos.x >= m_GraphView.contentContainer.layout.width - 100f)
            {
                vector.x = ((mousePos.x - (m_GraphView.contentContainer.layout.width - 100f)) / 100f + 0.5f) * 4f;
            }

            if (mousePos.y <= 100f)
            {
                vector.y = (0f - ((100f - mousePos.y) / 100f + 0.5f)) * 4f;
            }
            else if (mousePos.y >= m_GraphView.contentContainer.layout.height - 100f)
            {
                vector.y = ((mousePos.y - (m_GraphView.contentContainer.layout.height - 100f)) / 100f + 0.5f) * 4f;
            }

            vector = Vector2.ClampMagnitude(vector, 10f);
            return vector;
        }

        //
        // 摘要:
        //     Called on mouse move event.
        //
        // 参数:
        //   e:
        //     The event.
        protected new void OnMouseMove(MouseMoveEvent e)
        {
            if (!m_Active || m_GraphView == null)
            {
                return;
            }

            VisualElement src = (VisualElement)e.target;
            Vector2 mousePos = src.ChangeCoordinatesTo(m_GraphView.contentContainer, e.localMousePosition);
            m_PanDiff = GetEffectivePanSpeed(mousePos);
            if (m_PanDiff != Vector3.zero)
            {
                m_PanSchedule.Resume();
            }
            else
            {
                m_PanSchedule.Pause();
            }

            m_MouseDiff = m_originalMouse - e.mousePosition;
            Dictionary<Group, List<GraphElement>> dictionary = (e.shiftKey ? new Dictionary<Group, List<GraphElement>>() : null);
            foreach (KeyValuePair<GraphElement, OriginalPos> originalPo in m_OriginalPos)
            {
                GraphElement key = originalPo.Key;
                if (key.hierarchy.parent == null)
                {
                    continue;
                }

                if (!originalPo.Value.dragStarted)
                {
                    key.GetFirstAncestorOfType<StackNode>()?.OnStartDragging(key);
                    if (dictionary != null)
                    {
                        Group group = key.GetContainingScope() as Group;
                        if (group != null)
                        {
                            if (!dictionary.ContainsKey(group))
                            {
                                dictionary[group] = new List<GraphElement>();
                            }

                            dictionary[group].Add(key);
                        }
                    }

                    originalPo.Value.dragStarted = true;
                }

                MoveElement(key, originalPo.Value.pos);
            }

            List<ISelectable> selection = m_GraphView.selection;
            IDropTarget dropTargetAt = GetDropTargetAt(e.mousePosition, selection.OfType<VisualElement>());

            m_PrevDropTarget = dropTargetAt;
            selectedElement = null;
            e.StopPropagation();
        }

        private void Pan(TimerState ts)
        {
            //m_GraphView.viewTransform.position -= m_PanDiff;
            //m_ItemPanDiff += m_PanDiff;
            //foreach (KeyValuePair<GraphElement, OriginalPos> originalPo in m_OriginalPos)
            //{
            //    MoveElement(originalPo.Key, originalPo.Value.pos);
            //}
        }

        private void MoveElement(GraphElement element, Rect originalPos)
        {
            Matrix4x4 worldTransform = element.worldTransform;
            Vector3 vector = new Vector3(worldTransform.m00, worldTransform.m11, worldTransform.m22);
            Rect rect = new Rect(0f, 0f, originalPos.width, originalPos.height);
            rect.x = originalPos.x - (m_MouseDiff.x - m_ItemPanDiff.x) * base.panSpeed.x / vector.x * element.transform.scale.x;
            rect.y = originalPos.y - (m_MouseDiff.y - m_ItemPanDiff.y) * base.panSpeed.y / vector.y * element.transform.scale.y;
            element.SetPosition(m_GraphView.contentViewContainer.ChangeCoordinatesTo(element.hierarchy.parent, rect));
        }

        //
        // 摘要:
        //     Called on mouse up event.
        //
        // 参数:
        //   e:
        //     The event.
        //
        //   evt:
        protected new void OnMouseUp(MouseUpEvent evt)
        {
            if (m_GraphView == null)
            {
                if (m_Active)
                {
                    base.target.ReleaseMouse();
                    selectedElement = null;
                    m_Active = false;
                    m_PrevDropTarget = null;
                }

                return;
            }

            List<ISelectable> selection = m_GraphView.selection;
            if (!CanStopManipulation(evt))
            {
                return;
            }

            if (m_Active)
            {
                if (selectedElement == null)
                {
                    m_MovedElements.Clear();
                    foreach (IGrouping<StackNode, GraphElement> item in from v in m_OriginalPos
                                                                        group v.Key by v.Value.stack)
                    {
                        if (item.Key != null && m_GraphView.elementsRemovedFromStackNode != null)
                        {
                            m_GraphView.elementsRemovedFromStackNode(item.Key, item);
                        }

                        foreach (GraphElement item2 in item)
                        {
                            item2.UpdatePresenterPosition();
                        }

                        m_MovedElements.AddRange(item);
                    }

                    GraphView graphView = base.target as GraphView;
                    if (graphView != null && graphView.graphViewChanged != null)
                    {
                        KeyValuePair<GraphElement, OriginalPos> keyValuePair = m_OriginalPos.First();
                        m_GraphViewChange.moveDelta = keyValuePair.Key.GetPosition().position - keyValuePair.Value.pos.position;
                        graphView.graphViewChanged(m_GraphViewChange);
                    }
                }

                m_PanSchedule.Pause();
                if (m_ItemPanDiff != Vector3.zero)
                {
                    Vector3 position = m_GraphView.contentViewContainer.transform.position;
                    Vector3 scale = m_GraphView.contentViewContainer.transform.scale;
                    m_GraphView.UpdateViewTransform(position, scale);
                }

                base.target.ReleaseMouse();
                evt.StopPropagation();
            }

            selectedElement = null;
            m_Active = false;
            m_PrevDropTarget = null;
        }

        private void OnKeyDown(KeyDownEvent e)
        {
            if (e.keyCode != KeyCode.Escape || m_GraphView == null || !m_Active)
            {
                return;
            }

            Dictionary<Scope, List<GraphElement>> dictionary = new Dictionary<Scope, List<GraphElement>>();
            foreach (KeyValuePair<GraphElement, OriginalPos> originalPo in m_OriginalPos)
            {
                OriginalPos value = originalPo.Value;
                if (value.stack != null)
                {
                    value.stack.InsertElement(value.stackIndex, originalPo.Key);
                    continue;
                }

                if (value.scope != null)
                {
                    if (!dictionary.ContainsKey(value.scope))
                    {
                        dictionary[value.scope] = new List<GraphElement>();
                    }

                    dictionary[value.scope].Add(originalPo.Key);
                }

                originalPo.Key.SetPosition(value.pos);
            }

            foreach (KeyValuePair<Scope, List<GraphElement>> item in dictionary)
            {
                item.Key.AddElements(item.Value);
            }

            m_PanSchedule.Pause();
            if (m_ItemPanDiff != Vector3.zero)
            {
                Vector3 position = m_GraphView.contentViewContainer.transform.position;
                Vector3 scale = m_GraphView.contentViewContainer.transform.scale;
                m_GraphView.UpdateViewTransform(position, scale);
            }

            using (DragExitedEvent evt = EventBase<DragExitedEvent>.GetPooled())
            {
                List<ISelectable> selection = m_GraphView.selection;
                SendDragAndDropEvent(evt, selection, m_PrevDropTarget, m_GraphView);
            }

            base.target.ReleaseMouse();
            e.StopPropagation();
        }


    }
}
#endif