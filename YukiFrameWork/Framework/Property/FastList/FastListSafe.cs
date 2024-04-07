using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace YukiFrameWork
{
    /// <summary>
    /// 线程安全的List类, 无序的, 极速的
    /// </summary>
    /// <typeparam name="T"></typeparam>
   
    [DebuggerDisplay("Count = {Count}")]
    [Serializable]
  
    public class FastListSafe<T> : FastList<T>
    {
        public override void Add(T item, out int index)
        {
            lock (SyncRoot)
            {
                base.Add(item, out index);
            }
        }

        public override void AddRange(IEnumerable<T> collection)
        {
            lock (SyncRoot)
            {
                base.AddRange(collection);
            }
        }

        public override int BinarySearch(int index, int count, T item, IComparer<T> comparer)
        {
            lock (SyncRoot)
            {
                return base.BinarySearch(index, count, item, comparer);
            }
        }

        public override void Clear()
        {
            lock (SyncRoot) 
            {
                base.Clear();
            }
        }

        public override void CopyTo(T[] array)
        {
            CopyTo(array, 0);
        }

        public override void CopyTo(int index, T[] array, int arrayIndex, int count)
        {
            lock (SyncRoot)
            {
                base.CopyTo(index, array, arrayIndex, count);
            }
        }


        public override void CopyTo(T[] array, int arrayIndex)
        {
            lock (SyncRoot)
            {
                base.CopyTo(array, arrayIndex);
            }
        }

        public override void ForEach(Action<T> action)
        {
            lock (SyncRoot) 
            {
                base.ForEach(action);
            }
        }

        public override T[] GetRange(int index, int count)
        {
            lock (SyncRoot)
            {
                return base.GetRange(index, count);
            }
        }

        /// <summary>
        /// 获取列表对象, 并移除列表, 如果在多线程下, 多线程并行下, 是可以获取到对象, 但是会出现长度不是所指定的长度, 所以获取后要判断一下长度
        /// </summary>
        /// <param name="index"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public override T[] GetRemoveRange(int index, int count)
        {
            lock (SyncRoot)
            {
                return base.GetRemoveRange(index, count);
            }
        }

        public override void Insert(int index, T item)
        {
            lock (SyncRoot)
            {
                base.Insert(index, item);
            }
        }

        public override void InsertRange(int index, IEnumerable<T> collection)
        {
            lock (SyncRoot)
            {
                base.InsertRange(index, collection);
            }
        }

        public override bool Remove(T item)
        {
            lock (SyncRoot)
            {
                return base.Remove(item);
            }
        }

        public override int RemoveAll(Predicate<T> match)
        {
            lock (SyncRoot)
            {
                return base.RemoveAll(match);
            }
        }


        public override void RemoveAt(int index)
        {
            lock (SyncRoot)
            {
                base.RemoveAt(index);
            }
        }

        public override void RemoveRange(int index, int count)
        {
            lock (SyncRoot)
            {
                base.RemoveRange(index, count);
            }
        }

        public override void Reverse()
        {
            Reverse(0, Count);
        }

        public override void Reverse(int index, int count)
        {
            lock (SyncRoot)
            {
                base.Reverse(index, count);
            }
        }

        public override void Sort()
        {
            Sort(0, Count, null);
        }

        public override void Sort(IComparer<T> comparer)
        {
            Sort(0, Count, comparer);
        }

        public override void Sort(int index, int count, IComparer<T> comparer)
        {
            lock (SyncRoot)
            {
                base.Sort(index, count, comparer);
            }
        }

        public override void Sort(Comparison<T> comparison)
        {
            lock (SyncRoot)
            {
                base.Sort(comparison);
            }
        }

        public override T[] ToArray()
        {
            lock (SyncRoot)
            {
                return base.ToArray();
            }
        }
    }
}