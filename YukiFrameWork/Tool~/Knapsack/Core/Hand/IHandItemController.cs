///=====================================================
/// - FileName:      IHandItemController.cs
/// - NameSpace:     YukiFrameWork.Knapsack
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/2/16 14:01:25
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
namespace YukiFrameWork.Knapsack
{
	public interface IHandItemController
	{
        /// <summary>
        /// 初始化方法
        /// </summary>
        void Init();
        /// <summary>
        /// 每帧更新
        /// </summary>
        void Update();

        /// <summary>
        /// 手上的物品
        /// </summary>
		ItemUI PickedItem { get; set; }		

        /// <summary>
        /// 手部物品会挂在哪一个画布(如果要做物品跟随鼠标移动)
        /// </summary>
        Canvas Canvas { get; }

        /// <summary>
        /// 当前鼠标是否在面板UI上
        /// </summary>
        bool IsItemUIExit { get; set;}

        /// <summary>
        /// 捡起指定数量的物品到手上
        /// </summary>
        /// <param name="item">物品</param>
        /// <param name="amount">数量</param>
        void PickUpItem(ItemData item, int amount);

        /// <summary>
        /// 把物品从手上完全删除
        /// </summary>
        void ClearItem();

        /// <summary>
        /// 删除指定数量的物品
        /// </summary>
        /// <param name="amount"></param>
        void RemoveItem(int amount);

        /// <summary>
        /// 手上是不是已经拿着物品了
        /// </summary>
        bool IsPickedItem { get; }
    }
}
