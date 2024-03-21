///=====================================================
/// - FileName:      ViewControllerEditor.cs
/// - NameSpace:     YukiFrameWork.Project
/// - Created:       Yuki
/// - Email:         1274672030@qq.com
/// - Description:   编辑器拓展控制器
/// - Creation Time: 2024/1/15 1:50:56
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using YukiFrameWork.Events;
using System.IO;
using System.Text;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
namespace YukiFrameWork.Extension
{
    [CustomEditor(typeof(ViewController), true)]
    [CanEditMultipleObjects]
    public class ViewControllerEditor : CustomInspectorEditor
    {
        private List<string> list = new List<string>();
        [MenuItem("GameObject/YukiFrameWork/Create ViewController",false,-1000)]
        private static void CreateViewController()
        {           
            GameObject gameObject = Selection.activeGameObject;

            if (gameObject == null)
            {
                gameObject = new GameObject("ViewController");

                Undo.RegisterCreatedObjectUndo(gameObject, "Create ViewController");
            }

            if (gameObject.GetComponent<ViewController>() != null)
            {
                $"当前不需要为这个对象添加ViewController,脚本已存在。GameObject Name: {gameObject.name}".LogInfo(Log.W);
                return;
            }

            Undo.AddComponent<ViewController>(gameObject);
        }

        private void Awake()
        {         
            ViewController controller = target as ViewController;
            if (controller == null) return;
            controller.Data ??= new CustomData();

            if (string.IsNullOrEmpty(controller.Data.ScriptName))
                controller.Data.ScriptName = (target.name == "ViewController" ? (target.name + "Example") : target.name);

            string scriptFilePath = controller.Data.ScriptPath + @"/" + controller.Data.ScriptName + ".cs";          
            if (controller.GetType().ToString().Equals(typeof(ViewController).ToString()))
            {             
                if(!Update_ScriptGenericScriptDataInfo(scriptFilePath, controller))
                    controller.Data.ScriptName = (target.name == "ViewController" ? (target.name + "Example") : target.name);                    
            }
        }

        protected override void OnEnable()
        {            
            base.OnEnable();
            ViewController controller = target as ViewController;
            if (controller == null) return;
        
            string scriptFilePath = controller.Data.ScriptPath + @"/" + controller.Data.ScriptName + ".cs";
                     
            if (controller.Data.OnLoading)
            {
                controller.Data.OnLoading = false;
                Update_ScriptGenericScriptDataInfo(scriptFilePath, controller);            
            }             
            LocalGenericScriptInfo info = Resources.Load<LocalGenericScriptInfo>("LocalGenericScriptInfo");
            controller.Data.ScriptNamespace = !info ? controller.Data.ScriptNamespace : info.nameSpace;
            
            if(controller.Data.IsPartialLoading)           
                EditorApplication.delayCall = () => BindAllField(controller);

            list = controller.Data.AutoInfos;
            list.Clear();
            list.Add("None");
            try
            {        
                var types = AssemblyHelper.GetTypes(Assembly.Load(info.assembly));
                if (types != null)
                {
                    foreach (var type in types)
                        if (type.BaseType != null)
                            foreach (var baseInterface in type.BaseType.GetInterfaces())
                                if (baseInterface.ToString().Equals(typeof(IArchitecture).ToString()) && type.GetCustomAttribute<NoGenericArchitectureAttribute>() == null)
                                    list.Add(type.Name);
                }
            }
            catch { }

        }

        private void OnDisable()
        { 
            LocalGenericScriptInfo info = Resources.Load<LocalGenericScriptInfo>("LocalGenericScriptInfo");
            if (info == null) return;

            ViewController controller = target as ViewController;
            if (controller == null) return;

            info.nameSpace = controller.Data.ScriptNamespace;
        }

        private void BindAllField(ViewController controller)
        {
            if (Application.isPlaying) return;

            controller.Data.IsPartialLoading = false;
            IEnumerable<FieldInfo> fieldInfos = controller.GetType().GetRuntimeFields();

            ISerializedFieldInfo serialized = controller;
            foreach (FieldInfo fieldInfo in fieldInfos)
            {
                SerializeFieldData data = serialized.GetSerializeFields().FirstOrDefault(x => x.fieldName.Equals(fieldInfo.Name));

                if (data == null) continue;
                if (data.type == null) continue;             
                if (!data.type.IsSubclassOf(typeof(Component)))
                    fieldInfo.SetValue(target, data.target);
                else
                {
                    Component component = data.GetComponent();                 
                    fieldInfo.SetValue(target, component);
                }
            }

            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssets();
        }
        public bool IsPlaying => Application.isPlaying;
        public override void OnInspectorGUI()
        {
            ViewController controller = target as ViewController;
            if (EditorApplication.isCompiling)
            {
                EditorGUILayout.HelpBox("Loading...", MessageType.Warning);
                return;
            }
            base.OnInspectorGUI();
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical("OL box NoExpand");
            GUIStyle style = new GUIStyle("AM HeaderStyle")
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 16,
            };
            style.normal.textColor = Color.white;
            style.fontStyle = FontStyle.Bold;
            GUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            GUILayout.Label(GenericScriptDataInfo.TitleTip, style);
            EditorGUILayout.BeginHorizontal(GUILayout.Width(100));
            GUILayout.Label("EN");
            GenericScriptDataInfo.IsEN = EditorGUILayout.Toggle(GenericScriptDataInfo.IsEN);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndHorizontal();

            EditorGUI.BeginDisabledGroup(IsPlaying);
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            var Data = controller.Data;
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
            CodeManager.SelectFolder(Data);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginDisabledGroup(!target.GetType().Equals(typeof(ViewController)));
            GUILayout.Label(GenericScriptDataInfo.AutoInfo, GUILayout.Width(GenericScriptDataInfo.IsEN ? 300 : 200));
            Data.IsAutoMation = EditorGUILayout.Toggle(Data.IsAutoMation, GUILayout.Width(50));
            EditorGUILayout.EndHorizontal();
            if (Data.IsAutoMation)
            {
                EditorGUILayout.Space(10);
                SelectArchitecture(Data);
            }
            EditorGUILayout.Space(10);
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space(20);
            CodeManager.GenericControllerScripts(Data);

            EditorGUI.EndDisabledGroup();
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(target);
                AssetDatabase.SaveAssets();
            }
            EditorGUILayout.EndVertical();
            if (!target.GetType().Equals(typeof(ViewController)))
            {
                CodeManager.BindInspector(controller,controller, GenericPartialScripts);
                AddEventCenter(controller);
            }
        }
        private void SelectArchitecture(CustomData Data)
        {
            EditorGUILayout.BeginHorizontal(GUILayout.Width(300));
            EditorGUILayout.LabelField(GenericScriptDataInfo.IsEN ? "Select Architecture:" : "架构选择:", GUILayout.Width(120));
            Data.AutoArchitectureIndex = EditorGUILayout.Popup(Data.AutoArchitectureIndex, list.ToArray());
            EditorGUILayout.EndHorizontal();
        }
     
        private void AddEventCenter(ViewController controller)
        {
            RuntimeEventCenter center = controller.GetComponent<RuntimeEventCenter>();
            EditorGUI.BeginDisabledGroup(center);           
            if (GUILayout.Button(GenericScriptDataInfo.AddEventInfo, GUILayout.Height(30)))
            {
                Undo.AddComponent<RuntimeEventCenter>(controller.gameObject);
            }
            EditorGUI.EndDisabledGroup();

            if (center)
            {
                EditorGUILayout.BeginHorizontal(GUILayout.Width(150));              
                controller.initialized = (RuntimeInitialized)EditorGUILayout.EnumPopup(controller.initialized);
                EditorGUILayout.EndHorizontal();

                switch (controller.initialized)
                {
                    case RuntimeInitialized.Automation:
                        GUILayout.Label(GenericScriptDataInfo.EventAudioMationInfo);
                        break;
                    case RuntimeInitialized.Awake:
                        GUILayout.Label(GenericScriptDataInfo.EventAwakeInfo);
                        break;                 
                }
            }
        }               

        private void GenericPartialScripts()
        {
            ViewController controller = target as ViewController;
            if (controller == null) return;
            StringBuilder builder = new StringBuilder();

            string examplePath = controller.Data.ScriptPath + "/" + controller.Data.ScriptName + ".Example.cs";
            bool intited = File.Exists(examplePath);
            FileMode fileMode = intited ? FileMode.Open : FileMode.Create;
            if (intited)
            {
                File.WriteAllText(examplePath,string.Empty);
                AssetDatabase.Refresh();
            }
            builder.AppendLine("///=====================================================");
            builder.AppendLine("///这是由代码工具生成的代码文件,请勿手动改动此文件!");
            builder.AppendLine("///如果在代码里命名空间进行了变动,请在编辑器设置也对命名空间作出相同修改!");
            builder.AppendLine("///=====================================================");

            builder.AppendLine("using YukiFrameWork;");
            builder.AppendLine("using UnityEngine;");
            builder.AppendLine("using System;");
            builder.AppendLine();

            builder.AppendLine($"namespace {controller.Data.ScriptNamespace}");
            builder.AppendLine("{");

            builder.AppendLine($"\tpublic partial class {controller.Data.ScriptName}");
            builder.AppendLine("\t{");
            ISerializedFieldInfo serialized = controller as ISerializedFieldInfo;
            foreach (var info in serialized.GetSerializeFields())
            {
                builder.AppendLine($"\t\t[SerializeField]{info.fieldLevel[info.fieldLevelIndex]} {info.Components[info.fieldTypeIndex]} {info.fieldName};");
            }
            builder.AppendLine("\t}");

            builder.AppendLine("}");



            using (FileStream stream = new FileStream(examplePath, fileMode, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                StreamWriter streamWriter = new StreamWriter(stream,Encoding.UTF8);

                streamWriter.Write(builder);

                streamWriter.Close();
                stream.Close();
                controller.Data.IsPartialLoading = true;              
                AssetDatabase.Refresh();
            }
        }

        private bool Update_ScriptGenericScriptDataInfo(string path,ViewController controller)
        {           
            MonoScript monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(path);  
            if(monoScript == null) return false;
            if (!monoScript.GetClass().IsSubclassOf(typeof(ViewController))) return false;
            var component = controller.gameObject.AddComponent(monoScript.GetClass());
            ViewController currentController = component as ViewController;            
            currentController.Data = controller.Data;                      

            DestroyImmediate(controller);
            return true;
        }           
    }
}
#endif