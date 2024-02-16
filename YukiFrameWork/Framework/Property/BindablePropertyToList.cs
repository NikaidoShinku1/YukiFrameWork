using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YukiFrameWork.Pools;
namespace YukiFrameWork
{
    /// <summary>
    /// 列表绑定拓展
    /// </summary>
    /// <typeparam name="T">类型</typeparam>
    [Serializable]
    public class BindablePropertyToList<T> : List<T> , IBindableProperty<T>, IEnumerable<T>, IEnumerable, IList<T>, IReadOnlyList<T>, IReadOnlyCollection<T>,IList 
    {
        [NonSerialized]
        private readonly EasyEvent<T> onListChange = new EasyEvent<T>();

        [field:SerializeField]
        private List<T> list;  
        
        public bool IsReadOnly => ((ICollection<T>)list).IsReadOnly;

        public new T this[int index]
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

        public BindablePropertyToList(IEnumerable<T> list)
        {
            this.list = list as List<T>;          
        }

        public BindablePropertyToList()
        {
            this.list = ListPools<T>.Get();
        }

        public IUnRegister Register(Action<T> onEvent)
        {           
            onListChange.RegisterEvent(onEvent);
            return onListChange;
        }
        
        public void UnRegister(Action<T> onEvent)
        {
            onListChange.UnRegister(onEvent);
        }

        public new void Add(T item)
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

        public new bool Contains(T item)
            => list.Contains(item);

        public new void Clear() 
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
        public new IEnumerator<T> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }

        public new int IndexOf(T item)
        {
            return list.IndexOf(item);
        }

        public new void Insert(int index, T item)
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

        public new void CopyTo(T[] array, int arrayIndex)
        {
            list.CopyTo(array, arrayIndex);
        }

        bool ICollection<T>.Remove(T item)
        {
            return Remove(item, false);
        }

        #region Find Item
        public new T Find(Predicate<T> match)
        {
            return list.Find(match);
        }

        public new List<T> FindAll(Predicate<T> match)
        {
            return list.FindAll(match);
        }

        public new int FindIndex(Predicate<T> match)
        {
            return FindIndex(0,Count, match);
        }

        public new int FindIndex(int startIndex, Predicate<T> match)
        {
            return FindIndex(startIndex, Count - startIndex, match);
        }

        public new int FindIndex(int startIndex, int count, Predicate<T> match)
        {
            return list.FindIndex(startIndex, count, match);
        }

        public new T FindLast(Predicate<T> match)
        {
            return list.FindLast(match);
        }

        public new int FindLastIndex(Predicate<T> match)
        {
            return FindLastIndex(Count- 1, Count, match);
        }

        public new int FindLastIndex(int startIndex, Predicate<T> match)
        {
            return FindLastIndex(startIndex, startIndex + 1, match);
        }
       
        public new int FindLastIndex(int startIndex, int count, Predicate<T> match)
        {
            return list.FindLastIndex(startIndex, count, match);
        }
       
        public new void ForEach(Action<T> action)
        {
            list.ForEach(action);
        }
        
        #endregion
    }
}
