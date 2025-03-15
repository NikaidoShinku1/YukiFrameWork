///=====================================================
/// - FileName:      StateNodeGraphView.cs
/// - NameSpace:     YukiFrameWork.Machine
/// - Description:   高级定制脚本生成
/// - Creation Time: 2025/3/8 20:11:23
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.EventSystems;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.UIElements;

using UnityEditor.Experimental.GraphView;

namespace YukiFrameWork.Machine
{
    public class StateGraphView :GraphView
    {
        public Dictionary<string, StateNodeView> StateNodes = new Dictionary<string, StateNodeView>();

        internal MachineTransitionBackground transition = null;

        internal MachineRectangleSelector selector;

        public bool isMakeTransition = false;

        public StateNodeView MakeTransitionFrom = null;

        public StateNodeView HoverNode = null;

        private RuntimeStateMachineCore controller = null;

        //private StateNodeView selectNode = null;

        public StateGraphView()
        {
            BackGroundGraphView background = new BackGroundGraphView();
            Insert(0, background);

            transition = new MachineTransitionBackground();
            Insert(1, transition);
            //
            selector = new MachineRectangleSelector();
            Insert(3, selector);

            RegisterCallback<KeyDownEvent>(KeyDownControl);
            RegisterCallback<MouseDownEvent>(MouseDownControl);

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new MachineSelectionDragger());
            ContentZoomer zoomer = new ContentZoomer();
            zoomer.minScale = 0.1f;
            zoomer.maxScale = 3;
          
            this.AddManipulator(zoomer);

            RefreshView();
        }

        private void RefreshView()
        {
            if (Global.Instance.RuntimeStateMachineCore != null)
            {
                // 默认位置和缩放
                viewTransform.position = Global.Instance.RuntimeStateMachineCore.viewPosition;
                viewTransform.scale = Global.Instance.RuntimeStateMachineCore.viewScale;
            }
            controller = Global.Instance.RuntimeStateMachineCore;
            RefreshNodes();
        }

        private void SaveScaleAndPosition()
        {

            if (viewTransform == null || Global.Instance.RuntimeStateMachineCore == null)
                return;


            if (Global.Instance.RuntimeStateMachineCore.viewScale != viewTransform.scale)
            {
                Global.Instance.RuntimeStateMachineCore.viewScale = viewTransform.scale;
                Global.Instance.RuntimeStateMachineCore.Save();
            }


            if (Global.Instance.RuntimeStateMachineCore.viewPosition != viewTransform.position)
            {
                Global.Instance.RuntimeStateMachineCore.viewPosition = viewTransform.position;
                Global.Instance.RuntimeStateMachineCore.Save();
            }

        }

        public void Update()
        {
            if (controller != Global.Instance.RuntimeStateMachineCore)
            {
                RefreshView();
                controller = Global.Instance.RuntimeStateMachineCore;
            }

            SaveScaleAndPosition();
        }

        public void RefreshNodes()
        {

            foreach (var item in StateNodes.Values)
            {
                RemoveElement(item);
            }

            StateNodes.Clear();

            List<StateNodeData> datas = Global.Instance.GetCurrentShowStateNodeData();

            if (datas == null || datas.Count == 0)
                return;

            //selectNode = null;

            foreach (var item in datas)
            {
                StateNodeView node = new StateNodeView(item, this);
                node.title = item.DisPlayName;
                node.SetPosition(item.position);
                this.AddElement(node);
                StateNodes.Add(item.name, node);

                if (GetPrefsSelection(item.name))
                {
                    AddToSelection(node);
                }
            }


        }

        public void RenameState(string oldName, string newName)
        {         
            if (StateNodes.ContainsKey(newName))
                return;
            if (!StateNodes.ContainsKey(oldName))
                return;
            StateNodes.Add(newName, StateNodes[oldName]);
            StateNodes.Remove(oldName);
        }


        public StateNodeView GetNode(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            if (StateNodes.ContainsKey(name))
                return StateNodes[name];

            return null;
        }


        private void KeyDownControl(KeyDownEvent evt)
        {            
            if (evt.keyCode == KeyCode.Delete)
            {
                RuntimeStateMachineCore controller = Global.Instance.RuntimeStateMachineCore;
                if (controller == null) return;

                // 删除选中的过渡
                if (transition.Selection != null && controller != null)
                    StateTransitionFactory.DeleteTransition(controller, transition.Selection);

                RefreshNodes();
            }
        }

        private void MouseDownControl(MouseDownEvent evt)
        {
            //BehaviourTreeView.TreeWindow.WindowRoot.InspectorView.UpdateInspector();
            if (evt.button == 0 && evt.clickCount == 1)
            {
                // 左键单击
                transition.OnTransitionSelection(transition.CloseTransition);
                StopMakeTransition(null);
            }
        }


        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {

            StateNodeView node = evt.target as StateNodeView;

            if (node != null)
                transition.OnTransitionSelection(null);


            int count = evt.menu.MenuItems().Count;
            for (int i = 0; i < count; i++)
            {
                evt.menu.RemoveItemAt(0);
            }

            RuntimeStateMachineCore controller = Global.Instance.RuntimeStateMachineCore;

            bool disable = controller == null || EditorApplication.isPlaying;

            DropdownMenuAction.Status status = disable ? DropdownMenuAction.Status.Disabled : DropdownMenuAction.Status.Normal;

            if (node == null)
            {              
                evt.menu.AppendAction("新建 空状态", CreateState, (d) => status, Event.current.mousePosition);
                evt.menu.AppendAction("新建 子状态机", CreateSubStateMachine, (d) => status, Event.current.mousePosition);
            }
            else
            {
                node.ShowMenu(evt);
            }           
        }


        public void CreateState(DropdownMenuAction action)
        {
            Vector2 mousePosition = (Vector2)(action.userData);
            mousePosition.Set(mousePosition.x - parent.layout.x, mousePosition.y - layout.y - 20);
            Vector2 position = contentViewContainer.transform.matrix.inverse.MultiplyPoint(mousePosition);
            CreateState(false, position);
        }

        public void CreateSubStateMachine(DropdownMenuAction action)
        {
            Vector2 mousePosition = (Vector2)(action.userData);
            mousePosition.Set(mousePosition.x - parent.layout.x, mousePosition.y - layout.y - 20);
            Vector2 position = contentViewContainer.transform.matrix.inverse.MultiplyPoint(mousePosition);
            CreateState(true, position);
        }


        private void CreateState(bool isSubStateMachine, Vector3 mousePosition)
        {

            Rect rect = new Rect(0, 0, StateMachineConst.StateNodeWith, StateMachineConst.StateNodeHeight);

            rect.center = mousePosition;

            List<StateNodeData> states = Global.Instance.GetCurrentShowStateNodeData();

            bool isDefaultState = false ;

            if (states != null)
            {
                bool isAllBuildIn = true;
                foreach (var item in states)
                {
                    if (!item.isBuildInitState)
                    {
                        isAllBuildIn = false;
                        break;
                    }
                }
                if (isAllBuildIn) isDefaultState = true;
            }


            RuntimeStateMachineCore controller = Global.Instance.RuntimeStateMachineCore;
         
            StateNodeData state = StateNodeFactory.CreateStateNode(controller,Global.Instance.LayerParent, rect, isDefaultState, isSubStateMachine);

            if (isSubStateMachine)
            {
                // 给当前子状态机创建 Entry 状态
                Rect r = new Rect(0, 300, StateMachineConst.StateNodeWith, StateMachineConst.StateNodeHeight);
                CreateSubState(controller, StateMachineConst.entryState, state, r, StateMachineConst.entryState);
                // 给当前子状态机创建 Any 状态
                r = new Rect(0, 100, StateMachineConst.StateNodeWith, StateMachineConst.StateNodeHeight);
                CreateSubState(controller, StateMachineConst.anyState, state, r, StateMachineConst.anyState);
                // 给当前子状态机创建 Entry 状态
                r = new Rect(600, 300, StateMachineConst.StateNodeWith, StateMachineConst.StateNodeHeight);
                CreateSubState(controller, StateMachineConst.up, state, r, StateMachineConst.up);
            }


            RefreshNodes();
        }

        private void CreateSubState(RuntimeStateMachineCore controller,string name, StateNodeData state,  Rect rect, string buildInName)
        {
            // 创建三个内置状态                   
            StateNodeFactory.CreateStateNode(controller,name, state.name, rect, false, false,  true, buildInName);
        }


        public void StartMakeTransition(StateNodeView node)
        {
            isMakeTransition = true;
            MakeTransitionFrom = node;
        }

        public void StopMakeTransition(StateNodeView node)
        {
            if (node == MakeTransitionFrom)
                return;

            isMakeTransition = false;
            HoverNode = null;

            if (node == null)
                return;

            RuntimeStateMachineCore controller = Global.Instance.RuntimeStateMachineCore;
            if (controller == null)
                return;
            StateTransitionFactory.CreateTransition(controller, MakeTransitionFrom.Data.name, node.Data.name);
        }

        public void OnGUI()
        {

            // 取消拖拽
            if (Event.current.type == EventType.MouseLeaveWindow && Event.current.button == 0)
            {
                selector.CancelDrag();
            }

            if (Event.current.type == EventType.MouseUp && Event.current.button == 0)
            {
                selector.CancelDrag();
            }

        }


        protected override void ExecuteDefaultAction(EventBase evt)
        {
            base.ExecuteDefaultAction(evt);
            selector.ExecuteAction(evt);
        }

        internal static bool GetPrefsSelection(string name)
        {
            string key = PrefsSelectionKey(name);
            if (string.IsNullOrEmpty(key))
                return false;
            bool v = EditorPrefs.GetBool(key, false);
            if (v)
                EditorPrefs.DeleteKey(key);
            return v;
        }

        internal static void SavePrefsSelection(string name)
        {
            string key = PrefsSelectionKey(name);
            if (string.IsNullOrEmpty(key))
                return;
            EditorPrefs.SetBool(key, true);
        }

        internal static string PrefsSelectionKey(string name)
        {
            RuntimeStateMachineCore controller = Global.Instance.RuntimeStateMachineCore;
            if (controller == null)
                return string.Empty;

            return string.Format("Yuki有限状态机:{0}:{1}", controller.name, name);
        }

    }


}

#endif