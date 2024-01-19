using UnityEngine;
namespace YukiFrameWork.Knaspack
{
    public class InventoryManager : SingletonMono<InventoryManager>
    {            
        //物品的ToolTip显示介绍   
        public Canvas Canvas { get; private set; }

        #region PickedItem      

        public bool IsPickedItem
        {
            get => pickedItem != null && pickedItem.gameObject.activeInHierarchy;
        }

        private ItemUI pickedItem;
        public ItemUI PickedItem
        {
            get => pickedItem;
            set 
            {
                pickedItem = value;
                pickedItem.GetComponent<UnityEngine.UI.Image>().raycastTarget = false;
                pickedItem.transform.SetParent(Canvas.transform);
            }
        }
        #endregion

        //当前鼠标是否处于背包/箱子UI上
        public bool IsItemUIExit { get; set; } = false;

        protected override void Awake()
        {
            IsDonDestroyLoad = true;
            base.Awake();

            Canvas = GameObject.Find("Canvas").GetComponent<Canvas>();          
        }

        protected virtual void Update()
        {
            if (IsPickedItem)
            {
                Vector2 direction = Input.mousePosition;
                RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)Canvas.transform, direction, null, out Vector2 targetPosition);
                pickedItem.SetLocalPosition(targetPosition + new Vector2(0, 15));
                if (!IsItemUIExit)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        RemoveItem();
                        ItemKit.Config.ReleaseItemUI(pickedItem);
                        pickedItem = null;                      
                    }    
                }
            }
           
        }   
        /// <summary>
        /// 捡起物品中指定数量的物品
        /// </summary>
        /// <param name="itemUI"></param>
        public virtual void PickUpItem(ItemData item, int amount)
        {
            pickedItem.SetItem(item, amount);
            pickedItem.Show();           
        }

        /// <summary>
        /// 删除所有物品
        /// </summary>
        public virtual void RemoveItem()
        {
            pickedItem.Hide();                      
            pickedItem.Item = null;                            
        }

        /// <summary>
        /// 删除指定数量的物品
        /// </summary>
        /// <param name="amount">数量</param>
        public virtual void RemoveItem(int amount)
        {
            pickedItem.ReduceAmount(amount);
            if (pickedItem.Amount <= 0)
            {
                RemoveItem();
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            ItemKit.Release();
        }


    }
}