///=====================================================
/// - FileName:      TableKit.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/5/9 14:06:36
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using System;
using System.Collections.Generic;
using System.Collections;
using YukiFrameWork.Pools;
namespace YukiFrameWork
{
    [Serializable]
    public abstract class TableKit<TItemKey,TItem> : IEnumerable<KeyValuePair<TItemKey,List<TItem>>>, IDisposable
    {
        public abstract IDictionary<TItemKey, List<TItem>> Table { get; }
        public ICollection<TItemKey> Keys => Table.Keys;      

        public int Count => Table.Count;

        public bool IsReadOnly => false;

        public ICollection<List<TItem>> Values => Table.Values;

        public List<TItem> this[TItemKey key] { get => Table[key]; set => Table[key] = value; }

        public void Dispose()
        {          
            OnDispose();           
        }

        public void Add(TItemKey key, TItem item)
        {
            if (TryGetValue(key, out var items))
            {
                items.Add(item);
            }
            else
            {
                items = ListPools<TItem>.Get();
                items.Add(item);
                Table.Add(key, items);
            }
        }

        public bool Remove(TItemKey key, TItem item)
        {
            if (TryGetValue(key, out var items))
            {                          
                return items.Remove(item);
            }
            return false;
        }

        public bool Remove(TItemKey key)
        {
            return Table.Remove(key);
        }

        public void Clear()
        {
            Table.Clear();
        }

        public bool TryGetValue(TItemKey key, out List<TItem> items)
        {
            return Table.TryGetValue(key, out items);
        }

        public bool ContainsKey(TItemKey key) => Table.ContainsKey(key);

        public bool IsNullOrEmpty(TItemKey key)
        {
            if (!Table.ContainsKey(key))
                return false;

            return Table[key] == null || Table[key].Count == 0;
        }

        protected abstract void OnDispose();      

        public IEnumerator<KeyValuePair<TItemKey, List<TItem>>> GetEnumerator()
        {
            return Table.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }   
}
