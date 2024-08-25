///=====================================================
/// - FileName:      YDictionary.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/3/6 3:42:45
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using YukiFrameWork.Extension;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Linq;

namespace YukiFrameWork
{
    [Serializable]
    [ClassAPI("可支持面板序列化的字典")]    
    public class YDictionary
	{
#if UNITY_EDITOR
        public static SerializedProperty GetKeyProperty(SerializedProperty YDictProperty, int index)
        {
            return GetPairProperty(YDictProperty, index, "_key");
        }

        public static SerializedProperty GetValueProperty(SerializedProperty YDictProperty, int index)
        {
            return GetPairProperty(YDictProperty, index, "_value");
        }

        public static SerializedProperty GetListProperty(SerializedProperty YDictProperty)
        {
            return YDictProperty.FindPropertyRelative("list");
        }

        private static SerializedProperty GetPairProperty(SerializedProperty YDicProperty, int index,string valueName)
        {
            return GetListProperty(YDicProperty).GetArrayElementAtIndex(index).FindPropertyRelative(valueName);
        }
#endif
    }   
    [Serializable]    
    public class YDictionary<TKey, TValue> : YDictionary, ISerializationCallbackReceiver, IDictionary<TKey, TValue>,IEnumerable, IEnumerable<KeyValuePair<TKey, TValue>>,IBindableProperty<KeyValuePair<TKey,TValue>>
    {
        [Serializable]
        public struct Y_KeyValuePair
        {
            public TKey _key;
            public TValue _value;

            public Y_KeyValuePair(TKey key, TValue value)
            {
                _key = key;
                _value = value;
            }
        }      
        [SerializeField]       
        [InfoBox("请注意在添加过程中不允许出现相同的键!",infoMessageType:InfoMessageType.Warning)]      
        private List<Y_KeyValuePair> list = new List<Y_KeyValuePair>();
        private Dictionary<TKey, int> dictPosition = new Dictionary<TKey, int>();  
        public TValue this[TKey key]
        {
            get => list[dictPosition[key]]._value;
            set
            {
                var pair = new Y_KeyValuePair(key,value);
                if (dictPosition.ContainsKey(key))
                {
                    list[dictPosition[key]] = pair;
                }
                else
                {
                    dictPosition[key] = list.Count;
                    list.Add(pair);
                }
            }
        }

        public ICollection<TKey> Keys => list.Select(x => x._key).ToArray();

        public ICollection<TValue> Values => list.Select(x => x._value).ToArray();

        public int Count => list.Count;

        public bool IsReadOnly => false;

        public void Add(TKey key, TValue value)
        {
            if (dictPosition.ContainsKey(key))
                throw new ArgumentException("这个字典存在相同的键值无法添加! Key:" + key);
            else
            {
                dictPosition[key] = list.Count;
                list.Add(new Y_KeyValuePair(key, value));
            }
        }

        public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);
       

        public void Clear()
        {
            list.Clear();
            dictPosition.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
            => ContainsKey(item.Key);

        public bool ContainsKey(TKey key)       
            => dictPosition.ContainsKey(key);
        
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            int index = list.Count;
            if (array.Length - arrayIndex < index)
                throw new ArgumentException("数组超出索引范围");
            for (int i = 0; i < index; i++, arrayIndex++)
            {
                var entry = list[i];
                array[arrayIndex] = new KeyValuePair<TKey, TValue>(entry._key,entry._value);
            }
        }

        public static implicit operator YDictionary<TKey, TValue>(Dictionary<TKey,TValue> dictionary)
        {
            return dictionary.ToYDictionary();
        }

        public static implicit operator Dictionary<TKey, TValue>(YDictionary<TKey, TValue> yDictionary)
        {
            return yDictionary.ToDictionary();
        }
        
        private KeyValuePair<TKey,TValue> ToKeyValuePair(Y_KeyValuePair pair)
        {
            return new KeyValuePair<TKey, TValue>(pair._key,pair._value);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return list.Select(ToKeyValuePair).GetEnumerator();
        }

        private Dictionary<TKey, int> InitDictPosition()
        {
            var dict = new Dictionary<TKey, int>(list.Count);
            for (int i = 0; i < list.Count; i++)
            {
                dict[list[i]._key] = i;
            }          
            return dict;
        }

        public void OnAfterDeserialize()
        {
            dictPosition = InitDictPosition();
        }

        public void OnBeforeSerialize()
        {
           
        }
        public bool Remove(TKey key,out TValue value)
        {
            value = default;
            if (dictPosition.TryGetValue(key, out int index))
            {
                dictPosition.Remove(key);
                value = list[index]._value;
                list.RemoveAt(index);

                for (int i = 0; i < list.Count; i++)
                {
                    dictPosition[list[i]._key] = i;
                }
                return true;
            }
            return false;
        }

        public bool Remove(TKey key)
        {
            return Remove(key, out _);
        }
        public bool Remove(KeyValuePair<TKey, TValue> item)
            => Remove(item.Key);

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (dictPosition.TryGetValue(key, out int index))
            {
                value = list[index]._value;
                return true;
            }
            value = default(TValue);
            return false;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IUnRegister Register(Action<KeyValuePair<TKey, TValue>> action)
        {
            return onDictValueChanged.RegisterEvent(action);
        }

        public IUnRegister RegisterWithInitValue(Action<KeyValuePair<TKey, TValue>> action)
        {
            foreach (var item in list)
            {
                action?.Invoke(new KeyValuePair<TKey, TValue>(item._key,item._value));
            }
            return onDictValueChanged.RegisterEvent(action);
        }

        public void UnRegisterAllEvent()
        {
            onDictValueChanged.UnRegisterAllEvent();
        }

        private EasyEvent<KeyValuePair<TKey, TValue>> onDictValueChanged = new EasyEvent<KeyValuePair<TKey, TValue>>();
    }

    public static class YDictionaryExtension
    {
        public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this YDictionary<TKey, TValue> y)
        {
            Dictionary<TKey, TValue> newDict = new Dictionary<TKey, TValue>();
            foreach (var key in y.Keys)
            {
                var value = y[key];
                newDict[key] = value;
            }
            return newDict;
        }

        public static YDictionary<TKey, TValue> ToYDictionary<TKey, TValue>(this Dictionary<TKey, TValue> y)
        {
            YDictionary<TKey, TValue> newDict = new YDictionary<TKey, TValue>();
            foreach (var key in y.Keys)
            {
                var value = y[key];
                newDict[key] = value;
            }
            return newDict;
        }
    }
}
