///=====================================================
/// - FileName:      DyncGrid.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/7/18 1:39:16
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using System.Collections.Generic;
namespace YukiFrameWork
{
    public class DyncGrid<T> : IGrid<T>
    {
        private Dictionary<(int, int), T> mGrid = null;
        public DyncGrid()
        {
            mGrid = new Dictionary<(int, int), T>();
        }


        public void ForEach(Action<int, int, T> each)
        {
            foreach (var kvp in mGrid)
            {
                each(kvp.Key.Item1, kvp.Key.Item2, kvp.Value);
            }
        }

        public void ForEach(Action<T> each)
        {
            foreach (var kvp in mGrid)
            {
                each(kvp.Value);
            }
        }

        public T this[int xIndex, int yIndex]
        {
            get
            {
               
                return mGrid.TryGetValue((xIndex,yIndex), out var value) ? value : default;
            }
            set
            {
                var key = new Tuple<int, int>(xIndex, yIndex);
                mGrid[(xIndex, yIndex)] = value;
            }
        }

        public void Clear(Action<T> cleanupItem = null)
        {
            mGrid.Clear();
        }
    }
}
