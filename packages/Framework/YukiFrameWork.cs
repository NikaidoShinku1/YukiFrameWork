///=====================================================
/// - FileName:      YukiFrameWork.cs
/// - NameSpace:     YukiFrameWork
/// - Created:       Yuki
/// - Email:         Yuki@qq.com
/// - Description:   框架核心架构
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================

using System.Collections.Generic;
using System;
using YukiFrameWork.Command;
using Cysharp.Threading.Tasks;
using YukiFrameWork.MVC;
using YukiFrameWork.ECS;

namespace YukiFrameWork
{
    /// <summary>
    /// 框架体系结构
    /// </summary>
    public interface IArchitecture
    {
        ICommandCenter CommandCenter { get; }
        
        void RegisterModel<T>(T model) where T : class, IModel;
        void RegisterView<T>(T view) where T : class, IView;
        T GetModel<T>() where T : class, IModel;
        T GetView<T>() where T : class, IView;
        void RegisterEvent<T>(Action<T> onEvent);
        void SendEvent<T>(T t = default);
        void UnRegisterEvent<T>(Action<T> onEvent = null);
        void Clear();
    }

    public interface IRegisterModelOrView : IGetArchitecture
    {

    }

    public interface IGetModel : IGetArchitecture
    {

    }

    public interface IGetView : IGetArchitecture
    {

    }

    public interface IGetCommandCenter : IGetArchitecture
    {

    }

    public interface IGetEventTrigger : IGetArchitecture
    {

    }

    public interface IGetRegisterEvent : IGetArchitecture
    {

    }   

    public interface IGetArchitecture
    {
        IArchitecture GetArchitecture();
    }

    public interface ISetArchitecture
    {
        void SetArchitecture(IArchitecture architecture);
    }

    public class Architecture : IArchitecture,IDisposable
    {
        private readonly Dictionary<Type, object> architectureContainer = new Dictionary<Type, object>();
        private readonly EasyEventSystem eventSystem = new EasyEventSystem();
        public ICommandCenter CommandCenter { get; } = new CommandCenter();     
        public void RegisterModel<T>(T model) where T : class, IModel
        {
            if (Register(model))
            {
                model.SetArchitecture(this);
                model.Init();
            }
        }

        public void RegisterView<T>(T view) where T : class, IView
        {
            if (Register(view))
            {
                view.SetArchitecture(this);
                view.Init();
            }
        }

        private bool Register<T>(T t) where T : class
        {
            if (architectureContainer.ContainsKey(t.GetType()))
            {              
                return false;
            }
            architectureContainer.Add(t.GetType(), t);
            return true;
        }

        public T GetModel<T>() where T : class, IModel
            => Get<T>();           

        public T GetView<T>() where T : class, IView
            => Get<T>();

        private T Get<T>() where T : class
        {
            foreach (var m in architectureContainer.Values)
            {
                if (m is T t)
                {
                    return t;
                }
            }
            return null;
        }

        public void Clear()
        {
            architectureContainer.Clear();
        }

        public void Dispose()
        {
            Clear();
        }

        public void RegisterEvent<T>(Action<T> onEvent) => eventSystem.GetOrAddEvent<EasyEvent<T>>().RegisterEvent(onEvent);

        public void SendEvent<T>(T arg = default) => eventSystem.GetEvent<EasyEvent<T>>().EventTrigger(arg);

        public void UnRegisterEvent<T>(Action<T> onEvent = null)
            => eventSystem.GetEvent<EasyEvent<T>>().UnRegister(onEvent);
        
    }

    public interface IEasyEventSystem
    {

    }

    public class EasyEventSystem
    {
        private readonly Dictionary<Type, IEasyEventSystem> events = new Dictionary<Type, IEasyEventSystem>();

        public T GetOrAddEvent<T>() where T : IEasyEventSystem, new()
        {        
            if (!events.TryGetValue(typeof(T), out var easyEvent))
            {
                easyEvent = new T();
                events.Add(typeof(T), easyEvent);
            }
            return (T)easyEvent;
        }

        public T GetEvent<T>() where T : IEasyEventSystem => (T)events[typeof(T)];

        public void AddEvent<T>() where T : IEasyEventSystem, new() => events.Add(typeof(T), new T()); 
    }

    public class EasyEvent<T> : IEasyEventSystem
    {
        Action<T> OnEasyEvent;     

        public void RegisterEvent(Action<T> onEvent)
        {
            OnEasyEvent += onEvent;
        }

        public void EventTrigger(T t)
        {
            OnEasyEvent?.Invoke(t);
        }

        public void UnRegister(Action<T> onEvent = null)
        {
            if (onEvent == null) OnEasyEvent = null;
            else OnEasyEvent -= onEvent;
        }        
    }

    /// <summary>
    /// 控制器拓展
    /// </summary>
    public static class ControllerExtension
    {
        public static void RegisterModel<T>(this IRegisterModelOrView register, T model) where T : class, IModel
            => register.GetArchitecture().RegisterModel<T>(model);

        public static void RegisterView<T>(this IRegisterModelOrView register, T view) where T : class, IView
            => register.GetArchitecture().RegisterView<T>(view);

        public static T GetView<T>(this IGetView register) where T : class, IView
            => register.GetArchitecture().GetView<T>(); 

        public static T GetModel<T>(this IGetModel register) where T : class, IModel
            => register.GetArchitecture().GetModel<T>();

        public static void Clear(this IGetArchitecture architecture)
            => architecture.Clear();
    }

    /// <summary>
    /// 事件系统拓展
    /// </summary>
    public static class EventSystemExtension
    {
        public static void RegisterEvent<T>(this IGetRegisterEvent registerEvent, Action<T> onEvent)
            => registerEvent.GetArchitecture().RegisterEvent(onEvent);

        public static void UnRegisterEvent<T>(this IGetRegisterEvent registerEvent, Action<T> onEvent = null)
            => registerEvent.GetArchitecture().UnRegisterEvent(onEvent);

        public static void SendEvent<T>(this IGetEventTrigger eventTrigger,T arg)
            => eventTrigger.GetArchitecture().SendEvent(arg);
    }

    /// <summary>
    /// 命令中心拓展
    /// </summary>
    public static class CommandCenterExtension
    {
        public static bool SendCommand<T>(this IGetCommandCenter CommandCenter, IObjectContainer container = null, object data = null) where T : ICommand, new()
            => CommandCenter.GetArchitecture().CommandCenter.SendCommand<T>(container, data);

        public static bool SendCommand<T>(this IGetCommandCenter CommandCenter, T command, IObjectContainer container = null, object data = null) where T : ICommand
            => CommandCenter.GetArchitecture().CommandCenter.SendCommand(command, container, data);

        public static async UniTask Send_AsyncCommand<T>(this IGetCommandCenter CommandCenter, T command, IObjectContainer container = null, object data = null) where T : ICommand
            => await CommandCenter.GetArchitecture().CommandCenter.Send_AsyncCommand<T>(command, container, data);

        public static async UniTask Send_AsyncCommand<T>(this IGetCommandCenter CommandCenter, IObjectContainer container = null, object data = null) where T : ICommand,new()
            => await CommandCenter.GetArchitecture().CommandCenter.Send_AsyncCommand<T>(container, data);
    }   
}
