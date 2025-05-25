///=====================================================
/// - FileName:      UIOptionGroup.cs
/// - NameSpace:     YukiFrameWork.DiaLogue
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/5/2 21:39:37
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Collections;
using XFABManager;
namespace YukiFrameWork.DiaLogue
{
    [DisableViewWarning]
	public class UIOptionGroup : MonoBehaviour
	{
        public enum UIOptionGenericType
        {
            [LabelText("使用插槽预制体生成")]
            Template,
            [LabelText("使用已经存在的插槽")]
            OptionExist
        }
        [SerializeField,InfoBox("绑定通过DiaLogKit创造DiaLog时对应的标识")]
		internal string DiaLogKey;

        [SerializeField,LabelText("绑定的模式"),InfoBox("设置为安全模式时，条件的初始化会在Start方法后等待一帧执行，避免初始化生命周期混乱")]
        internal DiaLogLoadMode DiaLogLoadMode = DiaLogLoadMode.Normal;

        [LabelText("UI Option的生成类型:"), SerializeField]
        internal UIOptionGenericType optionGenericType;

        [LabelText("UIOption预制体选择:"),SerializeField,ShowIf(nameof(optionGenericType),UIOptionGenericType.Template)]
        //[InfoBox("当使用预制体生成时，在编辑器模式下，uiOption不应该直接置于uiOptionRoot节点下，运行后会自动进行父对象绑定，当你存在该操作失误时，作为预制体的节点运行也会自动转移节点到uiOptionRoot下方")]
		internal UIOption uIOption;

        [SerializeField,LabelText("UIOption的根节点:"),ShowIf(nameof(optionGenericType), UIOptionGenericType.Template)]
        internal RectTransform uiOptionRoot;

        [LabelText("已经存在的分组集合:"),SerializeField, ShowIf(nameof(optionGenericType), UIOptionGenericType.OptionExist)]
        internal List<UIOption> uiOptions = new List<UIOption>();

        private IEnumerator Start()
        {
            if (uIOption != null && optionGenericType == UIOptionGenericType.Template)
                uIOption.Hide();
            else if (optionGenericType == UIOptionGenericType.OptionExist && uiOptions.Count > 0)
                uiOptions.Hide();
            if (DiaLogLoadMode == DiaLogLoadMode.Safe)
                yield return null;
            InitOption();
        }

        private void InitOption()
        {
            DiaLog diaLog = DiaLogKit.GetDiaLogueByKey(DiaLogKey);

            if (diaLog == null)
            {
                throw new System.Exception("无法进行分支的初始化,请检查DiaLog是否在初始化之前通过DiaLogKit创建 DiaLogKey:" + DiaLogKey);
            }

            if (diaLog.tree == null)
            {
                throw new System.Exception("无法进行分支的初始化,请检查DiaLog是否持有了NodeTree DiaLogKey:" + DiaLogKey);
            }
            diaLog.RegisterWithNodeCompleteEvent(node =>
            {
                var options = node.optionItems;              
                switch (optionGenericType)
                {
                    case UIOptionGenericType.Template:
                        {
                            for (int i = 0; i < options.Count; i++)
                            {
                                int index = i;
                                 GameObjectLoader.Load(uIOption.gameObject,uiOptionRoot)
                                .Show().GetComponent<UIOption>().Core(o => o.Option = options[index]).InitUIOption(diaLog, node);
                            }
                        }
                        break;
                    case UIOptionGenericType.OptionExist:
                        {
                            for (int i = 0; i < Mathf.Min(uiOptions.Count,options.Count); i++)
                            {
                                var option = options[i];
                                uiOptions[i].Show().Core(o => o.Option = option).InitUIOption(diaLog,node);
                            }
                        }
                        break;                   
                }              
            }).UnRegisterWaitGameObjectDestroy(this);

            diaLog.RegisterWithNodeExitEvent(node => 
            {              
                switch (optionGenericType)
                {
                    case UIOptionGenericType.Template:
                        if (uiOptionRoot.childCount == 0) return;
                        
                        uiOptionRoot.UnLoadChildrenWithCondition(transform 
                            => transform.GetComponent<UIOption>() && transform != uIOption.transform);
                        break;
                    case UIOptionGenericType.OptionExist:                      
                        uiOptions.Hide();
                        break;
                }
            }).UnRegisterWaitGameObjectDestroy(this);
        }
    }
}
