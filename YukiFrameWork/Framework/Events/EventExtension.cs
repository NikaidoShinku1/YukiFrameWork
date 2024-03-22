using System;
namespace YukiFrameWork
{
    public static class EventExtension 
    {
        #region AddListener
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
        public static IUnRegister AddListener<T, K, Q, R, W>(this object user, Action<T, K, Q, R, W> call)
            => TypeEventSystem.Global.Register(call);
        public static IUnRegister AddListener<T, K, Q, R, W, P>(this object user, Action<T, K, Q, R, W, P> call)
            => TypeEventSystem.Global.Register(call);
        public static IUnRegister AddListener<T, K, Q, R, W, P, S>(this object user, Action<T, K, Q, R, W, P, S> call)
            => TypeEventSystem.Global.Register(call);
        public static IUnRegister AddListener<T, K, Q, R, W, P, S, F>(this object user, Action<T, K, Q, R, W, P, S, F> call)
            => TypeEventSystem.Global.Register(call);
        public static IUnRegister AddListener<T, K, Q, R, W, P, S, F, G>(this object user, Action<T, K, Q, R, W, P, S, F, G> call)
            => TypeEventSystem.Global.Register(call);
        public static IUnRegister AddListener<T, K, Q, R, W, P, S, F, G, M>(this object user, Action<T, K, Q, R, W, P, S, F, G, M> call)
            => TypeEventSystem.Global.Register(call);
        public static IUnRegister AddListener<T, K, Q, R, W, P, S, F, G, M, N>(this object user, Action<T, K, Q, R, W, P, S, F, G, M, N> call)
            => TypeEventSystem.Global.Register(call);
        public static IUnRegister AddListener<T, K, Q, R, W, P, S, F, G, M, N, B>(this object user, Action<T, K, Q, R, W, P, S, F, G, M, N, B> call)
            => TypeEventSystem.Global.Register(call);
        public static IUnRegister AddListener<T, K, Q, R, W, P, S, F, G, M, N, B, V>(this object user, Action<T, K, Q, R, W, P, S, F, G, M, N, B, V> call)
            => TypeEventSystem.Global.Register(call);
        public static IUnRegister AddListener<T, K, Q, R, W, P, S, F, G, M, N, B, V, J>(this object user, Action<T, K, Q, R, W, P, S, F, G, M, N, B, V, J> call)
            => TypeEventSystem.Global.Register(call);
        public static IUnRegister AddListener<T, K, Q, R, W, P, S, F, G, M, N, B, V, J, X>(this object user, Action<T, K, Q, R, W, P, S, F, G, M, N, B, V, J, X> call)
            => TypeEventSystem.Global.Register(call);
        public static IUnRegister AddListener<T, K, Q, R, W, P, S, F, G, M, N, B, V, J, X,Z>(this object user, Action<T, K, Q, R, W, P, S, F, G, M, N, B, V, J, X,Z> call)
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
        public static IUnRegister AddListener<T, K, Q, R, W>(this object user,string name, Action<T, K, Q, R, W> call)
            => StringEventSystem.Global.Register(name,call);
        public static IUnRegister AddListener<T, K, Q, R, W, P>(this object user,string name, Action<T, K, Q, R, W, P> call)
            => StringEventSystem.Global.Register(name,call);
        public static IUnRegister AddListener<T, K, Q, R, W, P, S>(this object user,string name, Action<T, K, Q, R, W, P, S> call)
            => StringEventSystem.Global.Register(name,call);
        public static IUnRegister AddListener<T, K, Q, R, W, P, S, F>(this object user,string name, Action<T, K, Q, R, W, P, S, F> call)
            => StringEventSystem.Global.Register(name,call);
        public static IUnRegister AddListener<T, K, Q, R, W, P, S, F, G>(this object user,string name, Action<T, K, Q, R, W, P, S, F, G> call)
            => StringEventSystem.Global.Register(name,call);
        public static IUnRegister AddListener<T, K, Q, R, W, P, S, F, G, M>(this object user,string name, Action<T, K, Q, R, W, P, S, F, G, M> call)
            => StringEventSystem.Global.Register(name,call);
        public static IUnRegister AddListener<T, K, Q, R, W, P, S, F, G, M, N>(this object user,string name, Action<T, K, Q, R, W, P, S, F, G, M, N> call)
            => StringEventSystem.Global.Register(name,call);
        public static IUnRegister AddListener<T, K, Q, R, W, P, S, F, G, M, N, B>(this object user,string name, Action<T, K, Q, R, W, P, S, F, G, M, N, B> call)
            => StringEventSystem.Global.Register(name,call);
        public static IUnRegister AddListener<T, K, Q, R, W, P, S, F, G, M, N, B, V>(this object user,string name, Action<T, K, Q, R, W, P, S, F, G, M, N, B, V> call)
            => StringEventSystem.Global.Register(name,call);
        public static IUnRegister AddListener<T, K, Q, R, W, P, S, F, G, M, N, B, V, J>(this object user,string name, Action<T, K, Q, R, W, P, S, F, G, M, N, B, V, J> call)
            => StringEventSystem.Global.Register(name,call);
        public static IUnRegister AddListener<T, K, Q, R, W, P, S, F, G, M, N, B, V, J, X>(this object user,string name, Action<T, K, Q, R, W, P, S, F, G, M, N, B, V, J, X> call)
            => StringEventSystem.Global.Register(name,call);
        public static IUnRegister AddListener<T, K, Q, R, W, P, S, F, G, M, N, B, V, J, X, Z>(this object user,string name, Action<T, K, Q, R, W, P, S, F, G, M, N, B, V, J, X, Z> call)
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
        public static IUnRegister AddListener<T, K, Q, R, W>(this object user, Enum name, Action<T, K, Q, R, W> call)
            => EnumEventSystem.Global.Register(name, call);
        public static IUnRegister AddListener<T, K, Q, R, W, P>(this object user, Enum name, Action<T, K, Q, R, W, P> call)
            => EnumEventSystem.Global.Register(name, call);
        public static IUnRegister AddListener<T, K, Q, R, W, P, S>(this object user, Enum name, Action<T, K, Q, R, W, P, S> call)
            => EnumEventSystem.Global.Register(name, call);
        public static IUnRegister AddListener<T, K, Q, R, W, P, S, F>(this object user, Enum name, Action<T, K, Q, R, W, P, S, F> call)
            => EnumEventSystem.Global.Register(name, call);
        public static IUnRegister AddListener<T, K, Q, R, W, P, S, F, G>(this object user, Enum name, Action<T, K, Q, R, W, P, S, F, G> call)
            => EnumEventSystem.Global.Register(name, call);
        public static IUnRegister AddListener<T, K, Q, R, W, P, S, F, G, M>(this object user, Enum name, Action<T, K, Q, R, W, P, S, F, G, M> call)
            => EnumEventSystem.Global.Register(name, call);
        public static IUnRegister AddListener<T, K, Q, R, W, P, S, F, G, M, N>(this object user, Enum name, Action<T, K, Q, R, W, P, S, F, G, M, N> call)
            => EnumEventSystem.Global.Register(name, call);
        public static IUnRegister AddListener<T, K, Q, R, W, P, S, F, G, M, N, B>(this object user, Enum name, Action<T, K, Q, R, W, P, S, F, G, M, N, B> call)
            => EnumEventSystem.Global.Register(name, call);
        public static IUnRegister AddListener<T, K, Q, R, W, P, S, F, G, M, N, B, V>(this object user, Enum name, Action<T, K, Q, R, W, P, S, F, G, M, N, B, V> call)
            => EnumEventSystem.Global.Register(name, call);
        public static IUnRegister AddListener<T, K, Q, R, W, P, S, F, G, M, N, B, V, J>(this object user, Enum name, Action<T, K, Q, R, W, P, S, F, G, M, N, B, V, J> call)
            => EnumEventSystem.Global.Register(name, call);
        public static IUnRegister AddListener<T, K, Q, R, W, P, S, F, G, M, N, B, V, J, X>(this object user, Enum name, Action<T, K, Q, R, W, P, S, F, G, M, N, B, V, J, X> call)
            => EnumEventSystem.Global.Register(name, call);
        public static IUnRegister AddListener<T, K, Q, R, W, P, S, F, G, M, N, B, V, J, X, Z>(this object user, Enum name, Action<T, K, Q, R, W, P, S, F, G, M, N, B, V, J, X, Z> call)
            => EnumEventSystem.Global.Register(name, call);
        #endregion

        #region SendEvent
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
        public static void SendGlobalEvent<T, K, Q, R, W>(this object user, T arg1, K arg2, Q arg3, R arg4, W arg5)
            => TypeEventSystem.Global.Send(arg1, arg2, arg3, arg4, arg5);
        public static void SendGlobalEvent<T, K, Q, R, W, P>(this object user, T arg1, K arg2, Q arg3, R arg4, W arg5,P arg6)
            => TypeEventSystem.Global.Send(arg1, arg2, arg3, arg4, arg5,arg6);
        public static void SendGlobalEvent<T, K, Q, R, W, P, S>(this object user, T arg1, K arg2, Q arg3, R arg4, W arg5, P arg6,S arg7)
            => TypeEventSystem.Global.Send(arg1, arg2, arg3, arg4, arg5, arg6,arg7);
        public static void SendGlobalEvent<T, K, Q, R, W, P, S, F>(this object user, T arg1, K arg2, Q arg3, R arg4, W arg5, P arg6, S arg7,F arg8)
            => TypeEventSystem.Global.Send(arg1, arg2, arg3, arg4, arg5, arg6, arg7,arg8);
        public static void SendGlobalEvent<T, K, Q, R, W, P, S, F,G>(this object user, T arg1, K arg2, Q arg3, R arg4, W arg5, P arg6, S arg7, F arg8,G arg9)
            => TypeEventSystem.Global.Send(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8,arg9);
        public static void SendGlobalEvent<T, K, Q, R, W, P, S, F, G,M>(this object user, T arg1, K arg2, Q arg3, R arg4, W arg5, P arg6, S arg7, F arg8, G arg9,M arg10)
            => TypeEventSystem.Global.Send(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9 ,arg10);
        public static void SendGlobalEvent<T, K, Q, R, W, P, S, F, G, M, N>(this object user, T arg1, K arg2, Q arg3, R arg4, W arg5, P arg6, S arg7, F arg8, G arg9, M arg10,N arg11)
            => TypeEventSystem.Global.Send(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10,arg11);
        public static void SendGlobalEvent<T, K, Q, R, W, P, S, F, G, M, N, B>(this object user, T arg1, K arg2, Q arg3, R arg4, W arg5, P arg6, S arg7, F arg8, G arg9, M arg10, N arg11, B arg12)
            => TypeEventSystem.Global.Send(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11,arg12);
        public static void SendGlobalEvent<T, K, Q, R, W, P, S, F, G, M, N, B,V>(this object user, T arg1, K arg2, Q arg3, R arg4, W arg5, P arg6, S arg7, F arg8, G arg9, M arg10, N arg11, B arg12,V arg13)
            => TypeEventSystem.Global.Send(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12,arg13);
        public static void SendGlobalEvent<T, K, Q, R, W, P, S, F, G, M, N, B, V, J>(this object user, T arg1, K arg2, Q arg3, R arg4, W arg5, P arg6, S arg7, F arg8, G arg9, M arg10, N arg11, B arg12, V arg13,J arg14)
            => TypeEventSystem.Global.Send(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13,arg14);
        public static void SendGlobalEvent<T, K, Q, R, W, P, S, F, G, M, N, B, V, J, X>(this object user, T arg1, K arg2, Q arg3, R arg4, W arg5, P arg6, S arg7, F arg8, G arg9, M arg10, N arg11, B arg12, V arg13, J arg14, X arg15)
            => TypeEventSystem.Global.Send(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14,arg15);
        public static void SendGlobalEvent<T, K, Q, R, W, P, S, F, G, M, N, B, V, J, X, Z>(this object user, T arg1, K arg2, Q arg3, R arg4, W arg5, P arg6, S arg7, F arg8, G arg9, M arg10, N arg11, B arg12, V arg13, J arg14, X arg15, Z arg16)
            => TypeEventSystem.Global.Send(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15,arg16);

        public static void SendStringGlobalEvent(this object user,string name)
            => StringEventSystem.Global.Send(name);
        public static void SendStringGlobalEvent<T>(this object user,string name, T arg1)
            => StringEventSystem.Global.Send(name,arg1);
        public static void SendStringGlobalEvent<T, K>(this object user,string name, T arg1, K arg2)
            => StringEventSystem.Global.Send(name,arg1, arg2);
        public static void SendStringGlobalEvent<T, K, Q>(this object user,string name, T arg1, K arg2, Q arg3)
            => StringEventSystem.Global.Send(name,arg1, arg2, arg3);
        public static void SendStringGlobalEvent<T, K, Q, R>(this object user,string name, T arg1, K arg2, Q arg3, R arg4)
            => StringEventSystem.Global.Send(name,arg1, arg2, arg3, arg4);
        public static void SendStringGlobalEvent<T, K, Q, R, W>(this object user,string name, T arg1, K arg2, Q arg3, R arg4, W arg5)
            => StringEventSystem.Global.Send(name,arg1, arg2, arg3, arg4, arg5);
        public static void SendStringGlobalEvent<T, K, Q, R, W, P>(this object user,string name, T arg1, K arg2, Q arg3, R arg4, W arg5, P arg6)
            => StringEventSystem.Global.Send(name,arg1, arg2, arg3, arg4, arg5, arg6);
        public static void SendStringGlobalEvent<T, K, Q, R, W, P, S>(this object user,string name, T arg1, K arg2, Q arg3, R arg4, W arg5, P arg6, S arg7)
            => StringEventSystem.Global.Send(name,arg1, arg2, arg3, arg4, arg5, arg6, arg7);
        public static void SendStringGlobalEvent<T, K, Q, R, W, P, S, F>(this object user,string name, T arg1, K arg2, Q arg3, R arg4, W arg5, P arg6, S arg7, F arg8)
            => StringEventSystem.Global.Send(name,arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
        public static void SendStringGlobalEvent<T, K, Q, R, W, P, S, F, G>(this object user,string name, T arg1, K arg2, Q arg3, R arg4, W arg5, P arg6, S arg7, F arg8, G arg9)
            => StringEventSystem.Global.Send(name,arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
        public static void SendStringGlobalEvent<T, K, Q, R, W, P, S, F, G, M>(this object user,string name, T arg1, K arg2, Q arg3, R arg4, W arg5, P arg6, S arg7, F arg8, G arg9, M arg10)
            => StringEventSystem.Global.Send(name,arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
        public static void SendStringGlobalEvent<T, K, Q, R, W, P, S, F, G, M, N>(this object user,string name, T arg1, K arg2, Q arg3, R arg4, W arg5, P arg6, S arg7, F arg8, G arg9, M arg10, N arg11)
            => StringEventSystem.Global.Send(name,arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
        public static void SendStringGlobalEvent<T, K, Q, R, W, P, S, F, G, M, N, B>(this object user,string name, T arg1, K arg2, Q arg3, R arg4, W arg5, P arg6, S arg7, F arg8, G arg9, M arg10, N arg11, B arg12)
            => StringEventSystem.Global.Send(name,arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);
        public static void SendStringGlobalEvent<T, K, Q, R, W, P, S, F, G, M, N, B, V>(this object user,string name, T arg1, K arg2, Q arg3, R arg4, W arg5, P arg6, S arg7, F arg8, G arg9, M arg10, N arg11, B arg12, V arg13)
            => StringEventSystem.Global.Send(name,arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13);
        public static void SendStringGlobalEvent<T, K, Q, R, W, P, S, F, G, M, N, B, V, J>(this object user,string name, T arg1, K arg2, Q arg3, R arg4, W arg5, P arg6, S arg7, F arg8, G arg9, M arg10, N arg11, B arg12, V arg13, J arg14)
            => StringEventSystem.Global.Send(name,arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14);
        public static void SendStringGlobalEvent<T, K, Q, R, W, P, S, F, G, M, N, B, V, J, X>(this object user,string name, T arg1, K arg2, Q arg3, R arg4, W arg5, P arg6, S arg7, F arg8, G arg9, M arg10, N arg11, B arg12, V arg13, J arg14, X arg15)
            => StringEventSystem.Global.Send(name,arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15);
        public static void SendStringGlobalEvent<T, K, Q, R, W, P, S, F, G, M, N, B, V, J, X, Z>(this object user,string name, T arg1, K arg2, Q arg3, R arg4, W arg5, P arg6, S arg7, F arg8, G arg9, M arg10, N arg11, B arg12, V arg13, J arg14, X arg15, Z arg16)
            => StringEventSystem.Global.Send(name,arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16);

        public static void SendEnumGlobalEvent(this object user, Enum name)
            => EnumEventSystem.Global.Send(name);
        public static void SendEnumGlobalEvent<T>(this object user, Enum name, T arg1)
            => EnumEventSystem.Global.Send(name, arg1);
        public static void SendEnumGlobalEvent<T, K>(this object user, Enum name, T arg1, K arg2)
            => EnumEventSystem.Global.Send(name, arg1, arg2);
        public static void SendEnumGlobalEvent<T, K, Q>(this object user, Enum name, T arg1, K arg2, Q arg3)
            => EnumEventSystem.Global.Send(name, arg1, arg2, arg3);
        public static void SendEnumGlobalEvent<T, K, Q, R>(this object user, Enum name, T arg1, K arg2, Q arg3, R arg4)
            => EnumEventSystem.Global.Send(name, arg1, arg2, arg3, arg4);
        public static void SendEnumGlobalEvent<T, K, Q, R, W>(this object user, Enum name, T arg1, K arg2, Q arg3, R arg4, W arg5)
            => EnumEventSystem.Global.Send(name, arg1, arg2, arg3, arg4, arg5);
        public static void SendEnumGlobalEvent<T, K, Q, R, W, P>(this object user, Enum name, T arg1, K arg2, Q arg3, R arg4, W arg5, P arg6)
            => EnumEventSystem.Global.Send(name, arg1, arg2, arg3, arg4, arg5, arg6);
        public static void SendEnumGlobalEvent<T, K, Q, R, W, P, S>(this object user, Enum name, T arg1, K arg2, Q arg3, R arg4, W arg5, P arg6, S arg7)
            => EnumEventSystem.Global.Send(name, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
        public static void SendEnumGlobalEvent<T, K, Q, R, W, P, S, F>(this object user, Enum name, T arg1, K arg2, Q arg3, R arg4, W arg5, P arg6, S arg7, F arg8)
            => EnumEventSystem.Global.Send(name, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
        public static void SendEnumGlobalEvent<T, K, Q, R, W, P, S, F, G>(this object user, Enum name, T arg1, K arg2, Q arg3, R arg4, W arg5, P arg6, S arg7, F arg8, G arg9)
            => EnumEventSystem.Global.Send(name, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
        public static void SendEnumGlobalEvent<T, K, Q, R, W, P, S, F, G, M>(this object user, Enum name, T arg1, K arg2, Q arg3, R arg4, W arg5, P arg6, S arg7, F arg8, G arg9, M arg10)
            => EnumEventSystem.Global.Send(name, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
        public static void SendEnumGlobalEvent<T, K, Q, R, W, P, S, F, G, M, N>(this object user, Enum name, T arg1, K arg2, Q arg3, R arg4, W arg5, P arg6, S arg7, F arg8, G arg9, M arg10, N arg11)
            => EnumEventSystem.Global.Send(name, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
        public static void SendEnumGlobalEvent<T, K, Q, R, W, P, S, F, G, M, N, B>(this object user, Enum name, T arg1, K arg2, Q arg3, R arg4, W arg5, P arg6, S arg7, F arg8, G arg9, M arg10, N arg11, B arg12)
            => EnumEventSystem.Global.Send(name, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);
        public static void SendEnumGlobalEvent<T, K, Q, R, W, P, S, F, G, M, N, B, V>(this object user, Enum name, T arg1, K arg2, Q arg3, R arg4, W arg5, P arg6, S arg7, F arg8, G arg9, M arg10, N arg11, B arg12, V arg13)
            => EnumEventSystem.Global.Send(name, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13);
        public static void SendEnumGlobalEvent<T, K, Q, R, W, P, S, F, G, M, N, B, V, J>(this object user, Enum name, T arg1, K arg2, Q arg3, R arg4, W arg5, P arg6, S arg7, F arg8, G arg9, M arg10, N arg11, B arg12, V arg13, J arg14)
            => EnumEventSystem.Global.Send(name, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14);
        public static void SendEnumGlobalEvent<T, K, Q, R, W, P, S, F, G, M, N, B, V, J, X>(this object user, Enum name, T arg1, K arg2, Q arg3, R arg4, W arg5, P arg6, S arg7, F arg8, G arg9, M arg10, N arg11, B arg12, V arg13, J arg14, X arg15)
            => EnumEventSystem.Global.Send(name, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15);
        public static void SendEnumlobalEvent<T, K, Q, R, W, P, S, F, G, M, N, B, V, J, X, Z>(this object user, Enum name, T arg1, K arg2, Q arg3, R arg4, W arg5, P arg6, S arg7, F arg8, G arg9, M arg10, N arg11, B arg12, V arg13, J arg14, X arg15, Z arg16)
            => EnumEventSystem.Global.Send(name, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16);
        #endregion

        #region RemoveListener
        public static void RemoveListener(this object user, Action call)
            => TypeEventSystem.Global.UnRegister(call);
        public static void RemoveListener<T>(this object user, Action<T> call)
            => TypeEventSystem.Global.UnRegister(call);
        public static void RemoveListener<T, K>(this object user, Action<T, K> call)
            => TypeEventSystem.Global.UnRegister(call);
        public static void RemoveListener<T, K, Q>(this object user, Action<T, K, Q> call)
            => TypeEventSystem.Global.UnRegister(call);
        public static void RemoveListener<T, K, Q, R>(this object user, Action<T, K, Q, R> call)
            => TypeEventSystem.Global.UnRegister(call);
        public static void RemoveListener<T, K, Q, R, W>(this object user, Action<T, K, Q, R, W> call)
            => TypeEventSystem.Global.UnRegister(call);
        public static void RemoveListener<T, K, Q, R, W, P>(this object user, Action<T, K, Q, R, W, P> call)
            => TypeEventSystem.Global.UnRegister(call);
        public static void RemoveListener<T, K, Q, R, W, P, S>(this object user, Action<T, K, Q, R, W, P, S> call)
            => TypeEventSystem.Global.UnRegister(call);
        public static void RemoveListener<T, K, Q, R, W, P, S, F>(this object user, Action<T, K, Q, R, W, P, S, F> call)
            => TypeEventSystem.Global.UnRegister(call);
        public static void RemoveListener<T, K, Q, R, W, P, S, F, G>(this object user, Action<T, K, Q, R, W, P, S, F, G> call)
            => TypeEventSystem.Global.UnRegister(call);
        public static void RemoveListener<T, K, Q, R, W, P, S, F, G, M>(this object user, Action<T, K, Q, R, W, P, S, F, G, M> call)
            => TypeEventSystem.Global.UnRegister(call);
        public static void RemoveListener<T, K, Q, R, W, P, S, F, G, M, N>(this object user, Action<T, K, Q, R, W, P, S, F, G, M, N> call)
            => TypeEventSystem.Global.UnRegister(call);
        public static void RemoveListener<T, K, Q, R, W, P, S, F, G, M, N, B>(this object user, Action<T, K, Q, R, W, P, S, F, G, M, N, B> call)
            => TypeEventSystem.Global.UnRegister(call);
        public static void RemoveListener<T, K, Q, R, W, P, S, F, G, M, N, B, V>(this object user, Action<T, K, Q, R, W, P, S, F, G, M, N, B, V> call)
            => TypeEventSystem.Global.UnRegister(call);
        public static void RemoveListener<T, K, Q, R, W, P, S, F, G, M, N, B, V, J>(this object user, Action<T, K, Q, R, W, P, S, F, G, M, N, B, V, J> call)
            => TypeEventSystem.Global.UnRegister(call);
        public static void RemoveListener<T, K, Q, R, W, P, S, F, G, M, N, B, V, J, X>(this object user, Action<T, K, Q, R, W, P, S, F, G, M, N, B, V, J, X> call)
            => TypeEventSystem.Global.UnRegister(call);
        public static void RemoveListener<T, K, Q, R, W, P, S, F, G, M, N, B, V, J, X, Z>(this object user, Action<T, K, Q, R, W, P, S, F, G, M, N, B, V, J, X, Z> call)
            => TypeEventSystem.Global.UnRegister(call);

        public static void RemoveListener(this object user, string name, Action call)
            => StringEventSystem.Global.UnRegister(name, call);
        public static void RemoveListener<T>(this object user, string name, Action<T> call)
            => StringEventSystem.Global.UnRegister(name, call);
        public static void RemoveListener<T, K>(this object user, string name, Action<T, K> call)
            => StringEventSystem.Global.UnRegister(name, call);
        public static void RemoveListener<T, K, Q>(this object user, string name, Action<T, K, Q> call)
            => StringEventSystem.Global.UnRegister(name, call);
        public static void RemoveListener<T, K, Q, R>(this object user, string name, Action<T, K, Q, R> call)
            => StringEventSystem.Global.UnRegister(name, call);
        public static void RemoveListener<T, K, Q, R, W>(this object user,string name, Action<T, K, Q, R, W> call)
            => StringEventSystem.Global.UnRegister(name,call);
        public static void RemoveListener<T, K, Q, R, W, P>(this object user,string name, Action<T, K, Q, R, W, P> call)
            => StringEventSystem.Global.UnRegister(name,call);
        public static void RemoveListener<T, K, Q, R, W, P, S>(this object user,string name, Action<T, K, Q, R, W, P, S> call)
            => StringEventSystem.Global.UnRegister(name,call);
        public static void RemoveListener<T, K, Q, R, W, P, S, F>(this object user,string name, Action<T, K, Q, R, W, P, S, F> call)
            => StringEventSystem.Global.UnRegister(name,call);
        public static void RemoveListener<T, K, Q, R, W, P, S, F, G>(this object user,string name, Action<T, K, Q, R, W, P, S, F, G> call)
            => StringEventSystem.Global.UnRegister(name,call);
        public static void RemoveListener<T, K, Q, R, W, P, S, F, G, M>(this object user,string name, Action<T, K, Q, R, W, P, S, F, G, M> call)
            => StringEventSystem.Global.UnRegister(name,call);
        public static void RemoveListener<T, K, Q, R, W, P, S, F, G, M, N>(this object user,string name, Action<T, K, Q, R, W, P, S, F, G, M, N> call)
            => StringEventSystem.Global.UnRegister(name,call);
        public static void RemoveListener<T, K, Q, R, W, P, S, F, G, M, N, B>(this object user,string name, Action<T, K, Q, R, W, P, S, F, G, M, N, B> call)
            => StringEventSystem.Global.UnRegister(name,call);
        public static void RemoveListener<T, K, Q, R, W, P, S, F, G, M, N, B, V>(this object user,string name, Action<T, K, Q, R, W, P, S, F, G, M, N, B, V> call)
            => StringEventSystem.Global.UnRegister(name,call);
        public static void RemoveListener<T, K, Q, R, W, P, S, F, G, M, N, B, V, J>(this object user,string name, Action<T, K, Q, R, W, P, S, F, G, M, N, B, V, J> call)
            => StringEventSystem.Global.UnRegister(name,call);
        public static void RemoveListener<T, K, Q, R, W, P, S, F, G, M, N, B, V, J, X>(this object user,string name, Action<T, K, Q, R, W, P, S, F, G, M, N, B, V, J, X> call)
            => StringEventSystem.Global.UnRegister(name,call);
        public static void RemoveListener<T, K, Q, R, W, P, S, F, G, M, N, B, V, J, X, Z>(this object user,string name, Action<T, K, Q, R, W, P, S, F, G, M, N, B, V, J, X, Z> call)
            => StringEventSystem.Global.UnRegister(name,call);


        public static void RemoveListener(this object user, Enum name, Action call)
            => EnumEventSystem.Global.UnRegister(name, call);
        public static void RemoveListener<T>(this object user, Enum name, Action<T> call)
            => EnumEventSystem.Global.UnRegister(name, call);
        public static void RemoveListener<T, K>(this object user, Enum name, Action<T, K> call)
            => EnumEventSystem.Global.UnRegister(name, call);
        public static void RemoveListener<T, K, Q>(this object user, Enum name, Action<T, K, Q> call)
            => EnumEventSystem.Global.UnRegister(name, call);
        public static void RemoveListener<T, K, Q, R>(this object user, Enum name, Action<T, K, Q, R> call)
            => EnumEventSystem.Global.UnRegister(name, call);
        public static void RemoveListener<T, K, Q, R, W>(this object user, Enum name, Action<T, K, Q, R, W> call)
            => EnumEventSystem.Global.UnRegister(name, call);
        public static void RemoveListener<T, K, Q, R, W, P>(this object user, Enum name, Action<T, K, Q, R, W, P> call)
            => EnumEventSystem.Global.UnRegister(name, call);
        public static void RemoveListener<T, K, Q, R, W, P, S>(this object user, Enum name, Action<T, K, Q, R, W, P, S> call)
            => EnumEventSystem.Global.UnRegister(name, call);
        public static void RemoveListener<T, K, Q, R, W, P, S, F>(this object user, Enum name, Action<T, K, Q, R, W, P, S, F> call)
            => EnumEventSystem.Global.UnRegister(name, call);
        public static void RemoveListener<T, K, Q, R, W, P, S, F, G>(this object user, Enum name, Action<T, K, Q, R, W, P, S, F, G> call)
            => EnumEventSystem.Global.UnRegister(name, call);
        public static void RemoveListener<T, K, Q, R, W, P, S, F, G, M>(this object user, Enum name, Action<T, K, Q, R, W, P, S, F, G, M> call)
            => EnumEventSystem.Global.UnRegister(name, call);
        public static void RemoveListener<T, K, Q, R, W, P, S, F, G, M, N>(this object user, Enum name, Action<T, K, Q, R, W, P, S, F, G, M, N> call)
            => EnumEventSystem.Global.UnRegister(name, call);
        public static void RemoveListener<T, K, Q, R, W, P, S, F, G, M, N, B>(this object user, Enum name, Action<T, K, Q, R, W, P, S, F, G, M, N, B> call)
            => EnumEventSystem.Global.UnRegister(name, call);
        public static void RemoveListener<T, K, Q, R, W, P, S, F, G, M, N, B, V>(this object user, Enum name, Action<T, K, Q, R, W, P, S, F, G, M, N, B, V> call)
            => EnumEventSystem.Global.UnRegister(name, call);
        public static void RemoveListener<T, K, Q, R, W, P, S, F, G, M, N, B, V, J>(this object user, Enum name, Action<T, K, Q, R, W, P, S, F, G, M, N, B, V, J> call)
            => EnumEventSystem.Global.UnRegister(name, call);
        public static void RemoveListener<T, K, Q, R, W, P, S, F, G, M, N, B, V, J, X>(this object user, Enum name, Action<T, K, Q, R, W, P, S, F, G, M, N, B, V, J, X> call)
            => EnumEventSystem.Global.UnRegister(name, call);
        public static void RemoveListener<T, K, Q, R, W, P, S, F, G, M, N, B, V, J, X, Z>(this object user, Enum name, Action<T, K, Q, R, W, P, S, F, G, M, N, B, V, J, X, Z> call)
            => EnumEventSystem.Global.UnRegister(name, call);
        #endregion
    }
}