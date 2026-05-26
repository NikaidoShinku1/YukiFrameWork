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
using System.Threading.Tasks;
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
        internal static Dictionary<AudioPlayType, BindablePropertyPlayerPrefsByFloat> mAudioGroupVolumeScales = new Dictionary<AudioPlayType, BindablePropertyPlayerPrefsByFloat>();
        internal static void StopAll(AudioPlayType audioPlayType)
        {
            if (!runtimeAudioGroups.TryGetValue(audioPlayType, out var dicts))
            {
                return;
            }
            foreach (var item in dicts.Values)
            {
                item.Stop();
            }
        }
        static AudioGroup()
        {
            for (AudioPlayType audioPlayType = AudioPlayType.Music; audioPlayType <= AudioPlayType.Sound; audioPlayType++)
            {
                runtimeAudioGroups.Add(audioPlayType, new Dictionary<string, AudioGroup>());
                mAudioGroupVolumeScales.Add(audioPlayType, new BindablePropertyPlayerPrefsByFloat("AUDIOKIT_" + audioPlayType.ToString() + "_GROUPVOLUMESCALE_SETTING", 1));
            }

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
            group = new AudioGroup() { GroupName = name,AudioGroupVolumeScale = new BindablePropertyPlayerPrefsByFloat($"AUDIOGROUP_DEFAULT_SCALEKEY_{name}_{audioPlayType}",1) };
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

        /// <summary>
        /// 这个音频分组的音频缩放
        /// </summary>
        public BindablePropertyPlayerPrefsByFloat AudioGroupVolumeScale
        {
            get;private set;
        }

        /// <summary>
        /// 这个音频分组所绑定的音频缩放(来源AudioKit的缩放)
        /// </summary>
        public BindablePropertyPlayerPrefsByFloat BindAudioGroupVolumeScale
        {
            get => AudioPlayType switch
            {
                AudioPlayType.Music => AudioKit.MusicVolumeScale,
                AudioPlayType.Voice => AudioKit.VoiceVolumeScale,
                AudioPlayType.Sound => AudioKit.SoundVolumeScale,
                _ => throw new Exception("未知分组")
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
        //[Obsolete("方法已弃用,请通过Build构建AudioPlayer后调用Play!")]
        public AudioPlayer Play(string name)
        {
            return Build(name).Play();
        }

        /// <summary>
        /// 构建音频播放器
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        public AudioPlayer Build(string name)
        {
            if (name.IsNullOrEmpty()) return null;
            //if (!CheckPlaySound(name)) return null;
            IAudioLoader audioLoader = AudioKit.GetOrAddAudioLoader(name);
            if (audioLoader == null) throw new NullReferenceException("丢失加载器，请检查AudioClip是否可以正确加载 name:" + name);

            return FindPlayerByGroup(name).SetNameOrPath(name).Clip(audioLoader.LoadClip(name)).SetLoader(audioLoader);
        }

        public AudioPlayer Build(AudioInfo audioInfo)
        {
            return Build(audioInfo.Clip).Parent(audioInfo.position == AudioInfo.Position.IgnorePosition ? audioInfo.transform : AudioManager.Instance.transform);
        }

        public void BuildAsync(string name,Action<AudioPlayer> callBack)
        {
            if (name.IsNullOrEmpty()) return;          
            //Debug.LogError(groupInfo.parent);
            IAudioLoader audioLoader = AudioKit.GetOrAddAudioLoader(name);
            if (audioLoader == null) throw new NullReferenceException("丢失加载器，请检查AudioClip是否可以正确加载 name:" + name);
            if (audioLoader.Clip != null)
            {
                var audioPlayer = FindPlayerByGroup(name).SetNameOrPath(name).Clip(audioLoader.Clip).SetLoader(audioLoader);
                callBack?.Invoke(audioPlayer);
            }
            else
                audioLoader.LoadClipAsync(name, clip =>
                {
                    var audioPlayer = FindPlayerByGroup(name).SetNameOrPath(name).Clip(clip).SetLoader(audioLoader);
                    callBack?.Invoke(audioPlayer);
                });
        }

        /// <summary>
        /// 传递AudioClip播放音频
        /// </summary>
        /// <param name="clip"></param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        //[Obsolete("方法已弃用,请通过Build构建AudioPlayer后调用Play!")]
        public AudioPlayer Play(AudioClip clip)
        {
            if (clip == null)
                throw new NullReferenceException("丢失音频无法播放");            
            return Build(clip).Play();
        }

        public AudioPlayer Build(AudioClip clip)
        {
            if (clip == null)
                throw new NullReferenceException("丢失音频无法播放");
            return FindPlayerByGroup(clip.name).SetNameOrPath(clip.name).Clip(clip);
        }
        /// <summary>
        /// 异步播放音频
        /// </summary>
        /// <param name="name"></param>
        /// <param name="callBack"></param>
        /// <exception cref="NullReferenceException"></exception>
        //[Obsolete("方法已弃用,请通过BuildAsync构建AudioPlayer后调用Play!")]
        public void PlayAsync(string name, Action<AudioPlayer> callBack)
        {
            if (name.IsNullOrEmpty()) return;
            BuildAsync(name, player => 
            {                
                player.Play();
                callBack?.Invoke(player);
            });         
        }

#if UNITY_2021_1_OR_NEWER
        /// <summary>
        /// 异步播放音频
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        //[Obsolete("方法已弃用,请通过BuildAsync构建AudioPlayer后调用Play!")]
        public async YieldTask<AudioPlayer> PlayAsync(string name)
        {
            bool isCompleted = false;
            AudioPlayer audioPlayer = null;
            PlayAsync(name, player => 
            {
                audioPlayer = player;
                isCompleted = true;
            });
            await CoroutineTool.WaitUntil(() => isCompleted);
            return audioPlayer;
        }

        public async YieldTask<AudioPlayer> BuildAsync(string name)
        {
            bool isCompleted = false;
            AudioPlayer audioPlayer = null;
            BuildAsync(name, player =>
            {
                audioPlayer = player;
                isCompleted = true;
            });
            await CoroutineTool.WaitUntil(() => isCompleted);
            return audioPlayer;
        }
#else
    
        /// <summary>
        /// 异步播放音频
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [Obsolete("方法已弃用,请通过BuildAsync构建AudioPlayer后调用Play!")]
        public IEnumerator PlayAsync(string name,Transform parent = null)
        {
            bool isCompleted = false;
            PlayAsync(name, _ => isCompleted = true,parent);
            yield return CoroutineTool.WaitUntil(() => isCompleted);
        }

         /// <summary>
        /// 异步播放音频
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IEnumerator BuildAsync(string name,Transform parent = null)
        {
            bool isCompleted = false;
            BuildAsync(name, _ => isCompleted = true,parent);
            yield return CoroutineTool.WaitUntil(() => isCompleted);
        }
#endif   
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
                throw new InvalidCastException("调用暂停方法所属的音频分组不为Sound层，无法进行精确暂停,请调用无参Resume重载!");

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
                throw new InvalidCastException("调用暂停方法所属的音频分组不为Sound层，无法进行精确暂停,请调用无参Stop重载!");

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
        internal static bool CheckPlaySound(string name)
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

        internal bool CheckPlaySounding(string name)
        {
            if (AudioPlayType != AudioPlayType.Sound)
                return true;

            if (!soundActivities.TryGetValue(name, out var list))
                return true;

            foreach (var item in list)
            {
                if (item.IsAudioFree) continue;

                if (item.AudioSource && item.AudioSource.clip)
                    return false;
            }

            return true;
        }

        private AudioPlayer FindPlayerByGroup(string name)
        {
            AudioPlayer audioPlayer = null;
            if (AudioPlayType == AudioPlayType.Sound)
            {
                audioPlayer = SoundActivitiesExist(name);
                if (audioPlayer == null)
                {
                    audioPlayer = AudioManager.Instance.GetAudio();
                    soundActivities[name].Add(audioPlayer);
                }
            }
            else
                audioPlayer = this.audioPlayer;

            return audioPlayer.SetAudioGroup(this);
        }

       //internal AudioPlayer PlayInternal(string clipNameOrPath, AudioClip audioClip, IAudioLoader audioLoader)
       //{
       //    AudioPlayer audioPlayer = FindPlayerByGroup(clipNameOrPath);
       //
       //    void SetAudioVolume(float value)
       //    {
       //        audioPlayer.Volume = value * AudioKit.AudioVolumeScale.Value * AudioGroupVolumeScale.Value;
       //    }
       //
       //    this.Setting.IsOn.Register(value =>
       //    {
       //        audioPlayer.Mute = !value;
       //    }).UnRegisterWaitGameObjectDestroy(groupInfo.parent);
       //
       //    groupInfo.onStartCallBack += _ =>
       //    {
       //        this.Setting.Volume.UnRegister(SetAudioVolume);
       //        this.Setting.Volume
       //        .RegisterWithInitValue(SetAudioVolume)
       //        .UnRegisterWaitGameObjectDestroy(groupInfo.parent);
       //    };
       //    AudioManager.Instance.CheckLoaderCache(audioLoader);
       //    audioPlayer.SetAudio(groupInfo.parent, audioClip, groupInfo.loop, groupInfo.onStartCallBack
       //     , groupInfo.onEndCallBack, groupInfo.isRealTime, audioLoader, groupInfo.soundSetting);
       //    audioPlayer.Mute = !this.Setting.IsOn.Value;
       //    //最后初始化
       //    ResetInfo();
       //
       //    return audioPlayer;
       //}

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
        public string nameOrPath;
        public bool loop;
        public bool isRealTime;
        public Action<float> onStartCallBack;
        public Action<float> onEndCallBack;
        public Transform parent;      
        public AudioSourceSoundSetting soundSetting;
        public AudioClip audioClip;
        public IAudioLoader loader;
        public bool interval;
        public void Reset()
        {
            nameOrPath = string.Empty;
            loop = isRealTime = false;
            onStartCallBack = null;
            onEndCallBack = null;
            parent = null;
            soundSetting = null;
            audioClip = null;
            loader = null;
            interval = false;
        }
    }
  
}
