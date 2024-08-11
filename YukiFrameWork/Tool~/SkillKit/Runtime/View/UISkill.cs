///=====================================================
/// - FileName:      UISkill.cs
/// - NameSpace:     YukiFrameWork.Skill
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/6/11 21:46:00
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using UnityEngine.UI;
namespace YukiFrameWork.Skill
{
	public abstract class UISkill : YMonoBehaviour
	{
		internal string GetSkillKey;    

        public ISkillData SkillData => SkillKit.GetSkillDataByKey(GetSkillKey);

        /// <summary>
        /// UI同步技能释放
        /// </summary>
        public abstract void OnRelease();

        /// <summary>
        /// UI同步技能完成释放
        /// </summary>
        public abstract void OnReleaseComplete();

        /// <summary>
        /// UI同步技能释放持续检测
        /// </summary>
        /// <param name="releasingTime"></param>
        /// <param name="releasingProgress"></param>
        public abstract void OnReleaseUpdate(float releasingTime, float releasingProgress);

        public virtual void OnReleaseFixedUpdate(float releasingTime, float releasingProgress) { }

        public virtual void OnReleaseLateUpdate(float releasingTime, float releasingProgress) { }

        /// <summary>
        /// UI同步冷却释放持续检测
        /// </summary>
        /// <param name="coolDownTime"></param>
        /// <param name="coolDownProgress"></param>
        public abstract void OnCoolingUpdate(float coolDownTime, float coolDownProgress);

        /// <summary>
        /// UI同步技能冷却结束
        /// </summary>
        public abstract void OnCoolingComplete();

        /// <summary>
        /// 技能等级改变时触发
        /// </summary>
        /// <param name="level"></param>
        public virtual void OnSkillLevelChanged(int level) { }
    }
}
