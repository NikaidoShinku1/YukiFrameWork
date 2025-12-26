///=====================================================
/// - FileName:      SkillKit.cs
/// - NameSpace:     YukiFrameWork.Skill
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/6/10 18:06:49
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
using XFABManager;
using System.Reflection;
using YukiFrameWork.Pools;
using YukiFrameWork.Extension;
using SkillContainer = System.Collections.Generic.Dictionary<YukiFrameWork.Skill.ISkillExecutor, System.Collections.Generic.Dictionary<string, YukiFrameWork.Skill.SkillController>>;
using System.Linq;
namespace YukiFrameWork.Skill
{
	public enum ReleaseSkillStatus
	{
		ExecutorCannotRelease,
        Releasing,
        DonotMeetTheCondition,
        OtherReleasing,
        InCooling,
        Success
    }

    public static class SkillKit
	{
		/// <summary>
		/// 运行中所有的技能信息
		/// </summary>
		private static Dictionary<string, ISkillData> runtime_allSkillDatas = new Dictionary<string, ISkillData>();

		/// <summary>
		/// 运行中所有的技能执行者信息
		/// </summary>
		private static SkillContainer runtime_allSkillControllers = new SkillContainer();
		private static ISkillLoader loader = null;
		public static void Init(string projectName)
		{
			Init(new ABManagerSkillLoader(projectName));			
		}

		public static void Init(ISkillLoader loader)
		{
			//每次初始化清空所有的数据信息
			runtime_allSkillDatas.Clear();
			runtime_allSkillControllers.Clear();
			SkillKit.loader = loader;		
		}

		[RuntimeInitializeOnLoadMethod]
		public static void Init_Update()
		{
			MonoHelper.Update_RemoveListener(Update);
			MonoHelper.FixedUpdate_RemoveListener(FixedUpdate);
			MonoHelper.LateUpdate_RemoveListener(LateUpdate);

			MonoHelper.Update_AddListener(Update);
			MonoHelper.FixedUpdate_AddListener(FixedUpdate);
			MonoHelper.LateUpdate_AddListener(LateUpdate);
		}

		private static List<ISkillExecutor> results = new List<ISkillExecutor>();

		private static void Update(MonoHelper _)
		{
			foreach (var item in runtime_allSkillControllers)
			{
				var player = item.Key;
				if (IsCheckExecutorDestroy(player))
				{
					results.Add(player);
					continue;
				}

				if (IsCheckExecutorDisable(player))
					continue;

				foreach (var controller in item.Value)
				{
					controller.Value.Update();
				}

			}
		}

        private static void FixedUpdate(MonoHelper _)
        {
            foreach (var item in runtime_allSkillControllers)
            {
                var player = item.Key;
                if (IsCheckExecutorDestroy(player))
                {                  
                    continue;
                }

                if (IsCheckExecutorDisable(player))
                    continue;

                foreach (var controller in item.Value)
                {
                    controller.Value.FixedUpdate();
                }

            }
        }

        private static void LateUpdate(MonoHelper _)
        {
            foreach (var item in runtime_allSkillControllers)
            {
                var player = item.Key;
                if (IsCheckExecutorDestroy(player))
                {                  
                    continue;
                }

                if (IsCheckExecutorDisable(player))
                    continue;

                foreach (var controller in item.Value)
                {
                    controller.Value.LateUpdate();
                }

            }

			if (results.Count == 0) return;

			foreach (var item in results)
			{
				if (runtime_allSkillControllers.ContainsKey(item))
				{
					item.RemoveAllSkills();
					runtime_allSkillControllers.Remove(item);
				}
			}
			results.Clear();
        }
     
		/// <summary>
		/// 加载技能配表
		/// </summary>
		/// <param name="path"></param>
		public static void LoadSkillDataBase(string path)
		{
			LoadSkillDataBase(loader.Load<SkillDataBase>(path));
		}
        /// <summary>
        /// 加载技能配表
        /// </summary>
        /// <param name="path"></param>
        public static void LoadSkillDataBase(SkillDataBase skillDataBase)
		{
			for (int i = 0; i < skillDataBase.SkillDataConfigs.Count; i++)
			{
				var skill = skillDataBase.SkillDataConfigs[i].Clone();
				if (skill == null) continue;
				AddSkillData(skill);
			}

			loader?.UnLoad(skillDataBase);
		}

		/// <summary>
		/// 添加新的技能配置
		/// </summary>
		/// <param name="skill"></param>
		public static void AddSkillData(ISkillData skill)
		{
			runtime_allSkillDatas.Add(skill.SkillKey, skill);           
        }             
       
		/// <summary>
		/// 获取技能数据
		/// </summary>
		/// <param name="skillKey"></param>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
        public static ISkillData GetSkillData(string skillKey) 
		{
			if (runtime_allSkillDatas.TryGetValue(skillKey, out var data))
				return data;

			throw new Exception("技能不存在,请检查是否加载技能到SkillKit中 SkillKey:" + skillKey);			
		}

		/// <summary>
		/// 异步加载
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static IEnumerator LoadSkillDataBaseAsync(string path)
		{
			bool completed = false;

			loader.LoadAsync<SkillDataBase>(path, data => 
			{
				LoadSkillDataBase(data);
				completed = true;
			});

			yield return CoroutineTool.WaitUntil(() => completed);
		}

        /// <summary>
        /// 根据一组技能配置标识为执行者添加一组技能
        /// </summary>
        /// <param name="player"></param>
        /// <param name="skillKeys"></param>
        /// <returns></returns>
        public static SkillController[] AddSkills(this ISkillExecutor player, params string[] skillKeys)
		{
			if (skillKeys == null || skillKeys.Length == 0) return null;
			ISkillData[] skillDatas = new ISkillData[skillKeys.Length];
			for (int i = 0; i < skillKeys.Length; i++)
			{
				skillDatas[i] = GetSkillData(skillKeys[i]);
				if (skillDatas[i] == null)
					Debug.LogWarning($"通过标识获取技能失败,请检查是否已经添加了技能信息 SkillKey:{skillKeys[i]}");
			}
			return AddSkills(player, skillDatas);
		}

		/// <summary>
		/// 根据一组技能配置为执行者添加一组技能
		/// </summary>
		/// <param name="player"></param>
		/// <param name="skills"></param>
		/// <returns></returns>
        public static SkillController[] AddSkills(this ISkillExecutor player, params ISkillData[] skills)
        {
			SkillController[] skillControllers = new SkillController[skills.Length];
			for (int i = 0; i < skills.Length; i++)
			{
				ISkillData skillData = skills[i];

				if (skillData == null) continue;
				SkillController controller = AddSkill(player, skillData);
				skillControllers[i] = controller;
			}
			return skillControllers;

        }

		/// <summary>
		/// 根据配置标识为执行者添加一个技能
		/// </summary>
		/// <param name="player"></param>
		/// <param name="skillKey"></param>
		/// <returns></returns>
        public static SkillController AddSkill(this ISkillExecutor player, string skillKey)
        {
			ISkillData skillData = GetSkillData(skillKey);
			return AddSkill(player, skillData);
        }

		/// <summary>
		/// 根据配置为执行者添加一个技能
		/// </summary>
		/// <param name="player"></param>
		/// <param name="skill"></param>
		/// <returns></returns>
		/// <exception cref="NullReferenceException"></exception>
		/// <exception cref="InvalidCastException"></exception>
		/// <exception cref="Exception"></exception>
        public static SkillController AddSkill(this ISkillExecutor player, ISkillData skill)
        {
			if (skill == null)
				throw new NullReferenceException("技能数据丢失，请检查是否添加!");
            Type cType = AssemblyHelper.GetType(skill.SkillControllerType);
            if (cType == null || !cType.IsSubclassOf(typeof(SkillController)))
                throw new InvalidCastException($"转换技能控制器失败，请检查技能{skill.SkillKey}的技能控制器类型是否正确 Type:{skill.SkillControllerType}");
			SkillController controller = SkillController.CreateInstance(player,skill,cType);

			if (!runtime_allSkillControllers.TryGetValue(player, out var dict))
				dict = new Dictionary<string, SkillController>() { { skill.SkillKey, controller } };
			else
			{
				if (dict.ContainsKey(skill.SkillKey))
					throw new Exception($"这个技能已经添加了! SkillKey:{skill.SkillKey}");
				dict.Add(skill.SkillKey, controller);
			}
			runtime_allSkillControllers[player] = dict;
			return controller;
        }

		/// <summary>
		/// 获取执行者持有的某一个技能
		/// </summary>
		/// <param name="player"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		public static SkillController GetSkillController(this ISkillExecutor player, string key)
		{
			if (runtime_allSkillControllers.TryGetValue(player, out var dict))
			{
				if (dict.TryGetValue(key, out var controller))
					return controller;
			}

			return null;
        }

        /// <summary>
        /// 释放技能
        /// </summary>
        /// <param name="player"></param>
        /// <param name="controller"></param>
        /// <returns></returns>
        public static ReleaseSkillStatus ReleaseSkill(this ISkillExecutor player, string skillKey, params object[] param)
		{
			var skills = GetSkillControllers(player);
			if (skills == null)
				throw new NullReferenceException("当前技能执行者没有添加任何技能,无法释放!");

            if (!skills.TryGetValue(skillKey, out SkillController controller))
            {
                throw new Exception("无法释放不存在的技能，请检查是否为该Handler添加指定的技能，SkillKey:" + skillKey);
            }

			//如果Player不能释放技能，则直接中断
			if (!player.IsCanRelease())
				return ReleaseSkillStatus.ExecutorCannotRelease;

            //如果这个技能是不允许释放，就直接中断,条件不满足
            if (!controller.IsCanRelease())
                return ReleaseSkillStatus.DonotMeetTheCondition;

            //如果技能正在释放，直接中断,技能正在释放
            if (controller.IsSkillRelease)
                return ReleaseSkillStatus.Releasing;

            //如果技能正在冷却，直接中断,技能正在冷却
            if (!controller.IsSkillCoolDown)
                return ReleaseSkillStatus.InCooling;

            //如果所有运行中技能的可同时释放技能标识里面没有该技能，则无法释放
            if (!CheckRuntimeSkillRelease(skills,skillKey))
                return ReleaseSkillStatus.OtherReleasing;

            controller.CoolDownTimer = 0;
            controller.ReleasingTimer = 0;
            controller.IsSkillCoolDown = false;
            controller.IsSkillRelease = true;
            controller.fixedTimer = 0;
            controller.OnRelease(param);
            return ReleaseSkillStatus.Success;
        }

		/// <summary>
		/// 获取执行者所有的技能
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
        public static Dictionary<string,SkillController> GetSkillControllers(this ISkillExecutor player)
        {
            if (!runtime_allSkillControllers.TryGetValue(player, out var dict) || dict.Count == 0)
                return null;
            return dict;
        }

		/// <summary>
		/// 获取执行者所有的技能
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
        public static SkillController[] GetSkillControllersToArray(this ISkillExecutor player)
		{
			return GetSkillControllers(player).Values.ToArray();
		}

        /// <summary>
        /// 检查所有正在释放的技能的可同时释放标识中是否包含指定技能
        /// </summary>
        /// <returns></returns>
        internal static bool CheckRuntimeSkillRelease(Dictionary<string,SkillController> skills,string skillKey)
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


        /// <summary>
        /// 取消技能的释放(拓展)
		/// <para>与打断技能释放的API不同的是，该API会受到配置中是否能够主动取消技能的选项影响，运行时可在Controller中修改属性(非配表)</para>
        /// </summary>
        /// <param name="player"></param>
        /// <param name="controller"></param>
        public static void CancelSkill(this ISkillExecutor player, string skillKey)
		{
			var skills = GetSkillControllers(player);
            if (!skills.TryGetValue(skillKey, out SkillController controller))
            {
                throw new Exception("不存在的技能,无法取消，请检查是否为该Handler添加指定的技能，SkillKey:" + skillKey);
            }

            if (controller.ActiveCancellation && controller.IsSkillRelease)
            {
                Interruption(player,controller);
            }
        }

        /// <summary>
        /// 打断技能的释放
        /// </summary>
        /// <param name="skillKey"></param>
        /// <exception cref="Exception"></exception>
        public static void InterruptionSkill(this ISkillExecutor player,string skillKey)
        {
			var skills = GetSkillControllers(player);
            if (!skills.TryGetValue(skillKey, out SkillController controller))
            {
                throw new Exception("不存在的技能,无法打断，请检查是否为该Executor添加指定的技能，SkillKey:" + skillKey);
            }
            Interruption(player, controller);
        }

		/// <summary>
		/// 打断执行者所有的技能释放
		/// </summary>
		/// <param name="player"></param>
		public static void InterruptionAllSkills(this ISkillExecutor player)
		{
			if (IsCheckExecutorDestroy(player)) return;

			if (!runtime_allSkillControllers.TryGetValue(player, out var dict)) return;

			foreach (var item in dict.Values)
			{
				item.Interrput();
			}
		}

		/// <summary>
		/// 执行者是否正在释放技能
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
        public static bool IsReleasingSkills(this ISkillExecutor player)
        {
            if (IsCheckExecutorDestroy(player)) return false;

			if (!runtime_allSkillControllers.TryGetValue(player, out var dict))
				return false;

			foreach (var item in dict.Values)
			{
				if (item.IsSkillRelease)
					return true;
			}
            return false;
        }

        private static void Interruption(ISkillExecutor player,SkillController controller)
        {
            if (!controller.IsSkillRelease || IsCheckExecutorDestroy(player)) return;
			controller.Interrput();
        }

		/// <summary>
		/// 检查执行者是否被销毁
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		private static bool IsCheckExecutorDestroy(ISkillExecutor player)
		{
			if (player is UnityEngine.Object obj)
			{
				return !obj;
			}

			return false;
		}

		/// <summary>
		/// 检查执行者是否被隐藏
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		private static bool IsCheckExecutorDisable(ISkillExecutor player)
		{
            if (player is Component component)
            {
                return !component.ActiveInHierarchy();
            }

            return false;
        }

     
		
		/// <summary>
		/// 重置技能冷却时间
		/// </summary>
		/// <param name="player"></param>
		/// <param name="controller"></param>
		public static void ResetSkillCoolingTime(this ISkillExecutor player, string skillKey)
		{
			SkillController controller = GetSkillController(player, skillKey);

			if (controller == null) return;
			controller.IsSkillCoolDown = true;
		}

		/// <summary>
		/// 重置所有技能的冷却时间
		/// </summary>
		/// <param name="player"></param>
		public static void ResetAllSkillsCoolingTime(this ISkillExecutor player)
		{
			var dict = GetSkillControllers(player);

			foreach (var item in dict.Values)
			{
				item.IsSkillCoolDown = true;
			}
		}

		/// <summary>
		/// 删除技能
		/// </summary>
		/// <param name="player"></param>
		/// <param name="controller"></param>
		public static void RemoveSkill(this ISkillExecutor player, string skillKey)
		{
			if (!runtime_allSkillControllers.TryGetValue(player, out var dict)) return;
			if (!dict.TryGetValue(skillKey, out var controller))
				return;
			controller.OnDestroy();
			dict.Remove(skillKey);
			runtime_allSkillControllers[player] = dict;
		}
        /// <summary>
        /// 删除一组技能
        /// </summary>
        /// <param name="player"></param>
        /// <param name="controller"></param>
        public static void RemoveSkills(this ISkillExecutor player, params string[] skillKeys)
		{
			if (skillKeys == null || skillKeys.Length == 0) return;

			for (int i = 0; i < skillKeys.Length; i++)
			{
				RemoveSkill(player,skillKeys[i]);
			}
		}

		/// <summary>
		/// 删除执行者所有的技能
		/// </summary>
		/// <param name="player"></param>
		public static void RemoveAllSkills(this ISkillExecutor player)
		{
            if (!runtime_allSkillControllers.TryGetValue(player, out var dict)) 
				return;
			foreach (var item in dict.Values)			
				item.OnDestroy();

			runtime_allSkillControllers[player].Clear();
        }
    }
}
