///=====================================================
/// - FileName:      ListPools.cs
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
    /// 列表对象池
    /// </summary>
    /// <typeparam name="TResult">类型</typeparam>
    public static class ListPools<TResult>
    {
        private readonly static SimpleObjectPools<List<TResult>> listPools
            = new SimpleObjectPools<List<TResult>>
            (() => new List<TResult>(), 10);

        public static List<TResult> Get()
            => listPools.Get();

        public static void ReleaseList(List<TResult> results)
        {
            if (listPools.Contains(results))
            {
                Debug.LogError($"重复回收，已经存在的列表：{results.GetType()}");
                return;
            }

            results.Clear();
            listPools.Release(results);
        }
    }
}