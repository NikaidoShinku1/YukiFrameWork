///=====================================================
/// - FileName:      EasyGrid.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/7/18 1:28:33
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
namespace YukiFrameWork
{
    public class EasyGrid<T> : IGrid<T>
    {
        private T[,] mGrid;      
        public int Height { get; private set; }
        public int Width { get; private set; }
        public EasyGrid(int width,int height)
        {
            this.Height = height;
            this.Width = width;
            mGrid = new T[width, height];
        }
        public T this[int xIndex, int yIndex]
        {
            get
            {
                if (xIndex >= 0 && xIndex < Width && yIndex >= 0 && yIndex < Height)
                {
                    return mGrid[xIndex, yIndex];
                }
                else
                {
                    LogKit.W($"out of bounds [{xIndex}:{yIndex}] in grid[{Width}:{Height}]");
                    return default;
                }
            }
            set
            {
                if (xIndex >= 0 && xIndex < Width && yIndex >= 0 && yIndex < Height)
                {
                    mGrid[xIndex, yIndex] = value;
                }
                else
                {
                    LogKit.W($"out of bounds [{xIndex}:{yIndex}] in grid[{Width}:{Height}]");
                }
            }
        }

        public void Clear(Action<T> cleanItem)
        {
            for (var x = 0; x < Width; x++)
            {
                for (var y = 0; y < Height; y++)
                {
                    cleanItem?.Invoke(mGrid[x, y]);
                    mGrid[x, y] = default;
                }
            }

            mGrid = null;
        }

        public void Fill(T t)
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    mGrid[x, y] = t;
                }
            }
        }

        public void Resize(int width, int height, Func<int, int, T> onAddCallBack)
        {
            var newGrid = new T[width, height];

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    newGrid[x, y] = mGrid[x, y];
                }

                for (int y = Height; y < height; y++)
                {
                    newGrid[x, y] = onAddCallBack.Invoke(x,y);
                }
            }

            for (int x = Width; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    newGrid[x, y] = onAddCallBack.Invoke(x, y);
                }
            }

            Fill(default(T));

            Width = width;
            Height = height;
            mGrid = newGrid;
        }

        public void Fill(Func<int, int, T> onFill)
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    mGrid[x, y] = onFill.Invoke(x, y);
                }
            }
        }

        public void ForEach(Action<int, int, T> each)
        {
            for(int x = 0;x < Width;x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    each?.Invoke(x, y, mGrid[x,y]);
                }
            }
        }

        public void ForEach(Action<T> each)
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    each?.Invoke(mGrid[x, y]);
                }
            }
        }
    }
}
