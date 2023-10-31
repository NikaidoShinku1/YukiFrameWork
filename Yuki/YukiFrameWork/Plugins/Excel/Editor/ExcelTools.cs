using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Text;

#if UNITY_EDITOR
namespace YukiFrameWork.Excel
{
    public enum EncodingType
    {
        UTF8,
        ASCII
    }

    public class ExcelTools : EditorWindow
    {
        private static ExcelTools instance;

        private static EncodingType type;

        private static Rect rect;

        private static List<string> excelList;

        private static string pathRoot;

        private static string createJsonPath;

        private static bool keepSource = true;

        [MenuItem("YukiFrameWork/ExcelToJson")]
        private static void ExcelToJson()
        {
            Init();
            LoadExcel();
            instance.Show();

        }

        private static void Init()
        {
            instance = GetWindow<ExcelTools>();
            createJsonPath = "";
            rect = new Rect(0, 0, 400, 300);
            instance.position = rect;
            instance.titleContent = new GUIContent("Excelתjson����");
            pathRoot = Application.dataPath;
            pathRoot = pathRoot.Substring(0, pathRoot.LastIndexOf("/"));          

        }

        private void OnGUI()
        {
            DrawOptions();
            DrawExport();
        }

        private void DrawExport()
        {
            if (excelList == null) return;
            if (excelList.Count < 1)
            {
                EditorGUILayout.LabelField("��ǰ��ûѡ��excel�ļ�");
            }
            else
            {
                GUILayout.BeginVertical();
                foreach (var s in excelList)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Toggle(true, s);
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();

                if (GUILayout.Button("����Json�ļ�"))
                {
                    Convert();
                }
            }
        }

        private static void Convert()
        {
            if (String.IsNullOrEmpty(createJsonPath))
            {
                Debug.LogError("·��������");
                return;
            }
            foreach (var excelPath in excelList)
            {
                string overPath = pathRoot + "/" + excelPath;
                ExcelUtility excel = new ExcelUtility(overPath);

                Encoding encoding = null;
                switch (type)
                {
                    case EncodingType.UTF8:
                        encoding = Encoding.UTF8;
                        break;
                    case EncodingType.ASCII:
                        encoding = Encoding.ASCII;
                        break;
                }

                string ex = overPath.Substring(overPath.LastIndexOf("/"));
                string exName = ex.Substring(0, ex.LastIndexOf("."));
                string output = createJsonPath + exName + ".Json";
                excel.ConvertToJson(output, encoding);

                if (!keepSource)
                {
                    FileUtil.DeleteFileOrDirectory(excelPath);
                }
                AssetDatabase.Refresh();
            }
            instance.Close();


        }

        private void DrawOptions()
        {
            GUILayout.BeginHorizontal();
            type = (EncodingType)EditorGUILayout.EnumPopup("��ѡ���������", type);

            GUILayout.EndHorizontal();
            keepSource = EditorGUILayout.Toggle("�Ƿ���Դ�ļ�", keepSource);

            GUILayout.BeginHorizontal();
            createJsonPath = GUILayout.TextField(createJsonPath);
            if (GUILayout.Button("ѡ�������ļ���", GUILayout.Width(150)))
            {
                createJsonPath = EditorUtility.SaveFolderPanel("Save Json Folder", "", "");
            }
            GUILayout.EndHorizontal();
        }

        public void OnSelectionChange()
        {
            Show();
            LoadExcel();
            Repaint();

        }

        private static void LoadExcel()
        {
            excelList = new List<string>();

            object[] selection = Selection.objects;

            if (selection.Length == 0) return;

            foreach (var obj in selection)
            {
                string path = AssetDatabase.GetAssetPath(obj as UnityEngine.Object);
                if (path != null)
                {
                    if (path.EndsWith(".xlsx"))
                    {
                        excelList.Add(path);
                    }
                }
            }
        }
    }
}
#endif