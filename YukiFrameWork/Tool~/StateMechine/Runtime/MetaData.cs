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
using Object = UnityEngine.Object;
namespace YukiFrameWork.States
{
    public enum DataType
    {
        Object = 0,
        Int16,
        Int32,
        Int64,
        UInt16,
        UInt32,
        UInt64,
        Single,
        Double,         
        Boolan,
        String,
        Enum      
    }
    [Serializable]
    public class MetaData
    {
        public string name;

        public string typeName;

        public DataType dataType;

        public object data;

        //可视化的数据保存
        public string value;

        public object Data
        {
            get
            {              
                value = data?.ToString();               
                return GetData();
            }
            set
            {
                WriteData(value);
            }
        }

        public Object Value;    

        public MetaData(string name, string typeName,DataType type,object data)
        {
            this.name = name;
            this.typeName = typeName;
            this.dataType = type;
            this.Data = data;
        }
             
        public MetaData() { }

        public object GetData()
        {
            return dataType switch
            {
                DataType.Object => Value,
                DataType.Int16 => Convert.ToInt16(data),
                DataType.Int32 => Convert.ToInt32(data),
                DataType.Int64 => Convert.ToInt64(data),
                DataType.UInt16 => Convert.ToUInt16(data),
                DataType.UInt32 => Convert.ToUInt32(data),
                DataType.UInt64 => Convert.ToUInt64(data),
                DataType.Single => Convert.ToSingle(data),
                DataType.Double => Convert.ToDouble(data),
                DataType.Boolan => Convert.ToBoolean(data),
                DataType.String => Convert.ToString(data),
                DataType.Enum => (Enum)Convert.ChangeType(data, typeof(Enum)),
                _ => null,
            };
        }

        public void WriteData(object value)
        {
            switch (dataType)
            {
                case DataType.Object:
                    Value = value as Object;
                    break;
                case DataType.Int16:
                    data = (short)value;
                    break;
                case DataType.Int32:
                    data = (int)value;
                    break;
                case DataType.Int64:
                    data = (long)value;
                    break;
                case DataType.UInt16:
                    data = (ushort)value;
                    break;
                case DataType.UInt32:
                    data = (uint)value;
                    break;
                case DataType.UInt64:
                    data = (ulong)value;
                    break;
                case DataType.Single:
                    data = (float)value;
                    break;
                case DataType.Double:
                    data = (double)value;
                    break;              
                case DataType.Boolan:
                    data = (bool)value;
                    break;
                case DataType.String:
                    data = (string)value;
                    break;
                case DataType.Enum:
                    data = (Enum)value;
                    break;
            }
        }
    }

    /// <summary>
    /// 序列化状态脚本，标记后可视化状态脚本所有公开的字段(数组,列表除外)，这个特性在类上标记
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