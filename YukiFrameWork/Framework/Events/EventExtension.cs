using System;
#if NET_4_6
namespace YukiFrameWork
{
    public static class EventExtension 
    {
        public static IUnRegister AddListener(this object user, Action call)
            => TypeEventSystem.Global.Register(call);
        public static IUnRegister AddListener<T>(this object user,Action<T> call)
            => TypeEventSystem.Global.Register(call);
        public static IUnRegister AddListener<T,K>(this object user, Action<T,K> call)
            => TypeEventSystem.Global.Register(call);
        public static IUnRegister AddListener<T,K,Q>(this object user, Action<T,K,Q> call)
            => TypeEventSystem.Global.Register(call);
        public static IUnRegister AddListener<T, K, Q, R>(this object user, Action<T, K, Q, R> call)
            => TypeEventSystem.Global.Register(call);

        public static IUnRegister AddListener(this object user,string name, Action call)
            => StringEventSystem.Global.Register(name,call);
        public static IUnRegister AddListener<T>(this object user,string name, Action<T> call)
            => StringEventSystem.Global.Register(name,call);
        public static IUnRegister AddListener<T, K>(this object user,string name, Action<T, K> call)
            => StringEventSystem.Global.Register(name,call);
        public static IUnRegister AddListener<T, K, Q>(this object user,string name, Action<T, K, Q> call)
            => StringEventSystem.Global.Register(name,call);
        public static IUnRegister AddListener<T, K, Q, R>(this object user,string name, Action<T, K, Q, R> call)
            => StringEventSystem.Global.Register(name,call);

        public static IUnRegister AddListener(this object user, Enum name, Action call)
            => EnumEventSystem.Global.Register(name, call);
        public static IUnRegister AddListener<T>(this object user, Enum name, Action<T> call)
            => EnumEventSystem.Global.Register(name, call);
        public static IUnRegister AddListener<T, K>(this object user, Enum name, Action<T, K> call)
            => EnumEventSystem.Global.Register(name, call);
        public static IUnRegister AddListener<T, K, Q>(this object user, Enum name, Action<T, K, Q> call)
            => EnumEventSystem.Global.Register(name, call);
        public static IUnRegister AddListener<T, K, Q, R>(this object user, Enum name, Action<T, K, Q, R> call)
            => EnumEventSystem.Global.Register(name, call);

        public static void SendGlobalEvent(this object user)
            => TypeEventSystem.Global.Send();
        public static void SendGlobalEvent<T>(this object user,T arg1)
            => TypeEventSystem.Global.Send(arg1);
        public static void SendGlobalEvent<T,K>(this object user, T arg1,K arg2)
            => TypeEventSystem.Global.Send(arg1,arg2);
        public static void SendGlobalEvent<T, K,Q>(this object user, T arg1, K arg2,Q arg3)
            => TypeEventSystem.Global.Send(arg1, arg2,arg3);
        public static void SendGlobalEvent<T, K, Q,R>(this object user, T arg1, K arg2, Q arg3,R arg4)
            => TypeEventSystem.Global.Send(arg1, arg2, arg3,arg4);

        public static void SendGlobalEvent(this object user,string name)
           => StringEventSystem.Global.Send(name);
        public static void SendGlobalEvent<T>(this object user,string name, T arg1)
            => StringEventSystem.Global.Send(name,arg1);
        public static void SendGlobalEvent<T, K>(this object user,string name, T arg1, K arg2)
            => StringEventSystem.Global.Send(name,arg1, arg2);
        public static void SendGlobalEvent<T, K, Q>(this object user,string name, T arg1, K arg2, Q arg3)
            => StringEventSystem.Global.Send(name,arg1, arg2, arg3);
        public static void SendGlobalEvent<T, K, Q, R>(this object user,string name, T arg1, K arg2, Q arg3, R arg4)
            => StringEventSystem.Global.Send(name,arg1, arg2, arg3, arg4);

        public static void SendGlobalEvent(this object user, Enum name)
           => EnumEventSystem.Global.Send(name);
        public static void SendGlobalEvent<T>(this object user, Enum name, T arg1)
            => EnumEventSystem.Global.Send(name, arg1);
        public static void SendGlobalEvent<T, K>(this object user, Enum name, T arg1, K arg2)
            => EnumEventSystem.Global.Send(name, arg1, arg2);
        public static void SendGlobalEvent<T, K, Q>(this object user, Enum name, T arg1, K arg2, Q arg3)
            => EnumEventSystem.Global.Send(name, arg1, arg2, arg3);
        public static void SendGlobalEvent<T, K, Q, R>(this object user, Enum name, T arg1, K arg2, Q arg3, R arg4)
            => EnumEventSystem.Global.Send(name, arg1, arg2, arg3, arg4);
    }
}
#endif