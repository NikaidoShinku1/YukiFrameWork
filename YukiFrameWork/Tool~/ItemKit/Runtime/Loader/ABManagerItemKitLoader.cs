///=====================================================
/// - FileName:      DefaultItemKitLoader.cs
/// - NameSpace:     YukiFrameWork.Item
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/4/26 21:07:16
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using XFABManager;
namespace YukiFrameWork.Item
{
    public class ABManagerItemKitLoader : IItemKitLoader
    {
        private string projectName;

        public ABManagerItemKitLoader(string projectName)
        {
            this.projectName = projectName;
        }

        public TItem Load<TItem>(string name) where TItem : ItemDataManager
        {
            return AssetBundleManager.LoadAsset<TItem>(projectName, name);
        }

        public void LoadAsync<TItem>(string name, Action<TItem> onCompleted) where TItem : ItemDataManager
        {
            AssetBundleManager.LoadAssetAsync<TItem>(projectName,name).AddCompleteEvent(v => onCompleted?.Invoke(v.asset as TItem));
        }

        public void UnLoad(ItemDataManager item)
        {
            AssetBundleManager.UnloadAsset(item);
        }
    }   
}
