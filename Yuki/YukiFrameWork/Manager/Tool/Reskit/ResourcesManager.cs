
using System.Collections.Generic;
using UnityEngine;
using YukiFrameWork.Pools;

namespace YukiFrameWork.Res
{
    /// <summary>
    /// 加载优先级
    /// </summary>
    public enum LoadResPriority
    {
        Res_Height = 0,
        Res_MIDDLE,
        Res_SLOW,
        Res_NUM
    }    
    [System.Serializable]
    public struct PathData
    {
        public string path;
        public uint crc;
    }

    public class AsyncLoadMoreResParam
    {
        public List<AsyncCallBack<List<Object>>> asyncCallBacks { get; private set; } = new List<AsyncCallBack<List<Object>>>();

        //路径信息
        public List<PathData> PathDatas { get; set; } = new List<PathData>();
        public LoadResPriority Priority { get; set; } = LoadResPriority.Res_SLOW;
        public bool IsSprite
        {
            get
            {
                return type != null && type == typeof(Sprite);
            }
        }
        public System.Type type { get; set; } = null;
        public void Reset()
        {
            asyncCallBacks.Clear();
            PathDatas.Clear();
            type = null;
            Priority = LoadResPriority.Res_SLOW;
        }
    }

    public class AsyncLoadResParam
    {
        public List<AsyncCallBack<Object>> asyncCallBacks { get; set; } = new List<AsyncCallBack<Object>>();
        public uint Crc { get; set; } = 0;
        public string Path { get; set; } = string.Empty;
        public bool IsSprite
        {
            get
            {
                return type != null && type == typeof(Sprite);
            }
        }
        public System.Type type { get; set; } = null;
        public LoadResPriority Priority { get; set; } = LoadResPriority.Res_SLOW;

        public void Reset()
        {
            asyncCallBacks.Clear();
            Crc = 0;
            Path = string.Empty;           
            Priority = LoadResPriority.Res_SLOW;
            type = null;
        }
    }

    public class AsyncCallBack<T>
    {
        public event System.Action<T,object,object,object> OnFinish = null;
        public object Arg1 { get; set; } = null;
        public object Arg2 { get; set; } = null;
        public object Arg3 { get; set; } = null;
        public void Reset()
        {
            Arg1 = Arg2 = Arg3 = null;
            OnFinish = null;
        }

        public void Invoke(T obj)
            => OnFinish?.Invoke(obj, Arg1, Arg2, Arg3);
    }

    /// <summary>
    /// 双向链表结构节点
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DoubleLinkedListNode<T> where T : class, new()
    {
        //前一个节点
        public DoubleLinkedListNode<T> prev = null;

        //后一个节点
        public DoubleLinkedListNode<T> next = null;

        //当前节点
        public T t = null;
    }

    /// <summary>
    /// 双向链表结构
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DoubleLinkList<T> where T : class, new()
    {
        /// <summary>
        /// 表头
        /// </summary>
        public DoubleLinkedListNode<T> Head = null;

        ///表尾
        public DoubleLinkedListNode<T> Tail = null;

        protected SimpleObjectPools<DoubleLinkedListNode<T>> simpleObjectPools
            = new SimpleObjectPools<DoubleLinkedListNode<T>>(() =>
            {
                return new DoubleLinkedListNode<T>();
            }, null, 500, 500);

        protected int count = 0;
        public int Count => count;

        /// <summary>
        /// 添加一个节点到头部
        /// </summary>
        /// <param name="t">节点</param>
        /// <returns>返回链表</returns>
        public DoubleLinkedListNode<T> AddToHeader(T t)
        {
            DoubleLinkedListNode<T> pList = simpleObjectPools.Get();
            pList.next = null;
            pList.prev = null;
            pList.t = t;
            return AddToHeader(pList);
        }

        public DoubleLinkedListNode<T> AddToHeader(DoubleLinkedListNode<T> pList)
        {
            if (pList == null) return null;

            pList.prev = null;

            if (Head == null)
                Head = Tail = pList;
            else
            {
                pList.next = Head;
                Head.prev = pList;
                Head = pList;
            }

            count++;
            return Head;
        }

        public DoubleLinkedListNode<T> AddToTail(T t)
        {
            DoubleLinkedListNode<T> pList = simpleObjectPools.Get();
            pList.next = pList.prev = null;
            pList.t = t;
            return AddToTail(pList);
        }

        /// <summary>
        /// 添加一个节点到尾部
        /// </summary>
        /// <param name="pList"></param>
        /// <returns></returns>
        public DoubleLinkedListNode<T> AddToTail(DoubleLinkedListNode<T> pList)
        {
            if (pList == null) return null;

            pList.next = null;

            if (Tail == null)
            {
                Head = Tail = pList;
            }
            else
            {
                pList.prev = Tail;
                Tail.next = pList;
                Tail = pList;
            }
            count++;
            return Tail;
        }

        public void RemoveNode(DoubleLinkedListNode<T> pList)
        {
            if (pList == null) return;

            if (pList == Head)
                Head = pList.next;

            if (pList == Tail)
                Tail = pList.prev;

            if (pList.prev != null)
                pList.prev.next = pList.next;

            if (pList.next != null)
                pList.next.prev = pList.prev;

            pList.next = pList.prev = null;
            pList.t = null;
            simpleObjectPools.Release(pList);
            count--;
        }

        public void MoveToHead(DoubleLinkedListNode<T> pList)
        {
            if (pList == null || pList == Head)
                return;

            if (pList.prev == null && pList.next == null)
                return;

            if (pList == Tail)
                Tail = pList.prev;

            if (pList.prev != null)
                pList.prev.next = pList.next;

            if (pList.next != null)
                pList.next.prev = pList.next;

            pList.prev = null;
            pList.next = Head;
            Head.prev = pList;
            Head = pList;

            if (Tail == null)
                Tail = Head;
        }
    }

    public class CMapList<T> where T : class, new()
    {
        private DoubleLinkList<T> m_Dlink = new DoubleLinkList<T>();

        private Dictionary<T, DoubleLinkedListNode<T>> m_FindMap = new Dictionary<T, DoubleLinkedListNode<T>>();

        ~CMapList()
        {
            Clear();
        }


        public void Clear()
        {
            while (m_Dlink.Tail != null)
            {
                Remove(m_Dlink.Tail.t);
            }
        }

        public void InsertToHead(T t)
        {
            if (m_FindMap.TryGetValue(t, out var node))
            {
                m_Dlink.AddToHeader(node);
                return;
            }

            m_Dlink.AddToHeader(t);
            m_FindMap.Add(t, m_Dlink.Head);
        }

        public void Pop()
        {
            if (m_Dlink.Tail != null)
            {
                Remove(m_Dlink.Tail.t);
            }
        }

        public void Remove(T t)
        {
            if (!m_FindMap.TryGetValue(t, out var node) || node == null)
            {
                return;
            }

            m_Dlink.RemoveNode(node);
            m_FindMap.Remove(t);
        }

        public T Back()
        {
            return m_Dlink.Tail?.t;
        }

        public int Size()
            => m_FindMap.Count;

        public bool Find(T t)
            => m_FindMap.ContainsKey(t);

        /// <summary>
        /// 刷新节点，把某个节点移动到头部
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public bool Reflesh(T t)
        {
            if (!m_FindMap.TryGetValue(t, out var node) || node == null) return false;

            m_Dlink.MoveToHead(node);
            return true;

        }
    }
}