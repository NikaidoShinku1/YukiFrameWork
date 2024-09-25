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
using YukiFrameWork.Events;

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
        void SendCommand<T>(T command) where T : ICommand;
        TResult SendCommand<TResult>(ICommand<TResult> command);
        TResult SendQuery<TResult>(IQuery<TResult> query);
        #region Framework Internal Gettter
        internal EasyContainer Container { get; }          
        #endregion
    }

    #region 层级规则
    public interface IGetModel : IGetArchitecture
    {

    }

    public interface ISendEvent
    {

    }

    public interface IGetRegisterEvent
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
       
        private ArchitectureTableConfig config;
#endregion

        internal bool IsInited = false;

        EasyContainer IArchitecture.Container => easyContainer;
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

        /// <summary>
        /// AbstractController会出现自动化事件注册因懒汉获取架构的方式没有执行注册，故提供该方法，在需要时可以使用
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        public static void BuildController<T>(this T t) where T : AbstractController
        {
            t.Build();        
        }
    }

    /// <summary>
    /// 事件系统拓展
    /// </summary>
    public static partial class EventSystemExtension
    {
        private const string message = "名称已过时，当继承IGetRegisterEvent接口时，";
        private const string defaultName = "this.AddListener方法";
        private const string asyncName = "this.AddListener_Async方法";
        private const string coroutineName = "this.AddListener_Coroutine方法";
        private const string removeDefaultName = "this.RemoveListener方法";
        private const string removeasyncName = "this.RemoveListener_Async方法";
        private const string removeCoroutineName = "this.RemoveListener_Coroutine方法";
        [Obsolete(message + defaultName)]
        public static IUnRegister RegisterEvent<T>(this IGetRegisterEvent registerEvent, Action<T> onEvent) where T : IEventArgs
            => EventManager.AddListener<T>(onEvent);

        public static IUnRegister AddListener<T>(this IGetRegisterEvent registerEvent, Action<T> onEvent) where T : IEventArgs
            => EventManager.AddListener<T>(onEvent);

        [Obsolete(message + asyncName)]
        public static IUnRegister RegisterEvent_Async<T>(this IGetRegisterEvent registerEvent, Func<T,Task> onEvent) where T : IEventArgs
            => EventManager.AddListener_Task<T>(onEvent);

        public static IUnRegister AddListener_Async<T>(this IGetRegisterEvent registerEvent, Func<T, Task> onEvent) where T : IEventArgs
            => EventManager.AddListener_Task<T>(onEvent);

        [Obsolete(message + defaultName)]
        public static IUnRegister RegisterEvent<T>(this IGetRegisterEvent registerEvent, string eventName, Action<T> onEvent) where T : IEventArgs
            => EventManager.AddListener<T>(eventName,onEvent);

        public static IUnRegister AddListener<T>(this IGetRegisterEvent registerEvent, string eventName, Action<T> onEvent) where T : IEventArgs
            => EventManager.AddListener<T>(eventName, onEvent);

        [Obsolete(message + defaultName)]
        public static IUnRegister RegisterEvent<T>(this IGetRegisterEvent registerEvent, Enum eventEnum, Action<T> onEvent) where T : IEventArgs
            => EventManager.AddListener<T>(eventEnum, onEvent);

        public static IUnRegister AddListener<T>(this IGetRegisterEvent registerEvent, Enum eventEnum, Action<T> onEvent) where T : IEventArgs
          => EventManager.AddListener<T>(eventEnum, onEvent);

        [Obsolete(message + removeDefaultName)]
        public static void UnRegisterEvent<T>(this IGetRegisterEvent registerEvent, Action<T> onEvent = null) where T : IEventArgs
            => EventManager.RemoveListener<T>(onEvent);

        public static void RemoveListener<T>(this IGetRegisterEvent registerEvent, Action<T> onEvent = null) where T : IEventArgs
           => EventManager.RemoveListener<T>(onEvent);

        [Obsolete(message + removeDefaultName)]
        public static void UnRegisterEvent<T>(this IGetRegisterEvent registerEvent,string name, Action<T> onEvent = null) where T : IEventArgs
           => EventManager.RemoveListener<T>(name,onEvent);

        public static void RemoveListener<T>(this IGetRegisterEvent registerEvent,string name, Action<T> onEvent = null) where T : IEventArgs
           => EventManager.RemoveListener<T>(name,onEvent);

        [Obsolete(message + removeDefaultName)]
        public static void UnRegisterEvent<T>(this IGetRegisterEvent registerEvent,Enum e, Action<T> onEvent = null) where T : IEventArgs
           => EventManager.RemoveListener<T>(e,onEvent);

        public static void RemoveListener<T>(this IGetRegisterEvent registerEvent, Enum e, Action<T> onEvent = null) where T : IEventArgs
           => EventManager.RemoveListener<T>(e,onEvent);

        [Obsolete(message + removeasyncName)]
        public static void UnRegisterEvent_Async<T>(this IGetRegisterEvent registerEvent, Func<T, Task> onEvent) where T : IEventArgs
            => EventManager.RemoveListener_Task<T>(onEvent);

        public static void RemoveListener_Async<T>(this IGetRegisterEvent registerEvent, Func<T, Task> onEvent) where T : IEventArgs
            => EventManager.RemoveListener_Task<T>(onEvent);


        public static void SendEvent<T>(this ISendEvent SendEvent, T arg = default) where T : IEventArgs
            => EventManager.SendEvent(arg);

        public static void SendEvent<T>(this ISendEvent SendEvent, string eventName, T arg = default) where T : IEventArgs
            => EventManager.SendEvent(eventName, arg);

        public static void SendEvent<T>(this ISendEvent SendEvent, Enum eventEnum, T arg = default) where T : IEventArgs
            => EventManager.SendEvent(eventEnum, arg);

        public static async Task SendEvent_Async<T>(this ISendEvent registerEvent, T t = default) where T : IEventArgs
            => await EventManager.SendEvent_Task(t);  

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
        internal JObjectType jObjectType;

        /// <summary>
        /// 标记该特性，进行反序列化
        /// </summary>
        /// <param name="pathOrName">配表路径/标识(应与架构配置一致)</param>
        /// <param name="property">配表的参数是否是属性?</param>
        public ConfigDeSerializeFieldAttribute(string pathOrName, bool property = false,JObjectType jObjectType = JObjectType.Object) 
        {
            this.pathOrName = pathOrName;
            this.property = property;
            this.jObjectType = jObjectType;
        }

        /// <summary>
        /// 标记该特性，进行反序列化
        /// </summary>
        /// <param name="fieldName">如果配表字段名称与Model里面的标记字段不同，请输入配表的字段名称</param>
        /// <param name="pathOrName">配表路径/标识(应与架构配置一致)</param>
        /// <param name="property">配表的参数是否是属性?</param>
        public ConfigDeSerializeFieldAttribute(string fieldName, string pathOrName,bool property, JObjectType jObjectType = JObjectType.Object) : this(pathOrName,property,jObjectType)
        {
            this.fieldName = fieldName;
        }
    }

    public enum JObjectType
    {
        Array,
        Object
    }

    /// <summary>
    /// 标记该特性，Model将自动标记为可自动进行配表反序列化的实体类
    /// </summary>
    public sealed class AutoInjectConfigAttribute : Attribute
    { 

    }
}
