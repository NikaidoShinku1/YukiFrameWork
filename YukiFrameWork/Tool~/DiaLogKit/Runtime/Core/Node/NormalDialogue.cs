using UnityEngine;
using UnityEngine.UI;

namespace YukiFrameWork.DiaLog
{
    // 普通对话节点 只会返回
    public class NormalDialogue : SingleNode
    {
        public override bool MoveToNext(out Node nextNode)
        {
            nextNode = child;
            return true;
        }

        public override void OnStart()
        {

        }

        public override void OnStop()
        {

        }

    }
}
