using UnityEngine;
using UnityEngine.UI;

namespace YukiFrameWork.DiaLog
{
    [RootNode]
    public class RootDialogue : Node
    {
        [SerializeField]
        internal Node child;
        public override bool IsDefaultMoveNext => true;
        public override Node MoveToNext()
        {
            return child;
        }
        public override void OnEnter()
        {

        }

        public override void OnExit() { }

        public override void OnUpdate()
        {
            
        }
    }
}