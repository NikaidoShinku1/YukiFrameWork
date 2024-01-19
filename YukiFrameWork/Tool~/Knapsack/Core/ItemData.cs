///=====================================================
/// - FileName:      ItemData.cs
/// - NameSpace:     YukiFrameWork.Knaspack
/// - Created:       Yuki
/// - Email:         1274672030@qq.com
/// - Description:   背包配置类
/// - Creation Time: 2023/12/26 18:49:45
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================

using UnityEngine;
using System;
namespace YukiFrameWork.Knaspack
{
    /// <summary>
    /// 物品类型
    /// </summary>
    public enum ItemType
    {
        //消耗品
        Comsumable,
        //装备
        Equipment,
        //武器
        Weapon,
        //材料
        Material
    }

    /// <summary>
    /// 物品的品质
    /// </summary>
    public enum ItemQuality
    {
        //普通
        Common,
        //稀有
        UnCommon,
        //史诗
        Epic,
        //传奇
        Lengendary,

    }
    public class ItemData
    {
        //物品id
        public int ID { get; set; }
        //物品名字
        public string Name { get; set; }
        //物品描述
        public string Description { get; set; }
        //物品的容量
        public int Capacity { get; set; }
        //购买价格
        public int Buyprice { get; set; }
        //售出价格
        public int Sellprice { get; set; }

        public string Sprites { get; set; }

        //物品的类型
        public ItemType ItemType { get; set; }
        //物品的品质
        public ItemQuality ItemQuality { get; set; }


        public ItemData(int id, string name, string description, int capacity, int buyprice, int sellprice, string sprites, ItemType itemType, ItemQuality itemQuality)
        {
            this.ID = id;
            this.Name = name;
            this.Description = description;
            this.Capacity = capacity;
            this.Buyprice = buyprice;
            this.Sellprice = sellprice;
            this.Sprites = sprites;
            this.ItemType = itemType;
            this.ItemQuality = itemQuality;

        }

        public ItemData()
        {
            this.ID = -1;
        }
    }
}