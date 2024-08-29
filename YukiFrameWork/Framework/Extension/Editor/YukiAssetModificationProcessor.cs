///=====================================================
/// - FileName:      YukiAssetModificationProcessor.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/4/3 18:13:31
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using YukiFrameWork.Extension;

#if UNITY_EDITOR
using UnityEditor;

namespace YukiFrameWork
{
	public class YukiAssetModificationProcessor : UnityEditor.AssetModificationProcessor
	{
        public static AssetDeleteResult OnWillDeleteAsset(string assetPath, RemoveAssetOptions option)
        {
            if (assetPath.StartsWith("Assets/Resources/SaveToolConfig"))
            {
                EditorUtility.DisplayDialog("提示", "文件:Assets/Resources/SaveToolConfig.asset 是框架SaveTool的配置文件,不能删除!否则会无法正常使用存档系统!", "确定");
                return AssetDeleteResult.FailedDelete;
            }
            else if (assetPath.StartsWith("Assets/Resources/FrameworkConfigInfo"))
            {
                EditorUtility.DisplayDialog("提示", "文件:Assets/Resources/FrameworkConfigInfo.asset 是架构的配置文件,不能删除!否则无法正确初始化架构!", "确定");
                return AssetDeleteResult.FailedDelete;
            }
            else if (assetPath.Contains("Sirenix"))
            {
                EditorUtility.DisplayDialog("提示", "文件夹:Sirenix 是框架依赖的编辑器拓展,不能删除!否则无法正确使用框架!", "确定");
                return AssetDeleteResult.FailedDelete;
            }
            else if (assetPath.EndsWith("LogConfig.asset"))
            {
                EditorUtility.DisplayDialog("提示", "LogConfig是框架依赖的日志系统配置，不能删除!", "确定");
                return AssetDeleteResult.FailedDelete;
            }
            else if (assetPath == $"{ImportSettingWindow.packagePath}/Framework/Abstract/Example/Example.asset")
            {
                EditorUtility.DisplayDialog("提示", "Example是框架提供的示例模块,不能删除!", "确定");
                return AssetDeleteResult.FailedDelete;
            }
            /*else if (assetPath.EndsWith(".json") || assetPath.EndsWith(".JSON") || assetPath.EndsWith(".Json"))
            {
                JsonSerializaConfig.Remove_Json_Object(AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath).GetHashCode());
                return AssetDeleteResult.DidDelete;
            }*/
            else
                return AssetDeleteResult.DidNotDelete;
        }
    }
}
#endif