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
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
using Sirenix.OdinInspector.Editor;
namespace YukiFrameWork
{
    public class FrameWorkDisignWindow : OdinMenuEditorWindow
    {      
        public static Rect LocalPosition => Instance.position;

        [MenuItem("YukiFrameWork/Local Configuration",false,-1000)]
        public static void OpenWindow()
        {            
            var instance = GetWindow<FrameWorkDisignWindow>();           
            instance.Show();
            instance.titleContent = new GUIContent("框架本地配置");
        }

        private Dictionary<string,int> titleDicts;

        private int selectIndex;
        private FrameworkConfigInfo config;    
       
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
                {"脚本生成器",0 },
                {"本地化配置",1 },
                {"程序集设置",2 },
                {"C#转文件流工具",0},
                { "Excel转Json",1},             
            };
        }

        protected override void OnImGUI()
        {
            EditorGUILayout.BeginHorizontal();
            base.OnImGUI();

            if (config == null)
                UpdateConfig();

            if (titleDicts == null)
                UpdateTitles();


            try
            {
                var configItem = MenuTree.MenuItems.FirstOrDefault(x => x.Name == "本地化配置" && x.IsSelected);

                if (configItem != null)
                {
                    config.SelectIndex = 1;
                    return;
                }

                var selectItem = UpdateSelect();
                if (selectItem != null)
                {
                    if (titleDicts.TryGetValue(selectItem.Name, out int value) == true)
                    {
                        if (selectItem.Parent != null)
                        {
                            if (selectItem.Parent.FlatTreeIndex == 0 || selectItem.Parent.FlatTreeIndex == 6)
                            {
                                config.SelectIndex = value;
                            }                        
                        }
                    }

                }
            }
            catch { }
                EditorGUILayout.EndHorizontal();

        }
        protected override OdinMenuTree BuildMenuTree()
        {
            string[] keys = titleDicts.Keys.ToArray();
            OdinMenuTree tree = new OdinMenuTree(supportsMultiSelect: true)
            {
                { $"框架基本设置/工具导入窗口",new ImportSettingWindow(this),SdfIconType.GearFill },
                { $"框架基本设置/{keys[0]}", config.GetFramework(0), Sirenix.OdinInspector.SdfIconType.File },
                { $"框架基本设置/{keys[2]}",config.GetFramework(2),Sirenix.OdinInspector.SdfIconType.AspectRatio},

                { $"框架序列化工具/{keys[3]}",new SerializationWindow(0),Sirenix.OdinInspector.SdfIconType.SegmentedNav},
                { $"框架序列化工具/{keys[4]}",new SerializationWindow(1),Sirenix.OdinInspector.SdfIconType.FileEarmarkExcel },
                { $"{keys[1]}",config.GetFramework(1),Sirenix.OdinInspector.SdfIconType.Controller},
            };

            foreach (var item in Resources.FindObjectsOfTypeAll<LocalizationConfig>())
            {
                tree.Add($"{keys[1]}/{item.name}", item, Sirenix.OdinInspector.SdfIconType.ClipboardData);
            }

            try
            {
                System.Type saveToolType = AssemblyHelper.GetType("YukiFrameWork.SaveToolConfig");

                if (saveToolType != null)
                {
                    tree.Add($"框架存档工具", Resources.Load("SaveToolConfig"),Sirenix.OdinInspector.SdfIconType.Save);
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
                        tree.Add($"XFABManager/About", Activator.CreateInstance(helpType),SdfIconType.PatchExclamationFill);
                }

            }
            catch { }

            try
            {
                System.Type buffType = AssemblyHelper.GetType("YukiFrameWork.Buffer.BuffDataBase");

                if (buffType != null)
                {
                    Object[] objects = Resources.FindObjectsOfTypeAll(buffType);
                    for (int i = 0; i < objects.Length; i++)
                    {
                        tree.Add($"BuffKit配置窗口集合/{objects[i].name}_{objects[i].GetInstanceID()}", objects[i],SdfIconType.BookmarkStar);
                    }
                }
            }
            catch { }

            try
            {
                System.Type itemType = AssemblyHelper.GetType("YukiFrameWork.Item.ItemDataBase");

                if (itemType != null)
                {
                    Object[] objects = Resources.FindObjectsOfTypeAll(itemType);
                    for (int i = 0; i < objects.Length; i++)
                    {
                        tree.Add($"ItemKit配置窗口集合/{objects[i].name}_{objects[i].GetInstanceID()}", objects[i], SdfIconType.Wallet);
                    }
                }
            }
            catch { }       

            tree.Add("Unity样式拓展工具", new GUIStyleExtensionWindow(),SdfIconType.Image);

            var example = AssetDatabase.LoadAssetAtPath<Example>($"{ImportSettingWindow.packagePath}/Framework/Abstract/Example/Example.asset");
            if (example != null)
            {
                tree.Add($"框架规则示例", example, SdfIconType.PersonRolodex);
            }
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
    }
}
#endif