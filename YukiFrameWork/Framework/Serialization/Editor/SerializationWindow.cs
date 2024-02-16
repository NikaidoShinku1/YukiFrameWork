#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace YukiFrameWork.Extension
{
    public class SerializationWindow : EditorWindow
    {
        private SerializeConfig config;
        private Vector2 scrollPosition;
        private GUIStyle urlStyle;
        private object genericObj = null;
        [MenuItem("YukiFrameWork/脚本序列化工具",false)]
        public static void OpenWindow()
        {
            var window = GetWindow<SerializationWindow>();

            window.Show();

            window.titleContent = new GUIContent("脚本快速序列化工具");
        }

        private void OnEnable()
        {
            config ??= new SerializeConfig();
        } 

        private void OnGUI()
        {
            EditorGUILayout.Space();
            DrawSerializableInfo();
        }

        private void DrawSerializableInfo()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Excel插件Github地址:",GUILayout.Width(150));
            urlStyle ??= new GUIStyle(GUI.skin.label);
            urlStyle.richText = true;
            urlStyle.normal.textColor = Color.yellow;
            urlStyle.fontStyle = FontStyle.Italic;
            urlStyle.alignment = TextAnchor.MiddleCenter;
            if (GUILayout.Button("https://github.com/neil3d/excel2json", urlStyle))
            {
                Application.OpenURL("https://github.com/neil3d/excel2json");
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            if (GUILayout.Button("Excel拓展:打开Excel转Json/C#插件",GUILayout.Height(30)))
            {
                string text = ImportSettingWindow.packagePath;
                UnityEngine.Object excel = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(text + @"/Framework/Serialization/excel2json/excel2json.exe");
                AssetDatabase.OpenAsset(excel);
            }
            EditorGUILayout.Space(20);
            var rect = EditorGUILayout.BeginVertical("LODBlackBox", GUILayout.Height(150),GUILayout.Width(position.width));
            Event e = Event.current;
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            GUIStyle style = new GUIStyle()
            {
                fontSize = 14
            };

            style.normal.textColor = Color.yellow;
            GUILayout.Label("将脚本拖入这块区域(如果拖了多个脚本则只按照第一个脚本处理)", style) ;
            EditorGUILayout.EndVertical();
            if (rect.Contains(e.mousePosition) && e.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();

                MonoScript asset = DragAndDrop.objectReferences.Where(x => x.GetType().Equals(typeof(MonoScript))).FirstOrDefault() as MonoScript;
             
                if (asset != null)
                {
                    try
                    {
                        if (asset.GetClass().IsSubclassOf(typeof(UnityEngine.Object)))                        
                            throw LogKit.Exception(new Exception().ToString() + "不允许拖入派生自UnityEngine.Object的脚本类");
                           
                        config.serializedType = asset.GetClass().ToString();
                    }
                    catch (Exception ex)
                    { throw LogKit.Exception(ex.ToString() + "拖进来的脚本里面的内容可能不是类或者它派生自UnityEngine.Object!"); }
                   
                }
             
            }
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("当前会序列化的类:   " + config.serializedType, "ProfilerBadge");
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("清空",GUILayout.Width(100))) config.serializedType = string.Empty;
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
                string info = config.SerializationInfo(genericObj);
                GUILayout.Label(info);
                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndVertical();
                config.genericPath = EditorGUILayout.TextField("生成文件流路径:", config.genericPath);    
                config.IsCustomOpen = EditorGUILayout.ToggleLeft("是否自定义文件流名称(不开启时默认以类名作为名称)",config.IsCustomOpen);
                if (config.IsCustomOpen)
                    config.customName = EditorGUILayout.TextField(config.customName,GUILayout.Height(15));
                EditorGUILayout.Space(20);
                if (GUILayout.Button("创建文件流",GUILayout.Height(25)))
                {
                    if (config.IsCustomOpen && string.IsNullOrEmpty(config.customName))
                    {
                        LogKit.E("当前开启了自定义名称,名称为空无法创建!");
                        return;
                    }

                    if (!Directory.Exists(config.genericPath))
                    {
                        Directory.CreateDirectory(config.genericPath);
                        AssetDatabase.Refresh();
                    }

                    using FileStream stream = new FileStream($"{config.genericPath}/{(config.IsCustomOpen ? config.customName : type.Name)}{config.GetSuffix()}",FileMode.Create,FileAccess.ReadWrite,FileShare.ReadWrite);

                    if (string.IsNullOrEmpty(info))
                    {
                        LogKit.E("数据信息为空,请检查是否正确序列化!");
                        return;
                    }

                    if (config.type == SerializationType.Bytes)
                    {
                        BinaryFormatter binaryFormatter = new BinaryFormatter();

                        stream.Seek(0, SeekOrigin.Begin);
                        stream.SetLength(0);

                        binaryFormatter.Serialize(stream,genericObj);
                    }
                    else
                    {
                        StreamWriter writer = new StreamWriter(stream);

                        writer.Write(info);

                        writer.Close();                      
                    }
                    stream.Close();

                    AssetDatabase.Refresh();
                }
            }
           
        }
    }    
}
#endif