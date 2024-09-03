using UnityEngine;

namespace YukiFrameWork.ActionStates
{
    public class AnimationStateMachine : IAnimationHandler
    {
        private Animation animation;

        public AnimationStateMachine(Animation animation)
        {
            this.animation = animation;
        }

        public void OnInit()
        {
        }

        public void OnPlayAnimation(State state, StateAction stateAction)
        {
            var clipName = stateAction.clipName;
            var animState = animation[clipName];
            animState.speed = state.animSpeed;
            if (state.isCrossFade)
            {
                if (animState.time >= animState.length)
                {
                    animation.Play(clipName);
                    animState.time = 0f;
                }
                else animation.CrossFade(clipName, state.duration);
            }
            else
            {
                animation.Play(clipName);
                animState.time = 0f;
            }

        }

        public void SetParams(params object[] args)
        {
            animation = args[0] as Animation;
        }

        public bool OnAnimationUpdate(State state, StateAction stateAction,UpdateStatus updateStatus)
        {
            var clipName = stateAction.clipName;
            var animState = animation[clipName];
            stateAction.animTime = animState.time / animState.length * 100f;
            bool isPlaying = animation.isPlaying;
            return isPlaying;
        }
    }
}