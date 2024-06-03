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
        internal const string MUSICPLAYERPREFS_VOLUME_KEY = nameof(MUSICPLAYERPREFS_VOLUME_KEY);
        internal const string VOICEPLAYERPREFS_VOLUME_KEY = nameof(VOICEPLAYERPREFS_VOLUME_KEY);
        internal const string SOUNDPLAYERPREFS_VOLUME_KEY = nameof(SOUNDPLAYERPREFS_VOLUME_KEY);

        internal const string MUSICPLAYERPREFS_ON_KEY = nameof(MUSICPLAYERPREFS_ON_KEY);
        internal const string VOICEPLAYERPREFS_ON_KEY = nameof(VOICEPLAYERPREFS_ON_KEY);
        internal const string SOUNDPLAYERPREFS_ON_KEY = nameof(SOUNDPLAYERPREFS_ON_KEY);

        internal const string AUDIOPLAYERPREFS_ON_KEY = nameof(AUDIOPLAYERPREFS_ON_KEY);

        public BindablePropertyPlayerPrefsByFloat MusicVolume { get; private set; }
        public BindablePropertyPlayerPrefsByFloat VoiceVolume { get; private set; }
        public BindablePropertyPlayerPrefsByFloat SoundVolume { get; private set; }

        public BindablePropertyPlayerPrefsByBoolan IsMusicOn { get; private set; }
        public BindablePropertyPlayerPrefsByBoolan IsVoiceOn { get; private set; }
        public BindablePropertyPlayerPrefsByBoolan IsSoundOn { get; private set; }

        //设置全部音频
        public BindablePropertyPlayerPrefsByBoolan IsAudioOn { get; private set; }

        public AudioSetting()
        {
            MusicVolume = new BindablePropertyPlayerPrefsByFloat(MUSICPLAYERPREFS_VOLUME_KEY,1);
            VoiceVolume = new BindablePropertyPlayerPrefsByFloat(VOICEPLAYERPREFS_VOLUME_KEY,1);
            SoundVolume = new BindablePropertyPlayerPrefsByFloat(SOUNDPLAYERPREFS_VOLUME_KEY,1);

            IsMusicOn = new BindablePropertyPlayerPrefsByBoolan(MUSICPLAYERPREFS_ON_KEY,true);
            IsVoiceOn = new BindablePropertyPlayerPrefsByBoolan(VOICEPLAYERPREFS_ON_KEY,true);
            IsSoundOn = new BindablePropertyPlayerPrefsByBoolan(SOUNDPLAYERPREFS_ON_KEY,true);

            IsAudioOn = new BindablePropertyPlayerPrefsByBoolan(AUDIOPLAYERPREFS_ON_KEY,true);

            IsAudioOn.Register(value => 
            {
                IsMusicOn.Value = value;
                IsVoiceOn.Value = value;
                IsSoundOn.Value = value;
            });
        }
    }
}