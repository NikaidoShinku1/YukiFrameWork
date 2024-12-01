///=====================================================
/// - FileName:      AudioVolumeListener.cs
/// - NameSpace:     YukiFrameWork.Audio
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/11/25 15:29:05
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using Sirenix.OdinInspector;
using UnityEngine.Events;
namespace YukiFrameWork.Audio
{
	public class AudioVolumeListener : YMonoBehaviour
	{
        [BoxGroup("音量改变触发事件")]
        public UnityEvent<float> musicVolume;
        [BoxGroup("音量改变触发事件")]
        public UnityEvent<float> voiceVolume;
        [BoxGroup("音量改变触发事件")]
        public UnityEvent<float> soundVolume;

        [BoxGroup("开关触发事件")]
        public UnityEvent<bool> isMusicOn;
        [BoxGroup("开关触发事件")]
        public UnityEvent<bool> isVoiceOn;
        [BoxGroup("开关触发事件")]
        public UnityEvent<bool> isSoundOn;

        [LabelText("是否确保对象仅在激活时触发事件")]
        public bool isEnable;

        protected override void Awake()
        {
            base.Awake();
            AudioKit.Setting.MusicVolume.RegisterWithInitValue(RegisterMusicVolume);
            AudioKit.Setting.VoiceVolume.RegisterWithInitValue(RegisterVoiceVolume);
            AudioKit.Setting.SoundVolume.RegisterWithInitValue(RegisterSoundVolume);
            AudioKit.Setting.IsMusicOn.RegisterWithInitValue(RegisterIsMusicOn);
            AudioKit.Setting.IsVoiceOn.RegisterWithInitValue(RegisterIsVoiceOn);
            AudioKit.Setting.IsSoundOn.RegisterWithInitValue(RegisterIsSoundOn);
        }

        private void RegisterMusicVolume(float volume)
        {
            if (isEnable && !gameObject.activeSelf)
                return;
            musicVolume?.Invoke(volume);
        }

        private void RegisterVoiceVolume(float volume)
        {
            if (isEnable && !gameObject.activeSelf)
                return;
            voiceVolume?.Invoke(volume);
        }

        private void RegisterSoundVolume(float volume)
        {
            if (isEnable && !gameObject.activeSelf)
                return;
            soundVolume?.Invoke(volume);
        }

        private void RegisterIsMusicOn(bool on)
        {
            if (isEnable && !gameObject.activeSelf)
                return;
            isMusicOn?.Invoke(on);
        }

        private void RegisterIsVoiceOn(bool on)
        {
            if (isEnable && !gameObject.activeSelf)
                return;
            isVoiceOn?.Invoke(on);
        }
        private void RegisterIsSoundOn(bool on)
        {
            if (isEnable && !gameObject.activeSelf)
                return;
            isSoundOn?.Invoke(on);
        }


        public void SetMusicVolume(float volume)
        {
            AudioKit.Setting.MusicVolume.Value = volume;
        }

        public void SetVoiceVolume(float volume)
        {
            AudioKit.Setting.VoiceVolume.Value = volume;
        }

        public void SetSoundVolume(float volume)
        {
            AudioKit.Setting.SoundVolume.Value = volume;
        }

        private void OnDestroy()
        {
            AudioKit.Setting.MusicVolume.UnRegister(RegisterMusicVolume);
            AudioKit.Setting.VoiceVolume.UnRegister(RegisterVoiceVolume);
            AudioKit.Setting.SoundVolume.UnRegister(RegisterSoundVolume);
            AudioKit.Setting.IsMusicOn.UnRegister(RegisterIsMusicOn);
            AudioKit.Setting.IsVoiceOn.UnRegister(RegisterIsVoiceOn);
            AudioKit.Setting.IsSoundOn.UnRegister(RegisterIsSoundOn);
        }
    }
}
