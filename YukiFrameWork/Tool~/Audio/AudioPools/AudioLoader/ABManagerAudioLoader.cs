///=====================================================
/// - FileName:      ABManagerAudioLoader.cs
/// - NameSpace:     YukiFrameWork.Audio
/// - Created:       Yuki
/// - Email:         Yuki@qq.com
/// - Description:   音频加载器类(通过框架ABManager定义加载)
/// - Creation Time: 2023年12月16日 13:26:15
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================

using UnityEngine;
using System;
using XFABManager;
namespace YukiFrameWork.Audio
{
    public class ABManagerAudioLoader : IAudioLoader
    {
        private AudioClip _clip;
        public AudioClip Clip => _clip;

        private string projectName;

        public ABManagerAudioLoader(string projectName)
            => this.projectName = projectName;

        public AudioClip LoadClip(string name)
        {
            if (_clip == null)
            {              
                _clip = AssetBundleManager.LoadAsset<AudioClip>(projectName, name);
            }
            return _clip;
        }

        public void LoadClipAsync(string name, Action<AudioClip> completedLoad)
        {
            if (_clip != null)
            {
                completedLoad?.Invoke(_clip);
                return;
            }          
            var request = AssetBundleManager.LoadAssetAsync<AudioClip>(projectName,name);
            request.AddCompleteEvent(operation => 
            {
                _clip = operation.asset as AudioClip;
                completedLoad?.Invoke(_clip);
            });           
        }

        public void UnLoad()
        {
            AssetBundleManager.UnloadAsset(_clip);
            _clip = null;
        }
    }
}