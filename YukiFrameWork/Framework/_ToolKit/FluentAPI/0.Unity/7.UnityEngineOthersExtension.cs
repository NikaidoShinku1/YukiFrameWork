/****************************************************************************
 * Copyright (c) 2016 - 2023 liangxiegame UNDER MIT License
 * 
 * http://qframework.cn
 * https://github.com/liangxiegame/QFramework
 * https://gitee.com/liangxiegame/QFramework
 ****************************************************************************/

using System.Collections.Generic;
using UnityEngine;

namespace YukiFrameWork.QF
{
#if UNITY_EDITOR888
    [ClassAPI("00.FluentAPI.Unity", "UnityEngine.Others", 8)]
    [APIDescriptionCN("其他的一些静态扩展")]
    [APIDescriptionEN("other extension")]
#endif
    public static class UnityEngineOthersExtension
    {
#if UNITY_EDITOR888
        // v1 No.155
        [MethodAPI]
        [APIDescriptionCN("随机 List 中的一个元素")]
        [APIDescriptionEN("get random item in a list")]
        [APIExampleCode(@"
new List<int>(){ 1,2,3 }.GetRandomItem();
")]
#endif
        public static T GetRandomItem<T>(this List<T> list)
        {
            return list[UnityEngine.Random.Range(0, list.Count)];
        }
        
#if UNITY_EDITOR888
        // v1.0.34
        [MethodAPI]
        [APIDescriptionCN("随机获取并删除 List 中的一个元素")]
        [APIDescriptionEN("get and remove random item in a list")]
        [APIExampleCode(@"
new List<int>(){ 1,2,3 }.GetAndRemoveRandomItem();
")]
#endif
        public static T GetAndRemoveRandomItem<T>(this List<T> list)
        {
            var randomIndex = UnityEngine.Random.Range(0, list.Count);
            var randomItem = list[randomIndex];
            list.RemoveAt(randomIndex);
            return randomItem;
        }

#if UNITY_EDITOR888
        // v1
        [MethodAPI]
        [APIDescriptionCN("为 SpriteRender 设置 alpha 值")]
        [APIDescriptionEN("set SpriteRender's alpha value")]
        [APIExampleCode(@"
mySprRender.Alpha(0.5f);
")]
#endif
        public static SpriteRenderer Alpha(this SpriteRenderer self, float alpha)
        {
            var color = self.color;
            color.a = alpha;
            self.color = color;
            return self;
        }

#if UNITY_EDITOR888
        // Added in v1.0.31
        [MethodAPI]
        [APIDescriptionCN("Mathf.Lerp")]
        [APIDescriptionEN("Mathf.Lerp")]
        [APIExampleCode(@"
var v = 0.5f.Lerp(0.1f,0.5f);
// v is 0.3f
")]
#endif
        public static float Lerp(this float self, float a, float b)
        {
            return Mathf.Lerp(a, b, self);
        }

#if UNITY_EDITOR888
        // Added in v1.0.31
        [MethodAPI]
        [APIDescriptionCN("Mathf.Abs")]
        [APIDescriptionEN("Mathf.Abs")]
        [APIExampleCode(@"
var absValue = -1.0f.Abs();
// absValue is 1.0f
")]
#endif
        public static float Abs(this float self)
        {
            return Mathf.Abs(self);
        }
        
        public static float Abs(this int self)
        {
            return Mathf.Abs(self);
        }

#if UNITY_EDITOR888
        // Added in v1.0.31
        [MethodAPI]
        [APIDescriptionCN("Mathf.Sign")]
        [APIDescriptionEN("Mathf.Sign")]
        [APIExampleCode(@"
var sign = -5.0f.Sign();
// sign is 5.0f
")]
#endif
        public static float Sign(this float self)
        {
            return Mathf.Sign(self);
        }
        
        public static float Sign(this int self)
        {
            return Mathf.Sign(self);
        }
        
#if UNITY_EDITOR888
        // Added in v1.0.32
        [MethodAPI]
        [APIDescriptionCN("Mathf.Cos")]
        [APIDescriptionEN("Mathf.Cos")]
        [APIExampleCode(@"
var cos = (90.0f * Mathf.Deg2Rad).Cos();
// cos is 0f
")]
#endif
        public static float Cos(this float self)
        {
            return Mathf.Cos(self);
        }
        
        public static float Cos(this int self)
        {
            return Mathf.Cos(self);
        }
        
#if UNITY_EDITOR888
        // Added in v1.0.32
        [MethodAPI]
        [APIDescriptionCN("Mathf.Sin")]
        [APIDescriptionEN("Mathf.Sin")]
        [APIExampleCode(@"
var sin = (90.0f * Mathf.Deg2Rad).Sin();
// sin is 1f
")]
#endif
        public static float Sin(this float self)
        {
            return Mathf.Sin(self);
        }
        
        public static float Sin(this int self)
        {
            return Mathf.Sin(self);
        }

#if UNITY_EDITOR888
        // Added in v1.0.32
        [MethodAPI]
        [APIDescriptionCN("Mathf.Cos(x * Mathf.Deg2Rad)")]
        [APIDescriptionEN("Mathf.Cos(x * Mathf.Deg2Rad)")]
        [APIExampleCode(@"
var cos = 90.0f.CosAngle();
// cos is 0f
")]
#endif
        public static float CosAngle(this float self)
        {
            return Mathf.Cos(self * Mathf.Deg2Rad);
        }
        
        public static float CosAngle(this int self)
        {
            return Mathf.Cos(self * Mathf.Deg2Rad);
        }

#if UNITY_EDITOR888
        // Added in v1.0.32
        [MethodAPI]
        [APIDescriptionCN("Mathf.Sin(x * Mathf.Deg2Rad)")]
        [APIDescriptionEN("Mathf.Sin(x * Mathf.Deg2Rad)")]
        [APIExampleCode(@"
var sin = 90.0f.SinAngle();
// sin is 1f
")]
#endif
        public static float SinAngle(this float self)
        {
            return Mathf.Sin(self * Mathf.Deg2Rad);
        }
        
        public static float SinAngle(this int self)
        {
            return Mathf.Sin(self * Mathf.Deg2Rad);
        }

#if UNITY_EDITOR888
        // Added in v1.0.32
        [MethodAPI]
        [APIDescriptionCN("Mathf.Deg2Rad")]
        [APIDescriptionEN("Mathf.Deg2Rad")]
        [APIExampleCode(@"
var radius = 90.0f.Deg2Rad();
// radius is 1.57f
")]
#endif
        public static float Deg2Rad(this float self)
        {
            return self * Mathf.Deg2Rad;
        }

        public static float Deg2Rad(this int self)
        {
            return self * Mathf.Deg2Rad;
        }
        
        
#if UNITY_EDITOR888
        // Added in v1.0.32
        [MethodAPI]
        [APIDescriptionCN("Mathf.Rad2Deg")]
        [APIDescriptionEN("Mathf.Rad2Deg")]
        [APIExampleCode(@"
var degree = 1.57f.Rad2Deg();
// degree is 90f
")]
#endif
        public static float Rad2Deg(this float self)
        {
            return self * Mathf.Rad2Deg;
        }
        
        public static float Rad2Deg(this int self)
        {
            return self * Mathf.Rad2Deg;
        }
        
#if UNITY_EDITOR888
        // Added in v1.0.129
        [MethodAPI]
        [APIDescriptionCN("将欧拉角转换为方向向量(Vector2)")]
        [APIDescriptionEN("Convert Degree To Direction(Vector2)")]
        [APIExampleCode(@"
var direction = 90.AngleToDirection2D();
// Vector2(1,0)
")]
#endif
                
        public static Vector2 AngleToDirection2D(this int self)
        {
            return new Vector2(self.CosAngle(), self.SinAngle());
        }
        
        public static Vector2 AngleToDirection2D(this float self)
        {
            return new Vector2(self.CosAngle(), self.SinAngle());
        }
        
#if UNITY_EDITOR888
        // Added in v1.0.129
        [MethodAPI]
        [APIDescriptionCN("将方向(Vector2)转换为欧拉角")]
        [APIDescriptionEN("Convert Direction To Degrees")]
        [APIExampleCode(@"
var direction = Vector2.right.ToAngle();
// Vector2(1,0)
")]
#endif
        public static float ToAngle(this Vector2 self)
        {
            return Mathf.Atan2(self.y, self.x).Rad2Deg();
        }
    }

#if UNITY_EDITOR888
    [ClassAPI("00.FluentAPI.Unity", "UnityEngine.Random", 7)]
    [APIDescriptionCN("针对随机做的一些封装")]
    [APIDescriptionEN("wrapper for random")]
#endif
    public static class RandomUtility
    {
#if UNITY_EDITOR888
        // v1
        [MethodAPI]
        [APIDescriptionCN("随机选择")]
        [APIDescriptionEN("RandomChoose")]
        [APIExampleCode(@"
var result = RandomUtility.Choose(1,1,1,2,2,2,2,3,3);

if (result == 3)
{
    // todo ...
}
")]
#endif
        public static T Choose<T>(params T[] args)
        {
            return args[UnityEngine.Random.Range(0, args.Length)];
        }
    }
}