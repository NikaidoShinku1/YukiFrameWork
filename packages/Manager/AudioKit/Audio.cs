using System;
using UnityEngine;
using UnityEngine.Audio;

namespace YukiFrameWork
{
    [Serializable]
    public class Audio
    {
        [Header("音频剪辑")]
        public AudioClip clip;

        [Header("音频分组")]
        public AudioMixerGroup outputAudioMixerGroup;

        [Header("音量")]
        [Range(0, 1)]
        public float volume;

        [Header("是否在初始化的时候播放音频")]
        public bool playOnAwake;

        [Header("是否循环播放")]
        public bool isLoop;


        public Audio() { }

        public Audio(AudioClip clip, AudioMixerGroup outputAudioMixerGroup, float volume, bool playOnAwake, bool isLoop)
        {
            this.clip = clip;
            this.outputAudioMixerGroup = outputAudioMixerGroup;
            this.volume = volume;
            this.playOnAwake = playOnAwake;
            this.isLoop = isLoop;
        }

    }
}
