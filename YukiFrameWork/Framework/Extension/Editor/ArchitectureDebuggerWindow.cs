///=====================================================
/// - FileName:      ArchitectureDebuggerWindow.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/6/7 1:13:48
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using UnityEngine;
using System;
using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using YukiFrameWork.Extension;
using YukiFrameWork.ExampleRule;
using System.Collections;

#if UNITY_EDITOR
using static YukiFrameWork.ArchitectureDebuggerWindow;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
namespace YukiFrameWork
{
    internal class ArchitectureDebuggerWindow : OdinMenuEditorWindow
    {
            
        internal enum Title
        {
            [LabelText("Model注册收集")]
            Model,
            [LabelText("System注册收集")]
            System,
            [LabelText("Utility注册收集")]
            Utility,
            [LabelText("Controller标记收集")]
            Controller,
            [LabelText("运行时事件收集")]
            Event
        }

        internal enum EventType
        {
            Type,
            String,
            Enum
        }

        internal enum SelectType
        {
            [LabelText("展开全部收集")]
            All,
            [LabelText("显示标记了该架构的模块")]
            Architecture,
            [LabelText("显示没有任何标记的模块")]
            NoAttribute
        }
        [MenuItem("YukiFrameWork/Architecture Debugger",false,-1000)]
        static void OpenWindow()
        {
            var window = GetWindow<ArchitectureDebuggerWindow>();
            window.titleContent = new GUIContent("架构调试器");
            window.position = new Rect(window.position.x, window.position.y, 1550, window.position.height);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            DrawUnityEditorPreview = true;
            autoRepaintOnSceneChange = true;
            //position = new Rect(position.x, position.y, 1550, position.height);

            ArchitectureDebugger.mtitle = (Title)PlayerPrefs.GetInt("Architecture_Key_Title", 0);
            ArchitectureDebugger.selectType = (SelectType)PlayerPrefs.GetInt("Architecture_Key_SelectType", 0);
        }   

        protected override void OnDisable()
        {
            base.OnDisable();
            PlayerPrefs.SetInt("Architecture_Key_Title", (int)ArchitectureDebugger.mtitle);
            PlayerPrefs.SetInt("Architecture_Key_SelectType", (int)ArchitectureDebugger.selectType);
        }

        private void Update()
        {
            Repaint();
        }    

        //private List<ArchitectureDebugger> debuggerWindows = new List<ArchitectureDebugger>();

        
        protected override OdinMenuTree BuildMenuTree()
        {
            OdinMenuTree tree = new OdinMenuTree();
            FrameworkConfigInfo info = Resources.Load<FrameworkConfigInfo>(nameof(FrameworkConfigInfo));
                       
            try
            {               
                Assembly assembly = Assembly.Load(info.assembly);
                Loading(assembly, tree);
                if (info.assemblies != null && info.assemblies.Length > 0)
                {
                    var names = info.assemblies;
                    Assembly[] assemblies = new Assembly[names.Length];
                    for (int i = 0; i < names.Length; i++)
                    {
                        assemblies[i] = Assembly.Load(names[i]);
                        Loading(assemblies[i], tree);
                    }
                }
            }
            catch(Exception ex) { LogKit.I("失败" + ex.ToString()); }
            return tree;
        }

        private void Loading(Assembly assembly,OdinMenuTree tree)
        {
            foreach (var type in assembly.GetTypes())
            {              
                if (typeof(IArchitecture).IsAssignableFrom(type))
                {
                    var window = ArchitectureDebugger.Create(type, this);
                    tree.Add("YukiFrameWork/" + "架构:" + type.Name,window,SdfIconType.ArchiveFill);                  
                }
            }
        }
    }

    internal class ArchitectureDebugger
    {
        [HideLabel, ShowInInspector, EnumToggleButtons, BoxGroup]
        internal static Title mtitle;


        [LabelText("检索类型"), PropertySpace(10), HideIf(nameof(mtitle), Title.Event), ShowInInspector, BoxGroup]
        internal static SelectType selectType;

        [EnumToggleButtons, ShowIf(nameof(mTitle), ArchitectureDebuggerWindow.Title.Event), PropertySpace(10),ShowInInspector]
        private ArchitectureDebuggerWindow.EventType eventType { get; set; }
       
        private string eventSearch;
        private IArchitecture architecture;       
        private ArchitectureDebuggerWindow editorWindow;
        private Type architectureType;
        void Init(Type type,ArchitectureDebuggerWindow editorWindow) 
        {
            this.architectureType = type;
            notRunStyle = new GUIStyle();
            notRunStyle.fontSize = 30;
            notRunStyle.normal.textColor = Color.blue;
            notRunStyle.alignment = TextAnchor.MiddleCenter;
            roldStyle = new GUIStyle("OL Box");
            roldStyle.alignment = TextAnchor.MiddleRight;

            fontStyle = new GUIStyle();
            fontStyle.fontSize = 15;
            fontStyle.fontStyle = FontStyle.Bold;
            fontStyle.alignment = TextAnchor.UpperCenter;
            fontStyle.normal.textColor = Color.cyan;
            this.editorWindow = editorWindow;
            middleStyle = new GUIStyle()
            {
                alignment = TextAnchor.MiddleCenter
                
            };

            middleStyle.normal.textColor = Color.white;

            runningStyle = new GUIStyle("MeLivePlayBar")
            {
                alignment = TextAnchor.UpperCenter
            };
            rolds.Clear();         
            rolds[Rule.Model] = GetRoldTypes(type => typeof(IModel).IsAssignableFrom(type) && !type.IsInterface);          
            rolds[Rule.System] = GetRoldTypes(type => typeof(ISystem).IsAssignableFrom(type) && !type.IsInterface);

            rolds[Rule.Utility] = GetRoldTypes(type => typeof(IUtility).IsAssignableFrom(type) && !type.IsInterface);

            rolds[Rule.Controller] = GetRoldTypesByController(type => typeof(IController).IsAssignableFrom(type) && type != typeof(ViewController) && !type.IsAbstract);          

        }

        [Button("架构信息收集",ButtonHeight = 30),PropertySpace(10)]
        [GUIColor("@Color.Lerp(Color.red, Color.green, Mathf.Abs(Mathf.Sin((float)EditorApplication.timeSinceStartup)))")]
        void Light()
        { }

        public static ArchitectureDebugger Create(Type type, ArchitectureDebuggerWindow editorWindow)
        {
            ArchitectureDebugger debugger = new ArchitectureDebugger();
            debugger.Init(type,editorWindow);
            return debugger;
        }
        class GenericInfo
        {
            [HideInInspector]
            public Type type;
            [HideInInspector]
            public Attribute registration;        

            public GenericInfo(Type type, Attribute registration)
            {
                this.type = type;
                this.registration = registration;
            }      
        }

        private bool IsRunning => Application.isPlaying;
      
        private Dictionary<Rule, GenericInfo[]> rolds = new Dictionary<Rule, GenericInfo[]>();
           
        private GenericInfo[] GetRoldTypes(Func<Type,bool> condition)
        {
            return AssemblyHelper
                .GetTypes(architectureType)
                .Select(type =>
            {
                RegistrationAttribute registration = type.GetCustomAttribute<RegistrationAttribute>(true);
                return new GenericInfo(type, registration);
            })
                .Where(type => condition?.Invoke(type.type) == true).ToArray();
        }

        private GenericInfo[] GetRoldTypesByController(Func<Type, bool> condition)
        {
            return AssemblyHelper
                .GetTypes(architectureType)
                .Select(type =>
                {
                    RuntimeInitializeOnArchitecture registration = type.GetCustomAttribute<RuntimeInitializeOnArchitecture>(true);
                    return new GenericInfo(type, registration);
                })
                .Where(type => condition?.Invoke(type.type) == true).ToArray();
        }
        GUIStyle notRunStyle;
        GUIStyle roldStyle;
        GUIStyle fontStyle;
        GUIStyle middleStyle;
        private const string Yuki_ModelFoldOut = nameof(Yuki_ModelFoldOut);
        private const string Yuki_SystemFoldOut = nameof(Yuki_SystemFoldOut);
        private const string Yuki_UtilityFoldOut = nameof(Yuki_UtilityFoldOut);
                                

        Vector2 modelPosition;
        Vector2 systemPosition;
        Vector2 utilityPosition;
        Vector2 controllerPosition;
        Vector2 eventPosition;

        ArchitectureStartUpRequest request = null;
        [OnInspectorGUI]
        public void OnImGUI()
        {           
            if (IsRunning)
            {
                if (ArchitectureConstructor.Instance.runtimeRequests.TryGetValue(architectureType.FullName, out request))
                {
                    if(architecture == null)
                        architecture = request.architecture;
                }
            }
        
            float width = editorWindow.position.width - editorWindow.MenuWidth;

            var rect = EditorGUILayout.BeginVertical();
            switch (mtitle)
            {
                case ArchitectureDebuggerWindow.Title.Model:                   
                    modelPosition = EditorGUILayout.BeginScrollView(modelPosition, GUILayout.Height(editorWindow.position.height - 300));
                    EditorGUILayout.BeginVertical(roldStyle,GUILayout.Height(editorWindow.position.height - 300));
                    EditorGUILayout.LabelField("Model注册收集", fontStyle);
                    DrawInfos(Rule.Model);
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndScrollView();
                    break;
                case ArchitectureDebuggerWindow.Title.System:
                   
                    systemPosition = EditorGUILayout.BeginScrollView(systemPosition,GUILayout.Height(editorWindow.position.height - 300));
                    EditorGUILayout.BeginVertical(roldStyle,GUILayout.Height(editorWindow.position.height - 300));
                    EditorGUILayout.LabelField("System注册收集", fontStyle);
                    DrawInfos(Rule.System);
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndScrollView();
                    break;
                case ArchitectureDebuggerWindow.Title.Utility:
                   
                    utilityPosition = EditorGUILayout.BeginScrollView(utilityPosition,GUILayout.Height(editorWindow.position.height - 300));
                    EditorGUILayout.BeginVertical(roldStyle,GUILayout.Height(editorWindow.position.height - 300));
                    EditorGUILayout.LabelField("Utility注册收集", fontStyle);
                    DrawInfos(Rule.Utility);
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndScrollView();
                    break;
                case ArchitectureDebuggerWindow.Title.Controller:                  
                    controllerPosition = EditorGUILayout.BeginScrollView(controllerPosition,GUILayout.Height(editorWindow.position.height - 300));
                    //EditorGUILayout.HelpBox("Controller标记收集仅收集使用框架提供的控制器基类ViewController才拥有自动注入的功能(状态机模块的状态类也支持但不划分为该标记)\n手动继承IController的控制器类应自己实现GetArchitecture方法", MessageType.Info);
                    EditorGUILayout.BeginVertical(roldStyle,GUILayout.Height(editorWindow.position.height - 300));
                    EditorGUILayout.LabelField("Controller标记收集", fontStyle);
                    {
                        DrawController();
                        //ViewController信息收集ToDo
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndScrollView();
                    break;
                case ArchitectureDebuggerWindow.Title.Event:
                    eventPosition = EditorGUILayout.BeginScrollView(eventPosition,GUILayout.Height(editorWindow.position.height - 300));
                    EditorGUILayout.BeginVertical(roldStyle,GUILayout.Height(editorWindow.position.height - 300));
                    EditorGUILayout.LabelField("运行时架构事件标记EventSystem", fontStyle);
                    {
                        DrawEventSystem();
                        //EventSystem信息收集ToDo
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndScrollView();
                    break;              
            }
            EditorGUILayout.Space(50);
            if (IsRunning)
            {
                if (request == null)
                {
                    EditorGUILayout.HelpBox("架构还没有准备! Architecture Type:" + architectureType, MessageType.Warning);
                }
                else if (request.isDone)
                {
                    if (request.error.IsNullOrEmpty())
                    {
                        EditorGUILayout.HelpBox("现在可以正常访问注册进架构的各个模块!", MessageType.Info);
                        GUI.color = Color.green;
                        GUILayout.Label("架构准备完成!", middleStyle);

                    }
                    else
                    {
                        GUILayout.Label("架构准备异常！", middleStyle);
                        EditorGUILayout.HelpBox(request.error, MessageType.Error);
                    }
                }
                else
                {
                    GUILayout.Label("架构正在准备中:" + $"{(request.progress * 100):F2}%", middleStyle);
                }
                if (request != null)
                {
                    GUILayout.Box(string.Empty, runningStyle, GUILayout.Width(request.progress * width));
                    GUI.color = Color.white;
                }
            }
            else
            {
                EditorGUILayout.HelpBox("项目还未运行,不会进行架构的准备进度显示,请运行后查看", MessageType.Warning);
            }
            EditorGUILayout.EndVertical();    

        }
        //private GUIStyle runningStyleBG = new GUIStyle("MeLivePlayBackground");
        private GUIStyle runningStyle;
     
        private static Color GetButtonColor()
        {
            Sirenix.Utilities.Editor.GUIHelper.RequestRepaint();
            return Color.HSVToRGB(Mathf.Cos((float)UnityEditor.EditorApplication.timeSinceStartup + 1f) * 0.225f + 0.325f, 1, 1);
        }
        string box;
        private ArchitectureDebuggerWindow.Title mTitle => mtitle;
       
        void DrawController()
        {
            foreach (var info in rolds[Rule.Controller])
            {
                box = box.IsNullOrEmpty() ? "Wizard Box" : box;
                RuntimeInitializeOnArchitecture runtimeInitialize = null;

                if (info.registration != null)
                {
                    runtimeInitialize = info.registration as RuntimeInitializeOnArchitecture;
                }

                if (selectType == ArchitectureDebuggerWindow.SelectType.Architecture && runtimeInitialize?.ArchitectureType != architectureType)                
                    continue;

                if (selectType == ArchitectureDebuggerWindow.SelectType.NoAttribute && runtimeInitialize != null)
                    continue;
                var tRect = EditorGUILayout.BeginHorizontal();
                var rect = EditorGUILayout.BeginHorizontal(tRect.Contains(Event.current.mousePosition) ? "SelectionRect" : "Wizard Box", GUILayout.Height(25));

                EditorGUILayout.LabelField(EditorGUIUtility.IconContent("cs Script Icon"), GUILayout.Width(20));
                GUILayout.Label(info.type.FullName, GUILayout.Width(400));
                EditorGUILayout.Space(20);

                if (info.type.IsSubclassOf(typeof(ViewController)) || info.type.IsSubclassOf(typeof(AbstractController)))
                {
                    if (runtimeInitialize == null)
                    {
                        GUI.color = Color.yellow;
                        GUILayout.Label("可标记RuntimeInitializeOnArchitecture特性进行架构的自动注入!", GUILayout.Width(550));
                    }
                    else
                    {
                        GUI.color = GetButtonColor();
                        GUILayout.Label("已标记RuntimeInitializeOnArchitecture特性! 标记架构:" + runtimeInitialize.ArchitectureType, GUILayout.Width(550));
                    }
                }
                else
                {
                    GUILayout.Label("手动实现IController接口控制器,没有自动操作", GUILayout.Width(550));
                }
    
                GUI.color = Color.white;
                OpenScript(info.type);
                string baseTitle = string.Empty;
                if (info.type.IsSubclassOf(typeof(ViewController)))
                {
                    GUI.color = Color.cyan;
                    baseTitle = "Base:ViewController";
                }
                else if (info.type.IsSubclassOf(typeof(AbstractController)))
                {
                    GUI.color = Color.green;
                    baseTitle = "Base:AbstractController";
                }
                else
                {
                    GUI.color = Color.yellow;
                    baseTitle = "Not Base:IController";
                }

                GUILayout.Label(baseTitle,GUILayout.Width(150));
                GUI.color = Color.white;
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space();
            }
        }

        void DrawEventSystem()
        {
            if (!IsRunning) return;

            if (architecture == null) return;
            IDictionary dictionary = null;

            eventSearch = EditorGUILayout.TextField(string.Empty, eventSearch, "ToolbarSearchTextField");

            switch (eventType)
            {
                case ArchitectureDebuggerWindow.EventType.Type:
                    dictionary = architecture.TypeEventSystem.Events.events;                  
                    break;
                case ArchitectureDebuggerWindow.EventType.String:
                    dictionary = architecture.StringEventSystem.StringEvents.events;                    
                    break;
                case ArchitectureDebuggerWindow.EventType.Enum:
                    dictionary = architecture.EnumEventSystem.WindowEvents.events;
                    break;              
            }

            foreach (var key in dictionary.Keys)
            {
                box = box.IsNullOrEmpty() ? "Wizard Box" : box;
                string info = "已注册事件---->标识:" + key.ToString();
                if (!eventSearch.IsNullOrEmpty())
                {
                    if (!info.Contains(eventSearch))
                        continue;
                }
                var tRect = EditorGUILayout.BeginHorizontal();
                var rect = EditorGUILayout.BeginHorizontal(tRect.Contains(Event.current.mousePosition) ? "SelectionRect" : "Wizard Box", GUILayout.Height(40));
                EditorGUILayout.LabelField(EditorGUIUtility.IconContent("d_console.infoicon"), GUILayout.Width(20));
                EditorGUILayout.BeginVertical();
               
                GUILayout.Label(info);
                GUILayout.Label("参数类型:" + dictionary[key]);

                EditorGUILayout.EndVertical();

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space();
            }
        }

        void DrawInfos(Rule rule)
        {
            foreach (var info in rolds[rule])
            {
                box = box.IsNullOrEmpty() ? "Wizard Box" : box;
                RegistrationAttribute registration = null;
                if (info.registration != null)
                {
                    registration = info.registration as RegistrationAttribute;
                    if (registration?.IsCustomType == true && !registration.registerType.IsAssignableFrom(info.type))
                    {
                        EditorGUILayout.HelpBox($"{info.type} 检测到自动标记使用的自定义类型与指定类没有任何关系，请检查是否为该模块的基类/接口", MessageType.Warning);
                    }
                }

                if (selectType == ArchitectureDebuggerWindow.SelectType.Architecture && registration?.architectureType != architectureType)
                    continue;

                if (selectType == ArchitectureDebuggerWindow.SelectType.NoAttribute && registration != null)
                    continue;

                var tRect = EditorGUILayout.BeginHorizontal();
                var rect = EditorGUILayout.BeginHorizontal(tRect.Contains(Event.current.mousePosition) ? "SelectionRect" : "Wizard Box", GUILayout.Height(25));
                
                EditorGUILayout.LabelField(EditorGUIUtility.IconContent("cs Script Icon"), GUILayout.Width(20));
                GUILayout.Label(info.type.FullName,GUILayout.Width(400));
                EditorGUILayout.Space(20);
                if (registration == null)
                {
                    GUI.color = Color.yellow;
                    GUILayout.Label("未标记自动获取",GUILayout.Width(550));
                }
                else
                {
                    GUI.color = GetButtonColor();                   
                    GUILayout.Label("已标记自动获取 标记架构:" + registration.architectureType + ">>order:" + registration.order,GUILayout.Width(550));                    
                }
                GUI.color = Color.white;
                OpenScript(info.type);

                if (architecture != null && IsRunning)
                {
                    bool contains = false;
                    bool IsInfo = false;
                    if (registration != null)
                        contains = architecture.Container.ContainsType(registration.IsCustomType ? registration.registerType : info.type);
                    else
                    {
                        contains = architecture.Container.ContainsInstance(info.type);
                        IsInfo = true;
                    }

                    if (contains)
                    {
                        GUI.color = Color.green;
                        GUILayout.Label(IsInfo ? "已注册(非自动)!" : "已注册!", GUILayout.Width(100));
                        GUI.color = Color.white;
                    }
                    else
                    {
                        GUILayout.Label(string.Empty, GUILayout.Width(100));
                    }
                }
                else 
                {
                    GUILayout.Label(string.Empty, GUILayout.Width(100));
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space();               
            }

        }

        void OpenScript(Type type)
        {
            if (GUILayout.Button("打开脚本", GUILayout.Width(80)))
            {
                try
                {
                    var item = AssetDatabase.GetAllAssetPaths()
                         .Where(x => x.EndsWith(".cs"))
                         .Select(path => AssetDatabase.LoadAssetAtPath<MonoScript>(path))
                         .Where(x => x.GetClass() == type).FirstOrDefault();
                    AssetDatabase.OpenAsset(item);

                    if (item == null)
                        LogKit.E("没有找到脚本文件，请检查是否文件命名与类名一致!");
                }
                catch
                {
                    LogKit.E("没有找到脚本文件，请检查是否文件命名与类名一致!");
                }
            }

        }
    }
}
#endif