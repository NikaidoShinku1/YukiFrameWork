///=====================================================
/// - FileName:      JsonExporter.cs
/// - NameSpace:     YukiFrameWork.Example
/// - Description:   Excel-Json解析器
/// - Creation Time: 2024/4/11 19:00:01
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using System;
using System.Collections.Generic;
using System.Data;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using YukiFrameWork.Extension;
using System.Diagnostics;
namespace YukiFrameWork
{
	public class JsonExporter
	{
		string mContext = string.Empty;
		int mHeaderRows = 0;

        private string fileName;

        public string context => mContext;

        public JsonExporter(ExcelLoader excel, bool lowcase, bool exportArray, string dateFormat, bool forceSheetName, int headerRows, string excludePrefix, bool cellJson, bool allString)
        {
            fileName = excel.filePath;
            mHeaderRows = headerRows - 1;
            List<DataTable> validSheets = new List<DataTable>();

            for (int i = 0; i < excel.Sheets.Count; i++)
            {
                DataTable table = excel.Sheets[i];

                ///如果没有这个表就过滤
                if (table == null) continue;

                string sheetName = table.TableName;
               
                ///过滤包含特定字符的菜单
                if (!string.IsNullOrEmpty(excludePrefix) && sheetName.StartsWith(excludePrefix))      
                    continue;                

                if (table.Columns.Count > 0 && table.Rows.Count > 0)              
                    validSheets.Add(table);                                                   
            }

            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                DateFormatString = dateFormat,
                Formatting = Formatting.Indented
            };

            if (!forceSheetName && validSheets.Count == 1)
            {
                object tableValue = convertSheet(validSheets[0], exportArray, lowcase, excludePrefix, cellJson, allString);
                mContext = JsonConvert.SerializeObject(tableValue, settings);
            }
            else
            {
                Dictionary<string,object> datas = new Dictionary<string,object>();

                foreach (var table in validSheets)
                {
                    object tableValue = convertSheet(table, exportArray, lowcase, excludePrefix, cellJson, allString);
                    datas.Add(table.TableName, tableValue);
                }

                mContext = JsonConvert.SerializeObject(datas, settings);
            }
        }

        private object convertSheet(DataTable sheet, bool exportArray, bool lowcase, string excludePrefix, bool cellJson, bool allString)
        {
            if (exportArray)
                return convertSheetToArray(sheet, lowcase, excludePrefix, cellJson, allString);
            else
                return convertSheetToDict(sheet, lowcase, excludePrefix, cellJson, allString);
        }

        private object convertSheetToArray(DataTable sheet, bool lowcase, string excludePrefix, bool cellJson, bool allString)
        {
            List<object> values = new List<object>();

            int firstDataRow = mHeaderRows;

            Dictionary<string, string> types = new Dictionary<string, string>();

            foreach (DataColumn column in sheet.Columns)
            {
                DataRow row_type = sheet.Rows[0]; // 默认第一行是字段名称
                if (!types.ContainsKey(column.ToString()))
                    types.Add(column.ToString(), row_type[column].ToString());
                else
                    throw new Exception(string.Format("发现重复的列:{0} fileName:{1}", column.ToString(), fileName));
            }


            for (int i = firstDataRow; i < sheet.Rows.Count; i++)
            {
                DataRow row = sheet.Rows[i];

                Dictionary<string, object> rowObj = convertRowToDict(sheet, row, lowcase, firstDataRow, excludePrefix, cellJson, allString);

                Dictionary<string, object> convertRowObj = new Dictionary<string, object>();

                foreach (var item in rowObj.Keys)
                {
                    if (string.IsNullOrEmpty(types[item])) continue;
                    convertRowObj.Add(item, TryParseContents(types[item], rowObj[item], item, i));
                }

                values.Add(convertRowObj);
            }

            return values;
        }

        public object TryParseContents(string type, object obj, string column, int row)
        {
            row = row + 2;         
            if (type.ToLower().Contains("string"))
                return obj.ToString();
            switch (type.ToLower().ToString())
            {               
                case "json":
                    try
                    {
                        return JsonConvert.DeserializeObject(obj.ToString());
                    }
                    catch (Exception e)
                    {
                        throw new Exception(string.Format("json解析异常:{0} column:{1} row:{2} file:{3} content:{4} error:{5}", type.ToLower(), column, row, fileName, obj.ToString(), e.ToString()));
                    }
                case "int":
                    try
                    {
                        return int.Parse(obj.ToString().Trim('"'));
                    }
                    catch (Exception e)
                    {
                        throw new Exception(string.Format("int解析异常:{0} column:{1} row:{2} file:{3} content:{4} error:{5}", type.ToLower(), column, row, fileName, obj.ToString(), e.ToString()));
                    }
                case "float":
                    try
                    {
                        return float.Parse(obj.ToString().Trim('"'));
                    }
                    catch (Exception e)
                    {
                        throw new Exception(string.Format("float解析异常:{0} column:{1} row:{2} file:{3} content:{4} error:{5}", type.ToLower(), column, row, fileName, obj.ToString(), e.ToString()));
                    }
                case "double":
                    try
                    {
                        return double.Parse(obj.ToString().Trim('"'));
                    }
                    catch (Exception e)
                    {
                        throw new Exception(string.Format("double解析异常:{0} column:{1} row:{2} file:{3} content:{4} error:{5}", type.ToLower(), column, row, fileName, obj.ToString(), e.ToString()));
                    }
                case "long":
                    try
                    {
                        return long.Parse(obj.ToString().Trim('"'));
                    }
                    catch (Exception e)
                    {
                        throw new Exception(string.Format("long解析异常:{0} column:{1} row:{2} file:{3} content:{4} error:{5}", type.ToLower(), column, row, fileName, obj.ToString(), e.ToString()));
                    }
                case "uint":
                    try
                    {
                        return uint.Parse(obj.ToString().Trim('"'));
                    }
                    catch (Exception e)
                    {
                        throw new Exception(string.Format("uint解析异常:{0} column:{1} row:{2} file:{3} content:{4} error:{5}", type.ToLower(), column, row, fileName, obj.ToString(), e.ToString()));
                    }
                case "short":
                    try
                    {
                        return short.Parse(obj.ToString().Trim('"'));
                    }
                    catch (Exception e)
                    {
                        throw new Exception(string.Format("short解析异常:{0} column:{1} row:{2} file:{3} content:{4} error:{5}", type.ToLower(), column, row, fileName, obj.ToString(), e.ToString()));
                    }
                case "bool":
                    try
                    {
                        return bool.Parse(obj.ToString().Trim('"'));
                    }
                    catch (Exception e)
                    {
                        throw new Exception(string.Format("bool解析异常:{0} column:{1} row:{2} file:{3} content:{4} error:{5}", type.ToLower(), column, row, fileName, obj.ToString(), e.ToString()));
                    }
                default:
                    throw new Exception(string.Format("未知的类型:{0} column:{1} row:{2} file:{3}", type.ToLower(), column, row, fileName));
            }

        }

        /// <summary>
        /// 以第一列为ID，转换成ID->Object的字典对象
        /// </summary>
        private object convertSheetToDict(DataTable sheet, bool lowcase, string excludePrefix, bool cellJson, bool allString)
        {
            Dictionary<string, object> importData =
                new Dictionary<string, object>();

            int firstDataRow = mHeaderRows;
            for (int i = firstDataRow; i < sheet.Rows.Count; i++)
            {
                DataRow row = sheet.Rows[i];
                string ID = row[sheet.Columns[0]].ToString();
                if (ID.Length <= 0)
                    ID = string.Format("row_{0}", i);

                var rowObject = convertRowToDict(sheet, row, lowcase, firstDataRow, excludePrefix, cellJson, allString);               
                importData[ID] = rowObject;
            }

            return importData;
        }

        /// <summary>
        /// 把一行数据转换成一个对象，每一列是一个属性
        /// </summary>
        private Dictionary<string, object> convertRowToDict(DataTable sheet, DataRow row, bool lowcase, int firstDataRow, string excludePrefix, bool cellJson, bool allString)
        {
            var rowData = new Dictionary<string, object>();
            int col = 0;
            foreach (DataColumn column in sheet.Columns)
            {
                // 过滤掉包含指定前缀的列
                string columnName = column.ToString();
                if (!string.IsNullOrEmpty(excludePrefix) && columnName.StartsWith(excludePrefix))
                    continue;

                object value = row[column];

                // 尝试将单元格字符串转换成 Json Array 或者 Json Object
                if (cellJson)
                {
                    string cellText = value.ToString().Trim();
                    if (cellText.StartsWith("[") || cellText.StartsWith("{"))
                    {
                        try
                        {
                            object cellJsonObj = JsonConvert.DeserializeObject(cellText);
                            if (cellJsonObj != null)
                                value = cellJsonObj;
                        }
                        catch (Exception exp)
                        {
                            UnityEngine.Debug.Log(exp);
                        }
                    }
                }

                if (value.GetType() == typeof(System.DBNull))
                {
                    value = getColumnDefault(sheet, column, firstDataRow);
                }
                else if (value.GetType() == typeof(double))
                { // 去掉数值字段的“.0”
                    double num = (double)value;
                    if ((int)num == num)
                        value = (int)num;
                }

                //全部转换为string
                //方便LitJson.JsonMapper.ToObject<List<Dictionary<string, string>>>(textAsset.text)等使用方式 之后根据自己的需求进行解析
                if (allString && !(value is string))
                {
                    value = value.ToString();
                }

                string fieldName = column.ToString();
                // 表头自动转换成小写
                if (lowcase)
                    fieldName = fieldName.ToLower();

                if (string.IsNullOrEmpty(fieldName))
                    fieldName = string.Format("col_{0}", col);

                rowData[fieldName] = value;
                col++;
            }

            return rowData;
        }

        /// <summary>
        /// 对于表格中的空值，找到一列中的非空值，并构造一个同类型的默认值
        /// </summary>
        private object getColumnDefault(DataTable sheet, DataColumn column, int firstDataRow)
        {
            for (int i = firstDataRow; i < sheet.Rows.Count; i++)
            {
                object value = sheet.Rows[i][column];
                Type valueType = value.GetType();
                if (valueType != typeof(System.DBNull))
                {
                    if (valueType.IsValueType)
                        return Activator.CreateInstance(valueType);
                    break;
                }
            }
            return "";
        }

        /// <summary>
        /// 将内部数据转换成Json文本，并保存至文件
        /// </summary>
        /// <param name="jsonPath">输出文件路径</param>
        public void SaveToFile(string filePath, Encoding encoding)
        {
            //-- 保存文件
            using (FileStream file = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                using (TextWriter writer = new StreamWriter(file, encoding))
                    writer.Write(mContext);
            }
        }

    }
}
