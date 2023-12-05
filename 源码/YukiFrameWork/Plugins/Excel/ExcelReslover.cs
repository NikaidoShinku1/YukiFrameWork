using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System;

namespace YukiFrameWork.Excel
{
    /// <summary>
    /// 重写Excel解析器
    /// </summary>
    public class ExcelReslover : JsonConverter
    {
        private Type doubleObjectType = typeof(double);
        private Type decimalObjectType = typeof(decimal);
        private void ConvertNumToArray<T>(JsonWriter writer,T t)
        {
            var s = t.ToString();

            if (s.EndsWith(".0"))
                writer.WriteRawValue(s.Substring(0, s.LastIndexOf(".")));
            else writer.WriteRawValue(s);
        }
        public override bool CanConvert(Type objectType)
        {
            return  objectType == typeof(double) || objectType == typeof(decimal);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            Type type = value.GetType();

            if (type == doubleObjectType)
            {
                ConvertNumToArray(writer, (double)value);
            }
            else if(type == decimalObjectType)
            {
                ConvertNumToArray(writer, (decimal)value);
            }
                       
        }
    }
}
