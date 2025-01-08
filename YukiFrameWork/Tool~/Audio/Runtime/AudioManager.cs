///=====================================================
/// - FileName:      AudioManager.cs
/// - NameSpace:     YukiFrameWork.Audio
/// - Created:       Yuki
/// - Email:         Yuki@qq.com
/// - Description:   这是一个框架工具创建的Mono脚本
/// - Creation Time: 2023年12月15日 10:13:46
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using YukiFrameWork.Pools;

namespace YukiFrameWork.Audio
{
    public class AudioManager : SingletonMono<AudioManager>
    {   
        private AudioListener audioListener;
        public AudioPlayer MusicPlayer { get; private set; }
        public AudioPlayer VoicePlayer { get; private set; }

        private SimpleObjectPools<AudioPlayer> audioPools;

        struct AudioCache
        {
            public string clipName;
            public float cacheLoadingTime;
            public IAudioLoader loader;      
            public AudioCache(IAudioLoader loader)
            {               
                this.loader = loader;
                clipName = loader.Clip.name;
                cacheLoadingTime = Time.time;      
            }
        }

        private Dictionary<string,AudioCache> cacheLoaderPools = new Dictionary<string, AudioCache>();
        private List<AudioCache> releaseCache = new List<AudioCache>();
        public override void OnInit()
        {            
            MusicPlayer = new AudioPlayer();
            VoicePlayer = new AudioPlayer();
            audioPools = new SimpleObjectPools<AudioPlayer>(() => new AudioPlayer(),10);
            
            CheckAudioListener();
            MonoHelper.Start(AudioCacheExecuted());
        }

        /// <summary>
        /// 添加加载器进入缓存计时
        /// </summary>
        /// <param name="loader"></param>
        public void AddLoaderCacheTime(IAudioLoader loader)
        {
            if (loader == null || loader.Clip == null) return;
            cacheLoaderPools[loader.Clip.name] = new AudioCache(loader);    
        }

        /// <summary>
        /// 检查加载器缓存，每次使用时检查如果存在则移除
        /// </summary>
        /// <param name="loader"></param>
        public void CheckLoaderCache(IAudioLoader loader)
        {
            if (loader == null || loader.Clip == null) return;

            if (cacheLoaderPools.ContainsKey(loader.Clip.name))
                cacheLoaderPools.Remove(loader.Clip.name);
        }
        [DisableEnumeratorWarning]
        private IEnumerator AudioCacheExecuted()
        {
            while (true)
            {
                foreach (var cache in cacheLoaderPools.Values)
                {
                    if (Time.time - cache.cacheLoadingTime >= AudioKit.UNLOAD_CACHESECOUND)
                    {                       
                        releaseCache.Add(cache);
                    }
                }
                for (int i = 0; i < releaseCache.Count; i++)
                {
                    var loader = releaseCache[i].loader;
                    cacheLoaderPools.Remove(loader.Clip.name);
                    AudioKit.Config.LoaderPools.Release(loader);

                    string cacheKey = string.Empty;
                    foreach (var item in AudioKit.audioLoaderDict)
                    {
                        if (item.Value == loader)
                        {
                            cacheKey = item.Key;
                            break;
                        }
                    }
                    if (cacheKey.IsNullOrEmpty())
                        continue;
                    AudioKit.audioLoaderDict.Remove(cacheKey);
                }
                releaseCache.Clear();
                yield return CoroutineTool.WaitForSeconds(AudioKit.DETECTION_INTERVAL);
            }
        }

        private void CheckAudioListener()
        {
            if (audioListener == null)
            {
#if UNITY_2023_1_OR_NEWER
                audioListener = FindAnyObjectByType<AudioListener>();
#else
                audioListener = FindObjectOfType<AudioListener>();
#endif
            }
            if (audioListener == null)
                audioListener = gameObject.AddComponent<AudioListener>();
        } 

        public AudioPlayer GetAudio() => audioPools.Get();

        public void Release(AudioPlayer audioPlayer) => audioPools.Release(audioPlayer);

        public override void OnDestroy()
        {
            base.OnDestroy();
            MusicPlayer = null;
            VoicePlayer = null;
            audioListener = null;
            AudioKit.Release();
        }

    }
}