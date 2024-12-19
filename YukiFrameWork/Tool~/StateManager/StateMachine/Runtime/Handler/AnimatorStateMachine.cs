using UnityEngine;

namespace YukiFrameWork.ActionStates
{
    public class AnimatorStateMachine : IAnimationHandler
    {
        private Animator animator;

        public AnimatorStateMachine(Animator animator)
        {
            this.animator = animator;
        }

        public void OnInit()
        {
        }

        public void SetParams(params object[] args)
        {
            animator = args[0] as Animator;
        }
        /// <summary>
        /// 记录动画剪辑的hash值
        /// </summary>
        private int hash = -1;       
        public void OnPlayAnimation(State state, StateAction stateAction)
        {
           
            var clipName = stateAction.clipName;
            animator.speed = state.animSpeed;
            StateAction.SetBlendTreeParameter(stateAction,animator);
            stateAction.animTime = 0;
            if(!clipName.IsNullOrEmpty())
                hash = Animator.StringToHash(clipName);         
            if (state.isCrossFade)
            {
                var stateInfo = animator.GetCurrentAnimatorStateInfo(stateAction.layer);
                      
                if (stateInfo.normalizedTime >= 1f)
                {                  
                    animator.Play(clipName, stateAction.layer, 0f);
                }
                else
                    animator.CrossFade(clipName, state.duration);               
            }
            else animator.Play(clipName, stateAction.layer, 0f);
        }
      
        public bool OnAnimationUpdate(State state, StateAction stateAction, UpdateStatus updateStatus)
        {          
            var isPlaying = true;
            var stateInfo = animator.GetCurrentAnimatorStateInfo(stateAction.layer);         
            if (stateInfo.shortNameHash == hash)//通过Animator获取当前状态信息的API，通常需要在切换后的下一帧才能在这里识别，必须确保状态的哈希是与我们需要调用的动画剪辑哈希一致才可以调用动画
                stateAction.animTime = stateInfo.normalizedTime / 1f * 100f;
            return isPlaying;
        }
    }
}