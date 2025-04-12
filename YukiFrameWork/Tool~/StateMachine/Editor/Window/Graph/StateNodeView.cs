///=====================================================
/// - FileName:      StateNodeView.cs
/// - NameSpace:     YukiFrameWork.Machine
/// - Description:   高级定制脚本生成
/// - Creation Time: 2025/3/9 23:07:48
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
namespace YukiFrameWork.Machine
{
  
    public class StateNodeView : Node
    {
        public StateNodeData Data { get; private set; }


        private StateGraphView graphView = null;

        private GUIStyle labelStyle = null;

        private float runingTimer;


        public StateNodeView(StateNodeData data, StateGraphView graphView)
        {
            this.Data = data;
            this.graphView = graphView;
            Clear();
            VisualElement image = new VisualElement();
            image.style.width = 200;
            image.style.height = 40;
            IMGUIContainer container = new IMGUIContainer();
            container.onGUIHandler += () =>
            {

                if (Global.Instance.RuntimeStateMachineCore == null)
                    return;

                if (labelStyle == null)
                {
                    labelStyle = new GUIStyle(GUI.skin.label);
                    labelStyle.alignment = TextAnchor.MiddleCenter;
                    labelStyle.fontSize = 15;
                    labelStyle.padding.top = -5;
                }

                GUI.Label(container.layout, string.Empty, GetStateStyle(selected));
                GUI.Label(container.layout, Data.DisPlayName, labelStyle);

                DrawRuning(container.layout);
            };

            container.StretchToParentSize();

            //image.name = "node-border"; 
            image.style.borderRightWidth = 0;
            image.style.borderLeftWidth = 0;
            image.style.borderTopWidth = 0;
            image.style.borderBottomWidth = 0;

            image.Add(container);
            Add(image);           
            RegisterCallback<MouseDownEvent>(OnMouseDownEvent);         
        }    
        public override void SetPosition(Rect newPos)
        {

            if (Vector2.Distance(newPos.position, GetPosition().position) < 10)
                return;

            // 实际位置 
            newPos.Set((int)(newPos.x / 20) * 20, (int)(newPos.y / 20) * 20, StateMachineConst.StateNodeWith, StateMachineConst.StateNodeHeight);

            // 显示位置
            base.SetPosition(new Rect(newPos.x - 2, newPos.y - 2, newPos.width, newPos.height));

            Data.position = newPos;
            if (Global.Instance.RuntimeStateMachineCore != null)
                Global.Instance.RuntimeStateMachineCore.Save();

        }


        public void ShowMenu(ContextualMenuPopulateEvent evt)
        {

            bool is_any = Data.IsAnyState;
            bool is_entry = Data.IsEntryState;

            if (!is_entry && !Data.IsUpState)
            {
                evt.menu.AppendAction("添加 过渡", (a) =>
                {
                    graphView.StartMakeTransition(this);
                });
            }
            else
            {
                evt.menu.AppendAction("添加 过渡", null, DropdownMenuAction.Status.Disabled);
            }

            if (!is_entry && !is_any && !Data.IsUpState)
            {

                if (Data.IsDefaultState)
                {
                    evt.menu.AppendAction("将这个状态设置为该层级的默认状态", null, DropdownMenuAction.Status.Disabled);
                }
                else
                {
                    evt.menu.AppendAction("将这个状态设置为该层级的默认状态", (a) =>
                    {
                        //SetDefaultState(node); 
                        SetDefaultState();
                        graphView.RefreshNodes();
                    });
                }

                evt.menu.AppendAction("删除状态", (a) =>
                {
                    DeleteStates();
                });
            }


        }


        private void DeleteStates()
        {

            RuntimeStateMachineCore controller = Global.Instance.RuntimeStateMachineCore;
            if (controller == null)
                return;
            foreach (var item in graphView.StateNodes.Values)
            {              
                if (item.selected)
                        StateNodeFactory.DeleteState(controller, item.Data,Global.Instance.LayerParent);
            }

            graphView.RefreshNodes();
        }

        private void SetDefaultState()
        {
            List<StateNodeData> states = Global.Instance.GetCurrentShowStateNodeData();
            if (states != null)
            {
                foreach (var item in states)
                {
                    item.IsDefaultState = false;
                }
            }
            this.Data.IsDefaultState = true;
            Global.Instance.RuntimeStateMachineCore.Save();
        }


        private GUIStyle GetStateStyle(bool selected)
        {
            if (Data.IsDefaultState)
            {

                if (Data.IsSubStateMachine)
                    return StyleTool.Get(selected ? Style.OrangeOnHEX : Style.OrangeHEX);

                return StyleTool.Get(selected ? Style.OrangeOn : Style.Orange);
            }
            else if (Data.IsEntryState)
            {
                if (Data.IsSubStateMachine)
                    return StyleTool.Get(selected ? Style.GreenOnHEX : Style.GreenHEX);

                return StyleTool.Get(selected ? Style.GreenOn : Style.Green);
            }
            else if (Data.IsAnyState)
            {
                if (Data.IsSubStateMachine)
                    return StyleTool.Get(selected ? Style.MintOnHEX : Style.MintHEX);

                return StyleTool.Get(selected ? Style.MintOn : Style.Mint);
            }
            else if (Data.IsUpState)
            {
                return StyleTool.Get(selected ? Style.RedOnHEX : Style.RedHEX);
            }

            else
            {
                if (Data.IsSubStateMachine)
                    return StyleTool.Get(selected ? Style.NormalOnHEX : Style.NormalHEX);

                return StyleTool.Get(selected ? Style.NormalOn : Style.Normal);
            }

        }

        public override void OnSelected()
        {
            base.OnSelected();
            EditorApplication.delayCall -= DelayCallSelection;
            EditorApplication.delayCall += DelayCallSelection;
        }

        private void DelayCallSelection()
        {
            StateInspectorHelper.Instance.Inspect(Global.Instance.RuntimeStateMachineCore, Data, graphView);
        }

        public override void OnUnselected()
        {
            base.OnUnselected();
            StateInspectorHelper.Instance.Inspect(Global.Instance.RuntimeStateMachineCore, null, graphView);
        }


        private void DrawRuning(Rect rect)
        {

            StateBase currentState = GetCurrentState();

            if (currentState == null) return;

            if (Event.current.type != EventType.Repaint || (!(currentState.Name == Data.name)))
                return;

            rect.Set(15, 28, rect.width - 30, 20);

            runingTimer = Time.realtimeSinceStartup % 1;

            GUIStyle gUIStyle = "MeLivePlayBackground";
            GUIStyle gUIStyle2 = "MeLivePlayBar";
            rect = gUIStyle.margin.Remove(rect);
            Rect rect2 = gUIStyle.padding.Remove(rect);

            rect2.width *= runingTimer;

            gUIStyle2.Draw(rect2, isHover: false, isActive: false, on: false, hasKeyboardFocus: false);
            gUIStyle.Draw(rect, isHover: false, isActive: false, on: false, hasKeyboardFocus: false);
        }

        private StateBase GetCurrentState()
        {
            if (!Application.isPlaying)
                return null;

            if (Global.Instance.StateManager == null) return null;
            if (Global.Instance.StateManager.Editor_All_StateMachineCores == null) return null;
            if (Global.Instance.RuntimeStateMachineCore == null) return null;

            // 如果在运行时 正在运行的状态是orange
            StateBase current_state_node = Global
                .Instance
                .StateManager
                .GetRuntimeMachineCore(Global.Instance.RuntimeStateMachineCore)
                .GetCurrentMachineStateInfo(Global.Instance.LayerParent);

            return current_state_node;

        }


        public void OnMouseDownEvent(MouseDownEvent evt)
        {
            if (evt != null && evt.pressedButtons == 1 && evt.clickCount == 1)
            {

                if (graphView.isMakeTransition)
                    graphView.StopMakeTransition(this);

                //graphView.selector.DragStateNodeStart();
                DelayCallSelection();
            }

            if (evt != null && evt.pressedButtons == 1 && evt.clickCount == 2)
            {
                if (Data.IsSubStateMachine)
                {
                    Global.Instance.AddLayer(Data.name);
                    graphView.RefreshNodes();
                }

                if (Data.IsUpState)
                {                   
                    Global.Instance.RemoveLast(Data.parentStateMachineName);
                    graphView.RefreshNodes();
                }

            }

        }


    }
}
#endif