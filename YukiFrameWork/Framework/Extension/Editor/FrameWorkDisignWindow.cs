///=====================================================
/// - FileName:      FrameWorkDisignWindow.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/5/7 21:47:08
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using YukiFrameWork.Extension;
using YukiFrameWork.ExampleRule;
using Sirenix.OdinInspector;
using System;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
using YukiFrameWork.JsonInspector;
using Sirenix.OdinInspector.Editor;
namespace YukiFrameWork
{
    public class FrameWorkDisignWindow : OdinMenuEditorWindow
    {
        public static Rect LocalPosition => Instance.position;

        [MenuItem("YukiFrameWork/Local Configuration", false, -1000)]
        public static void OpenWindow()
        {
            var instance = GetWindow<FrameWorkDisignWindow>();
            instance.Show();
            instance.titleContent = new GUIContent("框架本地配置");
        }

        private Dictionary<string, int> titleDicts;

        private int selectIndex;
        private FrameworkConfigInfo config;

        private void Awake()
        {
            Instance = this; 
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            UpdateTitles();
            UpdateConfig();
            Instance = this;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            JsonSerializeEditor.instance.OnDestroy();
            Instance = null;
        }

        OdinMenuItem UpdateSelect()
        {
            return MenuTree.MenuItems.SelectMany(x => x.GetChildMenuItemsRecursive(true)).Where(x => x.IsSelected).FirstOrDefault();
        }

        void UpdateConfig()
        {
            config = Resources.Load<FrameworkConfigInfo>(nameof(FrameworkConfigInfo));
        }

        void UpdateTitles()
        {
            titleDicts ??= new Dictionary<string, int>()
            {
                {"C#转文件流工具",0},
                { "Excel转Json",1},
            };
        }

        protected override void OnImGUI()
        {
            base.OnImGUI();

            if (config == null)
                UpdateConfig();

            if (titleDicts == null)
                UpdateTitles();


            try
            {

            }
            catch { }

        }
        protected override OdinMenuTree BuildMenuTree()
        {
            string[] keys = titleDicts.Keys.ToArray();
            OdinMenuTree tree = new OdinMenuTree(supportsMultiSelect: true)
            {
                { $"工具导入窗口",new ImportSettingWindow(this),SdfIconType.GearFill },
                { $"框架基本设置", config, Sirenix.OdinInspector.SdfIconType.File },

                { $"框架序列化工具/C#转文件流",new SerializationWindow(0),Sirenix.OdinInspector.SdfIconType.SegmentedNav},
                { $"框架序列化工具/Excel转Json工具",new SerializationWindow(1),Sirenix.OdinInspector.SdfIconType.FileEarmarkExcel },
                { $"框架序列化工具/Json可视化器",JsonSerializeEditor.instance,SdfIconType.ViewList}
            };

            foreach (var item in Resources.FindObjectsOfTypeAll<LocalizationConfig>())
            {
                tree.Add($"LocalizationConfig/{item.name}", item, Sirenix.OdinInspector.SdfIconType.ClipboardData);
            }

            try
            {
                LogConfig config = Resources.Load<LogConfig>(nameof(LogConfig));
                if (config != null)
                {
                    tree.Add($"LogKit日志配置窗口", config, SdfIconType.InfoCircleFill);
                }
            }
            catch
            {

            }

            try
            {
                System.Type saveToolType = AssemblyHelper.GetType("YukiFrameWork.SaveToolConfig");

                if (saveToolType != null)
                {
                    tree.Add($"框架存档工具", Resources.Load("SaveToolConfig"), Sirenix.OdinInspector.SdfIconType.Save);
                }
            }
            catch { }

            try
            {
                System.Type abManagerType = AssemblyHelper.GetType("XFABManager.XFAssetBundleManagerProjects");

                if (abManagerType != null)
                {
                    tree.Add($"XFABManager", Activator.CreateInstance(abManagerType));
                    Type projectType = AssemblyHelper.GetType("XFABManager.ProfileWindow");
                    if (projectType != null)
                        tree.Add($"XFABManager/ProfileWindow", Activator.CreateInstance(projectType), SdfIconType.Gear);

                    Type helpType = AssemblyHelper.GetType("XFABManager.XFAssetBundleManagerHelp");
                    if (helpType != null)
                        tree.Add($"XFABManager/About", Activator.CreateInstance(helpType), SdfIconType.PatchExclamationFill);
                }

            }
            catch { }         

            tree.Add("Unity样式拓展工具", new GUIStyleExtensionWindow(), SdfIconType.Image);

            var example = AssetDatabase.LoadAssetAtPath<Example>($"{ImportSettingWindow.packagePath}/Framework/Abstract/Example/Example.asset");
            if (example != null)
            {
                tree.Add($"框架规则示例", example, SdfIconType.PersonRolodex);
            }

            tree.Add("组件拓展特性介绍", new AttributeInfoWIndow(), SdfIconType.AlarmFill);
            tree.Add("框架说明文档", new FrameworkInfoByWindow(), SdfIconType.Bug);
            return tree;       
        }
        public static FrameWorkDisignWindow Instance;

        // 每秒10帧更新
        void OnInspectorUpdate()
        {
            //开启窗口的重绘，不然窗口信息不会刷新
            Repaint();
        }
    }

    public class FrameworkInfoByWindow
    {        
        public string updateInfo => AssetDatabase.LoadAssetAtPath<TextAsset>(ImportSettingWindow.packagePath + "/Framework/Extension/UpdateInfo.md")?.text;

        [OnInspectorGUI]
        void OnInspectorGUI()
        {
            var title = new GUIStyle()
            {
                fontSize = 20,
                alignment = TextAnchor.UpperCenter,
                fontStyle = FontStyle.Bold

            };
            title.normal.textColor = Color.white;
            GUILayout.Label("YukiFramework", title);
            GUILayout.Label("工具教学链接");
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical(GUILayout.Width(200));
            Button("LocalizationKit_本地化套件", "https://gitee.com/NikaidoShinku/YukiFrameWork/blob/master/YukiFrameWork/Framework/Localization/LocalizationInfo.md");
            Button("DiaLogKit_对话系统", "https://gitee.com/NikaidoShinku/YukiFrameWork/blob/master/YukiFrameWork/Tool~/DiaLogKit/DiaLogKit.md");
            Button("BuffKit_框架Buff系统", "https://gitee.com/NikaidoShinku/YukiFrameWork/blob/master/YukiFrameWork/Tool~/BuffKit/BuffKit.md");
            Button("LogKit_日志工具", "https://gitee.com/NikaidoShinku/YukiFrameWork/blob/master/YukiFrameWork/Framework/LogKit/15.%E6%8E%A7%E5%88%B6%E5%8F%B0%E6%97%A5%E5%BF%97%E5%B7%A5%E5%85%B7.md");
            Button("ItemKit_框架背包系统", "https://gitee.com/NikaidoShinku/YukiFrameWork/blob/master/YukiFrameWork/Tool~/ItemKit/14.%E8%83%8C%E5%8C%85%E7%B3%BB%E7%BB%9F(%E9%80%9A%E7%94%A8).md");
            Button("SerializationTool_框架序列化工具使用", "https://gitee.com/NikaidoShinku/YukiFrameWork/blob/master/YukiFrameWork/Plugins/Serialization/%E5%BA%8F%E5%88%97%E5%8C%96%E5%B7%A5%E5%85%B7.md");
            Button("SaveTool_框架存档工具使用", "https://gitee.com/NikaidoShinku/YukiFrameWork/blob/master/YukiFrameWork/Tool~/SaveTool/%E5%AD%98%E6%A1%A3%E7%B3%BB%E7%BB%9F.md");
            Button("框架引导工具使用", "https://gitee.com/NikaidoShinku/YukiFrameWork/blob/master/YukiFrameWork/Tool~/PilotKit/PilotKit.md");
            Button("Singleton_框架万能单例介绍", "https://gitee.com/NikaidoShinku/YukiFrameWork/blob/master/YukiFrameWork/Framework/Singleton/9.%E5%8D%95%E4%BE%8B.md");
            Button("EventSystem_框架广播系统介绍", "https://gitee.com/NikaidoShinku/YukiFrameWork/blob/master/YukiFrameWork/Framework/Events/7.%E6%B6%88%E6%81%AF%E5%B9%BF%E6%92%AD%E6%A8%A1%E5%9D%97.md");
            Button("UIKit_框架UI模块", "https://gitee.com/NikaidoShinku/YukiFrameWork/blob/master/YukiFrameWork/Tool~/UI/6.UI%E6%A8%A1%E5%9D%97.md");
            Button("AudioKit_框架声音管理", "https://gitee.com/NikaidoShinku/YukiFrameWork/blob/master/YukiFrameWork/Tool~/Audio/8.%E5%A3%B0%E9%9F%B3%E7%AE%A1%E7%90%86%E6%A8%A1%E5%9D%97.md");
            Button("ActionKit_时序动作套件说明", "https://gitee.com/NikaidoShinku/YukiFrameWork/blob/master/YukiFrameWork/Tool~/ActionKit/5.%E5%8A%A8%E4%BD%9C%E6%97%B6%E5%BA%8F%E7%AE%A1%E7%90%86%E6%A8%A1%E5%9D%97.md");
            Button("Old StateManager_框架可视化状态机模块", "https://gitee.com/NikaidoShinku/YukiFrameWork/blob/master/YukiFrameWork/Tool~/StateMechine/3.%E7%8A%B6%E6%80%81%E6%9C%BA.md",Color.yellow);
            Button("StateManager_全新动作设计状态机模块", "https://gitee.com/NikaidoShinku/YukiFrameWork/blob/master/YukiFrameWork/Tool~/StateManager/StateMachine/s.StateManager.md");
            Button("Extension_框架拓展模块", "https://gitee.com/NikaidoShinku/YukiFrameWork/blob/master/YukiFrameWork/Framework/Extension/13.%E6%8B%93%E5%B1%95.md");
            Button("PoolsKit_框架设置简易对象池", "https://gitee.com/NikaidoShinku/YukiFrameWork/blob/master/YukiFrameWork/Framework/Pools/12.%E5%AF%B9%E8%B1%A1%E6%B1%A0%E6%A8%A1%E5%9D%97.md");
            Button("BindableProperty强化数据绑定类", "https://gitee.com/NikaidoShinku/YukiFrameWork/blob/master/YukiFrameWork/Framework/Abstract/11.BindableProperty.md");
            Button("BezierUtility_框架贝塞尔曲线拓展", "https://gitee.com/NikaidoShinku/YukiFrameWork/blob/master/YukiFrameWork/Tool~/Bezier/Bezier.md");
            Button("XFABManager插件教程官网", "https://gitee.com/xianfengkeji/xfabmanager");

            GUILayout.EndVertical();     
            GUILayout.BeginVertical(GUILayout.Width(400));     
            GUILayout.Label(updateInfo, "Framebox");
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();            
      
        }

        void Button(string name,string url,Color color = default)
        {
            if(color != default)
            GUI.color = color;
            if (GUILayout.Button(name,GUILayout.Height(40),GUILayout.Width(250)))
            {
                Application.OpenURL(url);
            }
            GUI.color = Color.white;
            GUILayout.Space(5);
        }
       
    }


    public static class UnityEngineSavingExtension
    {
        public static void Save<T>(this T core) where T : UnityEngine.Object
        {
            EditorUtility.SetDirty(core);
            AssetDatabase.SaveAssets();          
        }

        public static List<SerializedProperty> GetAllSerializedProperty(this SerializedObject serializedObject)
        {
            List<SerializedProperty> properties = new List<SerializedProperty>();

            Type type = serializedObject.targetObject.GetType();

            List<FieldInfo> fields = GetFields(type);

            foreach (FieldInfo field in fields)
            {
                if (!field.IsInitOnly) // 忽略只读字段 
                {
                    SerializedProperty property = serializedObject.FindProperty(field.Name);
                    if (property == null) continue;
                    if (IsContainProperty(property, properties)) continue;
                    properties.Add(property);
                }
            }

            return properties;
        }

        private static bool IsContainProperty(SerializedProperty property, List<SerializedProperty> properties)
        {           
            foreach (var item in properties)
            {
                if (item == null) continue;
                if (item.name == property.name) return true;
            }

            return false;
        }

        private static List<FieldInfo> GetFields(Type type)
        {

            if (type == null) return null;

            List<FieldInfo> fields = new List<FieldInfo>();

            fields.AddRange(type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance));
            if (type.BaseType != null)
                fields.AddRange(GetFields(type.BaseType));
            return fields;
        }

      
        /// <summary>
        /// 在Scene视图绘制2D胶囊体
        /// </summary>
        /// <param name="center">中心点</param>
        /// <param name="size">大小</param>
        /// <param name="dir">方向</param>
        public static void DrawCapsule2D(this Vector3 center, Vector2 size, CapsuleDirection2D dir)
        {
            size.Set(Mathf.Abs(size.x), Mathf.Abs(size.y));

            if (dir == CapsuleDirection2D.Vertical)
            {
                if (size.y < size.x)
                    size.Set(size.x, size.x);

                Handles.DrawWireArc(new Vector3(0, (size.y - size.x) / 2) + center, -Vector3.forward, Vector3.left, 180, size.x / 2);

                Handles.DrawLine(new Vector3(-size.x / 2, -(size.y - size.x) / 2) + center, new Vector3(-size.x / 2, (size.y - size.x) / 2));
                Handles.DrawLine(new Vector3(size.x / 2, -(size.y - size.x) / 2) + center, new Vector3(size.x / 2, (size.y - size.x) / 2));

                Handles.DrawWireArc(new Vector3(0, -(size.y - size.x) / 2) + center, Vector3.forward, Vector3.left, 180, size.x / 2);
            }
            else
            {
                if (size.x < size.y)
                    size.Set(size.y, size.y);

                Handles.DrawWireArc(new Vector3(-(size.x - size.y) / 2, 0) + center, -Vector3.forward, Vector3.down, 180, size.y / 2);

                Handles.DrawLine(new Vector3(-(size.x - size.y) / 2, -size.y / 2) + center, center + new Vector3((size.x - size.y) / 2, -size.y / 2));
                Handles.DrawLine(new Vector3(-(size.x - size.y) / 2, size.y / 2) + center, center + new Vector3((size.x - size.y) / 2, size.y / 2));

                Handles.DrawWireArc(new Vector3((size.x - size.y) / 2, 0) + center, Vector3.forward, Vector3.down, 180, size.y / 2);
            }
        }
    }
}
#endif