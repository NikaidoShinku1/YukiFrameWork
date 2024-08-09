#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using YukiFrameWork.Extension;

namespace YukiFrameWork.ActionStates
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(StateManager))]
    public class StateManagerEditor : StateMachineViewEditor
    {
        private StateManager self;
        protected override StateMachineView Self { get => self.support; set => self.support = (StateMachineMono)value; }

        private SerializedProperty _animationProperty;
        private SerializedProperty animationProperty
        {
            get
            {
                if (_animationProperty == null)
                    _animationProperty = SupportObject.FindProperty("animation");
                return _animationProperty;
            }
        }

        private SerializedProperty _animatorProperty;
        private SerializedProperty animatorProperty
        {
            get
            {
                if (_animatorProperty == null)
                    _animatorProperty = SupportObject.FindProperty("animator");
                return _animatorProperty;
            }
        } 

        private SerializedProperty _directorProperty;
        private SerializedProperty directorProperty
        {
            get
            {
                if (_directorProperty == null)
                    _directorProperty = SupportObject.FindProperty("director");
                return _directorProperty;
            }
        }

        protected override void OnEnable()
        {
            self = target as StateManager;
            if (Self != null)
            {
                Self.EditorInit(self.transform);
                Self.editStateMachine.View = Self;
                if (string.IsNullOrEmpty(Self.editStateMachine.name))
                    Self.editStateMachine.name = "Base Layer";
                if (StateMachineWindow.support == null) //这个是假的“null”
                    StateMachineWindow.support = null;
                if (StateMachineWindow.support != Self)
                    StateMachineWindow.Init(Self);
            }
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

            try
            {
                FrameworkConfigInfo info = Resources.Load<FrameworkConfigInfo>("FrameworkConfigInfo");
                var types = AssemblyHelper.GetTypes(Assembly.Load(info.assembly));
                list.Clear();
                if (types != null)
                {
                    foreach (var type in types)
                        if (typeof(IArchitecture).IsAssignableFrom(type))
                            list.Add(type.Name);
           
                    if (Self != null && string.IsNullOrEmpty(Self.architectureName))
                    {
                        Self.architectureName = list[0];
                        Self.architectureIndex = 0;
                    }
                }
            }
            catch { }

        
        }
     
        private List<string> list => Self.architectures;

        protected override void ResetPropertys()
        {
            base.ResetPropertys();
            _animationProperty = null;
            _animatorProperty = null;
            _directorProperty = null;          
        }

        protected override void OnDrawPreField()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("support"), new GUIContent(StateMachineSetting.StateMechineController));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("initMode"), new GUIContent(StateMachineSetting.InitMode));
            StateMachineSetting.Language = (PluginLanguage)EditorGUILayout.EnumPopup(StateMachineSetting.SelectLanguageEditor, StateMachineSetting.Language);

            EditorGUILayout.Space();
        }

        private PluginLanguage language;

        protected override void OnDrawAnimationField()
        {
            var view = (StateMachineMono)Self;
            view.animMode = (AnimationMode)EditorGUILayout.EnumPopup(new GUIContent(StateMachineSetting.AnimationMode, "animMode"), view.animMode);
            switch (view.animMode)
            {
                case AnimationMode.Animation:
                    EditorGUILayout.PropertyField(animationProperty, new GUIContent(StateMachineSetting.OldAnimation, "animation"));
                    break;
                case AnimationMode.Animator:
                    EditorGUILayout.PropertyField(animatorProperty, new GUIContent(StateMachineSetting.NewAnimation, "animator"));
                    break;
                case AnimationMode.Timeline:
                    EditorGUILayout.PropertyField(directorProperty, new GUIContent(StateMachineSetting.DirectorAnimation, "director"));
                    EditorGUILayout.PropertyField(animatorProperty, new GUIContent(StateMachineSetting.NewAnimation, "animator"));
                    break;
            }
        }

        protected override void OnDrawActionPropertyField(SerializedProperty actionProperty)
        {
            var view = (StateMachineMono)Self;
            if (view.animMode == AnimationMode.Timeline)
                EditorGUILayout.PropertyField(actionProperty.FindPropertyRelative("clipAsset"), new GUIContent(StateMachineSetting.PlayableAsset, "clipAsset"));
        }

        protected override void OnPlayAnimation(StateAction action)
        {
            var view = (StateMachineMono)Self;
            EditorGUILayout.BeginHorizontal();
            var rect = EditorGUILayout.GetControlRect();
            if (GUI.Button(new Rect(rect.x + 45, rect.y, 30, rect.height), EditorGUIUtility.IconContent(animPlay ? "PauseButton" : "PlayButton")))
            {
                animPlay = !animPlay;
                animAction = action;
            }
            EditorGUI.BeginChangeCheck();
            action.animTime = GUI.HorizontalSlider(new Rect(rect.x + 75, rect.y, rect.width - 75, rect.height), action.animTime, 0f, action.animTimeMax);
            var normalizedTime = action.animTime / action.animTimeMax;
            EditorGUI.ProgressBar(new Rect(rect.x + 75, rect.y, rect.width - 75, rect.height), normalizedTime, $"动画进度:{action.animTime.ToString("f0")}");
            if (EditorGUI.EndChangeCheck())
            {
                animPlay = false;
                animAction = action;
                if (!EditorApplication.isPlaying)
                    PlayAnimation(view, action, normalizedTime);
            }
            EditorGUILayout.EndHorizontal();
            if (animPlay && animAction == action && !EditorApplication.isPlaying)
            {
                action.animTime += 20f * Time.deltaTime;
                if (action.animTime >= action.animTimeMax)
                    action.animTime = 0f;
                PlayAnimation(view, action, normalizedTime);
            }
        }

        private void PlayAnimation(StateMachineMono view, StateAction action, float normalizedTime)
        {
            switch (view.animMode)
            {
                case AnimationMode.Animation:
                    {
                        var animation = view.animation;
                        var clip = animation[action.clipName].clip;
                        float time = clip.length * normalizedTime;
                        clip.SampleAnimation(view.gameObject, time);
                    }
                    break;
                case AnimationMode.Animator:
                    {
                        var animator = view.animator;
                        animator.Play(action.clipName, 0, normalizedTime);
                        animator.Update(0f);
                    }
                    break;
                case AnimationMode.Timeline:
                    {
                    }
                    break;
            }
        }
    }
}
#endif