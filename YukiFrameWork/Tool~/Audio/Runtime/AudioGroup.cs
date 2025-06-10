///=====================================================
/// - FileName:      AudioGroup.cs
/// - NameSpace:     YukiFrameWork.Audio
/// - Description:   高级定制脚本生成
/// - Creation Time: 2025/6/9 14:23:20
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using System.Collections.Generic;
using RuntimeAudioGroups = System.Collections.Generic.Dictionary<YukiFrameWork.Audio.AudioPlayType, System.Collections.Generic.Dictionary<string, YukiFrameWork.Audio.AudioGroup>>;
using YukiFrameWork.Pools;
using System.Collections;
using System.Linq;
namespace YukiFrameWork.Audio
{
    public enum AudioPlayType
    {
        Music = 0,
        Voice,
        Sound
    }
    public class AudioGroup : IDisposable
    {
        public string GroupName { get; private set; }        
        public IAudioGroupSetting Setting { get; set; }
        public AudioPlayType AudioPlayType { get; private set; }

        
        internal static RuntimeAudioGroups runtimeAudioGroups = new RuntimeAudioGroups();

        static AudioGroup()
        {
            for (AudioPlayType audioPlayType = AudioPlayType.Music; audioPlayType <= AudioPlayType.Sound; audioPlayType++)            
                runtimeAudioGroups.Add(audioPlayType, new Dictionary<string, AudioGroup>());          
        }

        internal static void Release()
        {
            foreach(var item in runtimeAudioGroups)
            {
                foreach (var group in item.Value)
                {
                    group.Value.Dispose();
                }
                item.Value.Clear();
            }

            
        }      
        internal static AudioGroup GetOrAddAudioGroup(AudioPlayType audioPlayType,string name)
        {
            if (!runtimeAudioGroups.ContainsKey(audioPlayType))
                throw new NullReferenceException($"初始化AudioKit失败,无法访问音频组类型:{audioPlayType}，请检查是否调用AudioKit.Init进行对音频管理套件的初始化!");

            var dict = runtimeAudioGroups[audioPlayType];

            if (dict.TryGetValue(name, out var group))
                return group;
            group = new AudioGroup() { GroupName = name };
            var Setting = new DefaultAudioGroupSetting();
            group.AudioPlayType = audioPlayType;
            Setting.Create(group);
            if (audioPlayType != AudioPlayType.Sound)
                group.audioPlayer = new AudioPlayer();
            group.Setting = Setting;
            
            dict.Add(name, group);
            runtimeAudioGroups[audioPlayType] = dict;
            return group;
        }

        private AudioPlayer audioPlayer;
        private Dictionary<string, List<AudioPlayer>> soundActivities = new Dictionary<string, List<AudioPlayer>>();

        /// <summary>
        /// 这个音频分组是否是空闲的
        /// </summary>
        public bool IsAudioGroupFree
        {
            get => AudioPlayType switch
            {
                AudioPlayType.Music => IsMusicFree,
                AudioPlayType.Voice => IsVoiceFree,
                AudioPlayType.Sound => IsSoundFree,
                _ => true,
            };
        }
        private bool IsMusicFree => audioPlayer.IsAudioFree;
        private bool IsVoiceFree => audioPlayer.IsAudioFree;
        private bool IsSoundFree 
        {
            get
            {
                foreach (var players in soundActivities.Values)
                {                    
                    foreach (var player in players)
                    {
                        if (!player.IsAudioFree)
                            return player.IsAudioFree;
                    }
                }
               
                return true;
            }
        }
        /// <summary>
        /// 播放音频
        /// </summary>
        /// <param name="name">音频名称/路径</param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        public AudioPlayer Play(string name)
        {
            if (name.IsNullOrEmpty()) return null;
            if (!CheckPlaySound(name)) return null;
            if (!groupInfo.parent)
                groupInfo.parent = AudioManager.Instance.transform;
            IAudioLoader audioLoader = AudioKit.GetOrAddAudioLoader(name);
            if (audioLoader == null) throw new NullReferenceException("丢失加载器，请检查AudioClip是否可以正确加载 name:" + name);
            return PlayInternal(name, (audioLoader.Clip != null ? audioLoader.Clip : audioLoader.LoadClip(name)),audioLoader);
        }
        /// <summary>
        /// 传递AudioClip播放音频
        /// </summary>
        /// <param name="clip"></param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        public AudioPlayer Play(AudioClip clip)
        {
            if (clip == null)
                throw new NullReferenceException("丢失音频无法播放");
            if (!CheckPlaySound(clip.name)) return null;
            return PlayInternal(clip.name, clip, null);
        }
        /// <summary>
        /// 异步播放音频
        /// </summary>
        /// <param name="name"></param>
        /// <param name="callBack"></param>
        /// <exception cref="NullReferenceException"></exception>
        public void PlayAsync(string name, Action<AudioPlayer> callBack)
        {
            if (name.IsNullOrEmpty()) return;
            if (!CheckPlaySound(name)) return;
            if (!groupInfo.parent)
                groupInfo.parent = AudioManager.Instance.transform;
            IAudioLoader audioLoader = AudioKit.GetOrAddAudioLoader(name);
            if (audioLoader == null) throw new NullReferenceException("丢失加载器，请检查AudioClip是否可以正确加载 name:" + name);
            if (audioLoader.Clip != null)
            {
                var audioPlayer = PlayInternal(name, audioLoader.Clip, audioLoader);
                callBack?.Invoke(audioPlayer);
            }
            else
                audioLoader.LoadClipAsync(name, clip =>
                {
                   var audioPlayer = PlayInternal(name, clip, audioLoader);
                    callBack?.Invoke(audioPlayer);
                });
        }
        /// <summary>
        /// 异步播放音频
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IEnumerator PlayAsync(string name)
        {
            bool isCompleted = false;
            PlayAsync(name, _ => isCompleted = true);
            yield return CoroutineTool.WaitUntil(() => isCompleted);
        }
      
        /// <summary>
        /// 该音频播放开启循环
        /// </summary>
        /// <returns></returns>
        public AudioGroup Loop()
        {
            groupInfo.loop = true;
            return this;
        }

        /// <summary>
        /// 开始播放的事件注册
        /// </summary>
        /// <param name="onStartCallBack"></param>
        /// <returns></returns>
        public AudioGroup OnStartCallBack(Action<float> onStartCallBack)
        {
            groupInfo.onStartCallBack += onStartCallBack;
            return this;
        }

        /// <summary>
        /// 结束播放的事件注册,如果音频被中断,或者释放则不会执行，必须是正常播放结束
        /// </summary>
        /// <param name="onEndCallBack"></param>
        /// <returns></returns>
        public AudioGroup OnEndCallBack(Action<float> onEndCallBack)
        {
            groupInfo.onEndCallBack += onEndCallBack;
            return this;
        }

        /// <summary>
        /// 传递AudioSource的根节点，不设置时默认由AudioManager托管
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        public AudioGroup Parent(Transform parent)
        {
            groupInfo.parent = parent;
            return this;
        }
        /// <summary>
        /// 是否不受Time.TimeScale影响，当调用后，TimeScale为0时Clip Time仍会进行累加，正常播放到结束
        /// <para>适用于非循环音频</para>
        /// </summary>
        /// <returns></returns>
        public AudioGroup OnRealTime()
        {
            groupInfo.isRealTime = true;
            return this;
        }
        /// <summary>
        /// 传递AudioSourceSoundSetting设置类，以同步AudioSource的3dSoundSetting
        /// </summary>
        /// <param name="audioSourceSoundSetting"></param>
        /// <returns></returns>
        public AudioGroup AudioSource3DSetting(AudioSourceSoundSetting audioSourceSoundSetting)
        {
            groupInfo.soundSetting = audioSourceSoundSetting;
            return this;
        }

        internal AudioGroup SetAudioInfo(AudioInfo audioInfo)
        {
            if (audioInfo.Clip == null)
                throw new NullReferenceException("AudioInfo丢失音频!");

            audioInfo.currentClipName = audioInfo.Clip.name;

            if (audioInfo.position == AudioInfo.Position.IgnorePosition)
                groupInfo.parent = audioInfo.transform;
            else groupInfo.parent = AudioManager.Instance.transform;

            groupInfo.loop = audioInfo.Loop;
            groupInfo.onStartCallBack = value => audioInfo.onStartCallBack?.Invoke(value);
            groupInfo.onEndCallBack = value => audioInfo.onEndCallBack?.Invoke(value);
            groupInfo.soundSetting = audioInfo.SoundSetting;
            groupInfo.isRealTime = audioInfo.IsRealTime;
            return this;
        }

        /// <summary>
        /// 暂停音频
        /// </summary>
        public void Pause()
        {
            switch (AudioPlayType)
            {
                case AudioPlayType.Music:
                    AudioPlayer.Pause();
                    break;
                case AudioPlayType.Voice:
                    AudioPlayer.Pause();
                    break;
                case AudioPlayType.Sound:
                    foreach (var item in soundActivities.Values)
                    {
                        foreach (var sound in item)
                        {
                            sound.Pause();
                        }
                    }
                    break;
              
            }           
        }

        /// <summary>
        /// 如果该分组所属Sound层，则可以使用该重载，精确暂停播放的音频
        /// </summary>
        /// <param name="name"></param>
        /// <exception cref="InvalidCastException"></exception>
        public void Pause(string name)
        {
            if (AudioPlayType != AudioPlayType.Sound)
                throw new InvalidCastException("调用暂停方法所属的音频分组不为Sound层，无法进行精确暂停,请调用无参Pause重载!");

            foreach (var item in soundActivities.Values)
            {
                foreach (var sound in item)
                {
                    if(sound.ClipName == name)
                        sound.Pause();
                }
            }
        }

        /// <summary>
        /// 恢复音频
        /// </summary>
        public void Resume()
        {
            switch (AudioPlayType)
            {
                case AudioPlayType.Music:
                    AudioPlayer.Resume();
                    break;
                case AudioPlayType.Voice:
                    AudioPlayer.Resume();
                    break;
                case AudioPlayType.Sound:
                    foreach (var item in soundActivities.Values)
                    {
                        foreach (var sound in item)
                        {
                            sound.Resume();
                        }
                    }
                    break;

            }
        }

        /// <summary>
        /// 如果该分组所属Sound层，则可以使用该重载，精确恢复播放的音频
        /// </summary>
        /// <param name="name"></param>
        /// <exception cref="InvalidCastException"></exception>
        public void Resume(string name)
        {
            if (AudioPlayType != AudioPlayType.Sound)
                throw new InvalidCastException("调用暂停方法所属的音频分组不为Sound层，无法进行精确暂停,请调用无参Pause重载!");

            foreach (var item in soundActivities.Values)
            {
                foreach (var sound in item)
                {
                    if (sound.ClipName == name)
                        sound.Resume();
                }
            }
        }

        /// <summary>
        /// 停止音频播放
        /// </summary>
        public void Stop()
        {
            switch (AudioPlayType)
            {
                case AudioPlayType.Music:
                    AudioPlayer.Stop();
                    break;
                case AudioPlayType.Voice:
                    AudioPlayer.Stop();
                    break;
                case AudioPlayType.Sound:
                    foreach (var item in soundActivities.Values)
                    {
                        foreach (var sound in item)
                        {
                            sound.Stop();
                        }
                    }
                    break;

            }
        }

        /// <summary>
        ///  如果该分组所属Sound层，则可以使用该重载，精确停止的音频
        /// </summary>
        /// <param name="name"></param>
        /// <exception cref="InvalidCastException"></exception>
        public void Stop(string name)
        {
            if (AudioPlayType != AudioPlayType.Sound)
                throw new InvalidCastException("调用暂停方法所属的音频分组不为Sound层，无法进行精确暂停,请调用无参Pause重载!");

            foreach (var item in soundActivities.Values)
            {
                foreach (var sound in item)
                {
                    if (sound.ClipName == name)
                        sound.Stop();
                }
            }
        }

        /// <summary>
        /// 释放分组
        /// </summary>
        public void Dispose()
        {
            groupInfo.Reset();
            if (AudioPlayType == AudioPlayType.Sound)
            {
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
            }
            else
                this.AudioPlayer?.Cancel();
        }

        private static Dictionary<string, int> mSoundFrameCountForName = new Dictionary<string, int>();
        private static int mGlobalFrameCount = 0;
        private static bool CheckPlaySound(string name)
        {
            if (AudioKit.PlaySoundMode == AudioKit.PlaySoundModes.EveryOne)
                return true;

            if (Time.frameCount - mGlobalFrameCount <= AudioKit.SoundFrameCountForIgnoreSameSound)
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

        internal AudioPlayer PlayInternal(string clipNameOrPath, AudioClip audioClip, IAudioLoader audioLoader)
        {
            AudioPlayer audioPlayer = null;

            if (AudioPlayType == AudioPlayType.Sound)
            {
                audioPlayer = SoundActivitiesExist(clipNameOrPath);
                if (audioPlayer == null)
                {
                    audioPlayer = AudioManager.Instance.GetAudio();
                    soundActivities[clipNameOrPath].Add(audioPlayer);
                }
            }
            else
                audioPlayer = this.audioPlayer;
            void SetAudioVolume(float value)
            {
                audioPlayer.Volume = value;
            }

            this.Setting.IsOn.Register(value =>
            {
                audioPlayer.Mute = !value;
            }).UnRegisterWaitGameObjectDestroy(groupInfo.parent);

            groupInfo.onStartCallBack += _ =>
            {
                this.Setting.Volume.UnRegister(SetAudioVolume);
                this.Setting.Volume
                .RegisterWithInitValue(SetAudioVolume)
                .UnRegisterWaitGameObjectDestroy(groupInfo.parent);
            };
            AudioManager.Instance.CheckLoaderCache(audioLoader);
            audioPlayer.SetAudio(groupInfo.parent, audioClip, groupInfo.loop, groupInfo.onStartCallBack
             , groupInfo.onEndCallBack, groupInfo.isRealTime, audioLoader, groupInfo.soundSetting);
            audioPlayer.Mute = !this.Setting.IsOn.Value;
            //最后初始化
            ResetInfo();

            return audioPlayer;
        }

        internal AudioPlayer AudioPlayer => audioPlayer;

        internal AudioPlayer SoundActivitiesExist(string name)
        {
            if (AudioPlayType != AudioPlayType.Sound)
                return AudioPlayer;
            if (!soundActivities.TryGetValue(name, out var players))
            {
                players = ListPools<AudioPlayer>.Get();
                soundActivities.Add(name, players);
            }

            //是否有正在空闲的player
            var player = players.Count > 0 ? players.Find(x => x.IsAudioFree) : null;
            return player;
        }
        private AudioGroupInfo groupInfo = new AudioGroupInfo();
        internal void ResetInfo()
        {
            groupInfo.Reset();
        }

    }

    public interface IAudioGroupSetting
    {
        BindableProperty<float> Volume { get; }
        BindableProperty<bool> IsOn { get; }

        void Create(AudioGroup audioGroup);
    }

    public class DefaultAudioGroupSetting : IAudioGroupSetting
    {
        internal const string PLAYERPREFS_VOLUME_KEY = nameof(PLAYERPREFS_VOLUME_KEY);
       
        internal const string PLAYERPREFS_ON_KEY = nameof(PLAYERPREFS_ON_KEY);
        public BindableProperty<float> Volume { get; private set; }
        public BindableProperty<bool> IsOn { get ;private set; }
        public AudioPlayType AudioPlayType { get ;private set; }       

        public void Create(AudioGroup audioGroup)
        {
            this.AudioPlayType = audioGroup.AudioPlayType;
            Volume = new BindablePropertyPlayerPrefsByFloat( $"{audioGroup.GroupName}_{this.AudioPlayType}_{PLAYERPREFS_VOLUME_KEY}", 1);
            IsOn = new BindablePropertyPlayerPrefsByBoolan($"{audioGroup.GroupName}_{this.AudioPlayType}_{PLAYERPREFS_ON_KEY}", true);
        }
    }

    internal class AudioGroupInfo
    {
        public bool loop;
        public bool isRealTime;
        public Action<float> onStartCallBack;
        public Action<float> onEndCallBack;
        public Transform parent;      
        public AudioSourceSoundSetting soundSetting;

        public void Reset()
        {
            loop = isRealTime = false;
            onStartCallBack = null;
            onEndCallBack = null;
            parent = null;
            soundSetting = null;            
        }
    }
  
}
