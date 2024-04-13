using UnityEngine;
using UnityEngine.UI;

namespace YukiFrameWork.DiaLog
{
    public class RootDialogue : RootNode
    {
        public override bool MoveToNext(out Node nextNode)
        {
            nextNode = child;
            return true;
        }
        public override void OnStart()
        {

        }

        public override void OnStop() { }

    }
}