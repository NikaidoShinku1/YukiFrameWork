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
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Sirenix.OdinInspector.Editor;
using YukiFrameWork;
namespace YukiFrameWork.Extension
{
    [CustomEditor(typeof(ViewController), true)]
    [CanEditMultipleObjects]
    public class ViewControllerEditor : OdinEditor
    {        
        private List<string> list = new List<string>();       
        [MenuItem("GameObject/YukiFrameWork/Create ViewController",false,-1000)]
        [MenuItem("YukiFrameWork/创建ViewController %Q")]
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
                $"当前不需要为这个对象添加ViewController,脚本已存在。GameObject Name: {gameObject.name}".LogWarning();
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
            if (!controller.GetType().IsSubclassOf(typeof(ViewController)))
            {             
                if(!Update_ScriptFrameWorkConfigData(scriptFilePath, controller))
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
                Update_ScriptFrameWorkConfigData(scriptFilePath, controller);            
            }             
            FrameworkConfigInfo info = Resources.Load<FrameworkConfigInfo>("FrameworkConfigInfo");
            controller.Data.ScriptNamespace = !info ? controller.Data.ScriptNamespace : info.nameSpace;
            
            if(controller.Data.IsPartialLoading)           
                EditorApplication.delayCall = () =>
                {
                    SerializedFieldBinderUtility.BindAllFields(controller, controller);
                    controller.Data.IsPartialLoading = false;
                };

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
                                if (baseInterface.ToString().Equals(typeof(IArchitecture).ToString()))
                                    list.Add(type.Name);
                }
            }
            catch { }

            controller.Data.Parent.Clear();
            controller.Data.Parent.Insert(0, typeof(ViewController).FullName);
            try
            {
                var types = AssemblyHelper.GetTypes(Assembly.Load(info.assembly));

                if (types != null)
                {
                    foreach (var type in types)
                    {
                        if (type.IsSubclassOf(typeof(ViewController)))
                        {
                            controller.Data.Parent.Add(type.FullName);
                        }
                    }
                }
            }
            catch { }

            if (controller.Data.SelectIndex >= controller.Data.Parent.Count)
                controller.Data.SelectIndex = 0;

            if (controller.Data.AutoArchitectureIndex >= controller.Data.AutoInfos.Count)
                controller.Data.AutoArchitectureIndex = 0;

            var currentType = controller.GetType();
            if (currentType.IsSubclassOf(typeof(ViewController)) && currentType.BaseType != typeof(ViewController))
            {
                for (int i = 0; i < controller.Data.Parent.Count; i++)
                {
                    if (controller.Data.Parent[i] == currentType.BaseType.FullName)
                    {
                        controller.Data.SelectIndex = i;                       
                        break;
                    }
                }

            }

        }

        protected override  void OnDisable()
        {
            base.OnDisable();
            FrameworkConfigInfo info = Resources.Load<FrameworkConfigInfo>("FrameworkConfigInfo");
            if (info == null) return;

            ViewController controller = target as ViewController;
            if (controller == null) return;

            info.nameSpace = controller.Data.ScriptNamespace;
        }

        private void OnSceneGUI()
        {
            ViewController controller = target as ViewController;
            if (!controller) return;
            controller.OnSceneGUI();
        }
        public override void OnInspectorGUI()
        {
            ViewController controller = target as ViewController;          
            if(PrefabUtility.IsPartOfAnyPrefab(controller))
                EditorGUILayout.HelpBox("特殊警示:在预制件下生成脚本并不会自动进行挂载跟替换的操作，请自行处理。", MessageType.Warning);
            if(!controller.OnInspectorGUI())
                base.OnInspectorGUI();

            try
            {
                CodeManager.BindInspector(controller, controller, GenericPartialScripts, controller.Data);
            }
            catch { }

            DrawScriptGenerationPanel(controller);
        }

        private void DrawScriptGenerationPanel(ViewController controller)
        {
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
            GUILayout.Label(FrameWorkConfigData.TitleTip, style);
            EditorGUILayout.BeginHorizontal(GUILayout.Width(100));
            GUILayout.Label("EN");
            FrameWorkConfigData.IsEN = EditorGUILayout.Toggle(FrameWorkConfigData.IsEN);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndHorizontal();

            EditorGUI.BeginDisabledGroup(CodeManager.IsPlaying);
            EditorGUILayout.Space();

            var Data = controller.Data;
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(FrameWorkConfigData.Email, GUILayout.Width(200));
            Data.CreateEmail = EditorGUILayout.TextField(Data.CreateEmail);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            Data.SystemNowTime = DateTime.Now.ToString();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(FrameWorkConfigData.NameSpace, GUILayout.Width(200));
            Data.ScriptNamespace = EditorGUILayout.TextField(Data.ScriptNamespace);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(FrameWorkConfigData.Name, GUILayout.Width(200));
            Data.ScriptName = EditorGUILayout.TextField(Data.ScriptName);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            var rect = EditorGUILayout.BeginHorizontal();           
            GUILayout.Label(FrameWorkConfigData.Path, GUILayout.Width(200));
            GUILayout.TextField(Data.ScriptPath);
            CodeManager.SelectFolder(Data);
            CodeManager.DragObject(rect, out string path);
            if (!string.IsNullOrEmpty(path))
                Data.ScriptPath = path;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(!target.GetType().Equals(typeof(ViewController)));          
            EditorGUILayout.EndHorizontal();            
            SelectParentClass(Data);
            EditorGUILayout.Space();
            SelectArchitecture(Data);
            EditorGUILayout.Space();
            SetFolderCreated(Data);
            EditorGUILayout.Space();
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space(20);
            CodeManager.GenericControllerScripts(Data,() => 
            {
                if (CodeManager.CheckViewBindder(controller, controller.GetComponentsInChildren<YukiBind>()))
                {
                    GenericPartialScripts();
                }
            });

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(target);
                AssetDatabase.SaveAssets();
            }
            EditorGUILayout.EndVertical();
        }

        private void SelectParentClass(CustomData data)
        {
            EditorGUILayout.BeginHorizontal(GUILayout.Width(400));
           
            EditorGUILayout.LabelField(FrameWorkConfigData.ViewControllerParent,GUILayout.Width(120));
            data.SelectIndex = EditorGUILayout.Popup(data.SelectIndex, data.Parent.ToArray());
            EditorGUILayout.EndHorizontal();
        }     
        private void SelectArchitecture(CustomData Data)
        {
            EditorGUILayout.BeginHorizontal(GUILayout.Width(400));
            
            EditorGUILayout.LabelField(FrameWorkConfigData.IsEN ? "Select Architecture:" : "架构选择:", GUILayout.Width(120));
            Data.AutoArchitectureIndex = EditorGUILayout.Popup(Data.AutoArchitectureIndex, list.ToArray());
            EditorGUILayout.EndHorizontal();
        }

        private string[] folderTip = new string[] { "开启", "关闭" };
        private void SetFolderCreated(CustomData Data)
        {
            EditorGUILayout.HelpBox(!FrameWorkConfigData.IsEN ? "开启后会在构建脚本时自动生成保存该脚本的文件夹,并同时同步路径" : "If this function is enabled, a folder to save the script is automatically generated and the path is synchronized when the script is built", MessageType.Info);
            EditorGUILayout.BeginHorizontal(GUILayout.Width(400));
            
            EditorGUILayout.LabelField(FrameWorkConfigData.IsEN ? "Folder Separation:" : "文件夹分离:", GUILayout.Width(120));
            Data.IsFolderCreateScripts = EditorGUILayout.Popup(Data.IsFolderCreateScripts ? 0 : 1, folderTip) == 0;
            EditorGUILayout.EndHorizontal();
        }

        private void GenericPartialScripts()
        {
            ViewController controller = target as ViewController;
            if (controller == null) return;

            ViewControllerPartialCodeGenerator.Generate(
                controller.Data,
                controller,
                controller.GetComponentsInChildren<YukiBind>(),
                () => controller.Data.IsPartialLoading = true,
                "System");
        }

        private bool Update_ScriptFrameWorkConfigData(string path,ViewController controller)
        {           
            MonoScript monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(path);  
            if(monoScript == null || PrefabUtility.IsPartOfAnyPrefab(controller)) return false;
            if (controller.IsDestroy()) return false;
            if (!monoScript.GetClass().IsSubclassOf(typeof(ViewController))) return false;
            var component = controller.gameObject.AddComponent(monoScript.GetClass());
            ViewController currentController = component as ViewController;            
            currentController.Data = controller.Data;
            foreach (var item in (controller as ISerializedFieldInfo).GetSerializeFields())
            {
                (currentController as ISerializedFieldInfo).AddFieldData(item);
            }                
            DestroyImmediate(controller);
            currentController.gameObject.name = currentController.Data.ScriptName;
            return true;
        }           
    }
}
#endif
