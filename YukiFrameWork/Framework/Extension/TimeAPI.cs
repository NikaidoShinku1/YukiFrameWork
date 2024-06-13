///=====================================================
/// - FileName:      TimeAPI.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   时间戳拓展
/// - Creation Time: 2024/6/13 12:14:23
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using System.Collections;
namespace YukiFrameWork
{
	/// <summary>
	/// 实验性功能，Unity原有的Time中会被缩放影响的部分操作类
	/// </summary>
	public static class TimeAPI
	{
		internal static float _timeScale;

		public static event Action<float> timeScaleChaned = null;

		public static event Action onPauseCallBack = null;
		public static event Action onRemuseCallBack = null;

		public static float timeScale
		{
			get 
			{
				return _timeScale;
			}
			set
			{
				if (_timeScale != value)
				{
					if (value < 0)
						value = 0;

                    if (value == 0)
                        onPauseCallBack?.Invoke();
                    else if (_timeScale == 0)
                        onRemuseCallBack?.Invoke();

                    _timeScale = value;
                    timeScaleChaned?.Invoke(value);
					
                }
			}
		}

		public static float time { get; private set; }

        public static float deltaTime => Time.deltaTime * timeScale;

		public static float fixedDeltaTime => Time.fixedDeltaTime * timeScale;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		static void Init()
		{
			timeScale = 1;
			timeScaleChaned = null;
            
            if (Application.isPlaying)
			{
				MonoHelper.Destroy_AddListener(_ => Dispose());

				MonoHelper.Update_AddListener(_ => 
				{					
					time += deltaTime;
				});
			}
		}

        private static void Dispose()
        {
			timeScaleChaned = null;
			onRemuseCallBack = null;
			onPauseCallBack = null;
        }

		public static IEnumerator WaitForSeconds(float time)
		{
			yield return new CustomWaitForSeconds(time);
		}

        struct CustomWaitForSeconds : IEnumerator
        {
			internal float time;
			public CustomWaitForSeconds(float time)
			{
                this.time = time;
				currentTime = 0;
            }

			public object Current => null;

			private float currentTime;
            public bool MoveNext()
            {				
				currentTime += deltaTime;
				return currentTime < time;
            }

            public void Reset()
            {
				time = -1;
            }
        }
    }
}
