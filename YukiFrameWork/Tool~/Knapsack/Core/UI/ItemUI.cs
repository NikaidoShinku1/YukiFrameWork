using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XFABManager;

namespace YukiFrameWork.Knapsack
{
    public class ItemUI : MonoBehaviour
    {
        //物品信息
        public ItemData Item { get; set; }

        //物品数量
        public int Amount { get; set; }          
        public bool IsItemActive
            => Item != null;

        #region UIComponent
        [SerializeField]private Text amountText;

        private Text AmountText
        {
            get
            {
                if (amountText == null) amountText = GetComponentInChildren<Text>(true);
                return amountText;
            }

        }
        private Image itemImage;

        private Image ItemImage
        {
            get
            {
                if (itemImage == null)
                {
                    itemImage = GetComponent<Image>();
                }
                return itemImage;
            }
        }
        #endregion

        private void Update()
        {
            ItemKit.Config.InvokeAction(this);
        }

        /// <summary>
        /// 使物品放置到槽内
        /// </summary>
        /// <param name="item">物品</param>
        /// <param name="amount">初始物品数量</param>
        public void SetItem(ItemData item, int amount = 1)
        {
            Show();
            ItemKit.Config.InvokeReset(this);
            this.Item = item;
            this.Amount = amount;
            ItemImage.sprite = ItemKit.Config.LoadSprite(item.Sprites);      
            if (Item.Capacity == 1) AmountText.text = "";
            else RenderAmountText();
        }

        /// <summary>
        /// 添加物品数量
        /// </summary>
        /// <param name="amount">要添加的数量</param>
        public void AddAmount(int amount = 1)
        {
            this.Amount += amount;
            ItemKit.Config.InvokeReset(this);
            RenderAmountText();
        }

        /// <summary>
        /// 删除物品对应的数量
        /// </summary>
        /// <param name="amount">数量</param>
        public void ReduceAmount(int amount = 1)
        {
            ItemKit.Config.InvokeReset(this);
            this.Amount -= amount;
            RenderAmountText();
        }

        /// <summary>
        /// 复制
        /// </summary>
        /// <param name="amount"></param>
        public void SetAmount(int amount)
        {
            this.Amount = amount;
            ItemKit.Config.InvokeReset(this);
            RenderAmountText();
        }

        public void RenderAmountText()
        {
            AmountText.text = Amount.ToString();
            if (Item.Capacity == 1) AmountText.text = "";
        }

        public void Show()
        {          
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            Item = null;
            gameObject.SetActive(false);
        }

        public void SetLocalPosition(Vector3 position)
        {
            this.transform.localPosition = position;
        }




    }
}
