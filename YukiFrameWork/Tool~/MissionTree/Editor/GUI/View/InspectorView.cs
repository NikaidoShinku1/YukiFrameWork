///=====================================================
/// - FileName:      InspectorView.cs
/// - NameSpace:     YukiFrameWork.Behaviours
/// - Description:   高级定制脚本生成
/// - Creation Time: 2024/11/14 17:23:33
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================

#if UNITY_EDITOR
using YukiFrameWork;
using UnityEngine;
using UnityEngine.UIElements;

using System;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEditor.UIElements;
namespace YukiFrameWork.Missions
{
    public class InspectorView : VisualElement
    {

        public new class UxmlFactory : UxmlFactory<InspectorView, UxmlTraits>
        {
            
        }
        private OdinEditor editor;
        private Vector2 position;
        public void Update_InspectorView(GraphMissionView node)
        {
            Clear();

            if (editor != null)
                UnityEngine.Object.DestroyImmediate(editor);

            editor = Editor.CreateEditor(node.Mission as ScriptableObject, typeof(OdinEditor)) as OdinEditor;

            IMGUIContainer container = new IMGUIContainer()
            {
                onGUIHandler = () =>
                {
                    try
                    {
                        if (editor.target == null) return;
                        position = EditorGUILayout.BeginScrollView(position);
                        editor.OnInspectorGUI();
                        EditorGUILayout.EndScrollView();
                    }
                    catch { }

                }
            };

            Add(container);
        }
        
    }
}
#endif