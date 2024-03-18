///=====================================================
/// - FileName:      AudioManager.cs
/// - NameSpace:     YukiFrameWork.Audio
/// - Created:       Yuki
/// - Email:         Yuki@qq.com
/// - Description:   这是一个框架工具创建的Mono脚本
/// - Creation Time: 2023年12月15日 10:13:46
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================

using UnityEngine;
using YukiFrameWork.Pools;

namespace YukiFrameWork.Audio
{
    public class AudioManager : MonoBehaviour,ISingletonKit
    {
        public static AudioManager Instance
            => Singleton.GetMonoInstance<AudioManager>(true);

        private AudioListener audioListener;

        public AudioPlayer MusicPlayer { get; private set; }
        public AudioPlayer VoicePlayer { get; private set; }

        private SimpleObjectPools<AudioPlayer> audioPools;

        public void OnInit()
        {          
            MusicPlayer = new AudioPlayer();
            VoicePlayer = new AudioPlayer();
            audioPools = new SimpleObjectPools<AudioPlayer>(() => new AudioPlayer(),10);
            
            CheckAudioListener();
        }

        private void CheckAudioListener()
        {
            if (audioListener == null)
                audioListener = FindObjectOfType<AudioListener>();
            if (audioListener == null)
                audioListener = gameObject.AddComponent<AudioListener>();
        }

        public AudioPlayer GetAudio() => audioPools.Get();

        public void Release(AudioPlayer audioPlayer) => audioPools.Release(audioPlayer);

        public void OnDestroy()
        {
            MusicPlayer = null;
            VoicePlayer = null;
            audioListener = null;
            AudioKit.Release();
        }

    }
}