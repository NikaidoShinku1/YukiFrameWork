///=====================================================
/// - FileName:      AudioKit.cs
/// - NameSpace:     YukiFrameWork
/// - Created:       Yuki
/// - Email:         Yuki@qq.com
/// - Description:   AudioKit：音乐管理套件
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================

using UnityEngine;
using System;
using YukiFrameWork.Manager;

namespace YukiFrameWork
{
    public class AudioKit
    {
        private static AudioManager AudioManager;

        public static void Init()
        {
            var SceneAudioManager = UnityEngine.Object.FindObjectOfType<AudioManager>();
            if (SceneAudioManager == null)
            {
                SceneAudioManager = new GameObject(typeof(AudioManager).Name).AddComponent<AudioManager>();
                Debug.LogWarning("当前为代码加载声音管理器，此声音管理器没有任何参数跟剪辑，建议在编辑中自行添加!");
            }
            if (AudioManager != null && AudioManager != SceneAudioManager)
            {
                UnityEngine.Object.Destroy(AudioManager);
            }
            else
            {
                AudioManager = SceneAudioManager;
                UnityEngine.Object.DontDestroyOnLoad(AudioManager.gameObject);
            }
        }

        #region 音乐时序播放管理

        /// <summary>
        /// 播放人声(音效等)
        /// </summary>
        /// <param name="name">音频名</param>
        /// <param name="isWait">是否等待当前音频播放完毕</param>
        public static void PlayerVoices(string name, bool isWait = false)
        {
            AudioManager.PlayerVoices(name, isWait);
        }
        /// <summary>
        /// 播放音频
        /// </summary>
        /// <param name="name">名字</param>
        /// <param name="isWait">是否等待音频播放完毕</param>
        public static void PlayAudio(string name, bool isWait = false)
        {
            AudioManager.PlayAudio(name, isWait);
        }

        /// <summary>
        /// 结束正在播放的音频
        /// </summary>
        /// <param name="name">名字</param>
        public static void StopAudio(string name)
        {
            AudioManager.StopAudio(name);
        }

        public static void RemoveAudio(string name)
        {
            AudioManager.RemoveSource(name);
        }
        #endregion

        /// <summary>
        /// 调节指定音频的音量
        /// </summary>
        /// <param name="audioName">音频名</param>
        /// <param name="volume">音量大小，默认最大值为1</param>
        public static void SetVolume(string audioName, float volume = 1)
        {
            AudioManager.SetVolume(audioName, volume);
        }

        /// <summary>
        /// 调节指定音频分组的音量
        /// </summary>
        /// <param name="groupName">分组名</param>
        /// <param name="volume">音量大小，默认最大值为0(注意：使用此方法前请提前往AudioMixer暴露参数！)</param>
        public static void SetGroupVolume(string groupName, float volume = 0)
        {
            AudioManager.SetGroupVolume(groupName, volume);
        }

    }
}