///=====================================================
/// - FileName:      ABManagerSkillLoader.cs
/// - NameSpace:     YukiFrameWork.Skill
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/6/11 14:01:51
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using XFABManager;
using UnityEngine.U2D;
namespace YukiFrameWork.Skill
{
    public class ABManagerSkillLoader : ISkillLoader
    {
        private readonly string projectName;
        public ABManagerSkillLoader(string projectName)
            => this.projectName = projectName;
        public SkillDataBase Load(string path)
        {
            return AssetBundleManager.LoadAsset<SkillDataBase>(projectName,path);
        }

        public void LoadAsync(string path, Action<SkillDataBase> onCompleted)
        {
            AssetBundleManager
                .LoadAssetAsync<SkillDataBase>(projectName, path)
                .AddCompleteEvent(request => onCompleted?.Invoke(request.asset as SkillDataBase));
        }
  
    }
}
