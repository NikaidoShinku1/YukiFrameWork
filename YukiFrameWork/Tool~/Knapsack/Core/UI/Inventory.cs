using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
namespace YukiFrameWork.Knapsack
{
    public class Inventory : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler
    {
        [SerializeField] protected List<Slot> slots;           

        protected virtual void Awake()
        {
            InitSlot();                                   
        }     

        /// <summary>
        /// 初始化插槽数量
        /// </summary>
        private void InitSlot()
        {
            Slot[] slotsChild = GetComponentsInChildren<Slot>();
            slots = new List<Slot>(slotsChild);
        }

        /// <summary>
        /// 根据id储存物品
        /// </summary>
        /// <param name="id"></param>
        /// <returns>物体不为空返回true</returns>
        public virtual bool StoreItem(int id)
        {
            ItemData item = ItemKit.Config.GetItemByID(id);
            return StoreItem(item);
        }

        /// <summary>
        /// 根据物品数据储存物品
        /// </summary>
        /// <param name="item">物品</param>
        /// <returns>物体不为空返回true</returns>
        public virtual bool StoreItem(ItemData item)
        {
            Slot slot;
            if (item == null)
            {
                Debug.LogWarning("物品不存在");
                return false;
            }
            if (item.Capacity == 1)
            {
                slot = FindEmptySlot();
                if (slot == null)
                {
                    Debug.LogWarning("不存在空的物品槽位");
                    return false;
                }
            }
            else
            {
                slot = FindSlotId(item);
                if (slot == null)
                {
                    slot = FindEmptySlot();
                    if (slot == null)
                    {
                        Debug.LogWarning("不存在相同id/类型且空的物品槽位");
                        return false;
                    }
                }
            }
            slot.StoreItem(item);
            return true;

        }

        /// <summary>
        /// 寻找背包中空的槽位
        /// </summary>
        /// <returns>返回一个插槽</returns>
        public virtual Slot FindEmptySlot()
        {
            Slot slot = slots.Find(sl => !sl.IsItemActived);
            return slot;
        }

        /// <summary>
        /// 寻找背包中类型相同的槽位
        /// </summary>
        /// <returns></returns>
        public virtual Slot FindSlotId(ItemData item)
        {
            foreach (Slot slot in slots)
            {
                if (slot.IsItemActived && item.ID == slot.GetItemByid() && !slot.IsFilled())
                {
                    return slot;
                }
            }
            return null;
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            ItemKit.IsItemUIExit = false;
        }

        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            ItemKit.IsItemUIExit = true;
        }
    }
}