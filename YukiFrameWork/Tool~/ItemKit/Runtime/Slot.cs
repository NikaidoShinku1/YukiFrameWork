///=====================================================
/// - FileName:      Slot.cs
/// - NameSpace:     YukiFrameWork.Item
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/4/18 18:25:02
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using Sirenix.OdinInspector;
namespace YukiFrameWork.Item
{
    [Serializable]
    public class Slot
    {
        [field: SerializeField, LabelText("物品")]
        public IItem Item { get; set; }
        [field: SerializeField, LabelText("物品的数量")]
        public int ItemCount { get; set; }
        public readonly EasyEvent OnItemChanged = new EasyEvent();

        public SlotGroup slotGroup { get; }
        public Slot(IItem item, int itemCount,SlotGroup group)
        {
            Item = item;
            ItemCount = itemCount;
            this.slotGroup = group;
        }

        /// <summary>
        /// 是否插槽置换满足条件
        /// </summary>
        /// <param name="current">当前的物品</param>
        /// <returns></returns>
        internal bool CheckCondition(IItem current) => slotGroup.ConditionInvoke(current);
    }
}
