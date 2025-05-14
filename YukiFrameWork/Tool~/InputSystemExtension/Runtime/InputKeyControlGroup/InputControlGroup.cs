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
    public delegate void InputCallBackContext(InputAction.CallbackContext context, InputKeyControl inputKeyControl);

    /// <summary>
    /// 按键控制分组 该分组可以注册多个不同的InputKeyControl按键控制类，同时一个分组可以绑定一个InputAction，注册进分组的InputAction，支持全动态改键。
    /// </summary>
    public class InputControlGroup : IDisposable
    {
        #region static
        private static Dictionary<string, InputControlGroup> all_runtime_inputKeyControl_groups;

        static InputControlGroup()
        {
            all_runtime_inputKeyControl_groups = new Dictionary<string, InputControlGroup>();
            if (!Application.isPlaying) return;
            MonoHelper.Update_AddListener(Update);
        }

        /// <summary>
        /// 创建多个按键事件触发分组
        /// </summary>
        /// <param name="groupNames"></param>

        public static void CreateInputControlGroups(params string[] groupNames)
        {
            if (groupNames == null || groupNames.Length == 0)
                return;

            for (int i = 0; i < groupNames.Length; i++)
            {
                CreateInputControlGroup(groupNames[i]);
            }
        }

        public static void ForEach(Action<InputControlGroup> each)
        {
            foreach (var item in all_runtime_inputKeyControl_groups.Values)
            {
                each?.Invoke(item);
            }
        }

        /// <summary>
        /// 创建按钮事件触发分组
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static InputControlGroup CreateInputControlGroup(string groupName)
        {
            try
            {
                var item = new InputControlGroup(groupName);
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
        public static InputControlGroup[] CreateInputControlGroupsByInputAction(InputActionMap maps)
        {
            InputControlGroup[] inputKeyControlEventTriggerGroups = new InputControlGroup[maps.actions.Count];

            for (int i = 0; i < maps.actions.Count; i++)
            {
                var action = maps.actions[i];
                inputKeyControlEventTriggerGroups[i] = CreateInputControlGroupByInputAction(action);
            }
            return inputKeyControlEventTriggerGroups;
        }

        /// <summary>
        /// 传递指定的InputAction以创建分组. 组名称为InputAction的名称
        /// </summary>
        /// <param name="inputActions"></param>
        /// <returns></returns>
        public static InputControlGroup CreateInputControlGroupByInputAction(InputAction inputActions)
        {
            var group = CreateInputControlGroup(inputActions.name);
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
        public static void ReleaseGroup(string groupName, bool IsClear = false)
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
        public static InputControlGroup GetInputControlGroup(string groupName)
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

        #region InputAction
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

                    if (PlayerPrefs.HasKey(inputActionSavingKey))                    
                        inputAction.LoadBindingOverridesFromJson(PlayerPrefs.GetString(inputActionSavingKey));                   
                    inputAction.started += OnInputKeyDown;
                    inputAction.performed += OnInputKey;
                    inputAction.canceled += OnInputKeyUp;

                    //如果正在运行
                    if (IsRunning)
                        inputAction.Enable();
                }
            }
        }

        /// <summary>
        /// 当对InputAction配置进行按键重映射完成后触发
        /// </summary>
        public event Action onBindRebind = null;

        /// <summary>
        /// 当对InputAction配置进行按键重映射取消后触发
        /// </summary>
        public event Action onBindCancel = null;

        private string inputActionSavingKey => $"{GroupName}_INPUTACTIONSAVINTKEY_INPUTKEYCONTROLEVENTTRIGGERGROUP_{inputAction.name}"; 
        /// <summary>
        /// 如果分组设置了InputAction，则可以使用该方法进行对InputAction的自动重改键操作。改键会进行本地化储存。必须保证存在的InputAction名称是唯一的
        /// <para>当调用了该方法进行自动化按键映射，则当在方法触发后你下一个按下的任意按键即视为你重映射的按键。</para>
        /// <para>需要为该方法传递BindingIndex下标，可传递onBindRebound回调。该回调会早于onBindRebind事件触发。</para>
        /// </summary>
        /// <param name="bindingIndex">按键映射下标</param>
        /// <param name="onBindRebound">按键绑定完成触发回调</param>
        /// <exception cref="NullReferenceException"></exception>
        public void AutoPerformInteractiveRebinding(int bindingIndex, Action onBindRebound = null, Action onCancel = null)
        {
            if (InputAction == null)
                throw new NullReferenceException("该按键绑定分组没有设置InputAction,无法配置对InputAction的改键 InputControlGroup GroupName:" + GroupName);
            InputAction.Disable();
            PerformIntarctiveRebinding(bindingIndex,onBindRebound,onCancel).Start();
        }

        /// <summary>
        /// 如果分组设置了InputAction，则可以使用该方法进行对InputAction的重改键操作。改键会进行本地化储存。必须保证存在的InputAction名称是唯一的
        /// <para>需要为该方法传递BindingIndex下标，可传递onBindRebound回调。该回调会早于onBindRebind事件触发。</para>
        /// <para>注意，非自动化方法需要用户手动启动以及完成对按键的重映射，在这个阶段，该InputAction在完成之前都会处于Disable</para>
        /// <code>public void Test()
        /// {
        ///     var group = InputKeyControlEventTriggerGroup.GetInputKeyControlEventTriggerGroup("My Group Name");
        ///     var operation = group.PerformIntarctiveRebinding(0,() => { });
        ///     operation.Start();//手动启动
        ///     operation.Complete();//手动完成
        /// }</code>
        /// </summary>
        /// <param name="bindingIndex">按键映射下标</param>
        /// <param name="onBindRebound"></param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        public InputActionRebindingExtensions.RebindingOperation PerformIntarctiveRebinding(int bindingIndex,Action onBindRebound = null,Action onCancel = null)
        {
            if (InputAction == null)
                throw new NullReferenceException("该按键绑定分组没有设置InputAction,无法配置对InputAction的改键 InputControlGroup GroupName:" + GroupName);
            InputAction.Disable();
            var operation = InputAction.PerformInteractiveRebinding(bindingIndex).OnComplete(callBack =>
            {
                callBack.Dispose();
                if (IsRunning)
                    InputAction.Enable();
                onBindRebound?.Invoke();
                PlayerPrefs.SetString(inputActionSavingKey, InputAction.SaveBindingOverridesAsJson());
                PlayerPrefs.Save();
                onBindRebind?.Invoke();
            });
            operation.OnCancel(callBack => 
            {
                callBack.Dispose();
                onCancel?.Invoke();
                onBindCancel?.Invoke();
            });
            return operation;
        }

        /// <summary>
        /// 根据Binding的DisplayName进行BindingIndex的获取操作
        /// <para>如配置中的Up:W[Keyboard] --->displayName为W</para>
        /// </summary>
        /// <param name="displayName"></param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        public int FindBindingIndex(string displayName)
        {
            if (InputAction == null)
                throw new NullReferenceException("该按键绑定分组没有设置InputAction,无法获取BindingIndex InputControlGroup GroupName:" + GroupName);

            for (int i = 0; i < InputAction.bindings.Count; i++)
            {
                int index = i;
                InputBinding bind = InputAction.bindings[i];

                if (bind.ToDisplayString() == displayName)
                    return index;
            }

            return -1;
        }

        public int FindBindingIndex(InputBinding binding)
            => FindBindingIndex(binding.ToDisplayString());

        #endregion
        internal InputControlGroup(string groupName)
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
                    onKeyDown?.Invoke(default, control);

                if (control.GetKey())
                    onKey?.Invoke(default, control);

                if (control.GetKeyUp())
                    onKeyUp?.Invoke(default, control);
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
            onKeyDown?.Invoke(context, default);
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
