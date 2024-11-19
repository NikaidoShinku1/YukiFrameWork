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
namespace YukiFrameWork.Behaviours
{
    public class InspectorView : VisualElement
    {

        public new class UxmlFactory : UxmlFactory<InspectorView, UxmlTraits>
        {
            
        }
        private AIBehaviourEditor editor;
        private OdinEditor soEditor;
        public int drawIndex = 0;
        public void Update_InspectorView(GraphBehaviourView node)
        {            
            if (drawIndex != 0) return;
            Clear();
            
            if (editor != null)
                UnityEngine.Object.DestroyImmediate(editor);       
             editor = Editor.CreateEditor(node.Behaviour, typeof(AIBehaviourEditor)) as AIBehaviourEditor;        
            IMGUIContainer container = new IMGUIContainer()
            {
                onGUIHandler = () =>
                {
                    try
                    {
                        if (editor.target == null) return;
                        editor.OnInspectorGUI();
                    }
                    catch { }

                }
            };

            Add(container);
        }

        public void Update_ParamView(BehaviourTreeSO behaviourTree)
        {
            if (drawIndex != 1)
                return;
            Clear();           
            if (soEditor != null)
                UnityEngine.Object.DestroyImmediate(soEditor);
            soEditor = Editor.CreateEditor(behaviourTree, typeof(OdinEditor)) as OdinEditor;
            IMGUIContainer container = new IMGUIContainer()
            {
                onGUIHandler = () =>
                {
                    try
                    {
                        if (soEditor.target == null) return;
                        soEditor.OnInspectorGUI();
                    }
                    catch { }

                }
            };

            Add(container);
        }

        public void Update_DebuggerView(ObjectField objectField)
        {
            if (drawIndex != 2)
                return;
            Clear();

            IMGUIContainer container = new IMGUIContainer()
            {
                onGUIHandler = () => 
                {
                    if (!Application.isPlaying)
                    {
                        EditorGUILayout.HelpBox("该窗口仅运行时可查看，编辑器模式下没有任何作用", MessageType.Warning);
                    }
                    else
                    {
                        EditorGUILayout.Space();
                        for (int i = 0; i < BehaviourManager.Instance.BehaviourTrees.Count;i++)
                        {
                            var item = BehaviourManager.Instance.BehaviourTrees[i];
                            if (GUILayout.Button(item.name + "_" + item.GetInstanceID()))
                            {
                                objectField.value = item.Source;
                            }
                        }
                    }
                }
            };

            Add(container);

        }

        
    }
}
#endif