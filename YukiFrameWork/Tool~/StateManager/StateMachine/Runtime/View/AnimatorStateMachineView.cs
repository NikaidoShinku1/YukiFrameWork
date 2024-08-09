using UnityEngine;

namespace YukiFrameWork.ActionStates
{
    public class AnimatorStateMachineView : StateMachineView
    {
        public Animator animator;

        public override void Init()
        {
            stateMachine.Handler = new AnimatorStateMachine(animator);
            stateMachine.View = this;
            stateMachine.Init();
        }

#if UNITY_EDITOR
        public override void EditorInit(Transform root)
        {
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