using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using YukiFrameWork;

namespace YukiFrameWork.DiaLog
{
    public class BranchDialogue : CompositeNode
    {
        [SerializeField, LabelText("设置条件的文本,下标应与子对话节点相同"),DictionaryDrawerSettings(KeyLabel = "条件的语言",ValueLabel = "设置与子配置相同数量的条件设置" )]
        private YDictionary<Language, List<string>> optionList = new YDictionary<Language, List<string>>();

        private Node nextNode;

        public override bool MoveToNext(out Node nextNode)
        {
            if (this.nextNode == null)
            {
                nextNode = this;
                return false;
            }
            nextNode = this.nextNode;
            return true;
        }

        public void SetOptionSuccessed(int index)
        {
            nextNode = children[index];
        }

        public string GetOptionText(int index)
        {
            if (optionList.TryGetValue(currentLanguage, out List<string> options))
            {
                string content = options[index];
                return content;
            }
            else
            {
                throw new System.Exception("查询是否添加了该语言的条件文本或者下标是否存在：Language:" + currentLanguage + "index:" + index);
            }
        }

        public override void OnStart()
        {

        }

        public override void OnStop()
        {
            nextNode = null;
        }
    }
}