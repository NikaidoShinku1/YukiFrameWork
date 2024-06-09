///=====================================================
/// - FileName:      UIPrefabManager.cs
/// - NameSpace:     YukiFrameWork.Tower
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/6/3 2:36:04
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using UnityEngine.Events;
using System.Collections.Generic;
using Sirenix.OdinInspector;
namespace YukiFrameWork.UI
{
	/// <summary>
	/// 临时UI面板预制体执行者,简单操作，无需实例化，直接在场景处理，只控制面板的开关，仅对特殊情况使用(例如资源加载的掩盖面板等),位于所有层级之后，最底
	/// </summary>
	public class UIPrefabExector : MonoBehaviour
	{		
		[SerializeField,LabelText("预制体管理集合"),ReadOnly]
		internal List<BasePanel> prefabInfos = new List<BasePanel>();

		public void InitExector()
		{
			prefabInfos = new List<BasePanel>(GetComponentsInChildren<BasePanel>(true));
		
			for (int i = 0; i < prefabInfos.Count; i++)
			{
				var panel = prefabInfos[i];
				if (panel == null) continue;
				panel.Hide().OnInit();
			}
		}     

		public T ShowPanel<T>() where T : BasePanel,IPanel
		{
			IPanel panel = prefabInfos.Find(x => x.GetType() == typeof(T));
			if (panel == null) return null;
			panel.Enter();
            panel.gameObject.Show().SetAsLastSibling();
			return panel as T;
        }

		public T HidePanel<T>() where T : BasePanel
		{
            IPanel panel = prefabInfos.Find(x => x.GetType() == typeof(T));
			if (panel != null)
			{
				panel.Exit();
				panel.gameObject.Hide().SetParent(this);
				return panel as T;
			}
			return null;
        }

		public void AddPanel(BasePanel panel)
		{
			if (prefabInfos.Contains(panel)) return;
			panel.Hide().OnInit();
			prefabInfos.Add(panel);
		}	
		
    }
}
