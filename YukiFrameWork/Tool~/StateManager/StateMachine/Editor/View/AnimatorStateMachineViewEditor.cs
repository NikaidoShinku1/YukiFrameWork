#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace YukiFrameWork.ActionStates
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(AnimatorStateMachineView))]
    public class AnimatorStateMachineViewEditor : StateMachineViewEditor
    {
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

        protected override void ResetPropertys()
        {
            base.ResetPropertys();
            _animatorProperty = null;
        }

        protected override void OnDrawAnimationField()
        {
            EditorGUILayout.PropertyField(animatorProperty, new GUIContent(StateMachineSetting.NewAnimation, "animator"));
        }

        protected override void OnDrawActionPropertyField(SerializedProperty actionProperty)
        {
        }

        protected override void OnPlayAnimation(StateAction action)
        {
            var view = (AnimatorStateMachineView)Self;
            var animator = view.animator;
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
                {
                    StateAction.SetBlendTreeParameter(action, view.animator);
                    animator.Play(action.clipName, 0, normalizedTime);
                    animator.Update(0f);
                }
            }
            EditorGUILayout.EndHorizontal();
            if (animPlay && animAction == action && !EditorApplication.isPlaying)
            {
                StateAction.SetBlendTreeParameter(action, view.animator);
                action.animTime += 20f * Time.deltaTime;
                if (action.animTime >= action.animTimeMax)
                    action.animTime = 0f;              
                animator.Play(action.clipName, 0, normalizedTime);
                animator.Update(0f);
            }
        }
    }
}
#endif