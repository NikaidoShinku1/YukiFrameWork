﻿///=====================================================
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
namespace YukiFrameWork.Skill
{
	internal class SkillBindder
	{
		public ISkillData skillData;
		public Type controllerType;
	}
	public static class SkillKit
	{
		private static ISkillLoader loader = null;
		public static void InitLoader(string projectName)
		{
			isDefault = true;
			loader = new ABManagerSkillLoader(projectName);
			Init();
		}

		public static void InitLoader(ISkillLoader loader)
		{
			isDefault = false;
			SkillKit.loader = loader;
			Init();
		}
		private static bool isInited = false;
		private static void Init()
		{
			if (isInited) return;
			MonoHelper.Destroy_AddListener(_ => 
			{
				skills.Clear();

				if (isDefault)
				{
					foreach (var skill in skills.Values)
					{
						AssetBundleManager.UnloadAsset(skill);
					}
				}

				skills.Clear();
			});
			isInited = true;
		}

		private static bool isDefault = false;

		/// <summary>
		/// 保存所有的技能信息
		/// </summary>
		private static Dictionary<string, SkillBindder> skills = new Dictionary<string, SkillBindder>();

		public static void LoadSkillDataBase(string path)
		{
			LoadSkillDataBase(loader.Load(path));
		}

		public static void LoadSkillDataBase(SkillDataBase skillDataBase)
		{
			for (int i = 0; i < skillDataBase.SkillDataConfigs.Count; i++)
			{
				var skill = skillDataBase.SkillDataConfigs[i].Clone();
				if (skill == null) continue;
				AddSkill(skill);
			}
		}

		public static void AddSkill(ISkillData skill)
		{
            BindSkillControllerAttribute bind = skill.GetType().GetCustomAttribute<BindSkillControllerAttribute>();
            Type cType = bind != null ? bind.controllerType : null;
            skills.Add(skill.GetSkillKey, new SkillBindder() {skillData = skill, controllerType = cType });
        }

        public static void BindController<T>(string SkillKey) where T : ISkillController
        {
            BindController(SkillKey, typeof(T));
        }

        public static void BindController(string SkillKey, Type type)
        {
            if (!typeof(ISkillController).IsAssignableFrom(type))
            {
                throw new Exception("Type不继承ISkillController Type:" + type);
            }

            if (!skills.TryGetValue(SkillKey, out var bindder))
            {
                throw new Exception("没有对应的Skill标识，如果需要新增Skill并绑定请先使用SkillKit.AddSkill!如果是来自SkillDataBase管理的Skill，请先调用SkillKit.LoadSkillDataBase!");
            }

            Bind(bindder, type);
        }

        private static void Bind(SkillBindder buffBindder, Type type)
        {
            buffBindder.controllerType = type;
        }

        internal static ISkillController CreateSkillController(string SkillKey)
        {
            if (!skills.TryGetValue(SkillKey, out var bindder))
            {
                throw new Exception("Skill没有加载到SkillKit内! SkillKey:" + SkillKey);
            }

            if (bindder.controllerType == null)
            {
                throw new Exception("该Skill没有绑定控制器，请重试 SkillKey:" + SkillKey);
            }
            return GlobalObjectPools.GlobalAllocation(bindder.controllerType) as ISkillController;
        }  

        public static ISkillData GetSkillDataByKey(string skillKey)
		{
			if (skills.TryGetValue(skillKey, out var bindder))
				return bindder.skillData;

			throw new Exception("技能不存在,请检查是否加载技能到SkillKit中 SkillKey:" + skillKey);			
		}

		public static IEnumerator LoadSkillDataBaseAsync(string path)
		{
			bool completed = false;

			loader.LoadAsync(path, data => 
			{
				LoadSkillDataBase(data);
				completed = true;
			});

			yield return CoroutineTool.WaitUntil(() => completed);
		}

		/// <summary>
		/// 释放技能(拓展)
		/// </summary>
		/// <param name="executor"></param>
		/// <param name="controller"></param>
		/// <returns></returns>
		public static ReleaseSkillStatus ReleaseKill(this ISkillExecutor executor, ISkillController controller, params object[] param)
		{
			return executor.Handler.ReleaseSkill(controller.SkillData.GetSkillKey,param);
		}

		/// <summary>
		/// 取消技能的释放(拓展)
		/// </summary>
		/// <param name="executor"></param>
		/// <param name="controller"></param>
		public static void CancelSkill(this ISkillExecutor executor, ISkillController controller)
		{
			executor.Handler.CancelSkill(controller.SkillData.GetSkillKey);
		}

		/// <summary>
		/// 打断技能的释放(拓展)
		/// </summary>
		/// <param name="executor"></param>
		/// <param name="controller"></param>
		public static void InterruptionSkill(this ISkillExecutor executor, ISkillController controller)
		{
			executor.Handler.InterruptionSkill(controller.SkillData.GetSkillKey);
		}

		/// <summary>
		/// 重置技能释放时间(拓展)
		/// </summary>
		/// <param name="executor"></param>
		/// <param name="controller"></param>
		public static void ResetSkillReleasingTime(this ISkillExecutor executor, ISkillController controller)
		{
			executor.Handler.ResetSkillReleasingTime(controller.SkillData.GetSkillKey);
		}

		/// <summary>
		/// 重置技能冷却时间(拓展)
		/// </summary>
		/// <param name="executor"></param>
		/// <param name="controller"></param>
		public static void ResetSkillCoolingTime(this ISkillExecutor executor, ISkillController controller)
		{
			executor.Handler.ResetSkillCoolingTime(controller.SkillData.GetSkillKey);
		}

		/// <summary>
		/// 删除技能
		/// </summary>
		/// <param name="executor"></param>
		/// <param name="controller"></param>
		public static void RemoveSkill(this ISkillExecutor executor, ISkillController controller)
		{
			executor.Handler.RemoveSkill(controller.SkillData.GetSkillKey);
		}

        public static void DependLocalizationConfig(string configKey, char spilt = ':')
        {
            LocalizationConfigKey = configKey;
            Spilt = spilt;
            UseLocalizationConfig = true;
        }

        internal static bool UseLocalizationConfig { get; private set; } = false;

        internal static string LocalizationConfigKey { get; private set; }

        public static char Spilt { get; private set; }

        internal static ILocalizationData GetContent(string skillKey)
            => LocalizationKit.GetContent(LocalizationConfigKey, skillKey);

    }
}