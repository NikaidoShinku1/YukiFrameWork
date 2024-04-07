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
		private bool isPlaying => ApplicationHelper.GetRuntimeOrEditor();
		[BoxGroup("使用AudioInfo进行音频加载无需执行AudioKit的Init方法!")]
		[HideIf(nameof(isPlaying))]
		[InfoBox("音频的播放类型,Music用于背景音乐，Voice用于人声，Sound可以用于音效或者多个音频播放,\n特别提示:当使用Sound类型播放时，AudioSource会被添加在持有AudioInfo的这个对象上")
			,LabelText("音频的播放方式"),Space]
		[SerializeField,ShowIf(nameof(PlayOnAwake))] private PlayType playType;
        [field: BoxGroup("使用AudioInfo进行音频加载无需执行AudioKit的Init方法!")]
        [field: LabelText("是否在运行时自动播放")]
		[field: SerializeField,HideIf(nameof(isPlaying))]		
		public bool PlayOnAwake { get; set; } = false;
        [field: BoxGroup("使用AudioInfo进行音频加载无需执行AudioKit的Init方法!")]
        [field: LabelText("是否循环?")]
        [field: SerializeField, HideIf(nameof(isPlaying))]
        public bool Loop { get; set; } = false;
        [field: BoxGroup("使用AudioInfo进行音频加载无需执行AudioKit的Init方法!")]
        [field: LabelText("不受时间缩放影响?"), HideIf(nameof(isPlaying))]
        [field: SerializeField]
        public bool IsRealTime { get; set; } = false;
        [field: BoxGroup("使用AudioInfo进行音频加载无需执行AudioKit的Init方法!"), HideIf(nameof(isPlaying))]
        [field: PropertyRange(0, 1), LabelText("音频的初始播放音量大小"), SerializeField]
		public float Volume { get; set; } = 1;	
        [field: BoxGroup("使用AudioInfo进行音频加载无需执行AudioKit的Init方法!")]
        [field:SerializeField,LabelText("音频资源:"),HideIf(nameof(isPlaying))]
		public AudioClip Clip { get; set; }
        [BoxGroup("使用AudioInfo进行音频加载无需执行AudioKit的Init方法!")]
        [InfoBox("开始播放时的事件注册器",InfoMessageType.Warning),Space]
		public UnityEvent<float> onStartCallBack;
        [BoxGroup("使用AudioInfo进行音频加载无需执行AudioKit的Init方法!")]
        [InfoBox("结束/切换播放时的事件注册器", InfoMessageType.Warning)]
		public UnityEvent<float> onEndCallBack;
        private void Awake()
        {			
			if (PlayOnAwake)
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

}
