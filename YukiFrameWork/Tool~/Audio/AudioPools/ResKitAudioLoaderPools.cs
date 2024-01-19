///=====================================================
/// - FileName:      ResKitAudioLoaderPools.cs
/// - NameSpace:     YukiFrameWork.Audio
/// - Created:       Yuki
/// - Email:         Yuki@qq.com
/// - Description:   ResKit资源加载器类池
/// - Creation Time: 2023年12月16日 13:55:14
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================

using UnityEngine;
using System;
using YukiFrameWork.Pools;
namespace YukiFrameWork.Audio
{
    public class ResKitAudioLoaderPools : AudioLoaderPools
    {
        public ResKitAudioLoaderPools(string projectName) : base(projectName)
        {
        }

        public override IAudioLoader CreateAudioLoader(string projectName)
        {
            return new ResKitAudioLoader(projectName);
        }
    }
}