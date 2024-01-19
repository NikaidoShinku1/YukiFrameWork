///=====================================================
/// - FileName:      ItemKit.cs
/// - NameSpace:     YukiFrameWork.Knaspack
/// - Created:       Yuki
/// - Email:         1274672030@qq.com
/// - Description:   背包系统套件
/// - Creation Time: 2023/12/26 16:04:48
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================

using YukiFrameWork.ABManager;

namespace YukiFrameWork.Knaspack
{
    public class ItemKit
    {
        public static ItemConfigBase Config { get; private set; }
        public static bool IsInited = false;

        /// <summary>
        /// 背包模块初始化
        /// </summary>
        /// <param name="projectName">资源模块名</param>
        /// <param name="handItemName">手部物品预制体名称</param>
        public static void Init(string projectName,string handItemName,ItemConfigBase config = default)
        {
            if (IsInited) return;

            if (config == default)
                Config = new ItemDefaultConfig();
            else Config = config;

            Config.Init(projectName,handItemName);
            IsInited = true;
        }    

        public static void SetHandItem(string itemName)
        {
            InventoryManager manager = InventoryManager.Instance;
            if (string.IsNullOrEmpty(Config.ProjectName))
            {
                "当前未初始化ItemKit，请调用一次ItemKit.Init()方法,并传入资源模块名！".LogInfo(Log.E);
                return;
            }
            manager.PickedItem = GameObjectLoader.Load(Config.ProjectName, itemName).GetComponent<ItemUI>();
            manager.RemoveItem();
        }
        /// <summary>
        /// 取出物品到手上
        /// </summary>
        /// <param name="itemData">物品</param>
        /// <param name="amount">数量</param>
        public static void PickUpItem(ItemData itemData, int amount)
        {
            InventoryManager manager = InventoryManager.Instance;
            if (PickedItem == null)
            {
                manager.PickedItem = Config.GetItemUI(manager.Canvas.transform);
            }
            manager.PickUpItem(itemData,amount);
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
            get => InventoryManager.I.IsPickedItem;
        }      
        public static ItemUI PickedItem
        {
            get => InventoryManager.I.PickedItem;
        }
        #endregion

        /// <summary>
        /// 判断鼠标位置是否在ui上
        /// </summary>
        public static bool IsItemUIExit
        {
            get => InventoryManager.I.IsItemUIExit;
            set => InventoryManager.I.IsItemUIExit = value;
        }
        /// <summary>
        /// 把手里的物品清空
        /// </summary>
        public static void ClearItem()
        {
            InventoryManager manager = InventoryManager.Instance;
            manager.RemoveItem();
        }

        /// <summary>
        /// 清理手里指定数量的物品
        /// </summary>
        /// <param name="count"></param>
        public static void RemoveItem(int count)
        {
            InventoryManager manager = InventoryManager.Instance;
            manager.RemoveItem(count);
        }
    }
}