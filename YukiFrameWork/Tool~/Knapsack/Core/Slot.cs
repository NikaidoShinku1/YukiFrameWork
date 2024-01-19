using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using YukiFrameWork.Pools;
namespace YukiFrameWork.Knaspack
{
    public class Slot : MonoBehaviour, IPointerDownHandler
    {              
        public bool IsItemActived { get; private set; }

        private ItemUI itemUI;
        /// <summary>
        /// 储存物品到插槽内
        /// </summary>
        /// <param name="item">物品</param>
        public void StoreItem(ItemData item, int amount = 1)
        {
            itemUI = GetComponentInChildren<ItemUI>(true);
            if (itemUI == null)
                itemUI = ItemKit.Config.GetItemUI(transform);          
            if (!itemUI.IsItemActive)
            {
                itemUI.transform.localPosition = Vector3.zero;
                itemUI.SetItem(item, amount);
                itemUI.Show();
            }
            else itemUI.AddAmount();
            IsItemActived = true;
        }

        /// <summary>
        /// 删除插槽内的物品
        /// </summary>
        public void Destroy()
        {           
            if (itemUI == null || !itemUI.IsItemActive) return;
            itemUI.Hide();
            itemUI.Item = null;
            IsItemActived = false;
            ItemKit.Config.ReleaseItemUI(itemUI);
            itemUI = null;
        }

        /// <summary>
        /// 获取物品的类型id
        /// </summary>
        /// <returns>返回物品的id</returns>
        public int GetItemByid()
        {
            return itemUI.Item.ID;
        }

        /// <summary>
        /// 获取物品的类型
        /// </summary>
        /// <returns>返回物品的类型</returns>
        public ItemType GetItemType()
        {
            return itemUI.Item.ItemType;
        }

        /// <summary>
        /// 判断当前槽位物品是否已达到上限
        /// </summary>
        /// <returns></returns>
        public bool IsFilled()
        {           
            if (!itemUI) return false;
            return itemUI.Amount >= itemUI.Item.Capacity;
        }           
     
        public virtual void OnPointerDown(PointerEventData eventData)
        {          
            if (eventData.button != PointerEventData.InputButton.Left) return;
            
            //当插槽内有物品
            if (itemUI != null && itemUI.IsItemActive)
            {
                //当前手上没有物品
                if (!ItemKit.IsPickedItem)
                {
                    if (Input.GetKey(KeyCode.LeftControl))
                    {
                        //获取当前插槽内物体数量+1之后的一半，四舍五入取值
                        int amountPicked = (itemUI.Amount + 1) / 2;

                        //拿起对应数量的物品
                        ItemKit.PickUpItem(itemUI.Item, amountPicked);

                        //剩余物品数量
                        int amountRemained = itemUI.Amount - amountPicked;

                        //如果物品被全部取出
                        if (amountRemained <= 0)
                        {
                            Destroy();
                        }
                        else
                        {
                            itemUI.SetAmount(amountRemained);
                        }
                    }
                    else
                    {
                        //拿出所有物品
                        ItemKit.PickUpItem(itemUI.Item, itemUI.Amount);
                        Destroy();
                        //当前闲置空位已有物品                   
                    }
                }
                //手中已经有物品
                else
                {
                    ItemData item = ItemKit.PickedItem.Item;
                    //判断手中物品id是否等于插槽物品id

                    if (itemUI.Item.ID == ItemKit.PickedItem.Item.ID)
                    {
                        if (Input.GetKey(KeyCode.LeftControl))
                        {
                            //如果当前插槽物品数量已经到达上限则不执行下面代码
                            if (itemUI.Amount >= itemUI.Item.Capacity) return;

                            //增加一个物品
                            itemUI.AddAmount();

                            //手中减少一个物品
                            ItemKit.RemoveItem(1);

                        }
                        else
                        {
                            //插槽可容纳物品数量
                            int amountRemained = itemUI.Item.Capacity - itemUI.Amount;

                            //当可容纳数量大于手中物品数量
                            if (amountRemained >= ItemKit.PickedItem.Amount)
                            {
                                //插槽增加手中所有的物品数量
                                itemUI.AddAmount(ItemKit.PickedItem.Amount);
                                //删除手中物品
                                ItemKit.ClearItem();
                            }
                            else
                            {
                                //增加最大数量
                                itemUI.AddAmount(amountRemained);

                                //对应数量删除
                                ItemKit.RemoveItem(amountRemained);
                            }
                        }
                    }
                    else
                    {
                        ///交替物品                                        
                        int amount = ItemKit.PickedItem.Amount;
                        ItemKit.PickUpItem(itemUI.Item, itemUI.Amount);
                        itemUI.SetItem(item, amount);

                    }
                }
            }
            else
            {
                //如果手中有物品
                if (ItemKit.IsPickedItem)
                {
                    ItemUI item = ItemKit.PickedItem;
                    if (Input.GetKey(KeyCode.LeftControl))
                    {
                        StoreItem(item.Item);
                        ItemKit.RemoveItem(1);
                    }
                    else
                    {
                        for (int i = 0; i < item.Amount; i++)
                        {
                            StoreItem(item.Item);
                        }
                        ItemKit.ClearItem();
                    }
                }              
            }          
        }
    }

}