///=====================================================
/// - FileName:      ItemData.cs
/// - NameSpace:     YukiFrameWork.Knapsack
/// - Created:       Yuki
/// - Email:         1274672030@qq.com
/// - Description:   背包配置类
/// - Creation Time: 2023/12/26 18:49:45
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================

using UnityEngine;
using System;
using YukiFrameWork.Extension;
using Newtonsoft.Json;
namespace YukiFrameWork.Knapsack
{ 
    [Serializable]
    public class ItemData
    {
        //物品id
        [field:SerializeField]public int ID { get; set; }
        //物品名字
        [field: SerializeField] public string Name { get; set; }
        //物品描述
        [field: SerializeField] public string Description { get; set; }
        //物品的最大容量
        [field: SerializeField] public int Capacity { get; set; }     
        //物品的精灵路径/名称(以加载方式为准)
        [field: SerializeField] public string Sprites { get; set; }     
        //物品的品质
        [field: SerializeField] public ItemQuality ItemQuality { get; set; }

        [field:SerializeField] public string TypeName { get; set; }

        [NonSerialized]
        private Type type;

        [JsonIgnore]
        public Type Type
        {
            get
            {
                if (type == null)
                    type = AssemblyHelper.GetType(TypeName);
                return type;
            }
        }

        public ItemData(int id, string name, string description, int capacity,  string sprites, ItemQuality itemQuality)
        {
            this.ID = id;
            this.Name = name;
            this.Description = description;
            this.Capacity = capacity;        
            this.Sprites = sprites;           
            this.ItemQuality = itemQuality;          

        }

        public ItemData()
        {
            this.ID = -1;           
        }
    }
}