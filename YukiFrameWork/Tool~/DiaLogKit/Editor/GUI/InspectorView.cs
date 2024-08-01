///=====================================================
/// - FileName:      InspectorView.cs
/// - NameSpace:     YukiFrameWork.DiaLogue
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/7/27 18:57:22
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using UnityEngine.UIElements;

#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using UnityEditor;
namespace YukiFrameWork.DiaLogue
{
	public class InspectorView : VisualElement
    {
		public new class UxmlFactory : UxmlFactory<InspectorView, UxmlTraits> { }

		private OdinEditor editor = null;
		public void Update_InspectorView(GraphNodeView node)
		{
			Clear();

			if (editor != null)
				UnityEngine.Object.DestroyImmediate(editor);

			editor = Editor.CreateEditor(node.node, typeof(OdinEditor)) as OdinEditor;

			IMGUIContainer container = new IMGUIContainer() { onGUIHandler = () => 
			{
				try
				{
					if (editor.target == null) return;
					editor.OnInspectorGUI();
				}
				catch { }

            } };

			Add(container);
		}
	}
}
#endif