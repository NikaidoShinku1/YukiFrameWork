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
namespace YukiFrameWork.Dynamic
{
	[RuntimeInitializeOnArchitecture(typeof(ExampleRule.Example))]
	public partial class DynamicViewControllerExample : ViewController,IDynamicBuilder<ParticleSystem,OnTriggerEnter2DEvent,TestA>
	{
		[DynamicValue]

		public Transform mTransform;

		[DynamicValue]
		public DynamicViewControllerExample example;

		[DynamicValue("Cube")]
		public BoxCollider boxCollider;

		[DynamicValue(true,false)]
		public CapsuleCollider capsuleCollider;
		
		[DynamicValue("Sphere",false)]
		public SphereCollider sphereCollider;
		
		[DynamicValueFromScene]
		public Camera mCamera;
		
		[DynamicValueFromScene("Directional Light")]
		public Light mLight;

		[DynamicValueFromScene(false)]
		public MeshCollider meshCollider;

		[Button]
		public void Builder([DynamicValueFromScene]ParticleSystem authoring1, OnTriggerEnter2DEvent authoring2,[DynamicRegulation(typeof(TestARegulation))]TestA test)
        {
			Debug.Log(authoring1);
			Debug.Log(authoring2);
			Debug.Log(test);
        }
    }

	public class TestA
	{
		
	}

    public class TestARegulation : IDynamicRegulation
    {
        public object Build(Type parameterType, IDynamicMonoBehaviour builder)
        {
			return new TestA();
        }
    }

}
