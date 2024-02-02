#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Text;
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
            GenericScripts();
            GUILayout.EndVertical();
        }

        public override void GenericScripts()
        {
            string scriptFilePath = Data.ScriptPath + @"/" + Data.ScriptName + ".cs";

            if (!File.Exists(scriptFilePath))
            {
                if (GUILayout.Button(GenericScriptDataInfo.GenerateScriptBtn, GUILayout.Height(30)))
                {
                    StringBuilder builder = new StringBuilder();
                    builder.AppendLine("///=====================================================");
                    builder.AppendLine("/// - FileName:      " + Data?.ScriptName + ".cs");
                    builder.AppendLine("/// - NameSpace:     " + Data?.ScriptNamespace);
                    builder.AppendLine("/// - Description:   框架自定BasePanel");
                    builder.AppendLine("/// - Creation Time: " + System.DateTime.Now.ToString());
                    builder.AppendLine("/// -  (C) Copyright 2008 - 2024");
                    builder.AppendLine("/// -  All Rights Reserved.");
                    builder.AppendLine("///=====================================================");

                    builder.AppendLine("using YukiFrameWork.UI;");
                    builder.AppendLine("using UnityEngine;");
                    builder.AppendLine("using UnityEngine.UI;");
                    builder.AppendLine("using TMPro;");
                    builder.AppendLine($"namespace {Data?.ScriptNamespace}");
                    builder.AppendLine("{");                   
                    builder.AppendLine($"\tpublic partial class {Data?.ScriptName} : BasePanel");
                    builder.AppendLine("\t{");
                    builder.AppendLine("\t\tpublic override void OnInit()");
                    builder.AppendLine("\t\t{");
                    builder.AppendLine("");
                    builder.AppendLine("\t\t}");
                    builder.AppendLine("\t\tpublic override void OnEnter()");
                    builder.AppendLine("\t\t{");
                    builder.AppendLine("");
                    builder.AppendLine("\t\t}");
                    builder.AppendLine("\t\tpublic override void OnPause()");
                    builder.AppendLine("\t\t{");
                    builder.AppendLine("");
                    builder.AppendLine("\t\t}");
                    builder.AppendLine("\t\tpublic override void OnResume()");
                    builder.AppendLine("\t\t{");
                    builder.AppendLine("");
                    builder.AppendLine("\t\t}");
                    builder.AppendLine("\t\tpublic override void OnExit()");
                    builder.AppendLine("\t\t{");
                    builder.AppendLine("");
                    builder.AppendLine("\t\t}");
                    builder.AppendLine("");
                    builder.AppendLine("\t}");

                    builder.AppendLine("}");
                    if (string.IsNullOrEmpty(Data.ScriptPath))
                    {
                        Debug.LogError((GenericScriptDataInfo.IsEN ? "Cannot create script because path is empty!" : "路径为空无法创建脚本!"));
                        return;
                    }

                    if (!Directory.Exists(Data.ScriptPath))
                    {
                        Directory.CreateDirectory(Data.ScriptPath);
                        AssetDatabase.Refresh();
                    }

                    if (File.Exists(scriptFilePath))
                    {
                        Debug.LogError((GenericScriptDataInfo.IsEN ? $"Scripts already exist in this folder! Path:{scriptFilePath}" : $"脚本已经存在该文件夹! Path:{scriptFilePath}"));
                        return;
                    }

                    using (FileStream fileStream = new FileStream(scriptFilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
                    {
                        StreamWriter streamWriter = new StreamWriter(fileStream,Encoding.UTF8);

                        streamWriter.Write(builder);

                        streamWriter.Close();

                        fileStream.Close();
                        //正在改变脚本
                        Data.OnLoading = true;

                    }

                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();

                }
            }
            else
            {
                GUILayout.BeginVertical();
                MonoScript monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(scriptFilePath);
                if (GUILayout.Button(GenericScriptDataInfo.SelectScriptBtn, GUILayout.Height(30)))
                {
                    Selection.activeObject = monoScript;
                }
                string partialPath = Data.ScriptPath + @"/" + Data.ScriptName + ".Example" + ".cs";
                MonoScript partial = AssetDatabase.LoadAssetAtPath<MonoScript>(partialPath);

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(GenericScriptDataInfo.OpenScriptBtn, GUILayout.Height(30)))
                {
                    AssetDatabase.OpenAsset(monoScript);
                }

                if (partial != null)
                {
                    if (GUILayout.Button(GenericScriptDataInfo.OpenPartialScriptBtn, GUILayout.Height(30)))
                    {
                        AssetDatabase.OpenAsset(partial);
                    }
                }

                EditorGUILayout.EndHorizontal();
                GUILayout.EndVertical();
            }
        }

    }
}
#endif