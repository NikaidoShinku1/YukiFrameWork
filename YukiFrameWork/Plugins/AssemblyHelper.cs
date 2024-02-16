///=====================================================
/// - FileName:      AssemblyHelper.cs
/// - NameSpace:     YukiFrameWork.Project
/// - Created:       Yuki
/// - Email:         Yuki@qq.com
/// - Description:   这是一个框架工具创建的脚本
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================

using System;
using Newtonsoft.Json;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Linq;

namespace YukiFrameWork.Extension
{
    public class AssemblyHelper
    {
        private static Dictionary<string, Type> typeDict = new Dictionary<string, Type>();
    
        public static Type GetType(string typeName)
        {          
            if (typeDict.TryGetValue(typeName, out var type))
            {
                return type;
            }

            // 处理泛型类型          
            if (typeName.Contains('`'))
            {              
                int backtickIndex = typeName.IndexOf('`');
                int genericArgStartIndex = typeName.IndexOf("[", StringComparison.Ordinal);
                if (genericArgStartIndex > backtickIndex)
                {
                    string genericTypeName = typeName.Substring(0, genericArgStartIndex);
                    string args = typeName.Substring(genericArgStartIndex).Trim('[', ']');
                    var argumentTypes = args.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                             .Select(GetType)
                                             .ToArray();
                    if (argumentTypes.Any(t => t == null)) return null;                                      
                    else
                        type = Type.GetType(genericTypeName)?.MakeGenericType(argumentTypes);                                        
                    if (type != null)
                    {
                        typeDict.Add(typeName, type);
                        return type;
                    }
                }
            }

            // 处理数组类型           
            if (typeName.EndsWith("[]") || typeName.Contains("[,"))
            {
                var arrayTypeSpecifierIndex = typeName.IndexOf('[');
                string elementTypeString = typeName.Substring(0, arrayTypeSpecifierIndex);
                string arraySpecifier = typeName.Substring(arrayTypeSpecifierIndex);

                Type elementType = GetType(elementTypeString);
                if (elementType == null) return null;

                if (arraySpecifier.Contains(","))
                {
                    int rank = arraySpecifier.Count(c => c == ',') + 1; 
                    type = Array.CreateInstance(elementType, new int[rank]).GetType();
                }
                else 
                {
                    type = Array.CreateInstance(elementType, 0).GetType();
                }

                if (type != null)
                {
                    typeDict.Add(typeName, type);
                    return type;
                }
            }

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            for (int j = 0; j < assemblies.Length; j++)
            {
                Type[] types = assemblies[j].GetTypes();
                for (int i = 0; i < types.Length; i++)
                {
                    if (types[i].FullName == typeName)
                    {
                        typeDict.Add(typeName, types[i]);
                        return types[i];
                    }
                }
            }

            return null;
        }

        public static Type[] GetTypes(Type type)
        {
            return Assembly.GetAssembly(type).GetTypes();
        }       

        public static Type[] GetTypes(Assembly assembly)
        {           
            return assembly?.GetTypes();
        }

        public static T DeserializeObject<T>(string value)
            => JsonConvert.DeserializeObject<T>(value);


        public static object DeserializeObject(string value, Type type)
            => JsonConvert.DeserializeObject(value,type);

        public static string SerializedObject(object value, Newtonsoft.Json.Formatting formatting = Newtonsoft.Json.Formatting.Indented)
            => JsonConvert.SerializeObject(value, formatting);

        public static string XmlSerializedObject(object value,XmlWriterSettings settings = default)
        {            
            XmlSerializer xmlSerializer = new XmlSerializer(value.GetType());
            if(settings == default)
                settings = new XmlWriterSettings()
                {
                    Indent = true,
                    NewLineOnAttributes = true,

                };
            using (StringWriter writer = new StringWriter())
            {
                XmlWriter xmlWriter = XmlWriter.Create(writer, settings);
                xmlSerializer.Serialize(xmlWriter, value);

                return writer.ToString();
            }
        }

        public static T XmlDeserializeObject<T>(string value)
        {
            return (T)XmlDeserializeObject(value, typeof(T));
        }

        public static object XmlDeserializeObject(string value, Type type)
        {
            object obj = null;
            using (StringReader render = new StringReader(value))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(type);

                obj = xmlSerializer.Deserialize(render);
            }

            return obj;
        }

        public static byte[] ByteSerializeObject(object value)
        {
            BinaryFormatter formatter = new BinaryFormatter();

            using (MemoryStream stream = new MemoryStream())
            {
                formatter.Serialize(stream, value);

                return stream.ToArray();
            }           
        }       

        public static object ByteDeserializeObject(byte[] value)
        {
            MemoryStream stream = new MemoryStream(value);

            BinaryFormatter formatter = new BinaryFormatter();

            return formatter.Deserialize(stream);
        }
    }
}