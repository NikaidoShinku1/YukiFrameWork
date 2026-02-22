using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;

namespace YukiFrameWork.AI
{
    public class AIServicesInfo : ScriptableObject
    {
        [LabelText("AI的唯一Id")]
        public int id;
        [LabelText("AI的移动速度")]
        public float speed = 3.5f;
        
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
}