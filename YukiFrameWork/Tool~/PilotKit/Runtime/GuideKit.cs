///=====================================================
/// - FileName:      GuideKit.cs
/// - NameSpace:     YukiFrameWork.Pilot
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/8/14 19:36:19
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using System.Collections;
using Sirenix.OdinInspector;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
namespace YukiFrameWork.Pilot
{

    public enum GuideType
    {
        [LabelText("框架默认圆形遮挡")]
        Circle,
        [LabelText("框架默认矩形遮挡")]
        Rect,
        [LabelText("自定义遮挡")]
        Custom
    }
    public enum TransitionType
    {
        Direct,
        Slow
    }

    [Serializable]
    public class GuideInfo
    {
        [SerializeField,ReadOnly]
        internal GuideDataBase dataBase;
        [LabelText("触发标识")]
        [InfoBox("这个标识应该是唯一的")]
        public string guideKey;      
    
        [LabelText("过渡类型")]
        [JsonProperty,JsonConverter(typeof(StringEnumConverter))]
        public TransitionType transitionType;          

        [LabelText("引导类型")]
        [JsonProperty, JsonConverter(typeof(StringEnumConverter))]
        public GuideType guideType;

        [LabelText("材质"),ShowIf(nameof(guideType),GuideType.Custom),JsonIgnore]
        public Material material;

        [LabelText("脚本类型"), ShowIf(nameof(guideType), GuideType.Custom),ValueDropdown(nameof(list))]
        public string guideBaseType;    

        internal GuideBase guideBase;

        IEnumerable list => dataBase?.typeInfos;

        [LabelText("是否具有缩放")]
        public bool isScaling;

        [LabelText("缩放原始大小")]
        [InfoBox("调整缩放参数可以让引导具有一个简单的动画效果"),ShowIf(nameof(isScaling))]
        public float scale = 1;
        [LabelText("缩放检测时间"), ShowIf(nameof(isScaling))]
        public float scaleTime = 1;

        [ShowIf(nameof(transitionType),TransitionType.Slow)]
        [LabelText("过渡移动时间")]
        [InfoBox("遮罩平滑移动时间")]
        public float transTime = 1;               
    }

}
