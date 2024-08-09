using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
#endif
public static class ArrayExtend
{
    #region 数组For
    public static void For<T>(this T[] self, Action<T> action)
    {
        for (int i = 0; i < self.Length; i++)
        {
            action(self[i]);
        }
    }

    public static void For<T>(this T[] self, Action<int> action)
    {
        for (int i = 0; i < self.Length; i++)
        {
            action(i);
        }
    }

    public static void For<T>(this T[] self, int index, int count, Action<T> action)
    {
        for (int i = index; i < count; i++)
        {
            action(self[i]);
        }
    }

    public static void For<T>(this T[] self, int index, Action<T> action)
    {
        for (int i = index; i < self.Length; i++)
        {
            action(self[i]);
        }
    }

    public static void For<T>(this T[] self, Action<int, T> action)
    {
        for (int i = 0; i < self.Length; i++)
        {
            action(i, self[i]);
        }
    }
    #endregion

    /// <summary>
    /// 随机一个值,在数组0-count范围内
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="self"></param>
    /// <returns></returns>
    public static T Random<T>(this T[] self)
    {
        return self[UnityEngine.Random.Range(0, self.Length)];
    }

#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL
    public static void ClearObjects<T>(this T[] self) where T : Object
    {
        for (int i = 0; i < self.Length; i++)
        {
            if (self[i] != null)
                Object.Destroy(self[i]);
        }
    }

    public static void ClearObjects<Key, Value>(this Dictionary<Key, Value> self) where Value : Object
    {
        foreach (var item in self)
        {
            if (item.Value != null)
                Object.Destroy(item.Value);
        }
        self.Clear();
    }

    public static void SetActives<T>(this T[] self, bool active) where T : Object
    {
        for (int i = 0; i < self.Length; i++)
        {
            if (self[i] is GameObject go)
                go.SetActive(active);
            else if (self[i] is MonoBehaviour mb)
                mb.gameObject.SetActive(active);
        }
    }

    public static void SetActives<T>(this List<T> self, bool active) where T : Object
    {
        for (int i = 0; i < self.Count; i++)
        {
            if (self[i] is GameObject go)
                go.SetActive(active);
            else if (self[i] is MonoBehaviour mb)
                mb.gameObject.SetActive(active);
        }
    }

    public static void SetActives<T>(this T[] self, bool active, int start, int end) where T : Object
    {
        for (int i = start; i < end; i++)
        {
            if (self[i] is GameObject go)
                go.SetActive(active);
            else if (self[i] is MonoBehaviour mb)
                mb.gameObject.SetActive(active);
        }
    }

    public static void SetActives<T>(this List<T> self, bool active, int start, int end) where T : Object
    {
        for (int i = start; i < end; i++)
        {
            if (self[i] is GameObject go)
                go.SetActive(active);
            else if (self[i] is MonoBehaviour mb)
                mb.gameObject.SetActive(active);
        }
    }

    /// <summary>
    /// 循环全部并且设置, 如果指定的start和end内的物体则active, 否则!active
    /// </summary>
    public static void SetActiveAll<T>(this T[] self, bool active, int start, int end) where T : Object
    {
        for (int i = 0; i < self.Length; i++)
        {
            if (self[i] is GameObject go)
                go.SetActive(i < start | i >= end ? !active : active);
            else if (self[i] is MonoBehaviour mb)
                mb.gameObject.SetActive(i < start | i >= end ? !active : active);
        }
    }

    /// <summary>
    /// 循环全部并且设置, 如果指定的start和end内的物体则active, 负责!active
    /// </summary>
    public static void SetActiveAll<T>(this List<T> self, bool active, int start, int end) where T : Object
    {
        for (int i = 0; i < self.Count; i++)
        {
            if (self[i] is GameObject go)
                go.SetActive(i < start | i >= end ? !active : active);
            else if (self[i] is MonoBehaviour mb)
                mb.gameObject.SetActive(i < start | i >= end ? !active : active);
        }
    }

    /// <summary>
    /// 循环全部并且设置, 如果指定的start和end内的物体则active, 负责!active
    /// </summary>
    public static void SetActiveAll<T>(this T[] self, bool active, int index) where T : Object
    {
        for (int i = 0; i < self.Length; i++)
        {
            if (self[i] is GameObject go)
                go.SetActive(i != index ? !active : active);
            else if (self[i] is MonoBehaviour mb)
                mb.gameObject.SetActive(i != index ? !active : active);
        }
    }

    /// <summary>
    /// 循环全部并且设置, 如果指定的start和end内的物体则active, 负责!active
    /// </summary>
    public static void SetActiveAll<T>(this List<T> self, bool active, int index) where T : Object
    {
        for (int i = 0; i < self.Count; i++)
        {
            if (self[i] is GameObject go)
                go.SetActive(i != index ? !active : active);
            else if (self[i] is MonoBehaviour mb)
                mb.gameObject.SetActive(i != index ? !active : active);
        }
    }

    public static void SetField<T>(this T[] self, string name, object value) where T : Object
    {
        var type = typeof(T);
        var property = type.GetField(name);
        for (int i = 0; i < self.Length; i++)
        {
            property.SetValue(self[i], value);
        }
    }

    public static void SetField<T>(this List<T> self, string name, object value) where T : Object
    {
        var type = typeof(T);
        var property = type.GetField(name);
        for (int i = 0; i < self.Count; i++)
        {
            property.SetValue(self[i], value);
        }
    }

    public static void SetProperty<T>(this T[] self, string name, object value) where T : Object
    {
        var type = typeof(T);
        var property = type.GetProperty(name);
        for (int i = 0; i < self.Length; i++)
        {
            property.SetValue(self[i], value);
        }
    }

    public static void SetProperty<T>(this List<T> self, string name, object value) where T : Object
    {
        var type = typeof(T);
        var property = type.GetProperty(name);
        for (int i = 0; i < self.Count; i++)
        {
            property.SetValue(self[i], value);
        }
    }

    public static void InvokeMethod<T>(this T[] self, string name, params object[] pars) where T : Object
    {
        var type = typeof(T);
        var method = type.GetMethod(name);
        for (int i = 0; i < self.Length; i++)
        {
            method.Invoke(self[i], pars);
        }
    }

    public static void InvokeMethod<T>(this List<T> self, string name, params object[] pars) where T : Object
    {
        var type = typeof(T);
        var method = type.GetMethod(name);
        for (int i = 0; i < self.Count; i++)
        {
            method.Invoke(self[i], pars);
        }
    }

    public static void SetEnableds<T>(this T[] self, bool active) where T : Behaviour
    {
        for (int i = 0; i < self.Length; i++)
        {
            if (self[i] is MonoBehaviour mb)
                mb.enabled = active;
        }
    }

    public static void SetEnableds<T>(this List<T> self, bool active) where T : Behaviour
    {
        for (int i = 0; i < self.Count; i++)
        {
            if (self[i] is MonoBehaviour mb)
                mb.enabled = active;
        }
    }

    /// <summary>
    /// 循环全部并且设置, 如果指定的start和end内的物体则active, 负责!active
    /// </summary>
    public static void SetEnabledsAll<T>(this T[] self, bool active, int start, int end) where T : Behaviour
    {
        for (int i = 0; i < self.Length; i++)
        {
            if (self[i] is MonoBehaviour mb)
                mb.enabled = i < start | i >= end ? !active : active;
        }
    }

    /// <summary>
    /// 循环全部并且设置, 如果指定的start和end内的物体则active, 负责!active
    /// </summary>
    public static void SetEnabledsAll<T>(this List<T> self, bool active, int start, int end) where T : Behaviour
    {
        for (int i = 0; i < self.Count; i++)
        {
            if (self[i] is MonoBehaviour mb)
                mb.enabled = i < start | i >= end ? !active : active;
        }
    }

    /// <summary>
    /// 循环全部并且设置, 如果指定的start和end内的物体则active, 负责!active
    /// </summary>
    public static void SetEnabledsAll<T>(this T[] self, bool active, int index) where T : Behaviour
    {
        for (int i = 0; i < self.Length; i++)
        {
            if (self[i] is MonoBehaviour mb)
                mb.enabled = i != index ? !active : active;
        }
    }

    /// <summary>
    /// 循环全部并且设置, 如果指定的start和end内的物体则active, 负责!active
    /// </summary>
    public static void SetEnabledsAll<T>(this List<T> self, bool active, int index) where T : Behaviour
    {
        for (int i = 0; i < self.Count; i++)
        {
            if (self[i] is MonoBehaviour mb)
                mb.enabled = i != index ? !active : active;
        }
    }

    public static void SetSprites(this Image[] self, Sprite sprite)
    {
        for (int i = 0; i < self.Length; i++)
        {
            self[i].sprite = sprite;
        }
    }

    public static void SetSprites(this List<Image> self, Sprite sprite)
    {
        for (int i = 0; i < self.Count; i++)
        {
            self[i].sprite = sprite;
        }
    }

    public static void ClearObjects<T>(this List<T> self) where T : Component
    {
        for (int i = 0; i < self.Count; i++)
        {
            if (self[i] != null)
                Object.Destroy(self[i].gameObject);
        }
        self.Clear();
    }
    public static void ClearObjects(this List<GameObject> self)
    {
        for (int i = 0; i < self.Count; i++)
        {
            if (self[i] != null)
                Object.Destroy(self[i]);
        }
        self.Clear();
    }
    public static void ClearChildObjects(this GameObject self)
    {
        ClearChildObjects(self.transform);
    }
    public static void ClearChildObjects(this Transform self)
    {
        for (int i = 0; i < self.childCount; i++)
        {
            Object.Destroy(self.GetChild(i).gameObject);
        }
    }
    public static void SortTransforms<T>(this List<T> self, int offset = 0) where T : Component
    {
        for (int i = 0; i < self.Count; i++)
        {
            self[i].transform.SetSiblingIndex(i + offset);
        }
    }

    public static void SetLayer(this GameObject self, int layer)
    {
        var trans = self.GetComponentInChildren<Transform>();
        foreach (Transform tran in trans)
        {
            tran.gameObject.layer = layer;
        }
    }

    public static void SetIsOn(this List<Toggle> self, int isOnIndex)
    {
        for (int i = 0; i < self.Count; i++)
        {
            var isOn = i == isOnIndex;
            self[i].isOn = isOn;
            self[i].onValueChanged.Invoke(isOn);
        }
    }

    public static void SetIsOn(this Toggle[] self, int isOnIndex)
    {
        for (int i = 0; i < self.Length; i++)
        {
            var isOn = i == isOnIndex;
            self[i].isOn = isOn;
            self[i].onValueChanged.Invoke(isOn);
        }
    }
#endif

    #region HashSet For
    public static void For<T>(this HashSet<T> self, Action<T> action)
    {
        foreach (T t in self)
        {
            action(t);
        }
    }
    #endregion

    public static T[] ToArray<T>(this HashSet<T> self)
    {
        var array = new T[self.Count];
        self.CopyTo(array);
        return array;
    }

    /*public static byte[] ToArray(this byte[] self, int index, int count)
    {
        var buffer = new byte[count];
        Unsafe.CopyBlockUnaligned(ref buffer[0], ref self[index], (uint)count);
        return buffer;
    }*/

    public static T Find<T>(this List<T> self, Func<T, bool> func)
    {
        for (int i = 0; i < self.Count; i++)
        {
            if (func(self[i]))
            {
                return self[i];
            }
        }
        return default;
    }

    public static bool Find<T>(this List<T> self, Func<T, bool> func, out T item)
    {
        for (int i = 0; i < self.Count; i++)
        {
            if (func(self[i]))
            {
                item = self[i];
                return true;
            }
        }
        item = default;
        return false;
    }

    public static T Find<T>(this T[] self, Func<T, bool> func)
    {
        for (int i = 0; i < self.Length; i++)
        {
            if (func(self[i]))
            {
                return self[i];
            }
        }
        return default;
    }

    public static bool Find<T>(this T[] self, Func<T, bool> func, out T item)
    {
        for (int i = 0; i < self.Length; i++)
        {
            if (func(self[i]))
            {
                item = self[i];
                return true;
            }
        }
        item = default;
        return false;
    }

    public static void Add<T>(ref T[] self, T item)
    {
        var size = self.Length;
        var array = new T[size + 1];
        Array.Copy(self, 0, array, 0, size);
        array[size] = item;
        self = array;
    }

    public static void Remove<T>(ref T[] self, T item)
    {
        var index = Array.IndexOf(self, item);
        if (index == -1)
            return;
        RemoveAt(ref self, index);
    }

    public static void RemoveAt<T>(ref T[] self, int index)
    {
        var size = self.Length - 1;
        Array.Copy(self, index + 1, self, index, size - index);
        var array = new T[size];
        Array.Copy(self, 0, array, 0, size);
        self = array;
    }

    public static T[] Add<T>(T[] self, T item)
    {
        var size = self.Length;
        var array = new T[size + 1];
        Array.Copy(self, 0, array, 0, size);
        array[size] = item;
        return array;
    }

    public static T[] Remove<T>(T[] self, T item)
    {
        var index = Array.IndexOf(self, item);
        if (index == -1)
            return self;
        return RemoveAt(self, index);
    }

    public static T[] RemoveAt<T>(T[] self, int index)
    {
        var size = self.Length - 1;
        Array.Copy(self, index + 1, self, index, size - index);
        var array = new T[size];
        Array.Copy(self, 0, array, 0, size);
        return array;
    }
}