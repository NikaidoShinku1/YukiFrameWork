///=====================================================
/// - FileName:      ItemDefaultConfig.cs
/// - NameSpace:     YukiFrameWork.Knapsack
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
using YukiFrameWork.Extension;

namespace YukiFrameWork.Knapsack
{
    public class ItemDefaultParse : IItemInfomationParse
    {      
        public ItemData ParseJsonToItem(JsonData json)
        {
            ItemData itemData = AssemblyHelper.DeserializeObject<ItemData>(json.ToJson());
            itemData.LogInfo();
            return itemData;
        }
    }
}