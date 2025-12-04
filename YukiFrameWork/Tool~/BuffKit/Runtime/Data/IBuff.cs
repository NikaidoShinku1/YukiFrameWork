///=====================================================
/// - FileName:      IBuff.cs
/// - NameSpace:     YukiFrameWork.Buffer
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/5/5 16:26:49
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Sirenix.OdinInspector;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
namespace YukiFrameWork.Buffer
{
	public enum BuffMode
	{
		Single,
		Multiple
	}

    public enum ParamType
    {
        Integer,
        Float,
        String,
        Boolan,
    }
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class BuffParam
    {
        [SerializeField, LabelText("参数标识"), JsonProperty]
        [InfoBox("参数的标识应该唯一!仅为该Buff所使用")]
        internal string paramKey;
        [SerializeField, LabelText("参数类型"), JsonConverter(typeof(StringEnumConverter)), JsonProperty]
        private ParamType paramType;

        [SerializeField, LabelText("target"), ShowIf(nameof(paramType), ParamType.Float), JsonProperty]
        private float target1;
        [SerializeField, LabelText("target"), ShowIf(nameof(paramType), ParamType.Integer), JsonProperty]
        private int target2;
        [SerializeField, LabelText("target"), ShowIf(nameof(paramType), ParamType.Boolan), JsonProperty]
        private bool target3;
        [SerializeField, LabelText("target"), ShowIf(nameof(paramType), ParamType.String), JsonProperty]
        private string target4;
        public BuffParam(ParamType paramType, object value)
        {
            switch (paramType)
            {
                case ParamType.Integer:
                    target2 = (int)value;
                    break;
                case ParamType.Float:
                    target1 = (float)value;
                    break;
                case ParamType.String:
                    target4 = (string)value;
                    break;
                case ParamType.Boolan:
                    target3 = (bool)value;
                    break;

            }
        }

        [JsonConstructor]
        private BuffParam() { }

        /// <summary>
        /// 可以通过Value属性获取到对应的参数
        /// </summary>     
        public object Value => paramType switch
        {
            ParamType.Float => target1,
            ParamType.String => target4,
            ParamType.Boolan => target3,
            ParamType.Integer => target2,
            _ => default
        };

        /// <summary>
        /// 可以获取Float型参数，当参数类型不是Float时，会抛出异常
        /// </summary>
        public float FloatValue
        {
            get
            {
                if (paramType != ParamType.Float)
                    throw new Exception("引用异常，参数类型选择不是Float，无法读取FloatValue");

                return target1;
            }
            set
            {
                if (paramType != ParamType.Float)
                    throw new Exception("引用异常，参数类型选择不是Float，无法写入FloatValue");

                target1 = value;
            }
        }
        /// <summary>
        /// 可以获取String型参数，当参数类型不是String时，会抛出异常
        /// </summary>
        public string StringValue
        {
            get
            {
                if (paramType != ParamType.String)
                    throw new Exception("引用异常，参数类型选择不是String，无法读取StringValue");

                return target4;
            }
            set
            {
                if (paramType != ParamType.String)
                    throw new Exception("引用异常，参数类型选择不是String，无法写入StringValue");

                target4 = value;
            }
        }
        /// <summary>
        /// 可以获取Integer型参数，当参数类型不是Integer时，会抛出异常
        /// </summary>
        public int IntValue
        {
            get
            {
                if (paramType != ParamType.Integer)
                    throw new Exception("引用异常，参数类型选择不是Integer，无法读取IntValue");

                return target2;
            }
            set
            {
                if (paramType != ParamType.Integer)
                    throw new Exception("引用异常，参数类型选择不是Integer，无法写入IntValue");

                target2 = value;
            }
        }
        /// <summary>
        /// 可以获取Boolan型参数，当参数类型不是Boolan时，会抛出异常
        /// </summary>
        public bool BoolValue
        {
            get
            {
                if (paramType != ParamType.Boolan)
                    throw new Exception("引用异常，参数类型选择不是Boolan，无法读取BoolValue");

                return target3;
            }

            set
            {

                if (paramType != ParamType.Boolan)
                    throw new Exception("引用异常，参数类型选择不是Boolan，无法写入BoolValue");

                target3 = value;
            }
        }

    }

    /// <summary>
    /// Buff配置接口
    /// </summary>
    public interface IBuff
	{
		/// <summary>
		/// Buff的唯一标识
		/// </summary>
		string Key { get; set; }

		/// <summary>
		/// Buff的名称
		/// </summary>
		string Name { get; set; }

		/// <summary>
		/// Buff的介绍
		/// </summary>
		string Description { get; set; }

		/// <summary>
		/// Buff的可存在方式(如果Buff存在，是否可以叠加)
		/// </summary>
		BuffMode BuffMode { get; set; }

        /// <summary>
        /// Buff持续时间,Duration小于0时视为无限时间
        /// </summary>
        float Duration { get; set; }

		/// <summary>
		/// Buff的图标
		/// </summary>
		Sprite Icon { get; set; }

        /// <summary>
        /// Buff可用的所有参数
        /// </summary>
        BuffParam[] BuffParams { get; }

		/// <summary>
		/// 这个Buff存在的所有效果(如继承框架提供的Buff基类，需要自定义Effect的类型，可通过override的方式重写该属性)
		/// </summary>
		List<IEffect> EffectDatas { get; }

		/// <summary>
		/// Buff绑定的控制器类型
		/// <para>Tip:如自定义IBuff的情况下，该属性需要传递完全限定类型(包含命名空间)</para>
		/// </summary>
		string BuffControllerType { get; set; }
	}

	/// <summary>
	/// Buff效果配置接口
	/// </summary>
	public interface IEffect
	{
		/// <summary>
		/// 唯一标识
		/// </summary>
		string Key { get; set; }

		/// <summary>
		/// 效果的名称
		/// </summary>
		string Name { get; set; }

		/// <summary>
		/// 效果的介绍
		/// </summary>
		string Description { get; set; }	
		
		/// <summary>
		/// 效果的类型(当一个Buff有多个效果时,可以为效果指定类型，在查询时获取复数的效果)
		/// </summary>
		string Type { get; set; }
	}
}
