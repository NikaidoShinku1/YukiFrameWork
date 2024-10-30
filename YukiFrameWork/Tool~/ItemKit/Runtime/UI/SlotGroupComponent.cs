///=====================================================
/// - FileName:      SlotGroupComponent.cs
/// - NameSpace:     YukiFrameWork.Item
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/4/20 23:16:09
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using Sirenix.OdinInspector;
using System.Collections.Generic;
namespace YukiFrameWork.Item
{
	[DisableViewWarning]
	public class SlotGroupComponent : MonoBehaviour
	{	 
		[Serializable]
		public class SlotConfig
		{
			public string itemKey;
			public int count = 1;
            public UnityEngine.Events.UnityEvent<UISlot> mOnSlotInit;
            public UnityEngine.Events.UnityEvent<UISlot> mOnSlotSelect;
            public UnityEngine.Events.UnityEvent<UISlot> mOnSlotDeselect;
            public UnityEngine.Events.UnityEvent<UISlot> mOnSlotPointerEnter;
            public UnityEngine.Events.UnityEvent<UISlot> mOnSlotPointerExit;
        }

		[LabelText("SlotGroup分组的标识"),SerializeField]
		private string mGroupKey;
		[LabelText("所有的物品信息配置"),SerializeField]
		private List<SlotConfig> configs = new List<SlotConfig>();
		public IReadOnlyList<SlotConfig> Configs => configs;
		public string GroupKey => mGroupKey;

        private void Awake()
        {						
			var group = ItemKit.CreateSlotGroup(mGroupKey);			
			foreach (var config in configs)
			{

				group.CreateSlot(ItemKit.GetItemByKey(config.itemKey), config.count)
					.OnSlotInit(slot => config.mOnSlotInit?.Invoke(slot))
					.OnSlotSelect(slot => config.mOnSlotSelect?.Invoke(slot))
					.OnSlotPointerEnter(slot => config.mOnSlotPointerEnter?.Invoke(slot))
					.OnSlotPointerExit(slot => config.mOnSlotPointerExit?.Invoke(slot))
					.OnSlotDeselect(slot => config.mOnSlotDeselect?.Invoke(slot));
			}
        }     
    }
}
