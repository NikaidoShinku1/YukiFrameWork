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
        private int selectIndex = 0;
        [LabelText("Depend:"),ShowIf(nameof(selectIndex),2)]
        [SerializeField]
        private string[] assemblies;

        [DictionaryDrawerSettings(KeyLabel = "配置的唯一标识",ValueLabel = "配置文件")]       
        [SerializeField,LabelText("Configs:"),ShowIf(nameof(selectIndex),1)]
        private LocalizationConfigBase configBases;


        [InfoBox("运行时默认语言必须在配置中存在!",InfoMessageType.Warning)]
        [LabelText("运行时默认语言:"),ShowIf(nameof(selectIndex),1),PropertySpace(10)]
        public Language DefaultLanguage;       
        protected override void OnEnable() 
        {
            base.OnEnable();
            Update_Info();
            defaultContent = new GUIContent();
            OnAfterDeserialize();
            selectIndex = PlayerPrefs.GetInt("LocalConfigWindowSelectIndex");
        }

        protected override void OnAfterDeserialize()
        {
            assemblies = info.assemblies;
            configBases = info.configBases;
            DefaultLanguage = info.DefaultLanguage;           
        }

        protected override void OnBeforeSerialize()
        {
            info.assemblies = assemblies;
            info.configBases = configBases;
            info.DefaultLanguage = DefaultLanguage;       
        }
        private string[] ArrayInfo
        {
            get => new string[] { FrameWorkConfigData.GenericScriptInfo, FrameWorkConfigData.RuntimeLocalization,FrameWorkConfigData.RuntimeDepandAssembly};
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            OnBeforeSerialize();
            PlayerPrefs.SetInt("LocalConfigWindowSelectIndex", selectIndex);

        }
        protected override void OnImGUI()
        {
            if (info == null)
            {
                Update_Info();
                return;
            }
            EditorGUILayout.Space(5);
            selectIndex = GUILayout.Toolbar(selectIndex,ArrayInfo);         
            EditorGUILayout.Space(5);
            serializedObject.Update();
            GUILayout.BeginVertical("OL box NoExpand");
            EditorGUI.BeginChangeCheck();
            EditorGUI.BeginDisabledGroup(Application.isPlaying);
            FrameWorkConfigData.IsEN = EditorGUILayout.ToggleLeft("EN", FrameWorkConfigData.IsEN, GUILayout.Width(50));
            if (selectIndex == 0)
            {
                DrawScriptGenericWindow();
            }
            else if (selectIndex == 1)
            {
                DrawLocalizationWindow();
            }
            else if(selectIndex == 2)
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
#endif  
}
