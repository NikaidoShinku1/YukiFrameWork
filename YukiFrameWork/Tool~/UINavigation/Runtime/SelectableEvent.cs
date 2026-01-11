
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.InputSystem.Layouts;

namespace YukiFrameWork.UI
{

    public enum SelectionState
    {
        None,
        Normal,
        Highlighted,
        Pressed,
        Selected,
        Disabled
    }

    public enum SelectEnterState
    {
        [LabelText("UI监听事件")]
        UITrigger,
        [LabelText("自定绑定按键触发")]
        CustomKeyBind
    }
    [Serializable]
    public class CustomPressAction
    {
        [LabelText("按键路径"), InputControl()]
        public string _pressContorlPath;
        [HideInInspector]
        public InputControl customPressControl;
        /// <summary>
        /// 是否允许触发
        /// </summary>
        internal bool isAllowTrigger;
    }
    /// <summary>
    /// Selectable事件监听
    /// </summary>
    [RequireComponent(typeof(Selectable))]
    [DisallowMultipleComponent]
    public class SelectableEvent : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
    {

        #region 字段

        private Selectable selectable;
        private GamepadPanelExtension panelExtension;
        [SerializeField,LabelText("是否开启高亮")]
        private bool enableHighlighted = false;

        [SerializeField,LabelText("是否开启按下")]
        private bool enablePressed = false;

        [SerializeField,LabelText("是否开启选择")]
        private bool enableSelected = false;

        //private bool active;

        [LabelText("是否被禁用")]
        private bool _disabled;
        [LabelText("是否按下")]
        private bool _pressed;
        [LabelText("是否选中")]
        private bool _selected;
        [LabelText("是否高亮")]
        private bool _highlighted;
        [LabelText("普通状态")]
        private bool _normal;

        #endregion

        #region 属性

        private static GameObject currentSelectedGameObject = null;

        private static GameObject CurrentSelectedGameObject
        {
            get
            {
                if (EventSystem.current != null)
                    currentSelectedGameObject = EventSystem.current.currentSelectedGameObject;

                return currentSelectedGameObject;
            }
        }

        private bool isPointerInside { get; set; }

        private bool isPointerDown { get; set; }

        private bool hasSelection
        {
            get
            {
                return CurrentSelectedGameObject == gameObject;
            }
        }


        public bool EnableHighlighted
        {
            get
            {
                return enableHighlighted;
            }
            set
            {
                enableHighlighted = value;
            }

        }

        public bool EnablePressed
        {
            get
            {
                return enablePressed;
            }
            set
            {
                enablePressed = value;
            }
        }

        public bool EnableSelected
        {
            get
            {
                return enableSelected;
            }
            set
            {
                enableSelected = value;
            }
        }



        public bool Disabled
        {
            get
            {
                return _disabled;
            }
            private set
            {
                if (_disabled == value) return;
                _disabled = value;
                if (_disabled)
                    onDisabled?.Invoke();
            }
        }

        public bool Pressed
        {
            get
            {
                return _pressed;
            }
            private set
            {
                if (_pressed == value) return;
                _pressed = value;
                if((!panelExtension && _pressed) || (panelExtension && panelExtension.PressEnterState == SelectEnterState.UITrigger && _pressed))              
                    onPressed?.Invoke();
            }
        }

        public bool Selected
        {
            get
            {
                return _selected;
            }
            private set
            {
                if (_selected == value) return;
                _selected = value;
                if (_selected)
                    onSelected?.Invoke();
            }
        }

        public bool Highlighted
        {
            get
            {
                return _highlighted;
            }
            private set
            {
                if (_highlighted == value) return;
                _highlighted = value;
                if (_highlighted)
                    onHighlighted?.Invoke();
            }
        }

        public bool Normal
        {
            get
            {
                return _normal;
            }
            private set
            {
                if (_normal == value) return;
                _normal = value;
                if (_normal)
                    onNormal?.Invoke();
            }
        }

       // protected override string controlPathInternal { get => _pressContorlPath; set => _pressContorlPath = value; }

        #endregion

        #region 

        [LabelText("Normal状态,true:进入,false:退出!")]
        public UnityEvent onNormal;
        [LabelText("Highlighted状态,true:进入,false:退出!"),EnableIf(nameof(enableHighlighted))]
        public UnityEvent onHighlighted;
        [LabelText("Pressed状态,true:进入,false:退出!"),EnableIf(nameof(enablePressed))]
        public UnityEvent onPressed;
        [LabelText("Selected状态,true:进入,false:退出!"),EnableIf(nameof(enableSelected))]
        public UnityEvent onSelected;
        [LabelText("Disabled状态,true:进入,false:退出!")]
        public UnityEvent onDisabled;

        #endregion


        private void Awake()
        {
            selectable = GetComponent<Selectable>();
            panelExtension = GetComponentInParent<GamepadPanelExtension>();
        }

        private void OnEnable()
        {
            UpdateState();            
            //active = true;
        }

        private void Update()
        {
            UpdateState();
        }


        private void OnDisable()
        {           
            isPointerInside = false;
            //active = false;
            _normal = false;          
        }

        private bool GetDisabled()
        {
            return !selectable.interactable;
        }

        private bool GetPressed()
        {
            if (GetDisabled()) return false;
            return isPointerDown && enablePressed;
        }

        private bool GetHighlighted()
        {
            if (GetDisabled()) return false;
            return isPointerInside && enableHighlighted;
        }

        private bool GetSelection()
        {
            if (GetDisabled()) return false;
            return hasSelection && enableSelected;
        }

        private bool GetNormal()
        {
            if (GetDisabled()) return false;
            if (GetPressed()) return false;
            if (GetHighlighted()) return false;
            if (GetSelection()) return false;
            return true;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            //if (!enablePressedTrigger) return;           
            isPointerDown = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            isPointerDown = false;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            isPointerInside = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isPointerInside = false;
        }


        private void UpdateState()
        {
            // 当由普通状态 变为其他状态时 先处理普通状态事件 再处理其他状态事件
            if (_normal)
                Normal = GetNormal();

            Disabled = GetDisabled();
            Pressed = GetPressed();
            Highlighted = GetHighlighted();
            Selected = GetSelection();

            // 当由其他状态变为普通状态时 先处理其他的状态事件 再处理普通状态事件
            if (!_normal)
                Normal = GetNormal();

            // 总的来说就是要先处理当前状态事件,然后再处理下一个状态的事件,确保事件有序执行 
        }
    }
}
