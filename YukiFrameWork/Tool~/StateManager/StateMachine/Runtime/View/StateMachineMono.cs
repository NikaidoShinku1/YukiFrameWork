using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace YukiFrameWork.ActionStates
{
    /// <summary>
    /// 状态机 v2017/12/6
    /// </summary>
    public class StateMachineMono : StateMachineView
    {
        /// <summary>
        /// 动画选择模式
        /// </summary>
        public AnimationMode animMode;
        /// <summary>
        /// 旧版动画组件
        /// </summary>
		public new Animation animation;
        /// <summary>
        /// 新版动画组件
        /// </summary>
        public Animator animator;
        /// <summary>
        /// 可播放导演动画
        /// </summary>
        public PlayableDirector director;

        // 旧版本兼容问题，如果去掉这个字段，之前的状态会全部丢失！
#if UNITY_2020_1_OR_NEWER
        [NonReorderable]
#endif
        public State[] states;

        public override Transform Parent { get => transform.parent != null ? transform.parent : transform; }

        public static StateMachineMono CreateSupport(string name = "Base Layer")
        {
            var monoStateMachine = new GameObject(name).AddComponent<StateMachineMono>();
            monoStateMachine.name = name;
            var stateMachine = new StateMachineCore()
            {
                name = name,
                states = new State[0],
#if UNITY_EDITOR
                selectStates = new List<int>(),
#endif
                View = monoStateMachine,
            };
            monoStateMachine.stateMachine = stateMachine;
#if UNITY_EDITOR
            monoStateMachine.editStateMachine = stateMachine;
#endif
            return monoStateMachine;
        }

        public override void Init()
        {
            switch (animMode)
            {
                case AnimationMode.Animation:
                    stateMachine.Handler = new AnimationStateMachine(animation);
                    break;
                case AnimationMode.Animator:
                    stateMachine.Handler = new AnimatorStateMachine(animator);
                    break;
                case AnimationMode.Timeline:
                    stateMachine.Handler = new TimelineStateMachine(animator, director);
                    break;
                case AnimationMode.Time:
                    stateMachine.Handler = new TimeStateMachine();
                    break;
            }
            stateMachine.View = this;
            stateMachine.Init();
        }

#if UNITY_EDITOR
        public override void EditorInit(Transform root)
        {
            if (animation == null)
                animation = root.GetComponentInChildren<Animation>();
            if (animation != null)
            {
                var clips = UnityEditor.AnimationUtility.GetAnimationClips(animation.gameObject);
                ClipNames.Clear();
                foreach (var clip in clips)
                    ClipNames.Add(clip.name);
            }
            if (animator == null)
                animator = root.GetComponentInChildren<Animator>();
            if (animator != null)
            {
                if (animator.runtimeAnimatorController is UnityEditor.Animations.AnimatorController controller)
                {
                    if (controller.layers.Length > 0) //打AB包后选择这里是0
                    {
                        var layer = controller.layers[0];
                        var states = layer.stateMachine.states;
                        ClipNames.Clear();
                        foreach (var state in states)
                            ClipNames.Add(state.state.name);
                    }
                }
            }
            if (director == null)
                director = root.GetComponentInChildren<PlayableDirector>();
        }

        public override void OnScriptReload()
        {
            stateMachines ??= new List<IStateMachine>();
            if (states != null && states.Length > 0)
            {
                stateMachine.states = states;
                states = null;
                UnityEditor.EditorUtility.SetDirty(this);
            }
            if (stateMachine == null)
                return;
            stateMachine.View = this;
            stateMachines.Clear();
            stateMachine.OnScriptReload(this);
            for (int i = 0; i < stateMachines.Count; i++)
                stateMachines[i].Id = i;
            UpdateEditStateMachine(editStateMachineId);
        }
#endif
    }
}