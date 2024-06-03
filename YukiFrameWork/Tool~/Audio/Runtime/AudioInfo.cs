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
	public class AudioInfo : MonoBehaviour
	{
		enum PlayType
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
		private const string info = "使用AudioInfo进行音频加载无需执行AudioKit的Init方法!";

        private bool isPlaying => ApplicationHelper.GetRuntimeOrEditor();
		[BoxGroup(info)]
		[DisableIf(nameof(isPlaying))]
		[InfoBox("音频的播放类型,Music用于背景音乐，Voice用于人声，Sound可以用于音效或者多个音频播放,\n特别提示:当使用Sound类型播放时，AudioSource会被添加在持有AudioInfo的这个对象上")
			,LabelText("音频的播放方式"),Space]
		[SerializeField] private PlayType playType;
        [field: BoxGroup(info)]
        [field: LabelText("是否在运行时自动播放")]
		[field: SerializeField,DisableIf(nameof(isPlaying))]		
		public bool PlayOnAwake { get; set; } = false;
        [field: BoxGroup(info)]
        [field: LabelText("是否循环?")]
        [field: SerializeField, DisableIf(nameof(isPlaying))]
        public bool Loop { get; set; } = false;
        [field: BoxGroup(info)]
        [field: LabelText("不受时间缩放影响?"), DisableIf(nameof(isPlaying))]
        [field: SerializeField]
        public bool IsRealTime { get; set; } = false;    

		[SerializeField,LabelText("音频的资源加载类型"), BoxGroup(info)]
		private LoadType loadType = LoadType.XFABManager;

		[SerializeField,ShowIf(nameof(loadType),LoadType.Resources), BoxGroup(info)]
		private string resourcesPath;
        [SerializeField, ShowIf(nameof(loadType), LoadType.XFABManager), BoxGroup(info)]
        private string projectName;
        [SerializeField, ShowIf(nameof(loadType), LoadType.XFABManager), BoxGroup(info)]
        private string assetName;
        [field: BoxGroup(info)]
        [field:SerializeField,LabelText("音频资源:"),DisableIf(nameof(isPlaying)), ShowIf(nameof(loadType), LoadType.Clip)]
		public AudioClip Clip { get;private set; }

		[SerializeField,LabelText("音频管理的节点"), BoxGroup(info),ShowIf(nameof(playType),PlayType.Sound)]
		internal Position position;
        [BoxGroup(info)]
        [InfoBox("开始播放时的事件注册器",InfoMessageType.Warning),Space]
		public UnityEvent<float> onStartCallBack;
        [BoxGroup(info)]
        [InfoBox("结束/切换播放时的事件注册器", InfoMessageType.Warning)]
		public UnityEvent<float> onEndCallBack;
        private void Awake()
        {
			switch (loadType)
			{
				case LoadType.XFABManager:
					Clip = AssetBundleManager.LoadAsset<AudioClip>(projectName, assetName);
					break;
				case LoadType.Resources:
					Clip = Resources.Load<AudioClip>(resourcesPath);
					break;			
			}
			if (PlayOnAwake)
			{
				Play();
			}
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
                    AudioKit.PlaySound(this);
                    break;
            }
        }
    }

}
