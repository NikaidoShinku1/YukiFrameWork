///=====================================================
/// - FileName:      DictionaryPools.cs
/// - NameSpace:     YukiFrameWork.Project
/// - Created:       Yuki
/// - Email:         Yuki@qq.com
/// - Description:   这是一个框架工具创建的脚本
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================

using UnityEngine;
using System;
using System.Collections.Generic;

namespace YukiFrameWork.Pools
{
    /// <summary>
    /// 字典对象池
    /// </summary>
    /// <typeparam name="TKey">键</typeparam>
    /// <typeparam name="TValue">值</typeparam>
    public static class DictionaryPools<TKey, TValue>
    {
        public static SimpleObjectPools<Dictionary<TKey, TValue>> dictionaryPools
            = new SimpleObjectPools<Dictionary<TKey, TValue>>(() => new Dictionary<TKey, TValue>(), 10);

        public static Dictionary<TKey, TValue> Get()
            => dictionaryPools.Get();

        public static void ReleaseDict(Dictionary<TKey, TValue> keyValuePairs)
        {
            if (dictionaryPools.Contains(keyValuePairs))
            {
                Debug.LogError($"重复回收，已经存在的字典：{keyValuePairs.GetType()}");
                return;
            }

            keyValuePairs.Clear();
            dictionaryPools.Release(keyValuePairs);
        }
    }

}