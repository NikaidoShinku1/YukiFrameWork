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
using YukiFrameWork.Extension;
using YukiFrameWork.Pools;

namespace YukiFrameWork
{
    /// <summary>
    /// 框架体系结构
    /// </summary>
    public interface IArchitecture
    {
        void OnInit();
        void OnDestroy();
        void RegisterModel<T>(T model) where T : class, IModel;      
        void RegisterSystem<T>(T system) where T : class,ISystem;
        void RegisterUtility<T>(T utility) where T : class, IUtility;
        void UnRegisterModel<T>(T model = default) where T : class,IModel;      
        void UnRegisterSystem<T>(T view = default) where T : class,ISystem;
        void UnRegisterUtility<T>(T utility = default) where T : class,IUtility;
        T GetModel<T>() where T : class, IModel;      
        T GetSystem<T>() where T : class, ISystem;
        T GetUtility<T>() where T : class,IUtility;
            
        IUnRegister RegisterEvent<T>(string eventName, Action<T> onEvent);
        IUnRegister RegisterEvent<T>(Enum eventEnum, Action<T> onEvent);
        IUnRegister RegisterEvent<T>(Action<T> onEvent);
        void SendEvent<T>(T t = default);
        void SendEvent<T>(string eventName, T t = default);
        void SendEvent<T>(Enum eventEnum, T t = default);
        void UnRegisterEvent<T>(Action<T> onEvent);
        void UnRegisterEvent<T>(Enum eventEnum, Action<T> onEvent);
        void UnRegisterEvent<T>(string eventName, Action<T> onEvent);       
        void SendCommand<T>(T command) where T : ICommand;
        TResult SendCommand<TResult>(ICommand<TResult> command);       
    }

    #region 层级规则
    public interface IGetModel : IGetArchitecture
    {

    }

    public interface ISendEvent : IGetArchitecture
    {

    }

    public interface IGetRegisterEvent : IGetArchitecture
    {

    }

    public interface IGetSystem : IGetArchitecture
    {
        
    }

    public interface ISendCommand : IGetArchitecture
    {
        
    }

    public interface IGetUtility : IGetArchitecture
    {
        
    }
    #endregion

    #region Controller
    public interface IController :
        ISendCommand, IGetArchitecture, IGetModel, IGetUtility,
        IGetRegisterEvent,IGetSystem
    {

    }
    #endregion

    #region Model
    public interface IModel : ISetArchitecture, ISendEvent , IGetUtility, IGetArchitecture
    {               
        void Init();        
    }
    #endregion

    #region System
    public interface ISystem : IGetRegisterEvent,IGetUtility,ISendEvent,IGetModel,IGetSystem,IGetArchitecture,ISetArchitecture
    {
        void Init();
    }
    #endregion

    #region Utility
    public interface IUtility
    {
        
    }
    #endregion

    #region Command
    public interface ICommand : ISetArchitecture,ISendEvent,IGetRegisterEvent,IGetModel
        ,IGetUtility,IGetSystem,ISendCommand,IGetArchitecture
    {       
        void Execute();        
    }

    public interface ICommand<TResult> : ISetArchitecture, ISendEvent, IGetRegisterEvent, IGetModel
        , IGetUtility, IGetSystem, ISendCommand, IGetArchitecture
    {
        TResult Execute();
    }

    #endregion
    public interface IGetArchitecture
    {
        IArchitecture GetArchitecture();
    }

    public interface ISetArchitecture
    {
        void SetArchitecture(IArchitecture architecture);
    }

    [ClassAPI("框架核心本体")]
    [GUIDancePath("YukiFrameWork")]
    public abstract class Architecture<TCore> : IArchitecture,IDisposable where TCore : class,IArchitecture ,new()
    {
        #region Data
        private EasyContainer architectureContainer = new EasyContainer();
        private TypeEventSystem eventSystem = new TypeEventSystem();
        private EnumEventSystem enumEventSystem = new EnumEventSystem();
        private StringEventSystem stringEventSystem = new StringEventSystem();
        #endregion

        ~Architecture()
        {
            OnDestroy();
        }
      
        public abstract void OnInit();       

        public virtual void OnDestroy()
        {
            architectureContainer.Clear();
            eventSystem.Clear();
            enumEventSystem.Clear();
            stringEventSystem.Clear();
        }
        
        #region 架构全局模块，如果想要让架构设置为全局的可以使用这个
        private static TCore mGlobal = null;
        private readonly static object _object = new object();

        //内部访问对象直接定位全局 
        [SerializationArchitecture]
        public static TCore Global
        {
            get
            {
                lock (_object)
                {
                    if (mGlobal == null)
                    {
                        mGlobal = ArchitectureConstructor.I.GetOrAddArchitecture<TCore>() as TCore;                     
                    }
                    return mGlobal;
                }
            }
        }
        #endregion     

        public void RegisterModel<T>(T model) where T : class, IModel
        {
            if (Register(model))
            {
                model.SetArchitecture(this);
                model.Init();
            }
        }          

        public void RegisterSystem<T>(T system) where T : class,ISystem
        {
            if (Register(system))
            {
                system.SetArchitecture(this);
                system.Init();
            }
        }

        public void RegisterUtility<T>(T utility) where T : class, IUtility
            => Register(utility);
          
        private bool Register<T>(T t) where T : class
        {
            return architectureContainer.Register<T>(t);
        }

        private bool Contains<T>(T t = default) where T : class      
            => architectureContainer.Contains(t.GetType());
        
        public T GetModel<T>() where T : class,IModel
            => architectureContainer.Get<T>();

        public T GetSystem<T>() where T : class,ISystem
           => architectureContainer.Get<T>();

        public T GetUtility<T>() where T : class,IUtility
            => architectureContainer.Get<T>();      

        public void UnRegisterModel<T>(T model = default) where T : class,IModel
        {
            if (Contains(model))           
                architectureContainer.Remove(model.GetType());            
        }      

        public void UnRegisterSystem<T>(T system = default) where T :class, ISystem
        {
            if (Contains(system))
                architectureContainer.Remove(system.GetType());
        }

        public void UnRegisterUtility<T>(T utility = default) where T : class,IUtility
        {
            if(Contains(utility))
                architectureContainer.Remove(utility.GetType());
        }

        void IDisposable.Dispose() => OnDestroy();

        public IUnRegister RegisterEvent<T>(string eventName, Action<T> onEvent)      
            => stringEventSystem.Register(eventName, onEvent);

        public IUnRegister RegisterEvent<T>(Enum eventEnum, Action<T> onEvent)
            => enumEventSystem.Register(eventEnum, onEvent);

        public IUnRegister RegisterEvent<T>(Action<T> onEvent) 
            => eventSystem.Register(onEvent);

        void IArchitecture.SendEvent<T>(string eventName, T t)
            => stringEventSystem.Send(eventName, t);

        void IArchitecture.SendEvent<T>(Enum eventEnum, T t)
            => enumEventSystem.Send(eventEnum, t);

        public void UnRegisterEvent<T>(Enum eventEnum, Action<T> onEvent)
            => enumEventSystem.UnRegister(eventEnum, onEvent);

        public void UnRegisterEvent<T>(string eventName, Action<T> onEvent)
            => stringEventSystem.UnRegister(eventName, onEvent);

        void IArchitecture.SendEvent<T>(T arg) => eventSystem.Send(arg);

        public void UnRegisterEvent<T>(Action<T> onEvent = null)
            => eventSystem.UnRegister(onEvent);

        public virtual void SendCommand<T>(T command) where T : ICommand => ExecuteCommand(command);

        public virtual TResult SendCommand<TResult>(ICommand<TResult> command) => ExecuteCommand(command);         

        protected void ExecuteCommand<T>(T command) where T : ICommand
        {
            command.SetArchitecture(this);
            command.Execute();
        }

        protected TResult ExecuteCommand<TResult>(ICommand<TResult> command)
        {
            command.SetArchitecture(this);
            return command.Execute();         
        }        
    }

    public sealed class EasyContainer
    {
        private Dictionary<Type, object> architectureContainer = new Dictionary<Type, object>();

        public bool Register<T>(T obj)
        {
            if (Contains(typeof(T))) return false;

            architectureContainer.Add(typeof(T), obj);
            return true;
        }

        public T Get<T>() where T : class 
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

        public void Remove(Type type)
        {
            architectureContainer.Remove(type);
        }

        public void Clear() => architectureContainer.Clear();

        public bool Contains(Type type) => architectureContainer.ContainsKey(type);
    }

    public class EnumEventSystem
    {
        private readonly EnumEasyEvents events = new EnumEasyEvents();
        public static EnumEventSystem Global { get; } = new EnumEventSystem();

        public IUnRegister Register(Enum type, Action onEvent)
           => events.GetOrAddEvent<EasyEvent>(type).RegisterEvent(onEvent);

        public IUnRegister Register<T>(Enum type, Action<T> onEvent)
            => events.GetOrAddEvent<EasyEvent<T>>(type).RegisterEvent(onEvent);

        public IUnRegister Register<T, K>(Enum type, Action<T, K> onEvent)
            => events.GetOrAddEvent<EasyEvent<T, K>>(type).RegisterEvent(onEvent);

        public IUnRegister Register<T, K, Q>(Enum type, Action<T, K, Q> onEvent)
            => events.GetOrAddEvent<EasyEvent<T, K, Q>>(type).RegisterEvent(onEvent);

        public IUnRegister Register<T, K, Q, P>(Enum type, Action<T, K, Q, P> onEvent)
            => events.GetOrAddEvent<EasyEvent<T, K, Q, P>>(type).RegisterEvent(onEvent);

        public void Send(Enum type)
            => events.GetEvent<EasyEvent>(type)?.SendEvent();

        public void Send<T>(Enum type, T t)
            => events.GetEvent<EasyEvent<T>>(type)?.SendEvent(t);

        public void Send<T, K>(Enum type, T t, K k)
            => events.GetEvent<EasyEvent<T, K>>(type)?.SendEvent(t, k);

        public void Send<T, K, Q>(Enum type, T t, K k, Q q)
            => events.GetEvent<EasyEvent<T, K, Q>>(type)?.SendEvent(t, k, q);

        public void Send<T, K, Q, P>(Enum type, T t, K k, Q q, P p)
            => events.GetEvent<EasyEvent<T, K, Q, P>>(type)?.SendEvent(t, k, q, p);

        public void UnRegister(Enum type, Action onEvent)
            => events.GetEvent<EasyEvent>(type)?.UnRegister(onEvent);

        public void UnRegister<T>(Enum type, Action<T> onEvent)
            => events.GetEvent<EasyEvent<T>>(type)?.UnRegister(onEvent);

        public void UnRegister<T, K>(Enum type, Action<T, K> onEvent)
            => events.GetEvent<EasyEvent<T, K>>(type)?.UnRegister(onEvent);

        public void UnRegister<T, K, Q>(Enum type, Action<T, K, Q> onEvent)
            => events.GetEvent<EasyEvent<T, K, Q>>(type)?.UnRegister(onEvent);

        public void UnRegister<T, K, Q, P>(Enum type, Action<T, K, Q, P> onEvent)
            => events.GetEvent<EasyEvent<T, K, Q, P>>(type)?.UnRegister(onEvent);

        public void Clear() => events.ClearEvent();
    }

    public class StringEventSystem
    {
        private readonly StringEasyEvents events = new StringEasyEvents();

        public static StringEventSystem Global { get; } = new StringEventSystem();

        public IUnRegister Register(string name,Action onEvent)
           => events.GetOrAddEvent<EasyEvent>(name).RegisterEvent(onEvent);

        public IUnRegister Register<T>(string name, Action<T> onEvent)
            => events.GetOrAddEvent<EasyEvent<T>>(name).RegisterEvent(onEvent);

        public IUnRegister Register<T, K>(string name, Action<T, K> onEvent)
            => events.GetOrAddEvent<EasyEvent<T, K>>(name).RegisterEvent(onEvent);

        public IUnRegister Register<T, K, Q>(string name, Action<T, K, Q> onEvent)
            => events.GetOrAddEvent<EasyEvent<T, K, Q>>(name).RegisterEvent(onEvent);

        public IUnRegister Register<T, K, Q, P>(string name, Action<T, K, Q, P> onEvent)
            => events.GetOrAddEvent<EasyEvent<T, K, Q, P>>(name).RegisterEvent(onEvent);

        public void Send(string name)
            => events.GetEvent<EasyEvent>(name)?.SendEvent();

        public void Send<T>(string name, T t)
            => events.GetEvent<EasyEvent<T>>(name)?.SendEvent(t);

        public void Send<T, K>(string name, T t, K k)
            => events.GetEvent<EasyEvent<T, K>>(name)?.SendEvent(t, k);

        public void Send<T, K, Q>(string name, T t, K k, Q q)
            => events.GetEvent<EasyEvent<T, K, Q>>(name)?.SendEvent(t, k, q);

        public void Send<T, K, Q, P>(string name, T t, K k, Q q, P p)
            => events.GetEvent<EasyEvent<T, K, Q, P>>(name)?.SendEvent(t, k, q, p);

        public void UnRegister(string name, Action onEvent)
            => events.GetEvent<EasyEvent>(name)?.UnRegister(onEvent);

        public void UnRegister<T>(string name, Action<T> onEvent)
            => events.GetEvent<EasyEvent<T>>(name)?.UnRegister(onEvent);

        public void UnRegister<T, K>(string name, Action<T, K> onEvent)
            => events.GetEvent<EasyEvent<T, K>>(name)?.UnRegister(onEvent);

        public void UnRegister<T, K, Q>(string name, Action<T, K, Q> onEvent)
            => events.GetEvent<EasyEvent<T, K, Q>>(name)?.UnRegister(onEvent);

        public void UnRegister<T, K, Q, P>(string name, Action<T, K, Q, P> onEvent)
            => events.GetEvent<EasyEvent<T, K, Q, P>>(name)?.UnRegister(onEvent);

        public void Clear() => events.ClearEvent();
    }

    public class TypeEventSystem
    {
        private readonly EasyEvents events = new EasyEvents();

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

        public void Send()
            => events.GetEvent<EasyEvent>()?.SendEvent();                
       
        public void Send<T>(T t)
            => events.GetEvent<EasyEvent<T>>()?.SendEvent(t);

        public void Send<T, K>(T t, K k)
            => events.GetEvent<EasyEvent<T, K>>()?.SendEvent(t, k);

        public void Send<T, K, Q>(T t, K k, Q q)
            => events.GetEvent<EasyEvent<T, K, Q>>()?.SendEvent(t, k, q);

        public void Send<T, K, Q, P>(T t, K k, Q q, P p)
            => events.GetEvent<EasyEvent<T, K, Q,P>>()?.SendEvent(t, k, q,p);

        public void UnRegister(Action onEvent)
            => events.GetEvent<EasyEvent>()?.UnRegister(onEvent);

        public void UnRegister<T>(Action<T> onEvent)
            => events.GetEvent<EasyEvent<T>>()?.UnRegister(onEvent);

        public void UnRegister<T,K>(Action<T,K> onEvent)
            => events.GetEvent<EasyEvent<T,K>>()?.UnRegister(onEvent);

        public void UnRegister<T, K, Q>(Action<T, K,Q> onEvent)
            => events.GetEvent<EasyEvent<T, K, Q>>()?.UnRegister(onEvent); 

        public void UnRegister<T, K, Q, P>(Action<T, K, Q,P > onEvent)
            => events.GetEvent<EasyEvent<T, K, Q, P>>()?.UnRegister(onEvent);

        public void Clear() => events.ClearEvent();
    }

    public class EasyEvents
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

        public T GetEvent<T>() where T : IEasyEventSystem
        {
            events.TryGetValue(typeof(T), out var eventSystem);          
            return (T)eventSystem;
        }

        public void AddEvent<T>() where T : IEasyEventSystem, new() => events.Add(typeof(T), new T());

        public void ClearEvent()
        {
            events.Clear();
        }
    }

    public class StringEasyEvents
    {
        private readonly Dictionary<string, IEasyEventSystem> events = new Dictionary<string, IEasyEventSystem>();
        public T GetOrAddEvent<T>(string name) where T : IEasyEventSystem, new()
        {
            if (!events.TryGetValue(name, out var easyEvent))
            {             
                easyEvent = new T();
                events.Add(name, easyEvent);
            }
            return (T)easyEvent;
        }

        public T GetEvent<T>(string name) where T : IEasyEventSystem
        {
            events.TryGetValue(name, out var eventSystem);
            try
            {
                return (T)eventSystem;
            }
            catch
            {
                throw new InvalidCastException($"事件类型有误！当前事件标识是{name},返回错误的类型是{typeof(T)}");
            }        
        }

        public void AddEvent<T>(string name) where T : IEasyEventSystem, new() => events.Add(name, new T());

        public void ClearEvent()
        {
            events.Clear();
        }
    }

    public class EnumEasyEvents
    {
        private readonly Dictionary<Enum, IEasyEventSystem> events = new Dictionary<Enum, IEasyEventSystem>();

        public T GetOrAddEvent<T>(Enum type) where T : IEasyEventSystem, new()
        {
            if (!events.TryGetValue(type, out var easyEvent))
            {
                easyEvent = new T();
                events.Add(type, easyEvent);
            }
            return (T)easyEvent;
        }

        public T GetEvent<T>(Enum type) where T : IEasyEventSystem
        {
            events.TryGetValue(type, out var eventSystem);
            try
            {
                return (T)eventSystem;
            }
            catch
            {
                throw new InvalidCastException($"事件类型有误！当前事件标识是{type},返回错误的类型是{typeof(T)}");
            }
        }

        public void AddEvent<T>(Enum type) where T : IEasyEventSystem, new() => events.Add(type, new T());

        public void ClearEvent()
        {
            events.Clear();
        }
    }

    public class EasyEvent : IEasyEventSystem
    {
        Action OnEasyEvent;

        public IUnRegister RegisterEvent(Action onEvent)
        {
            OnEasyEvent += onEvent;
            return this;
        }

        public void SendEvent()
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

    public class EasyEvent<T> : IEasyEventSystem
    {
        Action<T> OnEasyEvent;     

        public IUnRegister RegisterEvent(Action<T> onEvent)
        {
            OnEasyEvent += onEvent;
            return this;
        }

        public void SendEvent(T t)
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

    public class EasyEvent<T, K> : IEasyEventSystem
    {
        Action<T,K> OnEasyEvent;

        public IUnRegister RegisterEvent(Action<T, K> onEvent)
        {
            OnEasyEvent += onEvent;           
            return this;
        }

        public void SendEvent(T t, K k)
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

    public class EasyEvent<T, K, Q> : IEasyEventSystem
    {
        Action<T, K, Q> OnEasyEvent;

        public IUnRegister RegisterEvent(Action<T, K,Q> onEvent)
        {
            OnEasyEvent += onEvent;
            return this;
        }

        public void SendEvent(T t, K k,Q q)
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

    public class EasyEvent<T, K, Q, P> : IEasyEventSystem
    {
        Action<T, K, Q, P> OnEasyEvent;

        public IUnRegister RegisterEvent(Action<T, K, Q, P> onEvent)
        {
            OnEasyEvent += onEvent;
            return this;
        }

        public void SendEvent(T t, K k, Q q,P p)
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
        /// 注销事件，并且绑定MonoBehaviour生命周期,当销毁的时自动清空事件
        /// </summary>
        /// <param name="gameObject">GameObject</param>
        public static void UnRegisterWaitGameObjectDestroy<Component>(this IUnRegister property, Component component,Action onFinish = null) where Component : UnityEngine.Component
        {
            if (!component.TryGetComponent(out OnGameObjectTrigger objectSend))
            {
                objectSend = component.gameObject.AddComponent<OnGameObjectTrigger>();
            }
            objectSend.AddUnRegister(property);

            objectSend.PushFinishEvent(onFinish);

        }
    }

    /// <summary>
    /// 控制器拓展
    /// </summary>
    public static class ControllerExtension
    {           
        public static T GetModel<T>(this IGetModel actor) where T : class, IModel
            => actor.GetArchitecture().GetModel<T>();

        public static T GetSystem<T>(this IGetSystem actor) where T : class, ISystem
            => actor.GetArchitecture().GetSystem<T>();

        public static T GetUtility<T>(this IGetUtility actor) where T : class, IUtility
            => actor.GetArchitecture().GetUtility<T>();

        public static void OnDestroy(this IGetArchitecture architecture)
            => architecture.GetArchitecture().OnDestroy();

        
    }

    /// <summary>
    /// 事件系统拓展
    /// </summary>
    public static class EventSystemExtension
    {
        public static IUnRegister RegisterEvent<T>(this IGetRegisterEvent registerEvent, Action<T> onEvent)
            => registerEvent.GetArchitecture().RegisterEvent(onEvent);

        public static IUnRegister RegisterEvent<T>(this IGetRegisterEvent registerEvent, string eventName, Action<T> onEvent)
            => registerEvent.GetArchitecture().RegisterEvent(eventName, onEvent);

        public static IUnRegister RegisterEvent<T>(this IGetRegisterEvent registerEvent, Enum eventEnum, Action<T> onEvent)
           => registerEvent.GetArchitecture().RegisterEvent(eventEnum, onEvent);

        public static void UnRegisterEvent<T>(this IGetRegisterEvent registerEvent, Action<T> onEvent = null)
            => registerEvent.GetArchitecture().UnRegisterEvent(onEvent);

        public static void UnRegisterEvent<T>(this IGetRegisterEvent registerEvent, string eventName, Action<T> onEvent)
          => registerEvent.GetArchitecture().UnRegisterEvent(eventName, onEvent);

        public static void UnRegisterEvent<T>(this IGetRegisterEvent registerEvent, Enum eventEnum, Action<T> onEvent)
        => registerEvent.GetArchitecture().UnRegisterEvent(eventEnum, onEvent);

        public static void SendEvent<T>(this ISendEvent SendEvent,T arg = default)
            => SendEvent.GetArchitecture().SendEvent(arg);

        public static void SendEvent<T>(this ISendEvent SendEvent,string eventName, T arg = default)
           => SendEvent.GetArchitecture().SendEvent(eventName,arg);

        public static void SendEvent<T>(this ISendEvent SendEvent, Enum eventEnum, T arg = default)
         => SendEvent.GetArchitecture().SendEvent(eventEnum, arg);
    }

    /// <summary>
    /// 命令中心拓展
    /// </summary>
    public static class CommandCenterExtension
    {
        public static void SendCommand<T>(this ISendCommand center) where T : ICommand, new()
        {
            T command = new T();
            center.GetArchitecture().SendCommand(command);
        }

        public static void SendCommand<T>(this ISendCommand center, T command) where T : ICommand
            => center.GetArchitecture().SendCommand<T>(command);

        public static TResult SendCommand<TResult>(this ISendCommand center, ICommand<TResult> command)
            => center.GetArchitecture().SendCommand(command);     
    }
}
