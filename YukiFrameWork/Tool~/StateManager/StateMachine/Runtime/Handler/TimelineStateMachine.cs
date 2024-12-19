using UnityEngine;
using UnityEngine.Playables;

namespace YukiFrameWork.ActionStates
{
    public class TimelineStateMachine : IAnimationHandler
    {
        private Animator animator;
        private PlayableDirector director;

        public TimelineStateMachine(Animator animator, PlayableDirector director)
        {
            this.animator = animator;
            this.director = director;
        }

        public void OnInit()
        {
        }
        public void SetParams(params object[] args)
        {
            animator = args[0] as Animator;
            director = args[1] as PlayableDirector;
        }
        /// <summary>
        /// 记录动画剪辑的hash值
        /// </summary>
        private int hash = -1;
        public void OnPlayAnimation(State state, StateAction stateAction)
        {
            var clipName = stateAction.clipName;
            stateAction.animTime = 0;
            if (stateAction.clipAsset != null)
            {
                director.Play(stateAction.clipAsset, DirectorWrapMode.None);
                var playableGraph = director.playableGraph;
                var playable = playableGraph.GetRootPlayable(0);
                playable.SetSpeed(state.animSpeed);
            }
            else
            {
                animator.speed = state.animSpeed;
                if (!clipName.IsNullOrEmpty())
                    hash = Animator.StringToHash(clipName);
               
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
        }

        public bool OnAnimationUpdate(State state, StateAction stateAction, UpdateStatus updateStatus)
        {
            var isPlaying = true;
            if (stateAction.clipAsset != null)
            {
                var time = director.time;
                var duration = director.duration;              
                stateAction.animTime = (float)(time / duration) * 100f;
                isPlaying = director.state == PlayState.Playing;
            }
            else
            {                
                var stateInfo1 = animator.GetCurrentAnimatorStateInfo(stateAction.layer);              
                if (stateInfo1.shortNameHash == hash)//通过Animator获取当前状态信息的API，通常需要在切换后的下一帧才能在这里识别，必须确保状态的哈希是与我们需要调用的动画剪辑哈希一致才可以调用动画
                    stateAction.animTime = stateInfo1.normalizedTime / 1f * 100f;
            }
            return isPlaying;
        }
    }
}