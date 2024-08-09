#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace YukiFrameWork.ActionStates
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(AnimationStateMachineView))]
    public class AnimationStateMachineViewEditor : StateMachineViewEditor
    {
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

        protected override void ResetPropertys()
        {
            base.ResetPropertys();
            _animationProperty = null;
        }

        protected override void OnDrawAnimationField()
        {
            EditorGUILayout.PropertyField(animationProperty, new GUIContent(StateMachineSetting.OldAnimation, "animation"));
        }

        protected override void OnDrawActionPropertyField(SerializedProperty actionProperty)
        {
        }

        protected override void OnPlayAnimation(StateAction action)
        {
            var view = (AnimationStateMachineView)Self;
            var animation = view.animation;
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
                    var clip = animation[action.clipName].clip;
                    float time = clip.length * normalizedTime;
                    clip.SampleAnimation(view.gameObject, time);
                }
            }
            EditorGUILayout.EndHorizontal();
            if (animPlay && animAction == action && !EditorApplication.isPlaying)
            {
                action.animTime += 20f * Time.deltaTime;
                if (action.animTime >= action.animTimeMax)
                    action.animTime = 0f;
                var clip = animation[action.clipName].clip;
                float time = clip.length * normalizedTime;
                clip.SampleAnimation(view.gameObject, time);
            }
        }
    }
}
#endif