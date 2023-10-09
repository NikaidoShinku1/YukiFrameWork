using System;
using System.Collections.Generic;

namespace YukiFrameWork.Events
{
    public interface IEventCenter
    {
        
    }

    public interface IBroadCast
    {
        
    }

    /// <summary>
    /// �¼�����
    /// </summary>
    public static class EventCenter
    {
        //��������
        private static readonly Dictionary<string, Delegate> m_events = new Dictionary<string, Delegate>();

        #region ��������ע������¼� RegisterEvent
        public static void AddListener(this IEventCenter eventCenter,string name, Action action)
        {
            //���ע���¼�
            RegisterEvent(name, action);

            //���ί��
            m_events[name] = m_events[name] as Action + action;
        }

        public static void AddListener<T0>(this IEventCenter eventCenter,string name, Action<T0> action)
        {
            //���ע���¼�
            RegisterEvent(name, action);

            //���ί��
            m_events[name] = m_events[name] as Action<T0> + action;
        }

        public static void AddListener<T0, T1>(this IEventCenter eventCenter,string name, Action<T0, T1> action)
        {
            //���ע���¼�
            RegisterEvent(name, action);

            //���ί��
            m_events[name] = m_events[name] as Action<T0, T1> + action;
        }

        public static void AddListener<T0, T1, T2>(this IEventCenter eventCenter,string name, Action<T0, T1, T2> action)
        {
            //���ע���¼�
            RegisterEvent(name, action);

            //���ί��
            m_events[name] = m_events[name] as Action<T0, T1, T2> + action;
        }

        /// <summary>
        /// ע���¼�
        /// </summary>
        #region ��������
        /// <nameparam name="T0"></nameparam>
        /// <nameparam name="T1"></nameparam>
        /// <nameparam name="T2"></nameparam>
        /// <nameparam name="T3"></nameparam>
        #endregion
        /// <param name="name">�¼�����</param>
        /// <param name="action">ί�б���</param>
        public static void AddListener<T0, T1, T2, T3>(this IEventCenter eventCenter,string name, Action<T0, T1, T2, T3> action)
        {
            //���ע���¼�
            RegisterEvent(name, action);

            //���ί��
            m_events[name] = m_events[name] as Action<T0, T1, T2, T3> + action;
        }

        /// <summary>
        /// �����¼�����ע���Ӧ�¼�
        /// </summary>
        /// <param name="name">�¼�����</param>
        /// <param name="checkEvent">������ί��</param>
        private static void RegisterEvent(string name, Delegate checkEvent)
        {
            //��ѯ�����Ƿ����
            if (!m_events.ContainsKey(name)) m_events.Add(name, null);

            Delegate ev = m_events[name];

            //��ѯ��ǰ�����Ƿ������¼�
            if (ev != null)
            {
                //�ж�Ҫ�������¼�����ӵ���¼������Ƿ�һ��
                if (ev.GetType() != checkEvent.GetType())
                {
                    throw new NullReferenceException($"ע���¼�ʧ�ܣ���ǰί�����¼�����Ϊ{ev.GetType()},Ҫ��ӵ��¼�����Ϊ{checkEvent.GetType()}");
                }
            }
        }
        #endregion

        #region ��������ע�������¼� LogOutEvent
        public static void RemoveListener(this IEventCenter eventCenter,string name, Action action)
        {
            //���ע���¼�
            LogOutEvent(name, action);

            //ע��ί��
            m_events[name] = m_events[name] as Action - action;

            //��ǰ�����¼�Ϊ����ɾ�������¼�
            OnListenerRemoving(name);
        }

        public static void RemoveListener<T0>(this IEventCenter eventCenter,string name, Action<T0> action)
        {
            //���ע���¼�
            LogOutEvent(name, action);

            //ע��ί��
            m_events[name] = m_events[name] as Action<T0> - action;

            //��ǰ�����¼�Ϊ����ɾ������name
            OnListenerRemoving(name);
        }

        public static void RemoveListener<T0, T1>(this IEventCenter eventCenter,string name, Action<T0, T1> action)
        {
            //���ע���¼�
            LogOutEvent(name, action);

            //ע��ί��
            m_events[name] = m_events[name] as Action<T0, T1> - action;

            //��ǰ�����¼�Ϊ����ɾ������name
            OnListenerRemoving(name);

        }

        public static void RemoveListener<T0, T1, T2>(this IEventCenter eventCenter,string name, Action<T0, T1, T2> action)
        {
            //���ע���¼�
            LogOutEvent(name, action);

            //ע��ί��
            m_events[name] = m_events[name] as Action<T0, T1, T2> - action;

            //��ǰ�����¼�Ϊ����ɾ������name
            OnListenerRemoving(name);

        }

        /// <summary>
        /// ע���¼�
        /// </summary>
        #region ֵ����
        /// <nameparam name="T0"></nameparam>
        /// <nameparam name="T1"></nameparam>
        /// <nameparam name="T2"></nameparam>
        /// <nameparam name="T3"></nameparam>
        #endregion
        /// <param name="name">�¼�����</param>
        /// <param name="action">ί�б���</param>
        public static void RemoveListener<T0, T1, T2, T3>(this IEventCenter eventCenter,string name, Action<T0, T1, T2, T3> action)
        {
            //���ע���¼�
            LogOutEvent(name, action);

            //ע��ί��
            m_events[name] = m_events[name] as Action<T0, T1, T2, T3> - action;

            //��ǰ�����¼�Ϊ����ɾ������name
            OnListenerRemoving(name);
        }

        /// <summary>
        /// ɾ��������������е��¼�
        /// </summary>
        /// <param name="name">�¼���</param>
        public static void RemoveAllListener(this IEventCenter eventCenter,string name)
        {
            m_events[name] = null;
        }

        /// <summary>
        /// �����¼�����ע��ί��
        /// </summary>
        /// <param name="name">�¼�����</param>
        /// <param name="checkEvent">������ί��</param>
        /// <returns></returns>
        private static void LogOutEvent(string name, Delegate checkEvent)
        {
            Delegate ev = m_events.TryGetValue(name);
            if (ev == null)
            {
                throw new NullReferenceException("�¼��е�ί��Ϊ��");
            }
            else if (ev.GetType() != checkEvent.GetType())
            {
                throw new NullReferenceException($"ע���¼�ʧ�ܣ����¼���������Ҫע�����¼����Ͳ������¼�����Ϊ{ev.GetType()},��Ҫע�����¼�����Ϊ{checkEvent.GetType()}");
            }
        }

        /// <summary>
        /// ɾ�������¼�
        /// </summary>
        /// <param name="name">����</param>
        private static void OnListenerRemoving(string name)
        {
            if (m_events[name] == null) m_events.Remove(name);
        }

        #endregion

        #region �㲥�¼� BroadCastEvent
        public static void BroadCast(this IBroadCast broadCast, string name)
        {
            Action callBack = m_events.TryGetValue(name) as Action;
            callBack?.Invoke();
        }

        public static void BroadCast<T0>(this IBroadCast broadCast,string name, T0 arg1)
        {
            Action<T0> callBack = m_events.TryGetValue(name) as Action<T0>;
            callBack?.Invoke(arg1);
        }

        public static void BroadCast<T0, T1>(this IBroadCast broadCast, string name, T0 arg1, T1 arg2)
        {
            Action<T0, T1> callBack = m_events.TryGetValue(name) as Action<T0, T1>;
            callBack?.Invoke(arg1, arg2);
        }

        public static void BroadCast<T0, T1, T2>(this IBroadCast broadCast, string name, T0 arg1, T1 arg2, T2 arg3)
        {
            Action<T0, T1, T2> callBack = m_events.TryGetValue(name) as Action<T0, T1, T2>;
            callBack?.Invoke(arg1, arg2, arg3);
        }

        /// <summary>
        /// �㲥�¼�
        /// </summary>
        #region ��������
        /// <nameparam name="T0"></nameparam>
        /// <nameparam name="T1"></nameparam>
        /// <nameparam name="T2"></nameparam>
        /// <nameparam name="T3"></nameparam>
        #endregion
        /// <param name="name">�¼�����</param>
        #region ����
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="arg3"></param>
        /// <param name="arg4"></param>
        #endregion
        public static void BroadCast<T0, T1, T2, T3>(this IBroadCast broadCast, string name, T0 arg1, T1 arg2, T2 arg3, T3 arg4)
        {
            Action<T0, T1, T2, T3> callBack = m_events.TryGetValue(name) as Action<T0, T1, T2, T3>;
            callBack?.Invoke(arg1, arg2, arg3, arg4);
        }
        #endregion
    }

    /// <summary>
    /// ��չ�ֵ�
    /// </summary>
    public static class DictionaryExpansion
    {
        public static TValue TryGetValue<Tkey, TValue>(this Dictionary<Tkey, TValue> dict, Tkey key)
        {
            if (!dict.TryGetValue(key, out TValue value))
            {
                throw new NullReferenceException($"���ͱ��岻����,�볢�����key: {key}");
            }
            return value;
        }
    }
}
