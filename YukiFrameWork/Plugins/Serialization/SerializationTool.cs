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
using OfficeOpenXml;
using System.Reflection;
using System.Collections;
using Newtonsoft.Json.Linq;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace YukiFrameWork.Extension
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ExcelIgnoreAttribute : Attribute
    {
        
    }
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

        #region Excel


        /// <summary>
        /// 支持的字段类型
        /// </summary>
        public enum ExcelFieldsType
        {
            None,
            Int,
            Float,
            Double,
            String,
            Bool,
            Long,
            Json,
            ObjectReference,
            Enum
        }


        public class ExcelFields
        {
            [Tooltip("名称")]
            public string name;
            [Tooltip("类型")]
            public ExcelFieldsType type;
            [Tooltip("备注")]
            public string notes;
        }
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
        /// 创建 Excel(仅能在编辑器模式调用,若在运行代码中调用,请添加UNITY_EDITOR宏!)
        /// </summary>
        /// <param name="path"></param>
        public static void CreateExcel(string path)
        {
            // 如果文件已经存在 直接return 即可
            if (File.Exists(path)) return;

            FileInfo fileInfo = new FileInfo(path);
            using ExcelPackage package = new ExcelPackage(fileInfo);
            package.Workbook.Worksheets.Add("Sheet1");
            package.Save();
        }
#if UNITY_EDITOR
        /// <summary>
        /// 将Excel文件的数据转换为可继承自IExcelSyncScriptableObject的ScriptableObject配置中
        /// </summary>
        /// <param name="excel_path"></param>
        /// <param name="excelSyncScriptableObject"></param>
        /// <returns></returns>
        public static bool ExcelToScriptableObject(string excel_path,int header, IExcelSyncScriptableObject excelSyncScriptableObject,string soDataPath = "")
        {
            try
            {
                string json = ExcelToJson(excel_path, header);

                Type type = excelSyncScriptableObject.ImportType;               
                Type generaticType = typeof(List<>).MakeGenericType(type);
                IList values = null;
                JObject[] jObjs = SerializationTool.DeserializedObject<JObject[]>(json);
                if (!typeof(ScriptableObject).IsAssignableFrom(type))
                {
                    values = Activator.CreateInstance(generaticType) as IList;
                    
                    for (int i = 0; i < jObjs.Length; i++)
                    {
                        var value = jObjs[i];                      
                        object data = null;
                       
                        if (IsValueType(type))
                        {
                            data = value["Element"].ToObject(type);
                        }
                        else
                        {
                            data = value.ToObject(type);
                            foreach (var item in value)
                            {
                                PropertyInfo propertyInfo = type.GetProperty(item.Key, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                                if (propertyInfo == null)
                                {
                                    FieldInfo fieldInfo = type.GetField(item.Key, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                                    if (fieldInfo == null) continue;
                                    if (fieldInfo.GetCustomAttribute<ExcelIgnoreAttribute>() != null) continue;
                                    if (fieldInfo.FieldType.IsSubclassOf(typeof(UnityEngine.Object)))
                                    {
#if UNITY_EDITOR
                                        fieldInfo.SetValue(data, UnityEditor.AssetDatabase.LoadAssetAtPath(UnityEditor.AssetDatabase.GUIDToAssetPath(item.Value.ToString()), fieldInfo.FieldType));
#endif
                                    }
                                    else
                                        fieldInfo.SetValue(data, item.Value.ToObject(fieldInfo.FieldType));

                                }
                                else
                                {
                                    if (propertyInfo.GetCustomAttribute<ExcelIgnoreAttribute>() != null) continue;
                                    if (propertyInfo.PropertyType.IsSubclassOf(typeof(UnityEngine.Object)))
                                    {
#if UNITY_EDITOR
                                        propertyInfo.SetValue(data, UnityEditor.AssetDatabase.LoadAssetAtPath(UnityEditor.AssetDatabase.GUIDToAssetPath(item.Value.ToString()), propertyInfo.PropertyType));
#endif
                                    }
                                    else
                                        propertyInfo.SetValue(data, item.Value.ToObject(propertyInfo.PropertyType));
                                }

                            }
                        }
                        values.Add(data);
                    }
                              
                }
                else
                {                                 
                    values = Activator.CreateInstance(generaticType) as IList;
                    ScriptableObject target = excelSyncScriptableObject as ScriptableObject;
                    string path = soDataPath + "/" + target.name;
                    if (Directory.Exists(path))
                        Directory.Delete(path, true);
                    for (int i = 0; i < jObjs.Length; i++)
                    {
                        var value = jObjs[i];
                        Type valueType;
                        try
                        {
                            valueType = AssemblyHelper.GetType(value[DATA_GENERATOR_TYPE].ToString());
                            if (valueType == null)
                                valueType = type;
                        }
                        catch { valueType = type; }
                        ScriptableObject scriptable = ScriptableObject.CreateInstance(valueType);
                        SerializedObject serializedObject = new SerializedObject(scriptable);
                        foreach (var property in GetAllSerializedProperty(serializedObject))
                        {
                            PropertyInfo propertyInfo = type.GetProperty(property.name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                            if (propertyInfo == null)
                            {
                                FieldInfo fieldInfo = type.GetField(property.name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                                if (fieldInfo == null)
                                {
                                    continue;
                                }
                                else
                                {
                                    if (fieldInfo.GetCustomAttribute<ExcelIgnoreAttribute>() != null) continue;
                                    if (value.TryGetValue(fieldInfo.Name, out var token))
                                    {
                                        if (fieldInfo.FieldType.IsSubclassOf(typeof(UnityEngine.Object)))
                                        {
#if UNITY_EDITOR
                                            fieldInfo.SetValue(scriptable, UnityEditor.AssetDatabase.LoadAssetAtPath(UnityEditor.AssetDatabase.GUIDToAssetPath(token.ToString()), fieldInfo.FieldType));
#endif
                                        }
                                        else
                                            fieldInfo.SetValue(scriptable, token.ToObject(fieldInfo.FieldType));
                                    }
                                }
                            }
                            else
                            {
                                if (propertyInfo.GetCustomAttribute<ExcelIgnoreAttribute>() != null) continue;
                                if (value.TryGetValue(propertyInfo.Name, out var token))
                                {
                                    if (propertyInfo.PropertyType.IsSubclassOf(typeof(UnityEngine.Object)))
                                    {
#if UNITY_EDITOR
                                        propertyInfo.SetValue(scriptable, UnityEditor.AssetDatabase.LoadAssetAtPath(UnityEditor.AssetDatabase.GUIDToAssetPath(token.ToString()), propertyInfo.PropertyType));
#endif
                                    }
                                    else
                                        propertyInfo.SetValue(scriptable, token.ToObject(propertyInfo.PropertyType));
                                }
                            }

                        }
                        if (excelSyncScriptableObject.ScriptableObjectConfigImport)
                        {
                            if (!Directory.Exists(soDataPath))
                            {
                                Directory.CreateDirectory(soDataPath);
                            }
                            int index = i;
#if UNITY_EDITOR
                            if (!Directory.Exists(path))
                                Directory.CreateDirectory(path);
                            AssetDatabase.CreateAsset(scriptable, $"{path}/{index}.asset");
#endif
                        }
                        values.Add(scriptable);

                    }


                }

                excelSyncScriptableObject.Create(values.Count);
                for (int i = 0; i < values.Count; i++)
                {
                    excelSyncScriptableObject.Import(i, values[i]);
                }

                excelSyncScriptableObject.Completed();
#if UNITY_EDITOR
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
#endif
                return true;

            }
            catch(Exception ex) { Debug.Log(ex.ToString()); return false; }
            
        }

        /// <summary>
        /// 把ScriptableObject集合转成Excel(仅能在编辑器模式调用,若在运行代码中调用,请添加UNITY_EDITOR宏!)
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="scriptables">ScriptableObject集合</param>
        /// <param name="excel_path">excel路径</param>
        /// <param name="error">报错信息</param>
        /// <returns></returns>
        public static bool ScriptableObjectToExcel(IExcelSyncScriptableObject scriptables, string excel_path, out string error)
        {
            if (string.IsNullOrEmpty(excel_path))
            {
                error = "excel路径不能为空!";
                Debug.LogError(error);
                return false;
            }

            if (!excel_path.EndsWith(".xlsx"))
            {
                error = string.Format("{0} 不是有效的excel文件路径,必须以.xlsx结尾!", excel_path);
                Debug.LogError(error);
                return false;
            }


            FileInfo fileInfo = new FileInfo(excel_path);
            using ExcelPackage package = new ExcelPackage(fileInfo);
            if (package.Workbook.Worksheets.Count == 0)
                package.Workbook.Worksheets.Add("Sheet1");

            ExcelWorksheet sheet = package.Workbook.Worksheets[1];

            // 删除excel中的原有数据
            int row = GetExcelRow(sheet);
            int col = GetExcelCol(sheet);

            for (int i = 1; i <= row; i++)
            {
                for (int j = 1; j <= col; j++)
                {
                    sheet.SetValue(i, j, null);
                }
            }

            // 往excel中写数据
            if (scriptables != null)
            {

                // 解析字段
                Dictionary<int, ExcelFields> fields = ParseFields(scriptables.Array);

                if (fields.Count != 0)
                {

                    // 往表中写入头
                    foreach (int column in fields.Keys)
                    {
                        sheet.SetValue(1, column, fields[column].name);
                        sheet.SetValue(2, column, fields[column].type.ToString().ToLower());
                        sheet.SetValue(3, column, fields[column].notes);

                        int index = 4;

                        ExcelFields excelField = fields[column];

                        if (column == 1)
                            sheet.Column(column).Width = 30;

                        foreach (object scriptable in scriptables.Array)
                        {
                            Type type = scriptable.GetType();

                            if (excelField.name == DATA_GENERATOR_TYPE)
                            {
                                sheet.SetValue(index,column,type.FullName);
                            }
                            else
                            {
                                if (IsValueType(type))
                                {
                                    sheet.SetValue(index, column, scriptable);
                                }
                                else
                                {
                                    FieldInfo field = GetFields(type).FirstOrDefault(x => x.Name == excelField.name);
                                    if (field != null)
                                    {
                                        object value = ParseFiledData(field.GetValue(scriptable), excelField.type, false, field);
                                        sheet.SetValue(index, column, value);
                                    }
                                }
                            }
                            index++;
                        }
                    }
                }
            }

            try
            {
                package.Save();
            }
            catch (InvalidOperationException)
            {
                error = string.Format("excel文件:{0}正在被别的应用程序使用,请关闭后重试!", excel_path);
                Debug.LogError(error);
                return false;

            }

            error = string.Empty;
            return true;
        }
        private const string DATA_GENERATOR_TYPE = "DATA_GENERATOR_TYPE";

        private static bool IsValueType(Type type)
        {
            return type == typeof(int)
                           || type == typeof(float)
                           || type == typeof(string)
                           || type == typeof(long)
                           || type == typeof(uint)
                           || type == typeof(bool)
                           || type == typeof(double)
                           || type == typeof(short)
                           || type == typeof(ushort)
                           || type == typeof(ulong);
        }
        private static Dictionary<int, ExcelFields> ParseFields(IList scriptables)
        {
            // 行数满足 读取所有的字段  key:列 value:字段 
            Dictionary<int, ExcelFields> fields = new Dictionary<int, ExcelFields>();

            ExcelFields data_type = new ExcelFields();
            data_type.name = DATA_GENERATOR_TYPE;
            data_type.type = ExcelFieldsType.String;
            data_type.notes = "配置真实类型";

            fields.Add(1, data_type);

            Dictionary<string, ExcelFields> scriptable_object_fields = new Dictionary<string, ExcelFields>();

            if (scriptables != null)
            {
                foreach (object value in scriptables)
                {
                    if (value is ScriptableObject item)
                    {
                        SerializedObject serializedObject = new SerializedObject(item);
                        List<SerializedProperty> properties = serializedObject.GetAllSerializedProperty();

                        Type type = value.GetType();
                        foreach (var fieldInfo in GetFields(type))
                        {
                           
                            if (scriptable_object_fields.ContainsKey(fieldInfo.Name)) continue;
                           
                            if (fieldInfo != null && fieldInfo.GetCustomAttribute<ExcelIgnoreAttribute>() != null) continue;                           
                            ExcelFields excelFields = new ExcelFields();
                            excelFields.name = fieldInfo.Name;
                            excelFields.type = ParseFieldType(fieldInfo);
                            TooltipAttribute tooltipAttribute = fieldInfo.GetCustomAttribute<TooltipAttribute>();
                            excelFields.notes = tooltipAttribute == null ? string.Empty : tooltipAttribute.tooltip;

                            scriptable_object_fields.Add(fieldInfo.Name, excelFields);                           
                        }
                       
                    }
                    else
                    {
                        Type type = value.GetType();

                        if (IsValueType(type))
                        {
                            if (scriptable_object_fields.ContainsKey("Element")) continue;
                            ExcelFields excelFields = new ExcelFields();
                            excelFields.name = "Element";
                            excelFields.notes = "基本数据结构统一名称";
                            excelFields.type = ParseFieldType(type);
                            scriptable_object_fields.Add(excelFields.name, excelFields);
                        }
                        else
                        {
                            foreach (var fieldInfo in GetFields(type))
                            {
                                if (scriptable_object_fields.ContainsKey(fieldInfo.Name)) continue;
                                if (fieldInfo != null && fieldInfo.GetCustomAttribute<ExcelIgnoreAttribute>() != null) continue;

                                ExcelFields excelFields = new ExcelFields();
                                excelFields.name = fieldInfo.Name;
                                excelFields.type = ParseFieldType(fieldInfo);
                                TooltipAttribute tooltipAttribute = fieldInfo.GetCustomAttribute<TooltipAttribute>();

                                excelFields.notes = tooltipAttribute == null ? string.Empty : tooltipAttribute.tooltip;

                                scriptable_object_fields.Add(fieldInfo.Name, excelFields);
                            }
                        }
                    }
                }
            }


         
            int index = fields.Count;

            foreach (var item in scriptable_object_fields.Values)
            {          
                index++;                
                fields.Add(index, item);
            }          
            return fields;
        }
        private static ExcelFieldsType PropertyTypeToExcelFieldsType(SerializedPropertyType propertyType, string type)
        {         
            switch (propertyType)
            {
                case SerializedPropertyType.Integer:
                    if (type == "ulong" || type == "long")
                        return ExcelFieldsType.Long;
                    return ExcelFieldsType.Int;
                case SerializedPropertyType.Boolean:
                    return ExcelFieldsType.Bool;
                case SerializedPropertyType.Float:
                    if (type == "double")
                        return ExcelFieldsType.Double;
                    return ExcelFieldsType.Float;
                case SerializedPropertyType.String:
                    return ExcelFieldsType.String;
                case SerializedPropertyType.ObjectReference:
                    return ExcelFieldsType.ObjectReference;

                case SerializedPropertyType.Enum:
                    return ExcelFieldsType.Enum;
            }

            return ExcelFieldsType.Json;
        }


        private static Dictionary<int, ExcelFields> ParseFields<T>()
        {
            Dictionary<int, ExcelFields> fields = new Dictionary<int, ExcelFields>();

            Type type = typeof(T);

            FieldInfo[] fieldInfos = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            int index = 1;

            foreach (var item in fieldInfos)
            {
                JsonIgnoreAttribute ignore = item.GetCustomAttribute<JsonIgnoreAttribute>();
                if (ignore != null) continue;

                if (item.IsNotSerialized) continue;

                if (item != null && item.GetCustomAttribute<ExcelIgnoreAttribute>() != null) continue;

                if (!item.IsPublic && item.GetCustomAttribute<SerializeField>() == null) continue;

                ExcelFields excelFields = new ExcelFields();
                excelFields.name = item.Name;
                excelFields.type = ParseFieldType(item);
                excelFields.notes = string.Empty;
                TooltipAttribute tooltip = item.GetCustomAttribute<TooltipAttribute>();
                if (tooltip != null)
                    excelFields.notes = tooltip.tooltip;

                fields.Add(index, excelFields);

                index++;
            }

            return fields;
        }

        private static ExcelFieldsType ParseFieldType(FieldInfo fieldInfo)
        {
          
            if (fieldInfo == null)
                return ExcelFieldsType.None;
            return ParseFieldType(fieldInfo.FieldType);
        }

        private static ExcelFieldsType ParseFieldType(Type type)
        {

            ExcelFieldsType t = ExcelFieldsType.None;
            if (type == typeof(int) || type == typeof(uint) || type == typeof(short) || type == typeof(ushort))
                t = ExcelFieldsType.Int;
            else if (type == typeof(float))
                t = ExcelFieldsType.Float;
            else if (type == typeof(double))
                t = ExcelFieldsType.Double;
            else if (type == typeof(string))
                t = ExcelFieldsType.String;
            else if (type == typeof(bool))
                t = ExcelFieldsType.Bool;
            else if (type == typeof(long))
                t = ExcelFieldsType.Long;
            else if (type.IsEnum)
                t = ExcelFieldsType.Enum;
            else if (type.IsSubclassOf(typeof(UnityEngine.Object)))
                t = ExcelFieldsType.ObjectReference;
            else
                t = ExcelFieldsType.Json;


            return t;

        }

        /// <summary>
        /// 解析或者处理数据 
        /// from_excel 为 true 时 obj 是从 excel中读到的内容 返回值是要写入ScriptableObject
        /// from_excel 为 false 时 obj 是 ScriptableObject的字段的值 返回值是要写入到Excel中的内容
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="type"></param>
        /// <param name="from_excel"></param>
        /// <returns></returns>
        private static object ParseFiledData(object obj, ExcelFieldsType type, bool from_excel = true, FieldInfo fieldInfo = null)
        {
            switch (type)
            {
                case ExcelFieldsType.Int:
                    int result_int = 0;
                    if (obj != null)
                        int.TryParse(obj.ToString(), out result_int);
                    return result_int;
                case ExcelFieldsType.Float:

                    float result_float = 0;
                    if (obj != null)
                        float.TryParse(obj.ToString(), out result_float);
                    return result_float;
                case ExcelFieldsType.Double:

                    double result_double = 0;

                    if (obj != null)
                        double.TryParse(obj.ToString(), out result_double);

                    return result_double;

                //string 一样处理 
                case ExcelFieldsType.String:
                    return obj == null ? string.Empty : obj.ToString();
                case ExcelFieldsType.Bool:
                    bool result_bool = false;
                    if (obj != null)
                        bool.TryParse(obj.ToString(), out result_bool);
                    return result_bool;
                case ExcelFieldsType.Long:
                    long result_long = 0;

                    if (obj != null)
                        long.TryParse(obj.ToString(), out result_long);

                    return result_long;

                case ExcelFieldsType.Json:
                    if (from_excel)
                        return obj != null ? obj.ToString() : string.Empty;

                    string json = string.Empty;

                    try
                    {
                        // json序列化可能会报错 所以来这里处理一下，如果出错就返回空
                        json = JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented);
                    }
                    catch (Exception e)
                    {
                        Debug.LogErrorFormat("Json序列化失败:{0}", e.ToString());
                    }

                    return json;

                case ExcelFieldsType.ObjectReference:

                    if (!from_excel)
                    {
                        UnityEngine.Object unityObj = obj as UnityEngine.Object;
                        string asset_path = AssetDatabase.GetAssetPath(unityObj);
                        return AssetDatabase.AssetPathToGUID(asset_path);
                    }

                    return obj != null ? obj.ToString() : string.Empty;

                case ExcelFieldsType.Enum:

                    int v = 0;

                    if (from_excel)
                    {
                        if (obj != null)
                            int.TryParse(obj.ToString(), out v);
                        return v;
                    }

                    try
                    {
                        v = (int)obj;
                    }
                    catch (Exception)
                    {
                    }

                    return v;

            }

            return string.Empty;
        }
        /// <summary>
        /// 查询Excel最大的列数(仅能在编辑器模式调用,若在运行代码中调用,请添加UNITY_EDITOR宏!)
        /// </summary>
        /// <param name="sheet"></param>
        /// <returns></returns>
        public static int GetExcelCol(ExcelWorksheet sheet)
        {
            int col = GetExcelCol(sheet, 1);

            if (col == 0)
                col = GetExcelCol(sheet, 2);

            if (col == 0)
                col = GetExcelCol(sheet, 3);

            return col;
        }

        private static int GetExcelCol(ExcelWorksheet sheet, int row)
        {
            int col = 0;

            int empty_number = 0;

            for (int i = 1; i <= MAX_COL; i++)
            {
                object v = sheet.GetValue(row, i);

                if (v != null && !string.IsNullOrEmpty(v.ToString()))
                {
                    col = i;
                    empty_number = 0;
                }
                else
                {
                    empty_number++;
                    // 如果连续30个都为空 则认为后面没有数据了
                    if (empty_number > 30)
                        break;
                }
            }

            return col;
        }
        /// <summary>
        /// Excel文件最大的行数
        /// </summary>
        private const int MAX_ROW = 1048576;

        /// <summary>
        /// Excel文件最大的列数
        /// </summary>
        private const int MAX_COL = 16384;

        /// <summary>
        /// 查询Excel最大的行数(仅能在编辑器模式调用,若在运行代码中调用,请添加UNITY_EDITOR宏!)
        /// </summary>
        /// <param name="sheet"></param>
        /// <returns></returns>
        public static int GetExcelRow(ExcelWorksheet sheet)
        {
            int row = GetExcelRow(sheet, 1);

            if (row == 0) // 等于0 说明第一列没有数据 尝试从第二列查找 
                row = GetExcelRow(sheet, 2);

            if (row == 0)// 第二列没有数据 尝试从第三列查找 
                row = GetExcelRow(sheet, 3);

            return row;
        }

        private static int GetExcelRow(ExcelWorksheet sheet, int col)
        {
            int row = 0;

            int empty_number = 0;

            for (int i = 1; i <= MAX_ROW; i++)
            {
                object v = sheet.GetValue(i, col);

                if (v != null && !string.IsNullOrEmpty(v.ToString()))
                {
                    row = i;
                    empty_number = 0;
                }
                else
                {
                    empty_number++;
                    // 如果连续30个都为空 则认为后面没有数据了
                    if (empty_number > 30)
                        break;
                }
            }

            return row;
        }
#endif
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
        #endregion
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
#if UNITY_EDITOR
                AssetDatabase.Refresh();
#endif
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
#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif
            return true;
        }
#if UNITY_EDITOR
        public static List<SerializedProperty> GetAllSerializedProperty(this SerializedObject serializedObject)
        {
            List<SerializedProperty> properties = new List<SerializedProperty>();

            Type type = serializedObject.targetObject.GetType();

            List<FieldInfo> fields = GetFields(type);

            foreach (FieldInfo field in fields)
            {
                if (!field.IsInitOnly) // 忽略只读字段 
                {
                    SerializedProperty property = serializedObject.FindProperty(field.Name);
                    if (property == null) continue;
                    if (IsContainProperty(property, properties)) continue;
                    properties.Add(property);
                }
            }

            return properties;
        }

        private static bool IsContainProperty(SerializedProperty property, List<SerializedProperty> properties)
        {
            foreach (var item in properties)
            {
                if (item == null) continue;
                if (item.name == property.name) return true;
            }

            return false;
        }
#endif
        private static List<FieldInfo> GetFields(Type type)
        {

            if (type == null) return null;

            List<FieldInfo> fields = new List<FieldInfo>();

            fields.AddRange(type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance));
            if (type.BaseType != null)
                fields.AddRange(GetFields(type.BaseType));
            return fields;
        }

    }
}
