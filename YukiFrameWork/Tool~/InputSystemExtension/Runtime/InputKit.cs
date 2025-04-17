using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
using XFABManager;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
#endif
namespace YukiFrameWork.InputSystemExtension
{ 
    /// <summary>
    /// 当前输入设备的类型
    /// </summary>
    public enum InputDeviceType
    {
        [Tooltip("空")]
        None,
        [Tooltip("手柄")]
        Gamepad, 
        [Tooltip("键盘")]
        Keyboard,
        [Tooltip("鼠标")]
        Mouse,
        [Tooltip("触摸")]
        Pointer
    }
  
    /// <summary>
    /// 输入业务逻辑
    /// </summary>
    public class InputKit
    {

        #region 静态字段

        [Tooltip("所有的手柄")]
        private static List<Gamepad> gamepads = null;
        [Tooltip("所有的键盘")]
        private static List<Keyboard> keyboards = null;
        [Tooltip("所有的鼠标")]
        private static List<Mouse> mouses = null; 
        [Tooltip("所有的触摸点")]
        private static List<Pointer> pointers = null;
        #endregion

        #region 静态属性

        /// <summary>
        /// 所有的手柄
        /// </summary>
        public static List<Gamepad> AllGamepads 
        {
            get {

                if (gamepads == null)
                    UpdateInputDevice();

                return gamepads;
            }
        }

        /// <summary>
        /// 所有的键盘
        /// </summary>
        public static List<Keyboard> AllKeyboards 
        { 
            get 
            {
                if(keyboards == null)
                    UpdateInputDevice();
             
                return keyboards;
            }
        }

        /// <summary>
        /// 所有鼠标
        /// </summary>
        public static List<Mouse> AllMouses
        {
            get
            {
                if (mouses == null)
                    UpdateInputDevice(); 

                return mouses;
            }
        }

        /// <summary>
        /// 所有Pointer
        /// </summary>
        public static List<Pointer> AllPointers
        {
            get
            {
                if (pointers == null)
                    UpdateInputDevice();
            
                return pointers;
            }
        }
      
        /// <summary>
        /// 当前鼠标位置
        /// </summary>
        public static Vector2 MousePosition 
        {
            get 
            {
                if (Mouse.current != null)
                    return Mouse.current.position.ReadValue();

                return Vector2.zero;
            }
        }

        #endregion


        #region 静态方法

        [RuntimeInitializeOnLoadMethod]
        private static void InitInputManager()
        { 
            gamepads = null;
            keyboards = null;
            mouses = null;
            pointers = null;
            
            InputSystem.onDeviceChange -= OnDeviceChange;
            InputSystem.onDeviceChange += OnDeviceChange;  
        }

        private static void OnDeviceChange(InputDevice device, InputDeviceChange state)
        {              
            UpdateInputDevice();
        }

        private static void UpdateInputDevice() 
        {
            if (gamepads == null)
                gamepads = new List<Gamepad>(); 
            gamepads.Clear();

            if (keyboards == null)
                keyboards = new List<Keyboard>(); 
            keyboards.Clear();

            if (mouses == null)
                mouses = new List<Mouse>(); 
            mouses.Clear();

            if (pointers == null)
                pointers = new List<Pointer>(); 
            pointers.Clear();

            foreach (var item in InputSystem.devices)
            { 
                if (item is Gamepad)
                    gamepads.Add(item as Gamepad); 
                else if (item is Keyboard)
                    keyboards.Add(item as Keyboard); 
                else if (item is Mouse)
                    mouses.Add(item as Mouse); 
                else if (item is Pointer)
                    pointers.Add(item as Pointer);  
            }
        }

        /// <summary>
        /// 是否有任意按键按下
        /// </summary>
        /// <returns></returns>
        public static bool AnyKeyDown()
        {
            ButtonControl control;
            InputDeviceType inputType;
            return AnyKeyDown(out control,out inputType);
        }

        /// <summary>
        /// 是否有任意按键按下
        /// </summary>
        /// <param name="buttonControl">按下的按钮</param>
        /// <param name="inputType">输入设备类型</param>
        /// <returns></returns>
        public static bool AnyKeyDown(out ButtonControl buttonControl,out InputDeviceType inputType)
        {

            foreach (var device in InputSystem.devices)
            {
                Gamepad gamepad = device as Gamepad;
             
                if (gamepad != null)
                {
                    foreach (var item in gamepad.allControls)
                    {
                        buttonControl = item as ButtonControl;
                        if (buttonControl == null) continue;

                        if (buttonControl.wasPressedThisFrame)
                        {
                            inputType = InputDeviceType.Gamepad;
                            return true;
                        }
                    }
                }

                Keyboard keyboard = device as Keyboard;
                if (keyboard != null)
                { 
                    foreach (var item in keyboard.allKeys)
                    {  
                        if (item.wasPressedThisFrame)
                        {
                            buttonControl = item;
                            inputType = InputDeviceType.Keyboard;
                            return true;
                        }
                    } 
                }

                Mouse mouse = device as Mouse;
                if (mouse != null)
                {
                    foreach (var item in mouse.allControls)
                    {
                        buttonControl = item as ButtonControl;
                        if (buttonControl == null) continue;
                        if (buttonControl.wasPressedThisFrame)
                        {
                            inputType = InputDeviceType.Mouse;
                            return true;
                        }
                    }
                }
            }

            buttonControl = null;
            inputType = InputDeviceType.None;
            return false;
        }

        /// <summary>
        /// 是否有任意按键抬起
        /// </summary>
        /// <returns></returns>
        public static bool AnyKeyUp()
        {
            ButtonControl control;
            InputDeviceType inputType;
            return AnyKeyUp(out control, out inputType);
        }

        /// <summary>
        /// 是否有任意按键抬起
        /// </summary>
        /// <param name="buttonControl">抬起的按钮</param>
        /// <param name="inputType">输入设备类型</param>
        /// <returns></returns>
        public static bool AnyKeyUp(out ButtonControl buttonControl, out InputDeviceType inputType)
        {

            foreach (var device in InputSystem.devices)
            {
                Gamepad gamepad = device as Gamepad;

                if (gamepad != null)
                {
                    foreach (var item in gamepad.allControls)
                    {
                        buttonControl = item as ButtonControl;
                        if (buttonControl == null) continue;

                        if (buttonControl.wasReleasedThisFrame)
                        {
                            inputType = InputDeviceType.Gamepad;
                            return true;
                        }
                    }
                }

                Keyboard keyboard = device as Keyboard;
                if (keyboard != null)
                {
                    foreach (var item in keyboard.allKeys)
                    {
                        if (item.wasReleasedThisFrame)
                        {
                            buttonControl = item;
                            inputType = InputDeviceType.Keyboard;
                            return true;
                        }
                    }
                }

                Mouse mouse = device as Mouse;
                if (mouse != null)
                {
                    foreach (var item in mouse.allControls)
                    {
                        buttonControl = item as ButtonControl;
                        if (buttonControl == null) continue;
                        if (buttonControl.wasReleasedThisFrame)
                        {
                            inputType = InputDeviceType.Mouse;
                            return true;
                        }
                    }
                }
            }

            buttonControl = null;
            inputType = InputDeviceType.None;
            return false;
        }

        /// <summary>
        /// 是否按住某个键盘按键
        /// </summary>
        /// <param name="key">按键</param>
        /// <returns></returns>
        public static bool GetKey(Key key) 
        {
            foreach (var item in AllKeyboards)
            {
                 if(GetKey(key,item))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 是否按下某个键盘按键
        /// </summary>
        /// <param name="key">按键</param>
        /// <returns></returns>
        public static bool GetKeyDown(Key key)
        {
            foreach (var item in AllKeyboards)
            {
                if (GetKeyDown(key,item))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 是否抬起某个键盘按键
        /// </summary>
        /// <param name="key">按键</param>
        /// <returns></returns>
        public static bool GetKeyUp(Key key) 
        {
            foreach (var item in AllKeyboards)
            {
                if (GetKeyUp(key,item))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 某个键盘是否按住某个按键
        /// </summary>
        /// <param name="key">按键</param>
        /// <param name="keyboard">键盘设备</param>
        /// <returns></returns>
        public static bool GetKey(Key key,Keyboard keyboard) 
        {
            UnityEngine.InputSystem.Controls.KeyControl control = GetInputControl(key, keyboard);
            if (control == null) return false;
            if (control.isPressed)
                return true; 
            return false;
        }

        /// <summary>
        /// 某个键盘是否按下某个按键
        /// </summary>
        /// <param name="key">按键</param>
        /// <param name="keyboard">键盘设备</param>
        /// <returns></returns>
        public static bool GetKeyDown(Key key, Keyboard keyboard) 
        {
            UnityEngine.InputSystem.Controls.KeyControl control = GetInputControl(key, keyboard);
            if (control == null) return false;
            if (control.wasPressedThisFrame)
                return true;
            return false;
        }

        /// <summary>
        /// 某个键盘是否抬起某个按键
        /// </summary>
        /// <param name="key">按键</param>
        /// <param name="keyboard">键盘设备</param>
        /// <returns></returns>
        public static bool GetKeyUp(Key key, Keyboard keyboard) 
        {
            UnityEngine.InputSystem.Controls.KeyControl control = GetInputControl(key, keyboard);
            if (control == null) return false;
            if (control.wasReleasedThisFrame)
                return true;
            return false;
        }
     
        /// <summary>
        /// 是否按住某个手柄按键
        /// </summary>
        /// <param name="button">手柄按键</param>
        /// <returns></returns>
        public static bool GetKey(GamepadButton button)
        {
            foreach (var gamepad in AllGamepads)
            {
                ButtonControl control = GetInputControl(button, gamepad) as ButtonControl;
                if (control == null) continue;
                if (control.isPressed)
                    return true;
            }
         
            return false;
        }

        /// <summary>
        /// 是否按下某个手柄按键
        /// </summary>
        /// <param name="button">手柄按键</param>
        /// <returns></returns>
        public static bool GetKeyDown(GamepadButton button )
        {
            foreach (var gamepad in AllGamepads) 
            {
                ButtonControl control = GetInputControl(button, gamepad) as ButtonControl;
                if (control == null) continue;
                if (control.wasPressedThisFrame)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 是否抬起某个手柄按键
        /// </summary>
        /// <param name="button">手柄按键</param>
        /// <returns></returns>
        public static bool GetKeyUp(GamepadButton button )
        {
            foreach (var gamepad in AllGamepads) 
            {
                ButtonControl control = GetInputControl(button, gamepad) as ButtonControl;
                if (control == null) continue;
                if (control.wasReleasedThisFrame)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 某个手柄是否按住某个按键
        /// </summary>
        /// <param name="button">手柄按键</param>
        /// <param name="gamepad">手柄设备</param>
        /// <returns></returns>
        public static bool GetKey(GamepadButton button, Gamepad gamepad) 
        {
            ButtonControl control = GetInputControl(button, gamepad) as ButtonControl;
            if (control == null) return false;
            if (control.isPressed) 
                return true;

            return false;
        }

        /// <summary>
        /// 某个手柄是否按下某个按键
        /// </summary>
        /// <param name="button">手柄按键</param>
        /// <param name="gamepad">手柄设备</param>
        /// <returns></returns>
        public static bool GetKeyDown(GamepadButton button, Gamepad gamepad)
        {
            ButtonControl control = GetInputControl(button, gamepad) as ButtonControl;
            if (control == null) return false;
            if (control.wasPressedThisFrame)
                return true;

            return false;
        }

        /// <summary>
        /// 某个手柄是否抬起某个按键
        /// </summary>
        /// <param name="button">手柄按键</param>
        /// <param name="gamepad">手柄设备</param>
        /// <returns></returns>
        public static bool GetKeyUp(GamepadButton button, Gamepad gamepad)
        {
            ButtonControl control = GetInputControl(button, gamepad) as ButtonControl;
            if (control == null) return false;
            if (control.wasReleasedThisFrame)
                return true;

            return false;
        }

        /// <summary>
        /// 某个鼠标是否按住某个按键
        /// </summary>
        /// <param name="button">鼠标按键</param>
        /// <param name="mouse">鼠标设备</param>
        /// <returns></returns>
        public static bool GetKey(MouseButton button, Mouse mouse)
        {
            ButtonControl control = GetInputControl(button, mouse) as ButtonControl;
            if (control == null) return false;
            if (control.isPressed)
                return true;

            return false;
        }

        /// <summary>
        /// 某个鼠标是否按下某个按键
        /// </summary>
        /// <param name="button">鼠标按键</param>
        /// <param name="mouse">鼠标设备</param>
        /// <returns></returns>
        public static bool GetKeyDown(MouseButton button, Mouse mouse)
        {
            ButtonControl control = GetInputControl(button, mouse) as ButtonControl;
            if (control == null) return false;
            if (control.wasPressedThisFrame)
                return true;

            return false;
        }

        /// <summary>
        /// 某个鼠标是否抬起某个按键
        /// </summary>
        /// <param name="button">鼠标按键</param>
        /// <param name="mouse">鼠标设备</param>
        /// <returns></returns>
        public static bool GetKeyUp(MouseButton button, Mouse mouse)
        {
            ButtonControl control = GetInputControl(button, mouse) as ButtonControl;
            if (control == null) return false;
            if (control.wasReleasedThisFrame)
                return true;

            return false;
        }

        /// <summary>
        /// 是否按住某个鼠标按键
        /// </summary>
        /// <param name="button">鼠标按键</param>
        /// <returns></returns>
        public static bool GetKey(MouseButton button ) 
        {
            foreach (var mouse in AllMouses)
            {
                ButtonControl control = GetInputControl(button, mouse) as ButtonControl;
                if(control == null) continue;
                if (control.isPressed)
                    return true;
            }
         
            return false;
        }

        /// <summary>
        /// 是否按下某个鼠标按键
        /// </summary>
        /// <param name="button">鼠标按键</param>
        /// <returns></returns>
        public static bool GetKeyDown(MouseButton button )
        {
            foreach (var mouse in AllMouses) 
            {
                ButtonControl control = GetInputControl(button, mouse) as ButtonControl;
                if (control == null) continue;
                if (control.wasPressedThisFrame)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 是否抬起某个鼠标按键
        /// </summary>
        /// <param name="button">鼠标按键</param>
        /// <returns></returns>
        public static bool GetKeyUp(MouseButton button )
        {
            foreach (var mouse in AllMouses) 
            {
                ButtonControl control = GetInputControl(button, mouse) as ButtonControl;
                if (control == null) continue;
                if (control.wasReleasedThisFrame)
                    return true;
            }
         
            return false;
        }
     

        /// <summary>
        /// 获取某个摇杆的值
        /// </summary>
        /// <param name="stick">摇杆</param>
        /// <returns></returns>
        public static Vector2 GetStickValue(GamepadButton stick) {
            foreach (var item in AllGamepads)
            {
                Vector2 v = GetStickValue(stick, item);
                if(v != Vector2.zero) 
                    return v;
            }

            return Vector2.zero;
        }

        /// <summary>
        /// 获取某个手柄的某个摇杆的值
        /// </summary>
        /// <param name="stick">摇杆</param>
        /// <param name="gamepad">手柄设备</param>
        /// <returns></returns>
        public static Vector2 GetStickValue(GamepadButton stick,Gamepad gamepad) 
        {
            if(stick != GamepadButton.RightStick && stick != GamepadButton.LeftStick)
                return Vector2.zero;

            Vector2 value = Vector2.zero;

            switch (stick)
            { 
                case GamepadButton.LeftStick:
                    value = gamepad.leftStick.ReadValue(); 
                    break;
                
                case GamepadButton.RightStick:
                    value = gamepad.rightStick.ReadValue();
                    break;
            }

            if (Mathf.Abs(value.x) <= 0.02f)
                value.x = 0;

            if(Mathf.Abs(value.y) <= 0.02f)
                value.y = 0;

            return value.normalized;
        }       
        /// <summary>
        /// 获取键盘的某个KeyControl
        /// </summary>
        /// <param name="key">按键</param>
        /// <returns></returns>
        public static UnityEngine.InputSystem.Controls.KeyControl GetInputControl(Key key) 
        {           
            return GetInputControl(key, Keyboard.current);
        }

        /// <summary>
        /// 获取某个键盘的某个KeyControl
        /// </summary>
        /// <param name="key">按键</param>
        /// <param name="keyboard">键盘设备</param>
        /// <returns></returns>
        public static UnityEngine.InputSystem.Controls.KeyControl GetInputControl(Key key, Keyboard keyboard)
        {
            if (keyboard == null) return null;
            return keyboard[key];
        }

        /// <summary>
        /// 获取手柄InputControl
        /// </summary>
        /// <param name="button">手柄按键</param>
        /// <returns></returns>
        public static InputControl GetInputControl(GamepadButton button) 
        {          
            return GetInputControl(button,Gamepad.current);
        }

        /// <summary>
        /// 获取某个手柄的某个InputControl
        /// </summary>
        /// <param name="button">手柄按键</param>
        /// <param name="gamepad">手柄设备</param>
        /// <returns></returns>
        public static InputControl GetInputControl(GamepadButton button, Gamepad gamepad)
        {
            if (gamepad == null) return null;  
            return gamepad[button];
        }

        /// <summary>
        /// 获取鼠标的InputControl
        /// </summary>
        /// <param name="button">鼠标按钮</param>
        /// <returns></returns>
        public static InputControl GetInputControl(MouseButton button) 
        {
            return GetInputControl(button, Mouse.current);
        }

        /// <summary>
        /// 获取某个鼠标的某个InputControl
        /// </summary>
        /// <param name="button">鼠标按钮</param>
        /// <param name="mouse">鼠标设备</param>
        /// <returns></returns>
        public static InputControl GetInputControl(MouseButton button, Mouse mouse) 
        {
            switch (button)
            {
                case MouseButton.LeftButton:
                    return mouse.leftButton; 
                case MouseButton.MiddleButton:
                    return mouse.middleButton;
                case MouseButton.RightButton:
                    return mouse.rightButton;
                case MouseButton.ForwardButton:
                    return mouse.forwardButton;
                case MouseButton.BackButton:
                    return mouse.backButton;
                case MouseButton.Scroll:
                    return mouse.scroll;
                case MouseButton.ClickCount:
                    return mouse.clickCount;
            }

            return null;
        }
       
        /// <summary>
        /// 让某个手柄震动
        /// </summary>
        /// <param name="gamepad">手柄设备</param>
        /// <param name="low">低频频率</param>
        /// <param name="hight">高频频率</param>
        /// <param name="time">震动时间</param>
        public static async void GamepadVibrate(Gamepad gamepad,float low,float hight,float time)
        {
            if (gamepad == null || !gamepad.enabled) return;
            // 设置震动速度
            gamepad.SetMotorSpeeds(low, hight);
            gamepad.ResumeHaptics();
            await CoroutineTool.WaitForSeconds(time);
            gamepad.SetMotorSpeeds(0, 0);
            gamepad.PauseHaptics();
        }

        /// <summary>
        /// 让手柄震动
        /// </summary>
        /// <param name="low">低频频率</param>
        /// <param name="hight">高频频率</param>
        /// <param name="time">震动时间</param>
        public static void GamepadVibrate(float low, float hight, float time)
        {
            GamepadVibrate(Gamepad.current,low, hight, time);
        }

        /// <summary>
        /// 停止手柄震动
        /// </summary>
        public static void GamepadStopVibrate() {
            GamepadStopVibrate(Gamepad.current);
        }

        /// <summary>
        /// 停止某个手柄震动
        /// </summary>
        /// <param name="gamepad">手柄设备</param>
        public static void GamepadStopVibrate(Gamepad gamepad) 
        {
            if (gamepad == null) return;
            gamepad.SetMotorSpeeds(0,0);
            gamepad.PauseHaptics();
        }
         
        #endregion
    }
}

