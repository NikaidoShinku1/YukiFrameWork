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
using System.Linq;
using System.Collections;
using YukiFrameWork.Extension;


#if UNITY_EDITOR
using UnityEditor;
#endif
namespace YukiFrameWork.Skill
{  
    [Serializable]
    [HideMonoScript]
    public class SkillData : ScriptableObject, ISkillData
    {
        
        [SerializeField,LabelText("Skill的唯一标识"),InfoBox("该标识应该唯一，不允许出现多个一样标识的技能"),JsonProperty()]
        private string skillKey;

        [JsonIgnore]
        public string SkillKey
        {
            get => skillKey;
            set => skillKey = value;
        }

        [SerializeField, LabelText("Skill的名称"), JsonProperty()]
        private string skillName;

        [JsonIgnore,ExcelIgnore]
        public string SkillName 
        {
            get => skillName; set => skillName = value;
        }

        [SerializeField,LabelText("技能介绍"),JsonProperty(),TextArea]
        private string description;

        [JsonIgnore,ExcelIgnore]
        public string Description 
        {
            get => description; set => description = value;
        }

        [SerializeField, LabelText("技能是否不受到时间影响")]
        [JsonProperty]
        private bool isInfiniteTime;
        [JsonIgnore,ExcelIgnore]
        public bool IsInfiniteTime { get => isInfiniteTime; set => isInfiniteTime = value; }

        [SerializeField, LabelText("技能是否可以主动取消")]
        [JsonProperty]
        private bool activeCancellation;
        [JsonIgnore, ExcelIgnore]
        public bool ActiveCancellation { get => activeCancellation; set => activeCancellation = value; }

        [SerializeField, LabelText("技能是否是可以被中途打断的")]
        [JsonProperty]
        private bool skillInterruption;
        [JsonIgnore, ExcelIgnore]
        public bool SkillInterruption { get => skillInterruption; set => skillInterruption = value; }
        [JsonIgnore, ExcelIgnore]
        private bool IsNotReleaseAndIsInfin => IsInfiniteTime;
        [SerializeField, LabelText("技能释放时间"), JsonProperty(), HideIf(nameof(IsNotReleaseAndIsInfin))]
        private float releaseTime;
        [JsonIgnore, ExcelIgnore]
        public float RealeaseTime
        {
            get => releaseTime;
            set
            {
                value = Mathf.Clamp(value, 0, float.MaxValue);
                releaseTime = value;
            }
        }

        [SerializeField, LabelText("技能冷却时间"), JsonProperty()]
        private float coolDownTime;

        [JsonIgnore, ExcelIgnore]
        public float CoolDownTime
        {
            get => coolDownTime;
            set
            {
                value = Mathf.Clamp(value, 0, float.MaxValue);
                coolDownTime = value;
            }
        }
        [SerializeField]
#if UNITY_EDITOR
        [CustomValueDrawer(nameof(DrawPreview))]
#endif
        private Sprite icon;
        [JsonIgnore,ExcelIgnore]
        public Sprite Icon { get => icon; set => icon = value; }

#if UNITY_EDITOR
        private void DrawPreview()
        {

            GUILayout.BeginHorizontal();

            GUILayout.Label("技能图标");
            icon = (Sprite)UnityEditor.EditorGUILayout.ObjectField(this.icon, typeof(Sprite), true, GUILayout.Width(50), GUILayout.Height(50));
            GUILayout.EndHorizontal();
        }
#endif

        [SerializeField, LabelText("在该技能释放期间可以同时释放的技能标识")]
#if UNITY_EDITOR
        [ValueDropdown(nameof(list))]
#endif
        [JsonProperty]
        private string[] simultaneousSkillKeys;
        [JsonIgnore,ExcelIgnore]
        public string[] SimultaneousSkillKeys{ get => simultaneousSkillKeys; set => simultaneousSkillKeys = value; }
#if UNITY_EDITOR
        [JsonIgnore, ExcelIgnore]
        IEnumerable list => SkillDataBase.allSkillNames;
#endif
        public static SkillData CreateInstance(string skillName, Type type)
        {
            SkillData skillData = ScriptableObject.CreateInstance(type) as SkillData;
            skillData.skillName = skillName;
            skillData.skillKey = "技能" + Mathf.Abs(skillData.GetInstanceID());
            return skillData;
        }

        public SkillData Clone() => GameObject.Instantiate(this);     
      
        [Button("打开脚本",ButtonHeight = 20),PropertySpace(20)]
        void OpenScript()
        {
#if UNITY_EDITOR
            AssetDatabase.OpenAsset(AssetDatabase.FindAssets("t:monoScript").Select(x => AssetDatabase.GUIDToAssetPath(x))
                .Select(x => AssetDatabase.LoadAssetAtPath<MonoScript>(x))
                .FirstOrDefault(x => x?.GetClass() == this.GetType()));
#endif
        }
    }
}
