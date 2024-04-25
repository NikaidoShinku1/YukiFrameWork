///=====================================================
/// - FileName:      UIKitStartTrigger.cs
/// - NameSpace:     YukiFrameWork.UI
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/4/24 17:14:28
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using UnityEngine;
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using XFABManager;
using UnityEngine.Events;

namespace YukiFrameWork.UI
{
	public class UIKitStartTrigger : SerializedMonoBehaviour
	{
		enum ResourcesType
		{
			Sync,
			Async
		}

		enum LoadType
		{
			[LabelText("使用框架XFABManager加载")]
			XFABManager,
			[LabelText("使用Resources加载")]
			Resources
		}

		[SerializeField, HideInInspector]
		private List<string> panels = new List<string>();

		public IReadOnlyList<string> ReadOnlyPanelTypes => panels;
		[SerializeField, HideInInspector]
		private int selectIndex;
		[SerializeField, BoxGroup("加载的方式:")]
		private ResourcesType resourcesType;
		[SerializeField, BoxGroup("加载的方式:")]
		private LoadType loadType = LoadType.XFABManager;
		[SerializeField, LabelText("面板的路径:"), ShowIf(nameof(loadType), LoadType.Resources), GUIColor("yellow")]
		private string panelPath = "Assets";
		[SerializeField, LabelText("模块配置名:"), ShowIf(nameof(loadType), LoadType.XFABManager),GUIColor("yellow")]
		private string projectName;
		[SerializeField, LabelText("面板的名称:"),PropertySpace,ShowIf(nameof(loadType), LoadType.XFABManager)]
		private string panelName;

		private BasePanel rootPanel;

		private Type panelType => typeof(IPanel);		

		/// <summary>
		/// 该面板启动器是否已经完成了加载并触发回调
		/// </summary>
		public bool IsCompleted { get; private set; }

		[LabelText("当面板加载完成后希望注册的回调"),PropertySpace]
		public UnityEvent<BasePanel> onPanelCallBack;
        
        private void Awake()
		{
			switch (resourcesType)
			{
				case ResourcesType.Sync:
					Init();
					break;
				case ResourcesType.Async:
					InitAsync();
					break;
			}
		}

		private async void InitAsync()
		{
			switch (loadType)
			{
				case LoadType.XFABManager:
					{
						var request = AssetBundleManager.LoadAssetAsync<GameObject>(projectName, panelName);
						request.AddCompleteEvent(v => rootPanel = (BasePanel)(v.asset as GameObject).GetComponent(panelType));
						await request.CancelWaitGameObjectDestroy(this);
					}
					break;
				case LoadType.Resources:
					{
						var request = Resources.LoadAsync(panelPath, panelType);
						await request.CancelWaitGameObjectDestroy(this);
						rootPanel = request.asset as BasePanel;
					}
					break;
			}
			SetPanel();
		}

		private void Init()
		{
			switch (loadType)
			{
				case LoadType.XFABManager:
					rootPanel = AssetBundleManager.LoadAsset<GameObject>(projectName, panelName).GetComponent(panelType) as BasePanel;
					break;
				case LoadType.Resources:
					rootPanel = Resources.Load(panelPath, panelType) as BasePanel;
					break;
			}
			SetPanel();
		}

		private void SetPanel()
		{						
			var current = UIKit.OpenPanel(rootPanel);
			onPanelCallBack?.Invoke(current);
			IsCompleted = true;
		}		
	}
}
