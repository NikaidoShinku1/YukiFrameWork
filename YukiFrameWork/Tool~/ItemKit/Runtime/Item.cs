///=====================================================
/// - FileName:      Item.cs
/// - NameSpace:     YukiFrameWork.Item
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/4/18 18:25:24
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using UnityEngine;
using System;
using Sirenix.OdinInspector;
using Newtonsoft.Json;
using System.Collections;
namespace YukiFrameWork.Item
{
    public interface IItem
    {
        /// <summary>
        /// 获取物品的唯一标识
        /// </summary>
        string GetKey { get;  }
        /// <summary>
        /// 获取物品的名称
        /// </summary>
        string GetName { get; }
        /// <summary>
        /// 获取物品的介绍
        /// </summary>
        string GetDescription { get; }
        /// <summary>
        /// 获取物品的图标
        /// </summary>
        Sprite GetIcon { get; set; }    
        /// <summary>
        /// 物品是否是可堆叠的
        /// </summary>
        bool IsStackable { get; }
        /// <summary>
        /// 物品是否有堆叠数量上限
        /// </summary>
        bool IsMaxStackableCount { get;}
        /// <summary>
        /// 物品的最大堆叠数量
        /// </summary>
        int MaxStackableCount { get; set; }
        /// <summary>
        /// 物品的类型
        /// </summary>
        string ItemType { get; set; }                 
    }

    [Serializable]
    public class Item : IItem 
    {
        [SerializeField, LabelText("唯一标识"),JsonProperty]
        private string Key;
        [JsonIgnore]
        public string GetKey
        {
            get => Key;
            set => Key = value;
        }
        [SerializeField, LabelText("名称"),JsonProperty] 
        private string Name;
        [JsonIgnore]
        public string GetName
        {
            get
            {
                if (ItemKit.UseLocalizationConfig)
                {
                    var localization = ItemKit.GetLocalization(Key);
                    string[] contexts = localization.Context.Split(ItemKit.Spilt);
                    return contexts[0];
                }
                return Name;
            }
            set
            {
                ///如果开启了本地化配置使用
                if (ItemKit.UseLocalizationConfig)
                {
                    ILocalizationData localization = ItemKit.GetLocalization(Key);
                    ///改变本地化的数据值
                    localization.Context = $"{value}{ItemKit.Spilt}{GetDescription}";                
                }
                else
                {
                    ///修改名称
                    Name = value;
                }                
            }
        }
        [SerializeField, LabelText("描述"), TextArea(minLines: 1, maxLines: 3),JsonProperty]
        private string Description;
        [JsonIgnore]
        public string GetDescription
        {
            get
            {
                if (ItemKit.UseLocalizationConfig) 
                {
                    var localization = ItemKit.GetLocalization(Key);
                    string[] contexts = localization.Context.Split(ItemKit.Spilt);
                    return contexts[1];
                }
                return Description;
            }
            set
            {
                ///如果开启了本地化配置使用
                if (ItemKit.UseLocalizationConfig)
                {
                    ILocalizationData localization = ItemKit.GetLocalization(Key);
                    ///改变本地化的数据值
                    localization.Context = $"{GetName}{ItemKit.Spilt}{value}";
                }
                else
                {
                    ///修改名称
                    Description = value;
                }
            }
        }
        public Item(string name, string key, Sprite icon, string itemType = "")
        {
            this.Name = name;
            this.Key = key;
            this.Icon = icon;
            this.ItemType = itemType;
        }

        public Item() { }
#if UNITY_EDITOR
        private IEnumerable allItemTypes => ItemDataBase.AllItemTypes;
#endif
        [field: SerializeField, LabelText("物品类型")]
#if UNITY_EDITOR
        [field: ValueDropdown(nameof(allItemTypes))]
#endif
        [JsonProperty]
        public string ItemType { get; set; }
        [field: SerializeField, LabelText("是否可堆叠")]
        [JsonProperty]
        public bool IsStackable { get; set; } = true;
        [field: SerializeField, LabelText("是否有堆叠上限"),ShowIf(nameof(IsStackable))]
        [JsonProperty]
        public bool IsMaxStackableCount { get ; set ; }
        [field: SerializeField, LabelText("上限的数量"),ShowIf(nameof(IsMeet))]
        [JsonProperty]
        public int MaxStackableCount { get ; set ; }
        [JsonIgnore]
        private bool IsMeet => IsMaxStackableCount && IsStackable;
        [field: SerializeField, LabelText("图标"), PreviewField(50)]
        [JsonIgnore]
        private Sprite Icon;
        [JsonIgnore]
        public Sprite GetIcon
        {
            get
            {
                if (ItemKit.UseLocalizationConfig)
                {
                    return ItemKit.GetLocalization(Key).Sprite;
                }                            
                return Icon;
            }
            set
            {
                if (ItemKit.UseLocalizationConfig)
                {
                    ItemKit.GetLocalization(Key).Sprite = value;
                }
                else Icon = value;
            }
        }                     
    }
}
