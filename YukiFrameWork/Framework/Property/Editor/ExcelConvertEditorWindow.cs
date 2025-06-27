///=====================================================
/// - FileName:      ExcelConvertEditorWindow.cs
/// - NameSpace:     YukiFrameWork.Machine
/// - Description:   高级定制脚本生成
/// - Creation Time: 2025/3/10 7:37:51
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using System.IO;
using YukiFrameWork.Extension;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
using Sirenix.OdinInspector.Editor;
namespace YukiFrameWork
{
    public class ExcelConvertEditorWindow : OdinMenuEditorWindow 
    {
        [MenuItem("YukiFrameWork/Excel-So转换工具",false,-4)]
        internal static void OpenWindow()
        {
            var window = GetWindow<ExcelConvertEditorWindow>();
            //window.Open();
            window.titleContent = new GUIContent("Excel转SO工具");
        }
        internal static void OpenWindow(IExcelSyncScriptableObject excelSyncScriptableObject)
        {
            var window = GetWindow<ExcelConvertEditorWindow>();
           // window.Open();
            window.titleContent = new GUIContent("Excel转SO工具");
            window.configInfo.excelConvertConfig = excelSyncScriptableObject as ScriptableObject;
        }
        FrameworkConfigInfo configInfo;       
       
        protected override void OnEnable()
        {
            base.OnEnable();
            configInfo = Resources.Load<FrameworkConfigInfo>(nameof(FrameworkConfigInfo));
        }       
        protected override void OnImGUI()
        {
            //EditorGUILayout.HelpBox("试验性功能", MessageType.Info);
            EditorGUILayout.Space();
#if UNITY_2021_1_OR_NEWER
            if (configInfo.excelConvertConfig && configInfo.excelConvertConfig is not IExcelSyncScriptableObject)
            {
                EditorGUILayout.HelpBox("传递的配置必须是继承自IExcelSyncScriptableObject的接口!", MessageType.Error);
            }          
#else
            if (configInfo.excelConvertConfig && !(configInfo.excelConvertConfig is IExcelSyncScriptableObject))
            {
                EditorGUILayout.HelpBox("传递的配置必须是继承自IExcelSyncScriptableObject的接口!", MessageType.Error);
            }
#endif
            EditorGUILayout.BeginHorizontal();
            configInfo.excelConvertConfig = (ScriptableObject)EditorGUILayout.ObjectField("配置", configInfo.excelConvertConfig, typeof(ScriptableObject), true);
            if (configInfo.excelConvertConfig is IExcelSyncScriptableObject && configInfo.excelConvertConfig && GUILayout.Button("刷新配置",GUILayout.Width(80)))
            {
                Init();
                this.ShowNotification(new GUIContent("刷新成功!"));
            }
            EditorGUILayout.EndHorizontal();
            if (configInfo.excelConvertConfig)
            {               
                if (configInfo.excelConvertConfig is IExcelSyncScriptableObject excel)
                {
                                     
                    EditorGUILayout.Space();
                    if (GUILayout.Button("选中配置查看"))
                    {
                        Selection.activeObject = configInfo.excelConvertConfig;
                    }
                    if(typeof(ScriptableObject).IsAssignableFrom(excel.ImportType) && excel.ScriptableObjectConfigImport)
                    {
                        EditorGUILayout.HelpBox("检测到类型依赖是ScriptableObject,需要指定配置依赖数据的生成路径!",MessageType.Info);
                        var rect = EditorGUILayout.BeginHorizontal();
                        configInfo.excelDataPath = EditorGUILayout.TextField("生成路径:", configInfo.excelDataPath);
                        CodeManager.DragObject(rect, out var dataPath);
                        if (!dataPath.IsNullOrEmpty()) configInfo.excelDataPath = dataPath;
                        if (GUILayout.Button("...", GUILayout.Width(40)))
                        {
                            configInfo.excelDataPath =  CodeManager.SelectFolder(configInfo.excelDataPath);                          
                        }
                        EditorGUILayout.EndHorizontal();
                    }                 
                    var tempRect = EditorGUILayout.BeginHorizontal();
                    configInfo.excelTempPath = EditorGUILayout.TextField("Excel路径:", configInfo.excelTempPath);
                    CodeManager.DragObject(tempRect,out var path);
                    if (!path.IsNullOrEmpty()) configInfo.excelTempPath = path;
                    if (GUILayout.Button("...", GUILayout.Width(40)))
                    {
                        configInfo.excelTempPath = EditorUtility.OpenFilePanel("",configInfo.excelTempPath.IsNullOrEmpty() ? Directory.GetCurrentDirectory() : configInfo.excelTempPath,"xlsx");
                    }
                    if (File.Exists(configInfo.excelTempPath) && GUILayout.Button("选择文件",GUILayout.Width(80)))
                    {
                        EditorApplication.delayCall += () =>
                        {
                            EditorUtility.RevealInFinder(configInfo.excelTempPath);
                        };
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space();
                    if (GUILayout.Button("导入Excel"))
                    {
                        if (SerializationTool.ExcelToScriptableObject(configInfo.excelTempPath, 3, excel,configInfo.excelDataPath))
                        {
                            Debug.Log("导入成功!");
                            this.ShowNotification(new GUIContent("导入成功!"));
                        }

                    }
                    EditorGUILayout.Space();
                    if (GUILayout.Button("导出Excel"))
                    {
                        if (SerializationTool.ScriptableObjectToExcel(excel,configInfo.excelTempPath, out _))
                        {
                            Debug.Log("导出成功!");
                            this.ShowNotification(new GUIContent("导出成功!"));
                        }

                    }
                    base.OnImGUI();
                    int temp = configInfo.excelConvertConfig.GetInstanceID();
                    if (temp != configInfo.excelInstanceId)
                    {
                        //更新实例Id， 初始化!
                        configInfo.excelInstanceId = temp;
                        Init();
                    }

                    
                }
                
            }
            else
            { }
                
        }

        private void Init()
        {
            if (MenuTree == null) return;
            ForceMenuTreeRebuild();
            string path = configInfo.excelConvertConfig.name + "_" + configInfo.excelInstanceId;
            MenuTree.Add(path, configInfo.excelConvertConfig,Sirenix.OdinInspector.SdfIconType.Instagram);          
            IExcelSyncScriptableObject excelSyncScriptableObject = configInfo.excelConvertConfig as IExcelSyncScriptableObject;
            IList list = excelSyncScriptableObject.Array;

            for (int i = 0; i < list.Count; i++)
            {
                int index = i;
                MenuTree.Add(path + "/" + index, list[i]);
            }
            Repaint();
        }

        protected override OdinMenuTree BuildMenuTree()
        {          
            return new OdinMenuTree();
        }
    }
}
#endif