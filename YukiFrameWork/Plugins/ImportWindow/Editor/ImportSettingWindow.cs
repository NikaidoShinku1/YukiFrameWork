///=====================================================
/// - FileName:      ImportSettingWindow.cs
/// - NameSpace:     YukiFrameWork.Extension
/// - Created:       Yuki
/// - Email:         1274672030@qq.com
/// - Description:   这是一个框架工具创建的脚本
/// - Creation Time: 2024/1/5 0:57:46
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================

#if UNITY_EDITOR
using UnityEngine;
using System;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System.Linq;
namespace YukiFrameWork.Extension
{
    public class ImportSettingWindow : OdinMenuEditorWindow
    {
        private Vector2 scrollPosition;
        private Data data;
        private VersionData versionData;
        public class Info
        {
            public string email;
            public string url;
        }
        protected override OdinMenuTree BuildMenuTree()
        {
            OdinMenuTree odinMenuTree = new OdinMenuTree();
            odinMenuTree.AddRange(moduleInfo, module => 
            {
                return $"本地工具包/{module.Key}";
            });
            odinMenuTree.Add("内置工具说明文档", new FrameworkInfoByWindow());
            return odinMenuTree;
        }
        public class VersionData
        {
            public string version;
            public Info author;
        }
        [MenuItem("YukiFrameWork/Import Setting Window", false, -1000)]
        static void Open()
        {
            GetWindow<ImportSettingWindow>().titleContent = new GUIContent("框架高级导入窗口");
        }
        public const string importPath = "Packages/com.yuki.yukiframework/Plugins/ImportWindow/Data/ImportPath.json";
        public const string packagePath = "Packages/com.yuki.yukiframework";
        public class Data
        {
            public string path;
            public int develop = 0;
            public bool isEN;
        }
        private static Data customData;
        public static Data GetData()
        {
            if (customData == null)
            {
                TextAsset text = AssetDatabase.LoadAssetAtPath<TextAsset>(importPath);
                customData = JsonUtility.FromJson<Data>(text.text);              
            }
            return customData;
        }        
        /// <summary>
        /// Key:Name  Value:Path
        /// </summary>
        internal static readonly Dictionary<string, string> moduleInfo = new Dictionary<string, string>()
        {
            ["Hyclr"] = packagePath + "/Tool~/HyCLRToolKit",
            ["ActionKit"] = packagePath + "/Tool~/ActionKit",
            ["Bezier"] = packagePath + "/Tool~/Bezier",
            ["SaveTool"] = packagePath + "/Tool~/SaveTool",          
            ["StateManager"] = packagePath + "/Tool~/StateManager",
            ["IOCContainer"] = packagePath + "/Tool~/IOCContainer",
            ["DiaLogKit"] = packagePath + "/Tool~/DiaLogKit",
            ["BuffKit"] = packagePath + "/Tool~/BuffKit",
            ["SkillKit"] = packagePath + "/Tool~/SkillKit",
            ["UI"] = packagePath + "/Tool~/UI",
            ["Audio"] = packagePath + "/Tool~/Audio",
            ["ItemKit"] = packagePath + "/Tool~/ItemKit",          
            ["NavMeshPlus"] = packagePath + "/Tool~/NavMeshPlus",
            ["MissionKit"] = packagePath + "/Tool~/MissionKit",
            ["BehaviourTree"] = packagePath + "/Tool~/BehaviourTree",
            ["StateMechine"] = packagePath + "/Tool~/StateMechine",
        };

        protected override void OnEnable()
        {           
            LoadData();           
        }
        protected override void OnDisable()
        {
            SaveData();         
        }

        void LoadData()
        {
            TextAsset text = AssetDatabase.LoadAssetAtPath<TextAsset>(importPath);
            string json = text.text;
            if (string.IsNullOrEmpty(json))
            {
                json = SerializationTool.SerializedObject(new Data()
                {
                    develop = 0,
                    path = "Assets/YukiFramework",
                    isEN = false,
                });
            }
            data = JsonUtility.FromJson<Data>(json);
            ImportWindowInfo.IsEN = data.isEN;

            TextAsset versionText = AssetDatabase.LoadAssetAtPath<TextAsset>(packagePath + "/package.json");

            versionData = SerializationTool.DeserializedObject<VersionData>(versionText.text);            
            
        }

        void SaveData()
        {
            string json = JsonUtility.ToJson(data);           
            using (FileStream fileStream = new FileStream(importPath, FileMode.Truncate, FileAccess.ReadWrite, FileShare.ReadWrite))
            {                
                StreamWriter streamWriter = new StreamWriter(fileStream);
                               
                streamWriter.Write(json);

                streamWriter.Close();
                fileStream.Close();
            }
            AssetDatabase.Refresh();
        }
        private GUIStyle titleStyle;
        private GUIStyle desStyle;

        protected override void DrawMenu()
        {
            base.DrawMenu();
        }
        protected override void DrawEditors()
        {
            base.DrawEditors();
            var rect = EditorGUILayout.BeginVertical(EditorStyles.helpBox,GUILayout.Width(position.width - MenuWidth));
            EditorGUILayout.BeginHorizontal();           
            if (versionData != null)
            {
                OpenUrl("Gitee", versionData.author.url);
                OpenUrl("作者邮箱:" + versionData.author.email, string.Empty);
                OpenUrl("框架版本:" + versionData.version, string.Empty);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            data.develop = EditorGUILayout.Popup(ImportWindowInfo.DeveloperModeInfo, data.develop, ImportWindowInfo.displayedOptions);
            EditorGUILayout.BeginHorizontal(GUILayout.Width(50));
            GUILayout.Label("EN");
            ImportWindowInfo.IsEN = EditorGUILayout.Toggle(ImportWindowInfo.IsEN);
            data.isEN = ImportWindowInfo.IsEN;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndHorizontal();      
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();

            titleStyle ??= new GUIStyle()
            {
                alignment = TextAnchor.MiddleLeft
            };
            titleStyle.fontSize = 20;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.normal.textColor = Color.white;
            desStyle ??= new GUIStyle()
            {
                alignment = TextAnchor.MiddleLeft
            };
            desStyle.normal.textColor = Color.white;

            desStyle.fontSize =10;        
            if (MenuTree != null)
            {
                foreach (var select in MenuTree.Selection)
                {
                    if (!moduleInfo.ContainsKey(select.Name)) continue;

                    EditorGUILayout.BeginVertical("FrameBox",GUILayout.Width(position.width - MenuWidth));
                    EditorGUILayout.Space(20);
                    GUILayout.Label(select.Name + "    " + "V1.0",titleStyle);
                    EditorGUILayout.Space(10);
                    GUILayout.Label(ImportWindowInfo.GetModuleInfo(select.Name),desStyle);
                    EditorGUILayout.Space(20);

                    EditorGUILayout.BeginHorizontal();
                    DrawBoxGUI(select.Name == "StateMechine" ? Color.yellow : Color.white, string.Format("{0}/{1}", data.path, select.Name), select.Name, moduleInfo[select.Name]);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();

                    EditorGUILayout.BeginVertical();
                    GUI.color = Color.cyan;
                    EditorGUILayout.LabelField("快捷代码示例文档");
                    EditorGUILayout.HelpBox("具体文档请打开下方内置工具文档示例使用,该快捷方式投射底层文本", MessageType.Info);
                    EditorGUILayout.Space();
                    GUI.color = Color.white;
                    string dir = moduleInfo[select.Name];
                    codeStyle ??= new GUIStyle(GUI.skin.box)
                    {
                        alignment = TextAnchor.UpperLeft
                    };
                    codeStyle.normal.textColor = Color.white;
                    
                    foreach (var path in Directory.GetFiles(dir))
                    {
                                            
                        if (path.Contains(".md")) 
                        {
                            EditorGUILayout.LabelField(File.ReadAllText(path), codeStyle);
                        }
                    }
                    EditorGUILayout.EndVertical();
                }
            }         

        }

        private GUIStyle codeStyle;   
    
        private void OpenUrl(string name,string url)
        {
            if (GUILayout.Button(name))
            {
                if (string.IsNullOrEmpty(url))
                    return;
                Application.OpenURL(url);
            }
        }

      /*  private void DrawReverBoxGUI(Color color,string message,MessageType Info, string name)
        {
            EditorGUILayout.HelpBox(message, Info);
            EditorGUILayout.BeginHorizontal();           
            GUI.color = color;
            if (GUILayout.Button($"{name}{(ImportWindowInfo.IsEN ? "": "模块")}", GUILayout.Height(20)))
            {
                foreach (var key in moduleInfo.Keys)
                {
                    string path = string.Format("{0}/YukiFrameWork/", Application.dataPath);
                    if (Directory.Exists(path + key))
                    {                       
                        ReverImport(moduleInfo[key], key);
                    }
                }
            }

            EditorGUILayout.EndHorizontal();
        }*/

        private void DrawBoxGUI(Color color,string path,string name,string copyPath = "")
        {         
            GUI.color = color;
            EditorGUILayout.BeginHorizontal();           
            DrawButtonGUI(path, name,copyPath);          
            EditorGUILayout.EndHorizontal();
            GUI.color = Color.white;
        }

        private void DrawButtonGUI(string path,string name,string copyPath)
        {
            EditorGUILayout.BeginHorizontal();
            if (Directory.Exists(path))
            {
              
                GUI.color = Color.white;
                if (GUILayout.Button(ImportWindowInfo.IsEN ? $"Reload Import {name} Module" : $"重新导入{name}模块",GUILayout.Height(20)))
                {   
                    File.Delete(path + ".meta");
                    Directory.Delete(path,true);
                    Import(copyPath,name);
                }

                if (data.develop == 1)
                {
                    if (GUILayout.Button(ImportWindowInfo.IsEN ? $"Reverse Import {name} Module" : $"反导{name}模块", GUILayout.Height(20)))
                    {
                        ReverImport(copyPath, name);
                    }
                }

                GUI.color = Color.red;
                if (GUILayout.Button(ImportWindowInfo.IsEN ? "Delete Module" : "删除模块", GUILayout.Height(20),data.develop != 1 ? GUILayout.Width(position.width / 2.5f) : GUILayout.Width(position.width / 4f)))
                {
                    File.Delete(path + ".meta");
                    Directory.Delete(path,true);                  
                    AssetDatabase.Refresh();
                }
                GUI.color = Color.white;
            }
            else
            {
                if (GUILayout.Button(ImportWindowInfo.IsEN ? $"Import {name} Module":$"导入{name}模块", GUILayout.Height(20)))
                {
                    Import(copyPath,name);                 
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void Import(string copyPath, string name)
        {
            if (!Directory.Exists(copyPath))
            {
                Debug.LogError(ImportWindowInfo.IsEN ? string.Format("Path lost! Check the Package root directory and be sure to manually add the framework package.json! path:{0}", copyPath) : string.Format("路径丢失！请检查包根目录以及确保在PackageManager手动添加框架Package.json! 路径:{0}", copyPath));
                GUIUtility.ExitGUI();
                return;
            }

            string checkPath = data.path + @"/" + name;
            if (!Directory.Exists(checkPath))
            {
                Directory.CreateDirectory(checkPath);
                AssetDatabase.Refresh();
            }
            var files = Directory.GetFiles(copyPath, "*.*", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                try
                {
                    var newFile = file.Replace(copyPath, "");
                    var newPath = data.path + $"/{name}" + newFile;
                    if (!Directory.Exists(Path.GetDirectoryName(newPath)))
                        Directory.CreateDirectory(Path.GetDirectoryName(newPath));
                    File.Copy(file, newPath, true);
                }
                catch (Exception ex) { throw ex; }
            }
            AssetDatabase.Refresh();
        }
        

        private void ReverImport(string copyPath, string name)
        {
            string checkPath = data.path + @"/" + name;
            if (!Directory.Exists(checkPath))
            {
                Debug.LogError(ImportWindowInfo.IsEN ? $"Cannot anti-missile {name} module, please check whether the {name} module has been imported into the project! -- File:{checkPath}" :$"无法反导{name}模块,请检查项目中是否已经导入了{name}模块! -- File:{checkPath}");
                GUIUtility.ExitGUI();
                return;
            }
            var rootPath = data.path + "/" + name;
            var files = Directory.GetFiles(rootPath, "*.*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                try
                {
                    var newFile = file.Replace(rootPath, "");
                    var newPath = copyPath + newFile;
                    if (!Directory.Exists(Path.GetDirectoryName(newPath)))
                        Directory.CreateDirectory(Path.GetDirectoryName(newPath));
                    File.Copy(file, newPath, true);                 
                }
                catch (Exception ex) { throw ex; }
            }
            AssetDatabase.Refresh();
        }
        public class FrameworkInfoByWindow
        {
            public string updateInfo => AssetDatabase.LoadAssetAtPath<TextAsset>(ImportSettingWindow.packagePath + "/Framework/Extension/UpdateInfo.md")?.text;

            [OnInspectorGUI]
            void OnInspectorGUI()
            {
                var title = new GUIStyle()
                {
                    fontSize = 20,
                    alignment = TextAnchor.UpperCenter,
                    fontStyle = FontStyle.Bold

                };
                GUILayout.BeginVertical();
                UnityEditor.EditorGUILayout.HelpBox("框架使用提示:当使用hybridclr等热更新插件时，可打开架构构造器脚本。\n因适配问题可将RuntimeInitializeOnLoadMethod特性注释，并在合适的时机手动调用ArchitectureConstructor.InitArchitecture()方法", MessageType.Warning);
                if (GUILayout.Button("打开脚本", GUILayout.Width(100)))
                {
                    AssetDatabase.OpenAsset(UnityEditor.AssetDatabase.LoadAssetAtPath<MonoScript>(ImportSettingWindow.packagePath + "/Framework/ViewController/" + "ArchitectureConstructor" + ".cs"));
                }
                GUILayout.EndVertical();
                title.normal.textColor = Color.white;
                GUILayout.Label("YukiFramework", title);
                GUILayout.Label("工具教学链接");
                GUILayout.BeginHorizontal();
                GUILayout.BeginVertical(GUILayout.Width(200));
                Button("LocalizationKit_本地化套件", "https://gitee.com/NikaidoShinku/YukiFrameWork/blob/master/YukiFrameWork/Framework/Localization/LocalizationInfo.md");
                Button("DiaLogKit_对话系统", "https://gitee.com/NikaidoShinku/YukiFrameWork/blob/master/YukiFrameWork/Tool~/DiaLogKit/DiaLogKit.md");
                Button("BuffKit_框架Buff系统", "https://gitee.com/NikaidoShinku/YukiFrameWork/blob/master/YukiFrameWork/Tool~/BuffKit/BuffKit.md");
                Button("LogKit_日志工具", "https://gitee.com/NikaidoShinku/YukiFrameWork/blob/master/YukiFrameWork/Framework/LogKit/15.%E6%8E%A7%E5%88%B6%E5%8F%B0%E6%97%A5%E5%BF%97%E5%B7%A5%E5%85%B7.md");
                Button("ItemKit_框架背包系统", "https://gitee.com/NikaidoShinku/YukiFrameWork/blob/master/YukiFrameWork/Tool~/ItemKit/14.%E8%83%8C%E5%8C%85%E7%B3%BB%E7%BB%9F(%E9%80%9A%E7%94%A8).md");
                Button("MissionKit框架任务系统", "https://gitee.com/NikaidoShinku/YukiFrameWork/blob/master/YukiFrameWork/Tool~/MissionKit/MissionKit.md");
                Button("SerializationTool_框架序列化工具使用", "https://gitee.com/NikaidoShinku/YukiFrameWork/blob/master/YukiFrameWork/Plugins/Serialization/%E5%BA%8F%E5%88%97%E5%8C%96%E5%B7%A5%E5%85%B7.md");
                Button("SaveTool_框架存档工具使用", "https://gitee.com/NikaidoShinku/YukiFrameWork/blob/master/YukiFrameWork/Tool~/SaveTool/%E5%AD%98%E6%A1%A3%E7%B3%BB%E7%BB%9F.md");
                Button("框架引导工具使用", "https://gitee.com/NikaidoShinku/YukiFrameWork/blob/master/YukiFrameWork/Tool~/PilotKit/PilotKit.md");
                Button("Singleton_框架万能单例介绍", "https://gitee.com/NikaidoShinku/YukiFrameWork/blob/master/YukiFrameWork/Framework/Singleton/9.%E5%8D%95%E4%BE%8B.md");
                Button("EventSystem_框架广播系统介绍", "https://gitee.com/NikaidoShinku/YukiFrameWork/blob/master/YukiFrameWork/Framework/Events/7.%E6%B6%88%E6%81%AF%E5%B9%BF%E6%92%AD%E6%A8%A1%E5%9D%97.md");
                Button("UIKit_框架UI模块", "https://gitee.com/NikaidoShinku/YukiFrameWork/blob/master/YukiFrameWork/Tool~/UI/6.UI%E6%A8%A1%E5%9D%97.md");
                Button("AudioKit_框架声音管理", "https://gitee.com/NikaidoShinku/YukiFrameWork/blob/master/YukiFrameWork/Tool~/Audio/8.%E5%A3%B0%E9%9F%B3%E7%AE%A1%E7%90%86%E6%A8%A1%E5%9D%97.md");
                Button("ActionKit_时序动作套件说明", "https://gitee.com/NikaidoShinku/YukiFrameWork/blob/master/YukiFrameWork/Tool~/ActionKit/5.%E5%8A%A8%E4%BD%9C%E6%97%B6%E5%BA%8F%E7%AE%A1%E7%90%86%E6%A8%A1%E5%9D%97.md");
                Button("Old StateManager_框架可视化状态机模块", "https://gitee.com/NikaidoShinku/YukiFrameWork/blob/master/YukiFrameWork/Tool~/StateMechine/3.%E7%8A%B6%E6%80%81%E6%9C%BA.md", Color.yellow);
                Button("StateManager_全新动作设计状态机模块", "https://gitee.com/NikaidoShinku/YukiFrameWork/blob/master/YukiFrameWork/Tool~/StateManager/StateManager.md");
                Button("Extension_框架拓展模块", "https://gitee.com/NikaidoShinku/YukiFrameWork/blob/master/YukiFrameWork/Framework/Extension/13.%E6%8B%93%E5%B1%95.md");
                Button("PoolsKit_框架设置简易对象池", "https://gitee.com/NikaidoShinku/YukiFrameWork/blob/master/YukiFrameWork/Framework/Pools/12.%E5%AF%B9%E8%B1%A1%E6%B1%A0%E6%A8%A1%E5%9D%97.md");
                Button("BindableProperty强化数据绑定类", "https://gitee.com/NikaidoShinku/YukiFrameWork/blob/master/YukiFrameWork/Framework/Abstract/11.BindableProperty.md");
                Button("BezierUtility_框架贝塞尔曲线拓展", "https://gitee.com/NikaidoShinku/YukiFrameWork/blob/master/YukiFrameWork/Tool~/Bezier/Bezier.md");
                Button("XFABManager插件官网地址", "https://gitee.com/xianfengkeji/xfabmanager");
                Button("2d NavMeshPlus 插件官网地址:", "https://github.com/h8man/NavMeshPlus/tree/master");
                Button("BehaviourTree文档示例", "https://gitee.com/NikaidoShinku/YukiFrameWork/blob/master/YukiFrameWork/Tool~/BehaviourTree/BehaviourTree.md");
                GUILayout.EndVertical();
                GUILayout.BeginVertical(GUILayout.Width(400));
                GUILayout.Label(updateInfo, "Framebox");
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();

            }

            void Button(string name, string url, Color color = default)
            {
                if (color != default)
                    GUI.color = color;
                if (GUILayout.Button(name, GUILayout.Height(40), GUILayout.Width(250)))
                {
                    Application.OpenURL(url);
                }
                GUI.color = Color.white;
                GUILayout.Space(5);
            }

        }

    }
}
#endif