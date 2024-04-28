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

        public ItemDataBase LoadItemDataBase(string dataBaseName)
        {
            return AssetBundleManager.LoadAsset<ItemDataBase>(projectName, dataBaseName);
        }

        public void LoadItemDataBaseAsync(string dataBaseName, Action<ItemDataBase> callBack)
        {
            AssetBundleManager.LoadAssetAsync<ItemDataBase>(projectName, dataBaseName).AddCompleteEvent(v => callBack?.Invoke(v.asset as ItemDataBase));
        }
    }
}
