///=====================================================
/// - FileName:      UISlotGroup.cs
/// - NameSpace:     YukiFrameWork.Item
/// - Description:   UI Slot分组
/// - Creation Time: 2024/4/19 22:05:26
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
	public class UISlotGroup : MonoBehaviour
	{
        public enum UISlotGenericType
        {
            [LabelText("使用插槽预制体生成")]
            Template,
            [LabelText("使用已经存在的插槽")]
            SlotExist
        }
        [SerializeField,LabelText("SlotGroup分组的标识：")]
        private string mGroupKey;

        public string GroupKey
        {
            get => mGroupKey;
            set
            {
                if (!mGroupKey.Equals(value))
                {
                    mGroupKey = value;
                    Refresh();
                }
            }
        }

        [LabelText("UI Slot生成的类型:"),PropertySpace()]
        public UISlotGenericType Type;

        [HideIf(nameof(Type),UISlotGenericType.SlotExist)]
		[LabelText("UISlot的预制体:")]
		public UISlot UISlotPrefab;
        [HideIf(nameof(Type), UISlotGenericType.SlotExist)]
        [LabelText("UISlot的根节点:")]
		public RectTransform UISlotRoot;

        [LabelText("已经存在的分组集合"), HideIf(nameof(Type),UISlotGenericType.Template)]
        public List<UISlot> existSlots = new List<UISlot>();     

        private void Start()
        {           
            Refresh();    
        }

        private void Refresh()
        {
            if (Type == UISlotGenericType.Template)
            {
                UISlotPrefab.Hide();
                UISlotRoot.DestroyChildrenWithCondition(core => core.GetComponent<UISlot>());

                foreach (var slot in ItemKit.GetSlotGroup(GroupKey).Slots)
                {
                    UISlotPrefab.Instantiate(UISlotRoot).InitSlot(slot).Show();
                }
            }
            else if (Type == UISlotGenericType.SlotExist)
            {
                for (int i = 0; i < existSlots.Count; i++)
                {
                    var slot = existSlots[i];
                    slot.InitSlot(ItemKit.GetSlotGroup(GroupKey).Slots[i]);
                }
            }
           
        }
    }
}
