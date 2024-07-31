///=====================================================
/// - FileName:      DiaLogExtension.cs
/// - NameSpace:     YukiFrameWork.Dialogue
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/7/27 18:44:52
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
namespace YukiFrameWork.DiaLogue
{
	public class DiaLogEditorRemoveTool : UnityEditor.AssetModificationProcessor
    {
        public static AssetDeleteResult OnWillDeleteAsset(string assetPath, RemoveAssetOptions option)
        {
            if (AssetDatabase.LoadAssetAtPath<NodeTree>(assetPath) is NodeTree tree)
            {
                DiaLogGraphWindow.CloseWindow();
            }
            return AssetDeleteResult.DidNotDelete;
        }
    }
}
#endif