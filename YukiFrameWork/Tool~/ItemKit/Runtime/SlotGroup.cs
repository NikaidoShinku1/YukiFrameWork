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
using System.Linq;
namespace YukiFrameWork.Item
{
    
	public class SlotGroup
	{
        [field:JsonProperty,SerializeField]
		public string Key { get; internal set; }

        [JsonProperty,SerializeField]
        private List<Slot> slots = new List<Slot>();

        [JsonIgnore,NonSerialized]
        private Func<IItem, bool> mCondition = _ => true;

        [JsonIgnore]
		public IReadOnlyList<Slot> Slots => slots;

        private event Action OrderRefresh  = null;

        public bool IsEmpty => !slots.Any(x => x.Item != null);

        public bool IsFull => !slots.Any(x => x.Item == null);

        public SlotGroup ForEach(Action<Slot> action)
        {
            ForEach((_, slot) => action?.Invoke(slot));
            return this;
        }

        public SlotGroup ForEach(Action<int, Slot> action)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                action?.Invoke(i, slots[i]);
            }
            return this;
        }

        /// <summary>
        /// 注册排序刷新事件
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public SlotGroup RegisterOrderRefresh(Action order)
        {
            OrderRefresh += order;
            return this;
        }


        /// <summary>
        /// 注销排序刷新事件
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public SlotGroup UnRegisterOrderRefresh(Action order)
        {
            OrderRefresh -= order;
            return this;
        }

        public SlotGroup(string key)
        {
            Key = key;           
        }

        /// <summary>
        /// 创建一个新的插槽
        /// </summary>
        /// <param name="item"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public SlotGroup CreateSlot(IItem item, int count)
        {
            if (!mCondition.Invoke(item))
                return this;
            slots.Add(new Slot(item, count,this));
            return this;
        }

        /// <summary>
        /// 创建一个新的插槽
        /// </summary>
        /// <param name="itemKey"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public SlotGroup CreateSlotByKey(string itemKey, int count)
        {
            return CreateSlot(ItemKit.GetItemByKey(itemKey), count);
        }

        /// <summary>
        /// 创建指定数量的空插槽
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public SlotGroup CreateSlotsByCount(uint count)
        {
            for (int i = 0; i < count; i++)
            {
                CreateSlot(null,0);
            }
            return this;
        }

        /// <summary>
        /// 为物品仓库排序
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="orders"></param>
        /// <returns></returns>
        public SlotGroup OrderBy<TKey>(Func<Slot,TKey> orders)
        {
            slots = slots.OrderBy(orders).ToList();
            OrderRefresh?.Invoke();
            return this;
        }

        /// <summary>
        /// 从高到低为物品仓库排序
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="orders"></param>
        /// <returns></returns>
        public SlotGroup OrderByDescending<TKey>(Func<Slot, TKey> orders)
        {
            slots = slots.OrderByDescending(orders).ToList();
            OrderRefresh?.Invoke();
            return this;
        }
        /// <summary>
        /// 通过物品标识查找指定的插槽
        /// </summary>
        /// <param name="itemKey"></param>
        /// <returns></returns>
        public Slot FindSlotByKey(string itemKey) => slots.Find(x => x.Item != null && x.Item.Key == itemKey && x.ItemCount != 0);

        /// <summary>
        /// 查找到空插槽
        /// </summary>
        /// <returns></returns>
        public Slot FindEmptySlot() => slots.Find(x => x.ItemCount == 0);

        /// <summary>
        /// 查找到可以继续添加物品的插槽
        /// </summary>
        /// <param name="itemKey"></param>
        /// <returns></returns>
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
                if (slot.ItemCount != 0 && slot.Item.Key == itemKey && slot.ItemCount < slot.Item.MaxStackableCount)
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

        /// <summary>
        /// 添加物品
        /// </summary>
        /// <param name="itemKey"></param>
        /// <param name="addCount"></param>
        /// <returns></returns>
        public ItemOperateResult StoreItem(string itemKey, int addCount = 1)
        {
            var item = ItemKit.ItemDicts[itemKey];
            return StoreItem(item, addCount);
        }

        /// <summary>
        /// 将物品直接插入指定下标的插槽(如果物品是相同的以插入结果为准)
        /// </summary>
        /// <param name="itemKey"></param>
        /// <param name="addCount"></param>
        /// <returns></returns>
        public ItemOperateResult InsertItem(int index,IItem item, int addCount = 1)
        {
            if(!mCondition.Invoke(item))
                return new ItemOperateResult() { Succeed = false ,RemainCount = addCount};
            Slot slot = slots[index];
            slot.ItemCount = 0;
            slot.Item = null;
            if (slot.Item?.Key == item.Key)
            {              
                return StoreItem(item,addCount);
            }           
            if (item.IsStackable && item.IsMaxStackableCount)
            {
                if (addCount <= item.MaxStackableCount)                
                    goto F;               
                return new ItemOperateResult() { Succeed = false,RemainCount = addCount};
            }
            F: slot.ItemCount += addCount;
            slot.Item = item;
            slot.OnItemChanged.SendEvent();
            return new ItemOperateResult() { Succeed = true, RemainCount = 0 };
        }

        /// <summary>
        /// 将物品直接插入指定下标的插槽(如果物品是相同的则等同于添加的效果)
        /// </summary>
        /// <param name="index"></param>
        /// <param name="itemKey"></param>
        /// <param name="addCount"></param>
        /// <returns></returns>
        public ItemOperateResult InsertItem(int index, string itemKey, int addCount = 1)
        {
            return InsertItem(index, ItemKit.GetItemByKey(itemKey), addCount);
        }

        public ItemOperateResult StoreItem(IItem item, int addCount = 1)
        {
            if (!mCondition.Invoke(item))
                return new ItemOperateResult() { Succeed = false, RemainCount = addCount };
            if (item.IsStackable && item.IsMaxStackableCount)
            {
                do
                {
                    var slot = FindAddableMaxStackableCountSlot(item.Key);
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
                var slot = FindAddableSlot(item.Key);

                if (slot == null)
                {
                    return new ItemOperateResult() { Succeed = false };
                }

                slot.ItemCount += addCount;
                slot.OnItemChanged.SendEvent();
                return new ItemOperateResult() { Succeed = true, RemainCount = 0 };
            }
        }
        /// <summary>
        /// 移除某个物品
        /// </summary>
        /// <param name="itemKey"></param>
        /// <param name="removeCount"></param>
        /// <returns></returns>
        public bool RemoveItem(string itemKey, int removeCount = 1)
        {
            var slot = FindSlotByKey(itemKey);

            if (slot == null) return false;

            if (slot.ItemCount >= removeCount)
            {
                slot.ItemCount -= removeCount;
                if (slot.ItemCount == 0)
                    slot.Item = null;
                slot.OnItemChanged.SendEvent();
                return true;
            }

            return false;
        }
        /// <summary>
        /// 移除某个物品
        /// </summary>
        /// <param name="itemKey"></param>
        /// <param name="removeCount"></param>
        /// <returns></returns>
        public bool RemoveItem(IItem item, int removeCount = 1)
        {
            return RemoveItem(item.Key, removeCount);
        }

        /// <summary>
        /// 通过物品的下标清空物品
        /// </summary>
        /// <param name="index"></param>
        /// <param name="removeCount"></param>
        /// <returns></returns>
        public bool ClearItemByIndex(int index)
        {           
            Slot slot = slots[index];
            if (slot.Item == null)
                return false;

            return RemoveItem(slot.Item, slot.ItemCount);
        }

        /// <summary>
        /// 清空所有的物品
        /// </summary>
        public void ClearItem()
        {
            int count = slots.Count;
            for (int i = 0; i < count; i++)
            {
                ClearItemByIndex(i);
            }
        }

        internal bool ConditionInvoke(IItem item)
            => mCondition(item);

        /// <summary>
        /// 注册物品的添加条件(不满足条件是不会进行添加的)
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
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

        public void SlotInitInvoke(UISlot slot)
            => mOnSlotInit?.Invoke(slot);

        public void SlotSelectInvoke(UISlot slot)
            => mOnSlotSelect?.Invoke(slot); 

        public void SlotDeselectInvoke(UISlot slot)
           => mOnSlotDeselect?.Invoke(slot);

        public void SlotPointerEnterInvoke(UISlot slot)
           => mOnSlotPointerEnter?.Invoke(slot);

        public void SlotPointerExitInvoke(UISlot slot)
           => mOnSlotPointerExit?.Invoke(slot);

        public SlotGroup OnSlotInit(Action<UISlot> onSlotInit)
        {
            mOnSlotInit += onSlotInit;
            return this;
        }

        public SlotGroup OnSlotSelect(Action<UISlot> onSlotSelect)
        {
            mOnSlotSelect += onSlotSelect;
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
