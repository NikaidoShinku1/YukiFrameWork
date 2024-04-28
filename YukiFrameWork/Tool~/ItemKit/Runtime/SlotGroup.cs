///=====================================================
/// - FileName:      SlotGroup.cs
/// - NameSpace:     YukiFrameWork.Item
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/4/19 21:35:09
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
namespace YukiFrameWork.Item
{
	public class SlotGroup
	{
        [field:JsonProperty,SerializeField]
		public string Key { get; set; }

        [JsonProperty,SerializeField]
        private List<Slot> slots = new List<Slot>();

        [JsonIgnore,NonSerialized]
        private Func<IItem, bool> mCondition = _ => true;

        [JsonIgnore]
		public IReadOnlyList<Slot> Slots => slots;

        public SlotGroup(string key)
        {
            Key = key;           
        }

        public SlotGroup CreateSlot(IItem item, int count)
        {
            slots.Add(new Slot(item, count,this));
            return this;
        }

        public SlotGroup CreateSlotByKey(string itemKey, int count)
        {
            return CreateSlot(ItemKit.GetItemByKey(itemKey), count);
        }

        public SlotGroup CreateSlotsByCount(uint count)
        {
            for (int i = 0; i < count; i++)
            {
                CreateSlot(null,0);
            }
            return this;
        }   
        public Slot FindSlotByKey(string itemKey) => slots.Find(x => x.Item != null && x.Item.GetKey == itemKey && x.ItemCount != 0);

        public Slot FindEmptySlot() => slots.Find(x => x.ItemCount == 0);

        public Slot FindAddableSlot(string itemKey)
        {
            var item = ItemKit.ItemDicts[itemKey];
            if (!item.IsStackable)
            {
                var slot = FindEmptySlot();
                if (slot != null)
                    slot.Item = item;
                return slot;
            }
            else if (item.IsStackable && !item.IsMaxStackableCount)
            {
                var slot = FindSlotByKey(itemKey);
                if (slot == null)
                {
                    slot = FindEmptySlot();
                    if (slot != null)
                        slot.Item = ItemKit.ItemDicts[itemKey];

                }
                return slot;
            }
            else return FindAddableMaxStackableCountSlot(itemKey);
        }

        /// <summary>
        /// 查找可以添加的可堆叠式物品且有最大值限制的的插槽
        /// </summary>
        /// <param name="itemKey"></param>
        /// <returns></returns>
        public Slot FindAddableMaxStackableCountSlot(string itemKey) 
        {
            foreach (var slot in slots)
            {
                if (slot.ItemCount != 0 && slot.Item.GetKey == itemKey && slot.ItemCount < slot.Item.MaxStackableCount)
                {
                    return slot;
                }
            }
            var emptySlot = FindEmptySlot();
            if (emptySlot != null)
                emptySlot.Item = ItemKit.ItemDicts[itemKey];
            return emptySlot;
        }

        public struct ItemOperateResult
        {
            public bool Succeed;

            public int RemainCount;          
        }

        public ItemOperateResult StoreItem(string itemKey, int addCount = 1)
        {
            var item = ItemKit.ItemDicts[itemKey];
            return StoreItem(item, addCount);
        }

        public ItemOperateResult StoreItem(IItem item, int addCount = 1)
        {          
            if (item.IsStackable && item.IsMaxStackableCount)
            {
                do
                {
                    var slot = FindAddableMaxStackableCountSlot(item.GetKey);
                    if (slot != null)
                    {
                        var tempCount = slot.Item.MaxStackableCount - slot.ItemCount;
                        if (addCount <= tempCount)
                        {
                            slot.ItemCount += addCount;
                            slot.OnItemChanged.SendEvent();
                            addCount = 0;
                        }
                        else
                        {
                            slot.ItemCount += tempCount;
                            slot.OnItemChanged.SendEvent();
                            addCount -= tempCount;

                        }
                    }
                    else
                    {
                        return new ItemOperateResult() { Succeed = false, RemainCount = addCount };
                    }
                } while (addCount > 0);
                return new ItemOperateResult() { Succeed = true, RemainCount = 0 };
            }
            else
            {
                var slot = FindAddableSlot(item.GetKey);

                if (slot == null)
                {
                    return new ItemOperateResult() { Succeed = false };
                }

                slot.ItemCount += addCount;
                slot.OnItemChanged.SendEvent();
                return new ItemOperateResult() { Succeed = true, RemainCount = 0 };
            }
        }

        public bool RemoveItem(string itemKey, int removeCount = 1)
        {
            var slot = FindSlotByKey(itemKey);

            if (slot == null) return false;

            if (slot.ItemCount >= removeCount)
            {
                slot.ItemCount -= removeCount;
                slot.OnItemChanged.SendEvent();
                return true;
            }

            return false;
        }

        public bool RemoveItem(IItem item, int removeCount = 1)
        {
            return RemoveItem(item.GetKey, removeCount);
        }


        internal bool ConditionInvoke(IItem item)
            => mCondition(item);

        public SlotGroup Condition(Func<IItem, bool> condition)
        {
            this.mCondition = condition;
            return this;
        }
        private Action<UISlot> mOnSlotInit;
        private Action<UISlot> mOnSlotSelect;
        private Action<UISlot> mOnSlotDeselect;
        private Action<UISlot> mOnSlotPointerEnter;
        private Action<UISlot> mOnSlotPointerExit;

        internal void SlotInitInvoke(UISlot slot)
            => mOnSlotInit?.Invoke(slot);

        internal void SlotSelectInvoke(UISlot slot)
            => mOnSlotSelect?.Invoke(slot);

        internal void SlotDeselectInvoke(UISlot slot)
           => mOnSlotDeselect?.Invoke(slot);

        internal void SlotPointerEnterInvoke(UISlot slot)
           => mOnSlotPointerEnter?.Invoke(slot);

        internal void SlotPointerExitInvoke(UISlot slot)
           => mOnSlotPointerExit?.Invoke(slot);

        public SlotGroup OnSlotInit(Action<UISlot> onSlotInit)
        {
            mOnSlotInit += onSlotInit;
            return this;
        }

        public SlotGroup OnSlotSelect(Action<UISlot> onSlotSelect)
        {
            mOnSlotDeselect += onSlotSelect;
            return this;
        }
        public SlotGroup OnSlotDeselect(Action<UISlot> onSlotDeslect)
        {
            mOnSlotDeselect += onSlotDeslect;
            return this;
        }
        public SlotGroup OnSlotPointerEnter(Action<UISlot> onSlotPointerEnter)
        {
            mOnSlotPointerEnter += onSlotPointerEnter;
            return this;
        }
        public SlotGroup OnSlotPointerExit(Action<UISlot> slotPointerExit)
        {
            mOnSlotPointerExit += slotPointerExit;
            return this;
        }

        internal void Clear()
        {
            mOnSlotDeselect = null;
            mOnSlotInit = null;
            mOnSlotPointerEnter = null;
            mOnSlotPointerExit = null;
            mOnSlotSelect = null;
            mOnSlotDeselect = null;
            slots.Clear();
        }
    }
}
