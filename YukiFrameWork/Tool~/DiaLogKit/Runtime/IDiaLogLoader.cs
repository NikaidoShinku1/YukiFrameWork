///=====================================================
/// - FileName:      IDiaLogLoader.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/7/14 14:00:26
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using XFABManager;
namespace YukiFrameWork.DiaLogue
{
	public interface IDiaLogLoader : IResLoader<NodeTree>
	{

	}

    public class ABManagerDiaLogLoader : IDiaLogLoader
    {
        readonly string projectName;
        public ABManagerDiaLogLoader(string projectName)
            => this.projectName = projectName;
        public TItem Load<TItem>(string name) where TItem : NodeTree
        {
            return AssetBundleManager.LoadAsset<TItem>(projectName,name);
        }
        public void LoadAsync<TItem>(string name, Action<TItem> onCompleted) where TItem : NodeTree
        {
            AssetBundleManager.LoadAssetAsync<TItem>(projectName, name)
                .AddCompleteEvent(v => onCompleted?.Invoke(v.asset as TItem));
        }

        public void UnLoad(NodeTree item)
        {
            AssetBundleManager.UnloadAsset(item);
        }
    }
}
