///=====================================================
/// - FileName:      IAudioLoader.cs
/// - NameSpace:     YukiFrameWork.Audio
/// - Created:       Yuki
/// - Email:         Yuki@qq.com
/// - Description:   音频加载器接口
/// - Creation Time: 2023年12月16日 13:21:42
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================

using UnityEngine;
using System;

namespace YukiFrameWork.Audio
{
    public interface IAudioLoader
    {
        AudioClip Clip { get; }

        AudioClip LoadClip(string path);

        void LoadClipAsync(string path, Action<AudioClip> completedLoad);

        void UnLoad();      
    }
}