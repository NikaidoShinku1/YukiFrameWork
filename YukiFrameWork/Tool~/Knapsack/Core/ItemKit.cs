using System;

///=====================================================
/// - FileName:      ItemKit.cs
/// - NameSpace:     YukiFrameWork.Knapsack
/// - Created:       Yuki
/// - Email:         1274672030@qq.com
/// - Description:   背包系统套件
/// - Creation Time: 2023/12/26 16:04:48
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================
namespace YukiFrameWork.Knapsack
{
    public class ItemKit
    {
        public static ItemConfigBase Config { get; private set; }
        public static bool IsInited = false;
        private static IHandItemController handItemController = null;
        public static void Init(ItemConfigData configData)
        {
            Init(configData, new HandItemDefaultController());
        }

        public static void Init(ItemConfigData configData,IHandItemController handItemController)
        {
            if (IsInited) return;

            IsInited = true;

            Config = new ItemConfigBase(configData);

            Config.Init();
            ItemKit.handItemController = handItemController;
            ItemKit.handItemController.Init();

            MonoHelper.Update_AddListener(heloer => ItemKit.handItemController.Update());
            MonoHelper.Destroy_AddListener(helper => Release());
        }
        /// <summary>
        /// 取出物品到手上
        /// </summary>
        /// <param name="itemData">物品</param>
        /// <param name="amount">数量</param>
        public static void PickUpItem(ItemData itemData, int amount)
        {           
            if (PickedItem == null)
            {
                handItemController.PickedItem = Config.GetItemUI(handItemController.Canvas.transform);
            }
            handItemController.PickUpItem(itemData,amount);
        }

        /// <summary>
        /// 设置物品UI的动画或者效果
        /// </summary>
        /// <param name="onReset">物品的效果初始化</param>
        /// <param name="onEvent">物品的动画效果</param>
        public static void SetItemResetAndAction(Action<ItemUI> onReset,Action<ItemUI> onAction)
        {
            Config.ItemAction = onAction;
            Config.ItemReset = onReset;
        }

        /// <summary>
        /// 释放背包管理套件
        /// </summary>
        public static void Release()
        {
            Config.Release();
            Config = null;
            IsInited = false;
        }

        #region PickedItem 是否正在拿着物品
        
        public static bool IsPickedItem
        {
            get => handItemController.IsPickedItem;
        }      
        public static ItemUI PickedItem
        {
            get =>  handItemController.PickedItem;
        }
        #endregion

        /// <summary>
        /// 判断鼠标位置是否在ui上
        /// </summary>
        public static bool IsItemUIExit
        {
            get => handItemController.IsItemUIExit;
            set => handItemController.IsItemUIExit = value;
        }
        /// <summary>
        /// 把手里的物品清空
        /// </summary>
        public static void ClearHandItem()
        {            
            handItemController.ClearItem();
        }

        /// <summary>
        /// 清理手里指定数量的物品
        /// </summary>
        /// <param name="count"></param>
        public static void RemoveHandItem(int count)
        {           
            handItemController.RemoveItem(count);
        }       
    }
}