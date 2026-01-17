///=====================================================
/// - FileName:      IMissionLoader.cs
/// - NameSpace:     YukiFrameWork.Missions
/// - Description:   高级定制脚本生成
/// - Creation Time: 1/15/2026 7:17:27 PM
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using XFABManager;
namespace YukiFrameWork.Missions
{
    public interface IMissionLoader : IResLoader<MissionTreeSO>
    {



    }

    public class ABManagerMissionLoader : IMissionLoader
    {
        readonly string projectName;
        public ABManagerMissionLoader(string projectName)
        {
            this.projectName = projectName;
        }

         public TItem Load<TItem>(string name) where TItem : MissionTreeSO
        {
            return AssetBundleManager.LoadAsset<TItem>(projectName,name);
        }

        public async void LoadAsync<TItem>(string name, Action<TItem> onCompleted) where TItem : MissionTreeSO
        {
            var result = await AssetBundleManager.LoadAssetAsync<TItem>(projectName,name);
            onCompleted?.Invoke(result);
        }

        public void UnLoad(MissionTreeSO item)
        {
            AssetBundleManager.UnloadAsset(item);
        }
    }
}
