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
using YukiFrameWork.MVC;
using YukiFrameWork.UI;
using YukiFrameWork.Res;

namespace YukiFrameWork
{
    /// <summary>
    /// 框架体系结构
    /// </summary>
    public interface IArchitecture
    {
        ICommandCenter CommandCenter { get; }
        IPanelManager PanelManager { get; }
        void RegisterModel<T>(T model) where T : class, IModel;
        void RegisterView<T>(T view) where T : class, IView;
        void InitPanelManager(string panelPath);
        IAsyncExtensionCore InitPanelManagerAsync(string panelPath,Action onFinish = null);
        void UnRegisterModel<T>(T model = default) where T : class,IModel;
        void UnRegisterView<T>(T view = default) where T : class, IView;
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

    public interface IUnRegisterModelOrView : IGetArchitecture
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

    public interface IUIPanelController : IGetArchitecture
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
        private readonly TypeEventSystem eventSystem = new TypeEventSystem();
        public ICommandCenter CommandCenter { get; } = new CommandCenter();

        public IPanelManager PanelManager { get; private set; }

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

        private bool Contains<T>(T t = default) where T : class      
            => architectureContainer.ContainsKey(t.GetType());
        
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

        public void UnRegisterModel<T>(T t = default) where T : class,IModel
        {
            if (Contains(t))
            {
                architectureContainer.Remove(t.GetType());
            }
        }

        public void UnRegisterView<T>(T t = default) where T : class, IView
        {          
            if (Contains(t))
            {              
                architectureContainer.Remove(t.GetType());               
            }
        }

        public void Clear()
        {
            architectureContainer.Clear();
        }

        public void Dispose()
        {
            Clear();
        }

        public void RegisterEvent<T>(Action<T> onEvent) => eventSystem.Register<T>(onEvent);

        public void SendEvent<T>(T arg = default) => eventSystem.Trigger(arg);

        public void UnRegisterEvent<T>(Action<T> onEvent = null)
            => eventSystem.UnRegister(onEvent);

        /// <summary>
        /// 初始化面板管理器
        /// </summary>
        /// <param name="panelPath">填写面板对应的路径文件夹</param>
        public void InitPanelManager(string panelPath)
        {
            PanelManager = new PanelManager(panelPath, LoadMode.同步);
        }

        public IAsyncExtensionCore InitPanelManagerAsync(string panelPath,Action onFinish = null)
        {
            PanelManager = new PanelManager(panelPath, LoadMode.同步);
            return ((PanelManager)PanelManager).InitAsync(onFinish);
        }
    }

    public interface IEasyEventsystem : IUnRegister
    {
        
    }

    public interface IUnRegister
    {
        void UnRegisterAllEvent();
    }

    public class EnumEventSystem
    {
        private readonly EnumEasyEvents events = new EnumEasyEvents();

        public static EnumEventSystem Global { get; } = new EnumEventSystem();

        public IUnRegister Register(Enum enumType,Action onEvent)
        => events.GetOrAddEvent<EasyEvent>(enumType).RegisterEvent(onEvent);

        public IUnRegister Register<T>(Enum enumType, Action<T> onEvent)
            => events.GetOrAddEvent<EasyEvent<T>>(enumType).RegisterEvent(onEvent);

        public IUnRegister Register<T, K>(Enum enumType, Action<T, K> onEvent)
            => events.GetOrAddEvent<EasyEvent<T, K>>(enumType).RegisterEvent(onEvent);

        public IUnRegister Register<T, K, Q>(Enum enumType, Action<T, K, Q> onEvent)
            => events.GetOrAddEvent<EasyEvent<T, K, Q>>(enumType).RegisterEvent(onEvent);

        public IUnRegister Register<T, K, Q, P>(Enum enumType, Action<T, K, Q, P> onEvent)
            => events.GetOrAddEvent<EasyEvent<T, K, Q, P>>(enumType).RegisterEvent(onEvent);

        public void Trigger(Enum enumType)
            => events.GetEvent<EasyEvent>(enumType)?.EventTrigger();

        public void Trigger<T>(Enum enumType,T t)
            => events.GetEvent<EasyEvent<T>>(enumType)?.EventTrigger(t);

        public void Trigger<T, K>(Enum enumType,T t, K k)
            => events.GetEvent<EasyEvent<T, K>>(enumType)?.EventTrigger(t, k);

        public void Trigger<T, K, Q>(Enum enumType,T t, K k, Q q)
            => events.GetEvent<EasyEvent<T, K, Q>>(enumType)?.EventTrigger(t, k, q);

        public void Trigger<T, K, Q, P>(Enum enumType,T t, K k, Q q, P p)
            => events.GetEvent<EasyEvent<T, K, Q, P>>(enumType)?.EventTrigger(t, k, q, p);

        public void UnRegister(Enum enumType, Action onEvent)
            => events.GetEvent<EasyEvent>(enumType)?.UnRegister(onEvent);

        public void UnRegister<T>(Enum enumType, Action<T> onEvent)
            => events.GetEvent<EasyEvent<T>>(enumType)?.UnRegister(onEvent);

        public void UnRegister<T, K>(Enum enumType, Action<T, K> onEvent)
            => events.GetEvent<EasyEvent<T, K>>(enumType)?.UnRegister(onEvent);

        public void UnRegister<T, K, Q>(Enum enumType, Action<T, K, Q> onEvent)
            => events.GetEvent<EasyEvent<T, K, Q>>(enumType)?.UnRegister(onEvent);

        public void UnRegister<T, K, Q, P>(Enum enumType, Action<T, K, Q, P> onEvent)
            => events.GetEvent<EasyEvent<T, K, Q, P>>(enumType)?.UnRegister(onEvent);
    }

    public class StringEventSystem
    {
        private readonly StringEasyEvents events = new StringEasyEvents();

        public static StringEventSystem Global { get; } = new StringEventSystem();

        public IUnRegister Register(string enumType, Action onEvent)
      => events.GetOrAddEvent<EasyEvent>(enumType).RegisterEvent(onEvent);

        public IUnRegister Register<T>(string enumType, Action<T> onEvent)
            => events.GetOrAddEvent<EasyEvent<T>>(enumType).RegisterEvent(onEvent);

        public IUnRegister Register<T, K>(string enumType, Action<T, K> onEvent)
            => events.GetOrAddEvent<EasyEvent<T, K>>(enumType).RegisterEvent(onEvent);

        public IUnRegister Register<T, K, Q>(string enumType, Action<T, K, Q> onEvent)
            => events.GetOrAddEvent<EasyEvent<T, K, Q>>(enumType).RegisterEvent(onEvent);

        public IUnRegister Register<T, K, Q, P>(string enumType, Action<T, K, Q, P> onEvent)
            => events.GetOrAddEvent<EasyEvent<T, K, Q, P>>(enumType).RegisterEvent(onEvent);

        public void Trigger(string enumType)
            => events.GetEvent<EasyEvent>(enumType)?.EventTrigger();

        public void Trigger<T>(string enumType, T t)
            => events.GetEvent<EasyEvent<T>>(enumType)?.EventTrigger(t);

        public void Trigger<T, K>(string enumType, T t, K k)
            => events.GetEvent<EasyEvent<T, K>>(enumType)?.EventTrigger(t, k);

        public void Trigger<T, K, Q>(string enumType, T t, K k, Q q)
            => events.GetEvent<EasyEvent<T, K, Q>>(enumType)?.EventTrigger(t, k, q);

        public void Trigger<T, K, Q, P>(string enumType, T t, K k, Q q, P p)
            => events.GetEvent<EasyEvent<T, K, Q, P>>(enumType)?.EventTrigger(t, k, q, p);

        public void UnRegister(string enumType, Action onEvent)
            => events.GetEvent<EasyEvent>(enumType)?.UnRegister(onEvent);

        public void UnRegister<T>(string enumType, Action<T> onEvent)
            => events.GetEvent<EasyEvent<T>>(enumType)?.UnRegister(onEvent);

        public void UnRegister<T, K>(string enumType, Action<T, K> onEvent)
            => events.GetEvent<EasyEvent<T, K>>(enumType)?.UnRegister(onEvent);

        public void UnRegister<T, K, Q>(string enumType, Action<T, K, Q> onEvent)
            => events.GetEvent<EasyEvent<T, K, Q>>(enumType)?.UnRegister(onEvent);

        public void UnRegister<T, K, Q, P>(string enumType, Action<T, K, Q, P> onEvent)
            => events.GetEvent<EasyEvent<T, K, Q, P>>(enumType)?.UnRegister(onEvent);
    }

    public class TypeEventSystem
    {
        private readonly TypeEasyEvents events = new TypeEasyEvents();

        public static TypeEventSystem Global { get; } = new TypeEventSystem();

        public IUnRegister Register(Action onEvent)
            => events.GetOrAddEvent<EasyEvent>().RegisterEvent(onEvent);               

        public IUnRegister Register<T>(Action<T> onEvent)
            => events.GetOrAddEvent<EasyEvent<T>>().RegisterEvent(onEvent);

        public IUnRegister Register<T, K>(Action<T, K> onEvent)
            => events.GetOrAddEvent<EasyEvent<T, K>>().RegisterEvent(onEvent);

        public IUnRegister Register<T, K, Q>(Action<T, K, Q> onEvent)
            => events.GetOrAddEvent<EasyEvent<T, K, Q>>().RegisterEvent(onEvent);

        public IUnRegister Register<T, K, Q, P>(Action<T, K, Q, P> onEvent)
            => events.GetOrAddEvent<EasyEvent<T, K, Q, P>>().RegisterEvent(onEvent);

        public void Trigger()
            => events.GetEvent<EasyEvent>()?.EventTrigger();                
       
        public void Trigger<T>(T t)
            => events.GetEvent<EasyEvent<T>>()?.EventTrigger(t);

        public void Trigger<T, K>(T t, K k)
            => events.GetEvent<EasyEvent<T, K>>()?.EventTrigger(t, k);

        public void Trigger<T, K, Q>(T t, K k, Q q)
            => events.GetEvent<EasyEvent<T, K, Q>>()?.EventTrigger(t, k, q);

        public void Trigger<T, K, Q, P>(T t, K k, Q q, P p)
            => events.GetEvent<EasyEvent<T, K, Q,P>>()?.EventTrigger(t, k, q,p);

        public void UnRegister(Action onEvent)
            => events.GetEvent<EasyEvent>()?.UnRegister(onEvent);

        public void UnRegister<T>(Action<T> onEvent)
            => events.GetEvent<EasyEvent<T>>()?.UnRegister(onEvent);

        public void UnRegister<T,K>(Action<T,K> onEvent)
            => events.GetEvent<EasyEvent<T,K>>()?.UnRegister(onEvent);

        public void UnRegister<T, K,Q>(Action<T, K,Q> onEvent)
            => events.GetEvent<EasyEvent<T, K, Q>>()?.UnRegister(onEvent); 

        public void UnRegister<T, K, Q,P>(Action<T, K, Q,P > onEvent)
            => events.GetEvent<EasyEvent<T, K, Q, P>>()?.UnRegister(onEvent);
    }

    public class StringEasyEvents
    {
        private readonly Dictionary<string, IEasyEventsystem> events = new Dictionary<string, IEasyEventsystem>();

        public T GetOrAddEvent<T>(string eventName) where T : IEasyEventsystem,new()
        {
            if (!events.TryGetValue(eventName, out var easeEvent))
            {
                easeEvent = new T();
                events.Add(eventName, easeEvent);
            }
            else
            {
                Type type = typeof(T);
                if (easeEvent.GetType() != type)
                {
                    throw new Exception($"当前key值{eventName}中已经存在的事件与需要添加的事件类型不符合，请重试！需要添加的类型为{type}");
                }
            }

            return (T)easeEvent;
        }

        public T GetEvent<T>(string eventName) where T : IEasyEventsystem
        {
            events.TryGetValue(eventName, out var easeEvent);
            Type type = typeof(T);
            if (easeEvent.GetType() != type)
            {
                throw new Exception($"当前key值{eventName}中已经存在的事件与需要添加的事件类型不符合，请重试！需要添加的类型为{type}");
            }
            return (T)easeEvent;
        }

        public void AddEvent<T>(string eventName) where T : IEasyEventsystem, new() => events.Add(eventName, new T());
    }


    public class EnumEasyEvents
    {
        private readonly Dictionary<Enum, IEasyEventsystem> events = new Dictionary<Enum, IEasyEventsystem>();

        public T GetOrAddEvent<T>(Enum enumType) where T : IEasyEventsystem, new()
        {
            if (!events.TryGetValue(enumType, out var easeEvent))
            {
                easeEvent = new T();
                events.Add(enumType, easeEvent);
            }
            else
            {
                Type type = typeof(T);
                if (easeEvent.GetType() != type)
                {
                    throw new Exception($"当前key值{enumType}中已经存在的事件与需要添加的事件类型不符合，请重试！需要添加的类型为{type}");
                }
            }

            return (T)easeEvent;
        }

        public T GetEvent<T>(Enum enumType) where T : IEasyEventsystem
        {
            events.TryGetValue(enumType, out var easeEvent);
            Type type = typeof(T);
            if (easeEvent.GetType() != type)
            {
                throw new Exception($"当前key值{enumType}中已经存在的事件与需要添加的事件类型不符合，请重试！需要添加的类型为{type}");
            }
            return (T)easeEvent;
        }

        public void AddEvent<T>(Enum enumType) where T : IEasyEventsystem, new() => events.Add(enumType, new T());
    }

    public class TypeEasyEvents
    {
        private readonly Dictionary<Type, IEasyEventsystem> events = new Dictionary<Type, IEasyEventsystem>();

        public T GetOrAddEvent<T>() where T : IEasyEventsystem, new()
        {        
            if (!events.TryGetValue(typeof(T), out var easyEvent))
            {
                easyEvent = new T();
                events.Add(typeof(T), easyEvent);
            }
            return (T)easyEvent;
        }

        public T GetEvent<T>() where T : IEasyEventsystem
        {
            events.TryGetValue(typeof(T), out var eventSystem);          
            return (T)eventSystem;
        }

        public void AddEvent<T>() where T : IEasyEventsystem, new() => events.Add(typeof(T), new T()); 
    }

    public class EasyEvent : IEasyEventsystem
    {
        Action OnEasyEvent;

        public IUnRegister RegisterEvent(Action onEvent)
        {
            OnEasyEvent += onEvent;
            return this;
        }

        public void EventTrigger()
        {
            OnEasyEvent?.Invoke();
        }

        public void UnRegister(Action onEvent)
        {
            OnEasyEvent -= onEvent;
        }

        public void UnRegisterAllEvent()
        {
            OnEasyEvent = null;
        }
    }

    public class EasyEvent<T> : IEasyEventsystem
    {
        Action<T> OnEasyEvent;     

        public IUnRegister RegisterEvent(Action<T> onEvent)
        {
            OnEasyEvent += onEvent;
            return this;
        }

        public void EventTrigger(T t)
        {
            OnEasyEvent?.Invoke(t);
        }

        public void UnRegister(Action<T> onEvent)
        {
            OnEasyEvent -= onEvent;
        }

        public void UnRegisterAllEvent()
        {
            OnEasyEvent = null;
        }
    }

    public class EasyEvent<T, K> : IEasyEventsystem
    {
        Action<T,K> OnEasyEvent;

        public IUnRegister RegisterEvent(Action<T, K> onEvent)
        {
            OnEasyEvent += onEvent;           
            return this;
        }

        public void EventTrigger(T t, K k)
            => OnEasyEvent?.Invoke(t, k);

        public void UnRegister(Action<T, K> onEvent)
        {
             OnEasyEvent -= onEvent;
        }

        public void UnRegisterAllEvent()
        {
            OnEasyEvent = null;
        }
    }

    public class EasyEvent<T, K, Q> : IEasyEventsystem
    {
        Action<T, K,Q> OnEasyEvent;

        public IUnRegister RegisterEvent(Action<T, K,Q> onEvent)
        {
            OnEasyEvent += onEvent;
            return this;
        }

        public void EventTrigger(T t, K k,Q q)
            => OnEasyEvent?.Invoke(t, k, q);

        public void UnRegister(Action<T, K, Q> onEvent)
        {
             OnEasyEvent -= onEvent;
        }

        public void UnRegisterAllEvent()
        {
            OnEasyEvent = null;
        }
    }

    public class EasyEvent<T, K, Q, P> : IEasyEventsystem
    {
        Action<T, K, Q, P> OnEasyEvent;

        public IUnRegister RegisterEvent(Action<T, K, Q, P> onEvent)
        {
            OnEasyEvent += onEvent;
            return this;
        }

        public void EventTrigger(T t, K k, Q q,P p)
            => OnEasyEvent?.Invoke(t, k, q,p);

        public void UnRegister(Action<T, K, Q,P> onEvent)
        {
            OnEasyEvent -= onEvent;
        }

        public void UnRegisterAllEvent()
        {
            OnEasyEvent = null;
        }
    }

    public static class BindablePropertyOrEventExtension
    {

        /// <summary>
        /// 注销事件，并且绑定MonoBehaviour生命周期,当销毁的时自动注销事件
        /// </summary>
        /// <param name="gameObject">GameObject</param>
        public static void UnRegisterWaitGameObjectDestroy<Component>(this IUnRegister property, Component component, Action callBack = null) where Component : UnityEngine.Component
        {
            if (!component.TryGetComponent(out OnGameObjectTrigger objectTrigger))
            {
                objectTrigger = component.gameObject.AddComponent<OnGameObjectTrigger>();
            }
            callBack += () => property.UnRegisterAllEvent();
            objectTrigger.PushFinishEvent(callBack);

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

        public static void UnRegisterModel<T>(this IUnRegisterModelOrView unRegister, T model) where T : class, IModel
            => unRegister.GetArchitecture().UnRegisterModel(model);

        public static void UnRegisterView<T>(this IUnRegisterModelOrView unRegister, T view) where T : class, IView
            => unRegister.GetArchitecture().UnRegisterView(view);

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

        public static IAsyncExtensionCore Send_AsyncCommand<T>(this IGetCommandCenter CommandCenter, T command, IObjectContainer container = null, object data = null) where T : ICommand
            =>  CommandCenter.GetArchitecture().CommandCenter.Send_AsyncCommand<T>(command, container, data).Start();

        public static IAsyncExtensionCore Send_AsyncCommand<T>(this IGetCommandCenter CommandCenter, IObjectContainer container = null, object data = null) where T : ICommand,new()
            => CommandCenter.GetArchitecture().CommandCenter.Send_AsyncCommand<T>(container, data).Start();
    }

    /// <summary>
    /// UI框架拓展
    /// </summary>
    public static class UIFrameWorkExtension
    {
        /// <summary>
        /// 初始化UI框架(仅在任一控制器下调用一次即可，无需在其他控制器重复调用)
        /// panelPath: ui面板所在的文件夹路径(通过ResKit资源配置加载，使用前需要提前配置)
        /// </summary>
        /// <param name="controller">控制器</param>
        /// <param name="panelPath">ui面板所在的文件夹路径(通过ResKit资源配置加载，使用前需要提前配置)</param>
        public static void UIFrameWorkInit(this IUIPanelController controller,string panelPath)
            => controller.GetArchitecture().InitPanelManager(panelPath);

        public static IAsyncExtensionCore UIFrameWorkInitAsync(this IUIPanelController controller, string panelPath, Action onFinish = null)
            => controller.GetArchitecture().InitPanelManagerAsync(panelPath, onFinish);

        /// <summary>
        /// 面板入栈(注意：使用UI控制器前都需要在控制器初始化一次)
        /// </summary>
        /// <typeparam name="TPanel">面板类型</typeparam>
        /// <param name="controller">面板控制器</param>
        /// <param name="type">层级类型</param>
        /// <returns>返回一个面板</returns>
        public static TPanel PushPanel<TPanel>(this IUIPanelController controller, UIPanelType type) where TPanel : BasePanel
        {
            try
            {
                var panel = controller.GetArchitecture().PanelManager.PushPanel<TPanel>(type);
                panel.SetArchitecture(controller.GetArchitecture());
                return panel;
            }
            catch
            {
                throw new Exception("没有对PanelManager进行初始化，在控制器类调用UIPanelInit方法");
            }
        }

        /// <summary>
        /// 面板出栈(注意：使用UI控制器前都需要在控制器初始化一次)
        /// </summary>
        /// <typeparam name="TPanel">面板类型</typeparam>
        /// <param name="controller">面板控制器</param>
        /// <param name="type">层级类型</param>
        public static void PopPanel(this IUIPanelController controller,UIPanelType type, bool isDestroy = false)
            => controller.GetArchitecture().PanelManager.PopPanel(type, isDestroy);

        /// <summary>
        /// 子面板入栈(该分支由面板的父级面板管理)(注意：使用UI控制器前都需要在控制器初始化一次)
        /// </summary>
        /// <typeparam name="TPanel">子面板类型</typeparam>
        /// <param name="panel">父面板</param>      
        public static TPanel PushChildPanel<TPanel>(this BasePanel panel) where TPanel : BasePanel
        {
            Type type = panel.GetType();
            var childPanel = panel.GetArchitecture().PanelManager.PushChildPanel<TPanel>(type);                      
            childPanel.SetArchitecture(panel.GetArchitecture());
            return childPanel;
        }

        /// <summary>
        /// 子面板出栈(注意：使用UI控制器前都需要在控制器初始化一次)
        /// </summary>
        /// <typeparam name="TParent">父面板的类型</typeparam>
        /// <param name="panel">操作使用的面板</param>
        /// <param name="isDestroy">是否在弹出时销毁面板</param>
        public static void PopChildPanel<TParent>(this BasePanel panel, bool isDestroy = false)
            => panel.GetArchitecture().PanelManager.PopChildPanel<TParent>(isDestroy);
    }
}
