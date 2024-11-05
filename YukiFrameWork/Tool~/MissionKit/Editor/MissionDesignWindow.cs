///=====================================================
/// - FileName:      MissionDesignWindow.cs
/// - NameSpace:     YukiFrameWork.MissionKit
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/10/20 20:43:13
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;

#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using UnityEditor;
namespace YukiFrameWork.Missions
{
    public class MissionDesignWindow : OdinMenuEditorWindow
    {
        protected override OdinMenuTree BuildMenuTree()
        {
            OdinMenuTree tree = new OdinMenuTree();
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(MissionConfigBase)}");

            foreach (var item in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(item);

                if (path.IsNullOrEmpty()) continue;

                MissionConfigBase config = AssetDatabase.LoadAssetAtPath<MissionConfigBase>(path);

                if (!config) continue;

                tree.Add($"MissionConfig配置窗口集合/{config.name}_{config.GetInstanceID()}", config);
            }

            return tree;
        }
        [MenuItem("YukiFrameWork/MissionDesignerWindow",false)]
        public static void OpenWindow()
        {
            GetWindow<MissionDesignWindow>().titleContent = new GUIContent("任务配置窗口(Plus)");
        }
    }
}
#endif