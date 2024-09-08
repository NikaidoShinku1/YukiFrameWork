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
        void SetValue(params object[] args);
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
                    if (value == null) return;
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

        void IBindableProperty.SetValue(params object[] args)
        {
            Value = (T)args[0];
        }
    }

    #region 快速持久化属性绑定
    public class BindablePropertyPlayerPrefsByInteger : BindableProperty<int>
    {
        public BindablePropertyPlayerPrefsByInteger(string key,int value) : base(value)
        {            
            this.Value = PlayerPrefs.GetInt(key,value);
            this.Register(item => PlayerPrefs.SetInt(key, item));
        }
    }

    public class BindablePropertyPlayerPrefsByBoolan : BindableProperty<bool>
    {
        public BindablePropertyPlayerPrefsByBoolan(string key, bool value) : base(value)
        {          
            this.Value = PlayerPrefs.GetInt(key, value ? 1 : 0) == 1 ? true : false;
            this.Register(item => 
            {
                PlayerPrefs.SetInt(key, item ? 1 : 0);
            });
        }
    }

    public class BindablePropertyPlayerPrefsByFloat : BindableProperty<float>
    {
        public BindablePropertyPlayerPrefsByFloat(string key, float value) : base(value)
        {
            this.Value = PlayerPrefs.GetFloat(key, value);
            this.Register(item => 
            {               
                PlayerPrefs.SetFloat(key, item);
            });
        }
    }

    public class BindablePropertyPlayerPrefsByString : BindableProperty<string>
    {
        public BindablePropertyPlayerPrefsByString(string key, string value) : base(value)
        {
            this.Value = PlayerPrefs.GetString(key, value);
            this.Register(item => PlayerPrefs.SetString(key, item));
        }
    }
    #endregion
}
