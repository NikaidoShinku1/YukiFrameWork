///=====================================================
/// - FileName:      UISlot.cs
/// - NameSpace:     YukiFrameWork.Item
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/4/18 18:36:59
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
namespace YukiFrameWork.Item
{
    [DisableViewWarning]
	public class UISlot : MonoBehaviour
	{
        public Image Icon;
		public Text Count;

        public Slot Slot { get;private set; }
      
        public UISlot InitSlot(Slot slot)
        {
            this.Slot?.OnItemChanged.UnRegister(UpdateView);
            this.Slot = slot;

            void UpdateView()
            {
                if (slot.ItemCount == 0)
                {
                    Count.text = string.Empty;
                    Icon.Hide();
                }
                else
                {
                    if (slot.Item.IsStackable)
                    {
                        Count.text = slot.ItemCount.ToString();
                        Count.Show();
                    }
                    else Count.Hide();
                    Icon.Show();

                    if (Slot.Item?.GetIcon)
                    {
                        Icon.sprite = Slot.Item.GetIcon;
                    }
                }
            }

            this.Slot.OnItemChanged.RegisterEvent(UpdateView).UnRegisterWaitGameObjectDestroy(gameObject);
            UpdateView();
            Slot.slotGroup.SlotInitInvoke(this);
            return this;
        }

        private void Start()
        {           
            this.BindBeginDragEvent(OnBeginDrag);
            this.BindDragEvent(OnDrag);
            this.BindEndDragEvent(OnEndDrag);
            this.BindPointerEnterEvent(OnPointerEnter);
            this.BindPointerExitEvent(OnPointerExit);
            this.BindSelectEvent(OnSelect);
            this.BindDeselectEvent(OnDeselect);
        }

        private void OnSelect(BaseEventData eventData)
        {
            Slot.slotGroup.SlotSelectInvoke(this);
        }

        private void OnDeselect(BaseEventData eventData)
        {
            Slot.slotGroup.SlotDeselectInvoke(this);            
        }

        private void OnPointerExit(PointerEventData eventData)
        {
            Slot.slotGroup.SlotPointerExitInvoke(this);        
            if (Equals(ItemKit.CurrentSlotPointer, this))
                ItemKit.CurrentSlotPointer = null;
        }

        private void OnPointerEnter(PointerEventData eventData)
        {
            Slot.slotGroup.SlotPointerEnterInvoke(this);          
            ItemKit.CurrentSlotPointer = this;
        }

        private bool mDragging = false;
        private void OnBeginDrag(PointerEventData eventData)
        {
            if (mDragging) return;
            mDragging = true;
            var canvas = Icon.gameObject.AddComponent<Canvas>();
            canvas.overrideSorting = true;
            canvas.sortingOrder = 1000;       
            Update_ItemPos();
        }

        private void Update_ItemPos()
        {                   
            var mousePosition = Input.mousePosition;
            if(RectTransformUtility
                .ScreenPointToLocalPointInRectangle(transform as RectTransform, mousePosition, null, out var localPosition))
                Icon.SetLocalPosition2D(localPosition);
        }

        private void OnDrag(PointerEventData eventData)
        {
            if (mDragging || Slot.ItemCount != 0)
                Update_ItemPos();
        }

        private void OnEndDrag(PointerEventData eventData)
        {
            if (mDragging)
            {
                var canvas = Icon.GetComponent<Canvas>();
                canvas.Destroy();
                mDragging = false;
                Icon.SetLocalPositionIdentity();

                if (ItemKit.CurrentSlotPointer)
                {
                    var uiSlot = ItemKit.CurrentSlotPointer;
                    RectTransform rectTransform = uiSlot.transform as RectTransform;

                    if (RectTransformUtility.RectangleContainsScreenPoint(rectTransform, Input.mousePosition))
                    {                      
                        if (Slot.ItemCount != 0 && uiSlot.Slot.CheckCondition(Slot.Item))
                        {                           
                            var current = uiSlot.Slot.Item;
                            var count = uiSlot.Slot.ItemCount;

                            uiSlot.Slot.Item = Slot.Item;
                            uiSlot.Slot.ItemCount = Slot.ItemCount;

                            Slot.Item = current;
                            Slot.ItemCount = count;
                            uiSlot.Slot.OnItemChanged.SendEvent();
                            Slot.OnItemChanged.SendEvent();                          
                            
                        }
                    }
                }
                else
                {
                    Slot.Item = null;
                    Slot.ItemCount = 0;
                    Slot.OnItemChanged.SendEvent();
                }              
            }
        }

        
    }
}
