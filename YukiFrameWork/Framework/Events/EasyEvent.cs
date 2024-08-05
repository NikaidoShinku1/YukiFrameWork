///=====================================================
/// - FileName:      EasyEvent.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/3/22 14:18:46
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using System;
using System.Threading.Tasks;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
namespace YukiFrameWork
{
    public class EasyEvent : EasyEventBase<Action>
    {
        public override IUnRegister RegisterEvent(Action onEvent)
        {
            OnEasyEvent += onEvent;
            return this;
        }

        public void SendEvent()
        {
            OnEasyEvent?.Invoke();
        }

        public override void UnRegister(Action onEvent)
        {
            OnEasyEvent -= onEvent;
        }
    }

    public class EasyEvent<T, K> : EasyEventBase<Action<T, K>>
    {
        public override IUnRegister RegisterEvent(Action<T, K> onEvent)
        {
            OnEasyEvent += onEvent;
            return this;
        }

        public void SendEvent(T t, K k)
            => OnEasyEvent?.Invoke(t, k);

        public override void UnRegister(Action<T, K> onEvent)
        {
            OnEasyEvent -= onEvent;
        }
    }

    public class EasyEvent<T, K, Q> : EasyEventBase<Action<T, K, Q>>
    {
        public override IUnRegister RegisterEvent(Action<T, K, Q> onEvent)
        {
            OnEasyEvent += onEvent;
            return this;
        }
        public void SendEvent(T t, K k, Q q)
            => OnEasyEvent?.Invoke(t, k, q);

        public override void UnRegister(Action<T, K, Q> onEvent)
        {
            OnEasyEvent -= onEvent;
        }
    }

    public class EasyEvent<T, K, Q, P> : EasyEventBase<Action<T, K, Q, P>>
    {
        public override IUnRegister RegisterEvent(Action<T, K, Q, P> onEvent)
        {
            OnEasyEvent += onEvent;
            return this;
        }

        public void SendEvent(T t, K k, Q q, P p)
            => OnEasyEvent?.Invoke(t, k, q, p);

        public override void UnRegister(Action<T, K, Q, P> onEvent)
        {
            OnEasyEvent -= onEvent;
        }
    }

    public class EasyEvent<T, K, Q, P, W> : EasyEventBase<Action<T, K, Q, P, W>>
    {
        public override IUnRegister RegisterEvent(Action<T, K, Q, P, W> onEvent)
        {
            OnEasyEvent += onEvent;
            return this;
        }

        public void SendEvent(T t, K k, Q q, P p, W w)
            => OnEasyEvent?.Invoke(t, k, q, p, w);

        public override void UnRegister(Action<T, K, Q, P, W> onEvent)
        {
            OnEasyEvent -= onEvent;
        }
    }

    public class EasyEvent<T, K, Q, P, W, R> : EasyEventBase<Action<T, K, Q, P, W, R>>
    {
        public override IUnRegister RegisterEvent(Action<T, K, Q, P, W, R> onEvent)
        {
            OnEasyEvent += onEvent;
            return this;
        }

        public override void UnRegister(Action<T, K, Q, P, W, R> onEvent)
        {
            OnEasyEvent -= onEvent;
        }

        public void SendEvent(T t, K k, Q q, P p, W w, R r)
           => OnEasyEvent?.Invoke(t, k, q, p, w, r);
    }

    public class EasyEvent<T, K, Q, P, W, R, S> : EasyEventBase<Action<T, K, Q, P, W, R, S>>
    {
        public override IUnRegister RegisterEvent(Action<T, K, Q, P, W, R, S> onEvent)
        {
            OnEasyEvent += onEvent;
            return this;
        }

        public void SendEvent(T t, K k, Q q, P p, W w, R r, S s)
          => OnEasyEvent?.Invoke(t, k, q, p, w, r, s);

        public override void UnRegister(Action<T, K, Q, P, W, R, S> onEvent)
        {
            OnEasyEvent -= onEvent;
        }
    }

    public class EasyEvent<T, K, Q, P, W, R, S, F> : EasyEventBase<Action<T, K, Q, P, W, R, S, F>>
    {
        public override IUnRegister RegisterEvent(Action<T, K, Q, P, W, R, S, F> onEvent)
        {
            OnEasyEvent += onEvent;
            return this;
        }

        public void SendEvent(T t, K k, Q q, P p, W w, R r, S s, F f)
          => OnEasyEvent?.Invoke(t, k, q, p, w, r, s, f);

        public override void UnRegister(Action<T, K, Q, P, W, R, S, F> onEvent)
        {
            OnEasyEvent -= onEvent;
        }
    }

    public class EasyEvent<T, K, Q, P, W, R, S, F, G> : EasyEventBase<Action<T, K, Q, P, W, R, S, F, G>>
    {
        public override IUnRegister RegisterEvent(Action<T, K, Q, P, W, R, S, F, G> onEvent)
        {
            OnEasyEvent += onEvent;
            return this;
        }

        public void SendEvent(T t, K k, Q q, P p, W w, R r, S s, F f, G g)
          => OnEasyEvent?.Invoke(t, k, q, p, w, r, s, f, g);

        public override void UnRegister(Action<T, K, Q, P, W, R, S, F, G> onEvent)
        {
            OnEasyEvent -= onEvent;
        }
    }

    public class EasyEvent<T, K, Q, P, W, R, S, F, G, M> : EasyEventBase<Action<T, K, Q, P, W, R, S, F, G, M>>
    {
        public override IUnRegister RegisterEvent(Action<T, K, Q, P, W, R, S, F, G, M> onEvent)
        {
            OnEasyEvent += onEvent;
            return this;
        }

        public void SendEvent(T t, K k, Q q, P p, W w, R r, S s, F f, G g, M m)
          => OnEasyEvent?.Invoke(t, k, q, p, w, r, s, f, g, m);

        public override void UnRegister(Action<T, K, Q, P, W, R, S, F, G, M> onEvent)
        {
            OnEasyEvent -= onEvent;
        }
    }

    public class EasyEvent<T, K, Q, P, W, R, S, F, G, M, N> : EasyEventBase<Action<T, K, Q, P, W, R, S, F, G, M, N>>
    {
        public override IUnRegister RegisterEvent(Action<T, K, Q, P, W, R, S, F, G, M, N> onEvent)
        {
            OnEasyEvent += onEvent;
            return this;
        }

        public void SendEvent(T t, K k, Q q, P p, W w, R r, S s, F f, G g, M m, N n)
          => OnEasyEvent?.Invoke(t, k, q, p, w, r, s, f, g, m, n);

        public override void UnRegister(Action<T, K, Q, P, W, R, S, F, G, M, N> onEvent)
        {
            OnEasyEvent -= onEvent;
        }
    }

    public class EasyEvent<T, K, Q, P, W, R, S, F, G, M, N, B> : EasyEventBase<Action<T, K, Q, P, W, R, S, F, G, M, N, B>>
    {
        public override IUnRegister RegisterEvent(Action<T, K, Q, P, W, R, S, F, G, M, N, B> onEvent)
        {
            OnEasyEvent += onEvent;
            return this;
        }

        public void SendEvent(T t, K k, Q q, P p, W w, R r, S s, F f, G g, M m, N n, B b)
          => OnEasyEvent?.Invoke(t, k, q, p, w, r, s, f, g, m, n, b);

        public override void UnRegister(Action<T, K, Q, P, W, R, S, F, G, M, N, B> onEvent)
        {
            OnEasyEvent -= onEvent;
        }
    }

    public class EasyEvent<T, K, Q, P, W, R, S, F, G, M, N, B, V> : EasyEventBase<Action<T, K, Q, P, W, R, S, F, G, M, N, B, V>>
    {
        public override IUnRegister RegisterEvent(Action<T, K, Q, P, W, R, S, F, G, M, N, B, V> onEvent)
        {
            OnEasyEvent += onEvent;
            return this;
        }

        public void SendEvent(T t, K k, Q q, P p, W w, R r, S s, F f, G g, M m, N n, B b, V v)
          => OnEasyEvent?.Invoke(t, k, q, p, w, r, s, f, g, m, n, b, v);

        public override void UnRegister(Action<T, K, Q, P, W, R, S, F, G, M, N, B, V> onEvent)
        {
            OnEasyEvent -= onEvent;
        }
    }

    public class EasyEvent<T, K, Q, P, W, R, S, F, G, M, N, B, V, J> : EasyEventBase<Action<T, K, Q, P, W, R, S, F, G, M, N, B, V, J>>
    {
        public override IUnRegister RegisterEvent(Action<T, K, Q, P, W, R, S, F, G, M, N, B, V, J> onEvent)
        {
            OnEasyEvent += onEvent;
            return this;
        }

        public void SendEvent(T t, K k, Q q, P p, W w, R r, S s, F f, G g, M m, N n, B b, V v, J j)
          => OnEasyEvent?.Invoke(t, k, q, p, w, r, s, f, g, m, n, b, v, j);

        public override void UnRegister(Action<T, K, Q, P, W, R, S, F, G, M, N, B, V, J> onEvent)
        {
            OnEasyEvent -= onEvent;
        }
    }

    public class EasyEvent<T, K, Q, P, W, R, S, F, G, M, N, B, V, J, X> : EasyEventBase<Action<T, K, Q, P, W, R, S, F, G, M, N, B, V, J, X>>
    {
        public override IUnRegister RegisterEvent(Action<T, K, Q, P, W, R, S, F, G, M, N, B, V, J, X> onEvent)
        {
            OnEasyEvent += onEvent;
            return this;
        }

        public void SendEvent(T t, K k, Q q, P p, W w, R r, S s, F f, G g, M m, N n, B b, V v, J j, X x)
          => OnEasyEvent?.Invoke(t, k, q, p, w, r, s, f, g, m, n, b, v, j, x);

        public override void UnRegister(Action<T, K, Q, P, W, R, S, F, G, M, N, B, V, J, X> onEvent)
        {
            OnEasyEvent -= onEvent;
        }
    }

    public class EasyEvent<T, K, Q, P, W, R, S, F, G, M, N, B, V, J, X, Z> : EasyEventBase<Action<T, K, Q, P, W, R, S, F, G, M, N, B, V, J, X, Z>>
    {
        public override IUnRegister RegisterEvent(Action<T, K, Q, P, W, R, S, F, G, M, N, B, V, J, X, Z> onEvent)
        {
            OnEasyEvent += onEvent;
            return this;
        }

        public void SendEvent(T t, K k, Q q, P p, W w, R r, S s, F f, G g, M m, N n, B b, V v, J j, X x, Z z)
          => OnEasyEvent?.Invoke(t, k, q, p, w, r, s, f, g, m, n, b, v, j, x, z);

        public override void UnRegister(Action<T, K, Q, P, W, R, S, F, G, M, N, B, V, J, X, Z> onEvent)
        {
            OnEasyEvent -= onEvent;
        }
    }

    public class EasyEvent<T> : EasyEventBase<Action<T>>
    {
        public override IUnRegister RegisterEvent(Action<T> onEvent)
        {
            OnEasyEvent += onEvent;
            return this;
        }

        public void SendEvent(T t)
        {
            OnEasyEvent?.Invoke(t);
        }

        public override void UnRegister(Action<T> onEvent)
        {
            OnEasyEvent -= onEvent;
        }
    }

    public class AsyncEasyEvent<T> : EasyEventBase<Func<T, Task>>
    {
        private List<Func<T, Task>> onAsyncEvent = new List<Func<T, Task>>();
        public override IUnRegister RegisterEvent(Func<T, Task> onEvent)
        {
            onAsyncEvent.Add(onEvent);
            return this;
        }    
    
        public override void UnRegister(Func<T, Task> onEvent)
        {
            onAsyncEvent.Remove(onEvent);            
        }

        public async Task SendEvent(T arg)
        {
            foreach (Func<T,Task> ev in onAsyncEvent)
            {
                if (ev == null) continue;
                var task = ev.Invoke(arg);
                if (task == null) continue;
                await task;
            }
        }

        public override void UnRegisterAllEvent()
        {
            onAsyncEvent.Clear();          
        } 
    }
#if UNITY_2021_1_OR_NEWER
    public class Unity_AsyncEasyEvent<T> : EasyEventBase<Func<T, YieldTask>>
    {
        private List<Func<T, YieldTask>> onAsyncEvent = new List<Func<T, YieldTask>>();
        public override IUnRegister RegisterEvent(Func<T, YieldTask> onEvent)
        {
            onAsyncEvent.Add(onEvent);
            return this;
        }
        
        public override void UnRegister(Func<T, YieldTask> onEvent)
        {
            onAsyncEvent.Remove(onEvent);
        }
        
        public async YieldTask SendEvent(T arg)
        {          
            foreach (Func<T, YieldTask> ev in onAsyncEvent)
            {
                if (ev == null) continue;
                var task = ev.Invoke(arg);
                if (task == null) continue;
                await task;
            }
        }

        public override void UnRegisterAllEvent()
        {
            onAsyncEvent.Clear();
        }

    }
#endif

    internal class DynamicEvent<T>  where T : Delegate
    {
        private List<DynamicEventInfo<T>> dynamics
             = new List<DynamicEventInfo<T>>();

        internal DynamicEventInfo<T> RegisterEvent_Dynamic(MethodInfo methodInfo, object target,params Type[] types)
        {
            T onEvent = methodInfo
                .CreateDelegate(typeof(T).MakeGenericType(types), target) as T;

            DynamicEventInfo<T> info = new DynamicEventInfo<T>();
            info.onEvent = onEvent;
            info.methodInfo = methodInfo;
            dynamics.Add(info);
            return info;
        }

        internal void UnRegister_Dynamic(T onEvent,Action<T> unRegister)
        {
            if (onEvent.Method == null) return;

            IEnumerable<DynamicEventInfo<T>> eventInfos = dynamics.Where(x => x.methodInfo == onEvent.Method);

            foreach (var item in eventInfos)
            {
                unRegister(item.onEvent);
            }
        }

        internal void Clear() => dynamics.Clear();
    }

    public abstract class DynamicEventBase<T> : EasyEventBase<T>, IDynamicEvent where T : Delegate
    {
        public abstract Type BaseDelegateType { get; }
        protected class DynamicActionInfo
        {
            public T onEvent;
            public MethodInfo methodInfo;
        }
        protected FastList<DynamicActionInfo> actionInfos = new FastList<DynamicActionInfo>();       

        public virtual IUnRegister RegisterEvent_Dynamic(MethodInfo methodInfo, object target)
        {
            Type parameterType = methodInfo.GetParameters()[0].ParameterType;
            T onEvent = GetLambda(parameterType,Expression.Constant(target),methodInfo);
            actionInfos.Add(new DynamicActionInfo() 
            {
                onEvent = onEvent,
                methodInfo = methodInfo
            });
            return RegisterEvent(onEvent);
        }

        protected T GetLambda(Type parameterType,ConstantExpression constant,MethodInfo methodInfo)
        {         
            ParameterExpression parameter = Expression.Parameter(typeof(IEventArgs), "arg");
            UnaryExpression unaryParameter = Expression.Convert(parameter, parameterType);
            MethodCallExpression methodCall = Expression.Call(constant, methodInfo, unaryParameter);
            LambdaExpression lambda = Expression.Lambda<T>(methodCall, parameter);
            T onEvent = (T)lambda.Compile();
            return onEvent;
        }

        public virtual void UnRegisterEvent_Dynamic(MethodInfo methodInfo)
        {
            if (methodInfo == null) return;
            DynamicActionInfo info = actionInfos.FirstOrDefault(x => x.methodInfo == methodInfo);

            if (info == null) return;
            UnRegister(info.onEvent);
        }
    }

    public class SyncDynamicEvent : DynamicEventBase<Action<IEventArgs>>
    {
        public override Type BaseDelegateType => typeof(Action<>);
        public override IUnRegister RegisterEvent(Action<IEventArgs> onEvent)
        {
            OnEasyEvent += onEvent;
            return this;
        }

        internal IUnRegister RegisterEvent_Dynamic(MethodInfo methodInfo, object target,Action<IEventArgs> onAdd)
        {
            Type parameterType = methodInfo.GetParameters()[0].ParameterType;
            Action<IEventArgs> onEvent = GetLambda(parameterType, Expression.Constant(target), methodInfo);
            onEvent += onAdd;
            actionInfos.Add(new DynamicActionInfo()
            {
                onEvent = onEvent,
                methodInfo = methodInfo
            });
            return RegisterEvent(onEvent);
        }

        public void SendEvent(IEventArgs arg)
        {
            OnEasyEvent?.Invoke(arg);
        }

        public override void UnRegister(Action<IEventArgs> onEvent)
        {
            OnEasyEvent -= onEvent;
        }
    }
}
