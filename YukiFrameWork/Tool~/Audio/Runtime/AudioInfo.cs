///=====================================================
/// - FileName:      AudioInfo.cs
/// - NameSpace:     YukiFrameWork.Audio
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/3/18 15:17:50
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using UnityEditor;
using UnityEngine.Events;
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

		[GUIGroup("使用AudioInfo进行音频加载无需执行AudioKit.Init方法!")]
		[RuntimeDisabledGroup]
		[HelperBox("音频的播放类型,Music用于背景音乐，Voice用于人声，Sound可以用于音效或者多个音频播放,\n特别提示:当使用Sound类型播放时，AudioSource会被添加在持有AudioInfo的这个对象上")
			,Label("音频的播放方式"),Space]
		[SerializeField,EnableIf("PlayOnAwake")] private PlayType playType;
        [field: GUIGroup("使用AudioInfo进行音频加载无需执行AudioKit.Init方法!")]
        [field: Label("是否在运行时自动播放")]
		[field: SerializeField,RuntimeDisabledGroup]		
		public bool PlayOnAwake { get; set; } = false;
        [field: GUIGroup("使用AudioInfo进行音频加载无需执行AudioKit.Init方法!")]
        [field: Label("是否循环?")]
        [field: SerializeField, RuntimeDisabledGroup]
        public bool Loop { get; set; } = false;
        [field: GUIGroup("使用AudioInfo进行音频加载无需执行AudioKit.Init方法!")]
        [field: Label("不受时间缩放影响?"), RuntimeDisabledGroup]
        [field: SerializeField]
        public bool IsRealTime { get; set; } = false;
        [field:GUIGroup("使用AudioInfo进行音频加载无需执行AudioKit.Init方法!"), RuntimeDisabledGroup]
        [field: PropertyRange(0, 1), Label("音频的初始播放音量大小"), SerializeField]
		public float Volume { get; set; } = 1;	
        [field: GUIGroup("使用AudioInfo进行音频加载无需执行AudioKit.Init方法!")]
        [field:SerializeField,Label("音频资源:"), RuntimeDisabledGroup]
		public AudioClip Clip { get; set; }
        [GUIGroup("使用AudioInfo进行音频加载无需执行AudioKit.Init方法!")]
        [HelperBox("开始播放时的事件注册器",Message.Warning),Space]
		public UnityEvent<float> onStartCallBack;
        [GUIGroup("使用AudioInfo进行音频加载无需执行AudioKit.Init方法!")]
        [HelperBox("结束/切换播放时的事件注册器", Message.Warning)]
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
