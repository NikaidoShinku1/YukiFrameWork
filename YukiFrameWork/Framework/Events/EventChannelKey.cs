using System;

namespace YukiFrameWork.Events
{
    public readonly struct EventChannelKey : IEquatable<EventChannelKey>
    {
        public EventRegisterType RegisterType { get; }
        public Type ArgType { get; }
        public string Identifier { get; }

        public EventChannelKey(EventRegisterType registerType, Type argType, string identifier = null)
        {
            RegisterType = registerType;
            ArgType = argType;
            Identifier = identifier;
        }

        public static EventChannelKey ForEasyEvent(Type easyEventType, EventRegisterType registerType, string identifier = null)
        {
            var argType = ResolveEventArgType(easyEventType);
            return new EventChannelKey(registerType, argType, identifier);
        }

        public static EventChannelKey ForType(Type argType)
            => new EventChannelKey(EventRegisterType.Type, argType);

        public static EventChannelKey ForAsyncType(Type argType)
            => new EventChannelKey(EventRegisterType.AsyncType, argType);

        public static EventChannelKey ForString(Type argType, string name)
            => new EventChannelKey(EventRegisterType.String, argType, name);

        public static EventChannelKey ForEnum(Type argType, Enum value)
            => new EventChannelKey(EventRegisterType.Enum, argType, value?.ToString());

        private static Type ResolveEventArgType(Type easyEventType)
        {
            var current = easyEventType;
            while (current != null)
            {
                if (current.IsGenericType)
                {
                    var genericDefinition = current.GetGenericTypeDefinition();
                    if (genericDefinition == typeof(YukiFrameWork.EasyEvent<>)
                        || genericDefinition == typeof(YukiFrameWork.AsyncEasyEvent<>))
                    {
                        return current.GetGenericArguments()[0];
                    }
                }

                current = current.BaseType;
            }

            return easyEventType;
        }

        public string DisplayName
        {
            get
            {
                var argName = ArgType?.Name ?? "Unknown";
                return RegisterType switch
                {
                    EventRegisterType.Type => argName,
                    EventRegisterType.AsyncType => $"[Async] {argName}",
                    EventRegisterType.String => $"{argName} ({Identifier})",
                    EventRegisterType.Enum => $"{argName} ({Identifier})",
                    _ => argName
                };
            }
        }

        public bool Equals(EventChannelKey other)
            => RegisterType == other.RegisterType
               && ArgType == other.ArgType
               && Identifier == other.Identifier;

        public override bool Equals(object obj)
            => obj is EventChannelKey other && Equals(other);

        public override int GetHashCode()
            => HashCode.Combine(RegisterType, ArgType, Identifier);

        public override string ToString() => DisplayName;
    }
}
