///=====================================================
/// - FileName:      MissionParams.cs
/// - NameSpace:     YukiFrameWork.Missions
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/11/3 19:06:01
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
using Sirenix.OdinInspector;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
namespace YukiFrameWork.Missions
{
    public enum ParamType
    {
        Integer,
        Float,
        String,
        Boolan
    }
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class MissionParam
    {
        [SerializeField, LabelText("参数类型"),JsonConverter(typeof(StringEnumConverter)),JsonProperty]
        private ParamType paramType;

        [SerializeField,LabelText("target"),ShowIf(nameof(paramType),ParamType.Float),JsonProperty]
        private float target1;
        [SerializeField, LabelText("target"), ShowIf(nameof(paramType), ParamType.Integer),JsonProperty]
        private int target2;
        [SerializeField, LabelText("target"), ShowIf(nameof(paramType), ParamType.Boolan),JsonProperty]
        private bool target3;
        [SerializeField, LabelText("target"), ShowIf(nameof(paramType), ParamType.String),JsonProperty]
        private string target4;

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
        }
        /// <summary>
        /// 可以获取String型参数，当参数类型不是Float时，会抛出异常
        /// </summary>
        public string StringValue
        {
            get
            {
                if (paramType != ParamType.String)
                    throw new Exception("引用异常，参数类型选择不是String，无法读取StringValue");

                return target4;
            }
        }
        /// <summary>
        /// 可以获取Integer型参数，当参数类型不是Float时，会抛出异常
        /// </summary>
        public int IntValue
        {
            get
            {
                if (paramType != ParamType.Integer)
                    throw new Exception("引用异常，参数类型选择不是Integer，无法读取IntValue");

                return target2;
            }
        }
        /// <summary>
        /// 可以获取Boolan型参数，当参数类型不是Float时，会抛出异常
        /// </summary>
        public bool BoolValue
        {
            get
            {
                if (paramType != ParamType.Boolan)
                    throw new Exception("引用异常，参数类型选择不是Boolan，无法读取BoolValue");

                return target3;
            }
        }

    }
   
}
