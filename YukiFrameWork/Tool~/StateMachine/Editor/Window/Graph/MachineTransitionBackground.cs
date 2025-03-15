///=====================================================
/// - FileName:      MachineTransitionBackground.cs
/// - NameSpace:     YukiFrameWork.Machine
/// - Description:   高级定制脚本生成
/// - Creation Time: 2025/3/10 0:03:54
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;

using System.Collections.Generic;


#if UNITY_EDITOR
using UnityEngine;
using System;
using UnityEditor.Graphs;
using UnityEngine.UIElements;
using UnityEditor;
namespace YukiFrameWork.Machine
{
    public class MachineTransitionBackground : ImmediateModeElement
    {
        private float edgeDistanceMultiplier => Mathf.Clamp(graphView.scale, 0.1f, 1);

        private float edgeSizeMultiplier => Mathf.Clamp(graphView.scale, 0.1f, 1);

        private static Slot s_TargetDraggingSlot;

        private StateGraphView graphView;

        private static Vector3 edgeToSelfOffsetVector => new Vector3(0f, 30f, 0f);

        private static Color selectedEdgeColor => new Color(0.42f, 0.7f, 1f, 1f);

        public StateTransitionData CloseTransition { get; internal set; }

        public StateTransitionData Selection { get; set; }

        private StateTransitionData entryToDefault = new StateTransitionData();

        private Vector3[] vectors = new Vector3[2];

        private Vector3[] points = new Vector3[2];

        private static Color defaultTransitionColor => new Color(0.6f, 0.4f, 0f, 1f);

        private static StateTransitionData current_change_transition;
        private float current_change_transition_timer = 0;

        private float lastRealTime;
        private float deltaTime;

        #region 退出当前子状态
        private StateBase currentState;
        private bool state_exiting = false;
        private string exit_state_parent;
        private string exit_state_name;
        private string exit_state_to_name;
        private float exit_state_timer = 0;
        #endregion

        // Fix编码
        protected override void ImmediateRepaint()
        {
            List<StateTransitionData> transitions = Global.Instance.GetCurrentShowTransitionData();
           
            deltaTime = Time.realtimeSinceStartup - lastRealTime;
            lastRealTime = Time.realtimeSinceStartup;

            if (deltaTime > 0.2) deltaTime = 0;


            if (Event.current.type == EventType.Repaint)
            {
                VisualElement visualElement = base.parent;
                graphView = visualElement as StateGraphView;
                if (graphView == null)
                    throw new InvalidOperationException("StateTransitionBackground can only be added to a GraphView");

                if (transitions != null)
                {
                    foreach (var transition in transitions)
                    {
                        DrawTransiton(transition, Color.white);
                        DrawCurrentTransition(transition);
                    }
                }

                List<StateNodeData> nodes = Global.Instance.GetCurrentShowStateNodeData();
                if (nodes != null)
                {
                    entryToDefault.fromStateName = string.Empty;
                    entryToDefault.toStateName = string.Empty;

                    foreach (var item in nodes)
                    {
                        if (item.IsEntryState)
                            entryToDefault.fromStateName = item.name;
                        if (item.IsDefaultState)
                            entryToDefault.toStateName = item.name;
                    }

                    DrawTransiton(entryToDefault, defaultTransitionColor);
                }

                DrawMakeTransition();

                DrawStateExitTransition();
            }

            if (graphView.HoverNode == null)
                CloseTransition = FindClosestTransition();

        }

        private void DrawTransiton(StateTransitionData data, Color color)
        {

            StateNodeView fromNode = graphView.GetNode(data.fromStateName);
            StateNodeView toNode = graphView.GetNode(data.toStateName);

            if (fromNode == null || toNode == null)
                return;



            if (data == Selection)
                color = selectedEdgeColor;

            Vector3[] edgePoints = GetTransitionPoints(data);

            DrawTransiton(edgePoints, color, data == CloseTransition, true);
        }

        private void DrawTransiton(Vector3[] edgePoints, Color color, bool bold, bool isDrawArrow)
        {
            Texture2D tex = (Texture2D)Styles.connectionTexture.image;
            float arrowSize = 5f * edgeSizeMultiplier;
            float outlineWidth = 2f * edgeSizeMultiplier;
            float arrowLength = 13f * edgeSizeMultiplier;
            Handles.color = color;
            float width = 10f * edgeSizeMultiplier;

            if (bold)
            {
                arrowSize *= 1.5f;
                width *= 1.5f;
            }

            Handles.DrawAAPolyLine(tex, width, edgePoints[0], edgePoints[1]);
            if (isDrawArrow)
            {
                Vector3 cross = Vector3.Cross((edgePoints[0] - edgePoints[1]).normalized, Vector3.forward);
                DrawArrows(color, cross, edgePoints, isSelf: false, arrowSize, outlineWidth, arrowLength);
            }
        }

        private Vector3[] GetTransitionPoints(StateTransitionData data)
        {
            return GetTransitionPoints(data.fromStateName, data.toStateName);
        }

        private Vector3[] GetTransitionPoints(string fromStateName, string toStateName)
        {
            StateNodeView fromNode = graphView.GetNode(fromStateName);
            StateNodeView toNode = graphView.GetNode(toStateName);

            if (fromNode == null || toNode == null)
                return null;

            float num = 5f * edgeDistanceMultiplier;

            points[0] = GetNodeViewPosition(fromNode);
            points[1] = GetNodeViewPosition(toNode);

            Vector3 cross = Vector3.Cross((points[0] - points[1]).normalized, Vector3.forward);
            points[0] += cross * num;
            points[1] += cross * num;
            return points;
        }

        private Vector3 GetNodeViewPosition(StateNodeView node)
        {
            return graphView.contentViewContainer.transform.matrix.MultiplyPoint(node.GetPosition().center);
        }

        private static void DrawArrows(Color color, Vector3 cross, Vector3[] edgePoints, bool isSelf, float arrowSize, float outlineWidth, float arrowLength)
        {
            Vector3 vector = edgePoints[1] - edgePoints[0];
            Vector3 normalized = vector.normalized;
            Vector3 vector2 = vector * 0.5f + edgePoints[0];
            vector2 -= cross * 0.5f;
            float num = Mathf.Min(arrowLength * 1.5f, (vector2 - edgePoints[0]).magnitude) * 0.66f;
            int num2 = 1;

            for (int i = 0; i < num2; i++)
            {
                Color color2 = color;

                Vector3 center = vector2 + (float)((num2 == 1) ? i : (i - 1)) * num * (isSelf ? cross : normalized);
                DrawArrow(color2, cross, normalized, center, arrowSize, outlineWidth);
            }
        }

        private static void DrawArrow(Color color, Vector3 cross, Vector3 direction, Vector3 center, float arrowSize, float outlineWidth)
        {
            if (Event.current.type == EventType.Repaint)
            {
                Vector3[] array = new Vector3[4];
                array[0] = center + direction * arrowSize;
                array[1] = center - direction * arrowSize + cross * arrowSize;
                array[2] = center - direction * arrowSize - cross * arrowSize;
                array[3] = array[0];
                Shader.SetGlobalColor("_HandleColor", color);
                Handles.color = color;
                Handles.DrawAAPolyLine((Texture2D)Styles.connectionTexture.image, outlineWidth, array);
                Handles.DrawAAConvexPolygon(array);

            }
        }

        public StateTransitionData FindClosestTransition()
        {
            StateTransitionData result = null;
            float num = float.PositiveInfinity;
            Vector3 vector = Event.current.mousePosition;

            List<StateTransitionData> transitions = Global.Instance.GetCurrentShowTransitionData();
            if (transitions == null || transitions.Count == 0)
                return null;

            foreach (var transition in transitions)
            {
                Vector3[] edgePoints = GetTransitionPoints(transition);

                if (edgePoints == null) continue;

                float num2 = float.PositiveInfinity;
                num2 = ((!(edgePoints[0] == edgePoints[1])) ? HandleUtility.DistancePointLine(vector, edgePoints[0], edgePoints[1]) : Vector3.Distance(edgeToSelfOffsetVector + edgePoints[0], vector));
                if (num2 < num && num2 < 10f)
                {
                    num = num2;
                    result = transition;
                }
            }

            return result;
        }

        public void OnTransitionSelection(StateTransitionData transition)
        {
            if (this.Selection != null && transition == null)
            {
                StateTransitionInspectorHelper.Instance.Inspect(Global.Instance.RuntimeStateMachineCore, null);
            }

            this.Selection = transition;       

            if (Selection != null)
            {
                EditorApplication.delayCall -= DelayCallSelection;
                EditorApplication.delayCall += DelayCallSelection;
            }

        }

        private void DrawMakeTransition()
        {
            if (!graphView.isMakeTransition)
                return;

            vectors[0] = GetNodeViewPosition(graphView.MakeTransitionFrom);

            if (graphView.HoverNode != null && !graphView.HoverNode.Data.isBuildInitState)
                vectors[1] = GetNodeViewPosition(graphView.HoverNode);
            else
                vectors[1] = Event.current.mousePosition;

            DrawTransiton(vectors, Color.white, false, true);
        }


        /// <summary>
        /// 绘制当前状态切换的过渡
        /// </summary>
        private void DrawCurrentTransition(StateTransitionData transition)
        {
            if (Global.Instance.StateManager == null)
                return;
            if (Global.Instance.StateManager.Editor_All_StateMachineCores == null)
                return;
            if (Global.Instance.RuntimeStateMachineCore == null)
                return;

            float width = 10f * edgeSizeMultiplier;

            if (CloseTransition == transition)
                width *= 1.5f;

            Vector3[] edgePoints = GetTransitionPoints(transition);

            // 找到当前的过渡

            StateTransition current_transition = null;
            if (Application.isPlaying)
            {
                current_transition = Global
                    .Instance
                    .StateManager
                    .GetRuntimeMachineCore(Global.Instance.RuntimeStateMachineCore)
                    .GetRuntimeMachine(Global.Instance.LayerParent).CurrentTransition;
            }
            if (current_transition == null)
            {
                //lastRealTime = Time.realtimeSinceStartup;
                return;
            }
            if (!transition.ToString().Equals(current_transition.TransitionData.ToString())) return;

            if (current_change_transition != current_transition.TransitionData)
            {
                current_change_transition = current_transition.TransitionData;
                current_change_transition_timer = 0;
            }

            if (current_change_transition.ToString().Equals(current_transition.TransitionData.ToString()) && current_change_transition_timer >= 1)
            {
                return;
            }

            StateNodeData from = Global.Instance.RuntimeStateMachineCore.GetCurrentNodeData(Global.Instance.LayerParent,current_change_transition.fromStateName);

            if (from == null || from.parentStateMachineName != Global.Instance.LayerParent)
            {
                current_change_transition_timer = 1;
                
                return;
            }


            current_change_transition_timer += deltaTime * 3;

            if (edgePoints != null && edgePoints.Length >= 2)
            {
                Handles.color = selectedEdgeColor;
                Handles.DrawAAPolyLine(width, edgePoints[0], Vector2.Lerp(edgePoints[0], edgePoints[1], current_change_transition_timer));
            }

        }

        private void DrawStateExitTransition()
        {
            if (Event.current.type != EventType.Repaint)
                return;

            if (!Application.isPlaying) return;
            if (Global.Instance.StateManager == null)
                return;
            if (Global.Instance.StateManager.Editor_All_StateMachineCores == null)
                return;
            if (Global.Instance.RuntimeStateMachineCore == null)
                return;

            float width = 10f * edgeSizeMultiplier;
            
            StateBase stateNode = Global
                .Instance
                .StateManager
                .GetRuntimeMachineCore(Global.Instance.RuntimeStateMachineCore)
                .GetCurrentMachineStateInfo(Global.Instance.LayerParent);

            if (currentState != null && stateNode == null)
            {
                state_exiting = true;
                exit_state_parent = currentState.Runtime_StateData.parentStateMachineName;
                exit_state_name = currentState.Name;
                exit_state_timer = 0;

                List<StateNodeData> nodes = Global.Instance.GetCurrentShowStateNodeData();

                if (nodes != null)
                {
                    foreach (var item in nodes)
                    {
                        if (item.isBuildInitState && item.buildStateName.Equals(StateMachineConst.up))
                        {
                            exit_state_to_name = item.name;
                            break;
                        }
                    }
                }

            }

            if (state_exiting)
            {
                exit_state_timer += Time.unscaledDeltaTime * 3;

                Handles.color = selectedEdgeColor;
                Vector3[] edgePoints = GetTransitionPoints(exit_state_name, exit_state_to_name);

                if (edgePoints != null)
                    Handles.DrawAAPolyLine(width, edgePoints[0], Vector2.Lerp(edgePoints[0], edgePoints[1], exit_state_timer));

                if (exit_state_timer >= 1 || !exit_state_parent.Equals(Global.Instance.LayerParent))
                {
                    state_exiting = false;
                    exit_state_parent = null;
                    exit_state_name = null;
                    exit_state_to_name = null;
                    exit_state_timer = 0;
                }
            }
            currentState = stateNode;

        }

        private void DelayCallSelection()
        {
            StateTransitionInspectorHelper.Instance.Inspect(Global.Instance.RuntimeStateMachineCore, Selection);
        }

    }
}
#endif