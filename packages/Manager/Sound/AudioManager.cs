///=====================================================
/// - FileName:      AudioManager.cs
/// - NameSpace:     YukiFrameWork.Manager
/// - Created:       Yuki
/// - Email:         Yuki@qq.com
/// - Description:   AudioManager�����ֹ���
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================


using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using YukiFrameWork.Res;
using UnityEngine.Audio;
using System.Collections.Generic;

namespace YukiFrameWork.Manager
{
    public enum LoadMode
    {
        ͬ��,
        �첽
    }
    /// <summary>
    /// ����������
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        [SerializeField] private AudioData AudioData = new AudioData();
        private AudioSource currentSource;
        private Queue<AudioSource> currentVoices = new Queue<AudioSource>();
        private readonly CancellationTokenSource tokenSource = new CancellationTokenSource();

        [Header("�Ƿ�̬������Ƶ")]
        [SerializeField]
        private bool IsLoad;

        [SerializeField]
        [Header("��������")]
        private Attribution attributionType;
        [SerializeField]
        [Header("���ط�ʽ")]
        private LoadMode loadMode;
        [Header("��Դ·��")]
        [SerializeField]
        private string ClipPath;

        [Header("��̬������Դ���󶨵ķ���")]
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
                    Debug.LogWarning($"���ӻ���Ƶ����δ���!Checked {Audio.GetType()}");
                    break;
                }

            }
            if (IsLoad)
            {
                switch (loadMode)
                {
                    case LoadMode.ͬ��:
                        var audioClips = ResKit.LoadAllSync<AudioClip>(attributionType, ClipPath);
                        InitClip(audioClips);
                        break;
                    case LoadMode.�첽:
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
        /// ��ӱ������е�����
        /// </summary>
        /// <param name="Audio">��Ƶ</param>
        /// <param name="source">��Ƶ����</param>
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
        /// �����Ƶ
        /// </summary>
        /// <param name="name">��Ƶ����</param>
        /// <param name="source">��Ƶ����</param>
        public void AddSource(string name, AudioSource source)
        {
            AudioData.AddSource(name, source);
        }

        /// <summary>
        /// ��������(��Ч��)
        /// </summary>
        /// <param name="name">��Ƶ��</param>
        /// <param name="isWait">�Ƿ�ȴ���ǰ��Ƶ�������</param>
        public void PlayerVoices(string name, bool isWait = false)
        {
            _ = _PlayerVoices(name, isWait);
        }

        private async UniTaskVoid _PlayerVoices(string name, bool isWait = false)
        {
            if (!AudioData.Exist(name))
            {
                Debug.LogError($"��ǰ����û�ж�Ӧ��Ƶ�޷����ţ���Ƶ��Ϊ{name}");
                return;
            }

            if (isWait)
            {
                if(currentVoices.Count > 0)
                {
                    foreach (var voices in currentVoices)
                    {
                        await UniTask.WaitUntil(() => !voices.isPlaying);
                    }
                }
            }

            var source = AudioData.GetAudioSource(name);
            currentVoices.Enqueue(source);
            source.Play();
            var tempVolume = currentSource.volume;
            if (currentSource != null) currentSource.volume = tempVolume / 2;
            await UniTask.WaitUntil(() => !source.isPlaying);
            currentVoices.Dequeue();
            currentSource.volume = tempVolume;
        }

        /// <summary>
        /// ������Ƶ
        /// </summary>
        /// <param name="name">��Ƶ����</param>
        /// <param name="isWait">������ڲ���������֣���ô����Ƿ�ȴ����ֲ�����</param>
        public void PlayAudio(string name, bool isWait = false)
        {
            _ = _PlayerAudio(name, isWait);
        }

        private async UniTaskVoid _PlayerAudio(string name,bool isWait)
        {
            if (!AudioData.Exist(name))
            {
                Debug.LogError($"��ǰ����û�ж�Ӧ��Ƶ�޷����ţ���Ƶ��Ϊ{name}");
                return;
            }
            var source = AudioData.GetAudioSource(name);
            if (currentSource != null)
            {
                if (isWait)
                {
                    await UniTask.WaitUntil(() => !currentSource.isPlaying, cancellationToken: tokenSource.Token);                 
                }
                currentSource.Stop();
                currentSource = source;
            }                      
            else
            {                
                currentSource = source;               
            }
            currentSource.Play();
        }

        /// <summary>
        /// ֹͣ��Ƶ
        /// </summary>
        /// <param name="name">��Ƶ����</param>
        public void StopAudio(string name)
        {
            if (!AudioData.Exist(name))
            {
                Debug.LogError($"��ǰ����û�ж�Ӧ��Ƶ�޷�ֹͣ����Ƶ��Ϊ{name}");
                return;
            }

            var source = AudioData.GetAudioSource(name);
            if (!source.isPlaying)
            {
                Debug.LogError($"��ǰ��Ƶû�б����ţ���Ƶ��Ϊ{name}");
                return;
            }

            source.Stop();

        }

        /// <summary>
        /// ɾ����Ƶ
        /// </summary>
        /// <param name="name"></param>
        public void RemoveSource(string name)
        {
            AudioData.RemoveSource(name);
        }

        private void OnDestroy()
        {
            Clear();
        }

        public void Clear()
        {
            AudioData.Clear();
        }
    }

}

