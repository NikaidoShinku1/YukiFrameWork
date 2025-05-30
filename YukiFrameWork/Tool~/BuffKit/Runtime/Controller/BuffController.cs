///=====================================================
/// - FileName:      BuffController.cs
/// - NameSpace:     YukiFrameWork.Buffer
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/5/8 15:45:11
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using YukiFrameWork.Pools;
namespace YukiFrameWork.Buffer
{
	
    public abstract class BuffController : AbstractController, IGlobalSign
	{
		void IGlobalSign.Init()
		{
			OnInit();
		}	
        public override void OnInit()
        {
            
        }
		internal float fixedTimer = 0;
        public BuffHandler Handler => Player.Handler;

		public Action<float> onBuffReleasing { get; set; }
		public Action<int> onBuffStart { get; set; }
		public Action<int> onBuffRemove { get; set; }	

		void IGlobalSign.Release()
		{			
			Player = null;
			Buffer = null;
			onBuffReleasing = null;
			onBuffStart = null;
			onBuffRemove = null;
		}
		
		internal static bool Release(BuffController controller) 
		{
			return GlobalObjectPools.GlobalRelease(controller);
		}

		//private static Dictionary<Type,Func<BuffController, bool>> OnReleasePairs = new Dictionary<Type, Func<BuffController, bool>>();

        public IBuff Buffer { get; internal set; }

		

		public string BuffKey => Buffer.GetBuffKey;		

        public int BuffLayer { get; internal set; }		

        public IBuffExecutor Player { get;internal set; }

		public bool IsMarkIdle { get; set; }		

		public float MaxTime => Buffer.BuffTimer;		

		/// <summary>
		/// 该Buff的剩余时间
		/// </summary>
		public float RemainingTime
		{
			get
			{
				if (Buffer.SurvivalType == BuffSurvivalType.Timer)
					return mRemainingTime;
				return -1;
			}
			internal set
			{
                mRemainingTime = value;
			}
		}

		/// <summary>
		/// Buff的剩余进度(1-0)
		/// </summary>
		public float RemainingProgress
		{
			get => Buffer.SurvivalType == BuffSurvivalType.Timer ? Mathf.Clamp01(RemainingTime / Buffer.BuffTimer) : 1;
        }

		private float mRemainingTime;
		
        public BuffController(IBuff buffer, IBuffExecutor player)
		{
			this.Buffer = buffer;
			this.Player = player;
			BuffLayer = 0;
		}

		public BuffController() { }
	
		public virtual bool OnAddBuffCondition() => true;
		
		public virtual bool OnRemoveBuffCondition() => false;
	
		public abstract void OnBuffAwake();	
		
		public abstract void OnBuffStart();	

		public virtual void OnBuffUpdate() { }

		public virtual void OnBuffFixedUpdate() { }

		public virtual void OnBuffLateUpdate() { }
		
		public abstract void OnBuffRemove();
		
		public virtual void OnBuffDestroy()
		{
			
		}	
    }
}
