///=====================================================
/// - FileName:      Reddot.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2025/2/19 15:14:20
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using Sirenix.OdinInspector;
using YukiFrameWork.Events;
namespace YukiFrameWork
{
	public class Reddot : YMonoBehaviour
	{
		[SerializeField,LabelText("红点父级路径"),Required("红点的父路径不能为空")]
		[InfoBox("红点组件控制的是挂载对象子物体的开关。为红点Image设置一个父物体并挂载该组件\nParent为红点路径中的唯一标识。不能为空")]
		private string parent;

		[SerializeField,LabelText("红点路径")]
		[InfoBox("当红点路径为空时，该组件不会单独受到红点控制。在设计上作为父红点使用。例如A组件的Parent为Test，Path为空，B组件的Parent也是Test且Path不为空，此时当B组件的红点亮起，A也会亮起")]
		private string path;

		public string ReddotParent
		{
			get
			{
				return parent;
			}
			set
			{
				if (value.IsNullOrEmpty())
					throw new NullReferenceException("父级路径是不能为空的!");
                parent = value;
				Refresh(default);
            }
		}

		public string ReddotPath
		{
			get => path;
			set
			{
				path = value;
				Refresh(default);
			}
		}

        private void OnEnable()
        {
			Refresh(default);
			EventManager.AddListener<ChangeReddotArg>(Refresh);
        }

        private void OnDisable()
        {
			EventManager.RemoveListener<ChangeReddotArg>(Refresh);
        }

		private void Refresh(ChangeReddotArg _)
		{
			bool active = ReddotKit.IsReddotActive(parent,path);
			SetChileActive(active);
		}

		/// <summary>
		/// 公开的移除红点方法(可用于事件)
		/// </summary>
		public void Remove()	
		{
			ReddotKit.RemoveReddotPath(parent,path);
		}

		private void SetChileActive(bool active)
		{
			for (int i = 0; i < transform.childCount; i++)
			{
				Transform child = transform.GetChild(i);

				if (!child) continue;
				child.ShowOrHide(active);
			}
		}
    }
}
