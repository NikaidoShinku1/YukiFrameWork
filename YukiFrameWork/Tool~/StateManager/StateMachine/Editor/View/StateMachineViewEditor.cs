#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;
using System.Reflection;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;
using System.IO.Pipes;

namespace YukiFrameWork.ActionStates
{
    public abstract class StateMachineViewEditor : Editor
    {
        protected virtual StateMachineView Self { get; set; }
        public string createScriptName
        {
            get => Self == null ? "NewStateBehaviour" : Self.createScriptName;
            set => Self.createScriptName = value;
        }
        public string stateActionScriptPath
        {
            get => Self == null ? "Assets/Scripts/StateManager/Actions" : Self.stateActionScriptPath;
            set => Self.stateActionScriptPath = value;
        }
        public string stateBehaviourScriptPath
        {
            get => Self == null ? "Assets/Scripts/StateManager/StateBehaviours" : Self.stateBehaviourScriptPath;
            set => Self.stateBehaviourScriptPath = value;
        }
        public string transitionScriptPath
        {
            get => Self == null ? "Assets/Scripts/StateManager/Transitions" : Self.transitionScriptPath;
            set => Self.transitionScriptPath = value;
        }
        protected StateBase addBehaviourState;
        protected StateAction animAction;
        private static bool compiling;
        protected static List<Type> findBehaviourTypes;
        protected static List<Type> findBehaviourTypes1;
        protected static List<Type> findBehaviourTypes2;
        protected static bool animPlay;

        private ICodeGenerator ActionGenerator = new ActionGenerator();
        private ICodeGenerator StateGenerator = new StateBehaviourGenerator();
        private ICodeGenerator TransitionGenerator = new TransitionGenerator();
        protected virtual void OnEnable()
        {
            Self = target as StateMachineView;
            Self.EditorInit(Self.transform);
            Self.editStateMachine.View = Self;
            if (string.IsNullOrEmpty(Self.editStateMachine.name))
                Self.editStateMachine.name = "Base Layer";
            if (StateMachineWindow.support == null) //这个是假的“null”
                StateMachineWindow.support = null;
            if (StateMachineWindow.support != Self)
                StateMachineWindow.Init(Self);
            if (findBehaviourTypes == null)
            {
                findBehaviourTypes = new List<Type>();
                AddBehaviourTypes(findBehaviourTypes, typeof(StateBehaviour));
            }
            if (findBehaviourTypes1 == null)
            {
                findBehaviourTypes1 = new List<Type>();
                AddBehaviourTypes(findBehaviourTypes1, typeof(ActionBehaviour));
            }
            if (findBehaviourTypes2 == null)
            {
                findBehaviourTypes2 = new List<Type>();
                AddBehaviourTypes(findBehaviourTypes2, typeof(TransitionBehaviour));
            }       
        }

        private List<string> list => Self.architectures;

        protected void AddBehaviourTypes(List<Type> types, Type type)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                var types1 = assembly.GetTypes().Where(t => t.IsSubclassOf(type) && !t.IsAbstract).ToArray();
                types.AddRange(types1);
            }
        }

        public override void OnInspectorGUI() 
        {
            if (Self == null)
            {
                OpenWindow();
                ResetPropertys();
                return;
            }
            EditorGUI.BeginChangeCheck();
            serializedObject.Update();
            OnDrawPreField();
            OpenWindow();
            var sm = Self.editStateMachine;
            if (sm.SelectState != null)
            {
                DrawState(sm.SelectState);
                EditorGUILayout.Space();
                for (int i = 0; i < sm.SelectState.transitions.Length; ++i)
                    DrawTransition(sm.SelectState.transitions[i]);
            }
            else if (StateMachineWindow.selectTransition != null)
            {
                DrawTransition(StateMachineWindow.selectTransition);
            }
            EditorGUILayout.Space();
            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(Self);
            Repaint();
        J: serializedObject.ApplyModifiedProperties();
        }

        private void OpenWindow()
        {
            if (GUILayout.Button(StateMachineSetting.OpenStateMachineEditor, GUI.skin.GetStyle("LargeButtonMid"), GUILayout.ExpandWidth(true)))
            {
                if (Self != null)
                    Self.OnScriptReload();
                StateMachineWindow.ShowWindow(Self);
            }
        }

        protected virtual void OnDrawPreField()
        {
        }

        private SerializedObject _supportObject;
        protected SerializedObject SupportObject
        {
            get
            {
                if (_supportObject == null)
                    _supportObject = new SerializedObject(Self);
                return _supportObject;
            }
        }

        private SerializedProperty _stateMachineObject;
        protected SerializedProperty StateMachineObject
        {
            get
            {
                if (_stateMachineObject == null)
                    _stateMachineObject = SupportObject.FindProperty("editStateMachine");
                return _stateMachineObject;
            }
        }

        private SerializedProperty _statesProperty;
        protected SerializedProperty StatesProperty
        {
            get
            {
                if (_statesProperty == null)
                    _statesProperty = StateMachineObject.FindPropertyRelative("states").GetArrayElementAtIndex(Self.editStateMachine.SelectState.ID);
                return _statesProperty;
            }
        }

        private SerializedProperty _nameProperty;
        protected SerializedProperty NameProperty
        {
            get
            {
                if (_nameProperty == null)
                    _nameProperty = StatesProperty.FindPropertyRelative("name");
                return _nameProperty;
            }
        }

        private SerializedProperty _actionSystemProperty;
        protected SerializedProperty ActionSystemProperty
        {
            get
            {
                if (_actionSystemProperty == null)
                    _actionSystemProperty = StatesProperty.FindPropertyRelative("actionSystem");
                return _actionSystemProperty;
            }
        }

        private SerializedProperty _animSpeedProperty;
        protected SerializedProperty animSpeedProperty
        {
            get
            {
                if (_animSpeedProperty == null)
                    _animSpeedProperty = StatesProperty.FindPropertyRelative("animSpeed");
                return _animSpeedProperty;
            }
        }

        private SerializedProperty _animLoopProperty;
        protected SerializedProperty animLoopProperty
        {
            get
            {
                if (_animLoopProperty == null)
                    _animLoopProperty = StatesProperty.FindPropertyRelative("animLoop");
                return _animLoopProperty;
            }
        }

        private SerializedProperty _actionsProperty;
        protected SerializedProperty actionsProperty
        {
            get
            {
                if (_actionsProperty == null)
                    _actionsProperty = StatesProperty.FindPropertyRelative("actions");
                return _actionsProperty;
            }
        }

        private static State CurrentState;

        protected virtual void ResetPropertys()
        {
            _supportObject = null;
            _stateMachineObject = null;
            _statesProperty = null;
            _nameProperty = null;
            _actionSystemProperty = null;
            _animSpeedProperty = null;
            _animLoopProperty = null;
            _actionsProperty = null;
        }

        protected virtual void OnDrawAnimationField() { }

        /// <summary>
        /// 绘制状态监视面板属性
        /// </summary>
        public void DrawState(State state)
        {
            if (CurrentState != state)
            {
                CurrentState = state;
                ResetPropertys();
            }
            SupportObject.Update();
            GUILayout.Button(StateMachineSetting.StateAttribute, GUI.skin.GetStyle("dragtabdropwindow"));
            EditorGUILayout.BeginVertical("ProgressBarBack");
            EditorGUILayout.PropertyField(NameProperty, new GUIContent(StateMachineSetting.StatusName, "name"));
            EditorGUILayout.IntField(new GUIContent(StateMachineSetting.StatusIdentifier, "stateID"), state.ID);
            if (state.Type == StateType.SubStateMachine)
            {
                EditorGUILayout.HelpBox("你在此状态连线到其他状态，当子状态机执行返回后，会进入到连线的状态。", MessageType.Info);
                EditorGUILayout.EndVertical();
                goto J;
            }
            if (state.Type == StateType.Parent)
            {
                EditorGUILayout.HelpBox("你在此状态连线到其他状态，当返回到父状态机执行后，会进入到连线的状态。", MessageType.Info);
                EditorGUILayout.EndVertical();
                goto J;
            }
            EditorGUILayout.PropertyField(ActionSystemProperty, new GUIContent(StateMachineSetting.ActionSystem, "actionSystem  专为玩家角色AI其怪物AI所设计的一套AI系统！"));
            if (state.actionSystem)
            {
                OnDrawAnimationField();
                state.animPlayMode = (AnimPlayMode)EditorGUILayout.Popup(new GUIContent(StateMachineSetting.ActionExecutionMode, "animPlayMode"), (int)state.animPlayMode, new GUIContent[]{
                    new GUIContent(StateMachineSetting.ActionRandomised,"Random"),
                    new GUIContent(StateMachineSetting.ActionSequence,"Sequence"),
                    new GUIContent(StateMachineSetting.ActionNone,"Code")
                });
                EditorGUILayout.PropertyField(animSpeedProperty, new GUIContent(StateMachineSetting.AnimationSpeed, "animSpeed"), true);
                EditorGUILayout.PropertyField(animLoopProperty, new GUIContent(StateMachineSetting.AnimationCycle, "animLoop"), true);
                state.isCrossFade = EditorGUILayout.Toggle(new GUIContent(StateMachineSetting.IsCrossFade, "isCrossFade"), state.isCrossFade);
                if (state.isCrossFade)
                    state.duration = EditorGUILayout.FloatField(new GUIContent(StateMachineSetting.Duration, "duration"), state.duration);
                state.isExitState = EditorGUILayout.Toggle(new GUIContent(StateMachineSetting.ExitStatusAtEndOfAction, "isExitState"), state.isExitState);
                if (state.isExitState)
                    state.DstStateID = EditorGUILayout.Popup(StateMachineSetting.GetIntoTheState, state.DstStateID, Array.ConvertAll(state.transitions.ToArray(), new Converter<Transition, string>(delegate (Transition t) { return t.CurrState.name + " -> " + t.NextState.name + "   ID:" + t.NextState.ID; })));
                BlueprintGUILayout.BeginStyleVertical(StateMachineSetting.ActionTree, "ProgressBarBack");
                EditorGUI.indentLevel = 1;
                var actRect = EditorGUILayout.GetControlRect();
                state.foldout = EditorGUI.Foldout(new Rect(actRect.position, new Vector2(actRect.size.x - 120f, 15)), state.foldout,StateMachineSetting.ActionTreeSet, true);
                if (GUI.Button(new Rect(new Vector2(actRect.size.x - 40f, actRect.position.y), new Vector2(60, 16)),StateMachineSetting.AddAction))
                {
                    ArrayExtend.Add(ref state.actions, new StateAction() { ID = state.ID, stateMachine = Self.editStateMachine, behaviours = new BehaviourBase[0] });
                    return;
                }
                if (GUI.Button(new Rect(new Vector2(actRect.size.x - 100, actRect.position.y), new Vector2(60, 16)), StateMachineSetting.RemoveAction))
                {
                    if (state.actions.Length > 1)
                        ArrayExtend.RemoveAt(ref state.actions, state.actions.Length - 1);
                    return;
                }
                if (state.foldout)
                {
                    EditorGUI.indentLevel = 2;
                    for (int x = 0; x < state.actions.Length; ++x)
                    {
                        var actionProperty = actionsProperty.GetArrayElementAtIndex(x);
                        if (actionProperty == null)
                            continue;
                        var act = state.actions[x];
                        var foldoutRect = EditorGUILayout.GetControlRect();
                        act.foldout = EditorGUI.Foldout(foldoutRect, act.foldout, new GUIContent(StateMachineSetting.ActionNext + x, "actions[" + x + "]"), true);
                        if (foldoutRect.Contains(Event.current.mousePosition) & Event.current.button == 1)
                        {
                            var menu = new GenericMenu();
                            menu.AddItem(new GUIContent(StateMachineSetting.RemoveAction), false, (obj) =>
                            {
                                ArrayExtend.RemoveAt(ref state.actions, (int)obj);
                            }, x);
                            menu.AddItem(new GUIContent(StateMachineSetting.CopyAction), false, (obj) =>
                            {
                                StateSystem.Component = state.actions[(int)obj];
                            }, x);
                            menu.AddItem(new GUIContent(StateMachineSetting.PasteNewAction), StateSystem.CopyComponent != null, () =>
                            {
                                if (StateSystem.Component is StateAction stateAction)
                                    ArrayExtend.Add(ref state.actions, CloneHelper.DeepCopy<StateAction>(stateAction, new List<Type>() { typeof(Object), typeof(StateMachineCore) }));
                            });
                            menu.AddItem(new GUIContent(StateMachineSetting.PasteActionValue), StateSystem.CopyComponent != null, (obj) =>
                            {
                                if (StateSystem.Component is StateAction stateAction)
                                {
                                    var index = (int)obj;
                                    if (stateAction == state.actions[index])//如果要黏贴的动作是复制的动作则返回
                                        return;
                                    state.actions[index] = CloneHelper.DeepCopy<StateAction>(stateAction, new List<Type>() { typeof(Object), typeof(StateMachineCore) });
                                }
                            }, x);
                            menu.ShowAsContext();
                        }
                        if (act.foldout)
                        {
                            EditorGUI.indentLevel = 3;
                            act.clipIndex = EditorGUILayout.Popup(new GUIContent(StateMachineSetting.MovieClips, "clipIndex"), act.clipIndex, Array.ConvertAll(Self.ClipNames.ToArray(), input => new GUIContent(input)));
                            if (Self.ClipNames.Count > 0 && act.clipIndex < Self.ClipNames.Count)
                                act.clipName = Self.ClipNames[act.clipIndex];
                            OnDrawActionPropertyField(actionProperty);
                            EditorGUILayout.PropertyField(actionProperty.FindPropertyRelative("animTime"), new GUIContent(StateMachineSetting.AnimationTime, "animTime"));
                            EditorGUILayout.PropertyField(actionProperty.FindPropertyRelative("animTimeMax"), new GUIContent(StateMachineSetting.AnimationLength, "animTimeMax"));
                            EditorGUILayout.PropertyField(actionProperty.FindPropertyRelative("layer"), new GUIContent(StateMachineSetting.AnimationLayer, "layer"));
                            for (int i = 0; i < act.behaviours.Length; ++i)
                            {
                                EditorGUILayout.BeginHorizontal();
                                Rect rect = EditorGUILayout.GetControlRect();
                                act.behaviours[i].show = EditorGUI.Foldout(new Rect(rect.x, rect.y, 50, rect.height), act.behaviours[i].show, GUIContent.none);
                                act.behaviours[i].Active = EditorGUI.ToggleLeft(new Rect(rect.x + 5, rect.y, 70, rect.height), GUIContent.none, act.behaviours[i].Active);
                                EditorGUI.LabelField(new Rect(rect.x + 20, rect.y, rect.width - 15, rect.height), act.behaviours[i].name, GUI.skin.GetStyle("BoldLabel"));
                                if (GUI.Button(new Rect(rect.x + rect.width - 15, rect.y, rect.width, rect.height), GUIContent.none, GUI.skin.GetStyle("ToggleMixed")))
                                {
                                    act.behaviours[i].OnDestroy();
                                    ArrayExtend.RemoveAt(ref act.behaviours, i);
                                    continue;
                                }
                                if (rect.Contains(Event.current.mousePosition) & Event.current.button == 1)
                                {
                                    var menu = new GenericMenu();
                                    menu.AddItem(new GUIContent(StateMachineSetting.RemoveActionScripts), false, (obj) =>
                                    {
                                        var index = (int)obj;
                                        act.behaviours[index].OnDestroy();
                                        ArrayExtend.RemoveAt(ref act.behaviours, index);
                                        return;
                                    }, i);
                                    menu.AddItem(new GUIContent(StateMachineSetting.CopyActionScripts), false, (obj) =>
                                    {
                                        var index = (int)obj;
                                        StateSystem.CopyComponent = act.behaviours[index];
                                    }, i);
                                    menu.AddItem(new GUIContent(StateMachineSetting.PasteNewActionScripts), StateSystem.CopyComponent != null, () =>
                                    {
                                        if (StateSystem.CopyComponent is ActionBehaviour behaviour)
                                        {
                                            ActionBehaviour ab = (ActionBehaviour)CloneHelper.DeepCopy(behaviour);
                                            ArrayExtend.Add(ref act.behaviours, ab);
                                        }
                                    });
                                    menu.AddItem(new GUIContent(StateMachineSetting.PasteActionScriptValues), StateSystem.CopyComponent != null, (obj) =>
                                    {
                                        if (StateSystem.CopyComponent is ActionBehaviour behaviour)
                                        {
                                            var index = (int)obj;
                                            if (behaviour.name == act.behaviours[index].name)
                                                act.behaviours[index] = (ActionBehaviour)CloneHelper.DeepCopy(StateSystem.CopyComponent);
                                        }
                                    }, i);
                                    menu.AddItem(new GUIContent(StateMachineSetting.EditActionScripts), false, (obj) =>
                                    {
                                        var index = (int)obj;
                                        var scriptName = act.behaviours[index].name;

                                        try
                                        {                                         
                                            var script = AssetDatabase.GetAllAssetPaths()
                                            .Where(x => x.EndsWith(".cs"))
                                            .Select(AssetDatabase.LoadAssetAtPath<MonoScript>)
                                            .Where(x => x && x.GetClass() != null && x.GetClass().FullName == scriptName).FirstOrDefault();
                                            if (script == null)
                                            {
                                                Debug.LogError("希望打开的脚本可能没有指定的MonoScript文件，只能打开具有单独文件且类型相同的脚本编辑");
                                                return;
                                            }
                                            AssetDatabase.OpenAsset(script);
                                        }
                                        catch
                                        {
                                            
                                        }
                                    }, i);
                                    menu.ShowAsContext();
                                }
                                EditorGUILayout.EndHorizontal();
                                if (act.behaviours[i].show)
                                {
                                    EditorGUI.indentLevel = 4;
                                    if (!act.behaviours[i].OnInspectorGUI(state))
                                        foreach (var metadata in act.behaviours[i].Metadatas)
                                            PropertyField(metadata, 60f, 5, 4);
                                    GUILayout.Space(4);
                                    GUILayout.Box("", BlueprintSetting.Instance.HorSpaceStyle, GUILayout.Height(1), GUILayout.ExpandWidth(true));
                                    GUILayout.Space(4);
                                    EditorGUI.indentLevel = 3;
                                }
                            }
                            OnPlayAnimation(act);
                            var r = EditorGUILayout.GetControlRect();
                            var rr = new Rect(new Vector2(r.x + (r.size.x / 4f), r.y), new Vector2(r.size.x / 2f, 20));
                            if (GUI.Button(rr, StateMachineSetting.AddActionScripts))
                                addBehaviourState = act;
                            if (addBehaviourState == act)
                            {
                                EditorGUILayout.Space();
                                try
                                {
                                    foreach (var type in findBehaviourTypes1)
                                    {
                                        if (GUILayout.Button(type.Name))
                                        {
                                            var stb = (ActionBehaviour)Activator.CreateInstance(type);
                                            stb.InitMetadatas();
                                            stb.ID = state.ID;
                                            ArrayExtend.Add(ref act.behaviours, stb);
                                            addBehaviourState = null;
                                            EditorUtility.SetDirty(Self);
                                        }
                                        if (compiling & type.Name == createScriptName)
                                        {
                                            var stb = (ActionBehaviour)Activator.CreateInstance(type);
                                            stb.InitMetadatas();
                                            stb.ID = state.ID;
                                            ArrayExtend.Add(ref act.behaviours, stb);
                                            addBehaviourState = null;
                                            compiling = false;
                                            EditorUtility.SetDirty(Self);
                                        }
                                    }
                                }
                                catch { }
                                EditorGUILayout.Space();
                                var rect = EditorGUILayout.BeginVertical();
                                EditorGUI.indentLevel = 0;                              
                                if (list.Count > 0)
                                {
                                    EditorGUILayout.BeginHorizontal();
                                    GUILayout.Label(StateMachineSetting.SelectArchitecture);
                                    Self.architectureIndex = EditorGUILayout.Popup(Self.architectureIndex, list.ToArray(), GUILayout.Width(350));
                                    EditorGUILayout.EndHorizontal();
                                }
                                EditorGUILayout.LabelField(StateMachineSetting.CreateActionScriptPaths);
                                stateActionScriptPath = EditorGUILayout.TextField(stateActionScriptPath);
                                CodeManager.DragObject(rect, out var newPath);
                                if(!newPath.IsNullOrEmpty())stateActionScriptPath = newPath;
                                var addRect = EditorGUILayout.GetControlRect();
                                createScriptName = EditorGUI.TextField(new Rect(addRect.position, new Vector2(addRect.size.x - 125f, 18)), createScriptName);
                                if (GUI.Button(new Rect(new Vector2(addRect.size.x - 100f, addRect.position.y), new Vector2(120, 18)), StateMachineSetting.CreateActionScripts))
                                {
                                    CreateActionScript(createScriptName,stateActionScriptPath,Self.architectureIndex);
                                    compiling = true;
                                }
                                if (GUILayout.Button(StateMachineSetting.Cancel))
                                    addBehaviourState = null;
                                EditorGUILayout.EndVertical();
                            }
                            EditorGUILayout.Space();
                        }
                        EditorGUI.indentLevel = 2;
                    }
                }
                BlueprintGUILayout.EndStyleVertical();
            }
            EditorGUILayout.Space();
            DrawBehaviours(state);
            EditorGUILayout.Space();
            EditorGUILayout.EndVertical();
            J: SupportObject.ApplyModifiedProperties();
        }

        protected virtual void OnPlayAnimation(StateAction action)
        {
        }

        protected virtual void OnDrawActionPropertyField(SerializedProperty actionProperty)
        {
        }

        /// <summary>
        /// 绘制状态行为
        /// </summary>
        public void DrawBehaviours(State s)
        {
            GUILayout.Space(10);
            GUILayout.Box("", BlueprintSetting.Instance.HorSpaceStyle, GUILayout.Height(1), GUILayout.ExpandWidth(true));
            GUILayout.Space(5);
            for (int i = 0; i < s.behaviours.Length; ++i)
            {
                EditorGUI.indentLevel = 1;
                EditorGUILayout.BeginHorizontal();
                var rect = EditorGUILayout.GetControlRect();
                s.behaviours[i].show = EditorGUI.Foldout(new Rect(rect.x, rect.y, 20, rect.height), s.behaviours[i].show, GUIContent.none);
                s.behaviours[i].Active = EditorGUI.ToggleLeft(new Rect(rect.x + 5, rect.y, 30, rect.height), GUIContent.none, s.behaviours[i].Active);
                EditorGUI.LabelField(new Rect(rect.x + 20, rect.y, rect.width - 15, rect.height), s.behaviours[i].name, GUI.skin.GetStyle("BoldLabel"));
                if (GUI.Button(new Rect(rect.x + rect.width - 15, rect.y, rect.width, rect.height), GUIContent.none, GUI.skin.GetStyle("ToggleMixed")))
                {
                    s.behaviours[i].OnDestroy();
                    ArrayExtend.RemoveAt(ref s.behaviours, i);
                    continue;
                }
                if (rect.Contains(Event.current.mousePosition) & Event.current.button == 1)
                {
                    var menu = new GenericMenu();
                    menu.AddItem(new GUIContent(StateMachineSetting.RemoveStatusScripts), false, (obj) =>
                    {
                        var index = (int)obj;
                        s.behaviours[index].OnDestroy();
                        ArrayExtend.RemoveAt(ref s.behaviours, index);
                        return;
                    }, i);
                    menu.AddItem(new GUIContent(StateMachineSetting.CopyStatusScript), false, (obj) =>
                    {
                        var index = (int)obj;
                        StateSystem.CopyComponent = s.behaviours[index];
                    }, i);
                    menu.AddItem(new GUIContent(StateMachineSetting.PasteNewStatusScript), StateSystem.CopyComponent != null, delegate ()
                    {
                        if (StateSystem.CopyComponent is StateBehaviour behaviour)
                        {
                            var ab = (StateBehaviour)CloneHelper.DeepCopy(behaviour);
                            ArrayExtend.Add(ref s.behaviours, ab);
                        }
                    });
                    menu.AddItem(new GUIContent(StateMachineSetting.PasteStatusScriptValues), StateSystem.CopyComponent != null, (obj) =>
                    {
                        if (StateSystem.CopyComponent is StateBehaviour behaviour)
                        {
                            var index = (int)obj;
                            if (behaviour.name == s.behaviours[index].name)
                                s.behaviours[index] = (StateBehaviour)CloneHelper.DeepCopy(behaviour);
                        }
                    }, i);
                    menu.AddItem(new GUIContent(StateMachineSetting.EditStatusScript), false, (obj) =>
                    {
                        var index = (int)obj;
                        var scriptName = s.behaviours[index].name;
                        try
                        {
                            var script = AssetDatabase.GetAllAssetPaths()
                            .Where(x => x.EndsWith(".cs"))
                            .Select(AssetDatabase.LoadAssetAtPath<MonoScript>)
                            .Where(x => x && x.GetClass() != null && x.GetClass().FullName == scriptName).FirstOrDefault();
                            if (script == null)
                            {
                                Debug.LogError("希望打开的脚本可能没有指定的MonoScript文件，只能打开具有单独文件且类型相同的脚本编辑");
                                return;
                            }
                            AssetDatabase.OpenAsset(script);
                        }
                        catch
                        {

                        }
                    }, i);
                    menu.ShowAsContext();
                }
                EditorGUILayout.EndHorizontal();
                if (s.behaviours[i].show)
                {
                    EditorGUI.indentLevel = 2;
                    if (!s.behaviours[i].OnInspectorGUI(s))
                    {
                        foreach (var metadata in s.behaviours[i].Metadatas)
                        {
                            PropertyField(metadata);
                        }
                    }
                    EditorGUI.indentLevel = 1;
                    GUILayout.Space(4);
                    GUILayout.Box("", BlueprintSetting.Instance.HorSpaceStyle, GUILayout.Height(1), GUILayout.ExpandWidth(true));
                }
            }

            var r = EditorGUILayout.GetControlRect();
            var rr = new Rect(new Vector2(r.x + (r.size.x / 4f), r.y), new Vector2(r.size.x / 2f, 20));
            if (GUI.Button(rr,StateMachineSetting.AddingStatusScripts))
                addBehaviourState = s;
            if (addBehaviourState == s)
            {
                try
                {
                    EditorGUILayout.Space();
                    foreach (var type in findBehaviourTypes)
                    {
                        if (GUILayout.Button(type.Name))
                        {
                            var stb = (StateBehaviour)Activator.CreateInstance(type);
                            stb.InitMetadatas();
                            stb.ID = s.ID;
                            ArrayExtend.Add(ref s.behaviours, stb);
                            addBehaviourState = null;
                            EditorUtility.SetDirty(Self);
                        }
                        if (compiling & type.Name == createScriptName)
                        {
                            var stb = (StateBehaviour)Activator.CreateInstance(type);
                            stb.InitMetadatas();
                            stb.ID = s.ID;
                            ArrayExtend.Add(ref s.behaviours, stb);
                            addBehaviourState = null;
                            compiling = false;
                            EditorUtility.SetDirty(Self);
                        }
                    }
                }
                catch { }
                EditorGUILayout.Space();
                EditorGUI.indentLevel = 0;
                if (list.Count > 0)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label(StateMachineSetting.SelectArchitecture);
                    Self.architectureIndex = EditorGUILayout.Popup(Self.architectureIndex, list.ToArray(), GUILayout.Width(350));
                    EditorGUILayout.EndHorizontal();
                }
                var rect =  EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField(StateMachineSetting.CreateStatusScriptPath);
                stateBehaviourScriptPath = EditorGUILayout.TextField(stateBehaviourScriptPath);
                CodeManager.DragObject(rect, out var newPath);
                if(!newPath.IsNullOrEmpty())stateBehaviourScriptPath = newPath;
                var addRect = EditorGUILayout.GetControlRect();
                createScriptName = EditorGUI.TextField(new Rect(addRect.position, new Vector2(addRect.size.x - 125f, 18)), createScriptName);
                if (GUI.Button(new Rect(new Vector2(addRect.size.x - 105f, addRect.position.y), new Vector2(120, 18)), StateMachineSetting.CreateStatusScripts))
                {
                    CreateBehaviourScript(createScriptName, stateBehaviourScriptPath,Self.architectureIndex);                  
                    compiling = true;
                }
                if (GUILayout.Button(StateMachineSetting.Cancel))
                    addBehaviourState = null;

                EditorGUILayout.EndVertical();
            }
        }

        private void CreateActionScript(string fileName, string filePath, int selectIndex)
        {
            if (!IsCheckGenerator(fileName, filePath, out string targetPath)) return;
            FrameworkConfigInfo info = Resources.Load<FrameworkConfigInfo>("FrameworkConfigInfo");
            using (FileStream stream = new FileStream(targetPath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                StringBuilder builder = ActionGenerator.BuildFile(fileName, info.nameSpace, selectIndex, list);
                StreamWriter sw = new StreamWriter(stream, Encoding.UTF8);
                sw.Write(builder);
                sw.Close();
                stream.Close();              
                AssetDatabase.Refresh();
            }
        }

        /// <summary>
        /// 创建脚本文件
        /// </summary>
        private void CreateBehaviourScript(string fileName, string filePath, int selectIndex)
        {
            if (!IsCheckGenerator(fileName, filePath, out string targetPath)) return;
            FrameworkConfigInfo info = Resources.Load<FrameworkConfigInfo>("FrameworkConfigInfo");
            using (FileStream fileStream = new FileStream(targetPath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                StringBuilder builder = StateGenerator.BuildFile(fileName, info.nameSpace, selectIndex, list);
                StreamWriter sw = new StreamWriter(fileStream, Encoding.UTF8);
                sw.Write(builder);
                sw.Close();
                fileStream.Close();
                AssetDatabase.Refresh();

            }
        }

        private void CreateTransitionScript(string fileName, string filePath, int selectIndex)
        {
            if (!IsCheckGenerator(fileName, filePath, out string targetPath)) return;
            FrameworkConfigInfo info = Resources.Load<FrameworkConfigInfo>("FrameworkConfigInfo");
            using (FileStream fileStream = new FileStream(targetPath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                StringBuilder builder = TransitionGenerator.BuildFile(fileName, info.nameSpace, selectIndex, list);
                StreamWriter sw = new StreamWriter(fileStream, Encoding.UTF8);
                sw.Write(builder);
                sw.Close();
                fileStream.Close();
                AssetDatabase.Refresh();

            }
        }

        private bool IsCheckGenerator(string fileName,string filePath,out string targetPath)
        {
            string regix = "^[a-zA-Z][a-zA-Z0-9_]*$";
            targetPath = string.Empty;
            if (!Regex.Match(fileName, regix).Success)
            {
                Debug.LogError("文件名称含有不对的字符，无法作为文件名生成请重试！");
                return false;
            }
            targetPath = filePath + "/" + fileName + ".cs";
            if (!Directory.Exists(targetPath))
            {
                Directory.CreateDirectory(filePath);
                AssetDatabase.Refresh();
            }

            int i = 1;
            while (File.Exists(targetPath))
            {
                fileName += i;
                targetPath = filePath + fileName + ".cs";
            }

            return true;
        }      

        private static void PropertyField(Metadata metadata, float width = 40f, int arrayBeginSpace = 3, int arrayEndSpace = 2)
        {
            if (metadata.type == TypeCode.Byte)
                metadata.value = (byte)EditorGUILayout.IntField(metadata.name, (byte)metadata.value);
            else if (metadata.type == TypeCode.SByte)
                metadata.value = (sbyte)EditorGUILayout.IntField(metadata.name, (sbyte)metadata.value);
            else if (metadata.type == TypeCode.Boolean)
                metadata.value = EditorGUILayout.Toggle(metadata.name, (bool)metadata.value);
            else if (metadata.type == TypeCode.Int16)
                metadata.value = (short)EditorGUILayout.IntField(metadata.name, (short)metadata.value);
            else if (metadata.type == TypeCode.UInt16)
                metadata.value = (ushort)EditorGUILayout.IntField(metadata.name, (ushort)metadata.value);
            else if (metadata.type == TypeCode.Char)
                metadata.value = EditorGUILayout.TextField(metadata.name, metadata.value.ToString()).ToCharArray();
            else if (metadata.type == TypeCode.Int32)
                metadata.value = EditorGUILayout.IntField(metadata.name, (int)metadata.value);
            else if (metadata.type == TypeCode.UInt32)
                metadata.value = (uint)EditorGUILayout.IntField(metadata.name, (int)metadata.value);
            else if (metadata.type == TypeCode.Single)
                metadata.value = EditorGUILayout.FloatField(metadata.name, (float)metadata.value);
            else if (metadata.type == TypeCode.Int64)
                metadata.value = EditorGUILayout.LongField(metadata.name, (long)metadata.value);
            else if (metadata.type == TypeCode.UInt64)
                metadata.value = (ulong)EditorGUILayout.LongField(metadata.name, (long)metadata.value);
            else if (metadata.type == TypeCode.Double)
                metadata.value = EditorGUILayout.DoubleField(metadata.name, (double)metadata.value);
            else if (metadata.type == TypeCode.String)
                metadata.value = EditorGUILayout.TextField(metadata.name, (string)metadata.value);
            else if (metadata.type == TypeCode.Enum)
                metadata.value = EditorGUILayout.EnumPopup(metadata.name, (Enum)metadata.value);
            else if (metadata.type == TypeCode.Vector2)
                metadata.value = EditorGUILayout.Vector2Field(metadata.name, (Vector2)metadata.value);
            else if (metadata.type == TypeCode.Vector3)
                metadata.value = EditorGUILayout.Vector3Field(metadata.name, (Vector3)metadata.value);
            else if (metadata.type == TypeCode.Vector4)
                metadata.value = EditorGUILayout.Vector4Field(metadata.name, (Vector4)metadata.value);
            else if (metadata.type == TypeCode.Quaternion)
            {
                var q = (Quaternion)metadata.value;
                var value = EditorGUILayout.Vector4Field(metadata.name, new Vector4(q.x, q.y, q.z, q.w));
                var q1 = new Quaternion(value.x, value.y, value.z, value.w);
                metadata.value = q1;
            }
            else if (metadata.type == TypeCode.Rect)
                metadata.value = EditorGUILayout.RectField(metadata.name, (Rect)metadata.value);
            else if (metadata.type == TypeCode.Color)
                metadata.value = EditorGUILayout.ColorField(metadata.name, (Color)metadata.value);
            else if (metadata.type == TypeCode.Color32)
                metadata.value = (Color32)EditorGUILayout.ColorField(metadata.name, (Color32)metadata.value);
            else if (metadata.type == TypeCode.AnimationCurve)
                metadata.value = EditorGUILayout.CurveField(metadata.name, (AnimationCurve)metadata.value);
            else if (metadata.type == TypeCode.Object)
                metadata.value = EditorGUILayout.ObjectField(metadata.name, (Object)metadata.value, metadata.Type, true);
            else if (metadata.type == TypeCode.GenericType | metadata.type == TypeCode.Array)
            {
                var rect = EditorGUILayout.GetControlRect();
                rect.x += width;
                metadata.foldout = EditorGUI.BeginFoldoutHeaderGroup(rect, metadata.foldout, metadata.name);
                if (metadata.foldout)
                {
                    EditorGUI.indentLevel = arrayBeginSpace;
                    EditorGUI.BeginChangeCheck();
                    var arraySize = EditorGUILayout.DelayedIntField("Size", metadata.arraySize);
                    bool flag8 = EditorGUI.EndChangeCheck();
                    IList list = (IList)metadata.value;
                    if (flag8 | list.Count != metadata.arraySize)
                    {
                        metadata.arraySize = arraySize;
                        IList list1 = Array.CreateInstance(metadata.itemType, arraySize);
                        for (int i = 0; i < list1.Count; i++)
                            if (i < list.Count)
                                list1[i] = list[i];
                        if (metadata.type == TypeCode.GenericType)
                        {
                            IList list2 = (IList)Activator.CreateInstance(metadata.Type);
                            for (int i = 0; i < list1.Count; i++)
                                list2.Add(list1[i]);
                            list = list2;
                        }
                        else list = list1;
                    }
                    for (int i = 0; i < list.Count; i++)
                        list[i] = PropertyField("Element " + i, list[i], metadata.itemType);
                    metadata.value = list;
                    EditorGUI.indentLevel = arrayEndSpace;
                }
                EditorGUI.EndFoldoutHeaderGroup();
            }
        }

        private static object PropertyField(string name, object obj, Type type)
        {
            var typeCode = (TypeCode)Type.GetTypeCode(type);
            if (typeCode == TypeCode.Byte)
                obj = (byte)EditorGUILayout.IntField(name, (byte)obj);
            else if (typeCode == TypeCode.SByte)
                obj = (sbyte)EditorGUILayout.IntField(name, (sbyte)obj);
            else if (typeCode == TypeCode.Boolean)
                obj = EditorGUILayout.Toggle(name, (bool)obj);
            else if (typeCode == TypeCode.Int16)
                obj = (short)EditorGUILayout.IntField(name, (short)obj);
            else if (typeCode == TypeCode.UInt16)
                obj = (ushort)EditorGUILayout.IntField(name, (ushort)obj);
            else if (typeCode == TypeCode.Char)
                obj = EditorGUILayout.TextField(name, (string)obj).ToCharArray();
            else if (typeCode == TypeCode.Int32)
                obj = EditorGUILayout.IntField(name, (int)obj);
            else if (typeCode == TypeCode.UInt32)
                obj = (uint)EditorGUILayout.IntField(name, (int)obj);
            else if (typeCode == TypeCode.Single)
                obj = EditorGUILayout.FloatField(name, (float)obj);
            else if (typeCode == TypeCode.Int64)
                obj = EditorGUILayout.LongField(name, (long)obj);
            else if (typeCode == TypeCode.UInt64)
                obj = (ulong)EditorGUILayout.LongField(name, (long)obj);
            else if (typeCode == TypeCode.Double)
                obj = EditorGUILayout.DoubleField(name, (double)obj);
            else if (typeCode == TypeCode.String)
                obj = EditorGUILayout.TextField(name, (string)obj);
            else if (type == typeof(Vector2))
                obj = EditorGUILayout.Vector2Field(name, (Vector2)obj);
            else if (type == typeof(Vector3))
                obj = EditorGUILayout.Vector3Field(name, (Vector3)obj);
            else if (type == typeof(Vector4))
                obj = EditorGUILayout.Vector4Field(name, (Vector4)obj);
            else if (type == typeof(Quaternion))
            {
                var value = EditorGUILayout.Vector4Field(name, (Vector4)obj);
                var quaternion = new Quaternion(value.x, value.y, value.z, value.w);
                obj = quaternion;
            }
            else if (type == typeof(Rect))
                obj = EditorGUILayout.RectField(name, (Rect)obj);
            else if (type == typeof(Color))
                obj = EditorGUILayout.ColorField(name, (Color)obj);
            else if (type == typeof(Color32))
                obj = EditorGUILayout.ColorField(name, (Color32)obj);
            else if (type == typeof(AnimationCurve))
                obj = EditorGUILayout.CurveField(name, (AnimationCurve)obj);
            else if (type.IsSubclassOf(typeof(Object)) | type == typeof(Object))
                obj = EditorGUILayout.ObjectField(name, (Object)obj, type, true);
            return obj;
        }

        /// <summary>
        /// 绘制状态连接行为
        /// </summary>
        public void DrawTransition(Transition tr)
        {
            EditorGUI.indentLevel = 0;
            var style = GUI.skin.GetStyle("dragtabdropwindow");
            style.fontStyle = FontStyle.Bold;
            style.font = Resources.Load<Font>("Arial");
            style.normal.textColor = Color.red;
            GUILayout.Button(StateMachineSetting.ConnectionProperties + tr.CurrState.name + " -> " + tr.NextState.name, style);
            tr.name = tr.CurrState.name + " -> " + tr.NextState.name;
            EditorGUILayout.BeginVertical("ProgressBarBack");

            EditorGUILayout.Space();

            tr.mode = (TransitionMode)EditorGUILayout.Popup(StateMachineSetting.ConnectionMode, (int)tr.mode, Enum.GetNames(typeof(TransitionMode)), GUI.skin.GetStyle("PreDropDown"));
            switch (tr.mode)
            {
                case TransitionMode.ExitTime:
                    tr.time = EditorGUILayout.FloatField(StateMachineSetting.CurrentTime, tr.time, GUI.skin.GetStyle("PreDropDown"));
                    tr.exitTime = EditorGUILayout.FloatField(StateMachineSetting.EndTime ,tr.exitTime, GUI.skin.GetStyle("PreDropDown"));
                    EditorGUILayout.HelpBox(StateMachineSetting.CurrentTimeAutoEnterNextState, MessageType.Info);
                    break;
            }

            GUILayout.Space(10);
            GUILayout.Box("", BlueprintSetting.Instance.HorSpaceStyle, GUILayout.Height(1), GUILayout.ExpandWidth(true));
            GUILayout.Space(10);

            tr.isEnterNextState = EditorGUILayout.Toggle(StateMachineSetting.EnterNextState, tr.isEnterNextState);

            GUILayout.Space(10);
            GUILayout.Box("", BlueprintSetting.Instance.HorSpaceStyle, GUILayout.Height(1), GUILayout.ExpandWidth(true));

            for (int i = 0; i < tr.behaviours.Length; ++i)
            {
                if (tr.behaviours[i] == null)
                {
                    ArrayExtend.RemoveAt(ref tr.behaviours, i);
                    continue;
                }
                EditorGUI.indentLevel = 1;
                EditorGUILayout.BeginHorizontal();
                var rect = EditorGUILayout.GetControlRect();
                EditorGUI.LabelField(new Rect(rect.x + 20, rect.y, rect.width - 15, 20), tr.behaviours[i].GetType().Name, GUI.skin.GetStyle("BoldLabel"));
                tr.behaviours[i].show = EditorGUI.Foldout(new Rect(rect.x, rect.y, 20, 20), tr.behaviours[i].show, GUIContent.none, true);
                tr.behaviours[i].Active = EditorGUI.ToggleLeft(new Rect(rect.x + 5, rect.y, 30, 20), GUIContent.none, tr.behaviours[i].Active);
                if (GUI.Button(new Rect(rect.x + rect.width - 15, rect.y, rect.width, rect.height), GUIContent.none, GUI.skin.GetStyle("ToggleMixed")))
                {
                    tr.behaviours[i].OnDestroy();
                    ArrayExtend.RemoveAt(ref tr.behaviours, i);
                    continue;
                }
                if (rect.Contains(Event.current.mousePosition) & Event.current.button == 1)
                {
                    var menu = new GenericMenu();
                    menu.AddItem(new GUIContent(StateMachineSetting.RemoveConnectionScripts), false, (obj) =>
                    {
                        var index = (int)obj;
                        tr.behaviours[index].OnDestroy();
                        ArrayExtend.RemoveAt(ref tr.behaviours, index);
                        return;
                    }, i);
                    menu.AddItem(new GUIContent(StateMachineSetting.CopyConnectionScripts), false, (obj) =>
                    {
                        var index = (int)obj;
                        StateSystem.CopyComponent = tr.behaviours[index];
                    }, i);
                    menu.AddItem(new GUIContent(StateMachineSetting.PasteNewConnectionScript), StateSystem.CopyComponent != null, () =>
                    {
                        if (StateSystem.CopyComponent is TransitionBehaviour behaviour)
                        {
                            TransitionBehaviour ab = (TransitionBehaviour)CloneHelper.DeepCopy(behaviour);
                            ArrayExtend.Add(ref tr.behaviours, ab);
                        }
                    });
                    menu.AddItem(new GUIContent(StateMachineSetting.PasteConnectionScriptValues), StateSystem.CopyComponent != null, (obj) =>
                    {
                        var index = (int)obj;
                        if (StateSystem.CopyComponent is TransitionBehaviour behaviour)
                            if (behaviour.name == tr.behaviours[index].name)
                                tr.behaviours[index] = (TransitionBehaviour)CloneHelper.DeepCopy(behaviour);
                    }, i);
                    menu.AddItem(new GUIContent(StateMachineSetting.EditConnectionScript), false, (obj) =>
                    {
                        var index = (int)obj;
                        var scriptName = tr.behaviours[index].name;
                        var script = AssetDatabase.GetAllAssetPaths()
                                            .Where(x => x.EndsWith(".cs"))
                                            .Select(AssetDatabase.LoadAssetAtPath<MonoScript>)
                                            .Where(x => x && x.GetClass() != null && x.GetClass().FullName == scriptName).FirstOrDefault();
                        if (script == null)
                        {
                            Debug.LogError("希望打开的脚本可能没有指定的MonoScript文件，只能打开具有单独文件且类型相同的脚本编辑");
                            return;
                        }
                        AssetDatabase.OpenAsset(script);
                    }, i);
                    menu.ShowAsContext();
                }
                EditorGUILayout.EndHorizontal();
                if (tr.behaviours[i].show)
                {
                    EditorGUI.indentLevel = 2;
                    if (!tr.behaviours[i].OnInspectorGUI(tr.CurrState))
                    {
                        foreach (var metadata in tr.behaviours[i].Metadatas)
                        {
                            PropertyField(metadata);
                        }
                    }
                    EditorGUI.indentLevel = 1;
                    GUILayout.Space(10);
                    GUILayout.Box("", BlueprintSetting.Instance.HorSpaceStyle, GUILayout.Height(1), GUILayout.ExpandWidth(true));
                }
            }

            GUILayout.Space(5);

            var r = EditorGUILayout.GetControlRect();
            var rr = new Rect(new Vector2(r.x + (r.size.x / 4f), r.y), new Vector2(r.size.x / 2f, 20));
            if (GUI.Button(rr, StateMachineSetting.AddConnectionScripts))
                addBehaviourState = tr;
            if (addBehaviourState == tr)
            {
                EditorGUILayout.Space();
                foreach (var type in findBehaviourTypes2)
                {
                    if (GUILayout.Button(type.Name))
                    {
                        var stb = (TransitionBehaviour)Activator.CreateInstance(type);
                        stb.InitMetadatas();
                        ArrayExtend.Add(ref tr.behaviours, stb);
                        addBehaviourState = null;
                    }
                    if (compiling & type.Name == createScriptName)
                    {
                        var stb = (TransitionBehaviour)Activator.CreateInstance(type);
                        stb.InitMetadatas();
                        ArrayExtend.Add(ref tr.behaviours, stb);
                        addBehaviourState = null;
                        compiling = false;
                    }
                }

                EditorGUILayout.Space();
                if (list.Count > 0)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label(StateMachineSetting.SelectArchitecture);
                    Self.architectureIndex = EditorGUILayout.Popup(Self.architectureIndex, list.ToArray(), GUILayout.Width(350));
                    EditorGUILayout.EndHorizontal();
                }
                var rect = EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField(StateMachineSetting.CreateConnectionScriptPath);                            
                transitionScriptPath = EditorGUILayout.TextField(transitionScriptPath);
                if (CodeManager.DragObject(rect, out string newPath))                
                    transitionScriptPath = newPath;
                
                var addRect = EditorGUILayout.GetControlRect();
                createScriptName = EditorGUI.TextField(new Rect(addRect.position, new Vector2(addRect.size.x - 125f, 18)), createScriptName);
                if (GUI.Button(new Rect(new Vector2(addRect.size.x - 105f, addRect.position.y), new Vector2(120, 18)), StateMachineSetting.CreateConnectionScripts))
                {
                    CreateTransitionScript(createScriptName, transitionScriptPath, Self.architectureIndex);
                    compiling = true;
                }
                if (GUILayout.Button(StateMachineSetting.Cancel))
                    addBehaviourState = null;
                EditorGUILayout.EndVertical();
            }
            GUILayout.Space(10);
            EditorGUILayout.HelpBox(StateMachineSetting.ConnectionBehaviorScriptInfo, MessageType.Info);
            GUILayout.Space(10);
            EditorGUILayout.EndHorizontal();
        }

        [UnityEditor.Callbacks.DidReloadScripts(0)]
        internal static void OnScriptReload()
        {
            var go = Selection.activeGameObject;
            if (go == null)
                return;
            if (!go.TryGetComponent<StateManager>(out var sm))
                return;
            if (sm.support == null)
                return;
            sm.support.OnScriptReload();
        }
    }
}
#endif