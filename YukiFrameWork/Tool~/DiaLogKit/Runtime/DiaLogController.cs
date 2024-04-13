///=====================================================
/// - FileName:      IDiaLogController.cs
/// - NameSpace:     YukiFrameWork.DiaLog
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/4/12 21:02:51
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.UI;
namespace YukiFrameWork.DiaLog
{
	public abstract class DiaLogController : ViewController
	{
		enum LoadType
		{
			[LabelText("编辑器拖拽")]
			Inspector,
			[LabelText("通过事件代码加载NodeTree")]
			Loading
		}

		[Serializable]
		protected class CompositeItem
		{
            [LabelText("可能设置的条件对应的按钮:")]
            public Button compositeButton;

			[LabelText("设置判断成功的对应下标:")]
			public int compositeIndex;

			[LabelText("与按钮UI对应的文本组件")]
			public MaskableGraphic compositeText;

			public CompositeItem Show()
			{
				compositeButton.Show();
				compositeText.Show();
				return this;
			}

			public CompositeItem Hide()
			{
				compositeText.Hide();
				compositeButton.Hide();
				return this;
			}
		}
		[LabelText("对话树:")]
		[SerializeField,ShowIf(nameof(loadType),LoadType.Inspector)]
		private NodeTree nodeTree;

		public NodeTree NodeTree => nodeTree;

		[SerializeField,LabelText("对话树的获得方式:")]
		private LoadType loadType = LoadType.Inspector;

		[SerializeField,LabelText("应用同步的语言"),DisableIf(nameof(IsSyncLanguage))]
		private Language language;

		[InfoBox("开启后语言将同步框架的Localization本地化系统的的默认语言")]
		[SerializeField,LabelText("是否语言同步")]
		protected bool IsSyncLanguage;

        [SerializeField]
		[InfoBox("使用事件的加载必须要让DiaLogController的派生类标记RuntimeInitializeOnArchitecture特性否则无法生效!")]
        [LabelText("事件的标识"), ShowIf(nameof(loadType), LoadType.Loading)]
		private string eventName;

		[FoldoutGroup("条件组件集合")]
		[SerializeField,LabelText("可能需要设置的条件:")]
		[InfoBox("设置条件的逻辑应该自行重写")]
		private CompositeItem[] composites;

		[FoldoutGroup("组件集合")]
		[SerializeField,LabelText("文本组件:")]
		protected Text dialogText;
        [FoldoutGroup("组件集合")]
        [SerializeField,LabelText("图片组件:")]
		protected Image diaLogImage;
        [FoldoutGroup("组件集合")]
        [SerializeField,LabelText("设置的名称组件:")]
		protected Text diaLogNameText;

        protected override void Awake()
        {
            base.Awake();
			if (IsSyncLanguage)
			{
				LocalizationKit.RegisterLanguageEvent(language => 
				{
					Debug.Log(this.language);
					this.language = language;
                    this.nodeTree.UpdateNode(language, UpdateDiaLogComponent);                  
                });

				language = LocalizationKit.LanguageType;
			}
			try
			{
				if (loadType == LoadType.Loading)
					this.RegisterEvent<NodeTree>(eventName, tree =>
					{
						this.nodeTree = tree;
						this.nodeTree.OnTreeStart();
						this.nodeTree.UpdateNode(language, UpdateDiaLogComponent);						
					});
				else
				{
                    this.nodeTree.OnTreeStart();
					this.nodeTree.UpdateNode(language, UpdateDiaLogComponent);                 
                }
			}
			catch { }
			foreach (var item in composites)
			{
				item.Hide();
			}
        }

        /// <summary>
        /// 树在运行时进行复位初始化的操作，可用于直接回档或跳转到哪一个节点，从该处继续
        /// </summary>
        /// <param name="currentNode">需要回档的节点</param>
        /// <param name="callBack">更新的回调</param>
        protected void OnTreeRunningInitialization(Language language,int nodeIndex, System.Action<string, Sprite, string> callBack)
		{
			nodeTree.OnTreeRunningInitialization(language, nodeIndex, callBack);
        }

        /// <summary>
        /// 如果需要按钮作为分支的逻辑判断则必须实现该方法
        /// </summary>
        /// <param name="branch">分支节点</param>
        /// <param name="buttons">所有的Button按钮(外部添加)</param>
        protected virtual void CompositeSetting(BranchDialogue branch, CompositeItem[] composites) { }	

		/// <summary>
		/// 设置更
		/// </summary>
		/// <param name="dialog"></param>
		/// <param name="sprite"></param>
		/// <param name="name"></param>
        protected void UpdateDiaLogComponent(string dialog,Sprite sprite,string name)
		{
            if (dialogText != null)
                dialogText.text = dialog;
            if (diaLogImage != null)
                diaLogImage.sprite = sprite;
            if (diaLogNameText != null)
                diaLogNameText.text = name;
        }

        /// <summary>
        /// 推进到下一个对话
        /// </summary>
        /// <param name="next"></param>
        /// <returns></returns>
        public virtual void MoveToNext()
		{			
            if (nodeTree.MoveNext())
			{
				nodeTree.UpdateNode(language,UpdateDiaLogComponent);
				if(nodeTree.runningNode is BranchDialogue branch)
					CompositeSetting(branch,composites);
				NodeStart(nodeTree.runningNode);
			}
		}

		/// <summary>
		/// 每次推进的时候都会执行这个方法
		/// </summary>
		/// <param name="node"></param>
		public virtual void NodeStart(Node node)
		{
			
		}

		protected virtual void OnDestroy()
		{
			nodeTree.OnTreeEnd();
			nodeTree = null;
		}
    }
}
