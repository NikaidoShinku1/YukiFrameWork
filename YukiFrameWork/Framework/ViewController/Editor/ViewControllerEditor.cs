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
using System.Linq;
using System;
using System.IO;
using YukiFrameWork.Events;
using System.Reflection;
namespace YukiFrameWork.Extension
{
    [CustomEditor(typeof(ViewController), true)]
    [CanEditMultipleObjects]
    public class ViewControllerEditor : Editor
    {       
        [MenuItem("GameObject/YukiFrameWork/Create ViewController",false,-1)]
        private static void CreateViewController()
        {           
            GameObject gameObject = Selection.activeGameObject;

            if (gameObject == null)
                gameObject = new GameObject("ViewController");

            if (gameObject.GetComponent<ViewController>() != null)
            {
                $"当前不需要为这个对象添加ViewController,脚本已存在。GameObject Name: {gameObject.name}".LogInfo(Log.W);
                return;
            }

            gameObject.AddComponent<ViewController>(); 
        }

        private void Awake()
        {         
            ViewController controller = target as ViewController;
            controller.Data ??= new ViewController.CustomData();

            if (string.IsNullOrEmpty(controller.Data.ScriptName))
                controller.Data.ScriptName = (target.name == "ViewController" ? (target.name + "Example") : target.name);

            string scriptFilePath = controller.Data.ScriptPath + @"/" + controller.Data.ScriptName + ".cs";
           
            if (controller.GetType().ToString().Equals(typeof(ViewController).ToString()))
            {             
                if(!Update_ScriptViewControllerDataInfo(scriptFilePath, controller))
                    controller.Data.ScriptName = (target.name == "ViewController" ? (target.name + "Example") : target.name);
            }
        }

        private void OnEnable()
        {            
            ViewController controller = target as ViewController;
            string scriptFilePath = controller.Data.ScriptPath + @"/" + controller.Data.ScriptName + ".cs";
          
            if (controller.Data.OnLoading)
            {
                controller.Data.OnLoading = false;
                Update_ScriptViewControllerDataInfo(scriptFilePath, controller);            
            }                   
        }      
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

#if UNITY_2021_1_OR_NEWER
            Texture2D icon = EditorGUIUtility.IconContent("d_Favorite").image as Texture2D;
            EditorGUIUtility.SetIconForObject(target, icon);
#endif
            EditorGUILayout.Space(20);
            DrawControllerGUI();
        }

        private void DrawControllerGUI()
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
            GUILayout.Label(ViewControllerDataInfo.TitleTip,style);
            EditorGUILayout.BeginHorizontal(GUILayout.Width(100));
            GUILayout.Label("EN");
            ViewControllerDataInfo.IsEN = EditorGUILayout.Toggle(ViewControllerDataInfo.IsEN);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndHorizontal();
            ViewController controller = target as ViewController;

            if (controller == null)
                return;           
            EditorGUI.BeginDisabledGroup(IsPlaying || !target.GetType().Equals(typeof(ViewController)));
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();

            GUILayout.Label(ViewControllerDataInfo.Email, GUILayout.Width(200));
            controller.Data.CreateEmail = EditorGUILayout.TextField(controller.Data.CreateEmail);          
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();          
            
            controller.Data.SystemNowTime = DateTime.Now.ToString();     

            EditorGUILayout.BeginHorizontal();

            GUILayout.Label(ViewControllerDataInfo.NameSpace, GUILayout.Width(200));
            controller.Data.ScriptNamespace = EditorGUILayout.TextField(controller.Data.ScriptNamespace);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();

            GUILayout.Label(ViewControllerDataInfo.Name, GUILayout.Width(200));
            controller.Data.ScriptName = EditorGUILayout.TextField(controller.Data.ScriptName);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();           
            
            GUILayout.Label(ViewControllerDataInfo.Path,GUILayout.Width(200));
            GUILayout.TextField(controller.Data.ScriptPath);
            SelectFolder(controller);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();

            GUILayout.Label(ViewControllerDataInfo.AutoInfo, GUILayout.Width(ViewControllerDataInfo.IsEN ? 300 : 200));         
            controller.Data.IsAutoMation = EditorGUILayout.Toggle(controller.Data.IsAutoMation, GUILayout.Width(50));          
            EditorGUILayout.EndHorizontal();
            if (controller.Data.IsAutoMation)
            {              
                EditorGUILayout.Space(10);
                SelectArchitecture(controller);
            }
            EditorGUILayout.Space(10);

            EditorGUI.EndDisabledGroup();
           
            controller.Data.IsCustomAssembly = EditorGUILayout.ToggleLeft(ViewControllerDataInfo.AssemblyInfo, controller.Data.IsCustomAssembly);
            if(controller.Data.IsCustomAssembly)
            {       
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(ViewControllerDataInfo.IsEN ? "Input Assembly Name:" : "输入程序集名称:");
                controller.Data.CustomAssemblyName = EditorGUILayout.TextField(controller.Data.CustomAssemblyName);
                EditorGUILayout.EndHorizontal();
            }           
            else controller.Data.CustomAssemblyName = "Assembly-CSharp";
            EditorGUILayout.Space(20);

            EditorGUILayout.BeginHorizontal();
            CreateScripts(controller);
            EditorGUILayout.EndHorizontal();


            EditorGUILayout.Space(15);          
            if (!target.GetType().Equals(typeof(ViewController)))
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(ViewControllerDataInfo.PrefabGenerationPath, GUILayout.Width(200));
                controller.Data.AssetPath = EditorGUILayout.TextField(controller.Data.AssetPath);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(15);
                GUILayout.Label(ViewControllerDataInfo.DescriptionInfo, "PreviewPackageInUse");
                var rect = EditorGUILayout.BeginHorizontal("LODBlackBox");
                EditorGUILayout.Space(40);
                DragFolderObject(rect, controller);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space();
                CreatePrefab(controller);
                EditorGUILayout.Space(15);

                AddEventCenter(controller);
            }
            GUILayout.EndVertical();           
        }

        private void SelectArchitecture(ViewController controller)
        {
            EditorGUILayout.BeginHorizontal(GUILayout.Width(300));
            EditorGUILayout.LabelField(ViewControllerDataInfo.IsEN ? "Select Architecture:" : "架构选择:",GUILayout.Width(120));

            var list = controller.Data.AutoInfos;
            list.Clear();
            list.Add("None");
            try
            {
                var types = AssemblyHelper.GetTypes( Assembly.Load(controller.Data.CustomAssemblyName));
                if(types != null)
                {
                    foreach (var type in types)            
                        if (type.BaseType != null)              
                            foreach (var baseInterface in type.BaseType.GetInterfaces())                                          
                                if (baseInterface.ToString().Equals(typeof(IArchitecture).ToString()))                                                   
                                    list.Add(type.Name);                                                                                     
                }
            }
            catch{ }
            controller.Data.AutoArchitectureIndex = EditorGUILayout.Popup(controller.Data.AutoArchitectureIndex, list.ToArray());          
            EditorGUILayout.EndHorizontal();          
        }

        private void AddEventCenter(ViewController controller)
        {
            RuntimeEventCenter center = controller.GetComponent<RuntimeEventCenter>();
            EditorGUI.BeginDisabledGroup(center);           
            if (GUILayout.Button(ViewControllerDataInfo.AddEventInfo, GUILayout.Height(30)))
            {
                controller.gameObject.AddComponent<RuntimeEventCenter>();
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
                        GUILayout.Label(ViewControllerDataInfo.EventAudioMationInfo);
                        break;
                    case RuntimeInitialized.Awake:
                        GUILayout.Label(ViewControllerDataInfo.EventAwakeInfo);
                        break;                 
                }
            }
        }

        private bool IsPlaying => Application.isPlaying;

        private void CreatePrefab(ViewController controller)
        {
            GameObject prefab = controller.gameObject;
            string targetPath = controller.Data.AssetPath + @"/" + prefab.name + ".prefab";
            if (GUILayout.Button((File.Exists(targetPath) && PrefabUtility.IsPartOfAnyPrefab(prefab)) ? ViewControllerDataInfo.Update_PrefabInfo : ViewControllerDataInfo.CreatePrefabInfo, GUILayout.Height(30)))
            {
                if (!File.Exists(targetPath) || !PrefabUtility.IsPartOfAnyPrefab(prefab))
                {
                    PrefabUtility.SaveAsPrefabAssetAndConnect(prefab, targetPath, InteractionMode.UserAction);
                }
                else
                {
                    PrefabUtility.ApplyPrefabInstance(prefab, InteractionMode.UserAction);
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        private void CreateScripts(ViewController controller)
        {
            string scriptFilePath = controller.Data.ScriptPath + @"/" + controller.Data.ScriptName + ".cs";
            if (!File.Exists(scriptFilePath) || string.IsNullOrEmpty(controller.Data.ScriptName))
            {
                if (GUILayout.Button(ViewControllerDataInfo.GenerateScriptBtn, GUILayout.Height(30)))
                {
                    if (string.IsNullOrEmpty(controller.Data.ScriptPath))
                    {
                        (ViewControllerDataInfo.IsEN ? "Cannot create script because path is empty!" : "路径为空无法创建脚本!").LogInfo(Log.E);
                        return;
                    }

                    if (controller.Data.IsAutoMation && controller.Data.AutoArchitectureIndex == 0)
                    {
                        (ViewControllerDataInfo.IsEN ? "After automation is turned on, you must create an Architecture that inherits from Architecture<> and select it. Current schema selection is None, cannot create script!" : "开启自动化后必须要创建一个架构继承自Architecture<>并进行选择。当前架构选择为None,无法创建脚本!").LogInfo(Log.E);
                        return;
                    }

                    if (!Directory.Exists(controller.Data.ScriptPath))
                    {
                        Directory.CreateDirectory(controller.Data.ScriptPath);
                        AssetDatabase.Refresh();
                    }
                   
                    using (FileStream fileStream = new FileStream(scriptFilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
                    {
                        StreamWriter streamWriter = new StreamWriter(fileStream);

                        TextAsset textAsset = Resources.Load<TextAsset>(controller.Data.IsAutoMation ? "ViewController(Auto)" : "ViewController");

                        string ViewControllerDataInfo = textAsset.text;
                        ViewControllerDataInfo = ViewControllerDataInfo.Replace("YukiFrameWork.Project", controller.Data.ScriptNamespace);
                        ViewControllerDataInfo = ViewControllerDataInfo.Replace("Yuki@qq.com", controller.Data.CreateEmail);
                        ViewControllerDataInfo = ViewControllerDataInfo.Replace("xxxx年x月xx日 xx:xx:xx", controller.Data.SystemNowTime);
                        ViewControllerDataInfo = ViewControllerDataInfo.Replace("#SCRIPTNAME#", controller.Data.ScriptName);
                        if(controller.Data.IsAutoMation)
                            ViewControllerDataInfo = ViewControllerDataInfo.Replace("IArchitecture", controller.Data.AutoInfos[controller.Data.AutoArchitectureIndex]);
                        streamWriter.Write(ViewControllerDataInfo);

                        streamWriter.Close();

                        fileStream.Close();
                        //正在改变脚本
                        controller.Data.OnLoading = true;
                      
                    }
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                   
                }
            }
            else
            {
                GUILayout.BeginVertical();
                MonoScript monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(scriptFilePath);
                if (GUILayout.Button(ViewControllerDataInfo.SelectScriptBtn, GUILayout.Height(30)))
                {
                    Selection.activeObject = monoScript;
                }

                if (GUILayout.Button(ViewControllerDataInfo.OpenScriptBtn, GUILayout.Height(30)))
                {                 
                    AssetDatabase.OpenAsset(monoScript);
                }
                GUILayout.EndVertical();
            }
        }

        private bool Update_ScriptViewControllerDataInfo(string path,ViewController controller)
        {           
            MonoScript monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(path);  
            if(monoScript == null) return false;
            var component = controller.gameObject.AddComponent(monoScript.GetClass());
            ViewController currentController = component as ViewController;

            currentController.Data = controller.Data;                      

            DestroyImmediate(controller);
            return true;
        }

        private void SelectFolder(ViewController controller)
        {
            if (GUILayout.Button("...", GUILayout.Width(40)))
            {
                controller.Data.ScriptPath = string.Empty;
                string path = EditorUtility.OpenFolderPanel("", controller.Data.ScriptPath, "");

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
                            controller.Data.ScriptPath += values[i] + "/";
                        else
                            controller.Data.ScriptPath += values[i];
                    }

                }

                GUIUtility.ExitGUI();
            }
        }

        private void DragFolderObject(Rect rect,ViewController controller)
        {
            Event e = Event.current;
            EditorGUILayout.Space(40);
            if (rect.Contains(e.mousePosition))
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
                if (e.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    DefaultAsset asset = DragAndDrop.objectReferences.FirstOrDefault(x => x.GetType().Equals(typeof(DefaultAsset))) as DefaultAsset;

                    if (asset != null)
                    {
                        controller.Data.AssetPath = AssetDatabase.GetAssetPath(asset);
                        e.Use();
                        AssetDatabase.Refresh();
                    }

                }
            }
        }
    }
}
#endif