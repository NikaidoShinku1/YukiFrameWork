///=====================================================
/// - FileName:      AudioPlayerRequest.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/4/16 13:57:36
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using UnityEngine;
using System.Collections;
namespace YukiFrameWork.Audio
{
    public class PlaySoundRequest : CustomYieldInstruction
    {
        public bool isDone => IsCompleted;
        
        public override bool keepWaiting => !isDone;

        public AudioPlayer Player { get; private set; }
        
        public bool IsCompleted
        {
            get => Player != null;
        }

        internal void OnCompleted(AudioPlayer player)
        {
            this.Player = player;   
        }
    }

    public static class PlaySoundRequestExtension
    {
        public static YieldTask<AudioPlayer> GetAwaiter(this PlaySoundRequest request)
        {
            var awaiter = new YieldTask<AudioPlayer>();
            YieldTaskExtension.SetRunOnUnityScheduler(awaiter, MonoHelper.Start(NextVoid()));
            IEnumerator NextVoid()
            {
                yield return request;
                awaiter.Complete(null, request.Player);
            }
            return awaiter;
        }
    }
}
