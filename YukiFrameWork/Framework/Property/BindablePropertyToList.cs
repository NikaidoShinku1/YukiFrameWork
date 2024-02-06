using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using YukiFrameWork.Pools;
using System.Threading;

namespace YukiFrameWork
{
    /// <summary>
    /// 列表绑定拓展
    /// </summary>
    /// <typeparam name="T">类型</typeparam>
    [Serializable]
    public class BindablePropertyToList<T> : IBindableProperty<T>, IEnumerable<T>, IEnumerable, IList<T>, IReadOnlyList<T>, IReadOnlyCollection<T>       
    {
        [NonSerialized]
        private readonly EasyEvent<T> onListChange = new EasyEvent<T>();

        [field:SerializeField]
        private List<T> list;

        public int Count => list.Count;

        public bool IsReadOnly => ((ICollection<T>)list).IsReadOnly;

        public T this[int index]
        {
            get => list[index];
            set
            {
                if (!Equals(value, list[index]))
                {
                    list[index] = value;
                    onListChange.SendEvent(value);
                }
            }
        }

        public BindablePropertyToList(List<T> list)
        {
            this.list = list;          
        }

        public BindablePropertyToList()
        {
            this.list = ListPools<T>.Get();
        }

        public IUnRegister Register(Action<T> onEvent)
        {
            return onListChange.RegisterEvent(onEvent);
        }
        
        public void UnRegister(Action<T> onEvent)
        {
            onListChange.UnRegister(onEvent);
        }

        public void Add(T item)
        {
            onListChange.SendEvent(item);
            list.Add(item);
        }

        public bool Remove(T item,bool sendEvent = true)
        {
            if(sendEvent)
                onListChange.SendEvent(item);
            return list.Remove(item);
        }

        public bool Contains(T item)
            => list.Contains(item);

        public void Clear() 
        {           
            list.Clear();           
        }

        public IUnRegister RegisterWithInitValue(Action<T> action)
        {
            onListChange.RegisterEvent(action);
            foreach (var value in list)
                onListChange.SendEvent(value);
            return onListChange;
        }

        public void UnRegisterAllEvent()
        {
            onListChange.UnRegisterAllEvent();  
        }
        public IEnumerator<T> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return list.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            list.Insert(index, item);
        }

        public void RemoveAt(int index, bool sendEvent = true)
        {
            try
            {
                if (sendEvent) onListChange.SendEvent(list[index]);                             
            }
            catch { }
            list.RemoveAt(index);
        }

        void IList<T>.RemoveAt(int index)
        {
            RemoveAt(index, false);
        }        

        public void CopyTo(T[] array, int arrayIndex)
        {
            list.CopyTo(array, arrayIndex);
        }

        bool ICollection<T>.Remove(T item)
        {
            return Remove(item, false);
        }

        #region Find Item
        public T Find(Predicate<T> match)
        {
            return list.Find(match);
        }
      
        public List<T> FindAll(Predicate<T> match)
        {
            return list.FindAll(match);
        }
       
        public int FindIndex(Predicate<T> match)
        {
            return FindIndex(0,Count, match);
        }
       
        public int FindIndex(int startIndex, Predicate<T> match)
        {
            return FindIndex(startIndex, Count - startIndex, match);
        }
    
        public int FindIndex(int startIndex, int count, Predicate<T> match)
        {
            return list.FindIndex(startIndex, count, match);
        }
       
        public T FindLast(Predicate<T> match)
        {
            return list.FindLast(match);
        }
       
        public int FindLastIndex(Predicate<T> match)
        {
            return FindLastIndex(Count- 1, Count, match);
        }
       
        public int FindLastIndex(int startIndex, Predicate<T> match)
        {
            return FindLastIndex(startIndex, startIndex + 1, match);
        }
       
        public int FindLastIndex(int startIndex, int count, Predicate<T> match)
        {
            return list.FindLastIndex(startIndex, count, match);
        }
       
        public void ForEach(Action<T> action)
        {
            list.ForEach(action);
        }
        #endregion
    }
}
