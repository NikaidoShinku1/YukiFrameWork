///=====================================================
/// - FileName:      Item.cs
/// - NameSpace:     YukiFrameWork.Item
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/4/18 18:25:24
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using Sirenix.OdinInspector;
using Newtonsoft.Json;
namespace YukiFrameWork.Item
{
    public interface IItem
    {
        string GetKey { get;  }
        string GetName { get; }
        string GetDescription { get; }
        Sprite GetIcon { get; }     
        bool IsStackable { get; }
        bool IsMaxStackableCount { get;}
        int MaxStackableCount { get; }
        ItemType ItemType { get; }
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
        public Item(string name, string key, Sprite icon, ItemType itemType)
        {
            this.Name = name;
            this.Key = key;
            this.Icon = icon;
            this.ItemType = itemType;
        }

        public Item() { }        
        [field: SerializeField, LabelText("物品类型"),InfoBox("设置物品的类型，例如武器，材料等")]
        public ItemType ItemType { get; set; }
        [field: SerializeField, LabelText("是否可堆叠")]
        public bool IsStackable { get; set; } = true;
        [field: SerializeField, LabelText("是否有堆叠上限"),ShowIf(nameof(IsStackable))]
        public bool IsMaxStackableCount { get ; set ; }
        [field: SerializeField, LabelText("上限的数量"),ShowIf(nameof(IsMeet))]
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
        [LabelText("精灵所在的图集"),JsonProperty]
        internal string SpriteAtlas;
        [LabelText("精灵的路径/名称"),JsonProperty]
        internal string Sprite;      
    }
}
