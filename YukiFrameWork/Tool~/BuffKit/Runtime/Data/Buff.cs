///=====================================================
/// - FileName:      Buff.cs
/// - NameSpace:     YukiFrameWork.Buffer
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/5/5 21:00:44
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using UnityEngine;
using System;
using Sirenix.OdinInspector;
using Newtonsoft.Json;
using System.Linq;
using System.Collections;
namespace YukiFrameWork.Buffer
{
    public abstract class Buff : ScriptableObject, IBuff
    {
        [SerializeField, LabelText("Buff的唯一标识:"), InfoBox("该标识应唯一，不允许出现多个buff同标识的情况"),JsonProperty]
        private string BuffKey;
        [JsonIgnore]
        public string GetBuffKey
        {
            get => BuffKey;
            set => BuffKey = value;
        }

        public static Buff CreateInstance(string buffName, Type type)
        {
            var buff = ScriptableObject.CreateInstance(type) as Buff;
            buff.BuffName = buffName;
            return buff;
        }

        public Buff Clone() => GameObject.Instantiate(this);

        [SerializeField, LabelText("Buff名称:"),JsonProperty]
        private string BuffName;
        [JsonIgnore]
        public string GetBuffName
        {
            get
            {
                return BuffKit.UseLocalizationConfig ? BuffKit.GetContent(BuffKey).Context.Split(BuffKit.Spilt)[0] : BuffName;
            }
            set
            {
                if (BuffKit.UseLocalizationConfig)
                    BuffKit.GetContent(BuffKey).Context = $"{value}{BuffKit.Spilt}{GetDescription}";
                else BuffName = value;
            }
        }
        [SerializeField, LabelText("Buff的介绍:"),JsonProperty,TextArea]
        private string Description;
        [JsonIgnore]
        public string GetDescription
        {
            get => BuffKit.UseLocalizationConfig ? BuffKit.GetContent(BuffKey).Context.Split(BuffKit.Spilt)[1] : Description;
            set
            {

                if (BuffKit.UseLocalizationConfig)
                    BuffKit.GetContent(BuffKey).Context = $"{GetBuffName}{BuffKit.Spilt}{value}";
                else Description = value;
            }
        }
        [SerializeField, LabelText("Buff重复添加的类型:"),JsonProperty]
        private BuffRepeatAdditionType additionType;

        [JsonIgnore]
        public BuffRepeatAdditionType AdditionType
        {
            get => additionType;
            set => additionType = value;
        }
        [JsonIgnore]
        private bool IsAddition => AdditionType == BuffRepeatAdditionType.Multiple || AdditionType == BuffRepeatAdditionType.MultipleAndReset;

        [field:SerializeField,LabelText("是否缓慢移除"),ShowIf(nameof(IsAddition)),InfoBox("如果Buff是可叠加的，当该属性为True时，Buff是一层一层减少，False则一次性消失")]
        public bool IsBuffRemoveBySlowly { get; set; }

        [field: SerializeField, LabelText("是否存在层数上限"), ShowIf(nameof(IsAddition))]
        public bool IsMaxStackableLayer { get; set; }

        [field: SerializeField, LabelText("上限层数"), ShowIf(nameof(isMax)),MinValue(1)]
        public int MaxStackableLayer { get; set; }

        [JsonIgnore]
        private bool isMax => IsAddition && IsMaxStackableLayer;

        [SerializeField,LabelText("Buff的周期类型:")]
        private BuffSurvivalType survivalType;
        [JsonIgnore]
        public BuffSurvivalType SurvivalType
        {
            get => survivalType;
            set => survivalType = value;
        }       

        [SerializeField,LabelText("Buff的持续时间(单位秒)"),ShowIf(nameof(survivalType),BuffSurvivalType.Timer)]
        private float buffTimer = 1;

        [JsonIgnore]
        public float BuffTimer
        {
            get => buffTimer;
            set
            {
                value = Mathf.Clamp(value, 0f, float.MaxValue);
                buffTimer = value;
            }
        }

        [field:SerializeField,JsonProperty,LabelText("该Buff会与指定相互抵消的BuffID")]
        [field:ValueDropdown(nameof(names))]
        [field:InfoBox("仅实现IBuff接口无法将ID在编辑器下转换成展开列表可视化，仅开发区别，无实际区别，只有派生自Buff时享受")]
        public string[] BuffCounteractID { get ; set ; }

        [field:SerializeField,JsonProperty,LabelText("该Buff运作时禁止添加的BuffID")]
        [field: ValueDropdown(nameof(names))]
        [field:InfoBox("仅实现IBuff接口无法将ID在编辑器下转换成展开列表可视化，仅开发区别，无实际区别，只有派生自Buff时享受")]
        public string[] BuffDisableID { get ; set ; }
        [field: SerializeField, LabelText("Buff的图标样式"), JsonIgnore, PreviewField(50)]
        [JsonIgnore]
        public Sprite BuffIcon { get; set; }

        [JsonProperty]
        internal string Sprite;

        [JsonProperty]
        internal string BuffType;

        [Button("打开脚本", ButtonHeight = 40), PropertySpace(20)]
        void OpenScript()
        {
#if UNITY_EDITOR
            UnityEditor.AssetDatabase.OpenAsset(UnityEditor.AssetDatabase.FindAssets("t:monoScript").Select(x => UnityEditor.AssetDatabase.GUIDToAssetPath(x))
                .Select(x => UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEditor.MonoScript>(x))
                .FirstOrDefault(x => x?.GetClass() == this.GetType()));
#endif
        }

        #region Buff自带依赖ID转换于编辑器显示
        [Searchable]
        [JsonIgnore]
        internal IEnumerable names => BuffDataBase.allBuffNames;
        #endregion
    }
}
