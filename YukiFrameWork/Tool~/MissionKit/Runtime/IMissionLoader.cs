///=====================================================
/// - FileName:      IMissionLoader.cs
/// - NameSpace:     YukiFrameWork.MissionKit
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/10/14 0:46:10
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using XFABManager;
namespace YukiFrameWork.Missions
{
    public interface IMissionLoader : IResLoader<MissionConfigManager>
    {
       
    }

    public class ABManagerMissionLoader : IMissionLoader
    {
        private readonly string projectName;
        public ABManagerMissionLoader(string projectName)
        {
            this.projectName = projectName;
        }
        public TItem Load<TItem>(string name) where TItem : MissionConfigManager
        {
            return AssetBundleManager.LoadAsset<TItem>(projectName,name);
        }

        public async void LoadAsync<TItem>(string name, Action<TItem> onCompleted) where TItem : MissionConfigManager
        {
            var item = await AssetBundleManager.LoadAssetAsync<TItem>(projectName, name);
            onCompleted?.Invoke(item);
        }

        public void UnLoad(MissionConfigManager item)
        {
            AssetBundleManager.UnloadAsset(item);
        }
    }
}
