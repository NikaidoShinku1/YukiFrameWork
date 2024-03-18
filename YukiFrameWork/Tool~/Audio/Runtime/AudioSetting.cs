///=====================================================
/// - FileName:      AudioSetting.cs
/// - NameSpace:     YukiFrameWork.Audio
/// - Created:       Yuki
/// - Email:         Yuki@qq.com
/// - Description:   音频设置类
/// - Creation Time: 2023年12月15日 10:13:38
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================

using UnityEngine;
using System;
namespace YukiFrameWork.Audio
{
    public class AudioSetting
    {
        public BindableProperty<float> MusicVolume { get; private set; }
        public BindableProperty<float> VoiceVolume { get; private set; }
        public BindableProperty<float> SoundVolume { get; private set; }

        public BindableProperty<bool> IsMusicOn { get; private set; }
        public BindableProperty<bool> IsVoiceOn { get; private set; }
        public BindableProperty<bool> IsSoundOn { get; private set; }

        //设置全部音频
        public BindableProperty<bool> IsAudioOn { get; private set; }

        public AudioSetting()
        {
            MusicVolume = new BindableProperty<float>(1);
            VoiceVolume = new BindableProperty<float>(1);
            SoundVolume = new BindableProperty<float>(1);

            IsMusicOn = new BindableProperty<bool>(true);
            IsVoiceOn = new BindableProperty<bool>(true);
            IsSoundOn = new BindableProperty<bool>(true);

            IsAudioOn = new BindableProperty<bool>(true);

            IsAudioOn.RegisterWithInitValue(value => 
            {
                IsMusicOn.Value = value;
                IsVoiceOn.Value = value;
                IsSoundOn.Value = value;
            });
        }
    }
}