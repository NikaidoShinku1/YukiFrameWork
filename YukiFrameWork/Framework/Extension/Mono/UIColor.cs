///=====================================================
/// - FileName:      UIColor.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/10/28 20:11:48
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;
using Sirenix.OdinInspector;
namespace YukiFrameWork
{
	
    [SerializeField]
	public class UIColor : YMonoBehaviour
	{
        private Graphic _graphic;
		internal Graphic graphic
		{
			get
			{
				if(!_graphic) 
					_graphic = GetComponent<Graphic>();
				return _graphic;
            }
		}
		public void Red()
		{
			SetGraphicColor("red");
		}

		public void Green()
		{
			SetGraphicColor("green");
		}

		public void White()
		{
			SetGraphicColor("white");
		}

		public void Black()
		{
			SetGraphicColor("black");
		}

		public void Blue()
		{
			SetGraphicColor("blue");
		}

		public void Cyan()
		{
			SetGraphicColor("cyan");
		}

		public void Yellow()
		{
			SetGraphicColor("yellow");
		}

		public void Gray()
		{
			SetGraphicColor("gray");
		}

		public void Grey()
		{
			SetGraphicColor("grey");
		}

		public void Set(Color color)
		{
			graphic.color = color;
		}
		public void SetGraphicColor(string color)
		{
			if (!graphic)
				throw new Exception("使用该组件必须存在于具备派生自Graphic的组件");
			
			switch (color)
			{
				case "red":
                    graphic.color = Color.red;
                    break;
				case "green":
					graphic.color = Color.green;
					break;
				case "white":
					graphic.color = Color.white;
					break;
				case "black":
					graphic.color = Color.black;
					break;
				case "blue":
					graphic.color = Color.blue;
					break;
				case "yellow":
					graphic.color = Color.yellow;
					break;
				case "cyan":
					graphic.color = Color.cyan;
					break;				
				case "gray":
					graphic.color = Color.gray;
					break;
				case "grey":
					graphic.color = Color.grey;
					break;
				default:
					break;
			}			
			
		}
	}

#if UNITY_EDITOR
	[UnityEditor.CustomEditor(typeof(UIColor))]
	public class UIColorEditor : Sirenix.OdinInspector.Editor.OdinEditor
	{
        public override void OnInspectorGUI()
        {
			UIColor color = target as UIColor;
			if (!color.graphic)
			{
				UnityEditor.EditorGUILayout.HelpBox("挂载该脚本，则该对象必须存在Graphic组件!", UnityEditor.MessageType.Warning);
			}
			else
			{
                UnityEditor.EditorGUILayout.HelpBox("通过注册该脚本中的SetGraphicColor方法即可。需要传递颜色参数，如果是Color.red就传入red即可。", UnityEditor.MessageType.Info);
            }
            base.OnInspectorGUI();

        }
    }
#endif
}
