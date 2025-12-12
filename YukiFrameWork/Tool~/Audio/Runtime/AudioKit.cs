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
using System.Linq;

namespace YukiFrameWork.Audio
{
    [ClassAPI("声音管理套件")] 
    public class AudioKit
    {     
        
        internal static AudioConfig Config { get; } = new AudioConfig();

        public const int UNLOAD_CACHESECOUND = 5 * 60;
        
        public const int DETECTION_INTERVAL = 60;
     
        private static bool isInit = false;
        private const string DEFAULT_MUSIC_GROUP_NAME = nameof(DEFAULT_MUSIC_GROUP_NAME);
        private const string DEFAULT_VOICE_GROUP_NAME = nameof(DEFAULT_VOICE_GROUP_NAME);
        private const string DEFAULT_SOUND_GROUP_NAME = nameof(DEFAULT_SOUND_GROUP_NAME);
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
        /// 活动中的音频加载器
        /// </summary>
        internal static Dictionary<string, IAudioLoader> audioLoaderDict = DictionaryPools<string, IAudioLoader>.Get();

        /// <summary>
        /// 获取指定组名的Music层分组
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns></returns>
        public static AudioGroup Music(string groupName)
        {
            return groupName.IsNullOrEmpty() ? Music() : AudioGroup.GetOrAddAudioGroup(AudioPlayType.Music,groupName);
        }
        /// <summary>
        /// 获取指定组名的Voice层分组
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns></returns>
        public static AudioGroup Voice(string groupName)
        {
            return groupName.IsNullOrEmpty() ? Voice() : AudioGroup.GetOrAddAudioGroup(AudioPlayType.Voice, groupName);
        }
        /// <summary>
        /// 获取指定组名的Sound层分组
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns></returns>
        public static AudioGroup Sound(string groupName)
        {
            return groupName.IsNullOrEmpty() ? Sound() : AudioGroup.GetOrAddAudioGroup(AudioPlayType.Sound, groupName);
        }

        /// <summary>
        /// 获取Music的默认组
        /// </summary>
        /// <returns></returns>
        public static AudioGroup Music()
        {
            return Music(DEFAULT_MUSIC_GROUP_NAME);
        }
        /// <summary>
        /// 获取Voice的默认组
        /// </summary>
        /// <returns></returns>
        public static AudioGroup Voice()
        {
            return Voice(DEFAULT_VOICE_GROUP_NAME);
        }
        /// <summary>
        /// 获取Sound的默认组
        /// </summary>
        /// <returns></returns>
        public static AudioGroup Sound()
        {
            return Sound(DEFAULT_SOUND_GROUP_NAME);
        }

        /// <summary>
        /// 添加或创建新的分组。
        /// </summary>
        /// <param name="audioPlayType">该音频分组所属层级</param>
        /// <param name="name">组名</param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        public static AudioGroup GetOrCreateAudioGroup(AudioPlayType audioPlayType,string groupName)
        {
            return AudioGroup.GetOrAddAudioGroup(audioPlayType, groupName);
        }
       
        /// <summary>
        /// Sound层播放的模式
        /// </summary>
        public static PlaySoundModes PlaySoundMode { get; set; } = PlaySoundModes.EveryOne;

        /// <summary>
        /// Sound层播放间隔帧数设置(默认为10)
        /// </summary>
        public static int SoundFrameCountForIgnoreSameSound { get; set; } = 10;

        /// <summary>
        /// AudioKit全层级共享音量缩放值，最终播放的音效会在设定好不同分组中内定的音量后根据缩放值计算
        /// <para>Tips:适合设计总音量时使用，默认值为1</para>
        /// </summary>
        public static BindablePropertyPlayerPrefsByFloat AudioVolumeScale { get;} = new BindablePropertyPlayerPrefsByFloat("AUDIOKIT_ALLAUDIOVOLUME_SCALE",1);

        /// <summary>
        /// Music分组的音量缩放值,默认是1
        /// <para>Music层所有的分组均共享该值，默认值为1</para>
        /// </summary>
        public static BindablePropertyPlayerPrefsByFloat MusicVolumeScale
        {
            get => AudioGroup.mAudioGroupVolumeScales[AudioPlayType.Music];
        }


        /// <summary>
        /// Voice分组的音量缩放值,默认是1
        /// <para>Voice层所有的分组均共享该值，默认值为1</para>
        /// </summary>
        public static BindablePropertyPlayerPrefsByFloat VoiceVolumeScale
        {
            get => AudioGroup.mAudioGroupVolumeScales[AudioPlayType.Voice];
        }


        /// <summary>
        /// Sound分组的音量缩放值,默认是1
        /// <para>Sound层所有的分组均共享该值，默认值为1</para>
        /// </summary>
        public static BindablePropertyPlayerPrefsByFloat SoundVolumeScale
        {
            get => AudioGroup.mAudioGroupVolumeScales[AudioPlayType.Sound];
        }

        public enum PlaySoundModes
        {
            EveryOne,           
            IgnoreSameSoundInSoundFrames
        }       

        internal static IAudioLoader GetOrAddAudioLoader(string name)
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
        [Obsolete("过时的播放音乐方法，请使用AudioKit.Music()进行对Music层的音频播放")]
        public static void PlayMusic(string name, bool loop = true, Action<float> onStartCallback = null, Action<float> onEndCallback = null, bool isRealTime = false, AudioSourceSoundSetting setting = default)
        {
            throw new Exception("PlayMusic方法已不再支持实现,请调用AudioKit.Music()方法!");
        }

        [Obsolete("过时的播放人声方法，请使用AudioKit.Voice()进行对Voice层的音频播放")]
        public static void PlayVoice(string name, bool loop = true, Action<float> onStartCallback = null, Action<float> onEndCallback = null, bool isRealTime = false, AudioSourceSoundSetting setting = default)
        {
            throw new Exception("PlayVoice方法已不再支持实现,请调用AudioKit.Voice()方法!");
        }

        [Obsolete("过时的播放音效方法，请使用AudioKit.Sound()进行对Sound层的音频播放")]
        public static void PlaySound(string name, bool loop = true, Action<float> onStartCallback = null, Action<float> onEndCallback = null, bool isRealTime = false, AudioSourceSoundSetting setting = default)
        {
            throw new Exception("PlaySound方法已不再支持实现,请调用AudioKit.Sound()方法!");
        }

        [Obsolete("过时的播放音乐方法，请使用AudioKit.Music()进行对Music层的音频播放")]
        public static void PlayMusicAsync(string name, bool loop = true, Action<float> onStartCallback = null, Action<float> onEndCallback = null, bool isRealTime = false, AudioSourceSoundSetting setting = default)
        {
            throw new Exception("PlayMusic方法已不再支持实现,请调用AudioKit.Music()方法!");
        }

        [Obsolete("过时的播放人声方法，请使用AudioKit.Voice()进行对Voice层的音频播放")]
        public static void PlayVoiceAsync(string name, bool loop = true, Action<float> onStartCallback = null, Action<float> onEndCallback = null, bool isRealTime = false, AudioSourceSoundSetting setting = default)
        {
            throw new Exception("PlayVoice方法已不再支持实现,请调用AudioKit.Voice()方法!");
        }

        [Obsolete("过时的播放音效方法，请使用AudioKit.Sound()进行对Sound层的音频播放")]
        public static void PlaySoundAsync(string name, bool loop = true, Action<float> onStartCallback = null, Action<float> onEndCallback = null, bool isRealTime = false, AudioSourceSoundSetting setting = default)
        {
            throw new Exception("PlaySound方法已不再支持实现,请调用AudioKit.Sound()方法!");
        }
        
        /// <summary>
        /// 停止所有的Music层播放
        /// </summary>
        public static void StopAllMusic()
        {
            AudioGroup.StopAll(AudioPlayType.Music);
        }

        /// <summary>
        /// 停止所有的Voice层播放
        /// </summary>
        public static void StopAllVoice()
        {
            AudioGroup.StopAll(AudioPlayType.Voice);
        }

        /// <summary>
        /// 停止所有的Sound层播放
        /// </summary>
        public static void StopAllSound()
        {
            AudioGroup.StopAll(AudioPlayType.Sound);
        }
        /// <summary>
        /// 获取已经被创建的加载器，可以手动通过加载器卸载音频，加载器名称与音频名称相同
        /// </summary>
        /// <param name="name">音频名称</param>
        /// <returns>返回一个音频加载器</returns>
        public static IAudioLoader GetAudioLoader(string name)
        {
            audioLoaderDict.TryGetValue(name, out var loader);
            return loader;
        }

        /// <summary>
        /// 释放AudioKit
        /// </summary>
        public static void Release()
        {
            AudioGroup.Release();
            foreach (var loader in audioLoaderDict.Values)
            {
                Config.LoaderPools.Release(loader);
            }

            audioLoaderDict.Release();
            isInit = false;
        }
    }
}