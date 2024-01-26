#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using YukiFrameWork.Extension;

namespace YukiFrameWork.UI
{
    public class UIBaseLayer : GenericLayer
    {
        private UICustomData Data;
       
        public UIBaseLayer(GenericDataBase data, Type targetType) : base(data, targetType)
        {
            this.Data = data as UICustomData;
            
        }

        public override void OnInspectorGUI()
        {
            GUILayout.BeginVertical("OL box NoExpand");
            GUIStyle style = new GUIStyle("AM HeaderStyle")
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 16,
            };
            style.normal.textColor = Color.white;
            style.fontStyle = FontStyle.Bold;
            GUILayout.BeginHorizontal();
            GUILayout.Label(GenericScriptDataInfo.TitleTip, style);
            EditorGUILayout.BeginHorizontal(GUILayout.Width(100));
            GUILayout.Label("EN");
            GenericScriptDataInfo.IsEN = EditorGUILayout.Toggle(GenericScriptDataInfo.IsEN);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUI.BeginDisabledGroup(IsPlaying);
            EditorGUILayout.BeginHorizontal();

            GUILayout.Label(GenericScriptDataInfo.Email, GUILayout.Width(200));
            Data.CreateEmail = EditorGUILayout.TextField(Data.CreateEmail);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            Data.SystemNowTime = DateTime.Now.ToString();

            EditorGUILayout.BeginHorizontal();

            GUILayout.Label(GenericScriptDataInfo.NameSpace, GUILayout.Width(200));
            Data.ScriptNamespace = EditorGUILayout.TextField(Data.ScriptNamespace);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();

            GUILayout.Label(GenericScriptDataInfo.Name, GUILayout.Width(200));
            Data.ScriptName = EditorGUILayout.TextField(Data.ScriptName);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();

            GUILayout.Label(GenericScriptDataInfo.Path, GUILayout.Width(200));
            GUILayout.TextField(Data.ScriptPath);
            SelectFolder(Data);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUI.EndDisabledGroup();
            GenericScripts(Data, "UIPanelScripts","由框架创建的UI面板生命周期类");
            GUILayout.EndVertical();
        }

    }
}
#endif