using UnityEngine;

namespace YukiFrameWork.ActionStates
{
    public class AnimatorStateMachine : IAnimationHandler
    {
        private readonly Animator animator;

        public AnimatorStateMachine(Animator animator)
        {
            this.animator = animator;
        }

        public void OnInit()
        {
        }

        public void OnPlayAnimation(State state, StateAction stateAction)
        {
            var clipName = stateAction.clipName;
            animator.speed = state.animSpeed;
            if (state.isCrossFade)
            {
                var stateInfo = animator.GetCurrentAnimatorStateInfo(stateAction.layer);
                if (stateInfo.normalizedTime >= 1f)
                    animator.Play(clipName, stateAction.layer, 0f);
                else
                    animator.CrossFade(clipName, state.duration);
            }
            else animator.Play(clipName, stateAction.layer, 0f);
        }

        public bool OnAnimationUpdate(State state, StateAction stateAction, UpdateStatus updateStatus)
        {
            var isPlaying = true;
            var stateInfo = animator.GetCurrentAnimatorStateInfo(stateAction.layer);
            stateAction.animTime = stateInfo.normalizedTime / 1f * 100f;
            return isPlaying;
        }
    }
}