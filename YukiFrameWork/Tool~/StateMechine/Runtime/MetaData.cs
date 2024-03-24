///=====================================================
/// - FileName:      MetaData.cs
/// - NameSpace:     YukiFrameWork.States
/// - Created:       Yuki
/// - Email:         Yuki@qq.com
/// - Description:   状态机数据本地类
/// - Creation Time: 2023年12月17日 15:36:28
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using YukiFrameWork.Extension;
using Object = UnityEngine.Object;
namespace YukiFrameWork.States
{
    public enum TypeCode
    {
        Empty = 0,
        Object = 1,
        Boolean = 3,
        Char = 4,
        SByte = 5,
        Byte = 6,
        Int16 = 7,
        UInt16 = 8,
        Int32 = 9,
        UInt32 = 10,
        Int64 = 11,
        UInt64 = 12,
        Single = 13,
        Double = 14,
        Decimal = 0xF,
        DateTime = 0x10,
        String = 18,
        Vector2,
        Vector3,
        Vector4,
        Rect,
        Color,
        Color32,
        Quaternion,
        AnimationCurve,
        GenericType,
        Array,
        Enum,
    }
    [Serializable]
    public class Metadata
    {
        public string name;
        public TypeCode type;
        public string typeName;
        public string data;
        public object target;
        public FieldInfo field;
        public Object Value;
        public List<Object> values;
        public List<Object> Values
        {
            get
            {
                values ??= new List<Object>();
                return values;
            }
            set { values = value; }
        }
        private object _value;
        public object value
        {
            get
            {
                if (target != null & field != null)
                    _value = field.GetValue(target);
                if (_value == null)
                    _value = Read();
                return _value;
            }
            set
            {
                _value = value;
                if (target != null & field != null)
                    field.SetValue(target, _value);
                Write(_value);
            }
        }
        private Type _type;
        public Type Type
        {
            get
            {
                if (_type == null)
                    _type = AssemblyHelper.GetType(typeName,Assembly.Load("YukiFrameWork"));
                return _type;
            }
        }
        public Type _itemType;
        public Type itemType
        {
            get
            {
                if (_itemType == null)
                    if (!GenericTypeArguments.TryGetValue(Type, out _itemType))
                    {                     
                        GenericTypeArguments.Add(Type, Type.GetInterface(typeof(IList<>).FullName).GetGenericArguments()[0]);
                       
                    }
                return _itemType;
            }
        }
        public int arraySize;
        public bool foldout;

        static readonly Dictionary<Type, Type> GenericTypeArguments = new Dictionary<Type, Type>();

        public Metadata() { }
        public Metadata(string name, string fullName, TypeCode type, object target, FieldInfo field)
        {
            this.name = name;
            typeName = fullName;           
            this.type = type;
            this.field = field;
            this.target = target;
            Write(value);
        }

        public object Read()
        {
            switch (type)
            {
                case TypeCode.Byte:
                    return Convert.ToByte(data);
                case TypeCode.SByte:
                    return Convert.ToSByte(data);
                case TypeCode.Boolean:
                    return Convert.ToBoolean(data);
                case TypeCode.Int16:
                    return Convert.ToInt16(data);
                case TypeCode.UInt16:
                    return Convert.ToUInt16(data);
                case TypeCode.Char:
                    return Convert.ToChar(data);
                case TypeCode.Int32:
                    return Convert.ToInt32(data);
                case TypeCode.UInt32:
                    return Convert.ToUInt32(data);
                case TypeCode.Single:
                    return Convert.ToSingle(data);
                case TypeCode.Int64:
                    return Convert.ToInt64(data);
                case TypeCode.UInt64:
                    return Convert.ToUInt64(data);
                case TypeCode.Double:
                    return Convert.ToDouble(data);
                case TypeCode.String:
                    return data;
                case TypeCode.Enum:
                    return Enum.Parse(Type, data);
                case TypeCode.Object:
                    if (Value == null)
                        return null;
                    return Value;
                case TypeCode.Vector2:
                    var datas = data.Split(',');
                    return new Vector2(float.Parse(datas[0]), float.Parse(datas[1]));
                case TypeCode.Vector3:
                    var datas1 = data.Split(',');
                    return new Vector3(float.Parse(datas1[0]), float.Parse(datas1[1]), float.Parse(datas1[2]));
                case TypeCode.Vector4:
                    var datas2 = data.Split(',');
                    return new Vector4(float.Parse(datas2[0]), float.Parse(datas2[1]), float.Parse(datas2[2]), float.Parse(datas2[2]));
                case TypeCode.Quaternion:
                    var datas3 = data.Split(',');
                    return new Quaternion(float.Parse(datas3[0]), float.Parse(datas3[1]), float.Parse(datas3[2]), float.Parse(datas3[2]));
                case TypeCode.Rect:
                    var datas4 = data.Split(',');
                    return new Rect(float.Parse(datas4[0]), float.Parse(datas4[1]), float.Parse(datas4[2]), float.Parse(datas4[2]));
                case TypeCode.Color:
                    var datas5 = data.Split(',');
                    return new Color(float.Parse(datas5[0]), float.Parse(datas5[1]), float.Parse(datas5[2]), float.Parse(datas5[2]));
                case TypeCode.Color32:
                    var datas6 = data.Split(',');
                    return new Color32(byte.Parse(datas6[0]), byte.Parse(datas6[1]), byte.Parse(datas6[2]), byte.Parse(datas6[2]));
                case TypeCode.GenericType:
                    if (itemType == typeof(Object) | itemType.IsSubclassOf(typeof(Object)))
                    {
                        IList list = (IList)Activator.CreateInstance(Type);
                        for (int i = 0; i < Values.Count; i++)
                        {
                            if (Values[i] == null)
                                list.Add(null);
                            else
                                list.Add(Values[i]);
                        }
                        return list;
                    }
                    else return AssemblyHelper.DeserializedObject(data, Type);
                case TypeCode.Array:
                    if (itemType == typeof(Object) | itemType.IsSubclassOf(typeof(Object)))
                    {
                        IList list = Array.CreateInstance(itemType, Values.Count);
                        for (int i = 0; i < Values.Count; i++)
                        {
                            if (Values[i] == null) continue;
                            list[i] = Values[i];
                        }
                        return list;
                    }
                    else return AssemblyHelper.DeserializedObject(data, Type);
            }
            return null;
        }

        public void Write(object value)
        {
            if (type == TypeCode.Object)
            {
                Value = (Object)value;
            }
            else if (value != null)
            {
                if (type == TypeCode.Vector2)
                {
                    Vector2 v2 = (Vector2)value;
                    data = $"{v2.x},{v2.y}";
                }
                else if (type == TypeCode.Vector3)
                {
                    Vector3 v = (Vector3)value;
                    data = $"{v.x},{v.y},{v.z}";
                }
                else if (type == TypeCode.Vector4)
                {
                    Vector4 v = (Vector4)value;
                    data = $"{v.x},{v.y},{v.z},{v.w}";
                }
                else if (type == TypeCode.Quaternion)
                {
                    Quaternion v = (Quaternion)value;
                    data = $"{v.x},{v.y},{v.z},{v.w}";
                }
                else if (type == TypeCode.Rect)
                {
                    Rect v = (Rect)value;
                    data = $"{v.x},{v.y},{v.width},{v.height}";
                }
                else if (type == TypeCode.Color)
                {
                    Color v = (Color)value;
                    data = $"{v.r},{v.g},{v.b},{v.a}";
                }
                else if (type == TypeCode.Color32)
                {
                    Color32 v = (Color32)value;
                    data = $"{v.r},{v.g},{v.b},{v.a}";
                }
                else if (type == TypeCode.GenericType | type == TypeCode.Array)
                {
                    if (itemType == typeof(Object) | itemType.IsSubclassOf(typeof(Object)))
                    {
                        Values.Clear();
                        IList list = (IList)value;
                        for (int i = 0; i < list.Count; i++)
                            Values.Add(list[i] as Object);
                    }
                    else data = AssemblyHelper.SerializedObject(value,Newtonsoft.Json.Formatting.None,new Newtonsoft.Json.JsonSerializerSettings()
                    {
                        ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore,
                        PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.Objects
                    });
                }
                else data = value.ToString();
            }
            else
            {
                data = null;
            }
        }
    }

    /// <summary>
    /// 序列化状态脚本，标记后可视化状态脚本公开的字段，这个特性在类上标记
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class SerializedStateAttribute : Attribute
    {
        
    }

    /// <summary>
    /// 状态脚本不显示的字段
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class HideFieldAttribute : Attribute
    {
        
    }
}