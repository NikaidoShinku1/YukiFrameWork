using Sirenix.OdinInspector;
using System.IO;
using System.Text;
#if UNITY_EDITOR
using UnityEditor;
using Sirenix.OdinInspector.Editor;
#endif
using UnityEngine;
using YukiFrameWork.Extension;
namespace YukiFrameWork
{
#if UNITY_EDITOR
    public class LocalConfigWindow : OdinEditorWindow
    {        
        [InitializeOnLoadMethod]
        public static void Init()
        {       
            var info = Resources.Load<FrameworkConfigInfo>("FrameworkConfigInfo");

            if (info == null)
            {
                info = ScriptableObject.CreateInstance<FrameworkConfigInfo>();

                string infoPath = "Assets/Resources";
                if (!Directory.Exists(infoPath))
                {
                    Directory.CreateDirectory(infoPath);
                    AssetDatabase.Refresh();
                }

                AssetDatabase.CreateAsset(info, infoPath + "/FrameworkConfigInfo.asset");
                AssetDatabase.Refresh();
                EditorUtility.SetDirty(info);
                AssetDatabase.SaveAssets();
            }           
        }

        [MenuItem("YukiFrameWork/LocalConfiguration")]
        private static void OpenWindow()
        {
            var window = GetWindow<LocalConfigWindow>();

            window.titleContent = new GUIContent("框架本地配置");
            window.Show();
        }

        private FrameworkConfigInfo info;
        private SerializedProperty nameProperty;
        private SerializedProperty namesPaceProperty;
        private SerializedProperty pathProperty;
        private SerializedProperty parentProperty;
        private SerializedProperty parentNameProperty;
        private SerializedObject serializedObject;
        private SerializedProperty assemblyProperty;
        private GUIContent defaultContent = new GUIContent();
        private GUIStyle boxStyle;

        private OdinEditor editor;
        protected override void OnEnable()
        {
            Update_Info();
            editor = OdinEditor.CreateEditor(info,typeof(OdinEditor)) as OdinEditor;
            base.OnEnable();        
            defaultContent = new GUIContent();                    
            wantsMouseEnterLeaveWindow = true;
            wantsMouseMove = true;
        }           
        private string[] ArrayInfo
        {
            get => new string[] { FrameWorkConfigData.GenericScriptInfo, FrameWorkConfigData.RuntimeLocalization,FrameWorkConfigData.RuntimeDepandAssembly};
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            OnBeforeSerialize();       
        }      
        protected override void OnImGUI()
        {
            if (info == null)
            {
                Update_Info();
                return;
            }
            EditorGUILayout.Space(5);
            info.SelectIndex = GUILayout.Toolbar(info.SelectIndex,ArrayInfo);         
            EditorGUILayout.Space(5);
            serializedObject.Update();
            GUILayout.BeginVertical("OL box NoExpand");
            EditorGUI.BeginChangeCheck();
            EditorGUI.BeginDisabledGroup(Application.isPlaying);
            FrameWorkConfigData.IsEN = EditorGUILayout.ToggleLeft("EN", FrameWorkConfigData.IsEN, GUILayout.Width(50));
            if (info.SelectIndex == 0)
            {
                DrawScriptGenericWindow();
            }
            else if (info.SelectIndex == 1)
            {
                DrawLocalizationWindow();
            }
            else if(info.SelectIndex == 2)
            {
                DrawAssemblyBindSettingWindow();             
            }
            base.OnImGUI();
            EditorGUILayout.EndVertical();
            EditorGUI.EndDisabledGroup();
            EditorGUI.EndChangeCheck();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawLocalizationWindow()
        {
            GUILayout.Label(FrameWorkConfigData.LocalizationInfo);       
            editor.DrawDefaultInspector();
        }
        private int selectIndex => info.SelectIndex;
        [Button("为配置生成代码"),ShowIf(nameof(selectIndex),1)]
        [InfoBox("生成的代码为配置的标识,类名为Localization,请注意:配置标识跟配置内对应文本的标识不能出现一致的情况，否则会出问题")]
        void GenericCode(string codePath = "Assets/Scripts/YukiFrameWork/Code")
        {
            if (info == null) return;

            if (info.dependConfigs.Count == 0)
            {
                Debug.LogWarning("没有添加配置无法生成代码");
                return;
            }           
            CodeCore codeCore = new CodeCore();
            CodeWriter codeWriter = new CodeWriter();
            foreach (var value in info.dependConfigs)
            {
                string configKey = value.Key;
                codeWriter.CustomCode($"public static string ConfigKey_{configKey} = \"{configKey}\";");
                LocalizationConfigBase localizationConfig = value.Value;
                foreach (var key in localizationConfig.ConfigKeys)
                {
                    codeWriter.CustomCode($"public static string Key_{key} = \"{key}\";");
                }
            }
            codeCore.Descripton("LocalizationKit", namesPaceProperty.stringValue, "这是本地化套件生成的用于快速调用标识的类，用于标记所有的标识", System.DateTime.Now.ToString());
            codeCore.Using("UnityEngine")
            .Using("System")
            .Using(namesPaceProperty.stringValue)
            .EmptyLine()
            .CodeSetting(namesPaceProperty.stringValue, "Localization", string.Empty, codeWriter, false, false, false)
            .builder.CreateFileStream(codePath, "Localization", ".cs");
            
        }

        private void DrawAssemblyBindSettingWindow()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginHorizontal(GUI.skin.box);
            GUI.color = Color.red;
            GUILayout.Label(FrameWorkConfigData.AssemblyInfo);
            GUI.color = Color.white;
            EditorGUILayout.PropertyField(assemblyProperty, defaultContent);
            EditorGUILayout.EndHorizontal();
            boxStyle ??= new GUIStyle(GUI.skin.box);
            boxStyle.normal.textColor = Color.white;
            GUILayout.Label(FrameWorkConfigData.AssemblyDependInfo, boxStyle, GUILayout.Width(position.width));
            editor.DrawDefaultInspector();
        }

        private void DrawScriptGenericWindow()
        {         
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(nameProperty, new GUIContent(FrameWorkConfigData.Name), parentProperty.boolValue ? GUILayout.Width(position.width / 2) : GUILayout.Width(position.width - 70));

            if (info.IsParent)
            {
                GUILayout.Label(":", GUILayout.Width(10));
                EditorGUILayout.PropertyField(parentNameProperty, new GUIContent(), GUILayout.Width((position.width - 60) / 2.5f));
            }
            else parentNameProperty.stringValue = "MonoBehaviour";
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(FrameWorkConfigData.IsEN ? "Inherit" : "继承");
            parentProperty.boolValue = EditorGUILayout.Toggle(parentProperty.boolValue);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(namesPaceProperty, new GUIContent(FrameWorkConfigData.NameSpace));

            EditorGUILayout.Space();

            var rect = EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(pathProperty, new GUIContent(FrameWorkConfigData.Path));
            DragObject(rect, out var dragPath);
            if (!string.IsNullOrEmpty(dragPath))
                pathProperty.stringValue = dragPath;
            if (GUILayout.Button("...", GUILayout.Width(30)))
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
            EditorGUILayout.EndHorizontal();           
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
            if (GUILayout.Button(FrameWorkConfigData.GenerateScriptBtn, GUILayout.Height(30)))
            {
                if (string.IsNullOrEmpty(pathProperty.stringValue))
                {
                    Debug.LogError((FrameWorkConfigData.IsEN ? "Cannot create script because path is empty!" : "路径为空无法创建脚本!"));
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
                    Debug.LogError((FrameWorkConfigData.IsEN ? $"Scripts already exist in this folder! Path:{scriptFilePath}" : $"脚本已经存在该文件夹! Path:{scriptFilePath}"));
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
            info = Resources.Load<FrameworkConfigInfo>("FrameworkConfigInfo");

            if (info == null)
            {
                info = ScriptableObject.CreateInstance<FrameworkConfigInfo>();
                
                string infoPath = "Assets/Resources";
                if (!Directory.Exists(infoPath))
                {
                    Directory.CreateDirectory(infoPath);
                    AssetDatabase.Refresh();
                }

                AssetDatabase.CreateAsset(info, infoPath + "/FrameworkConfigInfo.asset");                
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
            assemblyProperty = serializedObject.FindProperty("assembly");         
        }
    }

    public static class UnityEngineSavingExtension
    {
        public static void Save<T>(this T core) where T : UnityEngine.Object
        {
            AssetDatabase.SaveAssets();
            EditorUtility.SetDirty(core);
            AssetDatabase.Refresh();
        }
    }
#endif  
}
