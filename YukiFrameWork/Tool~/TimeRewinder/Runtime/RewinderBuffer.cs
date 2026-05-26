///=====================================================
/// - FileName:      RewinderBuffer.cs
/// - NameSpace:     YukiFrameWork.Example
/// - Description:   高级定制脚本生成
/// - Creation Time: 2026/5/23 16:21:00
/// -  (C) Copyright 2008 - 2030
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
namespace YukiFrameWork.Rewinder
{
    public class RewinderBuffer<TData>
    {
        private int capacity;

        private TData[] buffer;
        private float recordSeconds;
        private int position = -1;
        private int lastReadBackIndex;
        public float RecordTime { get;} 

        public RewinderBuffer(float recordTime)
        {
            this.RecordTime = recordTime;
            this.recordSeconds = 1 / Time.fixedDeltaTime;
            capacity = (int)(recordSeconds * RecordTime);
            lastReadBackIndex = -1;
            buffer = new TData[capacity];
        }
        
       
        
        /// <summary>
        /// 往最后写入数据
        /// </summary>
        /// <param name="value"></param>
        public void WriteLastValue(TData value)
        {
            position++;

            if (position >= capacity)
            {
                position = 0;
            }
            buffer[position] = value;
        }

        /// <summary>
        /// 往最后写入数据,如果没有数据则返回False,否则True且值为最后写入的数据
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryReadLastValue(out TData value)
        {
            if (position < 0)
            {
                value = default;
                return false;
            }

            value = buffer[position];
            return true;
        }

        /// <summary>
        /// 读取指定秒数的数据
        /// </summary>
        /// <param name="seconds"></param>
        /// <returns></returns>
        public TData ReadValue(float seconds)
        {
            var index = CalculateIndex(seconds);
            return  buffer[index];
        }

        /// <summary>
        /// 读取指定秒数的数据,如读取值与上次读取相同,则返回False
        /// </summary>
        /// <param name="seconds"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool ReadValueDifferent(float seconds,out TData value)
        {
            var index = CalculateIndex(seconds);
            var diff = lastReadBackIndex != index;
            lastReadBackIndex = index;
            value = buffer[index];
            return diff;
        }

        private int CalculateIndex(float seconds)
        {
            int howManyBeforeLast = (int)(recordSeconds * (seconds - 0.001));
            int moveBy = position - howManyBeforeLast;
       
            if (moveBy < 0)
            {
                return capacity + moveBy;
            }
            else
            {
                return position - howManyBeforeLast;
            }
        }



    }
}
