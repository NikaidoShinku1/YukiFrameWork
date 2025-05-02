
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel; 
 
namespace YukiFrameWork.InputSystemExtension
{ 
    /// <summary>
    /// 按键控制的业务逻辑
    /// </summary>
    public class InputKeyControl  
    {

        #region 字段

        [LabelText("名称(标识)")]
        private string name;       
        [LabelText("key(对应的键盘按键)")]
        private Key key;
        [LabelText("mouseButton(对应的鼠标按键)")]
        private MouseButton mouseButton = MouseButton.None;
        [LabelText("gamepadButton(对应的手柄按键)")]
        private GamepadButton gamepadButton = GamepadButton.A;
        [LabelText("是否启用手柄")]
        private bool isEnableGamepad = false;
        [LabelText("键盘输入设备")]
        private Keyboard keyboard;
        [LabelText("鼠标输入设备")]
        private Mouse mouse;
        [LabelText("手柄输入设备")]
        private Gamepad gamepad;

        private string PrefsKey_Keyboard = null; 
        private string PrefsKey_Mouse = null; 
        private string PrefsKey_Gamepad = null;


        private Key defaultKey;
        private MouseButton defaultMouseButton;
        private GamepadButton defaultGamepadButton;

        #endregion

        #region 属性       
        /// <summary>
        /// 名称
        /// <para>Tip:名称是不允许重复的</para>
        /// </summary>
        public string Name
        {
            get
            { 
                return name;
            }
            private set 
            {
                name = value;
                Init();
            }
        }
      
        /// <summary>
        /// 键盘按键值
        /// </summary>
        public Key Key
        {
            get 
            {  
                return this.key;
            }
            set 
            {
                if (this.key == value) return;

                this.key = value;

            

                if (this.key != Key.None)
                    MouseButton = MouseButton.None;

                // 持久化保存
                PlayerPrefs.SetInt(PrefsKey_Keyboard, (int)this.key);
                PlayerPrefs.Save();
            }
        }
     
        /// <summary>
        /// 鼠标按键值
        /// </summary>
        public MouseButton MouseButton
        {
            get {
                return mouseButton;
            }
            set 
            {                
                if (mouseButton == value) return;

                mouseButton = value;

                //Debug.LogFormat("mouseButton:{0}",mouseButton);

                if (mouseButton != MouseButton.None)
                    Key = Key.None;

                // 持久化保存
                PlayerPrefs.SetInt(PrefsKey_Mouse, (int)mouseButton);
                PlayerPrefs.Save();
            }
        }

        /// <summary>
        /// 手柄按键值
        /// </summary>
        public GamepadButton GamepadButton
        {
            get {
                return gamepadButton;
            }
            set {
                if (gamepadButton == value) return;

                gamepadButton = value;
                isEnableGamepad = true;

                // 持久化保存
                PlayerPrefs.SetInt(PrefsKey_Gamepad, (int)gamepadButton);
                PlayerPrefs.Save();
            }
        } 
     
        /// <summary>
        /// 是否启用手柄(只读) 
        /// </summary>
        public bool EnableGamepad => isEnableGamepad;

        #endregion


        #region 方法

        /// <summary>
        /// 默认使用键盘按键的输入控制
        /// </summary>
        /// <param name="name">唯一标识</param>
        /// <param name="defaultKey">默认键盘按键</param>
        public InputKeyControl(string name, Key defaultKey)
        {
            this.defaultKey = defaultKey;
            this.defaultMouseButton = MouseButton.None;
        

            this.key = defaultKey;
            this.mouseButton = MouseButton.None;
            isEnableGamepad = false;
            this.Name = name; 
        }

        /// <summary>
        /// 默认使用鼠标按键的输入控制
        /// </summary>
        /// <param name="name">唯一标识</param>
        /// <param name="defaultMouseButton">默认鼠标按键</param>
        public InputKeyControl(string name, MouseButton defaultMouseButton)
        {
            this.defaultKey = Key.None;
            this.defaultMouseButton = defaultMouseButton;

            this.mouseButton = defaultMouseButton; 
            this.key = Key.None; 
            isEnableGamepad = false;
            this.Name = name;
        }

        /// <summary>
        /// 默认使用手柄输入控制
        /// </summary>
        /// <param name="name">唯一标识</param>
        /// <param name="defaultMouseButton">默认鼠标按键</param>
        /// <param name="defaultGamepadButton">默认手柄按键</param>
        public InputKeyControl(string name, GamepadButton defaultGamepadButton)
        {
            this.defaultKey = Key.None;
            this.defaultGamepadButton = defaultGamepadButton;
            this.gamepadButton = defaultGamepadButton;
            this.mouseButton = MouseButton.None;
            this.key = Key.None;
            this.Name = name;

            isEnableGamepad = true;
        }


        /// <summary>
        /// 默认同时使用键盘和手柄输入控制
        /// </summary>
        /// <param name="name">唯一标识</param>
        /// <param name="defaultKey">默认键盘按键</param>
        /// <param name="defaultGamepadButton">默认手柄按键</param>
        public InputKeyControl(string name, Key defaultKey, GamepadButton defaultGamepadButton) 
        {
            this.defaultKey = defaultKey;
            this.defaultMouseButton = MouseButton.None;
            this.defaultGamepadButton = defaultGamepadButton;

            this.gamepadButton = defaultGamepadButton;
            this.key = defaultKey;
            this.mouseButton = MouseButton.None;
            this.Name = name;

            isEnableGamepad = true; 
        }

        /// <summary>
        /// 默认同时使用鼠标和手柄输入控制
        /// </summary>
        /// <param name="name">唯一标识</param>
        /// <param name="defaultMouseButton">默认鼠标按键</param>
        /// <param name="defaultGamepadButton">默认手柄按键</param>
        public InputKeyControl(string name, MouseButton defaultMouseButton, GamepadButton defaultGamepadButton)
        {
            this.defaultKey = Key.None;
            this.defaultMouseButton = defaultMouseButton;
            this.defaultGamepadButton = defaultGamepadButton;


            this.gamepadButton = defaultGamepadButton; 
            this.mouseButton = defaultMouseButton;
            this.key = Key.None; 
            this.Name = name;
        
            isEnableGamepad = true;
        }       

        /// <summary>
        /// 设置当前使用的键盘设备
        /// </summary>
        /// <param name="keyboard">键盘设备</param>
        public void SetInputDevice(Keyboard keyboard)
        {
            this.keyboard = keyboard;
        }

        /// <summary>
        /// 设置当前使用的鼠标设备
        /// </summary>
        /// <param name="mouse">鼠标设备</param>
        public void SetInputDevice(Mouse mouse) 
        {
            this.mouse = mouse;
        }

        /// <summary>
        /// 设置当前使用的手柄设备
        /// </summary>
        /// <param name="gamepad">手柄设备</param>
        public void SetInputDevice(Gamepad gamepad) 
        {
            this.gamepad = gamepad;
        }
         
        /// <summary>
        /// 获取当前使用的按键的显示名称
        /// </summary>
        /// <param name="inputDeviceType">输入设备类型</param>
        /// <returns></returns>
        public string GetDisplayName(InputDeviceType inputDeviceType)
        {    
            if (inputDeviceType == InputDeviceType.Gamepad)
            {
                // 判断是否启用手柄
                if (isEnableGamepad) 
                { 
                    bool switch_pro = false;

                    switch_pro = gamepad != null && gamepad.path.Contains("Switch");
                 
                    if(!switch_pro)
                        switch_pro = Gamepad.current != null && Gamepad.current.path.Contains("Switch");

                    if (switch_pro) 
                    {
                        if (GamepadButton == GamepadButton.South)
                            return "B";

                        if (GamepadButton == GamepadButton.East)
                            return "A"; 
                    }

                    return GamepadButton.ToString();
                }
                else 
                {
                    // 如果没有启用手柄 返回键盘或鼠标的按键
                    if (Key != Key.None)
                        return Key.ToString();

                    if (MouseButton != MouseButton.None)
                        return MouseButton.ToString();

                }
            }
            else if (inputDeviceType == InputDeviceType.Keyboard)
            {
                // 如果是键盘 优先返回键盘
                if (Key != Key.None)
                    return Key.ToString();

                if (MouseButton != MouseButton.None)
                    return MouseButton.ToString();

                if(isEnableGamepad)
                    return GamepadButton.ToString();

            }
            else if (inputDeviceType == InputDeviceType.Mouse) 
            {
                // 如果是鼠标 优先返回鼠标
                if (MouseButton != MouseButton.None)
                    return MouseButton.ToString();

                if (Key != Key.None)
                    return Key.ToString(); 

                if (isEnableGamepad)
                    return GamepadButton.ToString();
            }

            return string.Empty;
        }

        /// <summary>
        /// 获取手柄设备对象
        /// </summary>
        /// <returns></returns>
        public Gamepad GetGamepad() 
        {
            return  gamepad != null ? gamepad : Gamepad.current;
        }

        /// <summary>
        /// 获取键盘设备对象
        /// </summary>
        /// <returns></returns>
        public Keyboard GetKeyboard()
        {
            return keyboard != null ? keyboard : Keyboard.current;
        }

        /// <summary>
        /// 获取鼠标设备对象
        /// </summary>
        /// <returns></returns>
        public Mouse GetMouse()
        {
            return mouse != null ? mouse : Mouse.current;
        }

        /// <summary>
        /// 判断当前按键是否按下
        /// </summary>
        /// <returns></returns>
        public bool GetKeyDown()
        {
            if (this.Key != Key.None) 
            {
                Keyboard k = GetKeyboard();
             
                if (k != null && InputKit.GetKeyDown(this.Key, k))
                    return true;
            }

            if (this.MouseButton != MouseButton.None) 
            {
                Mouse m = GetMouse();
                if(m!= null && InputKit.GetKeyDown(this.MouseButton,m))
                    return true;
            }

            if (this.isEnableGamepad) 
            { 
                Gamepad g = GetGamepad();
                if (g != null && InputKit.GetKeyDown(this.GamepadButton, g))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 判断当前按键是否按住
        /// </summary>
        /// <returns></returns>
        public bool GetKey() 
        {
            if (this.Key != Key.None)
            {
                Keyboard k = GetKeyboard();

                if (k != null && InputKit.GetKey(this.Key, k))
                    return true;
            }

            if (this.MouseButton != MouseButton.None)
            {
                Mouse m = GetMouse();
                if (m != null && InputKit.GetKey(this.MouseButton, m))
                    return true;
            }

            if (this.isEnableGamepad)
            {
                Gamepad g = GetGamepad();
                if (g != null && InputKit.GetKey(this.GamepadButton, g))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 判断当前按键是否抬起
        /// </summary>
        /// <returns></returns>
        public bool GetKeyUp()
        {
            if (this.Key != Key.None)
            {
                Keyboard k = GetKeyboard();

                if (k != null && InputKit.GetKeyUp(this.Key, k))
                    return true;
            }

            if (this.MouseButton != MouseButton.None)
            {
                Mouse m = GetMouse();
                if (m != null && InputKit.GetKeyUp(this.MouseButton, m))
                    return true;
            }

            if (this.isEnableGamepad)
            {
                Gamepad g = GetGamepad();
                if (g != null && InputKit.GetKeyUp(this.GamepadButton, g))
                    return true;
            }

            return false;
        }

    
        /// <summary>
        /// 初始化
        /// </summary>
        private void Init() 
        { 
            PrefsKey_Keyboard = string.Format("{0}:{1}_Keyboard", GetType().FullName, Name);
            PrefsKey_Mouse = string.Format("{0}:{1}_Mouse", GetType().FullName, Name);
            PrefsKey_Gamepad = string.Format("{0}:{1}_Gamepad", GetType().FullName, Name);

            // 初始化

            int k = PlayerPrefs.GetInt(PrefsKey_Keyboard, -1);
            if (k != -1)
                Key = (Key)k;


            int m = PlayerPrefs.GetInt(PrefsKey_Mouse, -1);
            if (m != -1)
                MouseButton = (MouseButton)m;

            int g = PlayerPrefs.GetInt(PrefsKey_Gamepad, -1);
            if (g != -1)
                GamepadButton = (GamepadButton)g;
        }


        /// <summary>
        /// 重置(如果有按键修改则还原为默认值)
        /// </summary>
        public void Reset() 
        {
            if(defaultKey != Key.None)
                Key = defaultKey;

            if (defaultMouseButton != MouseButton.None)
                MouseButton = defaultMouseButton;

            if (isEnableGamepad)
                GamepadButton = defaultGamepadButton; 
        }     
        /// <summary>
        /// 是否能够通过某种类型的设备控制
        /// </summary>
        /// <param name="inputDeviceType">设备类型</param>
        /// <returns></returns>
        public bool IsCanControl(InputDeviceType inputDeviceType)
        {
            switch (inputDeviceType)
            { 
                case InputDeviceType.Gamepad: 
                    return isEnableGamepad;
                case InputDeviceType.Keyboard:
                    return Key != Key.None;
                case InputDeviceType.Mouse:
                    return MouseButton != MouseButton.None;
            }
            
            return false;
        }

        #endregion

    }
}
