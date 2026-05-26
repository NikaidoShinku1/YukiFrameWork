using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

namespace YukiFrameWork.Events
{
    public enum EventDiagnosticAction
    {
        Register,
        UnRegister,
        UnRegisterAll,
        LifecycleBind
    }

    public readonly struct EventSubscriptionSnapshot
    {
        public int Id { get; }
        public string HandlerName { get; }
        public string TargetTypeName { get; }
        public UnityEngine.Object TargetObject { get; }
        public bool IsAlive { get; }
        public EventLifecycleBindType LifecycleBind { get; }
        public UnityEngine.Object LifecycleOwner { get; }
        public EventLifecycleSafetyStatus SafetyStatus { get; }
        public string RegisterStackTrace { get; }
        public bool HasUnregisterRecord { get; }

        internal EventSubscriptionSnapshot(SubscriptionRecord record, EventLifecycleSafetyStatus status, bool hasUnregisterRecord)
        {
            Id = record.Id;
            HandlerName = record.Handler?.Method?.Name ?? "Unknown";
            var target = record.Handler?.Target;
            TargetObject = target as UnityEngine.Object;
            TargetTypeName = target == null
                ? "Static"
                : target is UnityEngine.Object unityObject
                    ? unityObject.GetType().Name
                    : target.GetType().Name;
            IsAlive = TargetObject == null || TargetObject;
            LifecycleBind = record.LifecycleBind;
            LifecycleOwner = record.LifecycleOwner;
            SafetyStatus = status;
            RegisterStackTrace = record.RegisterStackTrace;
            HasUnregisterRecord = hasUnregisterRecord;
        }

#if UNITY_EDITOR
        public EventSubscriptionSnapshot(
            int id,
            string handlerName,
            string targetTypeName,
            UnityEngine.Object targetObject,
            bool isAlive,
            EventLifecycleBindType lifecycleBind,
            UnityEngine.Object lifecycleOwner,
            EventLifecycleSafetyStatus safetyStatus,
            string registerStackTrace,
            bool hasUnregisterRecord)
        {
            Id = id;
            HandlerName = handlerName;
            TargetTypeName = targetTypeName;
            TargetObject = targetObject;
            IsAlive = isAlive;
            LifecycleBind = lifecycleBind;
            LifecycleOwner = lifecycleOwner;
            SafetyStatus = safetyStatus;
            RegisterStackTrace = registerStackTrace;
            HasUnregisterRecord = hasUnregisterRecord;
        }
#endif
    }

    public readonly struct EventChannelSnapshot
    {
        public EventChannelKey Key { get; }
        public int ListenerCount { get; }
        public IReadOnlyList<EventSubscriptionSnapshot> Subscriptions { get; }

        public EventChannelSnapshot(EventChannelKey key, int listenerCount, IReadOnlyList<EventSubscriptionSnapshot> subscriptions)
        {
            Key = key;
            ListenerCount = listenerCount;
            Subscriptions = subscriptions;
        }
    }

    public readonly struct EventDiagnosticRecord
    {
        public int SubscriptionId { get; }
        public EventDiagnosticAction Action { get; }
        public EventChannelKey ChannelKey { get; }
        public DateTime Timestamp { get; }
        public string HandlerName { get; }
        public string TargetTypeName { get; }
        public UnityEngine.Object TargetObject { get; }
        public int AffectedCount { get; }
        public EventLifecycleBindType LifecycleBind { get; }
        public string StackTrace { get; }

        internal EventDiagnosticRecord(
            int subscriptionId,
            EventDiagnosticAction action,
            EventChannelKey channelKey,
            Delegate handler,
            int affectedCount,
            EventLifecycleBindType lifecycleBind,
            string stackTrace)
        {
            SubscriptionId = subscriptionId;
            Action = action;
            ChannelKey = channelKey;
            Timestamp = DateTime.Now;
            HandlerName = handler?.Method?.Name ?? (affectedCount > 1 ? "All" : lifecycleBind != EventLifecycleBindType.None ? "Lifecycle" : "Unknown");
            var target = handler?.Target;
            TargetObject = target as UnityEngine.Object;
            TargetTypeName = target == null
                ? affectedCount > 1 ? "Multiple" : lifecycleBind != EventLifecycleBindType.None ? "Lifecycle" : "Static"
                : target is UnityEngine.Object unityObject
                    ? unityObject.GetType().Name
                    : target.GetType().Name;
            AffectedCount = affectedCount;
            LifecycleBind = lifecycleBind;
            StackTrace = stackTrace;
        }

#if UNITY_EDITOR
        public EventDiagnosticRecord(
            int subscriptionId,
            EventDiagnosticAction action,
            EventChannelKey channelKey,
            string handlerName,
            string targetTypeName,
            EventLifecycleBindType lifecycleBind,
            string stackTrace)
        {
            SubscriptionId = subscriptionId;
            Action = action;
            ChannelKey = channelKey;
            Timestamp = DateTime.Now;
            HandlerName = handlerName;
            TargetTypeName = targetTypeName;
            TargetObject = null;
            AffectedCount = 1;
            LifecycleBind = lifecycleBind;
            StackTrace = stackTrace;
        }
#endif
    }

    public readonly struct EventLifecycleReport
    {
        public int SubscriptionId { get; }
        public EventChannelKey ChannelKey { get; }
        public string HandlerName { get; }
        public string TargetTypeName { get; }
        public UnityEngine.Object TargetObject { get; }
        public bool IsActive { get; }
        public EventLifecycleBindType LifecycleBind { get; }
        public UnityEngine.Object LifecycleOwner { get; }
        public EventLifecycleSafetyStatus SafetyStatus { get; }
        public bool HasMatchingUnregister { get; }
        public string RegisterStackTrace { get; }
        public string UnregisterStackTrace { get; }
        public DateTime RegisterTime { get; }
        public DateTime? UnregisterTime { get; }
        public string AnalysisMessage { get; }

        internal EventLifecycleReport(SubscriptionRecord record, EventLifecycleSafetyStatus status, bool hasMatchingUnregister, string message)
        {
            SubscriptionId = record.Id;
            ChannelKey = record.ChannelKey;
            HandlerName = record.Handler?.Method?.Name ?? "Unknown";
            var target = record.Handler?.Target;
            TargetObject = target as UnityEngine.Object;
            TargetTypeName = target == null
                ? "Static"
                : target is UnityEngine.Object unityObject
                    ? unityObject.GetType().Name
                    : target.GetType().Name;
            IsActive = record.IsActive;
            LifecycleBind = record.LifecycleBind;
            LifecycleOwner = record.LifecycleOwner;
            SafetyStatus = status;
            HasMatchingUnregister = hasMatchingUnregister;
            RegisterStackTrace = record.RegisterStackTrace;
            UnregisterStackTrace = record.UnregisterStackTrace;
            RegisterTime = record.RegisterTime;
            UnregisterTime = record.UnregisterTime;
            AnalysisMessage = message;
        }

#if UNITY_EDITOR
        public EventLifecycleReport(
            int subscriptionId,
            EventChannelKey channelKey,
            string handlerName,
            string targetTypeName,
            UnityEngine.Object targetObject,
            bool isActive,
            EventLifecycleBindType lifecycleBind,
            UnityEngine.Object lifecycleOwner,
            EventLifecycleSafetyStatus safetyStatus,
            bool hasMatchingUnregister,
            string registerStackTrace,
            string unregisterStackTrace,
            DateTime registerTime,
            DateTime? unregisterTime,
            string analysisMessage)
        {
            SubscriptionId = subscriptionId;
            ChannelKey = channelKey;
            HandlerName = handlerName;
            TargetTypeName = targetTypeName;
            TargetObject = targetObject;
            IsActive = isActive;
            LifecycleBind = lifecycleBind;
            LifecycleOwner = lifecycleOwner;
            SafetyStatus = safetyStatus;
            HasMatchingUnregister = hasMatchingUnregister;
            RegisterStackTrace = registerStackTrace;
            UnregisterStackTrace = unregisterStackTrace;
            RegisterTime = registerTime;
            UnregisterTime = unregisterTime;
            AnalysisMessage = analysisMessage;
        }
#endif
    }

    internal interface IEventChannelOwner
    {
        EventChannelKey ChannelKey { get; set; }
        int GetListenerCount();
        IEnumerable<Delegate> GetHandlers();
    }

    internal sealed class SubscriptionRecord
    {
        public int Id { get; set; }
        public EventChannelKey ChannelKey { get; set; }
        public Delegate Handler { get; set; }
        public string RegisterStackTrace { get; set; }
        public DateTime RegisterTime { get; set; }
        public EventLifecycleBindType LifecycleBind { get; set; }
        public UnityEngine.Object LifecycleOwner { get; set; }
        public bool IsActive { get; set; } = true;
        public string UnregisterStackTrace { get; set; }
        public DateTime? UnregisterTime { get; set; }
        public EventDiagnosticAction? CloseAction { get; set; }
    }

#if UNITY_EDITOR
    public static class EventDiagnostics
    {
        private const int MaxHistoryCount = 2048;

        private static readonly Dictionary<EventChannelKey, List<int>> ActiveByChannel =
            new Dictionary<EventChannelKey, List<int>>();

        private static readonly Dictionary<int, SubscriptionRecord> Records = new Dictionary<int, SubscriptionRecord>();
        private static readonly List<EventDiagnosticRecord> History = new List<EventDiagnosticRecord>();
        private static int nextSubscriptionId = 1;

        public static event Action Changed;

        internal static void BindChannel(IEasyEvent easyEvent, EventChannelKey key)
        {
            if (easyEvent is IEventChannelOwner owner)
                owner.ChannelKey = key;
        }

        internal static void NotifyRegistered(EventChannelKey key, Delegate handler)
        {
            if (handler == null) return;

            var record = new SubscriptionRecord
            {
                Id = nextSubscriptionId++,
                ChannelKey = key,
                Handler = handler,
                RegisterStackTrace = CaptureStackTrace(),
                RegisterTime = DateTime.Now
            };

            Records[record.Id] = record;
            if (!ActiveByChannel.TryGetValue(key, out var ids))
            {
                ids = new List<int>();
                ActiveByChannel[key] = ids;
            }

            ids.Add(record.Id);
            AddHistory(record.Id, EventDiagnosticAction.Register, key, handler, 1, EventLifecycleBindType.None, record.RegisterStackTrace);
            Changed?.Invoke();
        }

        internal static void NotifyUnregistered(EventChannelKey key, Delegate handler)
        {
            if (handler == null) return;

            var record = FindActiveRecord(key, handler);
            if (record != null)
                CloseRecord(record, EventDiagnosticAction.UnRegister, CaptureStackTrace());
            else
                AddHistory(0, EventDiagnosticAction.UnRegister, key, handler, 1, EventLifecycleBindType.None, CaptureStackTrace());

            Changed?.Invoke();
        }

        internal static void NotifyUnregisteredAll(EventChannelKey key, IEnumerable<Delegate> handlers)
        {
            var count = 0;
            var stackTrace = CaptureStackTrace();
            if (ActiveByChannel.TryGetValue(key, out var ids))
            {
                var snapshot = ids.ToArray();
                foreach (var id in snapshot)
                {
                    if (!Records.TryGetValue(id, out var record) || !record.IsActive)
                        continue;

                    count++;
                    CloseRecord(record, EventDiagnosticAction.UnRegisterAll, stackTrace);
                }
            }
            else if (handlers != null)
            {
                foreach (var handler in handlers)
                {
                    if (handler != null)
                        count++;
                }
            }

            if (count == 0)
                AddHistory(0, EventDiagnosticAction.UnRegisterAll, key, null, count, EventLifecycleBindType.None, stackTrace);

            Changed?.Invoke();
        }

        internal static void NotifyLifecycleBind(IUnRegister register, EventLifecycleBindType bindType, UnityEngine.Object owner)
        {
            if (register is not IEventChannelOwner channelOwner)
                return;

            var key = channelOwner.ChannelKey;
            if (!ActiveByChannel.TryGetValue(key, out var ids) || ids.Count == 0)
                return;

            for (var i = ids.Count - 1; i >= 0; i--)
            {
                if (!Records.TryGetValue(ids[i], out var record) || !record.IsActive)
                    continue;

                record.LifecycleBind = bindType;
                record.LifecycleOwner = owner;
                AddHistory(record.Id, EventDiagnosticAction.LifecycleBind, key, record.Handler, 1, bindType, CaptureStackTrace());
                break;
            }

            Changed?.Invoke();
        }

        public static IReadOnlyList<EventChannelSnapshot> GetChannelSnapshots()
        {
            var snapshots = new List<EventChannelSnapshot>();
            foreach (var pair in ActiveByChannel)
            {
                var subscriptions = new List<EventSubscriptionSnapshot>();
                foreach (var id in pair.Value)
                {
                    if (!Records.TryGetValue(id, out var record) || !record.IsActive)
                        continue;

                    var status = EvaluateSafety(record, out _);
                    subscriptions.Add(new EventSubscriptionSnapshot(record, status, record.UnregisterTime.HasValue));
                }

                snapshots.Add(new EventChannelSnapshot(pair.Key, subscriptions.Count, subscriptions));
            }

            snapshots.Sort((a, b) => string.Compare(a.Key.DisplayName, b.Key.DisplayName, StringComparison.Ordinal));
            return snapshots;
        }

        public static IReadOnlyList<EventLifecycleReport> GetLifecycleReports()
        {
            var reports = new List<EventLifecycleReport>();
            foreach (var record in Records.Values)
            {
                var status = EvaluateSafety(record, out var message);
                reports.Add(new EventLifecycleReport(
                    record,
                    status,
                    record.UnregisterTime.HasValue,
                    message));
            }

            reports.Sort((a, b) =>
            {
                var activeCompare = b.IsActive.CompareTo(a.IsActive);
                return activeCompare != 0 ? activeCompare : b.RegisterTime.CompareTo(a.RegisterTime);
            });
            return reports;
        }

        public static IReadOnlyList<EventDiagnosticRecord> GetHistory()
            => History;

        public static int GetTotalSubscriptionCount()
        {
            var total = 0;
            foreach (var pair in ActiveByChannel)
            {
                foreach (var id in pair.Value)
                {
                    if (Records.TryGetValue(id, out var record) && record.IsActive)
                        total++;
                }
            }

            return total;
        }

        public static int GetRiskSubscriptionCount()
        {
            var total = 0;
            foreach (var pair in ActiveByChannel)
            {
                foreach (var id in pair.Value)
                {
                    if (!Records.TryGetValue(id, out var record) || !record.IsActive)
                        continue;

                    var status = EvaluateSafety(record, out _);
                    if (status == EventLifecycleSafetyStatus.ActiveOrphanRisk
                        || status == EventLifecycleSafetyStatus.LeakSuspect)
                        total++;
                }
            }

            return total;
        }

        public static void ClearHistory()
        {
            History.Clear();
            Changed?.Invoke();
        }

        public static void ResetAll()
        {
            ActiveByChannel.Clear();
            Records.Clear();
            History.Clear();
            nextSubscriptionId = 1;
            Changed?.Invoke();
        }

        private static SubscriptionRecord FindActiveRecord(EventChannelKey key, Delegate handler)
        {
            if (!ActiveByChannel.TryGetValue(key, out var ids))
                return null;

            for (var i = ids.Count - 1; i >= 0; i--)
            {
                if (!Records.TryGetValue(ids[i], out var record) || !record.IsActive)
                    continue;
                if (record.Handler == handler)
                    return record;
            }

            return null;
        }

        private static void CloseRecord(SubscriptionRecord record, EventDiagnosticAction action, string stackTrace)
        {
            record.IsActive = false;
            record.UnregisterStackTrace = stackTrace;
            record.UnregisterTime = DateTime.Now;
            record.CloseAction = action;

            if (ActiveByChannel.TryGetValue(record.ChannelKey, out var ids))
            {
                ids.Remove(record.Id);
                if (ids.Count == 0)
                    ActiveByChannel.Remove(record.ChannelKey);
            }

            AddHistory(record.Id, action, record.ChannelKey, record.Handler, 1, record.LifecycleBind, stackTrace);
        }

        private static EventLifecycleSafetyStatus EvaluateSafety(SubscriptionRecord record, out string message)
        {
            if (!record.IsActive)
            {
                message = record.CloseAction switch
                {
                    EventDiagnosticAction.UnRegister => "Matched manual unregister call.",
                    EventDiagnosticAction.UnRegisterAll => "Cleared by UnRegisterAllEvent.",
                    EventDiagnosticAction.LifecycleBind => "Closed by lifecycle binding.",
                    _ => record.UnregisterTime.HasValue ? "Subscription closed." : "Subscription closed without explicit unregister trace."
                };

                return record.CloseAction switch
                {
                    EventDiagnosticAction.UnRegister => EventLifecycleSafetyStatus.UnregisteredManual,
                    EventDiagnosticAction.UnRegisterAll => EventLifecycleSafetyStatus.UnregisteredAll,
                    _ when record.LifecycleBind != EventLifecycleBindType.None => EventLifecycleSafetyStatus.UnregisteredByLifecycle,
                    _ => EventLifecycleSafetyStatus.UnregisteredManual
                };
            }

            if (record.Handler?.Target == null)
            {
                message = "Static handler is active. Ensure manual unregister if needed.";
                return EventLifecycleSafetyStatus.ActiveStatic;
            }

            if (record.TargetIsDestroyed())
            {
                message = "Target object destroyed but subscription is still active. Potential leak.";
                return EventLifecycleSafetyStatus.LeakSuspect;
            }

            switch (record.LifecycleBind)
            {
                case EventLifecycleBindType.GameObjectDestroy:
                    message = record.LifecycleOwner
                        ? "Bound to GameObject destroy. Will auto unregister when owner is destroyed."
                        : "Bound to GameObject destroy, but lifecycle owner reference is missing.";
                    return EventLifecycleSafetyStatus.ActiveBoundDestroy;
                case EventLifecycleBindType.GameObjectDisable:
                    message = record.LifecycleOwner
                        ? "Bound to GameObject disable. Will auto unregister when owner is disabled."
                        : "Bound to GameObject disable, but lifecycle owner reference is missing.";
                    return EventLifecycleSafetyStatus.ActiveBoundDisable;
                case EventLifecycleBindType.SceneUnload:
                    message = "Bound to scene unload. Will auto unregister when scene unloads.";
                    return EventLifecycleSafetyStatus.ActiveBoundScene;
            }

            message = "Active subscription without lifecycle binding or manual unregister detected.";
            return EventLifecycleSafetyStatus.ActiveOrphanRisk;
        }

        private static bool TargetIsDestroyed(this SubscriptionRecord record)
        {
            var target = record.Handler?.Target as UnityEngine.Object;
            return target != null && !target;
        }

        private static void AddHistory(
            int subscriptionId,
            EventDiagnosticAction action,
            EventChannelKey key,
            Delegate handler,
            int affectedCount,
            EventLifecycleBindType lifecycleBind,
            string stackTrace)
        {
            History.Add(new EventDiagnosticRecord(subscriptionId, action, key, handler, affectedCount, lifecycleBind, stackTrace));
            if (History.Count > MaxHistoryCount)
                History.RemoveAt(0);
        }

        private static string CaptureStackTrace()
        {
            var stack = new StackTrace(3, true);
            return stack.ToString();
        }
    }
#else
    public static class EventDiagnostics
    {
        internal static void BindChannel(IEasyEvent easyEvent, EventChannelKey key) { }
        internal static void NotifyRegistered(EventChannelKey key, Delegate handler) { }
        internal static void NotifyUnregistered(EventChannelKey key, Delegate handler) { }
        internal static void NotifyUnregisteredAll(EventChannelKey key, IEnumerable<Delegate> handlers) { }
        internal static void NotifyLifecycleBind(IUnRegister register, EventLifecycleBindType bindType, UnityEngine.Object owner) { }
    }
#endif
}
