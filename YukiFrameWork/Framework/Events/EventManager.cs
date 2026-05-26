///=====================================================
/// - FileName:      EventManager.cs
/// - NameSpace:     YukiFrameWork.Events
/// - Description:   全局事件管理
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

    internal sealed class EventChannelTable<TKey> where TKey : notnull
    {
        private readonly Dictionary<TKey, IEasyEvent> pools = new Dictionary<TKey, IEasyEvent>();
        private readonly EventRegisterType registerType;

        internal EventChannelTable(EventRegisterType registerType)
        {
            this.registerType = registerType;
        }

        internal T GetOrAdd<T>(TKey key, Func<EventChannelKey> keyFactory) where T : IEasyEvent, new()
        {
            if (!pools.TryGetValue(key, out var value))
            {
                value = new T();
                pools[key] = value;
#if UNITY_2022_1_OR_NEWER
                value.RegisterType = registerType;
#endif
                EventDiagnostics.BindChannel(value, keyFactory());
            }

            return (T)value;
        }

        internal T Get<T>(TKey key) where T : IEasyEvent, new()
        {
            if (!pools.TryGetValue(key, out var value))
                return default;
            return (T)value;
        }

        internal void RemoveByInstance(IUnRegister unRegister, Action<TKey, IEasyEvent> onRemove)
        {
            var key = pools.Keys.FirstOrDefault(x => pools[x] == unRegister);
            if (key != null && pools.TryGetValue(key, out var easyEvent))
                onRemove(key, easyEvent);
        }
    }

    public class EventInfo
    {
        private readonly EventChannelTable<Type> typeEvents = new EventChannelTable<Type>(EventRegisterType.Type);
        private readonly EventChannelTable<Type> asyncEvents = new EventChannelTable<Type>(EventRegisterType.AsyncType);
        private readonly EventChannelTable<string> stringEvents = new EventChannelTable<string>(EventRegisterType.String);
        private readonly EventChannelTable<Enum> enumEvents = new EventChannelTable<Enum>(EventRegisterType.Enum);

        internal T GetOrAdd<T>() where T : IEasyEvent, new()
            => typeEvents.GetOrAdd<T>(typeof(T), () => EventChannelKey.ForEasyEvent(typeof(T), EventRegisterType.Type));

        internal T GetOrAdd_Async<T>() where T : IEasyEvent, new()
            => asyncEvents.GetOrAdd<T>(typeof(T), () => EventChannelKey.ForEasyEvent(typeof(T), EventRegisterType.AsyncType));

        internal T GetOrAdd<T>(string name) where T : IEasyEvent, new()
            => stringEvents.GetOrAdd<T>(name, () => EventChannelKey.ForEasyEvent(typeof(T), EventRegisterType.String, name));

        internal T GetOrAdd<T>(Enum e) where T : IEasyEvent, new()
            => enumEvents.GetOrAdd<T>(e, () => EventChannelKey.ForEasyEvent(typeof(T), EventRegisterType.Enum, e?.ToString()));

        internal T Get<T>() where T : IEasyEvent, new()
            => typeEvents.Get<T>(typeof(T));

        internal T Get_Async<T>() where T : IEasyEvent, new()
            => asyncEvents.Get<T>(typeof(T));

        internal T Get<T>(string name) where T : IEasyEvent, new()
            => stringEvents.Get<T>(name);

        internal T Get<T>(Enum e) where T : IEasyEvent, new()
            => enumEvents.Get<T>(e);

#if UNITY_2022_1_OR_NEWER
        internal void RemoveEvent(IUnRegister unRegister)
        {
            switch (unRegister.RegisterType)
            {
                case EventRegisterType.Type:
                    typeEvents.RemoveByInstance(unRegister, (_, easyEvent) => easyEvent?.UnRegisterAllEvent());
                    break;
                case EventRegisterType.String:
                    stringEvents.RemoveByInstance(unRegister, (key, easyEvent) =>
                    {
                        if (!key.IsNullOrEmpty())
                            easyEvent?.UnRegisterAllEvent();
                    });
                    break;
                case EventRegisterType.Enum:
                    enumEvents.RemoveByInstance(unRegister, (_, easyEvent) => easyEvent?.UnRegisterAllEvent());
                    break;
                case EventRegisterType.AsyncType:
                    asyncEvents.RemoveByInstance(unRegister, (_, easyEvent) => easyEvent?.UnRegisterAllEvent());
                    break;
            }

            unRegister?.UnRegisterAllEvent();
        }
#endif
    }

    public static class EventManager
    {
        private static readonly EventInfo eventInfo = new EventInfo();

        public static EventInfo Root => eventInfo;

#if UNITY_EDITOR
        public static IReadOnlyList<EventChannelSnapshot> GetChannelSnapshots()
            => EventDiagnostics.GetChannelSnapshots();

        public static IReadOnlyList<EventDiagnosticRecord> GetDiagnosticHistory()
            => EventDiagnostics.GetHistory();

        public static int GetTotalSubscriptionCount()
            => EventDiagnostics.GetTotalSubscriptionCount();

        public static int GetRiskSubscriptionCount()
            => EventDiagnostics.GetRiskSubscriptionCount();

        public static IReadOnlyList<EventLifecycleReport> GetLifecycleReports()
            => EventDiagnostics.GetLifecycleReports();

        public static void ClearDiagnosticHistory()
            => EventDiagnostics.ClearHistory();
#endif

        public static void Send<T>(this T arg) where T : IEventArgs
        {
            SendEvent(arg);
        }

        public static void Send<T>(this T arg, string eventName) where T : IEventArgs
        {
            SendEvent(eventName, arg);
        }

        public static void Send<T>(this T arg, Enum en) where T : IEventArgs
        {
            SendEvent(en, arg);
        }

        public async static Task Send_Task<T>(this T arg) where T : IEventArgs
        {
            await SendEvent_Task(arg);
        }

        public static IUnRegister AddListener<T>(Action<T> e) where T : IEventArgs
        {
            return eventInfo.GetOrAdd<EasyEvent<T>>().RegisterEvent(e);
        }

        public static void RemoveListener<T>(Action<T> e) where T : IEventArgs
        {
            eventInfo.Get<EasyEvent<T>>()?.UnRegister(e);
        }

        public static void RemoveAllListeners<T>() where T : IEventArgs
        {
            eventInfo.Get<EasyEvent<T>>()?.UnRegisterAllEvent();
        }

        public static IUnRegister AddListener_Task<T>(Func<T, Task> e) where T : IEventArgs
        {
            return eventInfo.GetOrAdd_Async<AsyncEasyEvent<T>>().RegisterEvent(e);
        }

        public static void RemoveListener_Task<T>(Func<T, Task> e) where T : IEventArgs
        {
            eventInfo.Get_Async<AsyncEasyEvent<T>>()?.UnRegister(e);
        }

        public static void RemoveAllListeners_Task<T>() where T : IEventArgs
        {
            eventInfo.Get_Async<AsyncEasyEvent<T>>()?.UnRegisterAllEvent();
        }

        public static void SendEvent<T>(T e = default, bool error = false) where T : IEventArgs
        {
            EasyEvent<T> easyEvent = eventInfo.Get<EasyEvent<T>>();
            Send(easyEvent, e, "事件没有注册，请检查 Event Type:" + typeof(T), error);
        }

        public static async Task SendEvent_Task<T>(T e = default, bool error = false) where T : IEventArgs
        {
            AsyncEasyEvent<T> easyEvent = eventInfo.Get_Async<AsyncEasyEvent<T>>();
            await Send(easyEvent, e, "事件没有注册，请检查 Event Type:" + typeof(T), error);
        }

        public static IUnRegister AddListener<T>(string name, Action<T> e) where T : IEventArgs
        {
            return eventInfo.GetOrAdd<EasyEvent<T>>(name).RegisterEvent(e);
        }

        public static void RemoveListener<T>(string name, Action<T> e) where T : IEventArgs
        {
            eventInfo.Get<EasyEvent<T>>(name)?.UnRegister(e);
        }

        public static void RemoveAllListeners<T>(string name) where T : IEventArgs
        {
            eventInfo.Get<EasyEvent<T>>(name)?.UnRegisterAllEvent();
        }

        public static void SendEvent<T>(string name, T e = default, bool error = false) where T : IEventArgs
        {
            EasyEvent<T> easyEvent = eventInfo.Get<EasyEvent<T>>(name);
            Send(easyEvent, e, "事件没有注册，请检查 Event Name:" + name, error);
        }

        public static IUnRegister AddListener<T>(Enum en, Action<T> e) where T : IEventArgs
        {
            return eventInfo.GetOrAdd<EasyEvent<T>>(en).RegisterEvent(e);
        }

        public static void RemoveListener<T>(Enum en, Action<T> e) where T : IEventArgs
        {
            eventInfo.Get<EasyEvent<T>>(en)?.UnRegister(e);
        }

        public static void RemoveAllListeners<T>(Enum name) where T : IEventArgs
        {
            eventInfo.Get<EasyEvent<T>>(name)?.UnRegisterAllEvent();
        }

        public static void SendEvent<T>(Enum en, T e = default, bool error = false) where T : IEventArgs
        {
            EasyEvent<T> easyEvent = eventInfo.Get<EasyEvent<T>>(en);
            Send(easyEvent, e, "事件没有注册，请检查 Event Enum:" + en, error);
        }

        private static void Send<T>(EasyEvent<T> easyEvent, T t, string error, bool isError) where T : IEventArgs
        {
            if (easyEvent == default)
            {
                if (isError)
                    throw new Exception(error);
                return;
            }
            easyEvent.SendEvent(t);
        }

        private static async Task Send<T>(AsyncEasyEvent<T> easyEvent, T t, string error, bool isError) where T : IEventArgs
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
