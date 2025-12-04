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
using System.Linq;
namespace YukiFrameWork.Buffer
{
    public abstract class BuffController : IGlobalSign,IController
	{
        #region IGlobalSign
        public bool IsMarkIdle { get ; set ; }

        void IGlobalSign.Init()
		{
            ExistedTime = 0;
            fixedTimer = 0;
        }	      
		void IGlobalSign.Release()
		{          
			Player = null;                  
            ExistedTime = 0;
            fixedTimer = 0;			
		}
        #endregion

        #region Architecture
        private object _object = new object();
        private IArchitecture mArchitecture;

        /// <summary>
        /// 可重写的架构属性,不使用特性初始化时需要重写该属性
        /// </summary>
        protected virtual IArchitecture RuntimeArchitecture
        {
            get
            {
                lock (_object)
                {
                    if (mArchitecture == null)
                        Build();
                    return mArchitecture;
                }
            }
        }
        IArchitecture IGetArchitecture.GetArchitecture()
        {
            return RuntimeArchitecture;
        }

        internal void Build()
        {
            if (mArchitecture == null)
            {
                mArchitecture = ArchitectureConstructor.I.Enquene(this);
            }
        }
        #endregion

        private float duration;
		private float fixedTimer;
        private Dictionary<string, BuffParam> buffParams;

        /// <summary>
        /// Buff执行者
        /// </summary>
        public IBuffExecutor Player { get;private set; }

		/// <summary>
		/// Buff的配置
		/// </summary>
		public IBuff Buff { get; private set; }
      
        /// <summary>
        /// Buff的可使用参数
        /// </summary>
        public Dictionary<string, BuffParam> BuffParams => buffParams;

		/// <summary>
		/// Buff添加后过的时间
		/// </summary>
		public float ExistedTime { get;private set; }

        /// <summary>
        /// Buff的持续时间,默认根据配表，小于0则为无限时间，可重写
        /// </summary>
        public virtual float Duration
		{
			get => duration;
			set
			{
                duration = value;
            }
		}

		/// <summary>
		/// Buff的进度,如果Buff是无限时间的，则该属性为-1
		/// </summary>
		public float Progress
		{
			get
			{
				if (Duration < 0)
					return -1;
				return ExistedTime / Duration;
			}
		}

        /// <summary>
        /// 当前Buff是否满足被添加的条件，如果该方法返回False，则该Buff无法被任何Buff执行者添加
        /// </summary>
        /// <returns></returns>
        public virtual bool OnAddBuffCondition() => true;

        internal void Update()
        {
                ExistedTime += Time.deltaTime;
            OnUpdate();
        }

        internal void FixedUpdate()
        {
            if (fixedTimer > ExistedTime - Time.fixedDeltaTime * 2)
                return;

            fixedTimer += Time.fixedDeltaTime;
            OnFixedUpdate();
        }

        internal void LateUpdate()
        {
            OnLateUpdate();
        }

        internal void Add(params object[] param)
        {
            OnAdd(param);
        }

        internal void Remove() => OnRemove();

        /// <summary>
        /// 当Buff添加时调用
        /// </summary>
        /// <param name="param"></param>
        protected virtual void OnAdd(params object[] param) { }

        protected virtual void OnUpdate() { }

        protected virtual void OnFixedUpdate() { }

        protected virtual void OnLateUpdate() { }

        /// <summary>
        /// 当Buff销毁(移除)时调用
        /// </summary>
        protected virtual void OnRemove() { }

        internal static BuffController CreateInstance(Type buffType,IBuff buff,IBuffExecutor buffExecutor) 
		{
			if (!buffType.IsSubclassOf(typeof(BuffController)))
				throw new InvalidCastException($"类型错误,传递的类型{buffType}并不是BuffController的派生");
			BuffController controller = GlobalObjectPools.GlobalAllocation(buffType) as BuffController;
			controller.Buff = buff;
			controller.Player = buffExecutor;
            controller.duration = buff.Duration;
            if(buff.BuffParams != null && buff.BuffParams.Length > 0)
                controller.buffParams = buff.BuffParams.ToDictionary(x => x.paramKey,x => x);
            return controller;
		}
    }
}
