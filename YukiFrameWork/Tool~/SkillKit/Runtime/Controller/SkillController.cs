///=====================================================
/// - FileName:      SkillController.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/6/10 13:07:10
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using System.Collections.Generic;
using YukiFrameWork.Pools;
using System.Linq;
namespace YukiFrameWork.Skill
{
    public abstract class SkillController : IController,IGlobalSign
    {
        void IGlobalSign.Init()
        {
            
        }

        bool IGlobalSign.IsMarkIdle { get; set; }

        void IGlobalSign.Release()
        {
            Player = null;
            SkillData = null;
            IsSkillRelease = false;
            IsSkillCoolDown = false;
            onCooling = null;
            onReleaseComplete = null;
            onReleasing = null;
            onCoolingComplete = null;
            onInterrup = null;
            fixedTimer = 0;
            CoolDownTime = 0;
            ReleaseTime = 0;
            ActiveCancellation = false;
            SimultaneousSkillKeys.Clear();
        }

        /// <summary>
        /// 技能的执行者
        /// </summary>
        public ISkillExecutor Player { get;internal set; }     

        /// <summary>
        /// 技能的配置数据
        /// </summary>
        public ISkillData SkillData { get; set; }    

        /// <summary>
        /// 技能是否正在释放
        /// </summary>
        public bool IsSkillRelease { get; internal set; }

        private float releaseTime;
        /// <summary>
        /// 技能释放时间
        /// </summary>
        public virtual float ReleaseTime
        {
            get => releaseTime;
            set => releaseTime = value;
        }
        
        /// <summary>
        /// 技能释放计时
        /// </summary>
        public float ReleasingTimer { get;internal set; }

        public List<string> SimultaneousSkillKeys { get; set; } = new List<string>();

        internal float fixedTimer;

        private Dictionary<string, SkillParam> skillParams = new Dictionary<string, SkillParam>();

        /// <summary>
        /// 技能所使用的参数(控制器可动态修改)
        /// </summary>
        public Dictionary<string, SkillParam> SkillParams => skillParams;

        public float ReleasingProgress
        {
            get
            {
                if (SkillData == null)
                    return 0;

                if (SkillData.IsInfiniteTime)
                    return 1;

                return Mathf.Clamp01(ReleasingTimer / ReleaseTime);
            }
        }

        private bool isSkillCoolDown;
        public bool IsSkillCoolDown
        {
            get => isSkillCoolDown;
            set
            {
                if (isSkillCoolDown == value) return;
                isSkillCoolDown = value;
                CoolDownTimer = 0;
                if (isSkillCoolDown)
                {                                   
                    onCoolingComplete?.Invoke(this);
                    OnCoolingComplete();
                }

            }
        }

        private float coolDownTime;
        /// <summary>
        /// 技能冷却时间
        /// </summary>
        public virtual float CoolDownTime
        {
            get => coolDownTime;
            set => coolDownTime = value;
        }

        /// <summary>
        /// 技能冷却计时
        /// </summary>
        public float CoolDownTimer { get; internal set; }

        /// <summary>
        /// 是否能够主动取消
        /// </summary>
        public bool ActiveCancellation { get; set; }      

        public float CoolDownProgress
        {
            get
            {
                if (SkillData == null)
                    return 0;

                return Mathf.Clamp01(CoolDownTimer / CoolDownTime);
            }
        }

        /// <summary>
        /// 当前技能状态
        /// </summary>
        public ReleaseSkillStatus ReleaseSkillStatus
        {
            get
            {
                if(!Player.IsCanRelease())
                    return ReleaseSkillStatus.ExecutorCannotRelease;
                if (!IsCanRelease())
                    return ReleaseSkillStatus.DonotMeetTheCondition;

                if (IsSkillRelease)
                    return ReleaseSkillStatus.Releasing;

                if (!IsSkillCoolDown)
                    return ReleaseSkillStatus.InCooling;

                if (!SkillKit.CheckRuntimeSkillRelease(SkillKit.GetSkillControllers(Player), SkillData.SkillKey))
                    return ReleaseSkillStatus.OtherReleasing;

                return ReleaseSkillStatus.Success;
            }
        }

        #region EventTrigger    
        public event Action<SkillController> onStartCooling;
        public event Action<SkillController, float> onCooling; 
        public event Action<SkillController,float> onReleasing; 
        public event Action<SkillController> onCoolingComplete; 
        public event Action<SkillController> onReleaseComplete;
        public event Action<SkillController> onInterrup;
        
        #endregion        

        internal static bool ReleaseController(SkillController controller)
        {
            return GlobalObjectPools.GlobalRelease(controller);
        }

        internal static SkillController CreateInstance(ISkillExecutor executor,ISkillData skill, Type type)
        {
            if (!type.IsSubclassOf(typeof(SkillController))) throw new InvalidCastException($"技能控制器类型不正确 Type:{type}");
            SkillController controller = GlobalObjectPools.GlobalAllocation(type) as SkillController;
            controller.Player = executor;
            controller.SkillData = skill;
            controller.coolDownTime = skill.CoolDownTime;
            controller.releaseTime = skill.ReleaseTime;
            controller.IsSkillCoolDown = true;
            if (skill.SkillParams != null && skill.SkillParams.Length > 0)
                controller.skillParams = skill.SkillParams.ToDictionary(x => x.paramKey,x => x);
            controller.ActiveCancellation = skill.ActiveCancellation;
            if(skill.SimultaneousSkillKeys != null && skill.SimultaneousSkillKeys.Length != 0)
                controller.SimultaneousSkillKeys = skill.SimultaneousSkillKeys.ToList();
            controller.OnAwake();
            return controller;
        }

        /// <summary>
        /// 技能是否能被释放(默认返回True)
        /// </summary>
        /// <returns></returns>
        public virtual bool IsCanRelease()
        {
            return true;
        }

        /// <summary>
        /// 技能是否能够完成(默认返回True)
        /// <para>Tips:当技能不受时间限制，需要重写该属性自行定义对技能的结束</para>
        /// </summary>
        /// <returns></returns>
        public virtual bool IsComplete()
        {
            return true;
        }

        /// <summary>
        /// 技能构建触发
        /// </summary>
        public abstract void OnAwake();
        
        /// <summary>
        /// 技能冷却触发
        /// </summary>
        public virtual void OnCooling()
        {
            
        }

        /// <summary>
        /// 技能冷却完成触发
        /// </summary>
        public virtual void OnCoolingComplete()
        {
            
        }

        /// <summary>
        /// 技能销毁触发
        /// </summary>
        public abstract void OnDestroy();     

        public virtual void OnFixedUpdate()
        {
            
        }        
        /// <summary>
        /// 技能被打断触发
        /// </summary>
        public virtual void OnInterruption()
        {
            
        }     

        public virtual void OnLateUpdate()
        {
           
        }

        /// <summary>
        /// 技能释放
        /// </summary>
        /// <param name="param"></param>
        public virtual void OnRelease(params object[] param)
        {
            
        }

        /// <summary>
        /// 技能释放完成
        /// </summary>
        public virtual void OnReleaseComplete()
        {
            
        }

        public virtual void OnUpdate()
        {
            
        }
        #region Internal
        internal void Update()
        {

            if (IsSkillRelease)
            {
                ReleasingTimer += Time.deltaTime;
                onReleasing?.Invoke(this, ReleasingProgress);
                OnUpdate();

                if ((ReleasingTimer >= ReleaseTime || SkillData.IsInfiniteTime) && IsComplete())
                {
                    ReleasingTimer = 0;
                    IsSkillRelease = false;
                    onReleaseComplete?.Invoke(this);
                    OnReleaseComplete();

                }
            }
            else
            {
                if (!IsSkillCoolDown)
                {
                    if (CoolDownTimer == 0)
                    {
                        onStartCooling?.Invoke(this);
                    }
                    CoolDownTimer += Time.deltaTime;
                    onCooling?.Invoke(this, CoolDownProgress);
                    OnCooling();

                    if (CoolDownTimer >= CoolDownTime)
                    {
                        IsSkillCoolDown = true;                       
                    }
                }
            }
        }

        internal void FixedUpdate()
        {
            if (IsSkillRelease)
            {
                //防止卡顿导致技能在FixedUpdate执行达不到预期
                if (fixedTimer - ReleasingTimer > Time.fixedDeltaTime * 2)
                    return;
                fixedTimer += Time.fixedDeltaTime;

                OnFixedUpdate();
            }
        }

        internal void LateUpdate()
        {
            if (IsSkillRelease)
            {
                OnLateUpdate();
            }
        }

        internal void Interrput()
        {
            if (!IsSkillRelease) return;
            IsSkillRelease = false;
            ReleasingTimer = 0;
            onInterrup?.Invoke(this);
            OnInterruption();
            onReleaseComplete?.Invoke(this);
            OnReleaseComplete();
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
    }
}
