﻿using System;
using UnityEngine;

namespace YukiFrameWork
{
    public partial class StGrid
    {
        public void DrawGrid() => GLKit.DrawGrid(xMin, yMin, row, colm, cw, ch, 0f);
        public void DrawGridGizmos() => GLKit.DrawGridGizmos(xMin, yMin, row, colm, cw, ch, 0f);
        public bool Contains(Vector2 p) => Contains(p.x, p.y);
        public Vector2 Origin
        {
            get => new Vector2(xMin, yMin);
            set
            {
                xMin = value.x;
                yMin = value.y;
            }
        }
        public Vector2 CellSize
        {
            get => new Vector2(cw, ch);
            set
            {
                cw = value.x;
                ch = value.y;
            }
        }
        public Vector2 BoundSize
        {
            get => new Vector2(W, H);
            set
            {
                if (colm == 0 || row == 0) return;
                cw = value.x / colm;
                ch = value.y / row;
            }
        }
        public Vector2Int RowColm
        {
            get => new Vector2Int(row, colm);
            set
            {
                row = value.x;
                colm = value.y;
            }
        }
        public StGrid(Vector2 center, float width, float height, int row, int colm, bool cellMode = false)
        {
            this.colm = colm;
            this.row = row;
            if (cellMode)
            {
                cw = width;
                ch = height;
                xMin = center.x - width * colm * 0.5f;
                yMin = center.y - height * row * 0.5f;
            }
            else
            {
                cw = width / colm;
                ch = height / row;
                xMin = center.x - width * 0.5f;
                yMin = center.y - height * 0.5f;
            }

        }
        public StGrid(Vector2 center, float totalSize, int num) :
            this(center, totalSize, totalSize, num, num, false)
        { }
        public Vector2 RandomPos()
        {
            CellIndexToWorldCoord(SysRandom.Range(row), SysRandom.Range(colm), out float x, out float y);
            return new Vector2(x, y);
        }
        public Vector2 GetCorner(Dir4 dir)
        {
            return dir switch
            {
                Dir4.Left | Dir4.Down => new Vector2(xMin, yMin),
                Dir4.Left | Dir4.Up => new Vector2(xMin, yMax),
                Dir4.Right | Dir4.Down => new Vector2(xMax, yMin),
                Dir4.Right | Dir4.Up => new Vector2(xMax, yMax),
                _ => Vector2.zero
            };
        }
        public void DrawValuesByLeft_RowMajor<T>(T[] values)
        {
            int len = values.Length;
            for (int i = 0; i < len; i++)
            {
                LinearIndexToCellIndex_RowMajor(i, out int r, out int c);
                CellIndexToCoordCenter(row - r - 1, c, out float x, out float y);
                Vector2 p = Camera.main.WorldToScreenPoint(new Vector2(x, y));
                GUI.Label(new Rect(p, p), values[i].ToString());
            }
        }
        public void CoordToCellIndex(Vector2 p, out int r, out int c) => CoordToCellIndex(p.x, p.y, out r, out c);
        public bool ContainsByLeftUp(Vector2 p, int r, int c, float w, float h)
        {
            CellIndexToWorldCoord(r, c, out float minx, out float miny);
            float xmax = minx + cw;
            float ymax = miny + ch;
            return p.x >= minx && p.x < xmax && p.y >= miny && p.y < ymax &&
                p.x < minx + w && p.y >= ymax - h;
        }
        public void Rect2GridIndex(Rect rect, out int sr, out int sc, out int er, out int ec)
        {
            // 将 Rect 的坐标转换为网格索引
            CoordToCellIndex(rect.xMin, rect.yMin, out sr, out sc);
            CoordToCellIndex(rect.xMax, rect.yMax, out er, out ec);
            // 确保索引在有效范围内
            sr.Clamp(0, row - 1);
            er.Clamp(0, row - 1);
            sc.Clamp(0, colm - 1);
            ec.Clamp(0, colm - 1);
        }
        public void GetIndicesWithinRectByLeftUp(Rect rect, PArray<int> indices)
        {
            Rect2GridIndex(rect, out int sr, out int sc, out int er, out int ec);
            indices.ToFirst();
            // 遍历所有在 Rect 内的网格单元
            for (int index; er >= sr; er--)
            {
                for (int c = sc; c <= ec; c++)
                {
                    index = CellIndexToLinearIndex_RowMajor(er, c);
                    indices.Push(index);
                }
            }
        }
        public void DrawTile(Vector2 p, bool isWire = true)
        {
            CoordToCellIndex(p.x, p.y, out int r, out int c);
            CellIndexToWorldCoord(r, c, out float x, out float y);
            GLKit.DrawRectLeft(x, y, cw, ch, 0f, isWire);
        }
    }
}