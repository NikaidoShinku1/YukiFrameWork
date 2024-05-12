using Sirenix.OdinInspector.Editor;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif
using UnityEngine;

#if UNITY_EDITOR
namespace YukiFrameWork.DiaLogue
{
    public class DiaLogParamLayer : DiaLogGraphLayer
    {
        private OdinEditor nodeEditor;
        private Node current;
        private GUIStyle fontStyle;
        public DiaLogParamLayer(DiaLogEditorWindow editorWindow) : base(editorWindow)
        {
            
        }

        private void DrawBackGround(Rect rect)
        {
            EditorGUI.DrawRect(rect, ColorConst.ParamBackGround);
        }

        public override void OnGUI(Rect rect)
        {
            base.OnGUI(rect);
            DrawBackGround(rect);
            if (this.Context.NodeTree == null) return;

            if (fontStyle == null)
            {
                fontStyle = new GUIStyle();
                fontStyle.fontStyle = FontStyle.Bold;
                fontStyle.alignment = TextAnchor.MiddleCenter;
                fontStyle.normal.textColor = Color.white;
            }
            GUILayout.BeginVertical(GUILayout.Width(rect.width - 10));
            GUILayout.Space(5);
            GUILayout.Label(new GUIContent(this.Context.NodeTree.name), fontStyle);
            GUILayout.Space(10);
            GUI.enabled = !Application.isPlaying;
            if (this.Context.parameterState != null && this.Context.parameterState != current)
            {
                current = this.Context.parameterState;
                nodeEditor = OdinEditor.CreateEditor(current,typeof(OdinEditor)) as OdinEditor;              
            }
            if (nodeEditor != null)
            {
                if(nodeEditor.serializedObject.targetObject != null)                  
                    nodeEditor.DrawDefaultInspector();
            }
            GUILayout.EndVertical();
            GUI.enabled = true;

            
        }
    }
}
#endif