///=====================================================
/// - FileName:      MissionParam.cs
/// - NameSpace:     YukiFrameWork.Missions
/// - Description:   高级定制脚本生成
/// - Creation Time: 1/14/2026 9:14:03 PM
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using Sirenix.OdinInspector;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using YukiFrameWork.Extension;
namespace YukiFrameWork.Missions
{
    public enum ParamType
    {
        Float,
        Integer,
        Boolan,
        String
    }
    [Serializable]
    public class MissionParam 
    {
        [SerializeField, LabelText("参数标识"), JsonProperty]
        [InfoBox("参数的标识应该唯一!仅为该任务所使用")]
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
        private MissionParam() { }

        public MissionParam(ParamType paramType, object value)
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
       [JsonIgnore,ExcelIgnore] public object Value => paramType switch
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
        [JsonIgnore,ExcelIgnore]public float FloatValue
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
        [JsonIgnore,ExcelIgnore]public string StringValue
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
        [JsonIgnore,ExcelIgnore]public int IntValue
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
        [JsonIgnore,ExcelIgnore]public bool BoolValue
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
}
