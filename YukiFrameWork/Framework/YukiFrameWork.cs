///=====================================================
///=====================================================
/// - FileName:      YukiFrameWork.cs
/// - NameSpace:     YukiFrameWork
/// - Created:       Yuki
/// - Email:         Yuki@qq.com
/// - Description: 
///>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>  
///	框架核心架构 基于QFrameWork拓展衍生的架构本体 框架gitee仓库链接:https://gitee.com/NikaidoShinku/YukiFrameWork.git
///	QFrameWork作者：凉鞋 QFrameWork  QFrameWork gitee仓库链接:https://gitee.com/liangxiegame/QFramework.git
///>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================

using System.Collections.Generic;
using System;
using YukiFrameWork.Extension;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;

namespace YukiFrameWork
{

    public enum SceneLoadType
    {
        Local,
        XFABManager
    } 
    /// <summary>
    /// 框架体系结构
    /// </summary>
    public interface IArchitecture : IDisposable
    {
        /// <summary>
        /// 使用XFABManager加载时，可以进行重写的模块名用于对应XFABManager的配置模块。
        /// <para>外部使用静态的ProjectName属性进行访问即可,例如World.ProjectName,World为架构</para>
        /// </summary>
        string OnProjectName { get; }
        /// <summary>
        /// 使用XFABManager打包场景进ab包或者已经有场景直接添加在Buil时，可以重写该方法，输入场景名称以及加载方式，当架构模块完全加载完以后会自动进入场景
        /// 
        /// 
        /// <para> 注意:使用XFABManager加载场景必须重写OnProjectName属性 </para>
        /// </summary>
        (string, SceneLoadType) DefaultSceneName { get; }

        void Init();
        void OnDestroy();
        [Obsolete("建议在Model类上方标记Registration特性进行注册自动化，而习惯放弃手动在架构时初始化")]
        void RegisterModel<T>(T model) where T : class, IModel;
        [Obsolete("建议在System类上方标记Registration特性进行注册自动化，而习惯放弃手动在架构时初始化")]
        void RegisterSystem<T>(T system) where T : class,ISystem;
        [Obsolete("建议在Utility类上方标记Registration特性进行注册自动化，而习惯放弃手动在架构时初始化")]
        void RegisterUtility<T>(T utility) where T : class, IUtility;
        void UnRegisterModel<T>(T model = default) where T : class,IModel;      
        void UnRegisterSystem<T>(T view = default) where T : class,ISystem;
        void UnRegisterUtility<T>(T utility = default) where T : class,IUtility;
        T GetModel<T>() where T : class, IModel;      
        T GetSystem<T>() where T : class, ISystem;
        T GetUtility<T>() where T : class,IUtility;

        ArchitectureTableConfig TableConfig { get; }

        #region Event
        IUnRegister RegisterEvent<T>(string eventName, Action<T> onEvent);
        IUnRegister RegisterEvent<TEnum,T>(TEnum eventEnum, Action<T> onEvent) where TEnum : IConvertible;
        IUnRegister RegisterEvent<T>(Action<T> onEvent);
        IUnRegister RegisterEvent_Async<T>(Func<T, Task> onEvent);
        IUnRegister RegisterEvent_Async<T>(string name,Func<T, Task> onEvent);
        IUnRegister RegisterEvent_Async<TEnum, T>(TEnum eventEnum, Func<T, Task> onEvent) where TEnum : IConvertible;
#if UNITY_2021_1_OR_NEWER
        IUnRegister RegisterEvent_Async_Unity<T>(Func<T, YieldTask> onEvent);
        IUnRegister RegisterEvent_Async_Unity<T>(string name, Func<T, YieldTask> onEvent);
        IUnRegister RegisterEvent_Async_Unity<TEnum, T>(TEnum eventEnum, Func<T, YieldTask> onEvent) where TEnum : IConvertible;
#endif
        void SendEvent<T>(T t = default);
        void SendEvent<T>(string eventName, T t = default);
        void SendEvent<TEnum,T>(TEnum eventEnum, T t = default) where TEnum : IConvertible;
        Task SendEvent_Async<T>(T t = default);
        Task SendEvent_Async<T>(string name,T t = default);
        Task SendEvent_Async<TEnum, T>(TEnum eventEnum,T t = default) where TEnum : IConvertible;
#if UNITY_2021_1_OR_NEWER
        YieldTask SendEvent_Async_Unity<T>(T t = default);
        YieldTask SendEvent_Async_Unity<T>(string name,T t = default);
        YieldTask SendEvent_Async_Unity<TEnum, T>(TEnum eventEnum, T t = default) where TEnum : IConvertible;
#endif
        void UnRegisterEvent<T>(Action<T> onEvent);
        void UnRegisterEvent<T>(string eventName, Action<T> onEvent);
        void UnRegisterEvent<TEnum, T>(TEnum eventEnum, Action<T> onEvent) where TEnum : IConvertible;
        void UnRegisterEvent_Async<T>(Func<T, Task> onEvent);
        void UnRegisterEvent_Async<T>(string name, Func<T, Task> onEvent);
        void UnRegisterEvent_Async<TEnum, T>(TEnum eventEnum, Func<T, Task> onEvent) where TEnum : IConvertible;
#if UNITY_2021_1_OR_NEWER
        void UnRegisterEvent_Async_Unity<T>(Func<T, YieldTask> onEvent);
        void UnRegisterEvent_Async_Unity<T>(string name, Func<T, YieldTask> onEvent);
        void UnRegisterEvent_Async_Unity<TEnum, T>(TEnum eventEnum, Func<T, YieldTask> onEvent) where TEnum : IConvertible;
#endif
        #endregion
        void SendCommand<T>(T command) where T : ICommand;
        TResult SendCommand<TResult>(ICommand<TResult> command);
        TResult SendQuery<TResult>(IQuery<TResult> query);
        #region Framework Internal Gettter
        internal EasyContainer Container { get; }  
        
        internal TypeEventSystem TypeEventSystem { get; }
        internal EnumEventSystem EnumEventSystem { get; }
        internal StringEventSystem StringEventSystem { get; }
        internal AsyncTypeEventSystem AsyncTypeEventSystem { get; }
        internal AsyncEnumEventSystem AsyncEnumEventSystem { get; }
        internal AsyncStringEventSystem AsyncStringEventSystem { get; }      
        internal SyncDynamicEventSystem SyncDynamicEventSystem { get; }

#if UNITY_2021_1_OR_NEWER
        internal Unity_AsyncTypeEventSystem AsyncTypeEventSystem_Unity { get; }
        internal Unity_AsyncEnumEventSystem AsyncEnumEventSystem_Unity { get; }
        internal Unity_AsyncStringEventSystem AsyncStringEventSystem_Unity { get; }
#endif
        #endregion
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

    public interface IGetQuery : IGetArchitecture
    {
        
    }

    public interface IGetConfig : IGetArchitecture
    {
        
    }

    #endregion

    public interface IDestroy
    {
        void Destroy();
    }

    #region Controller
    public interface IController :
        ISendCommand, IGetArchitecture, IGetModel, IGetUtility,
        IGetRegisterEvent,IGetSystem,IGetQuery
    {

    }
    #endregion

    #region Model
    public interface IModel : ISetArchitecture, ISendEvent , IGetUtility, IGetArchitecture,IDestroy,IGetConfig
    {                      
        /// <summary>
        /// 是否自动注入字段开启，开启后可以使用ConfigSerializeFieldAttribute特性进行自动配表注入。
        /// </summary>      
        void Init();        
    }
    #endregion


    #region System
    public interface ISystem : IGetRegisterEvent,IGetUtility,ISendEvent,IGetModel,IGetSystem,IGetArchitecture,ISetArchitecture,IDestroy
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
        ,IGetUtility,IGetSystem,ISendCommand,IGetArchitecture,IGetQuery
    {       
        void Execute();        
    }

    public interface ICommand<TResult> : ISetArchitecture, ISendEvent, IGetRegisterEvent, IGetModel
        , IGetUtility, IGetSystem, ISendCommand, IGetArchitecture,IGetQuery
    {
        TResult Execute();
    }

    #endregion

    #region Query
    public interface IQuery<TResult> : IGetArchitecture, ISetArchitecture, IGetModel, IGetSystem, IGetQuery
    {
        TResult Seek();
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
    public abstract partial class Architecture<TCore> : IArchitecture,IDisposable where TCore : class,IArchitecture ,new()
    {
        #region Data
        private EasyContainer easyContainer = new EasyContainer();
        private TypeEventSystem eventSystem = new TypeEventSystem();
        private EnumEventSystem enumEventSystem = new EnumEventSystem();
        private StringEventSystem stringEventSystem = new StringEventSystem();
        private AsyncTypeEventSystem asyncTypeEventSystem = new AsyncTypeEventSystem();
        private AsyncStringEventSystem asyncStringEventSystem = new AsyncStringEventSystem();
        private AsyncEnumEventSystem asyncEnumEventSystem = new AsyncEnumEventSystem();     
        private SyncDynamicEventSystem syncDynamicEventSystem = new SyncDynamicEventSystem();
        private ArchitectureTableConfig config;
#if UNITY_2021_1_OR_NEWER
        private Unity_AsyncTypeEventSystem asyncTypeEventSystem_Unity = new Unity_AsyncTypeEventSystem();
        private Unity_AsyncStringEventSystem asyncStringEventSystem_Unity = new Unity_AsyncStringEventSystem();
        private Unity_AsyncEnumEventSystem asyncEnumEventSystem_Unity = new Unity_AsyncEnumEventSystem();
#endif
#endregion

        internal bool IsInited = false;

        EasyContainer IArchitecture.Container => easyContainer;

        TypeEventSystem IArchitecture.TypeEventSystem => eventSystem;
        EnumEventSystem IArchitecture.EnumEventSystem => enumEventSystem;
        StringEventSystem IArchitecture.StringEventSystem => stringEventSystem;
#if UNITY_2021_1_OR_NEWER
        Unity_AsyncTypeEventSystem IArchitecture.AsyncTypeEventSystem_Unity => asyncTypeEventSystem_Unity;
        Unity_AsyncEnumEventSystem IArchitecture.AsyncEnumEventSystem_Unity { get; }
        Unity_AsyncStringEventSystem IArchitecture.AsyncStringEventSystem_Unity { get; }      
#endif
        SyncDynamicEventSystem IArchitecture.SyncDynamicEventSystem => syncDynamicEventSystem;
        AsyncTypeEventSystem IArchitecture.AsyncTypeEventSystem => asyncTypeEventSystem;
        AsyncEnumEventSystem IArchitecture.AsyncEnumEventSystem => asyncEnumEventSystem;
        AsyncStringEventSystem IArchitecture.AsyncStringEventSystem => asyncStringEventSystem;
        ArchitectureTableConfig IArchitecture.TableConfig
        {
            get
            {
                if (config == null)
                    throw new Exception("没有对配表的预加载进行配置，请在架构中重写BuildArchitectureTable方法,否则无法进行对配表的引用");
                return config;
            }
        }

        protected ArchitectureTableConfig TableConfig => (this as IArchitecture).TableConfig;

        void IArchitecture.Init()
        {
            if (IsInited) return;

            ArchitectureTable table = BuildArchitectureTable();

            if (table != null)
            {
                table.projectName = ProjectName;
                config = new ArchitectureTableConfig(table);                     
            }

            OnInit();
            IsInited = true;
        }

        /// <summary>
        /// V1.25.1以后，在架构准备完成，开始自动化注册各个模块之前就会调用架构的初始化方法
        /// </summary>
        public abstract void OnInit();

        public static ArchitectureStartUpRequest StartUp()
        {
            return ArchitectureStartUpRequest.StartUpArchitecture<TCore>();
        }

        public void OnDestroy()
        {
            easyContainer.Clear();
            eventSystem.Clear();
            enumEventSystem.Clear();
            stringEventSystem.Clear();
            IsInited = false;
        }     
        public virtual (string, SceneLoadType) DefaultSceneName => default;

       
        /// <summary>
        /// 构建架构配表器方法，如需让Model层可顺利访问配表层，必须要重写该方法进行配表器构建
        /// </summary>
        /// <returns></returns>
        protected virtual ArchitectureTable BuildArchitectureTable() => null;

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
        public virtual string OnProjectName => this.GetType().Name;
        public static string ProjectName => Global.OnProjectName;

        #endregion
        [Obsolete("建议在Model类上方标记Registration特性进行注册自动化，而习惯放弃手动在架构时初始化")]
        public void RegisterModel<T>(T model) where T : class, IModel
        {
            model.SetArchitecture(this);
            Register(model);                      
            model.Init();           
        }
        [Obsolete("建议在System类上方标记Registration特性进行注册自动化，而习惯放弃手动在架构时初始化")]
        public void RegisterSystem<T>(T system) where T : class,ISystem
        {
            system.SetArchitecture(this);
            Register(system);                     
            system.Init();
        }
        [Obsolete("建议在Utility类上方标记Registration特性进行注册自动化，而习惯放弃手动在架构时初始化")]
        public void RegisterUtility<T>(T utility) where T : class, IUtility
            => Register(utility);
          
        private void Register<T>(T t) where T : class
        {
            easyContainer.Register<T>(t);            
        }    
        
        public virtual T GetModel<T>() where T : class,IModel
            => easyContainer.Get<T>();

        public virtual T GetSystem<T>() where T : class,ISystem
           => easyContainer.Get<T>();

        public virtual T GetUtility<T>() where T : class,IUtility
            => easyContainer.Get<T>();
     
        public void UnRegisterModel<T>(T model = default) where T : class,IModel
        {                 
            easyContainer.Remove(typeof(T));            
        } 
   
        public void UnRegisterSystem<T>(T system = default) where T :class, ISystem
        {          
            easyContainer.Remove(typeof(T));
        }

        public void UnRegisterUtility<T>(T utility = default) where T : class,IUtility
        {         
            easyContainer.Remove(typeof(T));
        }

        void IDisposable.Dispose() => OnDestroy();

        public IUnRegister RegisterEvent<T>(string eventName, Action<T> onEvent)      
            => stringEventSystem.Register(eventName, onEvent);

        public IUnRegister RegisterEvent<TEnum,T>(TEnum eventEnum, Action<T> onEvent) where TEnum : IConvertible
            => enumEventSystem.Register(eventEnum, onEvent);

        public IUnRegister RegisterEvent<T>(Action<T> onEvent) 
            => eventSystem.Register(onEvent);

        void IArchitecture.SendEvent<T>(string eventName, T t)
        {
            if(t is IEventArgs arg)
                syncDynamicEventSystem.Send(eventName, arg);
            stringEventSystem.Send(eventName, t);
        }

        void IArchitecture.SendEvent<TEnum, T>(TEnum eventEnum, T t)
        {
            if (t is IEventArgs arg)
                syncDynamicEventSystem.Send(eventEnum, arg);
            enumEventSystem.Send(eventEnum, t);
        }

        public void UnRegisterEvent<TEnum, T>(TEnum eventEnum, Action<T> onEvent) where TEnum : IConvertible
        {
            syncDynamicEventSystem.UnRegister(typeof(TEnum), eventEnum.ToInt32(null), onEvent.Method);
            enumEventSystem.UnRegister(eventEnum, onEvent);
        }

        public void UnRegisterEvent<T>(string eventName, Action<T> onEvent)
        {
            syncDynamicEventSystem.UnRegister(eventName, onEvent.Method);
            stringEventSystem.UnRegister(eventName, onEvent);
        }

        void IArchitecture.SendEvent<T>(T arg)
        {
            if (arg is IEventArgs eventArgs)
                syncDynamicEventSystem.Send(typeof(T), eventArgs);
            eventSystem.Send(arg);
        }

        public void UnRegisterEvent<T>(Action<T> onEvent = null)
        {
            syncDynamicEventSystem.UnRegister(typeof(T),onEvent.Method);
            eventSystem.UnRegister(onEvent);
        }

        public void SendCommand<T>(T command) where T : ICommand => ExecuteCommand(command);

        public TResult SendCommand<TResult>(ICommand<TResult> command) => ExecuteCommand(command);

        public TResult SendQuery<TResult>(IQuery<TResult> query) => Query<TResult>(query);
       
        protected virtual TResult Query<TResult>(IQuery<TResult> query)
        {
            query.SetArchitecture(this);
            return query.Seek();
        }

        protected virtual void ExecuteCommand<T>(T command) where T : ICommand
        {
            command.SetArchitecture(this);
            command.Execute();
        }

        protected virtual TResult ExecuteCommand<TResult>(ICommand<TResult> command)
        {
            command.SetArchitecture(this);
            return command.Execute();         
        }

        public IUnRegister RegisterEvent_Async<T>(Func<T, Task> onEvent)
        {
            return asyncTypeEventSystem.Register(onEvent);
        }

        public IUnRegister RegisterEvent_Async<T>(string name, Func<T, Task> onEvent)
        {
            return asyncStringEventSystem.Register(name, onEvent);
        }

        public IUnRegister RegisterEvent_Async<TEnum, T>(TEnum eventEnum, Func<T, Task> onEvent) where TEnum : IConvertible
        {
            return asyncEnumEventSystem.Register(eventEnum, onEvent);
        }

#if UNITY_2021_1_OR_NEWER
        public IUnRegister RegisterEvent_Async_Unity<T>(Func<T, YieldTask> onEvent)
        {
            return asyncTypeEventSystem_Unity.Register(onEvent);
        }

        public IUnRegister RegisterEvent_Async_Unity<T>(string name, Func<T, YieldTask> onEvent)
        {
            return asyncStringEventSystem_Unity.Register(name, onEvent);
        }

        public IUnRegister RegisterEvent_Async_Unity<TEnum, T>(TEnum eventEnum, Func<T, YieldTask> onEvent) where TEnum : IConvertible
        {
            return asyncEnumEventSystem_Unity.Register(eventEnum, onEvent);
        }
#endif
        public async Task SendEvent_Async<T>(T t = default)
        {
            await asyncTypeEventSystem.Send(t);
        }

        public async Task SendEvent_Async<T>(string name, T t = default)
        {
            await asyncStringEventSystem.Send(name,t);
        }

        public async Task SendEvent_Async<TEnum, T>(TEnum eventEnum, T t = default) where TEnum : IConvertible
        {
            await asyncEnumEventSystem.Send(eventEnum, t);
        }



#if UNITY_2021_1_OR_NEWER
        public async YieldTask SendEvent_Async_Unity<T>(T t = default)
        {        
            await asyncTypeEventSystem_Unity.Send(t);
        }

        public async YieldTask SendEvent_Async_Unity<T>(string name, T t = default)
        {
            
            await asyncStringEventSystem_Unity.Send(name, t);
        }

        public async YieldTask SendEvent_Async_Unity<TEnum, T>(TEnum eventEnum, T t = default) where TEnum : IConvertible
        {
           
            await asyncEnumEventSystem_Unity.Send(eventEnum, t);
        }

#endif

        public void UnRegisterEvent_Async<T>(Func<T, Task> onEvent)
        {
            asyncTypeEventSystem.UnRegister<T>(onEvent);
        }

        public void UnRegisterEvent_Async<T>(string name, Func<T, Task> onEvent)
        {
            asyncStringEventSystem.UnRegister(name, onEvent);
        }

        public void UnRegisterEvent_Async<TEnum, T>(TEnum eventEnum, Func<T, Task> onEvent) where TEnum : IConvertible
        {
            asyncEnumEventSystem.UnRegister(eventEnum, onEvent);
        }

#if UNITY_2021_1_OR_NEWER
        public void UnRegisterEvent_Async_Unity<T>(Func<T, YieldTask> onEvent)
        {
            asyncTypeEventSystem_Unity.UnRegister(onEvent);
        }           
          
        public void UnRegisterEvent_Async_Unity<T>(string name, Func<T, YieldTask> onEvent)
        {
            asyncStringEventSystem_Unity.UnRegister(name, onEvent);
        }

        public void UnRegisterEvent_Async_Unity<TEnum, T>(TEnum eventEnum, Func<T, YieldTask> onEvent) where TEnum : IConvertible
        {
            asyncEnumEventSystem_Unity.UnRegister(eventEnum, onEvent);
        }
#endif
    }

    public sealed class EasyContainer
    {                      
       
        private Dictionary<Type, object> mInstances = new Dictionary<Type, object>(); 
      
        public void Register<T>(T obj)
        {
            Register(obj, typeof(T));
        }

        internal bool ContainsType(Type type)
        {
            return mInstances.ContainsKey(type);
        }

        internal bool ContainsInstance(Type type)
            => mInstances.Values.FirstOrDefault(instance => instance.GetType().Equals(type)) != null;

        internal void Register(object obj, Type type)
        {
            mInstances[type] = obj;
        }

        public T Get<T>() where T : class 
        {
            mInstances.TryGetValue(typeof(T), out var value);
            return value as T;
        }       

        public IEnumerable<T> GetInstanceByType<T>()
        {            
             return mInstances.Values.Where(x => typeof(T).IsInstanceOfType(x)).Cast<T>();         
        }

        public void Remove(Type type)
        {
#if UNITY_2021_1_OR_NEWER
            mInstances.Remove(type,out object instance);

            if (instance is IDestroy destroy)
                destroy.Destroy();
#else
            if (mInstances.TryGetValue(type, out object instance) && instance is IDestroy destroy)
                destroy.Destroy();
            mInstances.Remove(type);
#endif
        } 

        public void Clear()
        {
            FastList<IModel> models = new FastList<IModel>();
            FastList<ISystem> systems = new FastList<ISystem>();
            foreach (var instance in mInstances.Values)
            {
                if (instance is IModel model)
                    models.Add(model);
                else if (instance is ISystem system)
                    systems.Add(system);
            }

            for (int i = 0; i < models.Count; i++)
            {
                models[i].Destroy();
            }

            for (int i = 0; i < systems.Count; i++)
            {
                systems[i].Destroy();
            }
   
            models.Clear();
            systems.Clear();        
            mInstances.Clear();          
        }
       
    }

    public class DynamicEvents
    {
        private Dictionary<Type, IDynamicEvent> typeDynamics = new Dictionary<Type, IDynamicEvent>();
        private Dictionary<string, IDynamicEvent> stringDynamics = new Dictionary<string, IDynamicEvent>();
        private Dictionary<(int, Type), IDynamicEvent> enumDynamics = new Dictionary<(int, Type), IDynamicEvent>();

        public T GetOrAdd<T>(Type type) where T : IDynamicEvent,new()
        {
            if (!typeDynamics.TryGetValue(type, out IDynamicEvent ev))
            {
                ev = new T();
                typeDynamics.Add(type, ev);
            }

            return (T)ev;
        }     

        public T GetOrAdd<T>(string name) where T : IDynamicEvent,new()
        {
            if (!stringDynamics.TryGetValue(name, out IDynamicEvent ev))
            {
                ev = new T();
                stringDynamics.Add(name, ev);
            }

            return (T)ev;
        }    

        public T GetOrAdd<T>(int enumId,Type enumType) where T : IDynamicEvent,new()
        {
            var type = (enumId, enumType);
            if (!enumDynamics.TryGetValue(type, out IDynamicEvent ev))
            {
                ev = new T();
                enumDynamics.Add(type, ev);
            }

            return (T)ev;
        }      
    }

    internal abstract class DynamicEventSystem<T> where T : IDynamicEvent,new()
    {        
        internal readonly DynamicEvents events = new DynamicEvents();
        public IUnRegister Register(Type type,MethodInfo methodInfo,object target)
        {         
            return events.GetOrAdd<T>(type).RegisterEvent_Dynamic(methodInfo,target);
        }

        public IUnRegister Register(string name, MethodInfo methodInfo, object target)
        {
            return events.GetOrAdd<T>(name).RegisterEvent_Dynamic(methodInfo, target);
        }

        public IUnRegister Register(Type enumType,int enumId,MethodInfo methodInfo, object target)
        {
            return events.GetOrAdd<T>(enumId,enumType).RegisterEvent_Dynamic(methodInfo, target);
        }    

        public void UnRegister(Type type, MethodInfo methodInfo)
        {
            events.GetOrAdd<T>(type).UnRegisterEvent_Dynamic(methodInfo);
        }

        public void UnRegister(string name, MethodInfo methodInfo)
        {
            events.GetOrAdd<T>(name).UnRegisterEvent_Dynamic(methodInfo);
        }

        public void UnRegister(Type enumType, int enumId, MethodInfo methodInfo)
        {
            events.GetOrAdd<T>(enumId, enumType).UnRegisterEvent_Dynamic(methodInfo);
        }

        

    }

    internal class SyncDynamicEventSystem : DynamicEventSystem<SyncDynamicEvent>
    {
        public void Send(Type type, IEventArgs arg)
        {
            events.GetOrAdd<SyncDynamicEvent>(type)?.SendEvent(arg);
        }

        public void Send(string name, IEventArgs arg)
        {
            events.GetOrAdd<SyncDynamicEvent>(name)?.SendEvent(arg);
        }

        public void Send<TEnum>(TEnum type, IEventArgs arg) where TEnum : IConvertible
        {
            events.GetOrAdd<SyncDynamicEvent>(type.ToInt32(null),typeof(TEnum))?.SendEvent( arg);
        }
    }


    public class EnumEventSystem
    {
        private readonly EnumEasyEvents events = new EnumEasyEvents();

        /// <summary>
        /// 内部使用
        /// </summary>
        internal EnumEasyEvents Events => events;

        public static EnumEventSystem Global { get; } = new EnumEventSystem();

        public IUnRegister Register<TEnum>(TEnum type, Action onEvent) where TEnum : IConvertible
           => events.GetOrAddEvent<EasyEvent,TEnum>(type).RegisterEvent(onEvent);

        public IUnRegister Register<TEnum,T>(TEnum type, Action<T> onEvent) where TEnum : IConvertible
            => events.GetOrAddEvent<EasyEvent<T>,TEnum>(type).RegisterEvent(onEvent);

        public IUnRegister Register<TEnum,T, K>(TEnum type, Action<T, K> onEvent) where TEnum : IConvertible
            => events.GetOrAddEvent<EasyEvent<T, K>,TEnum>(type).RegisterEvent(onEvent);

        public IUnRegister Register<TEnum,T, K, Q>(TEnum type, Action<T, K, Q> onEvent) where TEnum : IConvertible
            => events.GetOrAddEvent<EasyEvent<T, K, Q>,TEnum>(type).RegisterEvent(onEvent);

        public IUnRegister Register<TEnum,T, K, Q, P>(TEnum type, Action<T, K, Q, P> onEvent) where TEnum : IConvertible
            => events.GetOrAddEvent<EasyEvent<T, K, Q, P>,TEnum>(type).RegisterEvent(onEvent);

        public IUnRegister Register<TEnum,T, K, Q, P, W>(TEnum type,Action<T,K,Q,P,W> onEvent) where TEnum : IConvertible
            => events.GetOrAddEvent<EasyEvent<T,K,Q,P, W>,TEnum>(type).RegisterEvent(onEvent);

        public IUnRegister Register<TEnum,T, K, Q, P, W, R>(TEnum type, Action<T, K, Q, P, W, R> onEvent) where TEnum : IConvertible
            => events.GetOrAddEvent<EasyEvent<T, K, Q, P, W, R>,TEnum>(type).RegisterEvent(onEvent);

        public IUnRegister Register<TEnum,T, K, Q, P, W, R, S>(TEnum type, Action<T, K, Q, P, W, R, S> onEvent) where TEnum : IConvertible
            => events.GetOrAddEvent<EasyEvent<T, K, Q, P, W, R, S>,TEnum>(type).RegisterEvent(onEvent);

        public IUnRegister Register<TEnum,T, K, Q, P, W, R, S, F>(TEnum type, Action<T, K, Q, P, W, R, S,F> onEvent) where TEnum : IConvertible
            => events.GetOrAddEvent<EasyEvent<T, K, Q, P, W, R, S, F>,TEnum>(type).RegisterEvent(onEvent);

        public IUnRegister Register<TEnum,T, K, Q, P, W, R, S, F, G>(TEnum type, Action<T, K, Q, P, W, R, S, F,G> onEvent) where TEnum : IConvertible
            => events.GetOrAddEvent<EasyEvent<T, K, Q, P, W, R, S, F, G>,TEnum>(type).RegisterEvent(onEvent);
        
        public IUnRegister Register<TEnum,T, K, Q, P, W, R, S, F, G, M>(TEnum type, Action<T, K, Q, P, W, R, S, F, G, M> onEvent) where TEnum : IConvertible
            => events.GetOrAddEvent<EasyEvent<T, K, Q, P, W, R, S, F, G, M>,TEnum>(type).RegisterEvent(onEvent);

        public IUnRegister Register<TEnum,T, K, Q, P, W, R, S, F, G, M, N>(TEnum type, Action<T, K, Q, P, W, R, S, F, G, M, N> onEvent) where TEnum : IConvertible
            => events.GetOrAddEvent<EasyEvent<T, K, Q, P, W, R, S, F, G, M, N>,TEnum>(type).RegisterEvent(onEvent);

        public IUnRegister Register<TEnum,T, K, Q, P, W, R, S, F, G, M, N, B>(TEnum type, Action<T, K, Q, P, W, R, S, F, G, M, N, B> onEvent) where TEnum : IConvertible
            => events.GetOrAddEvent<EasyEvent<T, K, Q, P, W, R, S, F, G, M, N, B>,TEnum>(type).RegisterEvent(onEvent);

        public IUnRegister Register<TEnum,T, K, Q, P, W, R, S, F, G, M, N, B, V>(TEnum type, Action<T, K, Q, P, W, R, S, F, G, M, N, B, V> onEvent) where TEnum : IConvertible
            => events.GetOrAddEvent<EasyEvent<T, K, Q, P, W, R, S, F, G, M, N, B, V>,TEnum>(type).RegisterEvent(onEvent);

        public IUnRegister Register<TEnum,T, K, Q, P, W, R, S, F, G, M, N, B, V, J>(TEnum type, Action<T, K, Q, P, W, R, S, F, G, M, N, B, V, J> onEvent) where TEnum : IConvertible
            => events.GetOrAddEvent<EasyEvent<T, K, Q, P, W, R, S, F, G, M, N, B, V, J>,TEnum>(type).RegisterEvent(onEvent);

        public IUnRegister Register<TEnum,T, K, Q, P, W, R, S, F, G, M, N, B, V, J, X>(TEnum type, Action<T, K, Q, P, W, R, S, F, G, M, N, B, V, J, X> onEvent) where TEnum : IConvertible
            => events.GetOrAddEvent<EasyEvent<T, K, Q, P, W, R, S, F, G, M, N, B, V, J, X>,TEnum>(type).RegisterEvent(onEvent);

        public IUnRegister Register<TEnum,T, K, Q, P, W, R, S, F, G, M, N, B, V, J, X, Z>(TEnum type, Action<T, K, Q, P, W, R, S, F, G, M, N, B, V, J, X,Z> onEvent) where TEnum : IConvertible
            => events.GetOrAddEvent<EasyEvent<T, K, Q, P, W, R, S, F, G, M, N, B, V, J, X, Z>,TEnum>(type).RegisterEvent(onEvent);

        public void Send<TEnum>(TEnum type) where TEnum : IConvertible
            => events.GetEvent<EasyEvent,TEnum>(type)?.SendEvent();

        public void Send<TEnum,T>(TEnum type, T t) where TEnum : IConvertible
            => events.GetEvent<EasyEvent<T>,TEnum>(type)?.SendEvent(t);

        public void Send<TEnum,T, K>(TEnum type, T t, K k) where TEnum : IConvertible
            => events.GetEvent<EasyEvent<T, K>,TEnum>(type)?.SendEvent(t, k);

        public void Send<TEnum,T, K, Q>(TEnum type, T t, K k, Q q) where TEnum : IConvertible
            => events.GetEvent<EasyEvent<T, K, Q>,TEnum>(type)?.SendEvent(t, k, q);

        public void Send<TEnum,T, K, Q, P>(TEnum type, T t, K k, Q q, P p) where TEnum : IConvertible
            => events.GetEvent<EasyEvent<T, K, Q, P>,TEnum>(type)?.SendEvent(t, k, q, p);

        public void Send<TEnum,T, K, Q, P, W>(TEnum type, T t, K k, Q q, P p,W w) where TEnum : IConvertible
            => events.GetEvent<EasyEvent<T, K, Q, P, W>,TEnum>(type)?.SendEvent(t, k, q, p, w);

        public void Send<TEnum,T, K, Q, P, W, R>(TEnum type, T t, K k, Q q, P p, W w, R r) where TEnum : IConvertible
            => events.GetEvent<EasyEvent<T, K, Q, P, W, R>,TEnum>(type)?.SendEvent(t, k, q, p, w, r);

        public void Send<TEnum,T, K, Q, P, W, R,S>(TEnum type, T t, K k, Q q, P p, W w, R r,S s) where TEnum : IConvertible
            => events.GetEvent<EasyEvent<T, K, Q, P, W, R,S>,TEnum>(type)?.SendEvent(t, k, q, p, w, r,s);

        public void Send<TEnum,T, K, Q, P, W, R, S,F>(TEnum type, T t, K k, Q q, P p, W w, R r, S s,F f) where TEnum : IConvertible
            => events.GetEvent<EasyEvent<T, K, Q, P, W, R, S,F>,TEnum>(type)?.SendEvent(t, k, q, p, w, r, s,f);

        public void Send<TEnum,T, K, Q, P, W, R, S, F,G>(TEnum type, T t, K k, Q q, P p, W w, R r, S s, F f,G g) where TEnum : IConvertible
            => events.GetEvent<EasyEvent<T, K, Q, P, W, R, S, F,G>,TEnum>(type)?.SendEvent(t, k, q, p, w, r, s, f,g);

        public void Send<TEnum,T, K, Q, P, W, R, S, F, G,M>(TEnum type, T t, K k, Q q, P p, W w, R r, S s, F f, G g,M m) where TEnum : IConvertible
            => events.GetEvent<EasyEvent<T, K, Q, P, W, R, S, F, G,M>,TEnum>(type)?.SendEvent(t, k, q, p, w, r, s, f, g,m);

        public void Send<TEnum,T, K, Q, P, W, R, S, F, G, M,N>(TEnum type, T t, K k, Q q, P p, W w, R r, S s, F f, G g, M m,N n) where TEnum : IConvertible
            => events.GetEvent<EasyEvent<T, K, Q, P, W, R, S, F, G, M,N>,TEnum>(type)?.SendEvent(t, k, q, p, w, r, s, f, g, m,n);

        public void Send<TEnum,T, K, Q, P, W, R, S, F, G, M, N,B>(TEnum type, T t, K k, Q q, P p, W w, R r, S s, F f, G g, M m, N n,B b) where TEnum : IConvertible
            => events.GetEvent<EasyEvent<T, K, Q, P, W, R, S, F, G, M, N,B>,TEnum>(type)?.SendEvent(t, k, q, p, w, r, s, f, g, m, n,b);

        public void Send<TEnum,T, K, Q, P, W, R, S, F, G, M, N, B,V>(TEnum type, T t, K k, Q q, P p, W w, R r, S s, F f, G g, M m, N n, B b,V x) where TEnum : IConvertible
            => events.GetEvent<EasyEvent<T, K, Q, P, W, R, S, F, G, M, N, B,V>,TEnum>(type)?.SendEvent(t, k, q, p, w, r, s, f, g, m, n, b,x);

        public void Send<TEnum,T, K, Q, P, W, R, S, F, G, M, N, B, V, J>(TEnum type, T t, K k, Q q, P p, W w, R r, S s, F f, G g, M m, N n, B b, V v, J j) where TEnum : IConvertible
          => events.GetEvent<EasyEvent<T, K, Q, P, W, R, S, F, G, M, N, B, V, J>,TEnum>(type)?.SendEvent(t, k, q, p, w, r, s, f, g, m, n, b, v, j);

        public void Send<TEnum,T, K, Q, P, W, R, S, F, G, M, N, B, V, J,X>(TEnum type, T t, K k, Q q, P p, W w, R r, S s, F f, G g, M m, N n, B b,V v, J j,X x) where TEnum : IConvertible
            => events.GetEvent<EasyEvent<T, K, Q, P, W, R, S, F, G, M, N, B,V,J,X>,TEnum>(type)?.SendEvent(t, k, q, p, w, r, s, f, g, m, n, b,v,j,x);

        public void Send<TEnum,T, K, Q, P, W, R, S, F, G, M, N, B, V, J, X,Z>(TEnum type, T t, K k, Q q, P p, W w, R r, S s, F f, G g, M m, N n, B b,V v, J j, X x,Z z) where TEnum : IConvertible
           => events.GetEvent<EasyEvent<T, K, Q, P, W, R, S, F, G, M, N, B,V, J, X,Z>,TEnum>(type)?.SendEvent(t, k, q, p, w, r, s, f, g, m, n, b,v, j, x,z);

        public void UnRegister<TEnum>(TEnum type, Action onEvent) where TEnum : IConvertible
            => events.GetEvent<EasyEvent,TEnum>(type)?.UnRegister(onEvent);

        public void UnRegister<T,TEnum>(TEnum type, Action<T> onEvent) where TEnum : IConvertible
            => events.GetEvent<EasyEvent<T>,TEnum>(type)?.UnRegister(onEvent);

        public void UnRegister<T, K,TEnum>(TEnum type, Action<T, K> onEvent) where TEnum : IConvertible
            => events.GetEvent<EasyEvent<T, K>,TEnum>(type)?.UnRegister(onEvent);

        public void UnRegister<T, K, Q,TEnum>(TEnum type, Action<T, K, Q> onEvent) where TEnum : IConvertible
            => events.GetEvent<EasyEvent<T, K, Q>,TEnum>(type)?.UnRegister(onEvent);

        public void UnRegister<T, K, Q, P,TEnum>(TEnum type, Action<T, K, Q, P> onEvent) where TEnum : IConvertible
            => events.GetEvent<EasyEvent<T, K, Q, P>,TEnum>(type)?.UnRegister(onEvent);

        public void UnRegister<T, K, Q, P, W,TEnum>(TEnum type, Action<T, K, Q, P, W> onEvent) where TEnum : IConvertible
           => events.GetOrAddEvent<EasyEvent<T, K, Q, P, W>,TEnum>(type).UnRegister(onEvent);

        public void UnRegister<T, K, Q, P, W, R,TEnum>(TEnum type, Action<T, K, Q, P, W, R> onEvent) where TEnum : IConvertible
            => events.GetOrAddEvent<EasyEvent<T, K, Q, P, W, R>,TEnum>(type).UnRegister(onEvent);

        public void UnRegister<T, K, Q, P, W, R, S,TEnum>(TEnum type, Action<T, K, Q, P, W, R, S> onEvent) where TEnum : IConvertible
            => events.GetOrAddEvent<EasyEvent<T, K, Q, P, W, R, S>,TEnum>(type).UnRegister(onEvent);

        public void UnRegister<T, K, Q, P, W, R, S, F,TEnum>(TEnum type, Action<T, K, Q, P, W, R, S, F> onEvent) where TEnum : IConvertible
            => events.GetOrAddEvent<EasyEvent<T, K, Q, P, W, R, S, F>,TEnum>(type).UnRegister(onEvent);

        public void UnRegister<T, K, Q, P, W, R, S, F, G,TEnum>(TEnum type, Action<T, K, Q, P, W, R, S, F, G> onEvent) where TEnum : IConvertible
            => events.GetOrAddEvent<EasyEvent<T, K, Q, P, W, R, S, F, G>,TEnum>(type).UnRegister(onEvent);

        public void UnRegister<T, K, Q, P, W, R, S, F, G, M,TEnum>(TEnum type, Action<T, K, Q, P, W, R, S, F, G, M> onEvent) where TEnum : IConvertible
            => events.GetOrAddEvent<EasyEvent<T, K, Q, P, W, R, S, F, G, M>,TEnum>(type).UnRegister(onEvent);

        public void UnRegister<T, K, Q, P, W, R, S, F, G, M, N,TEnum>(TEnum type, Action<T, K, Q, P, W, R, S, F, G, M, N> onEvent) where TEnum : IConvertible
            => events.GetOrAddEvent<EasyEvent<T, K, Q, P, W, R, S, F, G, M, N>,TEnum>(type).UnRegister(onEvent);

        public void UnRegister<T, K, Q, P, W, R, S, F, G, M, N, B,TEnum>(TEnum type, Action<T, K, Q, P, W, R, S, F, G, M, N, B> onEvent) where TEnum : IConvertible
            => events.GetOrAddEvent<EasyEvent<T, K, Q, P, W, R, S, F, G, M, N, B>,TEnum>(type).UnRegister(onEvent);

        public void UnRegister<T, K, Q, P, W, R, S, F, G, M, N, B, V,TEnum>(TEnum type, Action<T, K, Q, P, W, R, S, F, G, M, N, B, V> onEvent) where TEnum : IConvertible
            => events.GetOrAddEvent<EasyEvent<T, K, Q, P, W, R, S, F, G, M, N, B, V>,TEnum>(type).UnRegister(onEvent);

        public void UnRegister<T, K, Q, P, W, R, S, F, G, M, N, B, V, J,TEnum>(TEnum type, Action<T, K, Q, P, W, R, S, F, G, M, N, B, V, J> onEvent) where TEnum : IConvertible
            => events.GetOrAddEvent<EasyEvent<T, K, Q, P, W, R, S, F, G, M, N, B, V, J>,TEnum>(type).UnRegister(onEvent);

        public void UnRegister<T, K, Q, P, W, R, S, F, G, M, N, B, V, J, X,TEnum>(TEnum type, Action<T, K, Q, P, W, R, S, F, G, M, N, B, V, J, X> onEvent) where TEnum : IConvertible
            => events.GetOrAddEvent<EasyEvent<T, K, Q, P, W, R, S, F, G, M, N, B, V, J, X>,TEnum>(type).UnRegister(onEvent);

        public void UnRegister<T, K, Q, P, W, R, S, F, G, M, N, B, V, J, X, Z,TEnum>(TEnum type, Action<T, K, Q, P, W, R, S, F, G, M, N, B, V, J, X, Z> onEvent) where TEnum : IConvertible
            => events.GetOrAddEvent<EasyEvent<T, K, Q, P, W, R, S, F, G, M, N, B, V, J, X, Z>,TEnum>(type).UnRegister(onEvent);


        public void Clear() => events.ClearEvent();
    }

    public class StringEventSystem
    {
        private readonly StringEasyEvents events = new StringEasyEvents();

        /// <summary>
        /// 内部使用
        /// </summary>
        internal StringEasyEvents Events => events;

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

        public IUnRegister Register<T, K, Q, P, W>(string name, Action<T, K, Q, P, W> onEvent)
           => events.GetOrAddEvent<EasyEvent<T, K, Q, P, W>>(name).RegisterEvent(onEvent);

        public IUnRegister Register<T, K, Q, P, W, R>(string name, Action<T, K, Q, P, W, R> onEvent)
            => events.GetOrAddEvent<EasyEvent<T, K, Q, P, W, R>>(name).RegisterEvent(onEvent);

        public IUnRegister Register<T, K, Q, P, W, R, S>(string name, Action<T, K, Q, P, W, R, S> onEvent)
            => events.GetOrAddEvent<EasyEvent<T, K, Q, P, W, R, S>>(name).RegisterEvent(onEvent);

        public IUnRegister Register<T, K, Q, P, W, R, S, F>(string name, Action<T, K, Q, P, W, R, S, F> onEvent)
            => events.GetOrAddEvent<EasyEvent<T, K, Q, P, W, R, S, F>>(name).RegisterEvent(onEvent);

        public IUnRegister Register<T, K, Q, P, W, R, S, F, G>(string name, Action<T, K, Q, P, W, R, S, F, G> onEvent)
            => events.GetOrAddEvent<EasyEvent<T, K, Q, P, W, R, S, F, G>>(name).RegisterEvent(onEvent);

        public IUnRegister Register<T, K, Q, P, W, R, S, F, G, M>(string name, Action<T, K, Q, P, W, R, S, F, G, M> onEvent)
            => events.GetOrAddEvent<EasyEvent<T, K, Q, P, W, R, S, F, G, M>>(name).RegisterEvent(onEvent);

        public IUnRegister Register<T, K, Q, P, W, R, S, F, G, M, N>(string name, Action<T, K, Q, P, W, R, S, F, G, M, N> onEvent)
            => events.GetOrAddEvent<EasyEvent<T, K, Q, P, W, R, S, F, G, M, N>>(name).RegisterEvent(onEvent);

        public IUnRegister Register<T, K, Q, P, W, R, S, F, G, M, N, B>(string name, Action<T, K, Q, P, W, R, S, F, G, M, N, B> onEvent)
            => events.GetOrAddEvent<EasyEvent<T, K, Q, P, W, R, S, F, G, M, N, B>>(name).RegisterEvent(onEvent);

        public IUnRegister Register<T, K, Q, P, W, R, S, F, G, M, N, B, V>(string name, Action<T, K, Q, P, W, R, S, F, G, M, N, B, V> onEvent)
            => events.GetOrAddEvent<EasyEvent<T, K, Q, P, W, R, S, F, G, M, N, B, V>>(name).RegisterEvent(onEvent);

        public IUnRegister Register<T, K, Q, P, W, R, S, F, G, M, N, B, V, J>(string name, Action<T, K, Q, P, W, R, S, F, G, M, N, B, V, J> onEvent)
            => events.GetOrAddEvent<EasyEvent<T, K, Q, P, W, R, S, F, G, M, N, B, V, J>>(name).RegisterEvent(onEvent);

        public IUnRegister Register<T, K, Q, P, W, R, S, F, G, M, N, B, V, J, X>(string name, Action<T, K, Q, P, W, R, S, F, G, M, N, B, V, J, X> onEvent)
            => events.GetOrAddEvent<EasyEvent<T, K, Q, P, W, R, S, F, G, M, N, B, V, J, X>>(name).RegisterEvent(onEvent);

        public IUnRegister Register<T, K, Q, P, W, R, S, F, G, M, N, B, V, J, X, Z>(string name, Action<T, K, Q, P, W, R, S, F, G, M, N, B, V, J, X, Z> onEvent)
            => events.GetOrAddEvent<EasyEvent<T, K, Q, P, W, R, S, F, G, M, N, B, V, J, X, Z>>(name).RegisterEvent(onEvent);

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

        public void Send<T, K, Q, P, W>(string name, T t, K k, Q q, P p, W w)
           => events.GetEvent<EasyEvent<T, K, Q, P, W>>(name)?.SendEvent(t, k, q, p, w);

        public void Send<T, K, Q, P, W, R>(string name, T t, K k, Q q, P p, W w, R r)
            => events.GetEvent<EasyEvent<T, K, Q, P, W, R>>(name)?.SendEvent(t, k, q, p, w, r);

        public void Send<T, K, Q, P, W, R, S>(string name, T t, K k, Q q, P p, W w, R r, S s)
            => events.GetEvent<EasyEvent<T, K, Q, P, W, R, S>>(name)?.SendEvent(t, k, q, p, w, r, s);

        public void Send<T, K, Q, P, W, R, S, F>(string name, T t, K k, Q q, P p, W w, R r, S s, F f)
            => events.GetEvent<EasyEvent<T, K, Q, P, W, R, S, F>>(name)?.SendEvent(t, k, q, p, w, r, s, f);

        public void Send<T, K, Q, P, W, R, S, F, G>(string name, T t, K k, Q q, P p, W w, R r, S s, F f, G g)
            => events.GetEvent<EasyEvent<T, K, Q, P, W, R, S, F, G>>(name)?.SendEvent(t, k, q, p, w, r, s, f, g);

        public void Send<T, K, Q, P, W, R, S, F, G, M>(string name, T t, K k, Q q, P p, W w, R r, S s, F f, G g, M m)
            => events.GetEvent<EasyEvent<T, K, Q, P, W, R, S, F, G, M>>(name)?.SendEvent(t, k, q, p, w, r, s, f, g, m);

        public void Send<T, K, Q, P, W, R, S, F, G, M, N>(string name, T t, K k, Q q, P p, W w, R r, S s, F f, G g, M m, N n)
            => events.GetEvent<EasyEvent<T, K, Q, P, W, R, S, F, G, M, N>>(name)?.SendEvent(t, k, q, p, w, r, s, f, g, m, n);

        public void Send<T, K, Q, P, W, R, S, F, G, M, N, B>(string name, T t, K k, Q q, P p, W w, R r, S s, F f, G g, M m, N n, B b)
            => events.GetEvent<EasyEvent<T, K, Q, P, W, R, S, F, G, M, N, B>>(name)?.SendEvent(t, k, q, p, w, r, s, f, g, m, n, b);

        public void Send<T, K, Q, P, W, R, S, F, G, M, N, B, V>(string name, T t, K k, Q q, P p, W w, R r, S s, F f, G g, M m, N n, B b, V x)
            => events.GetEvent<EasyEvent<T, K, Q, P, W, R, S, F, G, M, N, B, V>>(name)?.SendEvent(t, k, q, p, w, r, s, f, g, m, n, b, x);

        public void Send<T, K, Q, P, W, R, S, F, G, M, N, B, V, J>(string name, T t, K k, Q q, P p, W w, R r, S s, F f, G g, M m, N n, B b, V v, J j)
          => events.GetEvent<EasyEvent<T, K, Q, P, W, R, S, F, G, M, N, B, V, J>>(name)?.SendEvent(t, k, q, p, w, r, s, f, g, m, n, b, v, j);

        public void Send<T, K, Q, P, W, R, S, F, G, M, N, B, V, J, X>(string name, T t, K k, Q q, P p, W w, R r, S s, F f, G g, M m, N n, B b, V v, J j, X x)
            => events.GetEvent<EasyEvent<T, K, Q, P, W, R, S, F, G, M, N, B, V, J, X>>(name)?.SendEvent(t, k, q, p, w, r, s, f, g, m, n, b, v, j, x);

        public void Send<T, K, Q, P, W, R, S, F, G, M, N, B, V, J, X, Z>(string name, T t, K k, Q q, P p, W w, R r, S s, F f, G g, M m, N n, B b, V v, J j, X x, Z z)
           => events.GetEvent<EasyEvent<T, K, Q, P, W, R, S, F, G, M, N, B, V, J, X, Z>>(name)?.SendEvent(t, k, q, p, w, r, s, f, g, m, n, b, v, j, x, z);

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

        public void UnRegister<T, K, Q, P, W>(string name, Action<T, K, Q, P, W> onEvent)
           => events.GetOrAddEvent<EasyEvent<T, K, Q, P, W>>(name).UnRegister(onEvent);

        public void UnRegister<T, K, Q, P, W, R>(string name, Action<T, K, Q, P, W, R> onEvent)
            => events.GetOrAddEvent<EasyEvent<T, K, Q, P, W, R>>(name).UnRegister(onEvent);

        public void UnRegister<T, K, Q, P, W, R, S>(string name, Action<T, K, Q, P, W, R, S> onEvent)
            => events.GetOrAddEvent<EasyEvent<T, K, Q, P, W, R, S>>(name).UnRegister(onEvent);

        public void UnRegister<T, K, Q, P, W, R, S, F>(string name, Action<T, K, Q, P, W, R, S, F> onEvent)
            => events.GetOrAddEvent<EasyEvent<T, K, Q, P, W, R, S, F>>(name).UnRegister(onEvent);

        public void UnRegister<T, K, Q, P, W, R, S, F, G>(string name, Action<T, K, Q, P, W, R, S, F, G> onEvent)
            => events.GetOrAddEvent<EasyEvent<T, K, Q, P, W, R, S, F, G>>(name).UnRegister(onEvent);

        public void UnRegister<T, K, Q, P, W, R, S, F, G, M>(string name, Action<T, K, Q, P, W, R, S, F, G, M> onEvent)
            => events.GetOrAddEvent<EasyEvent<T, K, Q, P, W, R, S, F, G, M>>(name).UnRegister(onEvent);

        public void UnRegister<T, K, Q, P, W, R, S, F, G, M, N>(string name, Action<T, K, Q, P, W, R, S, F, G, M, N> onEvent)
            => events.GetOrAddEvent<EasyEvent<T, K, Q, P, W, R, S, F, G, M, N>>(name).UnRegister(onEvent);

        public void UnRegister<T, K, Q, P, W, R, S, F, G, M, N, B>(string name, Action<T, K, Q, P, W, R, S, F, G, M, N, B> onEvent)
            => events.GetOrAddEvent<EasyEvent<T, K, Q, P, W, R, S, F, G, M, N, B>>(name).UnRegister(onEvent);

        public void UnRegister<T, K, Q, P, W, R, S, F, G, M, N, B, V>(string name, Action<T, K, Q, P, W, R, S, F, G, M, N, B, V> onEvent)
            => events.GetOrAddEvent<EasyEvent<T, K, Q, P, W, R, S, F, G, M, N, B, V>>(name).UnRegister(onEvent);

        public void UnRegister<T, K, Q, P, W, R, S, F, G, M, N, B, V, J>(string name, Action<T, K, Q, P, W, R, S, F, G, M, N, B, V, J> onEvent)
            => events.GetOrAddEvent<EasyEvent<T, K, Q, P, W, R, S, F, G, M, N, B, V, J>>(name).UnRegister(onEvent);

        public void UnRegister<T, K, Q, P, W, R, S, F, G, M, N, B, V, J, X>(string name, Action<T, K, Q, P, W, R, S, F, G, M, N, B, V, J, X> onEvent)
            => events.GetOrAddEvent<EasyEvent<T, K, Q, P, W, R, S, F, G, M, N, B, V, J, X>>(name).UnRegister(onEvent);

        public void UnRegister<T, K, Q, P, W, R, S, F, G, M, N, B, V, J, X, Z>(string name, Action<T, K, Q, P, W, R, S, F, G, M, N, B, V, J, X, Z> onEvent)
            => events.GetOrAddEvent<EasyEvent<T, K, Q, P, W, R, S, F, G, M, N, B, V, J, X, Z>>(name).UnRegister(onEvent);

        public void Clear() => events.ClearEvent();
    }

    public class TypeEventSystem
    {
        private readonly EasyEvents events = new EasyEvents();

        /// <summary>
        /// 内部使用
        /// </summary>
        internal EasyEvents Events => events;

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

        public IUnRegister Register<T, K, Q, P, W>(Action<T, K, Q, P, W> onEvent)
          => events.GetOrAddEvent<EasyEvent<T, K, Q, P, W>>().RegisterEvent(onEvent);

        public IUnRegister Register<T, K, Q, P, W, R>(Action<T, K, Q, P, W, R> onEvent)
            => events.GetOrAddEvent<EasyEvent<T, K, Q, P, W, R>>().RegisterEvent(onEvent);

        public IUnRegister Register<T, K, Q, P, W, R, S>(Action<T, K, Q, P, W, R, S> onEvent)
            => events.GetOrAddEvent<EasyEvent<T, K, Q, P, W, R, S>>().RegisterEvent(onEvent);

        public IUnRegister Register<T, K, Q, P, W, R, S, F>(Action<T, K, Q, P, W, R, S, F> onEvent)
            => events.GetOrAddEvent<EasyEvent<T, K, Q, P, W, R, S, F>>().RegisterEvent(onEvent);

        public IUnRegister Register<T, K, Q, P, W, R, S, F, G>(Action<T, K, Q, P, W, R, S, F, G> onEvent)
            => events.GetOrAddEvent<EasyEvent<T, K, Q, P, W, R, S, F, G>>().RegisterEvent(onEvent);

        public IUnRegister Register<T, K, Q, P, W, R, S, F, G, M>(Action<T, K, Q, P, W, R, S, F, G, M> onEvent)
            => events.GetOrAddEvent<EasyEvent<T, K, Q, P, W, R, S, F, G, M>>().RegisterEvent(onEvent);

        public IUnRegister Register<T, K, Q, P, W, R, S, F, G, M, N>(Action<T, K, Q, P, W, R, S, F, G, M, N> onEvent)
            => events.GetOrAddEvent<EasyEvent<T, K, Q, P, W, R, S, F, G, M, N>>().RegisterEvent(onEvent);

        public IUnRegister Register<T, K, Q, P, W, R, S, F, G, M, N, B>(Action<T, K, Q, P, W, R, S, F, G, M, N, B> onEvent)
            => events.GetOrAddEvent<EasyEvent<T, K, Q, P, W, R, S, F, G, M, N, B>>().RegisterEvent(onEvent);

        public IUnRegister Register<T, K, Q, P, W, R, S, F, G, M, N, B, V>(Action<T, K, Q, P, W, R, S, F, G, M, N, B, V> onEvent)
            => events.GetOrAddEvent<EasyEvent<T, K, Q, P, W, R, S, F, G, M, N, B, V>>().RegisterEvent(onEvent);

        public IUnRegister Register<T, K, Q, P, W, R, S, F, G, M, N, B, V, J>(Action<T, K, Q, P, W, R, S, F, G, M, N, B, V, J> onEvent)
            => events.GetOrAddEvent<EasyEvent<T, K, Q, P, W, R, S, F, G, M, N, B, V, J>>().RegisterEvent(onEvent);

        public IUnRegister Register<T, K, Q, P, W, R, S, F, G, M, N, B, V, J, X>(Action<T, K, Q, P, W, R, S, F, G, M, N, B, V, J, X> onEvent)
            => events.GetOrAddEvent<EasyEvent<T, K, Q, P, W, R, S, F, G, M, N, B, V, J, X>>().RegisterEvent(onEvent);

        public IUnRegister Register<T, K, Q, P, W, R, S, F, G, M, N, B, V, J, X, Z>(Action<T, K, Q, P, W, R, S, F, G, M, N, B, V, J, X, Z> onEvent)
            => events.GetOrAddEvent<EasyEvent<T, K, Q, P, W, R, S, F, G, M, N, B, V, J, X, Z>>().RegisterEvent(onEvent);

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

        public void Send<T, K, Q, P, W>(T t, K k, Q q, P p, W w)
           => events.GetEvent<EasyEvent<T, K, Q, P, W>>()?.SendEvent(t, k, q, p, w);

        public void Send<T, K, Q, P, W, R>(T t, K k, Q q, P p, W w, R r)
            => events.GetEvent<EasyEvent<T, K, Q, P, W, R>>()?.SendEvent(t, k, q, p, w, r);

        public void Send<T, K, Q, P, W, R, S>(T t, K k, Q q, P p, W w, R r, S s)
            => events.GetEvent<EasyEvent<T, K, Q, P, W, R, S>>()?.SendEvent(t, k, q, p, w, r, s);

        public void Send<T, K, Q, P, W, R, S, F>(T t, K k, Q q, P p, W w, R r, S s, F f)
            => events.GetEvent<EasyEvent<T, K, Q, P, W, R, S, F>>()?.SendEvent(t, k, q, p, w, r, s, f);

        public void Send<T, K, Q, P, W, R, S, F, G>(T t, K k, Q q, P p, W w, R r, S s, F f, G g)
            => events.GetEvent<EasyEvent<T, K, Q, P, W, R, S, F, G>>()?.SendEvent(t, k, q, p, w, r, s, f, g);

        public void Send<T, K, Q, P, W, R, S, F, G, M>(T t, K k, Q q, P p, W w, R r, S s, F f, G g, M m)
            => events.GetEvent<EasyEvent<T, K, Q, P, W, R, S, F, G, M>>()?.SendEvent(t, k, q, p, w, r, s, f, g, m);

        public void Send<T, K, Q, P, W, R, S, F, G, M, N>(T t, K k, Q q, P p, W w, R r, S s, F f, G g, M m, N n)
            => events.GetEvent<EasyEvent<T, K, Q, P, W, R, S, F, G, M, N>>()?.SendEvent(t, k, q, p, w, r, s, f, g, m, n);

        public void Send<T, K, Q, P, W, R, S, F, G, M, N, B>(T t, K k, Q q, P p, W w, R r, S s, F f, G g, M m, N n, B b)
            => events.GetEvent<EasyEvent<T, K, Q, P, W, R, S, F, G, M, N, B>>()?.SendEvent(t, k, q, p, w, r, s, f, g, m, n, b);

        public void Send<T, K, Q, P, W, R, S, F, G, M, N, B, V>(T t, K k, Q q, P p, W w, R r, S s, F f, G g, M m, N n, B b, V x)
            => events.GetEvent<EasyEvent<T, K, Q, P, W, R, S, F, G, M, N, B, V>>()?.SendEvent(t, k, q, p, w, r, s, f, g, m, n, b, x);

        public void Send<T, K, Q, P, W, R, S, F, G, M, N, B, V, J>(T t, K k, Q q, P p, W w, R r, S s, F f, G g, M m, N n, B b, V v, J j)
          => events.GetEvent<EasyEvent<T, K, Q, P, W, R, S, F, G, M, N, B, V, J>>()?.SendEvent(t, k, q, p, w, r, s, f, g, m, n, b, v, j);

        public void Send<T, K, Q, P, W, R, S, F, G, M, N, B, V, J, X>(T t, K k, Q q, P p, W w, R r, S s, F f, G g, M m, N n, B b, V v, J j, X x)
            => events.GetEvent<EasyEvent<T, K, Q, P, W, R, S, F, G, M, N, B, V, J, X>>()?.SendEvent(t, k, q, p, w, r, s, f, g, m, n, b, v, j, x);

        public void Send<T, K, Q, P, W, R, S, F, G, M, N, B, V, J, X, Z>(T t, K k, Q q, P p, W w, R r, S s, F f, G g, M m, N n, B b, V v, J j, X x, Z z)
           => events.GetEvent<EasyEvent<T, K, Q, P, W, R, S, F, G, M, N, B, V, J, X, Z>>()?.SendEvent(t, k, q, p, w, r, s, f, g, m, n, b, v, j, x, z);

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

        public void UnRegister<T, K, Q, P, W>(Action<T, K, Q, P, W> onEvent)
          => events.GetOrAddEvent<EasyEvent<T, K, Q, P, W>>().UnRegister(onEvent);

        public void UnRegister<T, K, Q, P, W, R>(Action<T, K, Q, P, W, R> onEvent)
            => events.GetOrAddEvent<EasyEvent<T, K, Q, P, W, R>>().UnRegister(onEvent);

        public void UnRegister<T, K, Q, P, W, R, S>(Action<T, K, Q, P, W, R, S> onEvent)
            => events.GetOrAddEvent<EasyEvent<T, K, Q, P, W, R, S>>().UnRegister(onEvent);

        public void UnRegister<T, K, Q, P, W, R, S, F>(Action<T, K, Q, P, W, R, S, F> onEvent)
            => events.GetOrAddEvent<EasyEvent<T, K, Q, P, W, R, S, F>>().UnRegister(onEvent);

        public void UnRegister<T, K, Q, P, W, R, S, F, G>(Action<T, K, Q, P, W, R, S, F, G> onEvent)
            => events.GetOrAddEvent<EasyEvent<T, K, Q, P, W, R, S, F, G>>().UnRegister(onEvent);

        public void UnRegister<T, K, Q, P, W, R, S, F, G, M>(Action<T, K, Q, P, W, R, S, F, G, M> onEvent)
            => events.GetOrAddEvent<EasyEvent<T, K, Q, P, W, R, S, F, G, M>>().UnRegister(onEvent);

        public void UnRegister<T, K, Q, P, W, R, S, F, G, M, N>(Action<T, K, Q, P, W, R, S, F, G, M, N> onEvent)
            => events.GetOrAddEvent<EasyEvent<T, K, Q, P, W, R, S, F, G, M, N>>().UnRegister(onEvent);

        public void UnRegister<T, K, Q, P, W, R, S, F, G, M, N, B>(Action<T, K, Q, P, W, R, S, F, G, M, N, B> onEvent)
            => events.GetOrAddEvent<EasyEvent<T, K, Q, P, W, R, S, F, G, M, N, B>>().UnRegister(onEvent);

        public void UnRegister<T, K, Q, P, W, R, S, F, G, M, N, B, V>(Action<T, K, Q, P, W, R, S, F, G, M, N, B, V> onEvent)
            => events.GetOrAddEvent<EasyEvent<T, K, Q, P, W, R, S, F, G, M, N, B, V>>().UnRegister(onEvent);

        public void UnRegister<T, K, Q, P, W, R, S, F, G, M, N, B, V, J>(Action<T, K, Q, P, W, R, S, F, G, M, N, B, V, J> onEvent)
            => events.GetOrAddEvent<EasyEvent<T, K, Q, P, W, R, S, F, G, M, N, B, V, J>>().UnRegister(onEvent);

        public void UnRegister<T, K, Q, P, W, R, S, F, G, M, N, B, V, J, X>(Action<T, K, Q, P, W, R, S, F, G, M, N, B, V, J, X> onEvent)
            => events.GetOrAddEvent<EasyEvent<T, K, Q, P, W, R, S, F, G, M, N, B, V, J, X>>().UnRegister(onEvent);

        public void UnRegister<T, K, Q, P, W, R, S, F, G, M, N, B, V, J, X, Z>(Action<T, K, Q, P, W, R, S, F, G, M, N, B, V, J, X, Z> onEvent)
            => events.GetOrAddEvent<EasyEvent<T, K, Q, P, W, R, S, F, G, M, N, B, V, J, X, Z>>().UnRegister(onEvent);

        public void Clear() => events.ClearEvent();
    }
    #region Async Event System
    public class AsyncTypeEventSystem
    {
        private readonly EasyEvents events = new EasyEvents();

        /// <summary>
        /// 内部使用
        /// </summary>
        internal EasyEvents Events => events;

        public static AsyncTypeEventSystem Global { get; } = new AsyncTypeEventSystem();

        public IUnRegister Register<T>(Func<T,Task> onEvent)
            => events.GetOrAddEvent<AsyncEasyEvent<T>>().RegisterEvent(onEvent);
      
        public async Task Send<T>(T t)
            => await events.GetEvent<AsyncEasyEvent<T>>()?.SendEvent(t);

       
        public void UnRegister<T>(Func<T, Task> onEvent)
            => events.GetEvent<AsyncEasyEvent<T>>()?.UnRegister(onEvent); 

        public void Clear() => events.ClearEvent();
    }

    public class AsyncStringEventSystem
    {
        private readonly StringEasyEvents events = new StringEasyEvents();

        /// <summary>
        /// 内部使用
        /// </summary>
        internal StringEasyEvents Events => events;

        public static AsyncStringEventSystem Global { get; } = new AsyncStringEventSystem();

        public IUnRegister Register<T>(string name,Func<T, Task> onEvent)
            => events.GetOrAddEvent<AsyncEasyEvent<T>>(name).RegisterEvent(onEvent);

        public async Task Send<T>(string name,T t)
            => await events.GetEvent<AsyncEasyEvent<T>>(name)?.SendEvent(t);


        public void UnRegister<T>(string name,Func<T, Task> onEvent)
            => events.GetEvent<AsyncEasyEvent<T>>(name)?.UnRegister(onEvent);

        public void Clear() => events.ClearEvent();
    }

    public class AsyncEnumEventSystem
    {
        private readonly EnumEasyEvents events = new EnumEasyEvents();

        /// <summary>
        /// 内部使用
        /// </summary>
        internal EnumEasyEvents Events => events;

        public static AsyncEnumEventSystem Global { get; } = new AsyncEnumEventSystem();

        public IUnRegister Register<TEnum,T>(TEnum e,Func<T, Task> onEvent) where TEnum : IConvertible
            => events.GetOrAddEvent<AsyncEasyEvent<T>,TEnum>(e).RegisterEvent(onEvent);

        public async Task Send<TEnum,T>(TEnum e,T t) where TEnum : IConvertible
            => await events.GetEvent<AsyncEasyEvent<T>, TEnum>(e)?.SendEvent(t);


        public void UnRegister<TEnum,T>(TEnum e,Func<T, Task> onEvent) where TEnum : IConvertible
            => events.GetEvent<AsyncEasyEvent<T>, TEnum>(e)?.UnRegister(onEvent);

        public void Clear() => events.ClearEvent();
    }
    #endregion

    #region Unity Async Event System
#if UNITY_2021_1_OR_NEWER
    public class Unity_AsyncTypeEventSystem
    {
        private readonly EasyEvents events = new EasyEvents();

        /// <summary>
        /// 内部使用
        /// </summary>
        internal EasyEvents Events => events;

        public static Unity_AsyncTypeEventSystem Global { get; } = new Unity_AsyncTypeEventSystem();

        public IUnRegister Register<T>(Func<T, YieldTask> onEvent)
            => events.GetOrAddEvent<Unity_AsyncEasyEvent<T>>().RegisterEvent(onEvent);

        public async YieldTask Send<T>(T t)
            => await events.GetEvent<Unity_AsyncEasyEvent<T>>()?.SendEvent(t);


        public void UnRegister<T>(Func<T, YieldTask> onEvent)
            => events.GetEvent<Unity_AsyncEasyEvent<T>>()?.UnRegister(onEvent);

        public void Clear() => events.ClearEvent();
    }

    public class Unity_AsyncStringEventSystem
    {
        private readonly StringEasyEvents events = new StringEasyEvents();

        /// <summary>
        /// 内部使用
        /// </summary>
        internal StringEasyEvents Events => events;

        public static Unity_AsyncStringEventSystem Global { get; } = new Unity_AsyncStringEventSystem();

        public IUnRegister Register<T>(string name, Func<T, YieldTask> onEvent)
            => events.GetOrAddEvent<Unity_AsyncEasyEvent<T>>(name).RegisterEvent(onEvent);

        public async Task Send<T>(string name, T t)
            => await events.GetEvent<Unity_AsyncEasyEvent<T>>(name)?.SendEvent(t);


        public void UnRegister<T>(string name, Func<T, YieldTask> onEvent)
            => events.GetEvent<Unity_AsyncEasyEvent<T>>(name)?.UnRegister(onEvent);

        public void Clear() => events.ClearEvent();
    }

    public class Unity_AsyncEnumEventSystem
    {
        private readonly EnumEasyEvents events = new EnumEasyEvents();

        /// <summary>
        /// 内部使用
        /// </summary>
        internal EnumEasyEvents Events => events;

        public static Unity_AsyncEnumEventSystem Global { get; } = new Unity_AsyncEnumEventSystem();

        public IUnRegister Register<TEnum, T>(TEnum e, Func<T, YieldTask> onEvent) where TEnum : IConvertible
            => events.GetOrAddEvent<Unity_AsyncEasyEvent<T>, TEnum>(e).RegisterEvent(onEvent);

        public async Task Send<TEnum, T>(TEnum e, T t) where TEnum : IConvertible
            => await events.GetEvent<Unity_AsyncEasyEvent<T>, TEnum>(e)?.SendEvent(t);


        public void UnRegister<TEnum, T>(TEnum e, Func<T, YieldTask> onEvent) where TEnum : IConvertible
            => events.GetEvent<Unity_AsyncEasyEvent<T>, TEnum>(e)?.UnRegister(onEvent);

        public void Clear() => events.ClearEvent();
    }
#endif
    #endregion
    public class EasyEvents
    {
        internal readonly Dictionary<Type, IEasyEvent> events = new Dictionary<Type, IEasyEvent>();

        public T GetOrAddEvent<T>() where T : IEasyEvent, new()
        {        
            if (!events.TryGetValue(typeof(T), out var easyEvent))
            {
                easyEvent = new T();
                events.Add(typeof(T), easyEvent);
            }
            return (T)easyEvent;
        }

        public T GetEvent<T>() where T : IEasyEvent
        {
            events.TryGetValue(typeof(T), out var eventSystem);          
            return (T)eventSystem;
        }

        public void AddEvent<T>() where T : IEasyEvent, new() => events.Add(typeof(T), new T());

        public void ClearEvent()
        {
            events.Clear();
        }
    }

    public class StringEasyEvents
    {
        internal readonly Dictionary<string, IEasyEvent> events = new Dictionary<string, IEasyEvent>();
        public T GetOrAddEvent<T>(string name) where T : IEasyEvent, new()
        {
            if (!events.TryGetValue(name, out var easyEvent))
            {             
                easyEvent = new T();
                events.Add(name, easyEvent);
            }
            return (T)easyEvent;
        }

        public T GetEvent<T>(string name) where T : IEasyEvent
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

        public void AddEvent<T>(string name) where T : IEasyEvent, new() => events.Add(name, new T());

        public void ClearEvent()
        {
            events.Clear();
        }
    }

    public class EnumEasyEvents
    {
        internal readonly Dictionary<(int,Type), IEasyEvent> events = new Dictionary<(int, Type), IEasyEvent>();

        internal T GetOrAddEvent_Internal<T>(IConvertible type) where T : IEasyEvent, new()
        {
            return GetOrAdd<T>(type.ToInt32(null), type.GetType());
        }

        public T GetOrAddEvent<T,TEnum>(TEnum type) where T : IEasyEvent, new() where TEnum : IConvertible
        { 
            return GetOrAdd<T>(type.ToInt32(null),typeof(TEnum));         
        }

        private T GetOrAdd<T>(int id,Type type) where T : IEasyEvent,new()
        {         
            if (!events.TryGetValue((id,type), out var easyEvent))
            {
                easyEvent = new T();

                events.Add((id, type), easyEvent);
            }
            return (T)easyEvent;
        }

        public T GetEvent<T,TEnum>(TEnum type) where T : IEasyEvent where TEnum : IConvertible
        {
            var core = (type.ToInt32(null), typeof(TEnum));
            events.TryGetValue(core, out var eventSystem);
            try
            {
                return (T)eventSystem;
            }
            catch
            {
                throw new InvalidCastException($"事件类型有误！当前事件标识是{type},返回错误的类型是{typeof(T)}");
            }
        }

        public void AddEvent<T,TEnum>(TEnum type) where TEnum : IConvertible where T : IEasyEvent, new() => events.Add((type.ToInt32(null), typeof(TEnum)), new T());

        public void ClearEvent()
        {
            events.Clear();
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
    }

    /// <summary>
    /// 事件系统拓展
    /// </summary>
    public static partial class EventSystemExtension
    {
        public static IUnRegister RegisterEvent<T>(this IGetRegisterEvent registerEvent, Action<T> onEvent)
            => registerEvent.GetArchitecture().RegisterEvent(onEvent);

        public static IUnRegister RegisterEvent_Async<T>(this IGetRegisterEvent registerEvent, Func<T,Task> onEvent)
            => registerEvent.GetArchitecture().RegisterEvent_Async(onEvent);

        public static IUnRegister RegisterEvent_Async<T>(this IGetRegisterEvent registerEvent,string name, Func<T, Task> onEvent)
            => registerEvent.GetArchitecture().RegisterEvent_Async(name,onEvent);

        public static IUnRegister RegisterEvent_Async<TEnum,T>(this IGetRegisterEvent registerEvent,TEnum e, Func<T, Task> onEvent) where TEnum : IConvertible
            => registerEvent.GetArchitecture().RegisterEvent_Async(e,onEvent);


#if UNITY_2021_1_OR_NEWER
        public static IUnRegister RegisterEvent_Async_Unity<T>(this IGetRegisterEvent registerEvent, Func<T, YieldTask> onEvent)
            => registerEvent.GetArchitecture().RegisterEvent_Async_Unity(onEvent);

        public static IUnRegister RegisterEvent_Async_Unity<T>(this IGetRegisterEvent registerEvent, string name, Func<T, YieldTask> onEvent)
            => registerEvent.GetArchitecture().RegisterEvent_Async_Unity(name, onEvent);

        public static IUnRegister RegisterEvent_Async_Unity<TEnum, T>(this IGetRegisterEvent registerEvent, TEnum e, Func<T, YieldTask> onEvent) where TEnum : IConvertible
            => registerEvent.GetArchitecture().RegisterEvent_Async_Unity(e, onEvent);
#endif
        public static IUnRegister RegisterEvent<T>(this IGetRegisterEvent registerEvent, string eventName, Action<T> onEvent)
            => registerEvent.GetArchitecture().RegisterEvent(eventName, onEvent);

        public static IUnRegister RegisterEvent<TEnum,T>(this IGetRegisterEvent registerEvent, TEnum eventEnum, Action<T> onEvent) where TEnum : IConvertible
            => registerEvent.GetArchitecture().RegisterEvent(eventEnum, onEvent);

        public static void UnRegisterEvent<T>(this IGetRegisterEvent registerEvent, Action<T> onEvent = null)
            => registerEvent.GetArchitecture().UnRegisterEvent(onEvent);

        public static void UnRegisterEvent<T>(this IGetRegisterEvent registerEvent, string eventName, Action<T> onEvent)
            => registerEvent.GetArchitecture().UnRegisterEvent(eventName, onEvent);

        public static void UnRegisterEvent<TEnum,T>(this IGetRegisterEvent registerEvent, TEnum eventEnum, Action<T> onEvent) where TEnum : IConvertible
            => registerEvent.GetArchitecture().UnRegisterEvent(eventEnum, onEvent);

        public static void UnRegisterEvent_Async<T>(this IGetRegisterEvent registerEvent, Func<T, Task> onEvent)
            => registerEvent.GetArchitecture().UnRegisterEvent_Async(onEvent);

        public static void UnRegisterEvent_Async<T>(this IGetRegisterEvent registerEvent,string name, Func<T, Task> onEvent)
           => registerEvent.GetArchitecture().UnRegisterEvent_Async(name,onEvent);

        public static void UnRegisterEvent_Async<TEnum,T>(this IGetRegisterEvent registerEvent,TEnum e, Func<T, Task> onEvent) where TEnum : IConvertible
           => registerEvent.GetArchitecture().UnRegisterEvent_Async(e,onEvent);

#if UNITY_2021_1_OR_NEWER
        public static void UnRegisterEvent_Async_Unity<T>(this IGetRegisterEvent registerEvent, Func<T, YieldTask> onEvent)
            => registerEvent.GetArchitecture().UnRegisterEvent_Async_Unity(onEvent);

        public static void UnRegisterEvent_Async_Unity<T>(this IGetRegisterEvent registerEvent, string name, Func<T, YieldTask> onEvent)
           => registerEvent.GetArchitecture().UnRegisterEvent_Async_Unity(name, onEvent);

        public static void UnRegisterEvent_Async_Unity<TEnum, T>(this IGetRegisterEvent registerEvent, TEnum e, Func<T, YieldTask> onEvent) where TEnum : IConvertible
           => registerEvent.GetArchitecture().UnRegisterEvent_Async_Unity(e, onEvent);
#endif


        public static void SendEvent<T>(this ISendEvent SendEvent,T arg = default)
            => SendEvent.GetArchitecture().SendEvent(arg);

        public static void SendEvent<T>(this ISendEvent SendEvent,string eventName, T arg = default)
            => SendEvent.GetArchitecture().SendEvent(eventName,arg);

        public static void SendEvent<TEnum,T>(this ISendEvent SendEvent, TEnum eventEnum, T arg = default) where TEnum : IConvertible
            => SendEvent.GetArchitecture().SendEvent(eventEnum, arg);

        public static async Task SendEvent_Async<T>(this IGetRegisterEvent registerEvent, T t = default)
            => await registerEvent.GetArchitecture().SendEvent_Async(t);

        public static async Task SendEvent_Async<T>(this IGetRegisterEvent registerEvent,string name, T t = default)
            => await registerEvent.GetArchitecture().SendEvent_Async(name,t);

        public static async Task SendEvent_Async<TEnum, T>(this IGetRegisterEvent registerEvent,TEnum e, T t = default) where TEnum : IConvertible
            => await registerEvent.GetArchitecture().SendEvent_Async(e,t);

#if UNITY_2021_1_OR_NEWER
        public static async YieldTask SendEvent_Async_Unity<TEnum,T>(this IGetRegisterEvent registerEvent,TEnum e, T t = default) where TEnum : IConvertible
            => await registerEvent.GetArchitecture().SendEvent_Async_Unity(e,t);

        public static async YieldTask SendEvent_Async_Unity<T>(this IGetRegisterEvent registerEvent, T t = default)
          => await registerEvent.GetArchitecture().SendEvent_Async_Unity(t);

        public static async YieldTask SendEvent_Async_Unity<T>(this IGetRegisterEvent registerEvent, string name, T t = default)
            => await registerEvent.GetArchitecture().SendEvent_Async_Unity(name, t);
#endif
    }

    /// <summary>
    /// 命令中心拓展
    /// </summary>
    public static partial class CommandExtension
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

        public static TResult SendCommand<TCommand, TResult>(this ISendCommand center) where TCommand : ICommand<TResult>, new()
        {
            ICommand<TResult> command = new TCommand();
            return center.GetArchitecture().SendCommand(command);
        }
    }

    public static class QueryExtension
    {
        public static T Query<T>(this IGetQuery actor, IQuery<T> query)
           => actor.GetArchitecture().SendQuery(query);

        public static T Query<TQuery, T>(this IGetQuery actor) where TQuery : IQuery<T>,new()
        {
            IQuery<T> query = new TQuery();
            return actor.GetArchitecture().SendQuery(query);
        }
    }

    public static class ConfigExtension
    {
        public static T GetConfig<T>(this IGetConfig getconfig,string path) where T : UnityEngine.Object
        {
            return GetConfig(getconfig,path) as T;
        }

        public static UnityEngine.Object GetConfig(this IGetConfig getconfig, string path)
        {
            return getconfig.GetArchitecture().TableConfig.GetConfig(path);
        }

        public static string GetConfigByFile(this IGetConfig getConfig, string path)
        {
            return getConfig.GetArchitecture().TableConfig.GetConfigByFile(path);
        }
    }   

    /// <summary>
    /// 配表自动反序列化，当Model层的实体类标记AutoInjectConfig特性后，Model可进行自动反序列化，同步配表，在架构中重写BuildArchitectureTable添加对应数据后，可使用该特性进行自动化处理
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class ConfigDeSerializeFieldAttribute : Attribute
    {
        internal string pathOrName;
        internal string fieldName;
        internal bool property;

        /// <summary>
        /// 标记该特性，进行反序列化
        /// </summary>
        /// <param name="pathOrName">配表路径/标识(应与架构配置一致)</param>
        /// <param name="property">配表的参数是否是属性?</param>
        public ConfigDeSerializeFieldAttribute(string pathOrName, bool property = false) 
        {
            this.pathOrName = pathOrName;
            this.property = property;
        }

        /// <summary>
        /// 标记该特性，进行反序列化
        /// </summary>
        /// <param name="fieldName">如果配表字段名称与Model里面的标记字段不同，请输入配表的字段名称</param>
        /// <param name="pathOrName">配表路径/标识(应与架构配置一致)</param>
        /// <param name="property">配表的参数是否是属性?</param>
        public ConfigDeSerializeFieldAttribute(string fieldName, string pathOrName,bool property) : this(pathOrName,property)
        {
            this.fieldName = fieldName;
        }
    }

    /// <summary>
    /// 标记该特性，Model将自动标记为可自动进行配表反序列化的实体类
    /// </summary>
    public sealed class AutoInjectConfigAttribute : Attribute
    { 

    }
}
