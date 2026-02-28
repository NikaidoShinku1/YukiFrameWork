using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;

namespace YukiFrameWork.AI
{
    public class AIServicesInfo : ScriptableObject
    {
        [LabelText("AI的唯一Id")]
        public int id;
        
        [LabelText("AI的导航速度")]
        public float agentSpeed = 3.5f;
        [HideInInspector,Obsolete("已丢弃的参数,请使用agentSpeed属性来设置AI的导航速度")]
        public float speed = 3.5f;
        
        [LabelText("AI的参数列表")]
        public AIServicesParam[] parameters = new AIServicesParam[0];
        
        [LabelText("ai的喜好区域")]
        [InfoBox("如果在信息中没有设置区域，那么AI将会默认应用全局设置")]
        public List<AIAreaCastInfo> areaCastInfos = new List<AIAreaCastInfo>()
        {
            new  AIAreaCastInfo()
            {
                areaName = "Walkable",
                cast = 1
            },
            new  AIAreaCastInfo()
            {
                areaName = "Jump",
                cast = 2
            }
        };

        [Serializable]
        public class AIAreaCastInfo
        {
            public string areaName;
            [InfoBox("AI对该区域的喜好程度，数值越小越喜欢\n计算公式为:距离x权重 = 成本\n成本越低ai越喜欢")]
            public float cast = 1;
        }

    }

    public enum AIServicesParamType
    {
        Float,
        Intger,
        String,
        Boolan
    }

    [Serializable]
    public class AIServicesParam
    {
        [SerializeField,LabelText("参数的唯一标识")]
        internal string paramKey;
        [SerializeField,LabelText("参数类型")]
        internal AIServicesParamType paramType;
        [SerializeField,ShowIf(nameof(paramType),AIServicesParamType.String)]private string stringValue;
        [SerializeField,ShowIf(nameof(paramType),AIServicesParamType.Intger)]private int intValue;
        [SerializeField,ShowIf(nameof(paramType),AIServicesParamType.Float)]private float floatValue;
        [SerializeField,ShowIf(nameof(paramType),AIServicesParamType.Boolan)]private bool boolValue;
        
        [JsonIgnore]public object Value
        {
            get => paramType switch
            {
                AIServicesParamType.Boolan => boolValue,
                AIServicesParamType.Float => floatValue,
                AIServicesParamType.Intger => intValue,
                AIServicesParamType.String => stringValue,
                _ => throw new Exception("未定义的参数类型")
            };
        }
        [JsonIgnore]public string StringValue => paramType == AIServicesParamType.String ? stringValue : throw new Exception("参数类型不匹配");
        [JsonIgnore]public int IntValue => paramType == AIServicesParamType.Intger ? intValue : throw new Exception("参数类型不匹配");
        [JsonIgnore]public float FloatValue => paramType == AIServicesParamType.Float ? floatValue : throw new Exception("参数类型不匹配");
        [JsonIgnore]public bool BoolValue => paramType == AIServicesParamType.Boolan ? boolValue : throw new Exception("参数类型不匹配");
    }
}