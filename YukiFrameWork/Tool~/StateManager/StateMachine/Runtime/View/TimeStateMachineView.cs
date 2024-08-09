namespace YukiFrameWork.ActionStates
{
    public class TimeStateMachineView : StateMachineView
    {
        public override void Init()
        {
            stateMachine.Handler = new TimeStateMachine();
            stateMachine.View = this;
            stateMachine.Init();
        }
    }
}