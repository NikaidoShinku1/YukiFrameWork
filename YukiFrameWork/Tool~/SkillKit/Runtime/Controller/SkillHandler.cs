﻿///=====================================================
/// - FileName:      SkillHandler.cs
/// - NameSpace:     YukiFrameWork.Skill
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/6/10 15:13:34
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using UnityEngine;
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using System.Linq;
using UnityEngine.Events;
using YukiFrameWork.Pools;
namespace YukiFrameWork.Skill
{
    public enum ReleaseSkillStatus
    {
        /// <summary>
        /// 释放成功
        /// </summary>
        [LabelText("释放成功")]
        Success,
        /// <summary>
        /// 释放失败,该技能正在释放中
        /// </summary>
        [LabelText("释放失败,该技能正在释放中")]
        Releasing,
        /// <summary>
        /// 释放失败,有其他的不能同时释放的技能正在释放
        /// </summary>
        [LabelText("释放失败,有其他的不能同时释放的技能正在释放")]
        OtherReleasing,
        /// <summary>
        /// 释放失败,当前技能释放条件不满足
        /// </summary>
        [LabelText("释放失败,当前技能释放条件不满足")]
        DonotMeetTheCondition,       
        /// <summary>
        /// 释放失败,该技能正在冷却中
        /// </summary>
        [LabelText("释放失败,该技能正在冷却中")]
        InCooling,       
    }
    [DisableViewWarning]
    public class SkillHandler : MonoBehaviour
    {       
        /// <summary>
        /// Key:技能标识
        /// Value:技能对应的控制器
        /// </summary>
        private Dictionary<string, SkillController> skills = new Dictionary<string, SkillController>();       

        /// <summary>
        /// 添加技能时触发的回调，可以拿到技能控制器
        /// </summary>
        [LabelText("添加技能时触发的回调，可以拿到技能控制器")]
        public UnityEvent<SkillController> onAddSkillEvent = new UnityEvent<SkillController>();

        /// <summary>
        /// 移除技能时触发的回调，可以拿到技能控制器
        /// </summary>
        [LabelText(" 移除技能时触发的回调，可以拿到技能控制器")]
        public UnityEvent<SkillController> onRemoveSkillEvent = new UnityEvent<SkillController>();     

        /// <summary>
        /// 添加多个技能
        /// </summary>
        /// <param name="controllers"></param>
        public void AddSkills(ISkillExecutor player,params string[] skillKeys)
        {
            if (player == null || skillKeys == null || skillKeys.Length == 0) return;          
            for (int i = 0; i < skillKeys.Length; i++)
            {
                AddSkill(player, SkillKit.GetSkillDataByKey(skillKeys[i]));
            }
        }

        public void AddSkills(ISkillExecutor player, params ISkillData[] skills)
        {
            if (player == null || skills == null || skills.Length == 0) return;
            for (int i = 0; i < skills.Length; i++)
            {
                AddSkill(player, skills[i]);
            }
        }


        /// <summary>
        /// 添加技能
        /// </summary>
        /// <typeparam name="T">技能控制器类型</typeparam>
        /// <param name="skill">技能信息</param>
        /// <param name="executor">技能执行者</param>
        public SkillController AddSkill(ISkillExecutor player, ISkillData skill)
        {
            if (skills.ContainsKey(skill.SkillKey))
            {
#if YukiFrameWork_DEBUGFULL
                LogKit.E("该技能已经被添加,试图重复添加!");
                return skills[skill.SkillKey];
#endif
            }
            var controller = CreateInstance(skill,player);
            Add(controller);
            return controller;
        }

        private SkillController CreateInstance(ISkillData skillData,ISkillExecutor player)
        {
            var skillController = SkillKit.CreateSkillController(skillData.SkillKey);
            skillController.Player = player;
            skillController.SkillData = skillData;
            skillController.IsSkillCoolDown = true;
            if (skillData.SimultaneousSkillKeys != null || skillData.SimultaneousSkillKeys.Length > 0)
                skillController.SimultaneousSkillKeys.AddRange(skillData.SimultaneousSkillKeys);
            return skillController;
        }

        private void Add(SkillController controller)
        {
            skills[controller.SkillData.SkillKey] = controller;         
            controller.OnAwake();
            onAddSkillEvent?.Invoke(controller);       
        }

        public SkillController AddSkill(ISkillExecutor player, string skillKey)
        {
            ISkillData skill = SkillKit.GetSkillDataByKey(skillKey);
            if (skill == null)
            {
#if YukiFrameWork_DEBUGFULL
                LogKit.E("技能丢失，请检查是否已经将技能全部加载到SkillKit中");
#endif
                return null;
            }
            return AddSkill(player, skill);
        }

        /// <summary>
        /// 获取已经添加的的技能
        /// </summary>
        /// <param name="skillKey"></param>
        /// <returns></returns>
        public SkillController GetSkillController(string skillKey)
        {
            skills.TryGetValue(skillKey, out var controller);
            return controller;
        }

        public ReleaseSkillStatus ReleaseSkill(string skillKey, params object[] param)
        {
            if (!skills.TryGetValue(skillKey, out SkillController controller))
            {
                throw new Exception("无法释放不存在的技能，请检查是否为该Handler添加指定的技能，SkillKey:" + skillKey);
            }

            //如果这个技能是不允许释放的或者Player本身就禁止释放技能，就直接中断,条件不满足
            if (!controller.IsCanRelease() || !controller.Player.IsCanRelease())
                return ReleaseSkillStatus.DonotMeetTheCondition;

            //如果技能正在释放，直接中断,技能正在释放
            if (controller.IsSkillRelease)
                return ReleaseSkillStatus.Releasing;

            //如果技能正在冷却，直接中断,技能正在冷却
            if (!controller.IsSkillCoolDown)
                return ReleaseSkillStatus.InCooling;           

            //如果所有运行中技能的可同时释放技能标识里面没有该技能，则无法释放
            if (!CheckRuntimeSkillRelease(skillKey))
                return ReleaseSkillStatus.OtherReleasing;        

            controller.CoolDownTime = 0;
            controller.ReleasingTime = 0;
            controller.IsSkillCoolDown = false;
            controller.IsSkillRelease = true;
            controller.fixedTimer = 0;
            controller.OnRelease(param);          
            return ReleaseSkillStatus.Success;
        }

        public void RemoveSkill(string skillKey)
        {
            if (!skills.TryGetValue(skillKey, out SkillController controller))
            {
                return;
            }

            Remove(controller);
        }

          

        private void Remove(SkillController controller)
        {           
            Interruption(controller);
            //触发销毁方法
            controller.OnDestroy();
            skills.Remove(controller.SkillData.SkillKey);
            onRemoveSkillEvent?.Invoke(controller);         
            SkillController.ReleaseController(controller);
        }

        public void RemoveAllSkills()
        {
            SkillController[] controllers = skills.Values.ToArray();

            for (int i = 0; i < controllers.Length; i++)
            {
                Remove(controllers[i]);
            }
        }

        /// <summary>
        /// 重置技能的冷却时间
        /// </summary>
        /// <param name="skillKey"></param>
        public void ResetSkillCoolingTime(string skillKey)
        {
            if (!skills.TryGetValue(skillKey, out SkillController controller))
            {
                throw new Exception("无法释放不存在的技能，请检查是否为该Handler添加指定的技能，SkillKey:" + skillKey);
            }

            controller.CoolDownTime = 0;
        }

        /// <summary>
        /// 重置技能释放时间为0
        /// </summary>
        /// <param name="skillKey"></param>
        /// <exception cref="Exception"></exception>
        public void ResetSkillReleasingTime(string skillKey)
        {
            if (!skills.TryGetValue(skillKey, out SkillController controller))
            {
                throw new Exception("无法释放不存在的技能，请检查是否为该Handler添加指定的技能，SkillKey:" + skillKey);
            }

            controller.ReleasingTime = 0;
        }

        /// <summary>
        /// 取消技能的释放
        /// </summary>
        /// <param name="skillKey"></param>
        /// <exception cref="Exception"></exception>
        public void CancelSkill(string skillKey)
        {
            if (!skills.TryGetValue(skillKey, out SkillController controller))
            {
                throw new Exception("不存在的技能,无法取消，请检查是否为该Handler添加指定的技能，SkillKey:" + skillKey);
            }

            if (controller.SkillData.ActiveCancellation && controller.IsSkillRelease)
            {
                Interruption(controller);
            }
        }

        /// <summary>
        /// 打断所有的技能
        /// </summary>
        public void InterruptionAllSkills()
        {
            foreach (var item in skills.Values)
            {
                Interruption(item);
            }
        }

        /// <summary>
        /// 该角色当前是否正在释放技能
        /// </summary>
        public bool IsReleasingSkill
        {
            get
            {
                foreach (var item in skills.Values)
                {
                    if (item.IsSkillRelease) return true;
                }

                return false;
            }
        }

        /// <summary>
        /// 打断技能的释放
        /// </summary>
        /// <param name="skillKey"></param>
        /// <exception cref="Exception"></exception>
        public void InterruptionSkill(string skillKey)
        {
            if (!skills.TryGetValue(skillKey, out SkillController controller))
            {
                throw new Exception("不存在的技能,无法打断，请检查是否为该Handler添加指定的技能，SkillKey:" + skillKey);
            }

            Interruption(controller);
        }

        private void Interruption(SkillController controller)
        {
            if (!controller.IsSkillRelease || !this) return;

            controller.IsSkillRelease = false;
            controller.ReleasingTime = 0;
            controller.onInterrup?.Invoke(controller);
            controller.OnInterruption();
            controller.onReleaseComplete?.Invoke(controller);
            controller.OnReleaseComplete();
          
        }

        /// <summary>
        /// 检查所有正在释放的技能的可同时释放标识中是否包含指定技能
        /// </summary>
        /// <returns></returns>
        private bool CheckRuntimeSkillRelease(string skillKey)
        {
            bool contains = true;
            foreach (var item in skills.Values)
            {                            
                if (!item.IsSkillRelease) 
                    continue;

                contains = false;

                List<string> simultaneousSkillKeys = item.SimultaneousSkillKeys;

                if (simultaneousSkillKeys == null || simultaneousSkillKeys.Count == 0) 
                {                   
                    continue;
                }

                for (int i = 0; i < simultaneousSkillKeys.Count; i++)
                {
                    if (skillKey == simultaneousSkillKeys[i])
                        return true;
                }             
            }

            return contains;
        }

        public SkillController[] GetAllSkillControllers() => skills.Values.ToArray();

        private void Update()
        {
            OnUpdateSetting(UpdateStatus.OnUpdate);
        }
       
        private void FixedUpdate()
        {
            OnUpdateSetting(UpdateStatus.OnFixedUpdate);
        }

        private void LateUpdate()
        {
            OnUpdateSetting(UpdateStatus.OnLateUpdate);
        }

        private void OnUpdateSetting(UpdateStatus updateStatus)
        {
            foreach (var controller in skills.Values)
            {
                ISkillData skillData = controller.SkillData;
                switch (updateStatus)
                {
                    case UpdateStatus.OnUpdate:
                        {
                            if (controller.IsSkillRelease)
                            {
                                controller.ReleasingTime += Time.deltaTime;
                                controller.onReleasing?.Invoke(controller,controller.ReleasingProgress);
                                controller.OnUpdate();
                               
                                if ((controller.ReleasingTime >= skillData.RealeaseTime || skillData.IsInfiniteTime) && controller.IsComplete())
                                {
                                    controller.ReleasingTime = 0;
                                    controller.IsSkillRelease = false;
                                    controller.onReleaseComplete?.Invoke(controller);
                                    controller.OnReleaseComplete();
                                    
                                }
                            }
                            else
                            {
                                if (!controller.IsSkillCoolDown)
                                {
                                    if (controller.CoolDownTime == 0)
                                    {
                                        controller.onStartCooling?.Invoke(controller);
                                    }
                                    controller.CoolDownTime += Time.deltaTime;
                                    controller.onCooling?.Invoke(controller, controller.CoolDownProgress);
                                    controller.OnCooling();                              

                                    if (controller.CoolDownTime >= skillData.CoolDownTime)
                                    {
                                        controller.IsSkillCoolDown = true;
                                        controller.CoolDownTime = 0;
                                        controller.onCoolingComplete?.Invoke(controller);
                                        controller.OnCoolingComplete();
                                        
                                    }
                                }
                            }
                        }
                        break;
                    case UpdateStatus.OnFixedUpdate:
                        {
                            if (controller.IsSkillRelease)
                            {
                                //防止卡顿导致技能在FixedUpdate执行达不到预期
                                if (controller.fixedTimer - controller.ReleasingTime > Time.fixedDeltaTime * 2)
                                    return;
                                controller.fixedTimer += Time.fixedDeltaTime;

                                controller.OnFixedUpdate();                       
                            }
                        }
                        break;
                    case UpdateStatus.OnLateUpdate:
                        {
                            if (controller.IsSkillRelease)
                            {
                                controller.OnLateUpdate();                       
                            }
                        }
                        break;

                }
            }
        }
        
        private void OnDestroy()
        {         
            onAddSkillEvent.RemoveAllListeners();
            onRemoveSkillEvent.RemoveAllListeners();
            RemoveAllSkills();
        }
    }
}
