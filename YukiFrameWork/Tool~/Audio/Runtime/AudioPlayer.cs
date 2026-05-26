///=====================================================
/// - FileName:      AudioPlayer.cs
/// - NameSpace:     YukiFrameWork.Audio
/// - Created:       Yuki
/// - Email:         Yuki@qq.com
/// - Description:   这是一个框架工具创建的脚本
/// - Creation Time: 2023年12月15日 10:13:32
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================

using UnityEngine;
using System;
namespace YukiFrameWork.Audio
{
    public class AudioPlayer
    {
        private AudioSource mAudioSource;
        public AudioSource AudioSource => mAudioSource;
        private Action<float> onEndCallback = null;      
        private IYieldExtension audioTimer;
        private bool isRealTime;
        private IAudioLoader loader;
        private AudioGroupInfo audioGroupInfo = new AudioGroupInfo();

        public bool IsAudioFree { get; private set; } = true;
        
        internal bool IsAudioSource => mAudioSource && mAudioSource.gameObject;
        internal AudioGroup audioGroup;
        public float Volume
        {
            get => IsAudioSource ? mAudioSource.volume : 0;
            set
            {
                if(IsAudioSource)
                    mAudioSource.volume = value;

            }
        }

        public bool Mute
        {
            get => IsAudioSource ? mAudioSource.mute : false;
            set
            {
                if(IsAudioSource)
                    mAudioSource.mute = value;
            }
        }

        public string ClipName
        {
            get => IsAudioSource && mAudioSource.clip ? mAudioSource.clip.name : string.Empty;
        }

        public AudioPlayer Loop()
        {
            audioGroupInfo.loop = true;
            return this;
        }

        public AudioPlayer OnStartCallBack(Action<float> onStartCallback)
        {
            audioGroupInfo.onStartCallBack += onStartCallback;
            return this;
        }

        public AudioPlayer OnEndCallBack(Action<float> onEndCallback)
        {
            audioGroupInfo.onEndCallBack += onEndCallback;
            return this;
        }

        /// <summary>
        /// 间隔播放,当属于Sound层生效,如果有同名clip正在播放则不会重复播放
        /// </summary>
        /// <returns></returns>
        public AudioPlayer Interval()
        {
            audioGroupInfo.interval = true;
            return this;
        }

        public AudioPlayer IsRealTime()
        {
            audioGroupInfo.isRealTime = true;
            return this;
        }

        public AudioPlayer Parent(Transform parent)
        {
            audioGroupInfo.parent = parent;
            return this;
        }

        public AudioPlayer Play()
        {
            if (audioGroup.AudioPlayType == AudioPlayType.Sound)
            {
                if (!AudioGroup.CheckPlaySound(audioGroupInfo.nameOrPath)) return this;

                if (audioGroupInfo.interval && !audioGroup.CheckPlaySounding(audioGroupInfo.nameOrPath))
                {                    
                    return this;
                }
            }
            //如果不设置父节点则统一AudioManager管理
            if (!audioGroupInfo.parent && AudioManager.Instance)
                audioGroupInfo.parent = AudioManager.Instance.transform;
            PlayInternal(audioGroupInfo.audioClip, audioGroupInfo.loader);
            return this;
        }

        public AudioPlayer Clip(AudioClip audioClip)
        {
            audioGroupInfo.audioClip = audioClip;
            return this;
        }
        
        

        internal AudioPlayer SetLoader(IAudioLoader audioLoader)
        {
            audioGroupInfo.loader = audioLoader;
            return this;
        }
        internal AudioPlayer SetAudioGroup(AudioGroup audioGroup)
        {
            this.audioGroup = audioGroup;
            return this;
        }

        internal AudioPlayer SetNameOrPath(string name)
        {
            audioGroupInfo.nameOrPath = name;
            return this;
        }
        public AudioPlayer AudioSourceSoundSetting(AudioSourceSoundSetting audioSourceSoundSetting)
        {
            audioGroupInfo.soundSetting = audioSourceSoundSetting;
            return this;
        }
        internal void  PlayInternal(AudioClip audioClip, IAudioLoader audioLoader)
        {                      
            void SetAudioVolume(float value)
            {
                if(audioGroup != null)
                    Volume = value * AudioKit.AudioVolumeScale.Value * audioGroup.BindAudioGroupVolumeScale.Value * audioGroup.AudioGroupVolumeScale.Value;
            }

            audioGroup.Setting.IsOn.Register(value =>
            {
                Mute = !value;
            }).UnRegisterWaitGameObjectDestroy(audioGroupInfo.parent);

            audioGroupInfo.onStartCallBack += _ =>
            {
                audioGroup.Setting.Volume.UnRegister(SetAudioVolume);
                audioGroup.Setting.Volume
                .RegisterWithInitValue(SetAudioVolume)
                .UnRegisterWaitGameObjectDestroy(audioGroupInfo.parent);
            };
            AudioManager.Instance.CheckLoaderCache(audioLoader);
            SetAudio(audioGroupInfo.parent, audioClip, audioGroupInfo.loop, audioGroupInfo.onStartCallBack
             , audioGroupInfo.onEndCallBack, audioGroupInfo.isRealTime, audioLoader, audioGroupInfo.soundSetting);
            Mute = !audioGroup.Setting.IsOn.Value;
            //最后初始化
                
        }

        public void SetAudio(Transform target,AudioClip clip, bool loop, Action<float> onStartCallback, Action<float> onEndCallback,bool isRealTime,IAudioLoader loader,AudioSourceSoundSetting soundSetting)
        {            
            if (!mAudioSource || !mAudioSource.gameObject || mAudioSource.transform != target)
            {
                if(mAudioSource && mAudioSource.transform != target)
                    UnityEngine.Object.Destroy(mAudioSource);
                
                mAudioSource = target.gameObject.AddComponent<AudioSource>();               
            }

            if (mAudioSource.clip == null || !mAudioSource.clip.Equals(clip))
            {    
                if(loader != null)
                    this.loader = loader;              
                onStartCallback?.Invoke(isRealTime ? Time.realtimeSinceStartup : Time.time);
                this.onEndCallback = onEndCallback;
                mAudioSource.clip = clip;
                mAudioSource.loop = loop;
                if (soundSetting != null)
                {
                    mAudioSource.spatialBlend = soundSetting.SpatitalBlend;
                    mAudioSource.rolloffMode = soundSetting.VolumeRolloff;
                    mAudioSource.dopplerLevel = soundSetting.DopplerLevel;
                    mAudioSource.pitch = soundSetting.Pitch;
                    mAudioSource.priority = soundSetting.Priority;
                    mAudioSource.panStereo = soundSetting.StereoPan;
                    mAudioSource.reverbZoneMix = soundSetting.ReverbZoneMix;
                    mAudioSource.spread = soundSetting.Spread;
                    mAudioSource.minDistance = soundSetting.MinDistance;
                    mAudioSource.maxDistance = soundSetting.MaxDistance;
                }
                IsAudioFree = false;
                mAudioSource.Play();                
                //如果协程再走，则需要先进行终止
                if (audioTimer?.IsRunning == true)
                    audioTimer.Cancel();
                if (!loop)
                {                  
                    audioTimer = StartTimer(clip.length, isRealTime).Start();
                }
            }
            this.isRealTime = isRealTime;
        }

        private System.Collections.IEnumerator StartTimer(float length,bool isRealTime)
        {            
            float time = 0;
            while (time < length)
            {              
                time += isRealTime ? Time.unscaledDeltaTime : Time.deltaTime;
                yield return null;
            }           
            Stop();
        }

        public void Pause()
        {
            if (mAudioSource == null || !mAudioSource.isPlaying) return;
            mAudioSource.Pause();

            if (audioTimer == null || mAudioSource.loop) return;
            audioTimer.OnPause();
        }

        public void Resume()
        {
            if (mAudioSource == null || mAudioSource.isPlaying) return;
            mAudioSource.UnPause();

            if (audioTimer == null || mAudioSource.loop) return;
            audioTimer.OnResume();
        }
     
        public void Stop()
        {
            bool isEndCallBack = false;
            if (audioTimer?.IsRunning == true)
            {              
                isEndCallBack = true;
                onEndCallback?.Invoke(isRealTime ? Time.realtimeSinceStartup : Time.time);

            }
            if (mAudioSource)
            {
                if(mAudioSource.loop && !isEndCallBack)
                    onEndCallback?.Invoke(isRealTime ? Time.realtimeSinceStartup : Time.time);              
            }
            Cancel();
           
        }

        public async void Cancel()
        {
            //释放多判断一次
            if (audioTimer?.IsRunning == true)
                audioTimer.Cancel();
            if (mAudioSource)
            {
                if (mAudioSource.isPlaying || mAudioSource.loop)
                {
                    mAudioSource.Stop();
                }
                mAudioSource.clip = null;
            }

            audioTimer = null;         
            onEndCallback = null;          
            loader = null;
            audioGroupInfo.Reset();
            audioGroup = null;
            IsAudioFree = true;
            if(AudioManager.Instance)
                AudioManager.Instance.AddLoaderCacheTime(loader);
        }
    }
}