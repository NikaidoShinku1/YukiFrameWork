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
        public static IAudioSetting Setting { get; set; } = new AudioSetting();

        public static AudioConfig Config { get; } = new AudioConfig();

        public const int UNLOAD_CACHESECOUND = 5 * 60;

        public const int DETECTION_INTERVAL = 60;

        private static AudioPlayer musicPlayer;
        private static AudioPlayer voicePlayer;

        public static bool IsMusicFree => musicPlayer.IsAudioFree;
        public static bool IsVoiceFree => voicePlayer.IsAudioFree;
        public static bool IsSoundFree(string name, bool IsFirstOrDefault = false)
        {
            if (!soundActivities.TryGetValue(name, out var players))
                return true;

            if (IsFirstOrDefault)
            {
                var player = players.FirstOrDefault(x => x.IsAudioFree);
                if (player == null)
                    return true;
                return player.IsAudioFree;
            }

            foreach (var player in players)
            {
                if (player.IsAudioFree)
                    return player.IsAudioFree;
            }

            return true;
        }
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

        /// <summary>
        /// 活动中的音频加载器
        /// </summary>
        internal static Dictionary<string, IAudioLoader> audioLoaderDict = DictionaryPools<string, IAudioLoader>.Get();

        [MethodAPI("播放音乐，适用于背景等")]
        public static void PlayMusic(string name, bool loop = true, Action<float> onStartCallback = null, Action<float> onEndCallback = null,bool isRealTime = false,AudioSourceSoundSetting setting = default)
        {
            if (name.IsNullOrEmpty()) return;
            //if (!Setting.IsMusicOn.Value) return;
            var audioMgr = AudioManager.Instance;
            var loader = GetOrAddAudioLoader(name);
            AudioClip clip = loader.Clip != null ? loader.Clip : loader.LoadClip(name);          
            PlayMusicExecute(loader,audioMgr,clip, loop, onStartCallback, onEndCallback, isRealTime,setting);
        }

        public static void PlayMusic(AudioClip clip, bool loop = true, Action<float> onStartCallback = null, Action<float> onEndCallback = null, bool isRealTime = false,AudioSourceSoundSetting setting = default)
        {
            if (!clip) return;
            //if (!Setting.IsMusicOn.Value) return;
            var audioMgr = AudioManager.Instance;         
            PlayMusicExecute(null, audioMgr, clip, loop, onStartCallback, onEndCallback, isRealTime, setting);
        }

        public static void PlayMusic(AudioInfo audioInfo)
        {
            //if (!Setting.IsMusicOn.Value) return;
            var audioMgr = AudioManager.Instance;
            AudioClip clip = audioInfo.Clip;            
            if (clip == null)
                throw new Exception("AudioInfo没有正确添加音频资源，请检查!");

            PlayMusicExecute(null,audioMgr, clip
                , audioInfo.Loop
                ,value => audioInfo.onStartCallBack?.Invoke(value)
                ,value => audioInfo.onEndCallBack?.Invoke(value)
                ,audioInfo.IsRealTime,audioInfo.SoundSetting);

        }
        [MethodAPI("(异步)播放音乐，适用于背景等")]
        public static void PlayMusicAsync(string name, bool loop = true, Action<float> onStartCallback = null, Action<float> onEndCallback = null, bool isRealTime = false,AudioSourceSoundSetting setting = default)
        {
            if (name.IsNullOrEmpty()) return;
            //if (!Setting.IsMusicOn.Value) return;
            var audioMgr = AudioManager.Instance;
            var loader = GetOrAddAudioLoader(name);

            if (loader.Clip != null)
                PlayMusicExecute(loader,audioMgr, loader.Clip, loop, onStartCallback, onEndCallback, isRealTime,setting);
            else
                loader.LoadClipAsync(name, clip =>
                {
                    PlayMusicExecute(loader, audioMgr, clip, loop, onStartCallback, onEndCallback, isRealTime,setting);
                });
        }

        private static void PlayMusicExecute(IAudioLoader loader,AudioManager audioMgr, AudioClip clip, bool loop = true, Action<float> onStartCallback = null, Action<float> onEndCallback = null, bool isRealTime = false,AudioSourceSoundSetting setting = default)
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
            AudioManager.I.CheckLoaderCache(loader);
            musicPlayer.SetAudio(audioMgr.transform, clip, loop, onStartCallback, onEndCallback, isRealTime, loader, setting);        
            musicPlayer.Mute = !Setting.IsMusicOn.Value;
        }

        [MethodAPI("播放人声，与背景音乐一致有单独的层，一般只适用于一个语音播放")]
        public static void PlayVoice(string name,bool loop = false, Action<float> onStartCallback = null, Action<float> onEndCallback = null, bool isRealTime = false,AudioSourceSoundSetting setting = default)
        {
            if (name.IsNullOrEmpty()) return;
            //if (!Setting.IsVoiceOn.Value) return;
            var audioMgr = AudioManager.Instance;
            var loader = GetOrAddAudioLoader(name);
            AudioClip clip = loader.Clip != null ? loader.Clip : loader.LoadClip(name);
            PlayVoiceExecute(loader, audioMgr,clip, loop, onStartCallback, onEndCallback, isRealTime,setting);
         
        }

        public static void PlayVoice(AudioClip clip, bool loop = true, Action<float> onStartCallback = null, Action<float> onEndCallback = null, bool isRealTime = false,AudioSourceSoundSetting setting = default)
        {
            if (!clip) return;
            //if (!Setting.IsMusicOn.Value) return;
            var audioMgr = AudioManager.Instance;
            PlayVoiceExecute(null, audioMgr, clip, loop, onStartCallback, onEndCallback, isRealTime, setting);
        }

        public static void PlayVoice(AudioInfo audioInfo)
        {
            //if (!Setting.IsMusicOn.Value) return;
            var audioMgr = AudioManager.Instance;
            AudioClip clip = audioInfo.Clip;

            if (clip == null)
                throw new Exception("AudioInfo没有正确添加音频资源，请检查!");

            PlayVoiceExecute(null,audioMgr, clip
                , audioInfo.Loop
                , value => audioInfo.onStartCallBack?.Invoke(value)
                , value => audioInfo.onEndCallBack?.Invoke(value)
                , audioInfo.IsRealTime,audioInfo.SoundSetting);

        }

        [MethodAPI("(异步)播放人声，与背景音乐一致有单独的层，一般只适用于一个语音播放")]
        public static void PlayVoiceAsync(string name, bool loop = false, Action<float> onStartCallback = null, Action<float> onEndCallback = null, bool isRealTime = false,AudioSourceSoundSetting setting = default)
        {
            if (name.IsNullOrEmpty()) return;
            //if (!Setting.IsVoiceOn.Value) return;
            var audioMgr = AudioManager.Instance;
            var loader = GetOrAddAudioLoader(name);          
            if (loader.Clip != null)
                PlayVoiceExecute(loader, audioMgr, loader.Clip, loop, onStartCallback, onEndCallback, isRealTime,setting);
            else
                loader.LoadClipAsync(name, clip => PlayVoiceExecute(loader,audioMgr,clip, loop, onStartCallback, onEndCallback, isRealTime, setting));
        }

        private static void PlayVoiceExecute(IAudioLoader loader,AudioManager audioMgr, AudioClip clip, bool loop = false, Action<float> onStartCallback = null, Action<float> onEndCallback = null, bool isRealTime = false,AudioSourceSoundSetting setting = default)
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
            AudioManager.I.CheckLoaderCache(loader);
            voicePlayer.SetAudio(audioMgr.transform, clip, loop, onStartCallback, onEndCallback, isRealTime,loader,setting);
            voicePlayer.Mute = !Setting.IsVoiceOn.Value;
        }

        /// <summary>
        /// 声音播放的模式
        /// </summary>
        public static PlaySoundModes PlaySoundMode { get; set; } = PlaySoundModes.EveryOne;

        /// <summary>
        /// 声音播放间隔帧数设置(默认为10)
        /// </summary>
        public static int SoundFrameCountForIgnoreSameSound { get; set; } = 10;      

        private static Dictionary<string, int> mSoundFrameCountForName = new Dictionary<string, int>();
        private static int mGlobalFrameCount = 0;

        public enum PlaySoundModes
        {
            EveryOne,           
            IgnoreSameSoundInSoundFrames
        }

        [MethodAPI("播放声音、特效等，可以用于在多人说话的时候使用,可以传递自定义的父节点挂载AudioSource")]        
        public static AudioPlayer PlaySound(string name,bool loop = false,Transform parent = null, Action<float> onStartCallback = null, Action<float> onEndCallback = null,bool isRealTime = false,AudioSourceSoundSetting setting = default)
        {
            if (name.IsNullOrEmpty()) return null;
            //if (!Setting.IsSoundOn.Value) return null;
            if (!CheckPlaySound(name)) return null;
            var audioMgr = AudioManager.Instance;
            var loader = GetOrAddAudioLoader(name);

            AudioClip clip = loader.Clip != null ? loader.Clip : loader.LoadClip(name);

            return PlaySoundExecute(loader,audioMgr,clip, loop, parent,onStartCallback, onEndCallback, isRealTime,setting);
        }

        public static void PlaySound(AudioClip clip, bool loop = true, Action<float> onStartCallback = null, Action<float> onEndCallback = null, bool isRealTime = false,AudioSourceSoundSetting setting = default)
        {
            if (!clip) return;
            //if (!Setting.IsMusicOn.Value) return;
            var audioMgr = AudioManager.Instance;
            PlaySoundExecute(null, audioMgr, clip, loop, null, onStartCallback,onEndCallback, isRealTime,setting);
        }

        public static AudioPlayer PlaySound(AudioInfo audioInfo)
        {
            audioInfo.currentClipName = string.Empty;
            //if (!Setting.IsMusicOn.Value) return null;
            AudioClip clip = audioInfo.Clip;

            if (clip == null)
                throw new Exception("AudioInfo没有正确添加音频资源，请检查!");
            if (!CheckPlaySound(clip.name)) return null;
            audioInfo.currentClipName = clip.name;
            var audioMgr = AudioManager.Instance;          

            return PlaySoundExecute(null,audioMgr, clip
                , audioInfo.Loop
                , audioInfo.position == AudioInfo.Position.IgnorePosition ? audioInfo.transform : null,value => audioInfo.onStartCallBack?.Invoke(value)
                , value => audioInfo.onEndCallBack?.Invoke(value)
                , audioInfo.IsRealTime
                ,audioInfo.SoundSetting);

        }

        [MethodAPI("(异步)播放声音、特效等，可以用于在多人说话的时候使用,可以传递自定义的父节点挂载AudioSource")]
        public static PlaySoundRequest PlaySoundAsync(string name, bool loop = false, Transform parent = null, Action<float> onStartCallback = null, Action<float> onEndCallback = null, bool isRealTime = false,AudioSourceSoundSetting setting = default)
        {
            if (name.IsNullOrEmpty()) return null;
            //if (!Setting.IsSoundOn.Value) return null;
            if (!CheckPlaySound(name)) return null;
            var audioMgr = AudioManager.Instance;
            var loader = GetOrAddAudioLoader(name);
            PlaySoundRequest request = new PlaySoundRequest();
            if (loader.Clip != null)
            {
                AudioPlayer player = PlaySoundExecute(loader, audioMgr, loader.Clip, loop, parent, onStartCallback, onEndCallback, isRealTime,setting);
                request.OnCompleted(player);
            }
            else
                loader.LoadClipAsync(name, clip =>
                {          
                    AudioPlayer player = PlaySoundExecute(loader, audioMgr, clip, loop, parent, onStartCallback, onEndCallback, isRealTime,setting);
                    request.OnCompleted(player);
                });

            return request;
        }

        private static AudioPlayer PlaySoundExecute(IAudioLoader loader,AudioManager audioMgr,AudioClip clip, bool loop = false, Transform parent = null, Action<float> onStartCallback = null, Action<float> onEndCallback = null, bool isRealTime = false,AudioSourceSoundSetting setting = default)
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
            AudioManager.I.CheckLoaderCache(loader);
            soundPlayer.SetAudio(parent == null ? audioMgr.transform : parent, clip, loop, onStartCallback, onEndCallback, isRealTime,loader, setting);
            SetAllSoundMute();
            return soundPlayer;
        }

        static async void SetAllSoundMute()
        {
            await CoroutineTool.WaitForEndOfFrame();
            SetAllSoundMute(Setting.IsSoundOn.Value);
        }

        private static bool CheckPlaySound(string name)
        {
            if (PlaySoundMode == PlaySoundModes.EveryOne)
                return true;

            if (Time.frameCount - mGlobalFrameCount <= SoundFrameCountForIgnoreSameSound)
            {
                if (mSoundFrameCountForName.ContainsKey(name))
                    return false;

                mSoundFrameCountForName.Add(name, 0);
            }
            else
            {
                mGlobalFrameCount = Time.frameCount;
                mSoundFrameCountForName.Clear();
                mSoundFrameCountForName.Add(name, 0);
            }

            return true;
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

        public static void PauseAllSound()
        {
            foreach (var players in soundActivities.Values)
            {
                for (int i = 0; i < players.Count; i++)
                {
                    players[i].Pause();
                }
            }
        }

        public static void ResumeAllSound()
        {
            foreach (var players in soundActivities.Values)
            {
                for (int i = 0; i < players.Count; i++)
                {
                    players[i].Resume();
                }
            }
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

        public static void StopSound(string name)
        {
            if (name.IsNullOrEmpty()) return;
            try
            {
                foreach (var players in soundActivities.Values)
                {
                    for (int i = 0; i < players.Count; i++)
                    {
                        if (players[i].ClipName == name)
                            players[i].Stop();
                    }
                }
            }
            catch 
            { }
        }

        public static void PauseSound(string name)
        {
            if (name.IsNullOrEmpty()) return;
            try
            {
                foreach (var players in soundActivities.Values)
                {
                    for (int i = 0; i < players.Count; i++)
                    {
                        if (players[i].ClipName == name)
                            players[i].Pause();
                    }
                }
            }
            catch
            { }
        }

        public static void ResumeSound(string name)
        {
            if (name.IsNullOrEmpty()) return;
            try
            {
                foreach (var players in soundActivities.Values)
                {
                    for (int i = 0; i < players.Count; i++)
                    {
                        if (players[i].ClipName == name)
                            players[i].Resume();
                    }
                }
            }
            catch
            { }
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
            voicePlayer?.Cancel();
            voicePlayer = null;
            musicPlayer?.Cancel();
            musicPlayer = null;

            foreach (var sounds in soundActivities.Values)
            {
                foreach (var sound in sounds)
                {
                    sound.Cancel();
                    AudioManager.Instance.Release(sound);
                }
                sounds.Clear();
            }

            soundActivities.Release();

            foreach (var loader in audioLoaderDict.Values)
            {
                Config.LoaderPools.Release(loader);
            }

            audioLoaderDict.Release();
            isInit = false;
        }
    }
}