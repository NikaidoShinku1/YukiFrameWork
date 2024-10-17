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

#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using UnityEditor;
#endif
namespace YukiFrameWork.UI
{
	/// <summary>
	/// 临时UI面板预制体执行者,简单操作，无需实例化，直接在场景处理，只控制面板的开关，仅对特殊情况使用(例如资源加载的掩盖面板等),位于所有层级之后，最底
	/// </summary>
	[DisableViewWarning]
	public class UIPrefabExector : MonoBehaviour
	{				
		internal List<BasePanel> prefabInfos = new List<BasePanel>();

		[LabelText("打开临时面板时触发的回调")]
		[InfoBox("在Panel执行Enter方法后调用")]
		public UnityEvent<BasePanel> ShowPanelCallBack;
		[LabelText("关闭临时面板时触发的回调")]
		[InfoBox("在Panel执行Exit方法后调用")]
		public UnityEvent<BasePanel> HidePanelCallBack;
		public void InitExector()
		{
			prefabInfos = new List<BasePanel>(GetComponentsInChildren<BasePanel>(true));
		
			for (int i = 0; i < prefabInfos.Count; i++)
			{
				var panel = prefabInfos[i];
				if (panel == null) continue;
				panel.Hide().OnInit();
			}

			if (defaultPanel != null)			
				Open(defaultPanel);           
		}

		[SerializeField,HideInInspector]
		internal BasePanel defaultPanel;

		[Obsolete("推荐直接使用UIKit.ShowPanel进行临时面板的开启")]
		public T ShowPanel<T>(params object[] param) where T : BasePanel
		{
			return Show_Internal<T>(param);
        }

        internal T Show_Internal<T>(params object[] param) where T : BasePanel
        {
            IPanel panel = prefabInfos.Find(x => x.GetType() == typeof(T));
            if (panel == null) return null;
			Open(panel,param);
            return panel as T;
        }

		private void Open(IPanel panel, params object[] param)
		{
            panel.Enter(param);
            panel.gameObject.Show().SetAsLastSibling();
            ShowPanelCallBack?.Invoke(panel as BasePanel);
        }			

        [Obsolete("推荐直接使用UIKit.HidePanel进行临时面板的关闭")]
        public T HidePanel<T>() where T : BasePanel
		{
			return Hide_Internal<T>();
        }

		/// <summary>
		/// 为UnityEvent拓展的打开方法。面板必须已经存在PrefabRoot的子节点中
		/// </summary>
		/// <param name="panel"></param>
		public void ShowPanel(BasePanel panel)
		{
			if (!prefabInfos.Contains(panel))
			{
				Debug.LogWarning("PrefabRoot节点下不存在这个面板:" + panel.GetType());
				return;
			}

			Open(panel);
		}

        /// <summary>
        /// 为UnityEvent拓展的关闭方法。面板必须已经存在PrefabRoot的子节点中
        /// </summary>
        /// <param name="panel"></param>		
        public void HidePanel(BasePanel panel)
        {
            if (!prefabInfos.Contains(panel))
            {
                Debug.LogWarning("PrefabRoot节点下不存在这个面板:" + panel.GetType());
                return;
            }

			Hide(panel);
        }

        internal T Hide_Internal<T>() where T : BasePanel
		{
            IPanel panel = prefabInfos.Find(x => x.GetType() == typeof(T));

			if (panel == null) return null;
			Hide(panel);
			return panel as T;       
        }

		private void Hide(IPanel panel)
		{
            panel.Exit();
            panel.gameObject.Hide();
            HidePanelCallBack?.Invoke(panel as BasePanel);
        }


        public void AddPanel(BasePanel panel)
		{
			if (prefabInfos.Contains(panel)) return;
			panel.Hide().OnInit();
			prefabInfos.Add(panel);
		}	
		
    }

#if UNITY_EDITOR
	[UnityEditor.CustomEditor(typeof(UIPrefabExector))]
	public class UIExecutorEditor : OdinEditor
	{
		private BasePanel[] panels;
		private string[] panelNames;
		private int selectIndex = 0;
		private UIPrefabExector exector;
        protected override void OnEnable()
        {
            base.OnEnable();

            exector = target as UIPrefabExector;

            if (exector == null) return;			

			panels = exector.GetComponentsInChildren<BasePanel>(true);
			if (panels != null)
			{
				panelNames = new string[panels.Length + 1];
				panelNames[0] = "None";
				for (int i = 1; i < panelNames.Length; i++)
				{
					panelNames[i] = panels[i - 1].name;
				}
			}
			if (exector.defaultPanel == null)
				selectIndex = 0;

			if (exector.defaultPanel != null && panels != null)
			{
				for (int i = 0; i < panels.Length; i++)
				{
					if (exector.defaultPanel == panels[i])
					{
						selectIndex = i + 1;
						break;
					}
				}
			}

        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

			EditorGUI.BeginChangeCheck();
			if (panelNames != null || panelNames.Length > 0)
			{
				selectIndex = EditorGUILayout.Popup("设置默认开启的面板",selectIndex, panelNames);	

				if (selectIndex != 0 && panels != null)
				{
					try
					{
						exector.defaultPanel = panels[selectIndex - 1];
					}
					catch { }
				}
				else if(selectIndex == 0)
				{
					exector.defaultPanel = null;                   
                }

			}

			if (EditorGUI.EndChangeCheck())
			{
				target.Save();
			}
			
        }
    }
#endif
}
