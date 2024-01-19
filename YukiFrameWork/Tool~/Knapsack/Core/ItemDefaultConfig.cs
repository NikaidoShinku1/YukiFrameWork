///=====================================================
/// - FileName:      ItemDefaultConfig.cs
/// - NameSpace:     YukiFrameWork.Knaspack
/// - Created:       Yuki
/// - Email:         1274672030@qq.com
/// - Description:   背包配置类
/// - Creation Time: 2023/12/26 16:45:44
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================

using UnityEngine;
using System;
using LitJson;

namespace YukiFrameWork.Knaspack
{
    public class ItemDefaultConfig : ItemConfigBase
    {
        public override void ParseItemToJson(JsonData jsonData)
        {
            Enum.TryParse(jsonData["itemType"].ToString(), out ItemType type);
            Enum.TryParse(jsonData["itemQuality"].ToString(), out ItemQuality quality);
            int id = (int)jsonData["id"];
            string name = jsonData["name"].ToString();
            string description = jsonData["description"].ToString();
            int capacity = (int)jsonData["capacity"];
            int buyprice = (int)jsonData["buyprice"];
            int sellprice = (int)jsonData["sellprice"];
            string sprites = jsonData["sprites"].ToString();

            ItemData itemData = new ItemData(id, name, description, capacity, buyprice, sellprice, sprites, type, quality);         
            AddItem(itemData);
        }
    }
}