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
    public interface IBindableProperty
    {
        void UnRegisterAllEvent();
    }

    public interface IBindableProperty<T1, T2, T3, T4> : IBindableProperty
    {
        IUnRegister Register(Action<T1, T2, T3, T4> action);
        IUnRegister RegisterWithInitValue(Action<T1, T2, T3, T4> action);
    }

    public interface IBindableProperty<T1, T2, T3> : IBindableProperty
    {
        IUnRegister Register(Action<T1, T2, T3> action);
        IUnRegister RegisterWithInitValue(Action<T1, T2, T3> action);
    }

    public interface IBindableProperty<T1, T2> : IBindableProperty
    {
        IUnRegister Register(Action<T1,T2> action);
        IUnRegister RegisterWithInitValue(Action<T1,T2> action);
    }

    public interface IBindableProperty<T> : IBindableProperty
    {
        IUnRegister Register(Action<T> action);
        IUnRegister RegisterWithInitValue(Action<T> action);       
    }
  
    // <summary>
    /// 属性绑定类，自增事件绑定
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class BindableProperty<T> : IBindableProperty<T>
    {
        [NonSerialized]
        protected EasyEvent<T> onValueChange = new EasyEvent<T>();
     
        [field: SerializeField] private T value;
        public T Value
        {
            get => value;
            set
            {
                if (!Equals(this.value,value))
                {
                    this.value = value;
                    onValueChange?.SendEvent(value);
                }
            }
        }     

        public BindableProperty(T value)
        {
            this.value = value;
        }

        public BindableProperty()
        {
            this.value = default;
        }

        public override string ToString()
        {
            return value.ToString();
        }

        /// <summary>
        /// 注册事件
        /// </summary>
        /// <param name="action">事件本体</param>
        /// <returns>返回自身</returns>
        public IUnRegister Register(Action<T> action)
        {
            onValueChange.RegisterEvent(action);
            return onValueChange;
        }

        /// <summary>
        /// 注册事件,在注册时自动初始化
        /// </summary>
        /// <param name="action">事件本体</param>
        /// <returns>返回自身</returns>
        public IUnRegister RegisterWithInitValue(Action<T> action)
        {
            action.Invoke(value);
            onValueChange.RegisterEvent(action);           
            return onValueChange;
        }

        /// <summary>
        /// 事件注销
        /// </summary>
        public void UnRegister(Action<T> onEvent)
        {
            onValueChange.UnRegister(onEvent);
        }

        /// <summary>
        /// 注销全部事件
        /// </summary>
        public void UnRegisterAllEvent()
        {
            onValueChange.UnRegisterAllEvent();
        }
    }

    public class BindableProperty<T1, T2> : IBindableProperty<T1, T2>
    {
        [NonSerialized]
        protected EasyEvent<T1, T2> onValueChanged = new EasyEvent<T1, T2>();
        public IUnRegister Register(Action<T1, T2> action)
        {
            return onValueChanged.RegisterEvent(action);
        }

        private T1 value1;
        private T2 value2;

        public T1 Value1
        {
            get => value1;
            set
            {
                if (!Equals(value, value1))
                {
                    value1 = value;
                    onValueChanged.SendEvent(value1,value2);
                }
            }
        }

        public T2 Value2
        {
            get => value2;
            set
            {
                if (!Equals(value, value2))
                {
                    value2 = value;
                    onValueChanged.SendEvent(value1, value2);
                }
            }
        }

        public IUnRegister RegisterWithInitValue(Action<T1, T2> action)
        {
            action.Invoke(value1,value2);
            return onValueChanged.RegisterEvent(action);
        }

        public void UnRegisterAllEvent()
        {
            onValueChanged.UnRegisterAllEvent();
        }
    }
}
