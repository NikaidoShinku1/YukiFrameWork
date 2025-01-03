///=====================================================
/// - FileName:      ABManagerBuffLoader.cs
/// - NameSpace:     YukiFrameWork.Buffer
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/5/8 15:36:43
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using XFABManager;
namespace YukiFrameWork.Buffer
{
    public class ABManagerBuffLoader : IBuffLoader
    {
        private readonly string projectName;      
        public ABManagerBuffLoader(string projectName)
        {
            this.projectName = projectName;
        }
        public TItem Load<TItem>(string path) where TItem : BuffDataBase
        {
            return AssetBundleManager.LoadAsset<TItem>(projectName, path);
        }

        public void LoadAsync<TItem>(string path, Action<TItem> callBack) where TItem : BuffDataBase
        {
            AssetBundleManager.LoadAssetAsync<TItem>(projectName, path)
                .AddCompleteEvent(v => callBack?.Invoke(v.asset as TItem));
        }

        public void UnLoad(BuffDataBase item)
        {
            AssetBundleManager.UnloadAsset(item);
        }
    }
}
