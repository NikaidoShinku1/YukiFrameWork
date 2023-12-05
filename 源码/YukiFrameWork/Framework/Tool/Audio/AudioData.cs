using System.Collections.Generic;
using UnityEngine;
using System;
namespace YukiFrameWork
{
    [Serializable]
    public class AudioData
    {
        [SerializeField]private List<Audio> Audios = new List<Audio>();
        private Dictionary<string, AudioSource> audioClips = new Dictionary<string, AudioSource>();

        public List<Audio> GetAudios() => Audios;

        public Dictionary<string,AudioSource> GetAudioDicts() => audioClips;

        public void Clear()
        {
            Audios.Clear();
            audioClips.Clear();
        }

        public void AddSource(string name, AudioSource source)
        {
            if (audioClips.ContainsKey(name))
            {
                Debug.LogWarning($"当前音频已存在，音频名称为{name}");
                return;
            }
            var audio = Audios.Find(x => x.clip.name == name);
            if (audio == null)
            {
                Audios.Add(new Audio(source.clip, source.outputAudioMixerGroup, source.volume, source.playOnAwake, source.loop));                
            }
            if(!audioClips.ContainsKey(name))
                audioClips.Add(name, source);

        }

        public AudioSource GetAudioSource(string name)
        {
            return audioClips[name];
        }

        public Audio GetAudioData(string name)
            => Audios.Find(x => x.clip.name == name);

        public void RemoveSource(string name)
        {
            if (!audioClips.ContainsKey(name)) return;
            audioClips[name] = null;
            audioClips.Remove(name);
        }

        public bool Exist(string name)
        {
            return audioClips.ContainsKey(name);
        }
    }
}
