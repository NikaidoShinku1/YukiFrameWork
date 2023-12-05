using System;
using System.Collections.Generic;

namespace YukiFrameWork.Events
{
    /// <summary>
    /// 事件中心
    /// </summary>
    [Obsolete("建议使用TypeEventSystem全局订阅")]
    public static class EventCenter
    {
        //储存中心
        private static readonly Dictionary<string, Delegate> m_events = new Dictionary<string, Delegate>();

        #region 根据类型注册监听事件 RegisterEvent
        [Obsolete("建议使用TypeEventSystem全局订阅")]
        public static void AddListener(string name, Action action)
        {
            //检测注册事件
            RegisterEvent(name, action);

            //添加委托
            m_events[name] = m_events[name] as Action + action;
        }
        [Obsolete("建议使用TypeEventSystem全局订阅")]
        public static void AddListener<T0>(string name, Action<T0> action)
        {
            //检测注册事件
            RegisterEvent(name, action);

            //添加委托
            m_events[name] = m_events[name] as Action<T0> + action;
        }
        [Obsolete("建议使用TypeEventSystem全局订阅")]
        public static void AddListener<T0, T1>(string name, Action<T0, T1> action)
        {
            //检测注册事件
            RegisterEvent(name, action);

            //添加委托
            m_events[name] = m_events[name] as Action<T0, T1> + action;
        }
        [Obsolete("建议使用TypeEventSystem全局订阅")]
        public static void AddListener<T0, T1, T2>(string name, Action<T0, T1, T2> action)
        {
            //检测注册事件
            RegisterEvent(name, action);

            //添加委托
            m_events[name] = m_events[name] as Action<T0, T1, T2> + action;
        }

        /// <summary>
        /// 注册事件
        /// </summary>
        #region 参数类型
        /// <nameparam name="T0"></nameparam>
        /// <nameparam name="T1"></nameparam>
        /// <nameparam name="T2"></nameparam>
        /// <nameparam name="T3"></nameparam>
        #endregion
        /// <param name="name">事件类型</param>
        /// <param name="action">委托本体</param>
        [Obsolete("建议使用TypeEventSystem全局订阅")]
        public static void AddListener<T0, T1, T2, T3>(string name, Action<T0, T1, T2, T3> action)
        {
            //检测注册事件
            RegisterEvent(name, action);

            //添加委托
            m_events[name] = m_events[name] as Action<T0, T1, T2, T3> + action;
        }

        /// <summary>
        /// 根据事件类型注册对应事件
        /// </summary>
        /// <param name="name">事件类型</param>
        /// <param name="checkEvent">被检查的委托</param>
        private static void RegisterEvent(string name, Delegate checkEvent)
        {
            //查询类型是否存在
            if (!m_events.ContainsKey(name)) m_events.Add(name, null);

            Delegate ev = m_events[name];

            //查询当前类型是否已有事件
            if (ev != null)
            {
                //判断要被检查的事件与已拥有事件类型是否一致
                if (ev.GetType() != checkEvent.GetType())
                {
                    throw new NullReferenceException($"注册事件失败，当前委托内事件类型为{ev.GetType()},要添加的事件类型为{checkEvent.GetType()}");
                }
            }
        }
        #endregion

        #region 根据类型注销监听事件 LogOutEvent
        [Obsolete]
        public static void RemoveListener(string name, Action action)
        {
            //检测注销事件
            LogOutEvent(name, action);

            //注销委托
            m_events[name] = m_events[name] as Action - action;

            //当前类型事件为空则删除整个事件
            OnListenerRemoving(name);
        }
        [Obsolete]
        public static void RemoveListener<T0>(string name, Action<T0> action)
        {
            //检测注销事件
            LogOutEvent(name, action);

            //注销委托
            m_events[name] = m_events[name] as Action<T0> - action;

            //当前类型事件为空则删除整个name
            OnListenerRemoving(name);
        }
        [Obsolete]
        public static void RemoveListener<T0, T1>(string name, Action<T0, T1> action)
        {
            //检测注销事件
            LogOutEvent(name, action);

            //注销委托
            m_events[name] = m_events[name] as Action<T0, T1> - action;

            //当前类型事件为空则删除整个name
            OnListenerRemoving(name);

        }
        [Obsolete]
        public static void RemoveListener<T0, T1, T2>(string name, Action<T0, T1, T2> action)
        {
            //检测注销事件
            LogOutEvent(name, action);

            //注销委托
            m_events[name] = m_events[name] as Action<T0, T1, T2> - action;

            //当前类型事件为空则删除整个name
            OnListenerRemoving(name);

        }

        /// <summary>
        /// 注销事件
        /// </summary>
        #region 值类型
        /// <nameparam name="T0"></nameparam>
        /// <nameparam name="T1"></nameparam>
        /// <nameparam name="T2"></nameparam>
        /// <nameparam name="T3"></nameparam>
        #endregion
        /// <param name="name">事件类型</param>
        /// <param name="action">委托本体</param>
        [Obsolete]
        public static void RemoveListener<T0, T1, T2, T3>(string name, Action<T0, T1, T2, T3> action)
        {
            //检测注销事件
            LogOutEvent(name, action);

            //注销委托
            m_events[name] = m_events[name] as Action<T0, T1, T2, T3> - action;

            //当前类型事件为空则删除整个name
            OnListenerRemoving(name);
        }

        /// <summary>
        /// 删除这个名字下所有的事件
        /// </summary>
        /// <param name="name">事件名</param>
        [Obsolete]
        public static void RemoveAllListener(string name)
        {
            m_events[name] = null;
        }

        /// <summary>
        /// 根据事件类型注销委托
        /// </summary>
        /// <param name="name">事件类型</param>
        /// <param name="checkEvent">被检查的委托</param>
        /// <returns></returns>
        private static void LogOutEvent(string name, Delegate checkEvent)
        {
            Delegate ev = m_events.TryGetValue(name);
            if (ev == null)
            {
                throw new NullReferenceException("事件中的委托为空");
            }
            else if (ev.GetType() != checkEvent.GetType())
            {
                throw new NullReferenceException($"注销事件失败，因事件类型与需要注销的事件类型不符，事件类型为{ev.GetType()},需要注销的事件类型为{checkEvent.GetType()}");
            }
        }

        /// <summary>
        /// 删除整个事件
        /// </summary>
        /// <param name="name">类型</param>
        private static void OnListenerRemoving(string name)
        {
            if (m_events[name] == null) m_events.Remove(name);
        }

        #endregion

        #region 广播事件 BroadCastEvent
        [Obsolete]
        public static void BroadCast(string name)
        {
            Action callBack = m_events.TryGetValue(name) as Action;
            callBack?.Invoke();
        }
        [Obsolete]
        public static void BroadCast<T0>(string name, T0 arg1)
        {
            Action<T0> callBack = m_events.TryGetValue(name) as Action<T0>;
            callBack?.Invoke(arg1);
        }
        [Obsolete]
        public static void BroadCast<T0, T1>(string name, T0 arg1, T1 arg2)
        {
            Action<T0, T1> callBack = m_events.TryGetValue(name) as Action<T0, T1>;
            callBack?.Invoke(arg1, arg2);
        }
        [Obsolete]
        public static void BroadCast<T0, T1, T2>(string name, T0 arg1, T1 arg2, T2 arg3)
        {
            Action<T0, T1, T2> callBack = m_events.TryGetValue(name) as Action<T0, T1, T2>;
            callBack?.Invoke(arg1, arg2, arg3);
        }

        /// <summary>
        /// 广播事件
        /// </summary>
        #region 参数类型
        /// <nameparam name="T0"></nameparam>
        /// <nameparam name="T1"></nameparam>
        /// <nameparam name="T2"></nameparam>
        /// <nameparam name="T3"></nameparam>
        #endregion
        /// <param name="name">事件类型</param>
        #region 参数
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="arg3"></param>
        /// <param name="arg4"></param>
        #endregion
        public static void BroadCast<T0, T1, T2, T3>(string name, T0 arg1, T1 arg2, T2 arg3, T3 arg4)
        {
            Action<T0, T1, T2, T3> callBack = m_events.TryGetValue(name) as Action<T0, T1, T2, T3>;
            callBack?.Invoke(arg1, arg2, arg3, arg4);
        }
        #endregion
    }

    /// <summary>
    /// 拓展字典
    /// </summary>
    public static class DictionaryExpansion
    {
        public static TValue TryGetValue<Tkey, TValue>(this Dictionary<Tkey, TValue> dict, Tkey key)
        {
            if (!dict.TryGetValue(key, out TValue value))
            {
                throw new NullReferenceException($"类型本体不存在,请尝试添加key: {key}");
            }
            return value;
        }
    }
}
