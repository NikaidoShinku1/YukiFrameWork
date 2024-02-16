using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YukiFrameWork.Extension;
using YukiFrameWork.Pools;


namespace YukiFrameWork
{
    /// <summary>
    /// 字典绑定拓展
    /// </summary>
    /// <typeparam name="TKey">键</typeparam>
    /// <typeparam name="TValue">值</typeparam>
    [ClassAPI("字典的可绑定属性:该拓展无法进行Xml转换")]
    [Serializable]
    public class BindablePropertyToDictionary<TKey, TValue> : IBindableProperty<TKey, TValue>,
        IEnumerable, IEnumerable<KeyValuePair<TKey, TValue>>, IDictionary<TKey, TValue>
    {
        [NonSerialized]
        private EasyEvent<TKey, TValue> onDictionaryChange = new EasyEvent<TKey, TValue>();

        [SerializeField]
        [Header("可视化字典绑定(使用可视化需要在逻辑中调用一次Update_SerializedInfo方法!)")]
        private VisualKeyValuePair[] pair = new VisualKeyValuePair[0];
        
        private Dictionary<TKey,TValue> dicts;

        public TValue this[TKey key]
        {
            get => dicts[key];
            set
            {
                if (!ContainsKey(key) || !Equals(value, dicts[key]))
                {
                    onDictionaryChange.SendEvent(key, value);
                    dicts[key] = value;
                }
            }
        }

        [Serializable]
        private struct VisualKeyValuePair
        {
            [Header("键:")]
            public TKey key;

            [Header("值:")]
            public TValue value;
        }

        public ICollection<TKey> Keys => dicts.Keys;

        public ICollection<TValue> Values => dicts.Values;

        public int Count => dicts.Count;

        public bool IsReadOnly => ((ICollection<TKey>)dicts).IsReadOnly;

        public BindablePropertyToDictionary(Dictionary<TKey, TValue> dicts)
        {
            this.dicts = dicts;           
        }
        public BindablePropertyToDictionary() 
        {
            this.dicts = DictionaryPools<TKey, TValue>.Get();                           
        }

        /// <summary>
        /// 初始化序列化过的字典
        /// </summary>
        public void Update_SerializedInfo()
        {           
            List<KeyValuePair<TKey, TValue>> keyValuePairs = ListPools<KeyValuePair<TKey, TValue>>.Get();
            for (int i = 0; i < pair.Length; i++)
            {              
                keyValuePairs.Add(new KeyValuePair<TKey, TValue>(pair[i].key, pair[i].value));
            }

            for (int i = 0; i < keyValuePairs.Count; i++)
            {
                var data = keyValuePairs[i];

                if (dicts.ContainsKey(data.Key))
                    dicts[data.Key] = data.Value;
                else dicts.Add(data.Key, data.Value);               
            }

            keyValuePairs.Release();
        }

        public IUnRegister Register(Action<TKey, TValue> action)
        {
            onDictionaryChange.RegisterEvent(action);
            return onDictionaryChange;
        }

        public IUnRegister RegisterWithInitValue(Action<TKey, TValue> action)
        {
            onDictionaryChange.RegisterEvent(action);
            SendAllEvent();
            return onDictionaryChange;
        }

        public void UnRegister(Action<TKey, TValue> action)
        {
            onDictionaryChange.UnRegister(action);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            bool contains = dicts.TryGetValue(key, out value);           
            return contains;
        }

        public bool ContainsKey(TKey key)
            => dicts.ContainsKey(key);

        public bool ContainsValue(TValue value)
            => dicts.ContainsValue(value);
        
        public void Add(TKey key,TValue value)
        {          
            dicts.Add(key, value);
            onDictionaryChange.SendEvent(key,value);
        }

        public bool Remove(TKey key,bool sendEvent = true)
        {
            if (sendEvent && ContainsKey(key))
                onDictionaryChange.SendEvent(key, dicts[key]);
            return dicts.Remove(key);
        }

        bool IDictionary<TKey,TValue>.Remove(TKey item)
        {
            return Remove(item, false);
        }

        public void Clear()
        {          
            dicts.Clear();       
        }

        private void SendAllEvent()
        {
            foreach (var key in dicts.Keys)
                onDictionaryChange.SendEvent(key, dicts[key]);
        }

        public void UnRegisterAllEvent()
        {
            onDictionaryChange.UnRegisterAllEvent();
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return dicts.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {          
            return dicts.GetEnumerator();
        }           

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
        {           
            Add(item.Key, item.Value);
        }       

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
        {
            return dicts.ContainsKey(item.Key) && dicts.ContainsValue(item.Value);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ((ICollection)dicts).CopyTo(array, arrayIndex);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            return Remove(item.Key);
        }        
    }
}
