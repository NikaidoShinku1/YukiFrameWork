///=====================================================
/// - FileName:      UISkillGroup.cs
/// - NameSpace:     YukiFrameWork.Skill
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/6/11 21:45:53
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using Sirenix.OdinInspector;
using System.Collections.Generic;
namespace YukiFrameWork.Skill
{
	public class UISkillGroup : MonoBehaviour
	{
		[SerializeField,LabelText("UI的技能显示同步预制体")]
		private UISkill skillPrefabs;

		private Queue<UISkill> generiedSkills = new Queue<UISkill>();

		private FastList<UISkill> showSkills = new FastList<UISkill>();

        private void Awake()
        {			
			skillPrefabs.Hide();
        }

		public UISkill CreateUISkill(string key)
		{
			UISkill skill = generiedSkills.Count > 0 ? generiedSkills.Dequeue().Show() : skillPrefabs.Instantiate(this).Show();
			skill.GetSkillKey = key;
			showSkills.Add(skill);	
			return skill;
		}

		public void UnLoadUISkill(string key)
		{
			UISkill skill = showSkills.Find(skill => key == skill.GetSkillKey);

			if (skill == null) return;

			showSkills.Remove(skill);
			skill.Hide();
			generiedSkills.Enqueue(skill);
		}
	}
}
