using UnityEngine;
using UnityEngine.UI;

namespace YukiFrameWork.DiaLog
{
    // 普通对话节点 只会返回
    public class NormalDialogue : SingleNode
    {
        public override Node MoveToNext()
        {
            return child;
        }

        public override void OnEnter()
        {

        }

        public override void OnExit()
        {

        }

        public override void OnUpdate()
        {

        }
    }
}
