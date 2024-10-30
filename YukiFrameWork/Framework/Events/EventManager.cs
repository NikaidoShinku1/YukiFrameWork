///=====================================================
/// - FileName:      EventListener.cs
/// - NameSpace:     YukiFrameWork.Events
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/9/20 16:55:32
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using YukiFrameWork.Extension;
namespace YukiFrameWork.Events
{
    public enum EventRegisterType
    {
        Type,
        String,
        Enum,
        AsyncType
    }
    public class EventInfo
	{
        private Dictionary<Type, IEasyEvent> eventPools = new Dictionary<Type, IEasyEvent>();
        private Dictionary<string, IEasyEvent> stringEventPools = new Dictionary<string, IEasyEvent>();
		private Dictionary<Enum, IEasyEvent> enumEventPools = new Dictionary<Enum, IEasyEvent>();
        private Dictionary<Type, IEasyEvent> asyncEventPools = new Dictionary<Type, IEasyEvent>();    
		internal T GetOrAdd<T>() where T : IEasyEvent,new()
		{
			if (!eventPools.TryGetValue(typeof(T), out var value))
			{
				value = new T();
				eventPools[typeof(T)] = value;
#if UNITY_2022_1_OR_NEWER
                value.RegisterType = EventRegisterType.Type;
#endif
            }          
			return (T)value;
		}

        internal T GetOrAdd_Async<T>() where T : IEasyEvent, new()
        {
            if (!asyncEventPools.TryGetValue(typeof(T), out var value))
            {
                value = new T();
                eventPools[typeof(T)] = value;
#if UNITY_2022_1_OR_NEWER
                value.RegisterType = EventRegisterType.AsyncType;
#endif
            }

            return (T)value;
        }

       
        internal T GetOrAdd<T>(string name) where T : IEasyEvent, new()
        {
            if (!stringEventPools.TryGetValue(name, out var value))
            {
                value = new T();
				stringEventPools[name] = value;
#if UNITY_2022_1_OR_NEWER
                value.RegisterType = EventRegisterType.String;
#endif
            }

            return (T)value;
        }

        internal T GetOrAdd<T>(Enum e) where T : IEasyEvent, new()
        {		
            if (!enumEventPools.TryGetValue(e, out var value))
            {
                value = new T();
				enumEventPools[e] = value;
#if UNITY_2022_1_OR_NEWER
                value.RegisterType = EventRegisterType.Enum;
#endif
            }
            return (T)value;
        }

        internal T Get<T>() where T : IEasyEvent, new()
        {
            if (!eventPools.TryGetValue(typeof(T), out var value))
            {
                return default;
            }          
            return (T)value;
        }

        internal T Get_Async<T>() where T : IEasyEvent, new()
        {
            if (!asyncEventPools.TryGetValue(typeof(T), out var value))
            {
                return default;
            }

            return (T)value;
        }
     

        internal T Get<T>(string name) where T : IEasyEvent, new()
        {
            if (!stringEventPools.TryGetValue(name, out var value))
            {
                return default;
            }

            return (T)value;
        }

        internal T Get<T>(Enum e) where T : IEasyEvent, new()
        {
            if (!enumEventPools.TryGetValue(e, out var value))
            {
                return default;
            }

            return (T)value;
        }
#if UNITY_2022_1_OR_NEWER
        internal void RemoveEvent(IUnRegister unRegister)
        {
            switch (unRegister.RegisterType)
            {
                case EventRegisterType.Type:
                    {
                        var key = eventPools.Keys.FirstOrDefault(x => eventPools[x] == unRegister);
                        if (key != null)
                            eventPools[key]?.UnRegisterAllEvent();
                    }
                    break;
                case EventRegisterType.String:
                    {
                        var key = stringEventPools.Keys.FirstOrDefault(x => stringEventPools[x] == unRegister);
                        if (!key.IsNullOrEmpty())
                            stringEventPools[key]?.UnRegisterAllEvent();
                    }
                    break;
                case EventRegisterType.Enum:
                    {
                        var key = enumEventPools.Keys.FirstOrDefault(x => enumEventPools[x] == unRegister);
                        if (key != null)
                            enumEventPools[key]?.UnRegisterAllEvent();
                    }
                    break;
                case EventRegisterType.AsyncType:
                    {
                        var key = asyncEventPools.Keys.FirstOrDefault(x => asyncEventPools[x] == unRegister);
                        if (key != null)
                            asyncEventPools[key]?.UnRegisterAllEvent();
                    }
                    break;             
            }
        }
#endif
    }

    public static class EventManager
    {
        private static EventInfo eventInfo;

        public static EventInfo Root => eventInfo;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Init_Inject_StaticMethod()
        {
            FrameworkConfigInfo info = Resources.Load<FrameworkConfigInfo>(nameof(FrameworkConfigInfo));

            if (info == null)
            {
                throw new Exception("框架配置丢失！请检查Resources是否生成配置");
            }

            List<string> assemblies = new List<string>();
            if (info.assemblies != null)
                assemblies.AddRange(info.assemblies);
            assemblies.Add(info.assembly);

            try
            {
                var InjectedNameSpace = "YukiFrameWork";
                var InjectedClazz = "Event_Builder";
                foreach (var ass in assemblies)
                {
                    
                    Assembly assembly = Assembly.Load(ass);
                    Type registerType = AssemblyHelper.GetType($"{InjectedNameSpace}.{InjectedClazz}", assembly);
                    if (registerType == null) continue;
                    IEventBuilder builder = Activator.CreateInstance(registerType) as IEventBuilder;
                    ///静态初始化
                    builder.StaticInit();
                }
            }
            catch(Exception ex) { LogKit.W(ex.ToString()); }
        }
        static EventManager()
        {
            eventInfo = new EventInfo();
        }

        public static void Send<T>(this T arg) where T : IEventArgs
        {
            EventManager.SendEvent(arg);
        }

        public static void Send<T>(this T arg,string eventName) where T : IEventArgs
        {
            EventManager.SendEvent(eventName, arg);
        }
        public static void Send<T>(this T arg,Enum en) where T : IEventArgs
        {
            EventManager.SendEvent(en, arg);
        }

        public async static Task Send_Task<T>(this T arg) where T : IEventArgs
        {
            await EventManager.SendEvent_Task(arg);
        }

        /// <summary>
        /// 注册事件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="e"></param>
        /// <returns></returns>
		public static IUnRegister AddListener<T>(Action<T> e) where T : IEventArgs
        {
            var easyEvent = eventInfo.GetOrAdd<EasyEvent<T>>().RegisterEvent(e);
            
            return easyEvent;
        }

        public static void RemoveListener<T>(Action<T> e) where T : IEventArgs
        {
            eventInfo.Get<EasyEvent<T>>()?.UnRegister(e);
        }

        public static void RemoveAllListeners<T>() where T : IEventArgs
        {
            eventInfo.Get<EasyEvent<T>>()?.UnRegisterAllEvent();
        }

        /// <summary>
        /// 注册异步事件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="e"></param>
        /// <returns></returns>
        public static IUnRegister AddListener_Task<T>(Func<T, Task> e) where T : IEventArgs
        {
            var easyEvent = eventInfo.GetOrAdd_Async<AsyncEasyEvent<T>>().RegisterEvent(e);
            return easyEvent;
        }

        public static void RemoveListener_Task<T>(Func<T, Task> e) where T : IEventArgs
        {
            eventInfo.Get_Async<AsyncEasyEvent<T>>()?.UnRegister(e);
        }

        public static void RemoveAllListeners_Task<T>() where T : IEventArgs
        {
            eventInfo.Get_Async<AsyncEasyEvent<T>>()?.UnRegisterAllEvent();
        }
        /// <summary>
        /// 发送事件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="e"></param>
        public static void SendEvent<T>(T e = default,bool error = false) where T : IEventArgs
		{
            EasyEvent<T> easyEvent = eventInfo.Get<EasyEvent<T>>();        
            Send(easyEvent, e, "事件没有注册，请检查 Event Type:" + typeof(T),error);
        }  

        /// <summary>
        /// 发送返回值为Task的异步事件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="e"></param>
        /// <returns></returns>
        public static async Task SendEvent_Task<T>(T e = default,bool error = false) where T : IEventArgs
        {
            AsyncEasyEvent<T> easyEvent = eventInfo.Get_Async<AsyncEasyEvent<T>>();
            await Send(easyEvent, e, "事件没有注册，请检查 Event Type:" + typeof(T),error);
        }
        /// <summary>
        /// 注册以字符串为标识的事件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public static IUnRegister AddListener<T>(string name,Action<T> e) where T : IEventArgs
        {
            var easyEvent = eventInfo.GetOrAdd<EasyEvent<T>>(name).RegisterEvent(e);
            return easyEvent;
        }

        public static void RemoveListener<T>(string name, Action<T> e) where T : IEventArgs
        {
            eventInfo.Get<EasyEvent<T>>(name)?.UnRegister(e);
            
        }

        public static void RemoveAllListeners<T>(string name) where T : IEventArgs
        {
            eventInfo.Get<EasyEvent<T>>(name)?.UnRegisterAllEvent();
        }

        /// <summary>
        /// 发送以字符串为标识的事件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="e"></param>
        public static void SendEvent<T>(string name,T e = default,bool error = false) where T : IEventArgs
        {
            EasyEvent<T> easyEvent = eventInfo.Get<EasyEvent<T>>(name);
            Send(easyEvent, e, "事件没有注册，请检查 Event Name:" + name,error);
        }

        /// <summary>
        /// 注册以枚举为标识的事件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="en"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public static IUnRegister AddListener<T>(Enum en,Action<T> e) where T : IEventArgs
        {
            var easyEvent = eventInfo.GetOrAdd<EasyEvent<T>>(en).RegisterEvent(e);
            return easyEvent;
        }

        public static void RemoveListener<T>(Enum en, Action<T> e) where T : IEventArgs
        {
            eventInfo.Get<EasyEvent<T>>(en)?.UnRegister(e);
        }

        public static void RemoveAllListeners<T>(Enum name) where T : IEventArgs
        {
            eventInfo.Get<EasyEvent<T>>(name)?.UnRegisterAllEvent();
        }

        /// <summary>
        /// 发送以枚举为标识的事件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="en"></param>
        /// <param name="e"></param>
        public static void SendEvent<T>(Enum en,T e = default,bool error = false) where T : IEventArgs
        {
            EasyEvent<T> easyEvent = eventInfo.Get<EasyEvent<T>>(en);
            Send(easyEvent, e,"事件没有注册，请检查 Event Enum:" + en,error);
        }

        private static void Send<T>(EasyEvent<T> easyEvent,T t,string error,bool isError) where T : IEventArgs
        {
            if (easyEvent == default)
            {
                if(isError)
                    throw new Exception(error);
                return;
            }
            easyEvent.SendEvent(t);
        }

        private static async Task Send<T>(AsyncEasyEvent<T> easyEvent, T t, string error,bool isError) where T : IEventArgs
        {
            if (easyEvent == default)
            {
                if (isError)
                    throw new Exception(error);
                return;
            }
            await easyEvent.SendEvent(t);           
        }     
    }


}
