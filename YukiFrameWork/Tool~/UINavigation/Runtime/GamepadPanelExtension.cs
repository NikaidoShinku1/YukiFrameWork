using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;
using YukiFrameWork.InputSystemExtension;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
#endif
namespace YukiFrameWork.UI
{
    /// <summary>
    /// 手柄控制拓展(如果想要通过手柄控制UI,挂上该组件即可!)
    /// <para>Tip:必须设置在拥有BasePanel组件的对象上!</para>
    /// </summary>
    // Extension    
    [DisallowMultipleComponent]
    public class GamepadPanelExtension : YMonoBehaviour
    {
        #region 字段   
        /// <summary>
        /// 是否监听返回按钮,如果监听,当按下 Esc(Keyboard)/Back(Gamepad) 时会关闭该界面(默认:false)
        /// </summary>
        [InfoBox("是否监听返回按钮,如果监听,当按下 Esc(Keyboard)/Back(Gamepad) 时会关闭该界面(默认:false)")]
        [SerializeField]
        private bool listenerBackButton;

        private float sliderTimer = 0;
         
        private SelectableNavigation currentSelectable;

        private bool isTop;

        private BasePanel panel;
        private BindablePropertyStruct<bool> panelActive = new BindablePropertyStruct<bool>((a, b) => a == b);
        #endregion 

        #region 属性

        /// <summary>
        /// BasePanel
        /// </summary>
        public BasePanel BasePanel => panel;

        /// <summary>
        /// 是否监听返回按钮,如果监听,当按下 Esc(Keyboard)/Back(Gamepad) 时会关闭该界面(默认:false)
        /// </summary>
        public bool ListenerBackButton => listenerBackButton;

        #endregion


        #region 生命周期
        
        protected override void Awake()
        {
            base.Awake();
            panel = GetComponent<BasePanel>();

            panelActive.Register(value =>
            {
                if (value)
                    PanelEnable();
                else PanelDisable();
            }).UnRegisterWaitGameObjectDestroy(this);
        }

        internal async void PanelEnable()
        {                 
            AddPanel(BasePanel);
            await CoroutineTool.WaitForFrame();
            SelectDefualtSelectable();
        }

        internal void PanelDisable()
        {
            Debug.Log(BasePanel);
            RemovePanel(BasePanel);
        }

        void Update()
        {
            panelActive.Value = BasePanel.IsActive && !BasePanel.IsPaused;
            if (!Application.isFocused)
                return;

            if (BasePanel.IsPaused || !BasePanel.IsActive) return;
             
            UpdateTop(IsTop(BasePanel));

            if (isTop == false)
                return;

            UpdateSelection();
            ListenerBack();
            ListenerEvent();

            if (SelectableNavigation.CurrentSelectedNavigation && SelectableNavigation.CurrentSelectedNavigation.Active)
                currentSelectable = SelectableNavigation.CurrentSelectedNavigation;
        }

        #endregion

        #region 方法

        /// <summary>
        /// 当界面开启时 选择一个默认的UI (或者当前界面没有选中UI，按下手柄方向键时调用该方法默认选择一个)
        /// </summary>
        protected void SelectDefualtSelectable()
        {
            if (!IsNeedUpdateSelection())
            {
                return;
            }

            SelectableNavigation targetSelect = null;

            // 找出默认选中的组件
            foreach (var navigation in SelectableNavigation.navigations)
            {
                if (!navigation) continue;

                if (!navigation.Selectable.IsInteractable())
                    continue;

                if (!navigation.Active)
                    continue;

                if (navigation.defaultSelected)
                {
                    targetSelect = navigation;
                    break;
                }
            }

            // 没有默认选中的
            if (!targetSelect)
            {
                foreach (var navigation in SelectableNavigation.navigations)
                {
                    if (!navigation) continue;

                    if (!navigation.Selectable.IsInteractable())
                        continue;

                    if (!navigation.Active)
                        continue;

                    if (targetSelect == null)
                    {
                        targetSelect = navigation;
                    }
                    else
                    {
                        // 找一个左上方的
                        if (navigation.transform.position.y - targetSelect.transform.position.y > 0.001f)
                        {
                            targetSelect = navigation;
                        }
                        else
                        {
                            // y值几乎相等
                            if (navigation.transform.position.x - targetSelect.transform.position.x < -0.001f)
                            {
                                targetSelect = navigation;
                            }
                        }

                    }

                }
            }


            if (targetSelect)
                targetSelect.Select();

            //UnityEngine.Debug.Log("SelectDefualtSelectable:" + targetSelect == null);
        }


        private void UpdateSelection()
        {
            if (IsNeedUpdateSelection())
            {
                // 监听方向按键(包括键盘和手柄)
                if (GetDirectionKeyDown())
                {
                    SelectDefualtSelectable();
                }
            }
        }

        private bool GetDirectionKeyDown()
        {

            foreach (var keyboard in InputKit.AllKeyboards)
            {
                if (GetDirectionKeyDown(keyboard))
                    return true;
            }

            foreach (var gamepad in InputKit.AllGamepads)
            {
                if (GetDirectionKeyDown(gamepad))
                    return true;
            }

            return false;
        }
         
        // 监听返回按键  
        private void ListenerBack()
        {
            if (GetBackKeyDown())
            {
                Back();
            }
        }

        private void Back()
        {
            if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null)
            {
                Dropdown dropdown = EventSystem.current.currentSelectedGameObject.GetComponentInParent<Dropdown>();
                if (dropdown != null)
                {

                    Transform child = transform.GetChild(transform.childCount - 1);
                    if (child.name == "Blocker")
                    {
                        PointerEventData eventData = new PointerEventData(EventSystem.current);
                        ExecuteEvents.Execute(child.gameObject, eventData, ExecuteEvents.pointerClickHandler);
                        return;
                    }
                }
            }

            if (listenerBackButton)
                panel.CloseSelf();
        }

        // 监听点击
        private void ListenerEvent()
        {
            //if (InputManager.InputType != InputDeviceType.Gamepad)
            //    return;
            if (EventSystem.current == null) return;
            if (EventSystem.current.currentSelectedGameObject == null) return;

#if !UNITY_6000_0_OR_NEWER

            // 经测试发现,UNITY_6000_0_OR_NEWER的版本,当按下手柄的确认按键 ，会自动处理按钮的点击
            // 所以这里就不用处理了
            foreach (var gamepad in InputKit.AllGamepads)
            {

                if (SelectableNavigation.CurrentSelectedNavigation != null)
                {
                    if (SelectableNavigation.CurrentSelectedNavigation.Selectable != null && SelectableNavigation.CurrentSelectedNavigation.Selectable is Slider)
                        break;
                }

                if (gamepad.buttonEast.wasPressedThisFrame)
                {
  
                    // 创建一个模拟的PointerEventData对象
                    PointerEventData eventData = new PointerEventData(EventSystem.current);
                    eventData.button = PointerEventData.InputButton.Left; // 指定按钮类型，例如左键
                    eventData.position = Input.mousePosition; // 设置鼠标位置

                    // 执行pointerDownHandler
                    ExecuteEvents.Execute(EventSystem.current.currentSelectedGameObject, eventData, ExecuteEvents.pointerDownHandler);
                    break;
                }
            }


            foreach (var gamepad in InputKit.AllGamepads)
            {

                if (SelectableNavigation.CurrentSelectedNavigation != null)
                {
                    if (SelectableNavigation.CurrentSelectedNavigation.Selectable != null && SelectableNavigation.CurrentSelectedNavigation.Selectable is Slider)
                        break;
                }


                if (gamepad.buttonEast.wasReleasedThisFrame)
                { 
                    // 创建一个模拟的PointerEventData对象
                    PointerEventData eventData = new PointerEventData(EventSystem.current);
                    eventData.button = PointerEventData.InputButton.Left; // 指定按钮类型，例如左键
                    eventData.position = Input.mousePosition; // 设置鼠标位置

                    // 执行pointerDownHandler
                    ExecuteEvents.Execute(EventSystem.current.currentSelectedGameObject, eventData, ExecuteEvents.pointerUpHandler);

                    //if (pointerClickHandler != null)
                    //    ExecuteEvents.pointerClickHandler(pointerClickHandler, eventData);

                    break;
                }
            }

#endif

            HandleScrollRect();

            HandleSliderEvent();
        }

        private void HandleScrollRect()
        {

            if (!SelectableNavigation.CurrentSelectedNavigation)
                return;

            if (!SelectableNavigation.CurrentSelectedNavigation.Active)
                return;

            if (!SelectableNavigation.CurrentSelectedNavigation.ChildOfScrollView)
                return;

            Vector2 stickValue = GetStickValue();

            if (stickValue != Vector2.zero)
            {
                if (SelectableNavigation.CurrentSelectedNavigation.ParentScrollRect == null)
                    return;

                PointerEventData pointer = new PointerEventData(EventSystem.current);
                pointer.scrollDelta = stickValue * 100 * Time.unscaledDeltaTime;

                ExecuteEvents.scrollHandler(SelectableNavigation.CurrentSelectedNavigation.ParentScrollRect, pointer);
            }

        }


        private void HandleSliderEvent()
        {

            //if (InputManager.InputType != InputDeviceType.Gamepad)
            //    return;

            if (SelectableNavigation.CurrentSelectedNavigation == null)
                return;

            Slider slider = SelectableNavigation.CurrentSelectedNavigation.Selectable as Slider;

            if (slider == null)
                return;

            foreach (var item in InputKit.AllGamepads)
            {
                if (item.dpad.right.wasPressedThisFrame)
                    sliderTimer = Mathf.InverseLerp(slider.minValue, slider.maxValue, slider.value) - 0.1f;

                if (item.dpad.right.isPressed)
                {
                    sliderTimer += Time.unscaledDeltaTime * 0.3f;
                    slider.value = Mathf.Lerp(slider.minValue, slider.maxValue, sliderTimer);

                    //UnityEngine.Debug.LogFormat("设置value:{0} sliderTimer:{1}", slider.value, sliderTimer);

                    break;
                }
            }

            foreach (var item in InputKit.AllGamepads)
            {
                if (item.dpad.left.wasPressedThisFrame)
                    sliderTimer = Mathf.InverseLerp(slider.minValue, slider.maxValue, slider.value) + 0.1f;

                if (item.dpad.left.isPressed)
                {
                    sliderTimer -= Time.unscaledDeltaTime * 0.3f;
                    slider.value = Mathf.Lerp(slider.minValue, slider.maxValue, sliderTimer);

                    //UnityEngine.Debug.LogFormat("设置value:{0} sliderTimer:{1}", slider.value, sliderTimer);
                    break;
                }
            }

        }

        private bool IsNeedUpdateSelection()
        {
            if (!EventSystem.current)
                return false;

            if (!EventSystem.current.currentSelectedGameObject)
            {
                return true;
            }

            if (!EventSystem.current.currentSelectedGameObject.gameObject.activeInHierarchy)
            {
                return true;
            }

            if (!SelectableNavigation.CurrentSelectedNavigation || !SelectableNavigation.CurrentSelectedNavigation.Active)
                return true;

            return false;
        }


        private bool GetDirectionKeyDown(Keyboard keyboard)
        {

            if (keyboard.upArrowKey.wasPressedThisFrame || keyboard.downArrowKey.wasPressedThisFrame || keyboard.leftArrowKey.wasPressedThisFrame || keyboard.rightArrowKey.wasPressedThisFrame)
                return true;

            return false;
        }

        private bool GetDirectionKeyDown(Gamepad gamepad)
        {
            if (gamepad.leftStick.up.wasPressedThisFrame || gamepad.leftStick.down.wasPressedThisFrame || gamepad.leftStick.left.wasPressedThisFrame || gamepad.leftStick.right.wasPressedThisFrame)
                return true;

            if (gamepad.rightStick.up.wasPressedThisFrame || gamepad.rightStick.down.wasPressedThisFrame || gamepad.rightStick.left.wasPressedThisFrame || gamepad.rightStick.right.wasPressedThisFrame)
                return true;

            if (gamepad.dpad.up.wasPressedThisFrame || gamepad.dpad.down.wasPressedThisFrame || gamepad.dpad.left.wasPressedThisFrame || gamepad.dpad.right.wasPressedThisFrame)
                return true;

            return false;
        }

        private Vector2 GetStickValue()
        {

            Vector2 value = InputKit.GetStickValue(GamepadButton.LeftStick);

            if (value != Vector2.zero)
                return value;

            value = InputKit.GetStickValue(GamepadButton.RightStick);

            if (value != Vector2.zero)
                return value;

            return Vector2.zero;
        }


        private bool GetBackKeyDown()
        {
            if (InputKit.GetKeyDown(Key.Escape))
                return true;

            if (InputKit.GetKeyDown(GamepadButton.South))
                return true;

            return false;
        }

        private void UpdateTop(bool top)
        {
            if (top == isTop) return;
            isTop = top;

            if (top)
            {
                if (currentSelectable)
                {

                    if (!SelectableNavigation.CurrentSelectedNavigation || !SelectableNavigation.CurrentSelectedNavigation.Active)
                    {
                        SelectableNavigation.CurrentSelectedNavigation = currentSelectable;
                        currentSelectable.Select();
                    }
                }
            }

        }
         
        #endregion


        #region Top
         
        private static List<BasePanel> panels = new List<BasePanel>();


        private static void AddPanel(BasePanel panel) { 
            panels.Add(panel);
        }

        private static void RemovePanel(BasePanel panel) { 
            panels.Remove(panel);
        }


        internal static bool IsTop(BasePanel panel)
        {
            if (panel.IsPaused || !panel.IsActive)
                return false;

            foreach (var item in panels)
            {
                if (item.Level >= panel.Level && item != panel) return false;
            }                        

            return true; 
        }


        #endregion

    }
#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(GamepadPanelExtension))]
    public class GamepadPanelExtensionEditor : OdinEditor
    {
        public override void OnInspectorGUI()
        {
            GamepadPanelExtension gamepadPanelExtension = target as GamepadPanelExtension;
            if (!gamepadPanelExtension.GetComponent<BasePanel>())
            {
                UnityEditor.EditorGUILayout.HelpBox("拓展没有挂载BasePanel组件，无法使用！",UnityEditor.MessageType.Error);
                return;
            }
            base.OnInspectorGUI();
        }
    }
#endif
}