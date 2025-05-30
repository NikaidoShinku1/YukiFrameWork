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
            if (!config)
            {
                UpdateConfig();
                if (!config)
                {
                    EditorGUILayout.HelpBox("框架配置文件丢失!请尝试点击下方按钮修复!", MessageType.Error);
                    if (GUILayout.Button("Fix Now", GUILayout.Height(50)))
                    {
                        EditorInit();
                    }
                    return;
                }
            }
            base.OnImGUI();

            
        }
        [InitializeOnLoadMethod]
        static void EditorInit()
        {
            FrameworkConfigInfo.CreateConfig();
            LogKit.EditorInit();
        }
        ImportSettingWindow.VersionData versionData;
        protected override void DrawMenu()
        {
            base.DrawMenu();
            TextAsset versionText = AssetDatabase.LoadAssetAtPath<TextAsset>(ImportSettingWindow.packagePath + "/package.json");
            if(versionData == null)
                versionData = SerializationTool.DeserializedObject<ImportSettingWindow.VersionData>(versionText.text);
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical();         
            OpenUrl("框架版本:" + versionData.version, versionData.author.url);
            GUILayout.EndVertical();
        }

        private void OpenUrl(string name, string url)
        {
            if (GUILayout.Button(name, "WarningOverlay"))
            {
                if (string.IsNullOrEmpty(url))
                    return;
                Application.OpenURL(url);
            }
        }
        protected override OdinMenuTree BuildMenuTree()
        {
           
            OdinMenuTree tree = new OdinMenuTree(supportsMultiSelect: true)
            {              
                { $"框架基本设置", config, Sirenix.OdinInspector.SdfIconType.File },

                { $"框架简单序列化工具/C#转文件流",new SerializationWindow(0),Sirenix.OdinInspector.SdfIconType.SegmentedNav},
                { $"框架简单序列化工具/Excel转Json工具",new SerializationWindow(1),Sirenix.OdinInspector.SdfIconType.FileEarmarkExcel },
                { $"框架简单序列化工具/Json可视化器",JsonSerializeEditor.instance,SdfIconType.ViewList},              
            };
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

            try
            {
                tree.Add("AnimationClip-Sprite转换工具/Single", new AnimationClipConvertWindow(), SdfIconType.Image);
                tree.Add("AnimationClip-Sprite转换工具/Multiple", new MultipleAnimationConvertWindow(), SdfIconType.Images);
            }
            catch
            {

            }


            tree.Add("Unity样式拓展工具", new GUIStyleExtensionWindow(), SdfIconType.Image);

            var example = AssetDatabase.LoadAssetAtPath<Example>($"{ImportSettingWindow.packagePath}/Framework/Abstract/Example/Example.asset");
            if (example != null)
            {
                tree.Add($"框架规则示例", example, SdfIconType.PersonRolodex);
            }
            tree.Add("DeepSeek AI代码生成设置", new DeepSeekSettingWindow(), SdfIconType.AppIndicator);
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

      
    


    public static class UnityEngineSavingExtension
    {
        public static void Save<T>(this T core) where T : UnityEngine.Object
        {
            EditorUtility.SetDirty(core);
            AssetDatabase.SaveAssets();          
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