using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using YukiFrameWork;

namespace YukiFrameWork.DiaLog
{
    public class BranchDialogue : CompositeNode
    {
        [SerializeField, LabelText("设置条件的文本,下标应与子对话节点相同"),DictionaryDrawerSettings(KeyLabel = "条件的语言",ValueLabel = "设置与子配置相同数量的条件设置" )]
        private YDictionary<Language, YDictionary<int,string>> optionList = new YDictionary<Language, YDictionary<int,string>>();

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

        public void SetOptionSuccessed(CompositeItem[] composites,System.Action moveNext)
        {          
            for (int i = 0; i < composites.Length; i++)
            {               
                var composite = composites[i];
                composite.compositeButton.RemoveListener(OnButtonClick);
                composite.compositeButton.onClick.AddListener(OnButtonClick);
                composite.CompositeLabelChanged?.Invoke(GetOptionText(composite.compositeIndex));              
                void OnButtonClick()
                {
                    SetOptionSuccessed(() => composite.compositeIndex);
                    moveNext?.Invoke();                   
                }              
            }
        }

        public void SetOptionSuccessed(Func<int> indexEvent)
        {
            if (indexEvent == null) return;
            nextNode = children[indexEvent.Invoke()];
        }

        public string GetOptionText(int index)
        {
            if (optionList.TryGetValue(currentLanguage, out var options))
            {
                if (options.TryGetValue(index,out string content))              
                    return content;               
            }
            throw new System.Exception("查询是否添加了该语言的条件文本或者下标是否存在：Language:" + currentLanguage + "index:" + index);

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