///=====================================================
/// - FileName:      UIOption.cs
/// - NameSpace:     YukiFrameWork.DiaLog
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/5/2 21:39:48
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using UnityEngine.Events;
namespace YukiFrameWork.DiaLog
{
	public class UIOption : MonoBehaviour
	{
		[SerializeField,LabelText("设置按钮")]
		internal Button onClickBtn;

		[LabelText("设置与按钮配套的文本"),SerializeField,HideIf(nameof(IsCustomContext))]
		internal Text mTextContext;		

		public Option Option { get; private set; }

		[LabelText("是否自定义按钮对应的文本组件"),SerializeField]
		internal bool IsCustomContext;

		[InfoBox("自定义分支按钮的对应文本组件接收文本数据(例如TextMeshProUGUI)"),ShowIf(nameof(IsCustomContext))]
		public UnityEvent<string> mContextCallBack;

		public void InitUIOption(DiaLog diaLog,CompositeNode node,Option option)
		{
			this.Option = option;
            if (IsCustomContext)
                mContextCallBack?.Invoke(option[node]);
            else mTextContext.text = option[node];
			onClickBtn.onClick.RemoveListener(ButtonClick);
			onClickBtn.onClick.AddListener(ButtonClick);
            void ButtonClick()
			{
				option.OnChangeClick(diaLog);			
			}
		}

	}
}
