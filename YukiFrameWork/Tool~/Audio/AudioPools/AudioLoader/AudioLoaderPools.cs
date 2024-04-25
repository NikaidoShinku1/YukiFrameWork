///=====================================================
/// - FileName:      AudioLoaderPools.cs
/// - NameSpace:     YukiFrameWork.Project
/// - Created:       Yuki
/// - Email:         Yuki@qq.com
/// - Description:   这是一个框架工具创建的脚本
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================

using YukiFrameWork.Pools;
using System;
namespace YukiFrameWork.Audio
{
    public class AudioLoaderPools : AbstarctPools<IAudioLoader>
    {
        public AudioLoaderPools(IAudioLoaderPools pools)
        {
            SetFectoryPool(() => pools.CreateAudioLoader());

            maxSize = 200;
            for (int i = 0; i < 10; i++)
            {
                var loader = fectoryPool.Create();

                Release(loader);
            }
        }             
        public override bool Release(IAudioLoader obj)
        {
            obj.UnLoad();
            if (cacheQueue.Count < maxSize)
            {
                cacheQueue.Enqueue(obj);
                return true;
            }
            return false;
        }

       
    }
}