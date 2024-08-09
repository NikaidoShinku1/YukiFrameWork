#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace YukiFrameWork.ActionStates
{
    internal static class Styles
    {
        public static readonly GUIStyle breadCrumbLeft = (GUIStyle)"GUIEditor.BreadcrumbLeft";
        public static readonly GUIStyle breadCrumbMid = (GUIStyle)"GUIEditor.BreadcrumbMid";
        public static readonly GUIStyle breadCrumbLeftBg = (GUIStyle)"GUIEditor.BreadcrumbLeftBackground";
        public static readonly GUIStyle breadCrumbMidBg = (GUIStyle)"GUIEditor.BreadcrumbMidBackground";

        /// 默认状态和被选中状态的皮肤
        public static readonly GUIStyle defaultAndSelectStyle = "flow node 5 on";
        /// 默认状态和当前执行状态经过的皮肤
        public static readonly GUIStyle defaultAndRuntimeIndexStyle = "flow node 5 on";
        /// 默认状态的皮肤
        public static readonly GUIStyle stateInDefaultStyle = "flow node 5";
        /// 状态执行经过的每个状态所显示的皮肤
        public static readonly GUIStyle indexInRuntimeStyle = "flow node 2 on";
        /// 当点击选择状态的皮肤
        public static readonly GUIStyle selectStateStyle = "flow node 0 on";
        /// 空闲状态的皮肤
        public static readonly GUIStyle defaultStyle = "flow node 0";

        /// 默认状态和被选中状态的皮肤
        public static readonly GUIStyle defaultAndSelectStyleSpecial = "flow node hex 5 on";
        /// 默认状态和当前执行状态经过的皮肤
        public static readonly GUIStyle defaultAndRuntimeIndexStyleSpecial = "flow node hex 5 on";
        /// 默认状态的皮肤
        public static readonly GUIStyle stateInDefaultStyleSpecial = "flow node hex 5";
        /// 状态执行经过的每个状态所显示的皮肤
        public static readonly GUIStyle indexInRuntimeStyleSpecial = "flow node hex 2 on";
        /// 当点击选择状态的皮肤
        public static readonly GUIStyle selectStateStyleSpecial = "flow node hex 0 on";
        /// 空闲状态的皮肤
        public static readonly GUIStyle defaultStyleSpecial = "flow node hex 0";
    }

    internal class StateLayer
    {
        internal string name;
        internal IStateMachine stateMachine;
    }

    public class StateMachineWindow : GraphEditor
    {
        public static StateMachineView support;
        public static IStateMachine stateMachine { get => support.editStateMachine; set => support.editStateMachine = (StateMachineCore)value; }
        private bool dragState = false;
        private State makeTransition;
        internal static Transition selectTransition;
        private const float DoubleClickTimeThreshold = 0.3f; // 双击时间间隔阈值
        private float lastClickTime = 0f;
        private static readonly List<StateLayer> layers = new List<StateLayer>();

        [MenuItem("YukiFrameWork/StateMachine",false,-1)]
        public static void ShowWindow()
        {
            GetWindow<StateMachineWindow>(StateMachineSetting.EditorWindow, true);
        }
        public static void ShowWindow(StateMachineView support)
        {
            GetWindow<StateMachineWindow>(StateMachineSetting.EditorWindow, true);
            Init(support);
        }
        public static void Init(StateMachineView support)
        {
            StateMachineWindow.support = support;
            layers.Clear();
            if (support != null)
            {
                support.UpdateEditStateMachine(0);
                layers.Add(new StateLayer()
                {
                    name = stateMachine.name,
                    stateMachine = stateMachine,
                });
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            lastClickTime = Time.realtimeSinceStartup;
        }

        private void BreadCrumb(int index, string name)
        {
            var style = index == 0 ? Styles.breadCrumbLeft : Styles.breadCrumbMid;
            var guiStyle = index == 0 ? Styles.breadCrumbLeftBg : Styles.breadCrumbMidBg;
            var content = new GUIContent(name);
            var vector2 = style.CalcSize(content);
            var rect = GUILayoutUtility.GetRect(content, style, GUILayout.Height(20), GUILayout.MaxWidth(vector2.x));
            if (Event.current.type == EventType.Repaint)
                guiStyle.Draw(rect, GUIContent.none, 0);
            if (GUI.Button(rect, content, style))
            {
                stateMachine = layers[index].stateMachine;              
                support.UpdateEditStateMachine(stateMachine.Id);
                layers.RemoveRange(index + 1, layers.Count - 1 - index);
            }
        }

        private void OnGUI()
        {
            GUILayout.BeginHorizontal();
            for (int i = 0; i < layers.Count; i++)
                BreadCrumb(i, layers[i].name);
            GUILayout.FlexibleSpace();
            GUILayout.Space(10);
            support = (StateMachineView)EditorGUILayout.ObjectField(string.Empty, support, typeof(StateMachineView), true, GUILayout.Width(150));
            if (GUILayout.Button("刷新脚本", EditorStyles.toolbarButton, GUILayout.Width(60)))
            {
                StateMachineViewEditor.OnScriptReload();
                Debug.Log("刷新脚本成功!");
            }
            if (GUILayout.Button(StateMachineSetting.Reset, EditorStyles.toolbarButton, GUILayout.Width(50)))
            {
                if (support == null)
                    return;
                if (stateMachine.States.Length > 0)
                    UpdateScrollPosition(stateMachine.States[0].rect.position - new Vector2(position.size.x / 2 - 75, position.size.y / 2 - 15)); //更新滑动矩阵
                else
                    UpdateScrollPosition(Center); //归位到矩形的中心
            }
            GUILayout.EndHorizontal();
            ZoomableAreaBegin(new Rect(0f, 0f, scaledCanvasSize.width, scaledCanvasSize.height + 21), scale, false);
            BeginWindow();
            if (support != null)
                DrawStates();
            else
                layers.Clear();
            EndWindow();
            ZoomableAreaEnd();
            if (support == null)
                CreateStateMachineMenu();
            else if (openStateMenu)
                OpenStateContextMenu(stateMachine.SelectState);
            else
                OpenWindowContextMenu();
            Repaint();
        }

        private void CreateStateMachineMenu()
        {
            if (currentType == EventType.MouseDown & Event.current.button == 1)
            {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent(StateMachineSetting.CreateStateMachine), false, delegate
                {
                    var go = Selection.activeGameObject;
                    if (go == null)
                    {
                        EditorUtility.DisplayDialog(
                             StateMachineSetting.Tips,
                             StateMachineSetting.PleaseSelectObjectToCreateStateMachine,
                             StateMachineSetting.Yes,
                            StateMachineSetting.No);
                        return;
                    }
                    if (!go.TryGetComponent<StateManager>(out var manager))
                        manager = go.AddComponent<StateManager>();
                    var support = StateMachineMono.CreateSupport();
                    manager.support = support;
                    support.transform.SetParent(manager.transform);
                    support.OnScriptReload();
                    Init(support);
                });
                menu.ShowAsContext();
                Event.current.Use();
            }
        }

        /// <summary>
        /// 绘制状态(状态的层,状态窗口举行)
        /// </summary>
        protected void DrawStates()
        {
            foreach (var state in stateMachine.States)
            {
                DrawLineStatePosToMousePosTransition(state);
                foreach (var t in state.transitions)
                {
                    if (selectTransition == t)
                    {
                        DrawConnection(state.rect.center, t.NextState.rect.center, Color.green, 1, true);
                        if (Event.current.keyCode == KeyCode.Delete)
                        {
                            ArrayExtend.Remove(ref state.transitions, t);
                            for (int i = 0; i < state.transitions.Length; i++)
                                state.transitions[i].ID = i;
                            return;
                        }
                        ClickTransition(state, t);
                    }
                    else
                    {
                        DrawConnection(state.rect.center, t.NextState.rect.center, Color.white, 1, true);
                        ClickTransition(state, t);
                    }
                }
                if (state.rect.Contains(Event.current.mousePosition) & currentType == EventType.MouseDown & Event.current.button == 0)
                {
                    var isSubStateMachine = false;
                    float clickTime = Time.realtimeSinceStartup;
                    if ((clickTime - lastClickTime) < DoubleClickTimeThreshold)
                    {
                        if (state.Type == StateType.SubStateMachine)
                        {
                            layers.Add(new StateLayer()
                            {
                                name = state.name,
                                stateMachine = state.subStateMachine,
                            });
                            stateMachine = state.subStateMachine;
                            support.UpdateEditStateMachine(stateMachine.Id);
                            isSubStateMachine = true;
                        }
                        else if (state.Type == StateType.Parent)
                        {
                            var index = layers.Count - 2;
                            if (index <= -1)
                            {
                                Debug.LogError("无法跳转，已经是根状态机!");
                                return;
                            }
                            stateMachine = layers[index].stateMachine;
                            support.UpdateEditStateMachine(stateMachine.Id);
                            layers.RemoveRange(index + 1, layers.Count - 1 - index);
                        }
                    }
                    else
                    {
                        if (Event.current.control)
                            stateMachine.SelectState = state;
                        else if (!stateMachine.SelectStates.Contains(state.ID))
                            stateMachine.SelectStates = new List<int> { state.ID };
                        if (state.transitions.Length == 0)
                            selectTransition = null;
                        else
                            selectTransition = state.transitions[0];
                    }
                    lastClickTime = clickTime;
                    if (isSubStateMachine)
                        return;
                }
                else if (state.rect.Contains(mousePosition) & currentType == EventType.MouseDown & currentEvent.button == 1)
                {
                    openStateMenu = true;
                    stateMachine.SelectState = state;
                }
                if (currentEvent.keyCode == KeyCode.Delete & currentEvent.type == EventType.KeyUp)
                {
                    DeletedState();
                    return;
                }
            }
            foreach (var state in stateMachine.States)
                DrawStateBox(state);
            DragSelectStates();
        }

        private void DrawStateBox(State state)
        {
            GUIStyle style;
            if (state == stateMachine.DefaultState & stateMachine.SelectState == stateMachine.DefaultState)
                style = state.Type == StateType.None ? Styles.defaultAndSelectStyle : Styles.defaultAndSelectStyleSpecial;
            else if (state == stateMachine.DefaultState & state.ID == stateMachine.StateId & Application.isPlaying)
                style = state.Type == StateType.None ? Styles.defaultAndRuntimeIndexStyle : Styles.defaultAndRuntimeIndexStyleSpecial;
            else if (state == stateMachine.DefaultState)
                style = state.Type == StateType.None ? Styles.stateInDefaultStyle : Styles.stateInDefaultStyleSpecial;
            else if (stateMachine.StateId == state.ID && Application.isPlaying && state.IsPlaying)
                style = state.Type == StateType.None ? Styles.indexInRuntimeStyle : Styles.indexInRuntimeStyleSpecial;
            else if (state == stateMachine.SelectState)
                style = state.Type == StateType.None ? Styles.selectStateStyle : Styles.selectStateStyleSpecial;
            else
                style = state.Type == StateType.None ? Styles.defaultStyle : Styles.defaultStyleSpecial;
            DragStateBox(state.rect, state.name, style);
        }

        private void DragSelectStates()
        {
            for (int i = 0; i < stateMachine.SelectStates.Count; i++)
            {
                var state = stateMachine.States[stateMachine.SelectStates[i]];
                DragStateBox(state.rect, state.name, state.Type == StateType.None ? Styles.selectStateStyle : Styles.selectStateStyleSpecial);
            }
            switch (currentType)
            {
                case EventType.MouseDown:
                    selectionStartPosition = mousePosition;
                    if (currentEvent.button == 2 | currentEvent.button == 1)
                    {
                        mode = SelectMode.None;
                        return;
                    }
                    foreach (State state in stateMachine.States)
                    {
                        if (state.rect.Contains(currentEvent.mousePosition))
                        {
                            mode = SelectMode.Drag;
                            return;
                        }
                    }
                    mode = SelectMode.DragEnd;
                    break;
                case EventType.MouseUp:
                    mode = SelectMode.None;
                    break;
            }
            switch (mode)
            {
                case SelectMode.DragEnd:
                    var rect = FromToRect(selectionStartPosition, mousePosition);
                    GUI.Box(rect, string.Empty, "SelectionRect");
                    SelectStatesInRect(rect);
                    break;
            }
        }

        private void SelectStatesInRect(Rect r)
        {
            var states = stateMachine.States;
            for (int i = 0; i < states.Length; i++)
            {
                var state = states[i];
                var rect = state.rect;
                if (rect.xMax < r.x || rect.x > r.xMax || rect.yMax < r.y || rect.y > r.yMax)
                {
                    stateMachine.SelectStates.Remove(state.ID);
                    continue;
                }
                if (!stateMachine.SelectStates.Contains(state.ID))
                {
                    stateMachine.SelectStates.Add(state.ID);
                }
                DragStateBox(state.rect, state.name, state.Type == StateType.None ? Styles.selectStateStyle : Styles.selectStateStyleSpecial);
            }
        }

        private Rect FromToRect(Vector2 start, Vector2 end)
        {
            var rect = new Rect(start.x, start.y, end.x - start.x, end.y - start.y);
            if (rect.width < 0f)
            {
                rect.x += rect.width;
                rect.width = -rect.width;
            }
            if (rect.height < 0f)
            {
                rect.y += rect.height;
                rect.height = -rect.height;
            }
            return rect;
        }

        /// <summary>
        /// 点击连接线条
        /// </summary>

        protected void ClickTransition(State state, Transition t)
        {
            if (state.rect.Contains(mousePosition) | t.NextState.rect.Contains(mousePosition))
                return;
            if (currentType == EventType.MouseDown)
            {
                bool offset = state.ID > t.NextState.ID;
                Vector3 start = state.rect.center;
                Vector3 end = t.NextState.rect.center;
                Vector3 cross = Vector3.Cross((start - end).normalized, Vector3.forward);
                if (offset)
                {
                    start += cross * 6;
                    end += cross * 6;
                }
                if (HandleUtility.DistanceToLine(start, end) < 8f)//返回到线的距离
                {
                    selectTransition = t;
                    stateMachine.SelectState = state;
                }
            }
        }

        /// <summary>
        /// 绘制一条从状态点到鼠标位置的线条
        /// </summary>

        protected void DrawLineStatePosToMousePosTransition(State state)
        {
            if (state == null)
                return;
            if (makeTransition == state)
            {
                var startpos = new Vector2(state.rect.x + 80, state.rect.y + 15);
                var endpos = currentEvent.mousePosition;
                DrawConnection(startpos, endpos, Color.white, 1, true);
                if (currentEvent.button == 0 & currentType == EventType.MouseDown)
                {
                    foreach (var s in stateMachine.States)
                    {
                        if (state != s & s.rect.Contains(mousePosition))
                        {
                            foreach (var t in state.transitions)
                            {
                                if (t.NextState == s)// 如果拖动的线包含在自身状态盒矩形内,则不添加连接线
                                {
                                    makeTransition = null;
                                    return;
                                }
                            }
                            Transition.CreateTransitionInstance(state, s);
                            break;
                        }
                    }
                    makeTransition = null;
                }
            }
        }

        /// <summary>
        /// 右键打开状态菜单
        /// </summary>
        protected void OpenStateContextMenu(State state)
        {
            if (state == null)
            {
                openStateMenu = false;
                return;
            }
            if (currentType == EventType.MouseDown & currentEvent.button == 0)
            {
                openStateMenu = false;
            }
            else if (currentType == EventType.MouseDown & currentEvent.button == 1)
            {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent(StateMachineSetting.CreateTransition), false, () => makeTransition = state);
                if (state.Type == StateType.None)
                {
                    menu.AddSeparator(string.Empty);
                    menu.AddItem(new GUIContent(StateMachineSetting.DefaultState), false, () => stateMachine.DefaultState = state);
                }
                menu.AddItem(new GUIContent(StateMachineSetting.DeletedState), false, DeletedState);
                menu.ShowAsContext();
                Event.current.Use();
            }
        }

        /// <summary>
        /// 删除状态节点
        /// </summary>
        private void DeletedState()
        {
            foreach (var state in stateMachine.States)
            {
                for (int n = 0; n < state.transitions.Length; n++)
                {
                    if (state.transitions[n].NextState == null)
                        continue;
                    if (stateMachine.SelectStates.Contains(state.transitions[n].NextState.ID))
                        ArrayExtend.RemoveAt(ref state.transitions, n);
                }
            }
            var ids = new List<int>();
            foreach (var i in stateMachine.SelectStates)
                ids.Add(stateMachine.States[i].ID);
            while (ids.Count > 0)
            {
                for (int i = 0; i < stateMachine.States.Length; i++)
                {
                    if (stateMachine.States[i].ID == ids[0])
                    {
                        stateMachine.States = ArrayExtend.RemoveAt(stateMachine.States, i);
                        EditorUtility.SetDirty(stateMachine.View);
                        break;
                    }
                }
                ids.RemoveAt(0);
            }
            stateMachine.UpdateStates();
            stateMachine.SelectStates.Clear();
            selectTransition = null;
        }

        /// <summary>
        /// 右键打开窗口菜单
        /// </summary>

        protected void OpenWindowContextMenu()
        {
            if (support == null)
                return;
            if (currentType == EventType.MouseDown && currentEvent.button == 1)
            {
                foreach (State state in stateMachine.States)
                {
                    if (state.rect.Contains(currentEvent.mousePosition))
                        return;
                }
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent(StateMachineSetting.CreateState), false, () =>
                {
                    State.AddNode(stateMachine, StateMachineSetting.NewState + stateMachine.States.Length, mousePosition);
                });
                menu.AddItem(new GUIContent("创建子状态机"), false, () =>
                {
                    var state = State.AddSubStateMachine(stateMachine, "子状态机" + stateMachine.States.Length, mousePosition);
                    support.OnScriptReload();
                });
                menu.AddItem(new GUIContent("创建返回状态"), false, () =>
                {
                    State.AddParent(stateMachine, "返回状态" + stateMachine.States.Length, mousePosition);
                });
                if (stateMachine.SelectState != null)
                {
                    menu.AddItem(new GUIContent(StateMachineSetting.PasteSelectionStatus), false, () =>
                    {
                        var states = new List<State>();
                        var seles = stateMachine.SelectStates;
                        var s = CloneHelper.DeepCopy<State>(stateMachine.States[seles[0]], new List<Type>() { typeof(Object), typeof(StateMachineCore) });
                        s.perID = s.ID;
                        s.ID = stateMachine.States.Length;
                        s.rect.center = mousePosition;
                        stateMachine.States = ArrayExtend.Add(stateMachine.States, s);
                        states.Add(s);
                        var dis = stateMachine.States[seles[0]].rect.center - mousePosition;
                        for (int i = 1; i < stateMachine.SelectStates.Count; ++i)
                        {
                            var ss = CloneHelper.DeepCopy<State>(stateMachine.States[seles[i]], new List<Type>() { typeof(Object), typeof(StateMachineCore) });
                            ss.perID = ss.ID;
                            ss.ID = stateMachine.States.Length;
                            ss.rect.position -= dis;
                            stateMachine.States = ArrayExtend.Add(stateMachine.States, ss);
                            states.Add(ss);
                        }
                        foreach (var state in states)
                            foreach (var tran in state.transitions)
                                foreach (var sta in states)
                                    if (tran.nextStateID == sta.perID)
                                        tran.nextStateID = sta.ID;
                        stateMachine.UpdateStates();
                        var list = new List<int>();
                        for (int i = 0; i < states.Count; ++i)
                            list.Add(states[i].ID);
                        stateMachine.SelectStates = list;
                    });
                    menu.AddItem(new GUIContent(StateMachineSetting.DeleteSelectionState), false, DeletedState);
                }
                menu.AddSeparator("");
                menu.AddItem(new GUIContent(StateMachineSetting.CreateAndReplaceStateMachines), false, () =>
                {
                    var go = Selection.activeGameObject;
                    if (go == null)
                    {
                        EditorUtility.DisplayDialog(
                           StateMachineSetting.Tips,
                           StateMachineSetting.PleaseSelectObjectToCreateStateMachine,
                           StateMachineSetting.Yes,
                          StateMachineSetting.No);
                        return;
                    }
                    if (!go.TryGetComponent<StateManager>(out var manager))
                        manager = go.AddComponent<StateManager>();
                    else if (manager.support != null)
                        DestroyImmediate(manager.support.gameObject, true);
                    var support = StateMachineMono.CreateSupport();
                    manager.support = support;
                    support.transform.SetParent(manager.transform);
                    support.OnScriptReload();
                    Init(support);
                });
                menu.AddItem(new GUIContent(StateMachineSetting.CreateAndReplaceStateMachines), false, () =>
                {
                    var go = Selection.activeGameObject;
                    if (go == null)
                    {
                        EditorUtility.DisplayDialog(
                            StateMachineSetting.Tips,
                            StateMachineSetting.PleaseSelectObjectToCreateStateMachine,
                            StateMachineSetting.Yes,
                           StateMachineSetting.No);
                        return;
                    }
                    if (!go.TryGetComponent<StateManager>(out var manager))
                        manager = go.AddComponent<StateManager>();
                    var support = StateMachineMono.CreateSupport(StateMachineSetting.NewStateMachine);
                    manager.support = support;
                    support.transform.SetParent(manager.transform);
                    support.OnScriptReload();
                    Init(support);
                });
                menu.AddItem(new GUIContent(StateMachineSetting.DeleteStateMachine), false, () =>
                {
                    if (support == null)
                        return;
                    Undo.DestroyObjectImmediate(support.gameObject);
                });
                menu.ShowAsContext();
                Event.current.Use();
            }
        }

        protected Rect DragStateBox(Rect dragRect, string name, GUIStyle style = null, int eventButton = 0)
        {
            GUI.Box(dragRect, name, style);
            if (Event.current.button == eventButton)
            {
                switch (Event.current.rawType)
                {
                    case EventType.MouseDown:
                        if (dragRect.Contains(Event.current.mousePosition))
                            dragState = true;
                        break;
                    case EventType.MouseDrag:
                        if (dragState)
                        {
                            foreach (var state in stateMachine.SelectStates)
                                stateMachine.States[state].rect.position += Event.current.delta;//拖到状态按钮
                        }
                        Event.current.Use();
                        break;
                    case EventType.MouseUp:
                        dragState = false;
                        break;
                }
            }
            return dragRect;
        }
    }
}
#endif