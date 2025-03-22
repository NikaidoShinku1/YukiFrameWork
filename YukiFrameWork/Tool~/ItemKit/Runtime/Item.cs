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
using YukiFrameWork.Extension;
namespace YukiFrameWork.Item
{
    public interface IItem
    {
        /// <summary>
        /// 获取物品的唯一标识
        /// </summary>
        string Key { get;  }
        /// <summary>
        /// 获取物品的名称
        /// </summary>
        string Name { get; }
        /// <summary>
        /// 获取物品的介绍
        /// </summary>
        string Description { get; }
        /// <summary>
        /// 获取物品的图标
        /// </summary>
        Sprite Icon { get; }    
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
        private string key;
        [JsonIgnore]
        public string Key
        {
            get => key;
            set => key = value;
        }
        [SerializeField, LabelText("名称"),JsonProperty] 
        private string name;
        [JsonIgnore]
        public string Name => name;
        
        [SerializeField, LabelText("描述"), TextArea(minLines: 1, maxLines: 3),JsonProperty]
        private string description;
        [JsonIgnore]
        public string Description => description;
        
        public Item(string name, string key, Sprite icon, string itemType = "")
        {
            this.name = name;
            this.key = key;
            this.icon = icon;
            this.ItemType = itemType;
        }

        public Item() { }
#if UNITY_EDITOR
        private IEnumerable allItemTypes => ItemDataManager.AllManager_ItemTypes;
#endif
        [SerializeField, LabelText("物品类型")]
#if UNITY_EDITOR
        [ValueDropdown(nameof(allItemTypes))]
#endif
        private string itemType;
        [JsonIgnore,ExcelIgnore]
        public string ItemType { get => itemType; set => itemType = value; }
        [SerializeField, LabelText("是否可堆叠")]
        [JsonProperty]
        private bool isStackable = true;
        [JsonIgnore,ExcelIgnore]
        public bool IsStackable { get => isStackable; set => isStackable = value; }
        [SerializeField, LabelText("是否有堆叠上限"), ShowIf(nameof(IsStackable))]
        [JsonProperty]
        private bool isMaxStackableCount;
        [JsonIgnore,ExcelIgnore]
        public bool IsMaxStackableCount { get => isMaxStackableCount ; set => isMaxStackableCount = value; }
        [SerializeField, LabelText("上限的数量"), ShowIf(nameof(IsMeet))]
        [JsonProperty]
        private int maxStackableCount;
        [JsonIgnore, ExcelIgnore]
        public int MaxStackableCount { get => maxStackableCount; set => maxStackableCount = value; }
        [JsonIgnore,ExcelIgnore]
        private bool IsMeet => IsMaxStackableCount && IsStackable;
        [SerializeField, LabelText("图标"), PreviewField(50)]       
        private Sprite icon;
        [JsonIgnore,ExcelIgnore]
        public Sprite Icon => icon;
            
    }
}
