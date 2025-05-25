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
namespace YukiFrameWork.Skill
{
    public abstract class SkillController : AbstractController,IGlobalSign
    {
        void IGlobalSign.Init()
        {
            OnInit();
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
            onLevelChanged = null;
            fixedTimer = 0;
            SimultaneousSkillKeys.Clear();
        }

        public ISkillExecutor Player { get;internal set; }

        public SkillHandler Handler => Player.Handler;      

        public ISkillData SkillData { get; set; }    

        public bool IsSkillRelease { get; internal set; }

        public float ReleasingTime { get; internal set; }           

        public List<string> SimultaneousSkillKeys { get; set; } = new List<string>();

        internal float fixedTimer;

        public float ReleasingProgress
        {
            get
            {
                if (SkillData == null)
                    return 0;

                if (SkillData.IsInfiniteTime)
                    return 1;

                return ReleasingTime / SkillData.RealeaseTime;
            }
        }

        public bool IsSkillCoolDown { get;internal set; } = true;

        public float CoolDownTime { get;internal set; }

        public float CoolDownProgress
        {
            get
            {
                if (SkillData == null)
                    return 0;

                return CoolDownTime / SkillData.CoolDownTime;
            }
        }
        #region EventTrigger      
        public Action<float> onCooling { get; set; }
        public Action<float> onReleasing { get; set; }
        public Action onCoolingComplete { get; set; }
        public Action onReleaseComplete { get; set; }
        public Action<int> onLevelChanged { get; set; }  
        
        #endregion        

        public static bool ReleaseController(SkillController controller)
        {
            return GlobalObjectPools.GlobalRelease(controller);
        }

        public virtual bool IsCanRelease()
        {
            return true;
        }

        public virtual bool IsComplete()
        {
            return true;
        }

        public abstract void OnAwake();
        
        public virtual void OnCooling()
        {
            
        }

        public virtual void OnCoolingComplete()
        {
            
        }

        public abstract void OnDestroy();     

        public virtual void OnFixedUpdate()
        {
            
        }

        /// <summary>
        /// 这个Init方法会在Awake之前执行
        /// </summary>
        public override void OnInit()
        {
            
        }

        public virtual void OnInterruption()
        {
            
        }     

        public virtual void OnLateUpdate()
        {
           
        }

        public virtual void OnRelease(params object[] param)
        {
            
        }

        public virtual void OnReleaseComplete()
        {
            
        }

        public virtual void OnUpdate()
        {
            
        }
    }
}
