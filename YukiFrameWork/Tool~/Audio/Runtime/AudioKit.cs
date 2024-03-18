///=====================================================
/// - FileName:      AudioKit.cs
/// - NameSpace:     YukiFrameWork.Audio
/// - Created:       Yuki
/// - Email:         Yuki@qq.com
/// - Description:   声音管理套件
/// - Creation Time: 2023年12月15日 9:58:32
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================

using UnityEngine;
using System;
using System.Collections.Generic;
using YukiFrameWork.Extension;
using YukiFrameWork.Pools;

namespace YukiFrameWork.Audio
{
    [ClassAPI("声音管理套件")] 
    public class AudioKit
    {
        public static AudioSetting Setting { get; private set; } = new AudioSetting();

        public static AudioConfig Config { get; } = new AudioConfig();

        private static AudioPlayer musicPlayer;
        private static AudioPlayer voicePlayer;
       
        private static bool isInit = false;
        public static bool Init(string projectName)
        {
            if (isInit)
            {
                "声音模块已经完成初始化，无需再次调用!".LogInfo();
                return false;
            }          
            isInit = true;
            Config.LoaderPools = new AudioLoaderPools(new ABManagerAudioLoaderPools(projectName));
            return isInit;         
        }

        public static bool Init(IAudioLoaderPools loaderPools)
        {
            if (isInit)
            {
                "声音模块已经完成初始化，无需再次调用!".LogInfo();
                return false;
            }
            isInit = true;
            Config.LoaderPools = new AudioLoaderPools(loaderPools);
            return isInit;
        }

        /// <summary>
        /// 当前正在活动的所有声音
        /// </summary>
        private static Dictionary<string, List<AudioPlayer>> soundActivities = DictionaryPools<string, List<AudioPlayer>>.Get();

        private static Dictionary<string, IAudioLoader> audioLoaderDict = DictionaryPools<string, IAudioLoader>.Get();

        [MethodAPI("播放音乐，适用于背景等")]
        public static void PlayMusic(string name, bool loop = true, Action<float> onStartCallback = null, Action<float> onEndCallback = null,bool isRealTime = false)
        {
            if (!Setting.IsMusicOn.Value) return;
            var audioMgr = AudioManager.Instance;
            var loader = GetAudioLoader(name);
            AudioClip clip = loader.Clip != null ? loader.Clip : loader.LoadClip(name);          
            PlayMusicExecute(audioMgr,clip, loop, onStartCallback, onEndCallback, isRealTime);
        }

        public static void PlayMusic(AudioInfo audioInfo)
        {
            if (!Setting.IsMusicOn.Value) return;
            var audioMgr = AudioManager.Instance;
            AudioClip clip = audioInfo.Clip;

            if (clip == null)
                LogKit.Exception("AudioInfo没有正确添加音频资源，请检查!");

            PlayMusicExecute(audioMgr, clip
                , audioInfo.Loop
                ,value => audioInfo.onStartCallBack?.Invoke(value)
                ,value => audioInfo.onEndCallBack?.Invoke(value)
                ,audioInfo.IsRealTime);

        }
        [MethodAPI("(异步)播放音乐，适用于背景等")]
        public static void PlayMusicAsync(string name, bool loop = true, Action<float> onStartCallback = null, Action<float> onEndCallback = null, bool isRealTime = false)
        {
            if (!Setting.IsMusicOn.Value) return;
            var audioMgr = AudioManager.Instance;
            var loader = GetAudioLoader(name);

            if (loader.Clip != null)
                PlayMusicExecute(audioMgr, loader.Clip, loop, onStartCallback, onEndCallback, isRealTime);
            else
                loader.LoadClipAsync(name, clip =>
                {
                    PlayMusicExecute(audioMgr, clip, loop, onStartCallback, onEndCallback, isRealTime);
                });
        }

        private static void PlayMusicExecute(AudioManager audioMgr, AudioClip clip, bool loop = true, Action<float> onStartCallback = null, Action<float> onEndCallback = null, bool isRealTime = false)
        {
            void SetAudioVolume(float value) => musicPlayer.Volume = value;
          
            if (musicPlayer == null)
            {
                musicPlayer = audioMgr.MusicPlayer;
                Setting.IsMusicOn.Register(value =>
                {
                    musicPlayer.Mute = !value;
                }).UnRegisterWaitGameObjectDestroy(audioMgr);
            }
            onStartCallback += _ =>
            {
                Setting.MusicVolume
                .UnRegister(SetAudioVolume);
                 Setting.MusicVolume
                .RegisterWithInitValue(SetAudioVolume)
                .UnRegisterWaitGameObjectDestroy(audioMgr);
            };
            musicPlayer.SetAudio(audioMgr.transform, clip, loop, onStartCallback, onEndCallback, isRealTime);
        }

        [MethodAPI("播放人声，与背景音乐一致有单独的层，一般只适用于一个语音播放")]
        public static void PlayVoice(string name,bool loop = false, Action<float> onStartCallback = null, Action<float> onEndCallback = null, bool isRealTime = false)
        {
            if (!Setting.IsVoiceOn.Value) return;
            var audioMgr = AudioManager.Instance;
            var loader = GetAudioLoader(name);
            AudioClip clip = loader.Clip != null ? loader.Clip : loader.LoadClip(name);
            PlayVoiceExecute(audioMgr,clip, loop, onStartCallback, onEndCallback, isRealTime);
         
        }

        public static void PlayVoice(AudioInfo audioInfo)
        {
            if (!Setting.IsMusicOn.Value) return;
            var audioMgr = AudioManager.Instance;
            AudioClip clip = audioInfo.Clip;

            if (clip == null)
                LogKit.Exception("AudioInfo没有正确添加音频资源，请检查!");

            PlayVoiceExecute(audioMgr, clip
                , audioInfo.Loop
                , value => audioInfo.onStartCallBack?.Invoke(value)
                , value => audioInfo.onEndCallBack?.Invoke(value)
                , audioInfo.IsRealTime);

        }

        [MethodAPI("(异步)播放人声，与背景音乐一致有单独的层，一般只适用于一个语音播放")]
        public static void PlayVoiceAsync(string name, bool loop = false, Action<float> onStartCallback = null, Action<float> onEndCallback = null, bool isRealTime = false)
        {
            if (!Setting.IsVoiceOn.Value) return;
            var audioMgr = AudioManager.Instance;
            var loader = GetAudioLoader(name);
            if (loader.Clip != null)
                PlayVoiceExecute(audioMgr, loader.Clip, loop, onStartCallback, onEndCallback, isRealTime);
            else
                loader.LoadClipAsync(name, clip => PlayVoiceExecute(audioMgr,clip, loop, onStartCallback, onEndCallback, isRealTime));
        }

        private static void PlayVoiceExecute(AudioManager audioMgr, AudioClip clip, bool loop = false, Action<float> onStartCallback = null, Action<float> onEndCallback = null, bool isRealTime = false)
        {           
            void SetAudioVolume(float value)
            {
                voicePlayer.Volume = value;
            }
            if (voicePlayer == null)
            {
                voicePlayer = audioMgr.VoicePlayer;

                Setting.IsVoiceOn.Register(value =>
                {
                    voicePlayer.Mute = !value;
                }).UnRegisterWaitGameObjectDestroy(audioMgr);
            }

            onStartCallback += _ =>
            {
                //先注销一次这个事件，避免重复添加
                Setting.VoiceVolume
                .UnRegister(SetAudioVolume);
                Setting.VoiceVolume
                .RegisterWithInitValue(SetAudioVolume)
                .UnRegisterWaitGameObjectDestroy(audioMgr);
            };

            voicePlayer.SetAudio(audioMgr.transform, clip, loop, onStartCallback, onEndCallback, isRealTime);
        }
        
        [MethodAPI("播放声音、特效等，可以用于在多人说话的时候使用,可以传递自定义的父节点挂载AudioSource")]        
        public static void PlaySound(string name,bool loop = false,Transform parent = null, Action<float> onStartCallback = null, Action<float> onEndCallback = null,bool isRealTime = false)
        {
            if (!Setting.IsSoundOn.Value) return;
            var audioMgr = AudioManager.Instance;
            var loader = GetAudioLoader(name);

            AudioClip clip = loader.Clip != null ? loader.Clip : loader.LoadClip(name);

            PlaySoundExecute(audioMgr,clip, loop, parent,onStartCallback, onEndCallback, isRealTime);
        }

        public static void PlaySound(AudioInfo audioInfo)
        {
            if (!Setting.IsMusicOn.Value) return;
            var audioMgr = AudioManager.Instance;
            AudioClip clip = audioInfo.Clip;

            if (clip == null)
                LogKit.Exception("AudioInfo没有正确添加音频资源，请检查!");

            PlaySoundExecute(audioMgr, clip
                , audioInfo.Loop
                , audioInfo.transform,value => audioInfo.onStartCallBack?.Invoke(value)
                , value => audioInfo.onEndCallBack?.Invoke(value)
                , audioInfo.IsRealTime);

        }

        [MethodAPI("(异步)播放声音、特效等，可以用于在多人说话的时候使用,可以传递自定义的父节点挂载AudioSource")]
        public static void PlaySoundAsync(string name, bool loop = false, Transform parent = null, Action<float> onStartCallback = null, Action<float> onEndCallback = null, bool isRealTime = false)
        {
            if (!Setting.IsSoundOn.Value) return;
            var audioMgr = AudioManager.Instance;
            var loader = GetAudioLoader(name);
            if (loader.Clip != null)
                PlaySoundExecute(audioMgr, loader.Clip, loop, parent,onStartCallback, onEndCallback, isRealTime);
            else
                loader.LoadClipAsync(name, clip => PlaySoundExecute(audioMgr, clip, loop, parent,onStartCallback, onEndCallback, isRealTime));
        }

        private static void PlaySoundExecute(AudioManager audioMgr,AudioClip clip, bool loop = false, Transform parent = null, Action<float> onStartCallback = null, Action<float> onEndCallback = null, bool isRealTime = false)
        {            
            var soundPlayer = SoundActivitiesExist(clip.name);
            //防止注册的事件重复，注册前注销一次
            Setting.IsSoundOn.UnRegister(SetAllSoundMute);
            Setting.IsSoundOn.Register(SetAllSoundMute).UnRegisterWaitGameObjectDestroy(audioMgr);

            void SetAudioVolume(float value) => soundPlayer.Volume = value;

            if (soundPlayer == null)
            {
                soundPlayer = audioMgr.GetAudio();
                soundActivities[clip.name].Add(soundPlayer);
            }
            onStartCallback += _ =>
            {
                Setting.SoundVolume
                .RegisterWithInitValue(SetAudioVolume)
                .UnRegisterWaitGameObjectDestroy(audioMgr);
            };
            soundPlayer.SetAudio(parent == null ? audioMgr.transform : parent, clip, loop, onStartCallback, onEndCallback, isRealTime);
        }

        private static AudioPlayer SoundActivitiesExist(string name)
        {
            if(!soundActivities.TryGetValue(name,out var players))
            {
                players = ListPools<AudioPlayer>.Get();
                soundActivities.Add(name, players);
            }

            //是否有正在空闲的player
            var player = players.Count > 0 ? players.Find(x => x.IsAudioFree) : null;
            return player;
        }

        private static IAudioLoader GetAudioLoader(string name)
        {
            if (!audioLoaderDict.TryGetValue(name, out var loader))
            {
                if (AudioKit.Config.LoaderPools == null)
                {
                    throw new System.NullReferenceException("加载没有对AudioKit进行初始化！请调用一次AudioKit.Init()方法才可以获取Loader！Config.LoaderPools is Null!");
                }
                loader = Config.LoaderPools.Get();
                audioLoaderDict.Add(name, loader);
            }
            return loader;
        }

        public static void PauseMusic()
        {
            musicPlayer?.Pause();
        }

        public static void ResumeMusic()
        {
            musicPlayer?.Resume();
        }

        public static void StopMusic()
        {
            musicPlayer?.Stop();      
        }

        public static void PauseVoice()
        {
            voicePlayer?.Pause();
        }

        public static void ResumeVoice()
        {
            voicePlayer?.Resume();
        }

        public static void StopVoice()
        {
            voicePlayer?.Stop();
        }

        public static void StopAllSound()
        {
            foreach (var players in soundActivities.Values)
            {
                for (int i = 0; i < players.Count; i++)
                {
                    players[i].Stop();
                }
            }
        }

        public static void SetAllSoundMute(bool mute)
        {
            foreach (var players in soundActivities.Values)
            {
                for (int i = 0; i < players.Count; i++)
                {
                    players[i].Mute = !mute;
                }
            }
        }

        public static void Release()
        {
            Setting = null;
            voicePlayer = null;
            musicPlayer = null;

            foreach (var sounds in soundActivities.Values)
            {
                sounds.Clear();
            }

            soundActivities.Release();

            foreach (var loader in audioLoaderDict.Values)
            {
                loader.UnLoad();
            }

            audioLoaderDict.Release();
        }
    }
}