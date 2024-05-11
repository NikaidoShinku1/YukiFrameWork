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
using System.Collections;
using System.Collections.Generic;
namespace YukiFrameWork.Buffer
{
    [Serializable,HideMonoScript]
    public abstract class Buff : ScriptableObject, IBuff
    {
        [SerializeField, LabelText("Buff的唯一标识:"), InfoBox("该标识应唯一，不允许出现多个buff同标识的情况"),JsonProperty]
        private string buffKey;
        [JsonIgnore]
        public string BuffKey
        {
            get => buffKey;
            set => buffKey = value;
        }

        public static Buff CreateInstance(string buffName, Type type)
        {
            var buff = ScriptableObject.CreateInstance(type) as Buff;
            buff.BuffName = buffName;
            return buff;
        }

        public Buff Clone() => GameObject.Instantiate(this);

        [SerializeField, LabelText("Buff名称:"),JsonProperty]
        private string buffName;
        [JsonIgnore]
        public string BuffName
        {
            get => buffName;
            set => buffName = value;
        }
        [SerializeField, LabelText("Buff的介绍:"),JsonProperty,TextArea]
        private string description;
        [JsonIgnore]
        public string Description
        {
            get => description;
            set => description = value;
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
        [field:ValueDropdown(nameof(Infos))]
        //[field:InfoBox("仅实现IBuff接口无法将ID在编辑器下转换成展开列表可视化，仅开发区别，无实际区别，只有派生自Buff时享受")]
        public string[] BuffCounteractID { get ; set ; }

        [field:SerializeField,JsonProperty,LabelText("该Buff运作时禁止添加的BuffID")]
        [field: ValueDropdown(nameof(Infos))]
        //[field:InfoBox("仅实现IBuff接口无法将ID在编辑器下转换成展开列表可视化，仅开发区别，无实际区别，只有派生自Buff时享受")]
        public string[] BuffDisableID { get ; set ; }

        [field: SerializeField, LabelText("Buff的图标样式"), JsonIgnore, PreviewField(50)]
        [JsonIgnore]
        public Sprite BuffIcon { get; set; }
        [field:JsonProperty]
        internal string Sprite { get; set; }
        [field:JsonProperty]
        internal string SpriteAtlas { get; set; }

        [JsonIgnore]
        internal BuffDataBase dataBase;

        #region Buff自带依赖ID转换于编辑器显示
        [Searchable]
        [JsonIgnore]
        private ValueDropdownList<string> names = new ValueDropdownList<string>();

        [JsonIgnore]
        IEnumerable Infos
        {
            get
            {
                names.Clear();

                for (int i = 0; i < dataBase.buffConfigs.Count; i++)
                {
                    var buff = dataBase.buffConfigs[i];

                    if (buff == null || buff.BuffName == BuffName) continue;

                    names.Add(buff.BuffName, buff.buffKey);
                }               

                return names;
            }
        }      
        #endregion
    }
}
