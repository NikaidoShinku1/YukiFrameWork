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
using System.Collections.Generic;
namespace YukiFrameWork.Buffer
{
	public interface IBuffController : IController, IGlobalSign
    {
		IBuff Buffer { get; internal set; }
		string BuffKey { get; }
		UIBuffer UIBuffer { get;internal set; }
		int BuffLayer { get;internal set; }
		GameObject Player { get;internal set; }
        float MaxTime { get; }
        float RemainingTime { get;internal set; }
		float RemainingProgress { get; }

        /// <summary>
        /// 内部的Buff添加条件，默认为True，当需要内部处理添加Buff的逻辑或者比如希望自己手动限制叠加的层数时可以使用
        /// </summary>
        /// <returns></returns>
        bool OnAddBuffCondition();
        /// <summary>
        /// 内部的Buff移除条件，默认为False，如需在内部处理移除Buff的逻辑可以使用，当该方法内返回True时，该Buff会被移除
        /// </summary>
        /// <returns></returns>
        bool OnRemoveBuffCondition();

        /// <summary>
        /// 除了可同时存在的Buff之外，同一Buff下，无论添加多少层，只要Buff存在，该Awake也仅只有第一次创建的时候调用。
        /// </summary>
        void OnBuffAwake();
        /// <summary>
        /// 每一次Buff启动或者叠加的时候都会调用
        /// </summary>
        void OnBuffStart();
		void OnBuffUpdate();
		void OnBuffFixedUpdate();
		void OnBuffLateUpdate();
        /// <summary>
        /// 每一次Buff移除的时候执行，如果Buff是叠加了多层的且开启了缓慢减少，则每次减少一层都会调用一次该方法
        /// </summary>
        void OnBuffRemove();
        /// <summary>
        /// 只有当该Buff完全销毁时才执行该方法。
        /// </summary>
        void OnBuffDestroy();
    }
    public abstract class BuffController : IBuffController
	{
		void IGlobalSign.Init()
		{

		}

		void IGlobalSign.Release()
		{
			UIBuffer = null;
			Player = null;
			Buffer = null;			
		}
		public static T CreateInstance<T>(IBuff buffer, GameObject Player) where T :class, IBuffController, new()
		{		
			var controller = GlobalObjectPools<T>.GlobalAllocation();
			controller.Buffer = buffer;
			controller.Player = Player;
			controller.BuffLayer = 0;
            LogKit.I("创建的控制器类型:" + typeof(T));
			OnReleasePairs[typeof(T)] = item => GlobalObjectPools<T>.GlobalRelease(item as T);
			return controller;
		}	

		public static bool Release(IBuffController controller,Type type) 
		{
			if (OnReleasePairs.TryGetValue(type, out var value))
			{
				return value.Invoke(controller);
			}

			LogKit.W("请检查BuffController是否使用框架提供的BuffController.CreateInstance创建,否则无法回收进对象池");
			return false;
		}

		private static Dictionary<Type,Func<IBuffController, bool>> OnReleasePairs = new Dictionary<Type, Func<IBuffController, bool>>();

        public IBuff Buffer { get; private set; }

		IBuff IBuffController.Buffer { get => Buffer; set => Buffer = value; }

		public string BuffKey => Buffer.GetBuffKey;

		public UIBuffer UIBuffer { get; internal set; }

		UIBuffer IBuffController.UIBuffer { get => UIBuffer;set => UIBuffer = value; }

        public int BuffLayer { get; internal set; }

		int IBuffController.BuffLayer { get => BuffLayer; set => BuffLayer = value; }	

        public GameObject Player { get; private set; }

		GameObject IBuffController.Player { get => Player; set => Player = value; }

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

				LogKit.W("试图访问设置成永久性的Buff中的时间流，请取消对该属性的调用!");

				return -1;
			}
			internal set
			{
                mRemainingTime = value;
			}
		}

		public float RemainingProgress
		{
			get => Buffer.SurvivalType == BuffSurvivalType.Timer ? Mathf.Clamp01(RemainingTime / Buffer.BuffTimer) : 1;
        }

		float IBuffController.RemainingTime { get => RemainingTime; set => RemainingTime = value; }

		private float mRemainingTime;
		
        public BuffController(IBuff buffer,GameObject player)
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

		private IArchitecture mArchitecture;

		public virtual IArchitecture Architecture
		{
			get 
			{
				if (mArchitecture == null)
				{
					mArchitecture = ArchitectureConstructor.Instance.Enquene(this);					
				}

				return mArchitecture;
			}
		}

        public IArchitecture GetArchitecture()
        {
			return Architecture;
        }
    }
}
