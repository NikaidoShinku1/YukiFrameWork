///=====================================================
/// - FileName:      ItemDesignerWindow.cs
/// - NameSpace:     YukiFrameWork.Item
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/5/31 17:14:08
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
using Sirenix.OdinInspector.Editor;
namespace YukiFrameWork.Item
{
    public class ItemDesignerWindow : OdinMenuEditorWindow
    {
        protected override OdinMenuTree BuildMenuTree()
        {
            var tree = new OdinMenuTree();

            Type dataType = typeof(ItemDataBase);

            foreach (var item in Resources.FindObjectsOfTypeAll(dataType))
            {
                tree.Add($"ItemDataBase配置集合/{item.name}_{item.GetInstanceID()}", item, Sirenix.OdinInspector.SdfIconType.BookmarkCheck);
            }
            return tree;
        }

        [MenuItem("YukiFrameWork/ItemDesignWindow")]
        public static void OpenWindow()
        {
            GetWindow<ItemDesignerWindow>().titleContent = new GUIContent("背包配置窗口");
        }
    }
}
#endif