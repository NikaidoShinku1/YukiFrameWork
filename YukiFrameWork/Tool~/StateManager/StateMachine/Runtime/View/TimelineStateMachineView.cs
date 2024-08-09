using UnityEngine;
using UnityEngine.Playables;

namespace YukiFrameWork.ActionStates
{
    public class TimelineStateMachineView : StateMachineView
    {
        public Animator animator;
        public PlayableDirector director;

        public override void Init()
        {
            stateMachine.Handler = new TimelineStateMachine(animator, director);
            stateMachine.View = this;
            stateMachine.Init();
        }

#if UNITY_EDITOR
        public override void EditorInit(Transform root)
        {
            if (director == null)
                director = root.GetComponentInChildren<PlayableDirector>();
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
        }
#endif
    }
}