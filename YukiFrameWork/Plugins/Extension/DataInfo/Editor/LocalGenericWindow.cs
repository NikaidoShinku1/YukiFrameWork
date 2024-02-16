using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using YukiFrameWork.Extension;

namespace YukiFrameWork
{
#if UNITY_EDITOR
    public class LocalGenericWindow : EditorWindow
    {     
        [MenuItem("YukiFrameWork/Local Scripts Generator")]
        private static void OpenWindow()
        {
            var window = GetWindow<LocalGenericWindow>();

            window.titleContent = new GUIContent("脚本生成器");
            window.Show();
        }

        private LocalGenericScriptInfo info;
        private SerializedProperty nameProperty;
        private SerializedProperty namesPaceProperty;
        private SerializedProperty pathProperty;
        private SerializedProperty parentProperty;
        private SerializedProperty parentNameProperty;
        private SerializedObject serializedObject;
        private void OnEnable()
        {
            Update_Info();
        }

        private void OnGUI()
        {
            if (info == null) 
            {
                Update_Info();
                return;
            }
            GUILayout.BeginVertical("OL box NoExpand");
            EditorGUI.BeginChangeCheck();
            EditorGUI.BeginDisabledGroup(Application.isPlaying);
            serializedObject.Update();
            GenericScriptDataInfo.IsEN = EditorGUILayout.ToggleLeft("EN",GenericScriptDataInfo.IsEN,GUILayout.Width(50));

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(nameProperty, new GUIContent(GenericScriptDataInfo.Name), parentProperty.boolValue ? GUILayout.Width(position.width / 2) : GUILayout.Width(position.width -70));

            if (info.IsParent)
            {
                GUILayout.Label(":", GUILayout.Width(10));
                EditorGUILayout.PropertyField(parentNameProperty, new GUIContent(), GUILayout.Width((position.width - 60) / 2.5f));
            }
            else parentNameProperty.stringValue = "MonoBehaviour";
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(GenericScriptDataInfo.IsEN ? "Inherit" : "继承");
            parentProperty.boolValue = EditorGUILayout.Toggle(parentProperty.boolValue);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(namesPaceProperty, new GUIContent(GenericScriptDataInfo.NameSpace));

            EditorGUILayout.Space();

            var rect = EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(pathProperty, new GUIContent(GenericScriptDataInfo.Path));
            DragObject(rect,out var dragPath);
            if (!string.IsNullOrEmpty(dragPath))
                pathProperty.stringValue = dragPath;
            if (GUILayout.Button("...",GUILayout.Width(30)))
            {
                pathProperty.stringValue = string.Empty;
                string path = EditorUtility.OpenFolderPanel("path", pathProperty.stringValue, "");

                bool append = false;

                string[] values = path.Split('/');

                for (int i = 0; i < values.Length; i++)
                {
                    if (values[i].Contains("Assets") || values[i] == "Assets")
                    {
                        append = true;
                    }
                    if (append)
                    {
                        if (i < values.Length - 1)
                            pathProperty.stringValue += values[i] + "/";
                        else
                            pathProperty.stringValue += values[i];
                    }

                }
                 serializedObject.ApplyModifiedProperties();
                GUIUtility.ExitGUI();
            }
            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
            Generic();
            EditorGUI.EndDisabledGroup();
            EditorGUI.EndChangeCheck();           
        }

        private void DragObject(Rect rect,out string path)
        {
            Event e = Event.current;
            path = string.Empty;
            if (rect.Contains(e.mousePosition))
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Generic;

                if (e.type == EventType.DragPerform)
                {
                    var assets = DragAndDrop.objectReferences;
                    if (assets[0].GetType().Equals(typeof(DefaultAsset)))
                    {
                        path = AssetDatabase.GetAssetPath(assets[0]);
                    }
                    e.Use();
                }
            }           
        }

        private void Generic()
        {
            if (GUILayout.Button(GenericScriptDataInfo.GenerateScriptBtn, GUILayout.Height(30)))
            {
                if (string.IsNullOrEmpty(pathProperty.stringValue))
                {
                    Debug.LogError((GenericScriptDataInfo.IsEN ? "Cannot create script because path is empty!" : "路径为空无法创建脚本!"));
                    return;
                }
                StringBuilder builder = new StringBuilder();
                builder.AppendLine("///=====================================================");
                builder.AppendLine("/// - FileName:      " + info.scriptName + ".cs");
                builder.AppendLine("/// - NameSpace:     " + info.nameSpace);
                builder.AppendLine("/// - Description:   通过本地的代码生成器创建的脚本");
                builder.AppendLine("/// - Creation Time: " + System.DateTime.Now.ToString());
                builder.AppendLine("/// -  (C) Copyright 2008 - 2024");
                builder.AppendLine("/// -  All Rights Reserved.");
                builder.AppendLine("///=====================================================");

                builder.AppendLine("using YukiFrameWork;");
                builder.AppendLine("using UnityEngine;");
                builder.AppendLine("using System;");
                builder.AppendLine($"namespace {info.nameSpace}");
                builder.AppendLine("{");
                builder.AppendLine($"\tpublic class {info.scriptName}{(info.IsParent ? $" : {info.parentName}" : string.Empty)}");
                builder.AppendLine("\t{");
                builder.AppendLine("");
                builder.AppendLine("\t}");

                builder.AppendLine("}");             

                if (!Directory.Exists(pathProperty.stringValue))
                {
                    Directory.CreateDirectory(pathProperty.stringValue);
                    AssetDatabase.Refresh();
                }
                string scriptFilePath = pathProperty.stringValue + @"/" + nameProperty.stringValue + ".cs";
                if (File.Exists(scriptFilePath))
                {
                    Debug.LogError((GenericScriptDataInfo.IsEN ? $"Scripts already exist in this folder! Path:{scriptFilePath}" : $"脚本已经存在该文件夹! Path:{scriptFilePath}"));
                    return;
                }

                using (FileStream fileStream = new FileStream(scriptFilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    StreamWriter streamWriter = new StreamWriter(fileStream, Encoding.UTF8);

                    streamWriter.Write(builder);

                    streamWriter.Close();

                    fileStream.Close();
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }
        private void Update_Info()
        {
            info = Resources.Load<LocalGenericScriptInfo>("LocalGenericScriptInfo");

            if (info == null)
            {
                info = ScriptableObject.CreateInstance<LocalGenericScriptInfo>();
                
                string infoPath = "Assets/Resources";
                if (!Directory.Exists(infoPath))
                {
                    Directory.CreateDirectory(infoPath);
                    AssetDatabase.Refresh();
                }

                AssetDatabase.CreateAsset(info, infoPath + "/LocalGenericScriptInfo.asset");                
                AssetDatabase.Refresh();
                EditorUtility.SetDirty(info);
                AssetDatabase.SaveAssets();
            }

            serializedObject = new SerializedObject(info);
            nameProperty = serializedObject.FindProperty("scriptName");
            namesPaceProperty = serializedObject.FindProperty("nameSpace");
            pathProperty = serializedObject.FindProperty("genericPath");
            parentProperty = serializedObject.FindProperty("IsParent");
            parentNameProperty = serializedObject.FindProperty("parentName");
        }
    }
#endif  
}
