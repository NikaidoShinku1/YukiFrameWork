///=====================================================
/// - FileName:      AudioInfo.cs
/// - NameSpace:     YukiFrameWork.Audio
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/3/18 15:17:50
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using YukiFrameWork.Extension;
using XFABManager;
namespace YukiFrameWork.Audio
{
	[System.Serializable]
	public class AudioSourceSoundSetting
	{
		
		[Range(0,256)]
		public int Priority = 128;

		[Range(-3,3)]
		public float Pitch = 1;

		[Range(-1,1)]
		public float StereoPan = 0;

		[Range(0,1)]
		public float SpatitalBlend = 0;

		[Range(0,1.1f)]
		public float ReverbZoneMix = 1;
		[Range(0,5)]
		public float DopplerLevel = 1;

		[Range(0,360)]
		public int Spread;

        public AudioRolloffMode VolumeRolloff;

        public float MinDistance = 1;

		public float MaxDistance = 500;		
    }
	[DisableViewWarning]
	public class AudioInfo : MonoBehaviour
	{
		internal enum PlayType
		{			
			Music,
			Voice,
			Sound
		}
		enum LoadType
		{
			XFABManager,
			Resources,
			Clip
		}

		internal enum Position
		{
			IgnorePosition,
			AudioManager
		}

		internal enum AudioType
		{
			[LabelText("默认音频选择")]
			Single,
			[LabelText("随机音频处理")]
            Random				
        }
        private const string info = "使用AudioInfo进行音频加载无需执行AudioKit的Init方法!";

        private bool isPlaying => ApplicationHelper.GetRuntimeOrEditor();
		[BoxGroup(info)]
		[DisableIf(nameof(isPlaying))]
		[InfoBox("音频的播放类型,Music用于背景音乐，Voice用于人声，Sound可以用于音效或者多个音频播放,\n特别提示:当使用Sound类型播放时，AudioSource会被添加在持有AudioInfo的这个对象上")
			,LabelText("音频的播放方式"),Space]
		[SerializeField] internal PlayType playType;
        [field: BoxGroup(info)]
        [field: LabelText("是否在运行时自动播放")]
		[field: SerializeField,DisableIf(nameof(isPlaying))]		
		public bool PlayOnAwake { get; set; } = false;
		[field:SerializeField,LabelText("是否在对象激活时自动播放"),DisableIf(nameof(isPlaying)), BoxGroup(info)]
		public bool PlayOnEnable { get; set; } = false;
        [field: BoxGroup(info)]
        [field: LabelText("是否循环?")]
        [field: SerializeField, DisableIf(nameof(isPlaying)),ShowIf(nameof(audioType),AudioType.Single)]
        public bool Loop { get; set; } = false;
        [field: BoxGroup(info)]
        [field: LabelText("不受时间缩放影响?"), DisableIf(nameof(isPlaying))]
        [field: SerializeField]
        public bool IsRealTime { get; set; } = false;

		[SerializeField,LabelText("是否自定义初始音量?"), BoxGroup(info), ShowIf(nameof(playType), PlayType.Sound), InfoBox("可以设置初始的音量，该音量仅保持在AudioKit.Setting对应的音频层级音量改动以前")]
		private bool IsVolume;
		[field: SerializeField, LabelText("初始默认音量?"), PropertyRange(0, 1), BoxGroup(info),ShowIf(nameof(isV))]
		public float Volume { get; set; } = 1;

		[SerializeField,LabelText("音频的资源加载类型"), BoxGroup(info)]
		private LoadType loadType = LoadType.XFABManager;

		[SerializeField, LabelText("音频的播放形式:"), BoxGroup(info)]
		internal AudioType audioType = AudioType.Single;

		[SerializeField,ShowIf(nameof(resSingle)) , BoxGroup(info)]
		private string resourcesPath;
        [SerializeField, ShowIf(nameof(loadType), LoadType.XFABManager), BoxGroup(info)]
        private string projectName;
        [SerializeField, ShowIf(nameof(abSingle)), BoxGroup(info)]
        private string assetName;

		[SerializeField,ShowIf(nameof(res)),BoxGroup(info)]
		[InfoBox("随机选择一个音频进行播放，随机音频是不允许循环播放的\n如果是XFABManager加载，请输入音频名称,如果是Resources，请自行处理")]
		private string[] assetOrResName;

		[BoxGroup(info)]
		[SerializeField, LabelText("音频资源:"), DisableIf(nameof(isPlaying)), ShowIf(nameof(s)), BoxGroup(info)]
		private AudioClip clip;

		public AudioClip Clip => audioType == AudioType.Single ? clip : (Clips == null || Clips.Length == 0) ? null : Clips[Random.Range(0,Clips.Length)];		

		[LabelText("3D Sound Setting")]
		public AudioSourceSoundSetting SoundSetting;
		
        #region Condition
        private bool s => loadType == LoadType.Clip && audioType == AudioType.Single;
        private bool cs => loadType == LoadType.Clip && audioType == AudioType.Random;
		private bool res => loadType != LoadType.Clip && audioType == AudioType.Random;
		private bool abSingle => loadType == LoadType.XFABManager && audioType == AudioType.Single;
		private bool resSingle => loadType == LoadType.Resources && audioType == AudioType.Single;
		private bool isV => playType == PlayType.Sound && IsVolume;
		#endregion

		[SerializeField, LabelText("音频资源:"), DisableIf(nameof(isPlaying)), ShowIf(nameof(cs)), BoxGroup(info)]
		private AudioClip[] Clips;

		[SerializeField,LabelText("音频管理的节点"), BoxGroup(info),ShowIf(nameof(playType),PlayType.Sound)]
		internal Position position;

        [BoxGroup(info)]
        [InfoBox("开始播放时的事件注册器",InfoMessageType.Warning),Space]
		public UnityEvent<float> onStartCallBack;
        [BoxGroup(info)]
        [InfoBox("结束/切换播放时的事件注册器", InfoMessageType.Warning)]
		public UnityEvent<float> onEndCallBack;

		internal string currentClipName;
        private void Awake()
        {
			if (audioType == AudioType.Single)
			{
				switch (loadType)
				{
					case LoadType.XFABManager:
						clip = AssetBundleManager.LoadAsset<AudioClip>(projectName, assetName);
						break;
					case LoadType.Resources:
						clip = Resources.Load<AudioClip>(resourcesPath);
						break;
				}
			}
			else
			{
				switch (loadType)
				{
					case LoadType.XFABManager:
						if (assetOrResName == null || assetOrResName.Length == 0)						
							return;
						Clips = new AudioClip[assetOrResName.Length];
						for (int i = 0; i < Clips.Length; i++)						
							Clips[i] = AssetBundleManager.LoadAsset<AudioClip>(projectName, assetOrResName[i]);
						
						break;
					case LoadType.Resources:
                        if (assetOrResName == null || assetOrResName.Length == 0)
                            return;
                        Clips = new AudioClip[assetOrResName.Length];
                        for (int i = 0; i < Clips.Length; i++)
                            Clips[i] = Resources.Load<AudioClip>(assetOrResName[i]);
                        break;
					case LoadType.Clip:
						break;					
				}

			}
			if (PlayOnAwake)
			{
				Play();
			}
        }

        private void OnEnable()
        {
			if (PlayOnEnable)
				Play();
        }

        public void Play()
		{
			
            switch (playType)
            {
                case PlayType.Music:
                    AudioKit.PlayMusic(this);
					
                    break;
                case PlayType.Voice:
                    AudioKit.PlayVoice(this);
                    break;
                case PlayType.Sound:
                    AudioPlayer player = AudioKit.PlaySound(this);

					if (player == null)
						return;

					if (playType != PlayType.Sound || !IsVolume)
						return;

					player.Volume = Volume;

                    break;
            }
        }

		public void Stop()
		{
			switch (playType)
			{
				case PlayType.Music:			
					AudioKit.StopMusic();
					break;
				case PlayType.Voice:
					AudioKit.StopVoice();
					break;
				case PlayType.Sound:
					AudioKit.StopSound(currentClipName);
					break;				
			}

		}
    }

}
