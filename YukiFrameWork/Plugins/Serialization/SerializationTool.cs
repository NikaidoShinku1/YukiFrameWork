///=====================================================
/// - FileName:      SerializationTool.cs
/// - NameSpace:     YukiFrameWork.Example
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/4/11 18:39:15
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using System.Xml;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif
namespace YukiFrameWork.Extension
{
    /// <summary>
    /// 序列化工具类
    /// </summary>
	public static class SerializationTool
	{
        public static T DeserializedObject<T>(string value,JsonSerializerSettings settings = default)
            => JsonConvert.DeserializeObject<T>(value,settings);

        public static T DeserializedObject<T>(string value,params JsonConverter[] converters)
          => JsonConvert.DeserializeObject<T>(value,converters);

        public static object DeserializedObject(string value, Type type)
          => JsonConvert.DeserializeObject(value, type);

        public static string SerializedObject(object value, Newtonsoft.Json.Formatting formatting = Newtonsoft.Json.Formatting.Indented, JsonSerializerSettings settings = null)
        {
            if (settings == null)
                return JsonConvert.SerializeObject(value, formatting);
            else return JsonConvert.SerializeObject(value, formatting, settings);
        }

        public static string XmlSerializedObject(object value, XmlWriterSettings settings = default)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(value.GetType());
            if (settings == default)
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

        public static T XmlDeserializedObject<T>(string value)
        {
            return (T)XmlDeserializedObject(value, typeof(T));
        }

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

        public static byte[] ByteSerializedObject(object value)
        {
            return Encoding.UTF8.GetBytes(SerializedObject(value));
        }

        public static object ByteDeserializedObject(byte[] value, Type type)
        {
            return DeserializedObject(Encoding.UTF8.GetString(value), type);
        }

        public static T ByteDeserializedObject<T>(byte[] value)
            => (T)ByteDeserializedObject(value, typeof(T));


        /// <summary>
        /// Excel表转Json
        /// </summary>
        /// <param name="excelPath">Excel文件的路径</param>     
        /// <param name="header">表中几行表头</param>
        /// <param name="exclude_prefix">导出时，过滤掉包含指定前缀的列</param>
        /// <returns>返回Json的字符串</returns>
        public static string ExcelToJson(string excelPath, int header, string exclude_prefix = "",bool cellJson = false,bool allString = false)
        {
            if (!File.Exists(excelPath))
            {
                throw new Exception("指定Excel的路径不存在!excelPath:" + excelPath);
            }

            ExcelLoader loader = new ExcelLoader(excelPath, header);
            JsonExporter exporter = new JsonExporter(loader, false, true, string.Empty, false, header, exclude_prefix, cellJson, allString);
            return exporter.context;
        }


        /// <summary>
        /// 把指定路径下所有的Excel转换成Json
        /// </summary>
        /// <param name="folderPath">文件夹名称</param>
        /// <param name="suffix">Excel的后缀(匹配模式)"*.xlsx"||"*.xls"</param>
        /// <param name="header"></param>
        /// <param name="exclude_prefix"></param>
        /// <returns>返回一个保存了所有被转换成Json字符串的字符串数组</returns>
        public static List<string> AllExcelToJson(string folderPath,string suffix,int header, string exclude_prefix = "")
        {
            if (!Directory.Exists(folderPath))
            {
                throw new Exception("指定的文件夹路径不存在请检查!folderPath:" + folderPath);
            }

            string[] files = Directory.GetFiles(folderPath,suffix,SearchOption.AllDirectories);
            List<string> jsons = new List<string>();
            for (int i = 0; i < files.Length; i++)
            {
#if UNITY_EDITOR
                EditorUtility.DisplayProgressBar("导出Json", $"Excel:{files[i]}",(float)i/files.Length);
#endif
                try
                {
                    if (Path.GetFileName(files[i]).StartsWith("~$"))
                    {
                        continue;

                    }

                    jsons.Add(ExcelToJson(files[i], header, exclude_prefix));
                    UnityEngine.Debug.Log("导出Excel:" + files[i]);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Excel:{files[i]}export error:{e.ToString()}");
                }
            }
#if UNITY_EDITOR
            EditorUtility.ClearProgressBar();
            Debug.Log("导出完成");
#endif

            return jsons;
        }

#if UNITY_EDITOR
        /// <summary>
        /// 可以快捷创建该字符串的文件流
        /// </summary>
        /// <param name="node">字符串本体</param>
        /// <param name="filePath">创建路径</param>
        /// <param name="fileName">创建的文件名称</param>
        /// <param name="suffix">创建的文件后缀</param>
        /// <returns></returns>
        public static bool CreateFileStream(this string node,string filePath,string fileName,string suffix)
        {
            return CreateFileStream(new StringBuilder(node), filePath, fileName, suffix);
        }

        public static bool CreateFileStream(this StringBuilder node, string filePath, string fileName, string suffix)
        {
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
                AssetDatabase.Refresh();
            }
            if (!suffix.StartsWith("."))
            {
                UnityEngine.Debug.LogError("请检查后缀是否填写正确，是否带有标点:Suffix:" + suffix);
                return false;
            }

            if (!filePath.EndsWith("/") || !filePath.EndsWith(@"\"))
            {
                filePath += "/";
            }
            string targetPath = filePath + fileName + suffix;
            FileMode mode = File.Exists(targetPath) ? FileMode.Open : FileMode.Create;
            if (mode == FileMode.Open)
                File.WriteAllText(targetPath, string.Empty);

            using (FileStream stream = new FileStream(targetPath, mode, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                StreamWriter streamWriter = new StreamWriter(stream);
                streamWriter.Write(node);
                streamWriter.Close();
                stream.Close();
            }
            AssetDatabase.Refresh();
            return true;
        }
#endif
    }
}
