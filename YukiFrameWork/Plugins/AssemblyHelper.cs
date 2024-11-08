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
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Linq;
using System.Text;

namespace YukiFrameWork.Extension
{
    public class AssemblyHelper 
    {
        private static Dictionary<string, Type> typeDict = new Dictionary<string, Type>();
    
        public static Type GetType(string typeName,Assembly assembly = default)
        {          
            if(string.IsNullOrEmpty(typeName))return null;
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
                                             .Select(t => GetType(t,assembly))
                                             .ToArray();
                    if (argumentTypes.Any(t => t == null)) return null;
                    else
                    {
                        if (assembly != default)
                            type = assembly.GetType(genericTypeName)?.MakeGenericType(argumentTypes);
                         type ??= Type.GetType(genericTypeName)?.MakeGenericType(argumentTypes);                       
                    }
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

            Type AllLoad(Type[] types)
            {
                for (int i = 0; i < types.Length; i++)
                {
                    if (types[i].ToString() == typeName)
                    {
                        typeDict.Add(typeName, types[i]);
                        return types[i];
                    }
                }
                return null;
            }

            if (assembly != default)
            {
                return AllLoad(assembly.GetTypes());
            }

            try
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();

                for (int j = 0; j < assemblies.Length; j++)
                {
                    Type[] types = assemblies[j].GetTypes();
                    Type current = AllLoad(types);
                    if (current != null)
                        return current;
                }
            }
            catch 
            {
                
            }

            return null;
        }

        public static Type[] GetTypes(Type type)
        {
            return Assembly.GetAssembly(type).GetTypes();
        }

        public static Type[] GetTypes(Func<Type,bool> condition)
        {
            return AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()).Where(condition).ToArray();
        }

        public static Type[] GetTypes(Assembly assembly)
        {           
            return assembly?.GetTypes();
        }
        #region Obsolete Serialization Function
        [Obsolete("The recommended DeserializedObject is the deserializeDobject of the SerializationTool class")]
        public static T DeserializedObject<T>(string value)
            => SerializationTool.DeserializedObject<T>(value);

        [Obsolete("The recommended DeserializedObject is the deserializeDobject of the SerializationTool class")]
        public static object DeserializedObject(string value, Type type)
            => JsonConvert.DeserializeObject(value,type);

        [Obsolete("The recommended SerializedObject is the serializeDobject of the SerializationTool class")]
        public static string SerializedObject(object value, Newtonsoft.Json.Formatting formatting = Newtonsoft.Json.Formatting.Indented, JsonSerializerSettings settings = null)
        {
            if (settings == null)
                return JsonConvert.SerializeObject(value, formatting);
            else return JsonConvert.SerializeObject(value, formatting, settings);
        }
        [Obsolete("The recommended XmlSerializedObject is the xmlserializeDobject of the SerializationTool class")]
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
        [Obsolete("The recommended XmlDeserializedObject is the xmldeserializeDobject of the SerializationTool class")]
        public static T XmlDeserializedObject<T>(string value)
        {
            return (T)XmlDeserializedObject(value, typeof(T));
        }
        [Obsolete("The recommended XmlDeserializedObject is the xmldeserializeDobject of the SerializationTool class")]
        public static object XmlDeserializedObject(string value, Type type)
        {
            object obj = null;
            using (StringReader render = new StringReader(value))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(type);

                obj = xmlSerializer.Deserialize(render);
            }

            return obj;
        }
        [Obsolete("The recommended ByteDeserializedObject is the bytedeserializeDobject of the SerializationTool class")]
        public static byte[] ByteSerializedObject(object value)
        {
            return Encoding.UTF8.GetBytes(SerializedObject(value));           
        }
        [Obsolete("The recommended BytesDeserializedObject is the bytesdeserializeDobject of the SerializationTool class")]
        public static object ByteDeserializedObject(byte[] value,Type type)
        {
            return DeserializedObject(Encoding.UTF8.GetString(value), type);          
        }
        [Obsolete("The recommended BytesDeserializedObject is the bytesdeserializeDobject of the SerializationTool class")]
        public static T ByteDeserializedObject<T>(byte[] value)
            => (T)ByteDeserializedObject(value, typeof(T));
        #endregion
    }
}