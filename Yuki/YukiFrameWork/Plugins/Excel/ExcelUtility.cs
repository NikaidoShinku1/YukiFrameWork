using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Excel;
using System.IO;
using System.Data;
using System.Text;
using Newtonsoft.Json;
namespace YukiFrameWork.Excel
{
    /// <summary>
    /// Excel生成工具
    /// </summary>
    public class ExcelUtility
    {
        private DataSet mResultSet;

        public ExcelUtility(string excelFile)
        {
            FileStream stream = File.Open(excelFile, FileMode.Open, FileAccess.Read);

            IExcelDataReader dataReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
            mResultSet = dataReader.AsDataSet();
        }

        public void ConvertToJson(string JsonPath, Encoding encoding)
        {
            if (mResultSet.Tables.Count < 1)
            {
                Debug.LogError($"不存在数据表请重试,数据集合{mResultSet}");
                return;
            }

            DataTable mSheet = mResultSet.Tables[0];

            if (mSheet.Rows.Count < 1) return;

            int rowCount = mSheet.Rows.Count;
            int colCount = mSheet.Columns.Count;

            List<Dictionary<string, object>> table = new List<Dictionary<string, object>>();

            for (int i = 1; i < rowCount; i++)
            {
                Dictionary<string, object> row = new Dictionary<string, object>();
                for (int j = 0; j < colCount; j++)
                {
                    string field = mSheet.Rows[0][j].ToString();
                    if (!string.IsNullOrEmpty(field))
                    {
                        var obj = mSheet.Rows[i][j];                      
                        row[field] = obj;
                        if (obj.ToString().LastIndexOf(".") != -1)obj.ToString().Substring(0, obj.ToString().LastIndexOf("."));
                    }
                }               
                table.Add(row);
            }
            
            //格式化Json
            string json = JsonConvert.SerializeObject(table, Formatting.Indented,new ExcelReslover());
            Debug.Log(json);
            using (FileStream stream = new FileStream(JsonPath, FileMode.Create, FileAccess.Write))
            {
                using (TextWriter textWriter = new StreamWriter(stream, encoding))
                {
                    textWriter.Write(json);
                }
            }
        }
    }
}
