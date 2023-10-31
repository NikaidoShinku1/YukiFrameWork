///=====================================================
/// - FileName:      BindableProperty.cs
/// - NameSpace:     YukiFrameWork
/// - Created:       Yuki
/// - Email:         Yuki@qq.com
/// - Description:   框架绑定数据类
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================

using UnityEngine;
using System;
#if UNITY_2020_3 || UNITY_2021_3 || UNITY_2022_3
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
#endif
namespace YukiFrameWork
{
    // <summary>
    /// 属性绑定类，自增事件绑定
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class BindableProperty<T>
    {
        private Action<T> OnValueChange;   
        [field: SerializeField] private T value;
        public T Value
        {
            get => value;
            set
            {
                if (!this.value.Equals(value))
                {
                    this.value = value;
                    OnValueChange?.Invoke(value);
                }
            }
        }

        public BindableProperty(T value = default)
        {
            this.value = value;
        }       

        /// <summary>
        /// 注册事件
        /// </summary>
        /// <param name="action">事件本体</param>
        /// <returns>返回自身</returns>
        public BindableProperty<T> Register(Action<T> action)
        {
            OnValueChange += action;
            return this;
        }

        /// <summary>
        /// 注册事件,在注册时自动初始化
        /// </summary>
        /// <param name="action">事件本体</param>
        /// <returns>返回自身</returns>
        public BindableProperty<T> RegisterWithInitValue(Action<T> action)
        {
            OnValueChange += action;
            OnValueChange?.Invoke(value);
            return this;
        }

        /// <summary>
        /// 事件注销
        /// </summary>
        public void UnRegisterEvent(Action<T> onEvent = null)
        {
            if(onEvent != null)
                OnValueChange -= onEvent;
            else
                OnValueChange = null;
        }
#if UNITY_2020 || UNITY_2021 || UNITY_2022
        /// <summary>
        /// 注销事件，并且绑定MonoBehaviour生命周期,当销毁的时自动注销事件
        /// </summary>
        /// <param name="gameObject">GameObject</param>
        public void UnRegisterWaitGameObjectDestroy<Component>(Component component,Action callBack = null) where Component : UnityEngine.Component
        {
            _ = _UnRegisterWaitGameObjectDestroy(component,callBack);
        }

        private async UniTaskVoid _UnRegisterWaitGameObjectDestroy<Component>(Component component, Action callBack = null) where Component : UnityEngine.Component
        {
            await component.gameObject.OnDestroyAsync();
            callBack?.Invoke();
            UnRegisterEvent();
        }

    }
#endif
}