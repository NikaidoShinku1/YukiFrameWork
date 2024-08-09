using UnityEngine;

namespace YukiFrameWork.ActionStates
{
    public class AnimationStateMachineView : StateMachineView
    {
        public new Animation animation;

        public override void Init()
        {
            stateMachine.Handler = new AnimationStateMachine(animation);
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
        }
#endif
    }
}