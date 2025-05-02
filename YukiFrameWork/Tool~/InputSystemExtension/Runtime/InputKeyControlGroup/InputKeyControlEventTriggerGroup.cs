///=====================================================
/// - FileName:      InputKeyControlEventTriggerGroup .cs
/// - NameSpace:     YukiFrameWork.InputSystemExtension
/// - Description:   高级定制脚本生成
/// - Creation Time: 2025/4/19 12:22:36
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;
namespace YukiFrameWork.InputSystemExtension
{
    /// <summary>
    ///  输入回调函数 将按键分配给组或通过传递InputActionMap配置后，注册相对应的事件在每当输入检测符合条件时自动触发
    /// </summary>
    /// <param name="context">当通过ActionMap触发按键时有值</param>
    /// <param name="inputKeyControl">当通过使用框架InputKeyControl触发有值</param>
    public delegate void InputCallBackContext(InputAction.CallbackContext context,InputKeyControl inputKeyControl);

    /// <summary>
    /// 按键事件触发分组
    /// <para>Tip:当InputKeyControl调用了EventTriggerEnable方法，则会自动添加进指定分组中。此时可以通过分组进行按键的事件注册。在指定按键触发后会发送！</para>
    /// </summary>
    public class InputKeyControlEventTriggerGroup : IDisposable
    {
        #region static
        private static Dictionary<string, InputKeyControlEventTriggerGroup> all_runtime_inputKeyControl_groups;
        
        static InputKeyControlEventTriggerGroup()
        {
            all_runtime_inputKeyControl_groups = new Dictionary<string, InputKeyControlEventTriggerGroup>();
            if (!Application.isPlaying) return;
            MonoHelper.Update_AddListener(Update);
        }

        /// <summary>
        /// 创建多个按键事件触发分组
        /// </summary>
        /// <param name="groupNames"></param>
       
        public static void CreateEventTriggerGroups(params string[] groupNames)
        {
            if (groupNames == null || groupNames.Length == 0)
                return;

            for (int i = 0; i < groupNames.Length; i++)
            {
                CreateEventTriggerGroup(groupNames[i]);
            }
        }

        /// <summary>
        /// 创建按钮事件触发分组
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static InputKeyControlEventTriggerGroup CreateEventTriggerGroup(string groupName)
        {
            try
            {
                var item = new InputKeyControlEventTriggerGroup(groupName);
                all_runtime_inputKeyControl_groups.Add(groupName, item);
                return item;
            }
            catch (Exception ex)
            {
                throw new Exception("存在相同组名的按键分组,请检查!" + ex.ToString());
            }
        }

        /// <summary>
        /// 通过InputActionAsset的配置取得指定的InputActionMap传递，会传递在ActionMap下所有的Action
        /// <para>每一个Action都可指定复数按键配置，故一个Action即为一个InputKeyControlEventTriggerGroup分组</para>
        /// <para>分组创建后仍可注册InputKeyControl，可同时使用</para>
        /// <para>Tip：请注意，一个Action会返回一个分组,必须保证Action的名称是唯一的</para>
        /// </summary>
        /// <param name="maps">在InputActionAsset中的任意一个InputActionMap</param>
        /// <returns></returns>
        public static InputKeyControlEventTriggerGroup[] CreateEventTriggerGroupsByInputAction(InputActionMap maps)
        {
            InputKeyControlEventTriggerGroup[] inputKeyControlEventTriggerGroups = new InputKeyControlEventTriggerGroup[maps.actions.Count];
           
            for (int i = 0; i < maps.actions.Count; i++)
            {
                var action = maps.actions[i];           
                inputKeyControlEventTriggerGroups[i] = CreateEventTriggerGroupByInputAction(action);
            }
            return inputKeyControlEventTriggerGroups;                       
        }

        /// <summary>
        /// 传递指定的InputAction以创建分组. 组名称为InputAction的名称
        /// </summary>
        /// <param name="inputActions"></param>
        /// <returns></returns>
        public static InputKeyControlEventTriggerGroup CreateEventTriggerGroupByInputAction(InputAction inputActions)
        {        
            var group = CreateEventTriggerGroup(inputActions.name);
            group.InputAction = inputActions;
            return group;
        }
        /// <summary>
        ///  释放所有已经构建出来的分组
        /// </summary>
        /// <param name="IsClear">分组实例是否一起清除</param>
        public static void Release(bool IsClear = false)
        {
            if (!Application.isPlaying) return;          
            foreach (var item in all_runtime_inputKeyControl_groups)
            {
                item.Value.Dispose();
            }

            if (IsClear)
                all_runtime_inputKeyControl_groups.Clear();
        }

        /// <summary>
        /// 释放指定分组
        /// </summary>
        /// <param name="groupName"></param>
        /// <param name="IsClear">分组实例是否一起清除</param>
        public static void ReleaseGroup(string groupName,bool IsClear = false)
        {
            if (!Application.isPlaying) return;
            if (all_runtime_inputKeyControl_groups.ContainsKey(groupName))
            {
                all_runtime_inputKeyControl_groups[groupName].Dispose();
                if (IsClear)
                    all_runtime_inputKeyControl_groups.Remove(groupName);
            }
        }

        /// <summary>
        /// 获取按键分组
        /// </summary>
        /// <param name="groupName">分组名称(标识)</param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        public static InputKeyControlEventTriggerGroup GetInputKeyControlEventTriggerGroup(string groupName)
        {
            if (all_runtime_inputKeyControl_groups.TryGetValue(groupName, out var group))
                return group;

            throw new NullReferenceException($"没有通过CreateEventTriggerGroup方法创建按键分组,请检查:groupName:{groupName}");
        }

        static void Update(MonoHelper _)
        {
            foreach (var group in all_runtime_inputKeyControl_groups)
            {
                group.Value.Update();
            }
        }
        #endregion
       
        private Dictionary<string, InputKeyControl> keyControl_dict;

        /// <summary>
        /// 分组的名称
        /// </summary>
        public string GroupName { get; }
        /// <summary>
        /// 分组是否处于运行状态
        /// </summary>
        public bool IsRunning { get; private set; }
     
        /// <summary>
        /// 当按住键
        /// </summary>
        public event InputCallBackContext onKey;
        /// <summary>
        /// 当按下键
        /// </summary>
        public event InputCallBackContext onKeyDown;
        /// <summary>
        /// 当抬起键
        /// </summary>
        public event InputCallBackContext onKeyUp;

        private InputAction inputAction;
        
        /// <summary>
        /// 这个分组所绑定的InputAction
        /// </summary>
        public InputAction InputAction
        {
            get
            {
                return inputAction;
            }
            set
            {              
                if (inputAction != value)
                {
                    inputAction?.Disable();
                    inputAction = value;

                    if (inputAction == null) return;

                    inputAction.started += OnInputKeyDown;
                    inputAction.performed += OnInputKey;
                    inputAction.canceled += OnInputKeyUp;

                    //如果正在运行
                    if (IsRunning)
                        inputAction.Enable();
                }
            }
        }

        internal InputKeyControlEventTriggerGroup(string groupName)
        {
            GroupName = groupName;
            keyControl_dict = new Dictionary<string, InputKeyControl>();
        }

        private void Update()
        {
            if (!IsRunning) return;
            
            foreach (var item in keyControl_dict)
            {
                var control = item.Value;              
                if (control.GetKeyDown())
                    onKeyDown?.Invoke(default,control);

                if (control.GetKey())
                    onKey?.Invoke(default,control);

                if (control.GetKeyUp())
                    onKeyUp?.Invoke(default,control);
            }           
        }
        
        /// <summary>
        /// 这个分组注册的所有按键
        /// </summary>
        public InputKeyControl[] KeyControls => keyControl_dict.Values.ToArray();
       
        /// <summary>
        /// 添加按键控制
        /// </summary>
        /// <param name="keyControl"></param>
        public void AddKeyControl(InputKeyControl keyControl)
        {
            try
            {
                keyControl_dict.Add(keyControl.Name, keyControl);
            }
            catch (Exception ex)
            {
                throw new Exception($"添加按键异常(这个按键已经有了)keyName:{keyControl.Name} --- {ex}");
            }
        }

        /// <summary>
        /// 添加多个按键控制
        /// </summary>
        /// <param name="keyControls"></param>
        public void AddKeyControls(params InputKeyControl[] keyControls)
        {
            if (keyControls == null || keyControls.Length == 0)
                return;

            for (int i = 0; i < keyControls.Length; i++)
            {
                var control = keyControls[i];
                AddKeyControl(control);
            }
        }

        /// <summary>
        /// 传递按键控制以在该分组中移除
        /// </summary>
        /// <param name="keyControl"></param>
        public void RemoveKeyControl(InputKeyControl keyControl)
        {
            RemoveKeyControl(keyControl.Name);
        }

        /// <summary>
        /// 通过标识移除按键控制
        /// </summary>
        /// <param name="keyName"></param>
        public void RemoveKeyControl(string keyName)
        {
            keyControl_dict.Remove(keyName);
        }


        public void Enable()
        {
            IsRunning = true;         
            InputAction?.Enable();
            
        }

        public void Disable()
        {
            IsRunning = false;
            InputAction?.Disable();
        }
        public void Dispose()
        {
            onKey = null;
            onKeyDown = null;
            onKeyUp = null;
            keyControl_dict.Clear();
            InputAction?.Dispose();
        }
        private void OnInputKeyDown(InputAction.CallbackContext context)
        {
            onKeyDown?.Invoke(context,default);
        }

        private void OnInputKey(InputAction.CallbackContext context)
        {
            onKey?.Invoke(context, default);
        }

        private void OnInputKeyUp(InputAction.CallbackContext context)
        {
            onKeyUp?.Invoke(context, default);
        }
    }
}
