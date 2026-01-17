///=====================================================
/// - FileName:      IMission.cs
/// - NameSpace:     YukiFrameWork.Missions
/// - Description:   高级定制脚本生成
/// - Creation Time: 1/12/2026 7:33:37 PM
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using System.Collections.Generic;
namespace YukiFrameWork.Missions
{
    [Serializable]
    public struct Position
    {
        public float x;
        public float y;
        public Position(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public static implicit operator Vector2(Position position)
            => new Vector2(position.x, position.y);

        public static implicit operator Position(Vector2 vector)
            => new Position(vector.x, vector.y);
    }
}
