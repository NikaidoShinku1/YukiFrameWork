///=====================================================
/// - FileName:      CodeManager.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/3/19 22:50:51
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using UnityEngine;
using System;
using System.IO;
using System.Text;
using YukiFrameWork.Extension;
using System.Linq;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
namespace YukiFrameWork
{

    public class CodeWriter
    {
        public List<string> codes = new List<string>();
        public CodeWriter CustomCode(string code)
        {
            codes.Add(code);
            return this;
        }
    }

    public class CodeCore
    {
        public StringBuilder builder = new StringBuilder();
        public CodeCore Descripton(string fileName,string nameSpace,string description,string dateTime)
        {
            builder.AppendLine("///=====================================================");
            builder.AppendLine("/// - FileName:      " + fileName + ".cs");
            builder.AppendLine("/// - NameSpace:     " + nameSpace);
            builder.AppendLine("/// - Description:   " + description);
            builder.AppendLine("/// - Creation Time: " + dateTime);
            builder.AppendLine("/// -  (C) Copyright 2008 - 2024");
            builder.AppendLine("/// -  All Rights Reserved.");
            builder.AppendLine("///=====================================================");
            return this;
        }

        public CodeCore Descripton(string description)
        {
            builder.AppendLine(description);          
            return this;
        }

        public CodeCore Using(string us)
        {
            builder.AppendLine($"using {us};");
            return this; 
        }

        public CodeCore EmptyLine()
        {
            builder.AppendLine();
            return this;
        }

        public CodeCore CodeSetting(string nameSpace, string className, string parentClassName,CodeWriter writer, bool isStatic = false,bool isPartial = false, params string[] interfaceNames)
        {
            builder.AppendLine($"namespace {nameSpace}");
            builder.AppendLine("{");                     
            builder.AppendLine($"\tpublic {(isStatic?"static" + " ": string.Empty)}{(isPartial? "partial" + " ": string.Empty)}class {className}{(string.IsNullOrEmpty(parentClassName)?string.Empty:$" : {parentClassName}")}");
            builder.AppendLine("\t{");
            if (writer == null)
            {
                builder.AppendLine("");
            }
            else
            {
                foreach (var info in writer.codes)
                {
                    builder.AppendLine($"\t\t{info}");
                }
            }
            builder.AppendLine("\t}");
            builder.AppendLine("}");
            return this;
        }
    }
    public static class CodeManager
	{           
        public static void GenericControllerScripts(CustomData Data)
        {
            string scriptFilePath = Data.ScriptPath + @"/" + Data.ScriptName + ".cs";

            if (!File.Exists(scriptFilePath))
            {
                if (GUILayout.Button(FrameWorkConfigData.GenerateScriptBtn, GUILayout.Height(30)))
                {
                    StringBuilder builder = new StringBuilder();                 
                    builder.AppendLine("///=====================================================");
                    builder.AppendLine("/// - FileName:      " + Data?.ScriptName + ".cs");
                    builder.AppendLine("/// - NameSpace:     " + Data?.ScriptNamespace);
                    builder.AppendLine("/// - Description:   框架自定ViewController");
                    builder.AppendLine("/// - Creation Time: " + System.DateTime.Now.ToString());
                    builder.AppendLine("/// -  (C) Copyright 2008 - 2024");
                    builder.AppendLine("/// -  All Rights Reserved.");
                    builder.AppendLine("///=====================================================");

                    builder.AppendLine("using YukiFrameWork;");
                    builder.AppendLine("using UnityEngine;");
                    builder.AppendLine("using System;");
                    builder.AppendLine($"namespace {Data?.ScriptNamespace}");
                    builder.AppendLine("{");
                    if (Data.IsAutoMation && Data.AutoArchitectureIndex != 0)
                        builder.AppendLine($"\t[RuntimeInitializeOnArchitecture(typeof({Data?.AutoInfos[Data.AutoArchitectureIndex]}),true)]");
                    builder.AppendLine($"\tpublic partial class {Data?.ScriptName} : ViewController");
                    builder.AppendLine("\t{");
                    builder.AppendLine("");
                    builder.AppendLine("\t}");

                    builder.AppendLine("}");
                    if (string.IsNullOrEmpty(Data.ScriptPath))
                    {
                        Debug.LogError((FrameWorkConfigData.IsEN ? "Cannot create script because path is empty!" : "路径为空无法创建脚本!"));
                        return;
                    }

                    if (!Directory.Exists(Data.ScriptPath))
                    {
                        Directory.CreateDirectory(Data.ScriptPath);
                        AssetDatabase.Refresh();
                    }

                    if (File.Exists(scriptFilePath))
                    {
                        Debug.LogError((FrameWorkConfigData.IsEN ? $"Scripts already exist in this folder! Path:{scriptFilePath}" : $"脚本已经存在该文件夹! Path:{scriptFilePath}"));
                        return;
                    }

                    using (FileStream fileStream = new FileStream(scriptFilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
                    {
                        StreamWriter streamWriter = new StreamWriter(fileStream, Encoding.UTF8);

                        streamWriter.Write(builder);

                        streamWriter.Close();

                        fileStream.Close();
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
                if (GUILayout.Button(FrameWorkConfigData.SelectScriptBtn, GUILayout.Height(30)))
                {
                    Selection.activeObject = monoScript;
                }
                string partialPath = Data.ScriptPath + @"/" + Data.ScriptName + ".Example" + ".cs";
                MonoScript partial = AssetDatabase.LoadAssetAtPath<MonoScript>(partialPath);

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(FrameWorkConfigData.OpenScriptBtn, GUILayout.Height(30)))
                {
                    EditorApplication.delayCall += () => AssetDatabase.OpenAsset(monoScript);
                }

                if (partial != null)
                {
                    if (GUILayout.Button(FrameWorkConfigData.OpenPartialScriptBtn, GUILayout.Height(30)))
                    {
                        EditorApplication.delayCall += () => AssetDatabase.OpenAsset(partial);
                    }
                }

                EditorGUILayout.EndHorizontal();
                GUILayout.EndVertical();
            }
        }

        public static void GenericPanelScripts(GenericDataBase Data)
        {
            string scriptFilePath = Data.ScriptPath + @"/" + Data.ScriptName + ".cs";

            if (!File.Exists(scriptFilePath))
            {
                if (GUILayout.Button(FrameWorkConfigData.GenerateScriptBtn, GUILayout.Height(30)))
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
                    builder.AppendLine("\t\t\tbase.OnInit();");
                    builder.AppendLine("\t\t}");
                    builder.AppendLine("\t\tpublic override void OnEnter()");
                    builder.AppendLine("\t\t{");
                    builder.AppendLine("\t\t\tbase.OnEnter();");
                    builder.AppendLine("\t\t}");
                    builder.AppendLine("\t\tpublic override void OnPause()");
                    builder.AppendLine("\t\t{");
                    builder.AppendLine("\t\t\tbase.OnPause();");
                    builder.AppendLine("\t\t}");
                    builder.AppendLine("\t\tpublic override void OnResume()");
                    builder.AppendLine("\t\t{");
                    builder.AppendLine("\t\t\tbase.OnResume();");
                    builder.AppendLine("\t\t}");
                    builder.AppendLine("\t\tpublic override void OnExit()");
                    builder.AppendLine("\t\t{");
                    builder.AppendLine("\t\t\tbase.OnExit();");
                    builder.AppendLine("\t\t}");
                    builder.AppendLine("");
                    builder.AppendLine("\t}");

                    builder.AppendLine("}");
                    if (string.IsNullOrEmpty(Data.ScriptPath))
                    {
                        Debug.LogError((FrameWorkConfigData.IsEN ? "Cannot create script because path is empty!" : "路径为空无法创建脚本!"));
                        return;
                    }

                    if (!Directory.Exists(Data.ScriptPath))
                    {
                        Directory.CreateDirectory(Data.ScriptPath);
                        AssetDatabase.Refresh();
                    }

                    if (File.Exists(scriptFilePath))
                    {
                        Debug.LogError((FrameWorkConfigData.IsEN ? $"Scripts already exist in this folder! Path:{scriptFilePath}" : $"脚本已经存在该文件夹! Path:{scriptFilePath}"));
                        return;
                    }

                    using (FileStream fileStream = new FileStream(scriptFilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
                    {
                        StreamWriter streamWriter = new StreamWriter(fileStream, Encoding.UTF8);

                        streamWriter.Write(builder);

                        streamWriter.Close();

                        fileStream.Close();
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
                if (GUILayout.Button(FrameWorkConfigData.SelectScriptBtn, GUILayout.Height(30)))
                {
                    Selection.activeObject = monoScript;
                }
                string partialPath = Data.ScriptPath + @"/" + Data.ScriptName + ".Example" + ".cs";
                MonoScript partial = AssetDatabase.LoadAssetAtPath<MonoScript>(partialPath);

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(FrameWorkConfigData.OpenScriptBtn, GUILayout.Height(30)))
                {
                    AssetDatabase.OpenAsset(monoScript);
                }

                if (partial != null)
                {
                    if (GUILayout.Button(FrameWorkConfigData.OpenPartialScriptBtn, GUILayout.Height(30)))
                    {
                        AssetDatabase.OpenAsset(partial);
                    }
                }

                EditorGUILayout.EndHorizontal();
                GUILayout.EndVertical();
            }
        }

        public static void SelectFolder<T>(T Data) where T : GenericDataBase
        {
            if (GUILayout.Button("...", GUILayout.Width(40)))
            {
                Data.ScriptPath = string.Empty;
                string path = EditorUtility.OpenFolderPanel("", Data.ScriptPath, "");

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
                            Data.ScriptPath += values[i] + "/";
                        else
                            Data.ScriptPath += values[i];
                    }

                }

                GUIUtility.ExitGUI();
            }
        }
        public static bool IsPlaying => Application.isPlaying;

        private static void SaveData(Component infoAsset)
        {
            if (PrefabUtility.IsPartOfPrefabInstance(infoAsset))
                PrefabUtility.RecordPrefabInstancePropertyModifications(infoAsset);

            EditorUtility.SetDirty(infoAsset);
            AssetDatabase.SaveAssets();
        }
        public static void BindInspector(ISerializedFieldInfo info,Component target, Action GenericCallBack = null)
        {       
            EditorGUI.BeginDisabledGroup(IsPlaying);
            EditorGUILayout.Space(20);
            var value = PlayerPrefs.GetInt("BindFoldOut") == 1;
            EditorGUILayout.BeginHorizontal();          
            PlayerPrefs.SetInt("BindFoldOut",EditorGUILayout.Foldout(PlayerPrefs.GetInt("BindFoldOut") == 1,string.Empty) ? 1 : 0);
            GUILayout.Label(FrameWorkConfigData.BindExtensionInfo, "PreviewPackageInUse");
            EditorGUILayout.EndHorizontal();
            if (value)
            {
                var rect = EditorGUILayout.BeginVertical("FrameBox", GUILayout.Height(100 + info.GetSerializeFields().Count() * 20));
                GUILayout.Label(FrameWorkConfigData.DragObjectInfo);
                foreach (var data in info.GetSerializeFields())
                {
                    EditorGUILayout.BeginHorizontal();
                    string fieldName = EditorGUILayout.TextField(data.fieldName);

                    if (data.fieldName != fieldName)
                    {
                        Undo.RecordObject(target, "Change Data");
                        data.fieldName = fieldName;

                        SaveData(target);
                    }

                    int levelIndex = EditorGUILayout.Popup(data.fieldLevelIndex, data.fieldLevel);

                    if (data.fieldLevelIndex != levelIndex)
                    {
                        Undo.RecordObject(target, "Change Level");
                        data.fieldLevelIndex = levelIndex;
                        SaveData(target);
                    }

                    int typeIndex = EditorGUILayout.Popup(data.fieldTypeIndex, data.Components?.ToArray());

                    if (typeIndex != data.fieldTypeIndex)
                    {
                        Undo.RecordObject(target, "Change TypeIndex");
                        data.fieldTypeIndex = typeIndex;
                        SaveData(target);
                    }

                    var obj = EditorGUILayout.ObjectField(data.target, typeof(UnityEngine.Object), true);

                    if (data.target != obj)
                    {
                        Undo.RecordObject(target, "Change Object");
                        data.target = obj;
                        SaveData(target);
                    }

                    if (GUILayout.Button("", "ToggleMixed"))
                    {
                        Undo.RecordObject(target, "Remove Data");
                        info.RemoveFieldData(data);

                        SaveData(target);
                        break;
                    }

                    EditorGUILayout.EndHorizontal();

                }

                DragObject(rect, target, info);

                GUILayout.FlexibleSpace();
                if (info.GetSerializeFields().Count() > 0 && GUILayout.Button("生成代码", GUILayout.Height(25)))
                {
                    GenericCallBack?.Invoke();
                }

                EditorGUILayout.EndVertical();
                /* if (!target.GetType().Equals(targetType))
                 {
                     BindEventCallBack?.Invoke();
                 }*/
            }
            EditorGUI.EndDisabledGroup();
        }

        private static void DragObject(Rect rect,Component target,ISerializedFieldInfo info)
        {
            Event e = Event.current;

            if (rect.Contains(e.mousePosition))
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Generic;

                if (e.type == EventType.DragPerform)
                {
                    var assets = DragAndDrop.objectReferences;

                    foreach (var asset in assets)
                    {
                        Undo.RecordObject(target, "Add Data");
                        info.AddFieldData(new SerializeFieldData(asset));

                        SaveData(target);
                    }
                    e.Use();
                }
            }
        }
    }
}
#endif