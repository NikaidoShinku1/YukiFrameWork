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
        private Action<float> onEndCallback = null;      
        private IYieldExtension audioTimer;
        private bool isRealTime;
        private IAudioLoader loader;
        public bool IsAudioFree { get; private set; } = true;

        public float Volume
        {
            get => mAudioSource.volume;
            set => mAudioSource.volume = value;
        }

        public bool Mute
        {
            get => mAudioSource.mute;
            set => mAudioSource.mute = value;
        }

        public void SetAudio(Transform target,AudioClip clip, bool loop, Action<float> onStartCallback, Action<float> onEndCallback,bool isRealTime,IAudioLoader loader)
        {
            if (mAudioSource == null)
            {
                mAudioSource = target.gameObject.AddComponent<AudioSource>();
            }

            if (mAudioSource.clip == null || !mAudioSource.clip.Equals(clip))
            {            
                Stop();
                if(loader != null)
                    this.loader = loader;
                onStartCallback?.Invoke(isRealTime ? Time.realtimeSinceStartup : Time.time);
                this.onEndCallback = onEndCallback;
                mAudioSource.clip = clip;
                mAudioSource.loop = loop;               
                mAudioSource.Play();             
                IsAudioFree = false;
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
            if (audioTimer?.IsRunning == true)                           
                audioTimer.Cancel();          
            audioTimer = null;
            IsAudioFree = true;
            onEndCallback?.Invoke(isRealTime ? Time.realtimeSinceStartup : Time.time);
            onEndCallback = null;           
            mAudioSource.clip = null;         
            AudioManager.Instance.AddLoaderCacheTime(loader);
            loader = null;
        }       
    }
}