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
        public bool IsAudioFree { get; private set; } = true;
        
        internal bool IsAudioSource => mAudioSource && mAudioSource.gameObject;
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
            get => IsAudioSource ? mAudioSource.clip.name : string.Empty;
        }

        public void SetAudio(Transform target,AudioClip clip, bool loop, Action<float> onStartCallback, Action<float> onEndCallback,bool isRealTime,IAudioLoader loader,AudioSourceSoundSetting soundSetting)
        {
           
            if (!mAudioSource || !mAudioSource.gameObject)
            {                
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
                mAudioSource.Play();             
                IsAudioFree = false;
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

        public void Cancel()
        {
            //释放多判断一次
            if (audioTimer?.IsRunning == true)
                audioTimer.Cancel();           
            if (mAudioSource)
                mAudioSource.clip = null;

            audioTimer = null;
            IsAudioFree = true;
            onEndCallback = null;

            AudioManager.Instance.AddLoaderCacheTime(loader);
            loader = null;
        }
    }
}