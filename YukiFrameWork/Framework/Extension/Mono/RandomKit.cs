///=====================================================
/// - FileName:      RandomKit.cs
/// - NameSpace:     YukiFrameWork.Random
/// - Description:   随机生成套件
/// - Creation Time: 2024/9/8 8:44:46
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using URandom = UnityEngine.Random;
using System.Collections.Generic;
using System.Linq;
namespace YukiFrameWork
{   
	public static class RandomKit
	{
		public static float Range(float min, float max)
		{
			return URandom.Range(min, max);
		}

        public static int Range(int min, int max)
        {
            return URandom.Range(min, max);
        }

		public static Vector3 RangeVector3(Vector3 min, Vector3 max)
		{
			float x = URandom.Range(min.x, max.x);
			float y = URandom.Range(min.y, max.y);
			float z = URandom.Range(min.z, max.z);
			return new Vector3(x, y, z);
		}
      
        public static Vector2 RangeVector2(Vector2 min, Vector2 max)
        {
			return RangeVector3(min, max);
        }

		public static Vector3 RangeX(this Vector3 current,float min,float max)
		{
			return new Vector3(Range(min, max), current.y, current.z);
		}

        public static Vector3 RangeY(this Vector3 current, float min, float max)
        {
            return new Vector3(current.x, Range(min, max), current.z);
        }

        public static Vector3 RangeZ(this Vector3 current, float min, float max)
        {
            return new Vector3(current.x, current.y, Range(min, max));
        }

        public static Vector2 RangeX(this Vector2 current, float min, float max)
        {
            return new Vector3(Range(min, max), current.y);
        }

        public static Vector2 RangeY(this Vector2 current, float min, float max)
        {
            return new Vector3(current.x, Range(min, max));
        }

        public static Quaternion EulerRangeX(this Quaternion quaternion, float min, float max)
        {
            return Quaternion.Euler(Range(min,max),quaternion.y,quaternion.z);
        }

        public static Quaternion EulerRangeY(this Quaternion quaternion, float min, float max)
        {
            return Quaternion.Euler(quaternion.x, Range(min, max), quaternion.z);
        }

        public static Quaternion EulerRangeZ(this Quaternion quaternion, float min, float max)
        {
            return Quaternion.Euler(quaternion.x, quaternion.y, Range(min, max));
        }

        public static Quaternion EulerRange(this Quaternion quaternion, float min, float max)
        {
            return Quaternion.Euler(Range(min, max), Range(min, max), Range(min, max));
        }

        public static IEnumerable<T> RangeEnumerable<T>(int min,int max) where T : new()
        {
            return new T[Range(min, max)]; 
        }

        public static float Range01
        {
            get => URandom.value;
        }

        public static Vector2 InsideUnitCircle
        {
            get => URandom.insideUnitCircle;
        }

        public static Vector2 InsideUnitSphere
        {
            get => URandom.insideUnitSphere;
        }

        public static Quaternion Rotation
        {
            get => URandom.rotation;
        }

        public static Quaternion RotationUniform
        {
            get => URandom.rotationUniform;
        }

        public static URandom.State State
        {
            get => URandom.state;
        }

        public static IEnumerable<T> RandomForEach<T>(this IList<T> enumerable, Action<T> each, int count)
        {
            return enumerable.RandomEnumerable(count).ForEach(each);
        }

        /// <summary>
        /// 随机列表，会获得随机之后的列表。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public static IEnumerable<T> RandomEnumerable<T>(this IEnumerable<T> enumerable, int count)
        {
            if (count > enumerable.Count())
                throw new IndexOutOfRangeException("传递的数量超出总数量");
            List<T> copy = new List<T>(enumerable);
            for (int i = 0; i < count; i++)
            {
                int randomIndex = RandomKit.Range(i, copy.Count);
                (copy[i], copy[randomIndex]) = (copy[randomIndex], copy[i]); // 交换
            }
            // 返回前 count 个元素
            return copy.Take(count);
        }
        //
        // 摘要:
        //     Generates a random color from HSV and alpha ranges.
        //
        // 参数:
        //   hueMin:
        //     Minimum hue [0..1].
        //
        //   hueMax:
        //     Maximum hue [0..1].
        //
        //   saturationMin:
        //     Minimum saturation [0..1].
        //
        //   saturationMax:
        //     Maximum saturation [0..1].
        //
        //   valueMin:
        //     Minimum value [0..1].
        //
        //   valueMax:
        //     Maximum value [0..1].
        //
        //   alphaMin:
        //     Minimum alpha [0..1].
        //
        //   alphaMax:
        //     Maximum alpha [0..1].
        //
        // 返回结果:
        //     A random color with HSV and alpha values in the (inclusive) input ranges. Values
        //     for each component are derived via linear interpolation of value.
        public static Color ColorHSV()
        {
            return URandom.ColorHSV();
        }

        public static Color ColorHSV(float hueMin, float hueMax, float saturationMin, float saturationMax, float valueMin, float valueMax)
        {
            return ColorHSV(hueMin, hueMax, saturationMin, saturationMax, valueMin, valueMax, 1f, 1f);
        }

        public static Color ColorHSV(float hueMin, float hueMax, float saturationMin, float saturationMax)
        {
            return ColorHSV(hueMin, hueMax, saturationMin, saturationMax, 0f, 1f, 1f, 1f);
        }  

        public static Color ColorHSV(float hueMin, float hueMax)
        {
            return ColorHSV(hueMin, hueMax, 0f, 1f, 0f, 1f, 1f, 1f);
        }  

        public static Color ColorHSV(float hueMin, float hueMax, float saturationMin, float saturationMax, float valueMin, float valueMax, float alphaMin, float alphaMax)
        {
            return URandom.ColorHSV(hueMin, hueMax, saturationMin, saturationMax, valueMin, valueMax, alphaMin, alphaMax);
        }

        /// <summary>
        /// 随机获取集合的其中元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <returns></returns>
        [Obsolete("该方法已过时，请使用RandomKit.Random方法随机取值")]
        public static T RangeGetter<T>(this IEnumerable<T> t) 
        {
            int current = 0;
            int target = Range(0, t.Count());

            foreach (var item in t)
            {
                if (current == target)
                    return item;
                current++;
            }

            return default;
        }       

        public static T Random<T>(this IEnumerable<T> t)
        {
            T[] targets = t.ToArray();
            if (targets == null || targets.Length == 0)
                throw new NullReferenceException("传递的集合内容是空的，无法进行随机");
            return targets[Range(0, targets.Length)];
        }

        public static T RandomChoose<T>(params T[] param)
        {
            return param[Range(0, param.Length)];
        }
        
        /// <summary>
        /// 随机概率获取
        /// </summary>
        public static float Probability
        {
            get => Range(1f, 101f);
        }

        /// <summary>
        /// 是否概率成功
        /// </summary>
        /// <param name="current"></param>
        /// <returns></returns>
        public static bool IsProbabilitySuccess(float current)
        {            
            return Probability <= current;
        }

        /// <summary>
        /// 生成指定位数的随机码（数字）
        /// </summary>
        /// <param name="length">生成数字的长度</param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static string RandomCode(int length, int min, int max)
        {           
            var result = new System.Text.StringBuilder();
            for (var i = 0; i < length; i++)
            {
                var random = new System.Random(System.Guid.NewGuid().GetHashCode());
                result.Append(random.Next(min, max));
            }
            return result.ToString();
        }

    }
}
