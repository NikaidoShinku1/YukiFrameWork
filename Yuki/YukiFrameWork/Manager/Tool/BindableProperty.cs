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
namespace YukiFrameWork
{   
    public interface IBindableProperty<TValue>
    {
        IUnRegister Register(Action<TValue> action);
        IUnRegister RegisterWithInitValue(Action<TValue> action);       
    }
    // <summary>
    /// 属性绑定类，自增事件绑定
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class BindableProperty<T> : IBindableProperty<T>,IUnRegister
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
        public IUnRegister Register(Action<T> action)
        {
            OnValueChange += action;
            return this;
        }

        /// <summary>
        /// 注册事件,在注册时自动初始化
        /// </summary>
        /// <param name="action">事件本体</param>
        /// <returns>返回自身</returns>
        public IUnRegister RegisterWithInitValue(Action<T> action)
        {
            OnValueChange += action;
            OnValueChange?.Invoke(value);
            return this;
        }

        /// <summary>
        /// 事件注销
        /// </summary>
        public void UnRegisterEvent(Action<T> onEvent)
        {
            OnValueChange -= onEvent;
        }

        /// <summary>
        /// 注销全部事件
        /// </summary>
        public void UnRegisterAllEvent()
        {
            OnValueChange = null;
        }
    }
}
