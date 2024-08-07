﻿///=====================================================
/// - FileName:      BezierManager.cs
/// - NameSpace:     YukiFrameWork
/// - Created:       Yuki
/// - Email:         Yuki@qq.com
/// - Description:   贝塞尔曲线计算管理器
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================


using System.Collections.Generic;
using UnityEngine;
using YukiFrameWork.Extension;
using System;
namespace YukiFrameWork
{
    /// <summary>
    /// 贝塞尔曲线标准公式：B(t) = P0 + (P1-P0)t
    /// </summary>
    [ClassAPI("贝塞尔曲线管理器")]
    [GUIDancePath("YukiFrameWork/Bezier")]
    public class BezierUtility
    {
        #region 贝塞尔曲线 一阶二阶三阶
        /// <summary>
        /// 一阶贝塞尔曲线 公式：B(t) = (1-t)P0 +tP1 Mathf.Clamp(t,0,1)
        /// </summary>
        /// <param name="p0">P0坐标点</param>
        /// <param name="p1">P1坐标点</param>
        /// <param name="t">0返回Start，1返回End</param>
        /// <returns>根据t值返回当前路径</returns>
        public static Vector3 BezierIntepolate(Vector3 p0, Vector3 p1, float t)
        {
            //Vector3 p0p1 = (1 - t) * p0 + t * p1;
            Vector3 p0p1 = p0 + (p1 - p0) * t;
            return p0p1;
        }

        /// <summary>
        /// 二阶贝塞尔曲线 公式：B(t) = (1-t)(1-t)p0 + 2t(1-t)P1 + t*t*P2 Mathf.Clamp(t,0,1)
        /// </summary>
        /// <param name="p0">P0坐标点</param>
        /// <param name="p1">P1坐标点</param>
        /// <param name="p2">P2坐标点</param>
        /// <param name="t">0返回Start，1返回End</param>
        /// <returns>根据t值返回当前路径</returns>
        public static Vector3 BezierIntepolate(Vector3 p0, Vector3 p1, Vector3 p2, float t)
        {
            //二阶完整公式运用
            //Vector3 p0p1p2 = (1 - t) * (1 - t) * p0 + 2 * t * (1 - t) * p1 + t * t * p2; 

            //标准公式嵌套
            Vector3 p0p1 = p0 + (p1 - p0) * t;
            Vector3 p1p2 = p1 + (p2 - p1) * t;

            Vector3 p0p1p2 = p0p1 + (p1p2 - p0p1) * t;
            return p0p1p2;
        }

        /// <summary>
        /// 三阶贝塞尔曲线 公式： B(t) = (1-t)(1-t)(1-t)P0 + 3P1 * t *(1-t)*(1-t) + 3P2 * t * t * (1-t)+P3* t * t * t Mathf.Clamp(t,0,1)
        /// </summary>
        /// <param name="p0">P0坐标点</param>
        /// <param name="p1">P1坐标点</param>
        /// <param name="p2">P2坐标点</param>
        /// <param name="p3">P3坐标点</param>
        /// <param name="t">0返回Start，1返回End</param>
        /// <returns>根据t值返回当前路径</returns>
        public static Vector3 BezierIntepolate(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            //Vector3 p0p1p2p3 = (1 - t) * (1 - t) * (1 - t) * p0 + p1 * 3 * t * (1 - t) * (1 - t) + p2 * 3 * t * t * (1 - t) + p3 * t * t * t;
            Vector3 p0p1 = p0 + (p1 - p0) * t;
            Vector3 p1p2 = p1 + (p2 - p1) * t;
            Vector3 p2p3 = p2 + (p3 - p2) * t;

            Vector3 p0p1p2 = p0p1 + (p1p2 - p0p1) * t;
            Vector3 p1p2p3 = p1p2 + (p2p3 - p1p2) * t;

            Vector3 p0p1p2p3 = p0p1p2 + (p1p2p3 - p0p1p2) * t;
            return p0p1p2p3;
        }
        #endregion      

        #region 插值版贝塞尔曲线 一阶二阶三阶
        /// <summary>
        /// 插值版一阶贝塞尔曲线
        /// </summary>
        /// <param name="p0">P0坐标点</param>
        /// <param name="p1">P1坐标点</param>
        /// <param name="t">0返回Start，1返回End</param>
        /// <returns>根据t值返回当前路径</returns>
        public static Vector3 BezierLerp(Vector3 p0, Vector3 p1, float t)
        {
            return Vector3.Lerp(p0, p1, t);
        }

        /// <summary>
        /// 插值版二阶贝塞尔曲线
        /// </summary>
        /// <param name="p0">P0坐标点</param>
        /// <param name="p1">P1坐标点</param>
        /// <param name="p2">P2坐标点</param>
        /// <param name="t">0返回Start，1返回End</param>
        /// <returns>根据t值返回当前路径</returns>
        public static Vector3 BezierLerp(Vector3 p0, Vector3 p1, Vector3 p2, float t)
        {
            Vector3 p0p1 = Vector3.Lerp(p0, p1, t);
            Vector3 p1p2 = Vector3.Lerp(p1, p2, t);
            Vector3 p0p1p2 = Vector3.Lerp(p0p1, p1p2, t);
            return p0p1p2;
        }

        /// <summary>
        /// 插值版三阶贝塞尔曲线
        /// </summary>
        /// <param name="p0">P0坐标点</param>
        /// <param name="p1">P1坐标点</param>
        /// <param name="p2">P2坐标点</param>
        /// <param name="p3">P3坐标点</param>
        /// <param name="t">0返回Start，1返回End</param>
        /// <returns>根据t值返回当前路径</returns>
        public static Vector3 BezierLerp(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            Vector3 p0p1 = Vector3.Lerp(p0, p1, t);
            Vector3 p1p2 = Vector3.Lerp(p1, p2, t);
            Vector3 p2p3 = Vector3.Lerp(p2, p3, t);

            Vector3 p0p1p2 = Vector3.Lerp(p0p1, p1p2, t);
            Vector3 p1p2p3 = Vector3.Lerp(p1p2, p2p3, t);
            return Vector3.Lerp(p0p1p2, p1p2p3, t);
        }
        #endregion

        #region 通过贝塞尔曲线的坐标数量获得沿途所有的路径点
        public static List<Vector3> GetBezierList(Vector3 p0, Vector3 p1, float count)
        {
            List<Vector3> result = new List<Vector3>();
            for (int i = 0; i < count; i++)
            {
                result.Add(BezierIntepolate(p0, p1, i / (count - 1)));
            }
            return result;
        }

        public static List<Vector3> GetBezierList(Vector3 p0, Vector3 p1, Vector3 p2, float count)
        {
            List<Vector3> result = new List<Vector3>();
            for (int i = 0; i < count; i++)
            {
                result.Add(BezierIntepolate(p0, p1, p2, i / (count - 1)));
            }
            return result;
        }

        public static List<Vector3> GetBezierList(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float count)
        {
            List<Vector3> result = new List<Vector3>();
            for (int i = 0; i < count; i++)
            {
                result.Add(BezierIntepolate(p0, p1, p2, p3, i / (count - 1)));
            }
            return result;
        }
        #endregion
    }
}
