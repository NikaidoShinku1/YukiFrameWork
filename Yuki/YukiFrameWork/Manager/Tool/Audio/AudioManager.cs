///=====================================================
/// - FileName:      AudioManager.cs
/// - NameSpace:     YukiFrameWork.Manager
/// - Created:       Yuki
/// - Email:         Yuki@qq.com
/// - Description:   AudioManager：音乐管理
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================


using UnityEngine;
using System.Linq;
using YukiFrameWork.Res;
using UnityEngine.Audio;
using System.Collections.Generic;
using System.Collections;
using System;

namespace YukiFrameWork.Manager
{    
    public enum Safe
    {
        默认模式 = 0,
        安全模式
    }

    /// <summary>
    /// 声音管理器
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        [SerializeField] private AudioData AudioData = new AudioData();
        private AudioSource currentSource;
        private readonly Queue<AudioSource> currentVoices = new Queue<AudioSource>();     
        private ResLoader ResLoader;
        [Header("是否动态加载音频")]
        [SerializeField]
        private bool IsAutoLoad = true;

        [SerializeField]
        [Header("加载源")]
        private ResourceType resType;
    
        [SerializeField]
        [Header("加载方式(使用Resources加载强制同步(选择异步也保持同步加载))")]
        private LoadMode loadMode;
        [Header("资源路径(使用前需要)")]
        [SerializeField]
        private string ClipPath;

        [Header("动态加载资源所绑定的分组")]
        [SerializeField]
        private AudioMixerGroup AudioMixerGroup;

        [Header("模式选择,安全模式在使用异步加载时有效，播放前强制检查音频是否完全加载完毕")]
        [SerializeField]
        private Safe safeType;

        private bool isCompleted;
        private void Awake()
        {
            if(resType == ResourceType.ResKit资源管理套件) 
                ResKit.Init();
            ResLoader = ResKit.GetLoader();
            Init();           
            isCompleted = false;
        }

        private void Init()
        {
            foreach (var Audio in AudioData.GetAudios())
            {
                try
                {
                    GameObject sourceObj = new GameObject(Audio.clip.name);
                    AudioSource source = sourceObj.AddComponent<AudioSource>();
                    sourceObj.transform.SetParent(transform);
                    SetSource(Audio, source);
                }
                catch
                {
                    Debug.LogWarning($"Visual audio not added !Checked {Audio.GetType()}");
                    break;
                }

            }
            if (IsAutoLoad)
            {
                switch (loadMode)
                {
                    case LoadMode.同步:
                        try
                        {
                            switch (resType)
                            {
                                case ResourceType.Resources:
                                    {
                                        var audioClips = Resources.LoadAll<AudioClip>(ClipPath);
                                        InitClip(audioClips.ToList());
                                    }
                                    break;
                                case ResourceType.ResKit资源管理套件:
                                    {
                                        var audioClips = ResLoader.LoadAllAssets<AudioClip>(ClipPath);
                                        InitClip(audioClips);
                                    }
                                    break;                             
                            }
                          
                        }
                        catch
                        {
                            Debug.LogError("Dynamic loading failed. Please check the path！path: " + ClipPath);
                        }
                        break;
                    case LoadMode.异步:
                        try
                        {
                            switch (resType)
                            {
                                case ResourceType.Resources:
                                    {
                                        var audioClips = Resources.LoadAll<AudioClip>(ClipPath);
                                        InitClip(audioClips.ToList());
                                    }
                                    break;
                                case ResourceType.ResKit资源管理套件:
                                    {
                                        ResLoader.InitAsync();
                                        ResLoader.LoadAllAssetsAsync<AudioClip>(ClipPath,(a,b,c,d)=> InitClip(a));                                        
                                    }
                                    break;
                            }
                        }
                        catch
                        {
                            Debug.LogError("Dynamic loading failed. Please check the path！path: " + ClipPath);
                        }
                        break;
                }
            }
        }

        public IEnumerator LoadAsync()
        {
            yield return new WaitUntil(() => isCompleted);
        }

        private void InitClip(List<AudioClip> audioClips)
        {
            foreach (var clip in audioClips)
            {
                Audio audio = new Audio(clip, AudioMixerGroup, 1, false, false);
                GameObject sourceObj = new GameObject(audio.clip.name);
                AudioSource source = sourceObj.AddComponent<AudioSource>();
                sourceObj.transform.SetParent(transform);
                SetSource(audio, source);                
            }
            isCompleted = true;
        }

        /// <summary>
        /// 添加保存所有的音乐
        /// </summary>
        /// <param name="Audio">音频</param>
        /// <param name="source">音频管理</param>
        private void SetSource(Audio Audio, AudioSource source)
        {
            source.transform.SetParent(transform);
            source.clip = Audio.clip;
            source.playOnAwake = Audio.playOnAwake;
            source.loop = Audio.isLoop;
            source.volume = Audio.volume;
            source.outputAudioMixerGroup = Audio.outputAudioMixerGroup;
            if (source.playOnAwake) source.Play();
            AddSource(Audio.clip.name, source);

        }

        /// <summary>
        /// 添加音频
        /// </summary>
        /// <param name="name">音频名字</param>
        /// <param name="source">音频管理</param>
        public void AddSource(string name, AudioSource source)
        {
            AudioData.AddSource(name, source);
        }

        /// <summary>
        /// 播放人声(音效等)
        /// </summary>
        /// <param name="name">音频名</param>
        /// <param name="isWait">是否等待当前音频播放完毕</param>
        public void PlayVoices(string name, bool isWait = false)
        {
             _PlayerVoices(name, isWait).Start();
        }

        private IEnumerator _PlayerVoices(string name, bool isWait = false)
        {
            if (safeType == Safe.安全模式)
                yield return new WaitUntil(() => isCompleted);
            if (!AudioData.Exist(name))
            {
                Debug.LogError($"当前名字没有对应音频无法播放！音频名为{name}");
                yield break;
            }           

            var source = AudioData.GetAudioSource(name);
            if (isWait)
            {
                yield return new WaitUntil(() => 
                {
                    if(currentVoices.Count > 0)
                        return !currentVoices.Peek().isPlaying;
                    return true;
                });
            }
            currentVoices.Enqueue(source);
            source.Play();         
            yield return new WaitUntil(() =>
                {
                    if(source != null)
                        return !source.isPlaying;
                    return true;
                });

            if(currentVoices.Count >= 5)
            currentVoices.Dequeue();
           
        }

        /// <summary>
        /// 播放音频
        /// </summary>
        /// <param name="name">音频名字</param>
        /// <param name="isWait">如果正在播放这个音乐，那么检查是否等待音乐播放完</param>
        public void PlayAudio(string name, bool isWait = false)
        {
            _PlayerAudio(name, isWait).Start();
        }

        private IEnumerator _PlayerAudio(string name,bool isWait)
        {
            if (safeType == Safe.安全模式)
                yield return new WaitUntil(() => isCompleted);
            if (!AudioData.Exist(name))
            {
                Debug.LogError($"当前名字没有对应音频无法播放！音频名为{name}");
                yield break;
            }          
            var source = AudioData.GetAudioSource(name);
            if (currentSource != null)
            {
                if (isWait)
                {
                    yield return new WaitUntil(() =>
                    {
                        if (currentSource != null)
                            return !currentSource.isPlaying;
                        return true;
                    });
                }
                currentSource.Stop();
                
                currentSource = source;
                currentSource.Play();

            }         
            else
            {
                currentSource = source;
                currentSource.Play();
            }            
        }

        /// <summary>
        /// 停止音频
        /// </summary>
        /// <param name="name">音频名字</param>
        public void StopAudioOrVoices(string name)
        {
            if (!AudioData.Exist(name))
            {
                Debug.LogError($"当前名字没有对应音频无法停止！音频名为{name}");
                return;
            }

            var source = AudioData.GetAudioSource(name);
           
            if (!source.isPlaying)
            {
                Debug.LogError($"当前音频没有被播放！音频名为{name}");
                return;
            }

            source.Stop();

        }

        public void PauseAudioOrVoices(string name)
        {
            if (!AudioData.Exist(name))
            {
                Debug.LogError($"当前名字没有对应音频无法暂停！音频名为{name}");
                return;
            }

            var source = AudioData.GetAudioSource(name);

            if (!source.isPlaying)
            {
                Debug.LogError($"当前音频没有被播放！音频名为{name}");
                return;
            }

            source.Pause();
        }

        /// <summary>
        /// 停止所有正在播放的音频
        /// </summary>
        public void StopAllSource()
        {
            if (currentSource != null) currentSource.Stop();
            while(currentVoices.Count > 0)
            {
                currentVoices.Dequeue().Stop();
            }
        }

        /// <summary>
        /// 删除音频
        /// </summary>
        /// <param name="name"></param>
        public void RemoveSource(string name)
        {
            AudioData.RemoveSource(name);
        }

        public AudioSource GetSource(string name)
        {
            return AudioData.GetAudioSource(name);
        }

        private void Update()
        {
            Update_SourceVolume();
        }

        private void Update_SourceVolume()
        {
            if (currentSource != null)
            {
                bool isVoices = false;
                foreach (var v in currentVoices)
                {
                    if (v.isPlaying)
                    {
                        isVoices = true;
                        break;
                    }
                }

                if (isVoices)                
                    currentSource.volume = (float)(AudioData.GetAudioData(currentSource.clip.name)?.volume) / 2;                
                else
                    currentSource.volume = (float)(AudioData.GetAudioData(currentSource.clip.name)?.volume);
            }
        }

        private void OnDestroy()
        {
            Clear();
        }

        public void Clear()
        {
            AudioData.Clear();
        }

        public void SetVolume(string audioName, float volume)
        {
            foreach(var item in AudioData.GetAudioDicts().Values)
            {
                if (item.name == audioName)               
                    item.volume = volume;
                
                if (currentSource != null && item.name == currentSource.name)              
                    currentSource.volume =  item.volume = volume;


                if (currentVoices.Count > 0)
                {
                    foreach (var voices in currentVoices)
                    {
                        if(voices.name == audioName)
                            voices.volume = volume;
                    }
                }
            }            
        }

        public void SetGroupVolume(string groupName, float volume)
        {
            foreach (var item in AudioData.GetAudioDicts().Values)
            {
                if (item.outputAudioMixerGroup.name == groupName)
                {
                    Debug.Log(item.outputAudioMixerGroup.audioMixer.name);
                    item.outputAudioMixerGroup.audioMixer.SetFloat(groupName, volume);
                    break;
                }
            }
        }
    }

}

