#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Sirenix.OdinInspector.Editor;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace YukiFrameWork.Extension
{
    public class SerializationWindow : OdinEditorWindow
    {
        private SerializeConfig config;
        private Vector2 scrollPosition;       
        private object genericObj = null;
        [MenuItem("YukiFrameWork/SerializationTool",false)]
        public static void OpenWindow()
        {
            var window = GetWindow<SerializationWindow>();

            window.Show();

            window.titleContent = new GUIContent("框架序列化工具窗口");
        }
        class JsonData
        {
            [LabelText("文件路径"),HorizontalGroup]
            public string jsonPath;
            [LabelText("文件名称"),HorizontalGroup]
            public string jsonName;
            [InfoBox("判断多少行是表头")]
            [LabelText("表头")]
            public int header;

            [InfoBox("该项会过滤包含这里输入的字符的列")]
            [LabelText("过滤字符")]
            public string ex_suffix;
        }
        private int selectIndex;
        [LabelText("保存所有的excel路径配置"),SerializeField,Sirenix.OdinInspector.DictionaryDrawerSettings(KeyLabel = "Excel文件路径",ValueLabel = "导出的Json文件路径"),ShowIf(nameof(selectIndex),1)]
        private Dictionary<string, JsonData> excelPaths = new Dictionary<string, JsonData>();
        private const string serializationSelectIndexItem = "serializationSelectIndexItem";
        protected override void OnEnable()
        {
            base.OnEnable();
            selectIndex = PlayerPrefs.GetInt(nameof(serializationSelectIndexItem));
            config ??= new SerializeConfig();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            PlayerPrefs.SetInt(nameof(serializationSelectIndexItem), selectIndex);
        }

        protected override void OnImGUI()
        {
            EditorGUILayout.Space();
            selectIndex = GUILayout.Toolbar(selectIndex, new string[]
            {
                "C#转文件流窗口","Excel转Json窗口"
            });

            EditorGUILayout.Space(20);
            var rect = EditorGUILayout.BeginVertical("LODBlackBox", GUILayout.Height(150), GUILayout.Width(position.width));
            Event e = Event.current;
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            GUIStyle style = new GUIStyle()
            {
                fontSize = 14
            };

            style.normal.textColor = Color.yellow;
            GUILayout.Label(selectIndex == 0 ? "将脚本拖入这块区域(如果拖了多个脚本则只按照第一个脚本处理)" : "将Excel表格拖入这块区域", style);
            EditorGUILayout.EndVertical();
            if (rect.Contains(e.mousePosition) && e.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();

                if (selectIndex == 0)
                {
                    MonoScript asset = DragAndDrop.objectReferences.Where(x => x.GetType().Equals(typeof(MonoScript))).FirstOrDefault() as MonoScript;

                    if (asset != null)
                    {
                        try
                        {
                            if (asset.GetClass().IsSubclassOf(typeof(UnityEngine.Object)))
                                throw new Exception(new Exception().ToString() + "不允许拖入派生自UnityEngine.Object的脚本类");

                            config.serializedType = asset.GetClass().ToString();
                        }
                        catch (Exception ex)
                        { throw new Exception(ex.ToString() + "拖进来的脚本里面的内容可能不是类或者它派生自UnityEngine.Object!"); }

                    }
                }
                else
                {                  
                    var asset = DragAndDrop.objectReferences;
                    if (asset != null)
                    {
                        foreach (var item in asset)
                        {
                            string path = AssetDatabase.GetAssetPath(item);

                            if ((path.EndsWith("xlsx") || path.EndsWith("xls")) && !path.StartsWith("~$"))
                            {
                                string[] title = Path.GetFileName(path).Split('.');
                                excelPaths[path] = new JsonData()
                                {
                                    jsonPath = "Assets",
                                    jsonName = title[0], 
                                    header = 3
                                };
                            }
                        }
                    }
                }

            }
            if (selectIndex == 0)
                DrawSerializableInfo();            
            base.OnImGUI();                       
        }
        private bool showBtn => excelPaths != null && excelPaths.Count > 0 && selectIndex == 1;
        [Button("创建Json文件",ButtonHeight = 25),PropertySpace(20),ShowIf(nameof(showBtn))]
        private void DrawExcelToJsonInfo()
        {
            foreach (var excelPath in excelPaths.Keys)
            {
                JsonData data = excelPaths[excelPath];
                string json = SerializationTool.ExcelToJson(excelPath, data.header, data.ex_suffix);
                json.CreateFileStream(data.jsonPath,data.jsonName,".json");
            }
        }

        private void DrawSerializableInfo()
        {                  
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("当前会序列化的类:   " + config.serializedType, "ProfilerBadge");
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("清空",GUILayout.Width(100))) 
                config.serializedType = string.Empty;
            EditorGUILayout.EndHorizontal();
            if (!string.IsNullOrEmpty(config.serializedType))
            {
                EditorGUILayout.Space();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("转换的类型:");
                config.type = (SerializationType)EditorGUILayout.EnumPopup(config.type);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.HelpBox("警告:如果要转换成Xml一定要记得标记序列化Xml需要的特性例如XmlAttribute,如果序列化字节且是派生类的情况下则必须给该继承关系所有类打上Serializable特性", MessageType.Warning);
                EditorGUILayout.BeginVertical(GUI.skin.box);


                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                Type type = AssemblyHelper.GetType(config.serializedType);
                if (genericObj == null || !genericObj.GetType().Equals(type))
                    genericObj = Activator.CreateInstance(type);
                if (genericObj != null)
                {
                    string info = config.SerializationInfo(genericObj);
                    GUILayout.Label(info);
                    EditorGUILayout.EndScrollView();
                    config.genericPath = EditorGUILayout.TextField("生成文件流路径:", config.genericPath);
                    config.IsCustomOpen = EditorGUILayout.ToggleLeft("是否自定义文件流名称(不开启时默认以类名作为名称)", config.IsCustomOpen);
                    if (config.IsCustomOpen)
                        config.customName = EditorGUILayout.TextField(config.customName, GUILayout.Height(15));
                    EditorGUILayout.Space(20);
                    if (GUILayout.Button("创建文件流", GUILayout.Height(25)))
                    {
                        if (string.IsNullOrEmpty(info))
                        {
                            Debug.LogError("没有正确序列化，无法创建文件流: Class Type:" + type);
                            return;
                        }
                        if (config.type == SerializationType.Bytes)
                        {
                            Debug.LogError("因二进制的特殊性，暂时不支持创建对应文件流，请改变创建类型");
                            return;
                        }

                        if (config.IsCustomOpen && string.IsNullOrEmpty(config.customName))
                        {
                            Debug.LogError("当前开启了自定义名称,名称为空无法创建!");
                            return;
                        }

                        if (!Directory.Exists(config.genericPath))
                        {
                            Directory.CreateDirectory(config.genericPath);
                            AssetDatabase.Refresh();
                        }
                        info.CreateFileStream(config.genericPath, config.IsCustomOpen ? config.customName : type.Name, config.GetSuffix());
                    }
                }
                EditorGUILayout.EndVertical();
            }
           
        }
    }    
}
#endif