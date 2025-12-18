
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using YukiFrameWork.InputSystemExtension;
using Sirenix.OdinInspector;

namespace YukiFrameWork.UI
{ 

    public enum NavigationType
    {
        [LabelText("自动选择")]
        Auto,
        [LabelText("指定具体的几个组件")]
        Explicit,
        [LabelText("不参与导航")]
        None
    }

    [Serializable]
    public class NavigationOption
    {
        public NavigationType Type = NavigationType.Auto;
        [ShowIf(nameof(Type), NavigationType.Explicit)]
        public List<SelectableNavigation> selectables;
    }

    /// <summary>
    /// UI组件切换导航(如果UI组件能够通过手柄选中和切换,需要挂上该组件!)
    /// </summary>
    [RequireComponent(typeof(Selectable))]
    [DisallowMultipleComponent]
    public class SelectableNavigation : MonoBehaviour
    {

        #region 静态字段

        internal static List<SelectableNavigation> navigations = new List<SelectableNavigation>();

        public static SelectableNavigation CurrentSelectedNavigation { get; set; }

        internal static void SendCurrentSelectableEvent(SelectionState selectionState, GamepadPanelExtension gamepadPanelExtension)
        {
            if (CurrentSelectedNavigation.gamepadPanel != gamepadPanelExtension)
                return;
            SendCurrentSelectableEvent(selectionState);
        }
        /// <summary>
        /// 手动发送当前导航选择对象的事件
        /// <para>Tips:需要挂载SelectableEvent组件</para>
        /// </summary>
        /// <param name="selectionState"></param>
        public static void SendCurrentSelectableEvent(SelectionState selectionState)
        {
            if (!CurrentSelectedNavigation) return;

            if (selectionState == SelectionState.None) return;

            SelectableEvent selectableEvent = CurrentSelectedNavigation.GetComponent<SelectableEvent>();

            if (!selectableEvent) return;
            switch (selectionState)
            {
                case SelectionState.Normal:
                    selectableEvent.onNormal?.Invoke();
                    break;
                case SelectionState.Highlighted:
                    selectableEvent.onHighlighted?.Invoke();
                    break;
                case SelectionState.Pressed:
                    selectableEvent.onPressed?.Invoke();
                    break;
                case SelectionState.Selected:
                    selectableEvent.onSelected?.Invoke();
                    break;
                case SelectionState.Disabled:
                    selectableEvent.onDisabled?.Invoke();
                    break;
                default:
                    break;
            }
        }
        #endregion


        #region 字段


        private Selectable selectable;

        /// <summary>
        /// 是否默认选中
        /// </summary>
        [LabelText("是否默认选中!")]
        public bool defaultSelected = false;

        private GamepadPanelExtension gamepadPanel;

        [SerializeField]
        private NavigationOption up;
        [SerializeField]
        private NavigationOption down;
        [SerializeField]
        private NavigationOption left;
        [SerializeField]
        private NavigationOption right;


        private bool childOfScrollView = false;

        private bool childOfDropdown = false;
       
        private List<SelectableNavigation> sameParentNavigations;

        private float upHoldTimer;
        private float downHoldTimer;
        private float leftHoldTimer;
        private float rightHoldTimer;

        #endregion

        #region 属性
        private GamepadPanelExtension GamepadPanel
        {
            get
            {
                if (!gamepadPanel)
                    gamepadPanel = GetComponentInParent<GamepadPanelExtension>();

                return gamepadPanel;
            }
        }
         
        internal bool Active
        {
            get
            {
                if (!gameObject.activeInHierarchy)
                    return false;

                if (!GamepadPanel || !GamepadPanel.BasePanel || !GamepadPanel.BasePanel.IsActive || GamepadPanel.BasePanel.IsPaused)
                    return false;

                if (!GamepadPanelExtension.IsTop(GamepadPanel.BasePanel))
                    return false;

                return true;
            }
        }

        /// <summary>
        /// 上方导航选项
        /// </summary>
        public NavigationOption Up => up;

        /// <summary>
        /// 下方导航选项
        /// </summary>
        public NavigationOption Down => down;

        /// <summary>
        /// 左方导航选项
        /// </summary>
        public NavigationOption Left => left;

        /// <summary>
        /// 右方导航选项
        /// </summary>
        public NavigationOption Right => right;

        /// <summary>
        /// UI组件
        /// </summary>
        public Selectable Selectable => selectable;
 
        internal List<SelectableNavigation> SameParentNavigations
        {
            get
            {               
                if (sameParentNavigations == null)
                    sameParentNavigations = new List<SelectableNavigation>();

                sameParentNavigations.Clear();

                foreach (var item in navigations)
                {
                    if (!item) continue;
                    if (item.transform.parent != transform.parent) continue;

                    sameParentNavigations.Add(item);
                }

                return sameParentNavigations;
            }
        }
         
        internal bool ChildOfScrollView => childOfScrollView;
         
        internal ScrollRect ParentScrollRect { get; private set; }

        #endregion

        #region 静态方法

        /// <summary>
        /// 如果在部分平台以及插件适配上该方法不触发，则可以手动触发
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void OnStartGame()
        {
            navigations.Clear();
        }

        #endregion

        #region 生命周期

        private void Awake()
        {
            selectable = GetComponent<Selectable>();
            Navigation navigation = selectable.navigation;
            navigation.mode = Navigation.Mode.None;
            selectable.navigation = navigation;
        }

        private void OnEnable()
        {
            navigations.Add(this);
            if (defaultSelected && EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(gameObject);

            if (transform.parent)
            {
                ParentScrollRect = transform.parent.GetComponentInParent<ScrollRect>();
                childOfDropdown = transform.parent.GetComponentInParent<Dropdown>() != null;
            }
            else
            {
                ParentScrollRect = null;
                childOfDropdown = false;
            }

            childOfScrollView = ParentScrollRect != null;
        }

        private void OnTransformParentChanged()
        {
            if (transform.parent)
            {
                ParentScrollRect = transform.parent.GetComponentInParent<ScrollRect>();
                childOfDropdown = transform.parent.GetComponentInParent<Dropdown>() != null;
            }
            else
            {
                ParentScrollRect = null;
                childOfDropdown = false;
            }

            childOfScrollView = ParentScrollRect != null;
        }

        private void OnDisable()
        {
            navigations.Remove(this);

            if (CurrentSelectedNavigation == this)
                CurrentSelectedNavigation = null;
        }

      

        private void Update()
        {

            if (!Application.isFocused)
                return;

            // 控制导航
            if (selectable.navigation.mode != Navigation.Mode.None)
            {
                Navigation navigation = selectable.navigation;
                navigation.mode = Navigation.Mode.None;
                selectable.navigation = navigation;
            }

            if (!IsSelected())
            {
                if (CurrentSelectedNavigation == this)
                    CurrentSelectedNavigation = null;
                return;
            }

            CurrentSelectedNavigation = this;
            
            if (gamepadPanel && gamepadPanel.KeyBoardListener)
            {
                //监听键盘上下左右
                foreach (var keyboard in InputKit.AllKeyboards)
                {
                    if (keyboard.upArrowKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame)
                    {
                        // 上
                        Selectable upObj = FindSelectableUp();
                        if (upObj != null)
                            StartCoroutine(DelaySelect(upObj));
                        break;
                    }

                    if (keyboard.downArrowKey.wasPressedThisFrame || keyboard.sKey.wasPressedThisFrame)
                    {
                        // 下
                        Selectable upObj = FindSelectableDown();
                        if (upObj != null)
                            StartCoroutine(DelaySelect(upObj));
                        break;
                    }

                    if (keyboard.leftArrowKey.wasPressedThisFrame || keyboard.aKey.wasPressedThisFrame)
                    {
                        // 左
                        Selectable upObj = FindSelectableLeft();
                        if (upObj != null)
                            StartCoroutine(DelaySelect(upObj));
                        break;
                    }

                    if (keyboard.rightArrowKey.wasPressedThisFrame || keyboard.dKey.wasPressedThisFrame)
                    {
                        // 右
                        Selectable upObj = FindSelectableRight();
                        if (upObj != null)
                            StartCoroutine(DelaySelect(upObj));
                        break;
                    }
                }
            }
            // 监听手柄上下左右
            foreach (var gamepad in InputKit.AllGamepads)
            {
                if (LeftStickUpIsPressed(gamepad) || gamepad.rightStick.up.wasPressedThisFrame || gamepad.dpad.up.wasPressedThisFrame)
                {
                    // 上
                    Selectable upObj = FindSelectableUp();
                    if (upObj != null)
                        StartCoroutine(DelaySelect(upObj));
                    break;
                }

                if (LeftStickDownIsPressed(gamepad) || gamepad.rightStick.down.wasPressedThisFrame || gamepad.dpad.down.wasPressedThisFrame)
                {
                    // 下
                    Selectable downObj = FindSelectableDown();
                    if (downObj != null)
                        StartCoroutine(DelaySelect(downObj));
                    break;
                }

                if (LeftStickLeftIsPressed(gamepad) || gamepad.rightStick.left.wasPressedThisFrame)
                {
                    // 左
                    Selectable leftObj = FindSelectableLeft();

                    //Debug.LogFormat("查找左边的游戏物体:{0}", leftObj != null ? leftObj.name : null);

                    if (leftObj != null)
                        StartCoroutine(DelaySelect(leftObj));
                    break;
                }

                if (LeftStickRightIsPressed(gamepad) || gamepad.rightStick.right.wasPressedThisFrame)
                {
                    // 右 
                    Selectable rightObj = FindSelectableRight();

                    if (rightObj != null)
                        StartCoroutine(DelaySelect(rightObj));

                    break;
                }

                if (gamepad.dpad.left.wasPressedThisFrame)
                {
                    if (selectable is Slider)
                        return;

                    // 左 ( 先判断是不是滑动条，如果是滑动条就不处理了 )
                    Selectable leftObj = FindSelectableLeft();

                    if (leftObj != null)
                        StartCoroutine(DelaySelect(leftObj));
                    break;
                }

                if (gamepad.dpad.right.wasPressedThisFrame)
                {
                    // 右 ( 先判断是不是滑动条，如果是滑动条就不用处理了 )
                    if (selectable is Slider)
                        return;

                    Selectable rightObj = FindSelectableRight();

                    if (rightObj != null)
                        StartCoroutine(DelaySelect(rightObj));
                    break;
                }


            }


            if (GamepadUpPress())
            {
                upHoldTimer += Time.unscaledDeltaTime;
                if (upHoldTimer > 0.2f)
                {
                    Selectable upObj = FindSelectableUp();
                    if (upObj != null)
                        StartCoroutine(DelaySelect(upObj));

                    upHoldTimer = 0;
                }
            }
            else
            {
                upHoldTimer = 0;
            }

            if (GamepadDownPress())
            {
                downHoldTimer += Time.unscaledDeltaTime;
                if (downHoldTimer > 0.2f)
                {
                    Selectable upObj = FindSelectableDown();
                    if (upObj != null)
                        StartCoroutine(DelaySelect(upObj));

                    downHoldTimer = 0;
                }
            }
            else
            {
                downHoldTimer = 0;
            }

            if (GamepadLeftPress())
            {
                leftHoldTimer += Time.unscaledDeltaTime;
                if (leftHoldTimer > 0.2f)
                {
                    Selectable upObj = FindSelectableLeft();
                    if (upObj != null)
                        StartCoroutine(DelaySelect(upObj));

                    leftHoldTimer = 0;
                }
            }
            else
            {
                leftHoldTimer = 0;
            }

            if (GamepadRightPress())
            {
                rightHoldTimer += Time.unscaledDeltaTime;
                if (rightHoldTimer > 0.2f)
                {
                    Selectable upObj = FindSelectableRight();
                    if (upObj != null)
                        StartCoroutine(DelaySelect(upObj));

                    rightHoldTimer = 0;
                }
            }
            else
            {
                rightHoldTimer = 0;
            }


        }

        private bool LeftStickUpIsPressed(Gamepad gamepad)
        {
            // 如果在滚动列表下面 左边的摇杆只能用来操作滚动列表
            if (childOfScrollView)
            {
                return false;
            }

            return gamepad.leftStick.up.wasPressedThisFrame;
        }

        private bool LeftStickDownIsPressed(Gamepad gamepad)
        {
            // 如果在滚动列表下面 左边的摇杆只能用来操作滚动列表
            if (childOfScrollView)
            {
                return false;
            }

            return gamepad.leftStick.down.wasPressedThisFrame;
        }


        private bool LeftStickLeftIsPressed(Gamepad gamepad)
        {
            // 如果在滚动列表下面 左边的摇杆只能用来操作滚动列表
            if (childOfScrollView)
            {
                return false;
            }

            return gamepad.leftStick.left.wasPressedThisFrame;
        }

        private bool LeftStickRightIsPressed(Gamepad gamepad)
        {
            // 如果在滚动列表下面 左边的摇杆只能用来操作滚动列表
            if (childOfScrollView)
            {
                return false;
            }

            return gamepad.leftStick.right.wasPressedThisFrame;
        }


        private bool GamepadUpPress()
        {
            foreach (var gamepad in InputKit.AllGamepads)
            {
                if (gamepad.leftStick.up.isPressed && !childOfScrollView)
                    return true;

                if (gamepad.rightStick.up.isPressed)
                    return true;

                if (gamepad.dpad.up.isPressed)
                    return true;
            }

            return false;
        }

        private bool GamepadDownPress()
        {
            foreach (var gamepad in InputKit.AllGamepads)
            {
                if (gamepad.leftStick.down.isPressed && !childOfScrollView)
                    return true;

                if (gamepad.rightStick.down.isPressed)
                    return true;

                if (gamepad.dpad.down.isPressed)
                    return true;
            }

            return false;
        }

        private bool GamepadLeftPress()
        {
            foreach (var gamepad in InputKit.AllGamepads)
            {
                if (gamepad.leftStick.left.isPressed && !childOfScrollView)
                    return true;

                if (gamepad.rightStick.left.isPressed)
                    return true;

                if (gamepad.dpad.left.isPressed)
                    return true;
            }

            return false;
        }

        private bool GamepadRightPress()
        {
            foreach (var gamepad in InputKit.AllGamepads)
            {
                if (gamepad.leftStick.right.isPressed && !childOfScrollView)
                    return true;

                if (gamepad.rightStick.right.isPressed)
                    return true;

                if (gamepad.dpad.right.isPressed)
                    return true;
            }

            return false;
        }

        #endregion

        #region 方法

        private bool IsSelected()
        {

            if (!selectable.IsInteractable())
                return false;

            if (CurrentSelectedNavigation == this)
                return true;

            if (EventSystem.current == null)
                return false;

            if (EventSystem.current.currentSelectedGameObject == gameObject)
                return true;

            return false;
        }
         
        private Selectable FindSelectableUp()
        {
            SelectableNavigation targetUp = null;

            switch (Up.Type)
            {
                case NavigationType.Auto:

                    targetUp = FindSelectableUp(SameParentNavigations);
                    // dropdown 只在同级找
                    if (targetUp == null && !childOfDropdown)
                        targetUp = FindSelectableUp(navigations);

                    break;

                case NavigationType.Explicit:

                    targetUp = FindSelectableUp(Up.selectables);

                    break;
            }

            if (targetUp != null)
                return targetUp.Selectable;

            return null;
        }

        private SelectableNavigation FindSelectableUp(IList<SelectableNavigation> n)
        {
            SelectableNavigation targetUp = null;

            foreach (var item in n)
            {
                if (!item) continue;

                if (!item.selectable)
                    continue;

                if (!item.selectable.IsInteractable())
                    continue;

                if (item == this)
                    continue;

                if (!item.Active)
                    continue;

                if (item.transform.position.y <= transform.position.y)
                    continue;

                if (targetUp == null)
                    targetUp = item;
                else
                {
                    float delta = item.transform.position.y - transform.position.y;
                    float last_delta = targetUp.transform.position.y - transform.position.y;

                    if (delta < last_delta)
                        targetUp = item;
                    else if (Mathf.Abs(delta - last_delta) < 0.001f)
                    {
                        // 计算 x 
                        float deltaX = Mathf.Abs(item.transform.position.x - transform.position.x);
                        float lastDeltaX = Mathf.Abs(targetUp.transform.position.x - transform.position.x);

                        if (deltaX < lastDeltaX)
                            targetUp = item;
                    }
                }
            }

            return targetUp;

        }

        private Selectable FindSelectableDown()
        {
            SelectableNavigation targetDown = null;

            switch (Down.Type)
            {
                case NavigationType.Auto:

                    targetDown = FindSelectableDown(SameParentNavigations);
                    if (targetDown == null && !childOfDropdown)
                        targetDown = FindSelectableDown(navigations);

                    break;

                case NavigationType.Explicit:

                    targetDown = FindSelectableDown(Down.selectables);

                    break;
            }

            if (targetDown != null)
                return targetDown.Selectable;

            return null;
        }

        private SelectableNavigation FindSelectableDown(IList<SelectableNavigation> n)
        {
            SelectableNavigation targetDown = null;

            foreach (var item in n)
            {
                //Debug.LogFormat("name:{0} position_y:{1}",item.name,item.transform.position.y);

                if (!item) continue;

                if (!item.selectable)
                    continue;

                if (!item.selectable.IsInteractable())
                    continue;

                if (item == this)
                    continue;
                if (!item.Active)
                    continue;
                if (item.transform.position.y >= transform.position.y)
                    continue;

                if (targetDown == null)
                    targetDown = item;
                else
                {
                    float delta = Mathf.Abs(item.transform.position.y - transform.position.y);
                    float last_delta = Mathf.Abs(targetDown.transform.position.y - transform.position.y);
                    if (delta < last_delta)
                        targetDown = item;
                    else if (Mathf.Abs(delta - last_delta) < 0.001f)
                    {
                        // 计算 x 
                        float deltaX = Mathf.Abs(item.transform.position.x - transform.position.x);
                        float lastDeltaX = Mathf.Abs(targetDown.transform.position.x - transform.position.x);

                        if (deltaX < lastDeltaX)
                            targetDown = item;
                    }
                }
            }

            return targetDown;
        }

        private Selectable FindSelectableLeft()
        {
            SelectableNavigation targetLeft = null;

            switch (Left.Type)
            {
                case NavigationType.Auto:

                    targetLeft = FindSelectableLeft(SameParentNavigations);

                    // dropdown 只在同级找
                    if (targetLeft == null && !childOfDropdown)
                        targetLeft = FindSelectableLeft(navigations);
                    break;

                case NavigationType.Explicit:
                    targetLeft = FindSelectableLeft(Left.selectables);
                    break;
            }

            if (targetLeft != null)
                return targetLeft.Selectable;

            return null;
        }

        private SelectableNavigation FindSelectableLeft(IList<SelectableNavigation> n)
        {
            SelectableNavigation targetLeft = null;

            foreach (var item in n)
            {
                if (!item) continue;

                if (!item.selectable)
                    continue;

                if (!item.selectable.IsInteractable())
                    continue;

                if (item == this)
                    continue;
                if (!item.Active)
                    continue;
                if (item.transform.position.x >= transform.position.x) continue;

                if (targetLeft == null)
                    targetLeft = item;
                else
                {
                    float delta = Mathf.Abs(item.transform.position.x - transform.position.x);
                    float last_delta = Mathf.Abs(targetLeft.transform.position.x - transform.position.x);
                    if (delta < last_delta)
                        targetLeft = item;
                    else if (Mathf.Abs(delta - last_delta) < 0.001f)
                    {
                        float deltaY = Mathf.Abs(item.transform.position.y - transform.position.y);
                        float lastDeltaY = Mathf.Abs(targetLeft.transform.position.y - transform.position.y);

                        if (deltaY < lastDeltaY)
                            targetLeft = item;

                    }
                }
            }


            return targetLeft;
        }

        private Selectable FindSelectableRight()
        {
            SelectableNavigation targetRight = null;

            switch (Right.Type)
            {
                case NavigationType.Auto:

                    targetRight = FindSelectableRight(SameParentNavigations);
                    // dropdown 只在同级找
                    if (targetRight == null && !childOfDropdown)
                        targetRight = FindSelectableRight(navigations);
                    break;

                case NavigationType.Explicit:
                    targetRight = FindSelectableRight(Right.selectables);
                    break;
            }

            if (targetRight != null)
                return targetRight.Selectable;

            return null;
        }

        private SelectableNavigation FindSelectableRight(IList<SelectableNavigation> n)
        {

            SelectableNavigation targetRight = null;

            foreach (var item in n)
            {
                if (!item) continue;

                if (!item.selectable)
                    continue;

                if (!item.selectable.IsInteractable())
                    continue;

                if (item == this)
                    continue;
                if (!item.Active)
                    continue;
                if (item.transform.position.x <= transform.position.x) continue;

                if (targetRight == null)
                    targetRight = item;
                else
                {
                    float delta = Mathf.Abs(item.transform.position.x - transform.position.x);
                    float last_delta = Mathf.Abs(targetRight.transform.position.x - transform.position.x);

                    if (delta < last_delta)
                        targetRight = item;
                    else if (Mathf.Abs(delta - last_delta) < 0.001f)
                    {
                        float deltaY = Mathf.Abs(item.transform.position.y - transform.position.y);
                        float lastDeltaY = Mathf.Abs(targetRight.transform.position.y - transform.position.y);

                        if (deltaY < lastDeltaY)
                            targetRight = item;
                    }

                }
            }

            return targetRight;
        }

        private IEnumerator DelaySelect(Selectable obj)
        {
            if (obj == null || EventSystem.current == null)
                yield break;
            yield return null;
            
            obj.Select();
        }

        /// <summary>
        /// 选中
        /// </summary>
        public void Select()
        {
            if (!gameObject.activeInHierarchy) return;
            StartCoroutine(DelaySelect(selectable));
        }

        #endregion 
    }
}