///=====================================================
/// - FileName:      DefaultItemDataBase.cs
/// - NameSpace:     YukiFrameWork.Item
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/5/6 14:06:44
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System.Linq;
using Sirenix.OdinInspector;
using System.Collections.Generic;
namespace YukiFrameWork.Item
{
    [CreateAssetMenu(menuName = "YukiFrameWork/Item DataBase", fileName = nameof(DefaultItemDataBase))]
    public class DefaultItemDataBase : ItemDataBase<Item>
    {
        [SerializeField, Searchable, FoldoutGroup("物品管理", -1)]
        private List<Item> items = new List<Item>();

        public override List<IItem> Items
        {
            get => items.Select(x => x as IItem).ToList();
            set => items = value.Select(x => x as Item).ToList();
        }
    }
}
