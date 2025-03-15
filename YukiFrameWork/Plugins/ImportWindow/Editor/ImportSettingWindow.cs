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
        [Serializable]
        public class ToolDataInfo
        {
            public string key;
            public string path;
            public bool active;
            public string url;
        }
        /// <summary>
        /// Key:Name  Value:Path
        /// </summary>
        internal static readonly Dictionary<string, ToolDataInfo> moduleInfo = new Dictionary<string, ToolDataInfo>()
        {
            ["ActionKit"] = new ToolDataInfo()
            {
                key = "ActionKit",
                path = packagePath + "/Tool~/ActionKit",
                active = true,
                url = "https://gitee.com/NikaidoShinku/YukiFrameWork/blob/master/YukiFrameWork/Tool~/ActionKit/5.%E5%8A%A8%E4%BD%9C%E6%97%B6%E5%BA%8F%E7%AE%A1%E7%90%86%E6%A8%A1%E5%9D%97.md"

            },
            ["Bezier"] = new ToolDataInfo()
            {
                key = "Bezier",
                path = packagePath + "/Tool~/Bezier",
                active = true,
                url = "https://gitee.com/NikaidoShinku/YukiFrameWork/blob/master/YukiFrameWork/Tool~/Bezier/Bezier.md"
            },
            ["Entities"] = new ToolDataInfo()
            {
                key = "Entities",
                path = packagePath + "/Tool~/Entities",
                url = string.Empty,
                active = false,
            },
            ["SaveTool"] = new ToolDataInfo()
            {
                key = "SaveTool",
                path = packagePath + "/Tool~/SaveTool",
                active = true,
                url = "https://gitee.com/NikaidoShinku/YukiFrameWork/blob/master/YukiFrameWork/Tool~/SaveTool/%E5%AD%98%E6%A1%A3%E7%B3%BB%E7%BB%9F.md"
            },
            ["StateMachine"] = new ToolDataInfo()
            {
                key = "StateMachine",
                active = true,
                path = packagePath + "/Tool~/StateMachine",
                url = "https://gitee.com/NikaidoShinku/YukiFrameWork/blob/master/YukiFrameWork/Tool~/StateMachine/StateMachine.md"
            },          
            ["IOCContainer"] = new ToolDataInfo() 
            {
                key = "IOCContainer",
                path = packagePath + "/Tool~/IOCContainer",
                active = true,
                url = "https://gitee.com/NikaidoShinku/YukiFrameWork/blob/master/YukiFrameWork/Tool~/IOCContainer/1.LifeTimeScope.md"
            },
            ["DiaLogKit"] = new ToolDataInfo() 
            {
                key = "DiaLogKit",
                path = packagePath + "/Tool~/DiaLogKit",
                active = true,
                url = "https://gitee.com/NikaidoShinku/YukiFrameWork/blob/master/YukiFrameWork/Tool~/DiaLogKit/DiaLogKit.md"
            },
            ["BuffKit"] = new ToolDataInfo() 
            {
                active = true,
                key = "BuffKit",
                path = packagePath + "/Tool~/BuffKit",
                url = "https://gitee.com/NikaidoShinku/YukiFrameWork/blob/master/YukiFrameWork/Tool~/BuffKit/BuffKit.md"
            },
            ["SkillKit"] = new ToolDataInfo() 
            {
                key = "SkillKit",
                path = packagePath + "/Tool~/SkillKit",
                active = true,
                url = "https://gitee.com/NikaidoShinku/YukiFrameWork/blob/master/YukiFrameWork/Tool~/SkillKit/SkillKit.md",
            },
            ["UI"] = new ToolDataInfo() 
            {
                key = "UI",
                path = packagePath + "/Tool~/UI",
                active = true,
                url = "https://gitee.com/NikaidoShinku/YukiFrameWork/blob/master/YukiFrameWork/Tool~/UI/6.UI%E6%A8%A1%E5%9D%97.md"
            },
            ["Audio"] = new ToolDataInfo() 
            {
                active = true,
                key = "Audio",
                path = packagePath + "/Tool~/Audio",
                url = "https://gitee.com/NikaidoShinku/YukiFrameWork/blob/master/YukiFrameWork/Tool~/Audio/8.%E5%A3%B0%E9%9F%B3%E7%AE%A1%E7%90%86%E6%A8%A1%E5%9D%97.md"
            }, 
            ["ItemKit"] = new ToolDataInfo() 
            {
                key = "ItemKit",
                path = packagePath + "/Tool~/ItemKit",
                active = true,
                url = "https://gitee.com/NikaidoShinku/YukiFrameWork/blob/master/YukiFrameWork/Tool~/ItemKit/14.%E8%83%8C%E5%8C%85%E7%B3%BB%E7%BB%9F(%E9%80%9A%E7%94%A8).md"
            },
            ["NavMeshPlus"] = new ToolDataInfo() 
            {
                key = "NavMeshPlus",
                active = true,
                path = packagePath + "/Tool~/NavMeshPlus",
                url = "https://gitee.com/NikaidoShinku/YukiFrameWork/blob/master/YukiFrameWork/Tool~/NavMeshPlus/README.md"
            },
            ["MissionKit"] = new ToolDataInfo() 
            {
                key = "MissionKit",
                active = true,
                path = packagePath + "/Tool~/MissionKit",
                url = "https://gitee.com/NikaidoShinku/YukiFrameWork/blob/master/YukiFrameWork/Tool~/MissionKit/MissionKit.md"
            },
            ["BehaviourTree"] = new ToolDataInfo() 
            {
                key = "BehaviourTree",
                path = packagePath + "/Tool~/BehaviourTree",
                active = true,
                url = "https://gitee.com/NikaidoShinku/YukiFrameWork/blob/master/YukiFrameWork/Tool~/BehaviourTree/BehaviourTree.md"
            },
            ["StateManager"] = new ToolDataInfo()
            {
                key = "StateManager",
                active = true,
                path = packagePath + "/Tool~/StateManager",
                url = "https://gitee.com/NikaidoShinku/YukiFrameWork/blob/master/YukiFrameWork/Tool~/StateManager/StateManager.md"
            },
        };      


        public static string GetLocalPath(string key) => moduleInfo[key].path;

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
                    GUILayout.Label(select.Name + "    " ,titleStyle);
                    EditorGUILayout.Space(10);
                    GUILayout.Label(ImportWindowInfo.GetModuleInfo(select.Name),desStyle);
                    EditorGUILayout.Space(20);
                   
                    bool isImport = moduleInfo[select.Name].active;
                    if (!isImport)
                        EditorGUILayout.HelpBox("目前还尚未公开", MessageType.Warning);
                    EditorGUILayout.BeginHorizontal();
                    DrawBoxGUI(select.Name == "StateMechine" ? Color.yellow : Color.white, string.Format("{0}/{1}", data.path, select.Name), select.Name, moduleInfo[select.Name].path, isImport);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();

                    Button(moduleInfo[select.Name].url);
                 
                }
            }         

        }      
        void Button(string url, Color color = default)
        {
            if (color != default)
                GUI.color = color;
            if (GUILayout.Button("文档官网", GUILayout.Height(40), GUILayout.Width(250)))
            {
                Application.OpenURL(url);
            }
            GUI.color = Color.white;
            GUILayout.Space(5);
        }

        private void OpenUrl(string name,string url)
        {
            if (GUILayout.Button(name))
            {
                if (string.IsNullOrEmpty(url))
                    return;
                Application.OpenURL(url);
            }
        }   

        private void DrawBoxGUI(Color color,string path,string name,string copyPath = "",bool isImport = true)
        {         
            GUI.color = color;          
            EditorGUILayout.BeginHorizontal();           
            DrawButtonGUI(path, name,copyPath,isImport);          
            EditorGUILayout.EndHorizontal();
            GUI.color = Color.white;
        }

        private void DrawButtonGUI(string path,string name,string copyPath,bool isImport)
        {
                     
            EditorGUILayout.BeginHorizontal();
          
            if (Directory.Exists(path) && isImport)
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
                GUI.enabled = isImport;
               
                if (GUILayout.Button(ImportWindowInfo.IsEN ? $"Import {name} Module":$"导入{name}模块", GUILayout.Height(20)))
                {
                    Import(copyPath,name);                 
                }
                GUI.enabled = true;
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
    }
}
#endif