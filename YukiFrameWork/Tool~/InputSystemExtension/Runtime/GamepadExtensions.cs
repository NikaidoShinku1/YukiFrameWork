using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;

namespace YukiFrameWork.InputSystemExtension
{

    /// <summary>
    /// Gamepad拓展方法
    /// </summary>
    public static class GamepadExtensions  
    {

        /// <summary>
        /// 根据ButtonControl获取GamepadButton
        /// </summary>
        /// <param name="gamepad"></param>
        /// <param name="buttonControl"></param>
        /// <returns></returns>
        public static GamepadButton GetGamepadButton(this Gamepad gamepad,ButtonControl buttonControl) 
        {
            foreach (var item in Enum.GetValues(typeof(GamepadButton)))
            {
                GamepadButton b = (GamepadButton)item;
                if (gamepad[b] == buttonControl)
                    return b;
            }

            return GamepadButton.A;
        }
    
    }
}

