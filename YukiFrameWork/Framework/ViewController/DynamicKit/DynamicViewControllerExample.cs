///=====================================================
/// - FileName:      TestScripts.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   框架自定ViewController
/// - Creation Time: 12/26/2025 3:35:51 PM
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using Sirenix.OdinInspector;
namespace YukiFrameWork
{
	public partial class DynamicViewControllerExample : YukiFrameWork.DynamicViewController
	{
		[DynamicValue]
		[InfoBox("[DynamicValue]")]
		public Transform mTransform;
		[InfoBox("[DynamicValue]")]
		[DynamicValue]
		public DynamicViewControllerExample example;
		[InfoBox("[DynamicValue(\"Cube\")]")]
		[DynamicValue("Cube")]
		public BoxCollider boxCollider;
		[InfoBox("[DynamicValue(true,true)]")]
		[DynamicValue(true,true)]
		public CapsuleCollider capsuleCollider;
		[InfoBox("[DynamicValue(\"Sphere\",false)]")]
		[DynamicValue("Sphere",false)]
		public SphereCollider sphereCollider;
		[InfoBox("[DynamicValueFromScene]")]
		[DynamicValueFromScene]
		public Camera mCamera;
		[InfoBox("[DynamicValueFromScene(\"Directional Light\")]")]
		[DynamicValueFromScene("Directional Light")]
		public Light mLight;
		[InfoBox("[DynamicValueFromScene(true)]")]
		[DynamicValueFromScene(true)]
		public MeshCollider meshCollider;
	}
	
}
