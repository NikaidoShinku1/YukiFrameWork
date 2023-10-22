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
using Cysharp.Threading.Tasks;
using System.Threading;
using YukiFrameWork.Res;
using UnityEngine.Audio;
using System.Collections.Generic;
using System;

namespace YukiFrameWork.Manager
{
    public enum LoadMode
    {
        同步,
        异步
    }
    /// <summary>
    /// 声音管理器
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        [SerializeField] private AudioData AudioData = new AudioData();
        private AudioSource currentSource;
        private Queue<AudioSource> currentVoices = new Queue<AudioSource>();
        private readonly CancellationTokenSource tokenSource = new CancellationTokenSource();

        [Header("是否动态加载音频")]
        [SerializeField]
        private bool IsLoad;

        [SerializeField]
        [Header("加载类型")]
        private Attribution attributionType;
        [SerializeField]
        [Header("加载方式")]
        private LoadMode loadMode;
        [Header("资源路径")]
        [SerializeField]
        private string ClipPath;

        [Header("动态加载资源所绑定的分组")]
        [SerializeField]
        private AudioMixerGroup AudioMixerGroup;
        private void Awake()
        {
            Init();
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
                    Debug.LogWarning($"可视化音频剪辑未添加!Checked {Audio.GetType()}");
                    break;
                }

            }
            if (IsLoad)
            {
                switch (loadMode)
                {
                    case LoadMode.同步:
                        var audioClips = ResKit.LoadAllSync<AudioClip>(attributionType, ClipPath);
                        InitClip(audioClips);
                        break;
                    case LoadMode.异步:
                        _ = ResKit.LoadAllAsync<AudioClip>(attributionType, ClipPath, clips =>
                        {
                            InitClip(clips);
                        });
                        break;
                }
            }
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
                Debug.Log(audio);
            }
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
            _ = _PlayerVoices(name, isWait);
        }

        private async UniTaskVoid _PlayerVoices(string name, bool isWait = false)
        {
            if (!AudioData.Exist(name))
            {
                Debug.LogError($"当前名字没有对应音频无法播放！音频名为{name}");
                return;
            }           

            var source = AudioData.GetAudioSource(name);
            if (isWait)
            {
                await UniTask.WaitUntil(() => 
                {
                    return !currentVoices.Peek().isPlaying;
                },cancellationToken:tokenSource.Token);
            }
            currentVoices.Enqueue(source);
            source.Play();
            var tempVolume = 1f;
            if(currentSource != null)
            tempVolume = currentSource.volume;
            if (currentSource != null) currentSource.volume = tempVolume / 2;
            await UniTask.WaitUntil(() =>
                {
                    if(source != null)
                        return !source.isPlaying;
                    return true;
                });

            if(currentVoices.Count >= 5)
            currentVoices.Dequeue();

            if(currentSource != null)
            currentSource.volume = tempVolume;
        }

        /// <summary>
        /// 播放音频
        /// </summary>
        /// <param name="name">音频名字</param>
        /// <param name="isWait">如果正在播放这个音乐，那么检查是否等待音乐播放完</param>
        public void PlayAudio(string name, bool isWait = false)
        {
            _ = _PlayerAudio(name, isWait);
        }

        private async UniTaskVoid _PlayerAudio(string name,bool isWait)
        {
            if (!AudioData.Exist(name))
            {
                Debug.LogError($"当前名字没有对应音频无法播放！音频名为{name}");
                return;
            }          
            var source = AudioData.GetAudioSource(name);
            if (currentSource != null && currentSource.name != name)
            {
                if (isWait)
                {
                    await UniTask.WaitUntil(() =>
                    {
                        if (currentSource != null)
                            return !currentSource.isPlaying;
                        return true;
                    }, cancellationToken: tokenSource.Token);
                }
                currentSource.Stop();
                currentSource = source;
                currentSource.Play();

            }
            else if (currentSource != null)
            {
                Debug.Log(currentSource.name);                 
                currentSource.UnPause();
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

