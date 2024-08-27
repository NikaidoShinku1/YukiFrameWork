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
using Newtonsoft.Json;
namespace YukiFrameWork
{
    [Serializable]
    public abstract class TableKit<TItemKey, TItem> : IEnumerable<KeyValuePair<TItemKey, List<TItem>>>, IDisposable, IBindableProperty<KeyValuePair<TItemKey, List<TItem>>>
    {
        [JsonIgnore]
        public abstract IDictionary<TItemKey, List<TItem>> Table { get; }
        public ICollection<TItemKey> Keys => Table.Keys;
        [JsonIgnore]
        public int Count => Table.Count;
        [JsonIgnore]
        public bool IsReadOnly => false;
        [JsonIgnore]
        public ICollection<List<TItem>> Values => Table.Values;
        [JsonIgnore]
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
            onTableValueChanged.SendEvent(new KeyValuePair<TItemKey, List<TItem>>(key,items));
        }

        public bool Remove(TItemKey key, TItem item)
        {
            if (TryGetValue(key, out var items))
            {
                onTableValueChanged.SendEvent(new KeyValuePair<TItemKey, List<TItem>>(key, items));
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

        public IUnRegister Register(Action<KeyValuePair<TItemKey, List<TItem>>> action)
        {
            return onTableValueChanged.RegisterEvent(action);
        }

        public IUnRegister RegisterWithInitValue(Action<KeyValuePair<TItemKey, List<TItem>>> action)
        {
            foreach (var item in Table)
            {
                action?.Invoke(item);
            }
            return onTableValueChanged.RegisterEvent(action);
        }

        public void UnRegisterAllEvent()
        {
            onTableValueChanged.UnRegisterAllEvent();
        }

        private EasyEvent<KeyValuePair<TItemKey, List<TItem>>> onTableValueChanged = new EasyEvent<KeyValuePair<TItemKey, List<TItem>>>();
    }   
}
