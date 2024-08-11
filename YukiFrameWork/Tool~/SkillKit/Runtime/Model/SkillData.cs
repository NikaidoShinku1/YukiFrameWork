///=====================================================
/// - FileName:      SkillData.cs
/// - NameSpace:     YukiFrameWork.Skill
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/6/10 18:26:35
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using Sirenix.OdinInspector;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using YukiFrameWork.Extension;
using System.Linq;
using Newtonsoft.Json.Converters;

#if UNITY_EDITOR
using UnityEditor;
#endif
namespace YukiFrameWork.Skill
{  
    [Serializable]
    public class SkillData : ScriptableObject, ISkillData
    {
        
        [SerializeField,LabelText("Skill的唯一标识"),InfoBox("该标识应该唯一，不允许出现多个一样标识的技能"),JsonProperty]
        private string SkillKey;

        [JsonIgnore]
        public string GetSkillKey => SkillKey;

        [SerializeField, LabelText("Skill的名称"), JsonProperty]
        private string SkillName;

        [JsonIgnore]
        public string GetSkillName => SkillKit.UseLocalizationConfig 
            ? SkillKit.GetContent(GetSkillKey).Context.Split(SkillKit.Spilt)[0] : SkillName;

        [SerializeField,LabelText("技能介绍"),JsonProperty,TextArea]
        private string Description;

        [JsonIgnore]
        public string GetDescription => SkillKit.UseLocalizationConfig
            ? SkillKit.GetContent(GetSkillKey).Context.Split(SkillKit.Spilt)[1] : Description;

        [field:SerializeField,LabelText("技能是否不受到时间影响")]
        [JsonProperty]
        public bool IsInfiniteTime { get; set; }

        [field:SerializeField,LabelText("技能是否可以主动取消")]
        [JsonProperty]
        public bool ActiveCancellation { get; set; }

        [field: SerializeField, LabelText("技能是否是可以被中途打断的")]
        [JsonProperty]
        public bool SkillInterruption { get; set; }

        [field:SerializeField,LabelText("技能是否拥有等级")]
        [JsonProperty]
		public bool IsSkillLavel { get; set; }

        [field: SerializeField, LabelText("技能是否拥有最大等级限制"),ShowIf(nameof(IsSkillLavel))]
        [JsonProperty]
        public bool IsSkillMaxLevel { get; set; }

        [JsonIgnore]
        private bool skillMaxLevel => IsSkillLavel && IsSkillMaxLevel;

        [field: SerializeField, LabelText("技能最大等级"),ShowIf(nameof(skillMaxLevel))]
        [JsonProperty]
        public int SkillMaxLevel { get; set; }   

        [field:SerializeField,LabelText("技能图标/静态"),PreviewField(50)]
        [JsonIgnore]
        public Sprite GetIcon { get; set; }
     
        [JsonProperty]
        internal string Sprite;

        [JsonIgnore]
        private bool IsNotReleaseAndIsInfin => IsInfiniteTime;
        [SerializeField,LabelText("技能释放时间"),JsonProperty(nameof(RealeaseTime)),HideIf(nameof(IsNotReleaseAndIsInfin))]
        private float releaseTime;
        [JsonIgnore]
        public float RealeaseTime
        {
            get => releaseTime;
            set
            {
                value = Mathf.Clamp(value, 0, float.MaxValue);
                releaseTime = value;
            }
        }

        [SerializeField,LabelText("技能冷却时间"),JsonProperty(nameof(CoolDownTime))]
        private float coolDownTime;

        [JsonIgnore]
        public float CoolDownTime
        {
            get => coolDownTime;
            set
            {
                value = Mathf.Clamp(value, 0, float.MaxValue);
                coolDownTime = value;
            }
        }

        [JsonProperty]
        internal string SKillType;

        [field:SerializeField,LabelText("在该技能释放期间可以同时释放的技能标识"),ValueDropdown(nameof(names))]
        [JsonProperty]
        public string[] SimultaneousSkillKeys{ get; set; }

        public static SkillData CreateInstance(string skillName, Type type)
        {
            SkillData skillData = ScriptableObject.CreateInstance(type) as SkillData;
            skillData.SkillName = skillName;
            skillData.SkillKey = "技能" + Mathf.Abs(skillData.GetInstanceID());
            return skillData;
        }

        public SkillData Clone() => GameObject.Instantiate(this);     
      
        [JsonIgnore]
        internal ValueDropdownList<string> names = new ValueDropdownList<string>();    
    }
}
