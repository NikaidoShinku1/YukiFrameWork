///=====================================================
/// - FileName:      IEquipmentLoader.cs
/// - NameSpace:     RPG
/// - Description:   高级定制脚本生成
/// - Creation Time: 12/12/2025 1:23:17 PM
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using XFABManager;
namespace YukiFrameWork.Equips
{
    public interface IEquipmentLoader : IResLoader<EquipmentConfigDataManager>
    {



    }

    public class ABManagerEquipmentLoader : IEquipmentLoader
    {
        private readonly string projectName;
        public ABManagerEquipmentLoader(string projectName)
        {
            this.projectName = projectName;
        }
        public TItem Load<TItem>(string name) where TItem : EquipmentConfigDataManager
        {
            return AssetBundleManager.LoadAsset<TItem>(projectName, name);
        }

        public async void LoadAsync<TItem>(string name, Action<TItem> onCompleted) where TItem : EquipmentConfigDataManager
        {
            var item = await AssetBundleManager.LoadAssetAsync<TItem>(projectName, name);
            onCompleted?.Invoke(item);
        }

        public void UnLoad(EquipmentConfigDataManager item)
        {
            AssetBundleManager.UnloadAsset(item);
        }
    }
}
