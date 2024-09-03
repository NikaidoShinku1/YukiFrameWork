using UnityEngine;

namespace YukiFrameWork.ActionStates
{
    public class TimeStateMachine : IAnimationHandler
    {
        public TimeStateMachine()
        {
        }

        public void OnInit()
        {
        }

        public void SetParams(params object[] args)
        {
        }

        public void OnPlayAnimation(State state, StateAction stateAction)
        {
            stateAction.animTime = 0f;
        }

        public bool OnAnimationUpdate(State state, StateAction stateAction, UpdateStatus updateStatus)
        {
            var isPlaying = true;
            if(updateStatus == UpdateStatus.OnUpdate)
                stateAction.animTime += state.animSpeed * stateAction.animTimeMax * Time.deltaTime;
            return isPlaying;
        }
    }
}