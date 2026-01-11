///=====================================================
/// - FileName:      InjectAttribute.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   高级定制脚本生成
/// - Creation Time: 12/25/2025 11:01:15 PM
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
namespace YukiFrameWork
{

    /// <summary>
    /// YukiFrameWork带有的动态字段特性,标记该动态字段后类型继承IDynamicMonoBehaviour接口即可实现对组件的完全自动赋值，无需手动操作绑定
    /// <para>组件可从自身或子对象进行查找!</para>
    /// <code>[DynamicValue]public Transform Value</code>
    /// <para>Tips:任何情况下，DynamicValue都要比DynamicFromSceneValue要快一步执行，若非必要情况不要两个特性同时挂给一个字段</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public class DynamicValueAttribute : PropertyAttribute
    {
        /// <summary>
        /// 子对象名称
        /// </summary>
        public string ChildObjName { get; }
        /// <summary>
        /// 是否查找所有的子对象
        /// </summary>
        public bool FindAllChild { get; }
        /// <summary>
        /// 是否查找只激活的对象
        /// </summary>
        public bool OnlyMonoEnable { get; }
        /// <summary>
        /// 查找自身子物体的对象是否存在该类型的组件，需要传递子对象的名称
        /// </summary>
        /// <param name="childObjName">子对象的名称</param>
        /// <param name="OnlyMonoEnable">查找的对象是否激活，如该参数为False，则即使对象失活也可以查找</param>
        public DynamicValueAttribute(string childObjName,bool OnlyMonoEnable = true)
        {
            this.ChildObjName = childObjName;
            this.OnlyMonoEnable = OnlyMonoEnable;
        }
        /// <summary>
        /// 查找自身
        /// </summary>
        public DynamicValueAttribute()
        {
            this.ChildObjName = string.Empty;
        }
        /// <summary>
        /// 查找所有的子对象返回第一个
        /// </summary>
        /// <param name="FindAllChild"></param>
        public DynamicValueAttribute(bool FindAllChild, bool OnlyMonoEnable = true)
        {
            this.FindAllChild = FindAllChild;
            this.OnlyMonoEnable = OnlyMonoEnable;
        }
    }
    /// <summary>
    /// YukiFrameWork带有的动态字段特性,标记该动态字段后类型继承IDynamicMonoBehaviour接口即可实现对组件的完全自动赋值，无需手动操作绑定
    /// <code>[DynamicValueFromScene]public Transform Value</code>
    /// <para>Tips:任何情况下，不要两个特性同时挂给一个字段(参数),DynamicValue会覆盖掉该特性</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter,AllowMultiple = false, Inherited = true)]
    public class DynamicValueFromSceneAttribute : PropertyAttribute
    {
        /// <summary>
        /// 场景中存在的对象标识,根据Mode的不同有不同的获取方式
        /// </summary>
        public string SceneObjLabel { get; }
        /// <summary>
        /// 是否查找只激活的对象
        /// </summary>
        public bool OnlyMonoEnable { get; }

        /// <summary>
        /// 场景动态添加的方式
        /// <para>Tips:Name模式下通过从场景的根(非子对象)进行查找，必须保证对象的名称唯一</para>
        /// <para>Name模式下通过从场景的根(非子对象)进行查找，必须保证对象的名称唯一</para>
        /// <para>Path模式等同于GameObject.Find</para>
        /// <para>Tag模式等同于GameObject.FindGameObjectWithTag</para>
        /// </summary>
        public DynamicValueFromSceneMode DynamicValueFromSceneMode { get; }
        /// <summary>
        /// 输入场景中存在的对象名称，通过指定的对象查找组件
        /// </summary>
        /// <param name="sceneObjLebel"></param>
        public DynamicValueFromSceneAttribute(string sceneObjLebel,DynamicValueFromSceneMode dynamicValueFromSceneMode = DynamicValueFromSceneMode.Name)
        {          
            this.SceneObjLabel = sceneObjLebel;
            this.DynamicValueFromSceneMode = dynamicValueFromSceneMode;
        }

        /// <summary>
        /// 全场景查找组件
        /// </summary>
        /// <param name="onlyMonoEnable"></param>
        public DynamicValueFromSceneAttribute(bool onlyMonoEnable = true)
        {
            this.OnlyMonoEnable = onlyMonoEnable;
        }
    }

    public enum DynamicValueFromSceneMode
    {
        Path,
        Name,
        Tag
    }

    /// <summary>
    /// 动态特性标记,如注入希望自定义，则需要定义规则器后标记该特性进行识别，字段/方法参数会优先识别参数的该特性，如具备该特性，则DynamicValue与DynamicValueFromScene在该特性上的标记均会被覆盖。
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Field,AllowMultiple = false)]
    public class DynamicRegulationAttribute : PropertyAttribute
    {
        public Type RegulationType { get; }
        public DynamicRegulationAttribute(Type regulationType)
        {
            this.RegulationType = regulationType;
        }
    }
}
