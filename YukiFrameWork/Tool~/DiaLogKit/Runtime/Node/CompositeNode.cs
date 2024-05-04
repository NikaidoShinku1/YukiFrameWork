
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
namespace YukiFrameWork.DiaLog
{
    [Serializable]
    public class Option
    {
        [ReadOnly]
        public int childIndex;
        [LabelText("设置条件的文本"), DictionaryDrawerSettings(KeyLabel = "语言", ValueLabel = "条件文本",KeyColumnWidth = 15)]
        public YDictionary<Language, string> optionTexts = new YDictionary<Language, string>();

        public string this[CompositeNode node]
        {
            get => optionTexts[node.currentLanguage];
        }

        public void OnChangeClick(DiaLog diaLog)
        {
            diaLog.MoveByNodeIndex(childIndex);
        }
    }   

    public abstract class CompositeNode : Node
    {
        [Searchable,SerializeField,ListDrawerSettings(HideAddButton = true)]
        internal List<Option> options = new List<Option>();

        public override bool IsDefaultMoveNext => false;

        protected Action<CompositeNode,Option[]> OnOptionCallBack = null;
        protected Action onFinishCallBack = null;
        public override Node MoveToNext() => null;            
      
        public virtual void OnOptionsCompleted(Action<CompositeNode,Option[]> action,Action callBack)
        {           
            OnOptionCallBack = action;
            this.onFinishCallBack = callBack;
        }

        public override void OnEnter()
        {
            var options = this.options.ToArray();          
            OnOptionCallBack?.Invoke(this,options);
        }

        public override void OnExit()
        {
            onFinishCallBack?.Invoke();
        }       
    }
}