///=====================================================
/// - FileName:      ABManagerAudioLoaderPools.cs
/// - NameSpace:     YukiFrameWork.Audio
/// - Created:       Yuki
/// - Email:         Yuki@qq.com
/// - Description:   ABManager资源加载器类池
/// - Creation Time: 2023年12月16日 13:55:14
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================

using UnityEngine;
using System;
using YukiFrameWork.Pools;
namespace YukiFrameWork.Audio
{
    public class ABManagerAudioLoaderPools : IAudioLoaderPools
    {
        private string projectName;
        public ABManagerAudioLoaderPools(string projectName)
        {
            this.projectName = projectName;
        }

        public IAudioLoader CreateAudioLoader()
        {
            return new ABManagerAudioLoader(projectName);
        }
    }
}