///=====================================================
/// - FileName:      IAudioLoaderPools.cs
/// - NameSpace:     YukiFrameWork.Audio
/// - Created:       Yuki
/// - Email:         Yuki@qq.com
/// - Description:   声音加载器类池
/// - Creation Time: 2023年12月16日 13:27:26
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================

using UnityEngine;
using System;
using YukiFrameWork.Pools;
namespace YukiFrameWork.Audio
{
    public interface IAudioLoaderPools
    {
        IAudioLoader CreateAudioLoader();
    }
}