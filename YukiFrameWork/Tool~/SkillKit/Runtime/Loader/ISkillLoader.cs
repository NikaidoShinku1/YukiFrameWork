///=====================================================
/// - FileName:      ISkillLoader.cs
/// - NameSpace:     YukiFrameWork.Skill
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/6/10 18:09:40
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
namespace YukiFrameWork.Skill
{
	public interface ISkillLoader
	{
		SkillDataBase Load(string path);

		void LoadAsync(string path, Action<SkillDataBase> onCompleted);
	
	}
}
