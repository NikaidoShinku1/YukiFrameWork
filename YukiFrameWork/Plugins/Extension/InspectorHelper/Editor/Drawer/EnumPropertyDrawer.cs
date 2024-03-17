///=====================================================
/// - FileName:      EnumPropertyDrawer.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/3/8 3:36:34
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;

namespace YukiFrameWork
{
    [CustomPropertyDrawer(typeof(Enum), true)]
    public class EnumPropertyDrawer : BasePropertyDrawer
	{

	}
}
#endif