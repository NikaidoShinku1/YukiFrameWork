///=====================================================
/// - FileName:      InjectInfo.cs
/// - NameSpace:     YukiFrameWork.IOC
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/2/21 21:12:39
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using System.Reflection;
using YukiFrameWork.Extension;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace YukiFrameWork.IOC
{
	
	[Serializable]
	public class InjectInfo
	{
		public MonoScript monoScript;				
		public LifeTime lifeTime = LifeTime.Transient;			
	}

#if UNITY_EDITOR

	[CustomEditor(typeof(InjectInfo))]
	public class InjectInfoInspector : Editor
	{
        public override void OnInspectorGUI()
        {
			Debug.Log("Hello World");
        }
    }

#endif
}
