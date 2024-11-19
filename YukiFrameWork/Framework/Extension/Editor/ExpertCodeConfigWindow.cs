///=====================================================
/// - FileName:      ExpertCodeConfigWindow.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/11/7 20:13:20
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using YukiFrameWork.Extension;

#if UNITY_EDITOR
using Sirenix.Utilities;
using UnityEditor;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
namespace YukiFrameWork
{
	public class ExpertCodeConfigWindow : EditorWindow
	{
        //AttributesExampleWindow
		[MenuItem("YukiFrameWork/" + nameof(ExpertCodeConfigWindow),false,-1000)]
		public static void OpenWindow()
		{
			GetWindow<ExpertCodeConfigWindow>().titleContent = new GUIContent("高级脚本生成设置");
		}
        internal ExpertCodeConfig config;
        private void OnEnable()
        {
            config = Resources.Load<ExpertCodeConfig>(nameof(ExpertCodeConfig));
            if (!config)
                throw new Exception("配置丢失");
            foreach (var window in Resources.FindObjectsOfTypeAll<ExpertCodeShowWindow>())
            {
                window.Close();
            }
            FrameworkConfigInfo info = Resources.Load<FrameworkConfigInfo>(nameof(FrameworkConfigInfo));
            if (!info) return;
            if (info.nameSpace != config.NameSpace)
                config.NameSpace = info.nameSpace;          
            childWindowRect = new Rect(position.x, position.y, 200, 100);
            architectures.Clear();
            architectures.Add("None");
            List<string> assemblies = config.configInfo.assemblies.ToList(); 
            assemblies.Add(config.configInfo.assembly);

            foreach (var item in assemblies)
            {
                try
                {
                    Assembly assembly = Assembly.Load(item);
                    foreach (var type in assembly.GetTypes())
                    {
                        if (typeof(IArchitecture).IsAssignableFrom(type))
                        {
                            architectures.Add(type.ToString());
                        }
                    }
                }
                catch
                { }
            }
            
            if (config.architecture.IsNullOrEmpty() || !architectures.Contains(config.architecture))
                selectIndex = 0;

            if (config.AssembliesContasts.Count == 0)
            {
                Assembly assembly1 = null;               
                Assembly assembly5 = null;
                try
                {
                    assembly1 = Assembly.Load("Assembly-CSharp");                   
                }
                catch { }                           
                
                try
                {
                    assembly5 = Assembly.Load("mscorlib");
                }
                catch { }

                if (assembly1 != null)
                    config.AssembliesContasts.Add(assembly1.GetName().Name);             
                if (assembly5 != null)
                    config.AssembliesContasts.Add(assembly5.GetName().Name);

                var allassemblies = AppDomain.CurrentDomain.GetAssemblies();

                foreach (var assembly in allassemblies)
                {
                    if (assembly.FullName.StartsWith("UnityEngine"))
                    {
                        config.AssembliesContasts.Add(assembly.GetName().Name);
                    }
                }
            }
        }

        private OdinEditor odinEditor;

        private GUIStyle titleStyle;
        private Rect childWindowRect;
        internal ExpertCodeShowWindow showWindow;
        private Vector2 scrollViewRect;
        private List<string> architectures = new List<string>();
        private int selectIndex = 0;       
        private void OnGUI()
        {         
           // Debug.Log(typeof(int).ToString());
            if (!config) return;
            if (selectIndex >= architectures.Count)
                selectIndex = 0;
            if (architectures.Contains(config.architecture))
                selectIndex = architectures.IndexOf(config.architecture);

            titleStyle ??= new GUIStyle()
            {
                fontSize = 20,
                alignment = TextAnchor.MiddleCenter,
            };
            titleStyle.normal.textColor = Color.white;
            scrollViewRect = EditorGUILayout.BeginScrollView(scrollViewRect);
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical("FrameBox");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("高级脚本设置", titleStyle);
            if (GUILayout.Button("刷新",GUILayout.Width(50)))
            {
                config.methodDatas.Clear();
                config.fieldDatas.Clear();
                config.Name = string.Empty;
                config.Save();
            }
            if (GUILayout.Button(showWindow ? "关闭预览窗口":"打开预览窗口",GUILayout.Width(100)))
            {
                if (!showWindow)
                    showWindow = ExpertCodeShowWindow.OpenWindow(this);
                else
                {
                    showWindow.Close();
                    showWindow = null;
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            EditorGUILayout.HelpBox(ExpertCodeConfig.tip,MessageType.Warning);
            EditorGUILayout.Space(20);
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginVertical("FrameBox");
            config.NameSpace = EditorGUILayout.TextField("命名空间:", config.NameSpace);
            var rect = EditorGUILayout.BeginHorizontal();
            config.FoldPath = EditorGUILayout.TextField("文件路径:", config.FoldPath);
            if (GUILayout.Button("...", GUILayout.Width(40)))
            {
                config.FoldPath = CodeManager.SelectFolder(config.FoldPath);
            }
            CodeManager.DragObject(rect, out string path);
            if (!path.IsNullOrEmpty())
                config.FoldPath = path;
            EditorGUILayout.EndHorizontal();
            config.Name = EditorGUILayout.TextField("脚本名称:" ,config.Name);
            config.ParentType = (ParentType)EditorGUILayout.EnumPopup("继承规则:", config.ParentType);
            if (config.ParentType != ParentType.Architecture)
            {
                if (config.ParentType == ParentType.Model || config.ParentType == ParentType.System || config.ParentType == ParentType.Utility || config.ParentType == ParentType.Controller)
                {
                    if (config.ParentType == ParentType.Controller)
                    {
                        EditorGUILayout.HelpBox("在高级设置中，Controller层级生成非ViewController版本，ViewController请使用组件方式添加", MessageType.Info);
                    }
                    config.levelInterface = EditorGUILayout.Toggle("是否以接口形式继承", config.levelInterface);
                    if (config.ParentType != ParentType.Controller)
                    {
                        config.customInterface = EditorGUILayout.Toggle("是否自定义接口派生:", config.customInterface);
                        if (config.levelInterface)
                        {
                            config.IsScruct = EditorGUILayout.Toggle("是否是结构体", config.IsScruct);
                        }
                    }
                }
                else config.levelInterface = false;
                if (config.ParentType == ParentType.Model || config.ParentType == ParentType.System || config.ParentType == ParentType.Utility || config.ParentType == ParentType.Controller)
                {
                    selectIndex = EditorGUILayout.Popup("注册架构选择", selectIndex, architectures.ToArray());
                    if (architectures.Count > 0)
                        config.architecture = architectures[selectIndex];
                }

                if (config.ParentType == ParentType.ScriptableObject)
                {
                    config.soFileName = EditorGUILayout.TextField("创建so文件名:", config.soFileName);
                    if (config.soFileName.IsNullOrEmpty())
                        config.soFileName = config.Name;
                    config.soMenuName = EditorGUILayout.TextField("创建so菜单名:", config.soMenuName);
                    if (config.soMenuName.IsNullOrEmpty())
                        config.soMenuName = "YukiFrameWork/" + config.soFileName;
                }
                else
                {
                    config.soFileName = string.Empty;
                    config.soMenuName = string.Empty; 
                }
            }
            else
            {
                
            }
            EditorGUILayout.HelpBox("开启后会在构建脚本时自动生成保存该脚本的文件夹,并同时同步路径", MessageType.Info);
            config.FoldExport = EditorGUILayout.ToggleLeft("文件夹分离", config.FoldExport);
            config.IsOpenExpertCode = EditorGUILayout.ToggleLeft("开启字段高级注入模式:",config.IsOpenExpertCode);
            EditorGUILayout.Space(15);
            if (config.IsOpenExpertCode)
            {
                odinEditor ??= OdinEditor.CreateEditor(config, typeof(OdinEditor)) as OdinEditor;
                odinEditor.OnInspectorGUI();
            }          
            string targetPath = config.FoldPath + "/" + config.Name + ".cs";
            if (System.IO.File.Exists(targetPath))
            {
                EditorGUILayout.BeginVertical();
                if (GUILayout.Button("选择脚本", GUILayout.Height(30)))
                {
                    MonoScript monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(targetPath);
                    Selection.activeObject = monoScript;
                }
                if (GUILayout.Button("打开脚本", GUILayout.Height(30)))
                {
                    MonoScript monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(targetPath);
                    AssetDatabase.OpenAsset(monoScript);
                }
                if (GUILayout.Button("更新脚本", GUILayout.Height(30)))
                {
                    CreateOrUpdateCode(false);
                }
                EditorGUILayout.EndVertical();
            }
            else
            {
                if (GUILayout.Button("生成脚本",GUILayout.Height(30)))
                {
                    CreateOrUpdateCode(true);
                }               
            }           

            
            EditorGUILayout.EndVertical();
           
            if(EditorGUI.EndChangeCheck())
                config.Save();
            EditorGUILayout.EndScrollView();
        }
        private void CreateOrUpdateCode(bool tickPath = false)
        {
            if (config.FoldExport && tickPath)
            {
                if (!config.FoldPath.EndsWith("/") || !config.FoldPath.EndsWith(@"\"))
                {
                    config.FoldPath += "/";
                }
                config.FoldPath += config.Name;
            }
            string text = drawCode.BuildFile(config).ToString();
            text = text.Replace($"<color=#{ColorUtility.ToHtmlStringRGBA(ExpertCodeConfig.BackgroundColor)}>", string.Empty);
            text = text.Replace($"<color=#{ColorUtility.ToHtmlStringRGBA(ExpertCodeConfig.IdentifierColor)}>", string.Empty);
            text = text.Replace($"<color=#{ColorUtility.ToHtmlStringRGBA(ExpertCodeConfig.KeywordColor)}>", string.Empty);
            text = text.Replace($"<color=#{ColorUtility.ToHtmlStringRGBA(ExpertCodeConfig.LiteralColor)}>", string.Empty);
            text = text.Replace($"<color=#{ColorUtility.ToHtmlStringRGBA(ExpertCodeConfig.StringLiteralColor)}>", string.Empty);
            text = text.Replace($"<color=#{ColorUtility.ToHtmlStringRGBA(ExpertCodeConfig.CommentColor)}>", string.Empty);
            text = text.Replace($"<color=#{ColorUtility.ToHtmlStringRGBA(ExpertCodeConfig.StringLiteralColor)}>", string.Empty);
            text = text.Replace("</color>", string.Empty);
            text.CreateFileStream(config.FoldPath, config.Name, ".cs");
        }
        private void OnDisable()
        {           
            if (showWindow)
                showWindow.Close();
        }
        public DrawCode drawCode = new DrawCode();
    }

    public class ExpertCodeShowWindow : EditorWindow
    {
        private ExpertCodeConfigWindow Instance;
       
        internal static ExpertCodeShowWindow OpenWindow(ExpertCodeConfigWindow Instance)
        {
            ExpertCodeShowWindow showWindow = GetWindow<ExpertCodeShowWindow>();
            showWindow.titleContent = new GUIContent("脚本预览窗口");
            showWindow.Show();
            showWindow.Instance = Instance;
            showWindow.position = new Rect(showWindow.targetX, showWindow.targetY, 600, Instance.position.height);
            showWindow.text = showWindow.drawCode.BuildFile(Instance.config).ToString();
            return showWindow;
        }
        private DrawCode drawCode => Instance.drawCode;
        private float targetX => Instance.position.x + Instance.position.width + 10;
        private float targetY => Instance.position.y + 10;
        private void OnEnable()
        {
           
        }
        Vector2 scrollRect;
        public string text;
        private void OnGUI()
        {
            if (!Instance.config) return;
            var config = Instance.config;            
          
            //MonoScript monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(targetPath);
            GUILayout.Space(15);
            var rect = EditorGUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("Assembly Information");
            GUI.color = Color.yellow;
            GUILayout.Label("Prefabricated scripts have no assembly");
            GUI.color = Color.white;
            if (GUILayout.Button("刷新", GUILayout.Width(200)))
            {
                text = drawCode.BuildFile(Instance.config).ToString();
                Repaint();
            }
            EditorGUILayout.EndVertical();
            if (codeTextStyle == null)
            {
                codeTextStyle = new GUIStyle(SirenixGUIStyles.MultiLineLabel);
                codeTextStyle.normal.textColor = ExpertCodeConfig.TextColor;
                codeTextStyle.active.textColor = ExpertCodeConfig.TextColor;
                codeTextStyle.focused.textColor = ExpertCodeConfig.TextColor;
                codeTextStyle.wordWrap = false;
            }
            scrollRect = EditorGUILayout.BeginScrollView(scrollRect);
            var textRect = EditorGUILayout.BeginVertical();
            EditorGUI.DrawRect(textRect, SirenixGUIStyles.BorderColor);
            Vector2 vector = codeTextStyle.CalcSize(text);
            Rect position = GUILayoutUtility.GetRect(vector.x + 50f, vector.y).AddXMin(4f).AddY(2f);
            EditorGUI.SelectableLabel(position,config.Name.IsNullOrEmpty()  ? "请输入文件名称以预览脚本" : text,codeTextStyle);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }
        private GUIStyle codeTextStyle;
  
        private void OnInspectorUpdate()
        {
            if (!Instance) return;

            position = new Rect(targetX, targetY, position.width, position.height);
            Repaint();
        }       
    }
}
#endif