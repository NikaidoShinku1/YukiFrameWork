///=====================================================
/// - FileName:      UISlot.cs
/// - NameSpace:     YukiFrameWork.Item
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/4/18 18:36:59
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using Sirenix.OdinInspector;
namespace YukiFrameWork.Item
{
    /// <summary>
    /// IUISlot的视觉控制接口。在框架默认情况下，只支持Text与Image，当用户希望自由定制组件进行更新物品的UI时。可在UISlot下赋值
    /// </summary>
    public interface IUISlotVisual
    {
        /// <summary>
        /// 更新数量
        /// </summary>
        /// <param name="slot">物品插槽</param>
        void UpdateCount(int count);

        /// <summary>
        /// 物品数量的UI
        /// </summary>
        MaskableGraphic ItemCount { get; }
        /// <summary>
        /// 物品数量的图标
        /// </summary>
        MaskableGraphic ItemIcon { get; }
        /// <summary>
        /// 设置图标
        /// </summary>
        /// <param name="sprite"></param>
        void SetIcon(Sprite sprite);   
    }

    public class DefaultUISlotVisual : IUISlotVisual
    {
        private Image iconImage;
        private Text countText;
        public DefaultUISlotVisual(Image image, Text text)
        {
            this.iconImage = image;
            this.countText = text;
        }
        public MaskableGraphic ItemCount => countText;

        public MaskableGraphic ItemIcon => iconImage;

        public void SetIcon(Sprite sprite)
        {
            iconImage.sprite = sprite;
        }

        public void UpdateCount(int count)
        {
            if (count == 0)
                this.countText.text = string.Empty;
            else this.countText.text = count.ToString();
        }

    }
    [DisableViewWarning]
	public class UISlot : YMonoBehaviour
	{
        [SerializeField]
        private Image Icon;
        [SerializeField]
		private Text Count;

        public Slot Slot { get;private set; }
   
        /// <summary>
        /// UISlot自由定制视觉接口。当不希望使用内部自带的Image与Text时，继承UISlot添加组件/或直接使用后为该属性赋值即可。
        /// </summary>
        public IUISlotVisual Visual { get; set; }

        public virtual void UpdateView(Slot slot)
        {          
            if (slot.ItemCount <= 0)
            {
                Visual.UpdateCount(0);
                Visual.ItemIcon.Hide();               
            }
            else
            {
                if (slot.Item.IsStackable)
                {
                    Visual.UpdateCount(slot.ItemCount);
                    Visual.ItemCount.Show();                  
                }
                else
                {
                    Visual.ItemCount.Hide();
                }

                Visual.ItemIcon.Show();

                if (Slot.Item?.Icon)
                {                    
                    Visual.SetIcon(Slot.Item.Icon);                  
                }
            }     
        }      
        public UISlot InitSlot(Slot slot)
        {
            this.Slot?.OnItemChanged.UnRegister(UpdateView);
            this.Slot = slot;

            void UpdateView()
            {
                this.UpdateView(slot);
            }

            this.Slot.OnItemChanged.RegisterEvent(UpdateView).UnRegisterWaitGameObjectDisable(gameObject);
            UpdateView();
            Slot.slotGroup.SlotInitInvoke(this);
            return this;
        }
        [LabelText("是否支持默认拖拽事件注册")]
        public bool IsAutoRegisterDrag = true;

        [LabelText("拖拽到非插槽位置是否自动销毁"),ShowIf(nameof(IsAutoRegisterDrag))]
        public bool IsPointerDestory = true;

        protected override void Awake()
        {
            base.Awake();
          
            //只有没有赋值的情况下才对Visual赋值
            this.Visual ??= new DefaultUISlotVisual(Icon,Count);
        }

        protected virtual void Start()
        {
            
            if (!IsAutoRegisterDrag) return;
            this.BindBeginDragEvent(OnBeginDrag);
            this.BindDragEvent(OnDrag);
            this.BindEndDragEvent(OnEndDrag);
            this.BindPointerEnterEvent(OnPointerEnter);
            this.BindPointerExitEvent(OnPointerExit);
            this.BindSelectEvent(OnSelect);
            this.BindDeselectEvent(OnDeselect);
        }

        protected virtual void OnSelect(BaseEventData eventData)
        {
            Slot.slotGroup.SlotSelectInvoke(this);
        }

        protected virtual void OnDeselect(BaseEventData eventData)
        {
            Slot.slotGroup.SlotDeselectInvoke(this);            
        }

        protected virtual void OnPointerExit(PointerEventData eventData)
        {
            Slot.slotGroup.SlotPointerExitInvoke(this);        
            if (Equals(ItemKit.CurrentSlotPointer, this))
                ItemKit.CurrentSlotPointer = null;
        }

        protected virtual void OnPointerEnter(PointerEventData eventData)
        {
            Slot.slotGroup.SlotPointerEnterInvoke(this);          
            ItemKit.CurrentSlotPointer = this;
        }

        private bool mDragging = false;
        protected virtual void OnBeginDrag(PointerEventData eventData)
        {
            if (mDragging) return;
            mDragging = true;
            var canvas = Visual.ItemIcon.gameObject.AddComponent<Canvas>();
            canvas.overrideSorting = true;
            canvas.sortingOrder = 1000;       
            Update_ItemPos();
        }

        private void Update_ItemPos()
        {                   
            var mousePosition = Input.mousePosition;
            if (RectTransformUtility
                .ScreenPointToLocalPointInRectangle(transform as RectTransform, mousePosition, null, out var localPosition))
            {
                Visual.ItemIcon.SetLocalPosition2D(localPosition);
            }
        }

        protected virtual void OnDrag(PointerEventData eventData)
        {
            if (mDragging || Slot.ItemCount != 0)
                Update_ItemPos();
        }

        protected virtual void OnEndDrag(PointerEventData eventData)
        {
            if (mDragging)
            {
                var canvas = Visual.ItemIcon.GetComponent<Canvas>();
                canvas.Destroy();
                mDragging = false;
                Visual.ItemIcon.SetLocalPositionIdentity();

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
                else if(IsPointerDestory)
                {
                    Slot.Item = null;
                    Slot.ItemCount = 0;
                    Slot.OnItemChanged.SendEvent();
                }              
            }
        }       
    }
}
