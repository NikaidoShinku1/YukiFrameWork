///=====================================================
/// - FileName:      ISkillData.cs
/// - NameSpace:     YukiFrameWork.Skill
/// - Description:   技能数据接口
/// - Creation Time: 2024/5/28 21:13:44
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using Sirenix.OdinInspector;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
namespace YukiFrameWork.Skill
{

    public enum ParamType
    {
        Integer,
        Float,
        String,
        Boolan,
    }
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class SkillParam
    {
        [SerializeField,LabelText("参数标识"), JsonProperty]
        [InfoBox("参数的标识应该唯一!仅为该技能所使用")]
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

        [JsonConstructor]
        private SkillParam() { }
        
        public SkillParam(ParamType paramType, object value)
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
    public interface ISkillData
	{
		/// <summary>
		/// 技能标识(唯一)
		/// </summary>
		string SkillKey { get; set; }
		/// <summary>
		/// 技能名称
		/// </summary>
		string SkillName { get; set; }	
		/// <summary>
		/// 技能介绍
		/// </summary>
		string Description { get; set; }
		/// <summary>
		/// 技能图标
		/// </summary>
		Sprite Icon { get; set; }

        /// <summary>
        /// 技能所可能需要的参数
        /// </summary>
        SkillParam[] SkillParams { get; }

		/// <summary>
		/// 技能释放是否是无限时间的
		/// </summary>		
		bool IsInfiniteTime { get; set; }

		/// <summary>
		/// 技能是否可以主动取消
		/// </summary>
		bool ActiveCancellation { get; set; }

		/// <summary>
		/// 技能释放时间
		/// </summary>
		float ReleaseTime { get; set; }
		
		/// <summary>
		/// 技能冷却时间
		/// </summary>
		float CoolDownTime { get; set; }
		
		/// <summary>
		/// 可以同时释放的技能标识
		/// </summary>
		string[] SimultaneousSkillKeys { get; set; }

		/// <summary>
		/// 技能所绑定的控制器类型
		/// </summary>
		string SkillControllerType { get; set; }
	
	}
}
