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
namespace YukiFrameWork.Extension
{
    public class ImportSettingWindow
    {
        private Vector2 scrollPosition;
        private Data data;
        private VersionData versionData;
        public class Info
        {
            public string email;
            public string url;
        }

        public class VersionData
        {
            public string version;
            public Info author;
        }
        private OdinMenuEditorWindow window;
        public ImportSettingWindow(OdinMenuEditorWindow menuEditorWindow)
        {
            this.window = menuEditorWindow;
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

        /* [MenuItem("YukiFrameWork/Import Window",false,-1000)]
         static void ShowWindow()
         {
             var window = GetWindow<ImportSettingWindow>("YukiFrameWork");
             window.Show();
         }      */
        [OnInspectorInit]
        private void OnEnable()
        {
            LoadData();        
        }
        [OnInspectorDispose]
        private void OnDisable()
        {
            SaveData();
           
        }
        /// <summary>
        /// Key:Name  Value:Path
        /// </summary>
        private static readonly Dictionary<string, string> moduleInfo = new Dictionary<string, string>()
        {
            ["ActionKit"] = packagePath + "/Tool~/ActionKit",
            ["Bezier"] = packagePath + "/Tool~/Bezier",
            ["SaveTool"] = packagePath + "/Tool~/SaveTool",
            ["StateMechine"] = packagePath + "/Tool~/StateMechine",
            ["StateManager"] = packagePath + "/Tool~/StateManager",
            ["IOCContainer"] = packagePath + "/Tool~/IOCContainer",
            ["DiaLogKit"] = packagePath + "/Tool~/DiaLogKit",
            ["BuffKit"] = packagePath + "/Tool~/BuffKit",
            ["SkillKit"] = packagePath + "/Tool~/SkillKit",
            ["UI"] = packagePath + "/Tool~/UI",
            ["Audio"] = packagePath + "/Tool~/Audio",
            ["ItemKit"] = packagePath + "/Tool~/ItemKit",
            ["PilotKit"] = packagePath + "/Tool~/PilotKit",
            ["NavMeshPlus"] = packagePath + "/Tool~/NavMeshPlus"
        };    

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
        [OnInspectorGUI]
        private void OnGUI()
        {                  
            EditorGUILayout.Space();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginVertical("FrameBox");
            EditorGUILayout.BeginHorizontal();
            data.develop = EditorGUILayout.Popup(ImportWindowInfo.DeveloperModeInfo, data.develop, ImportWindowInfo.displayedOptions);
            EditorGUILayout.BeginHorizontal(GUILayout.Width(50));
            GUILayout.Label("EN");
            ImportWindowInfo.IsEN = EditorGUILayout.Toggle(ImportWindowInfo.IsEN);
            data.isEN = ImportWindowInfo.IsEN;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(ImportWindowInfo.ImportPath, data.path);
#if UNITY_2021_1_OR_NEWER
            if (GUILayout.Button(ImportWindowInfo.SelectPath, GUILayout.Width(80)))
            {
                var importPath = EditorUtility.OpenFolderPanel("选择导入路径", "", "");
                ///相对于Assets路径
                string[] paths = importPath.Split('/');
                string completedPath = string.Empty;
                bool add = false;
                for (int i = 0; i < paths.Length; i++)
                {
                    if (paths[i].Equals("Assets")) 
                        add = true;
                    if (add)
                    {
                        if (i == paths.Length - 1)
                            completedPath += paths[i];
                        else
                            completedPath += paths[i] + "/";
                    }
                }
                if (!add)
                {
                    Debug.LogError("选择路径不在Assets文件夹下,请重新选择 Path finding failure!");
                    GUIUtility.ExitGUI();
                    return;
                }
                data.path = completedPath;
                SaveData();
            }
#endif
            EditorGUILayout.EndHorizontal();           
            EditorGUILayout.Space();
            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginVertical("GroupBox");
            scrollPosition = GUILayout.BeginScrollView(scrollPosition,false,true);
           
            DrawBoxGUI(Color.white, ImportWindowInfo.ActionKitInfo
                , MessageType.Info, string.Format("{0}/ActionKit", data.path), "ActionKit", moduleInfo["ActionKit"]);

            DrawBoxGUI(Color.white, ImportWindowInfo.BezierInfo
            , MessageType.Info, string.Format("{0}/Bezier", data.path), "Bezier", moduleInfo["Bezier"]);
            
            DrawBoxGUI(Color.white, ImportWindowInfo.SaveToolInfo
            , MessageType.Info, string.Format("{0}/SaveTool", data.path), "SaveTool", moduleInfo["SaveTool"]);

            DrawBoxGUI(Color.white, ImportWindowInfo.StateManagerInfo
            ,MessageType.Info, string.Format("{0}/StateManager",data.path), "StateManager", moduleInfo["StateManager"]);

            DrawBoxGUI(Color.white, ImportWindowInfo.NavMeshPlusInfo
                , MessageType.Info, string.Format("{0}/NavMeshPlus", data.path), "NavMeshPlus", moduleInfo["NavMeshPlus"]);

            DrawBoxGUI(Color.white, ImportWindowInfo.GuideInfo
                , MessageType.Info, string.Format("{0}/PilotKit", data.path), "PilotKit", moduleInfo["PilotKit"]);

            DrawBoxGUI(Color.white, ImportWindowInfo.DiaLogInfo
            , MessageType.Info, string.Format("{0}/DiaLogKit", data.path), "DiaLogKit", moduleInfo["DiaLogKit"]); 

            DrawBoxGUI(Color.white, ImportWindowInfo.BuffKitInfo
                , MessageType.Info, string.Format("{0}/BuffKit", data.path), "BuffKit", moduleInfo["BuffKit"]);

            DrawBoxGUI(Color.white, ImportWindowInfo.SkillInfo
               , MessageType.Info, string.Format("{0}/SkillKit", data.path), "SkillKit", moduleInfo["SkillKit"]);

            DrawBoxGUI(Color.white, ImportWindowInfo.IOCInfo
            , MessageType.Info, string.Format("{0}/IOCContainer", data.path), "IOCContainer", moduleInfo["IOCContainer"]);

            DrawBoxGUI(Color.white, ImportWindowInfo.UIInfo
           , MessageType.Info, string.Format("{0}/UI",data.path), "UI", moduleInfo["UI"]);

            DrawBoxGUI(Color.white, ImportWindowInfo.AudioInfo
           , MessageType.Info, string.Format("{0}/Audio", data.path), "Audio", moduleInfo["Audio"]);
            
            DrawBoxGUI(Color.white, ImportWindowInfo.KnapsackInfo
            , MessageType.Info, string.Format("{0}/ItemKit", data.path), "ItemKit", moduleInfo["ItemKit"]);

            DrawBoxGUI(Color.yellow, ImportWindowInfo.StateMechineInfo
          , MessageType.Warning, string.Format("{0}/StateMechine", data.path), "StateMechine", moduleInfo["StateMechine"]);

            EditorGUILayout.HelpBox(ImportWindowInfo.ImportAllModuleInfo, MessageType.Warning);
            EditorGUILayout.BeginHorizontal();
            GUI.color = Color.white;
            if (GUILayout.Button(ImportWindowInfo.ImportAllModuleInfo, GUILayout.Height(20)))
            {
                foreach (var key in moduleInfo.Keys)
                {
                    Import(moduleInfo[key], key);
                }              
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.HelpBox(ImportWindowInfo.ReImportAllModuleInfo, MessageType.Warning);
            EditorGUILayout.BeginHorizontal();
            GUI.color = Color.white;
            if (GUILayout.Button(ImportWindowInfo.ReImportAllModuleInfo, GUILayout.Height(20)))
            {
                foreach (var key in moduleInfo.Keys)
                {
                    string path = string.Format("{0}/", data.path);                 
                    if (Directory.Exists(path + key)) 
                    {
                        Directory.Delete(path + key,true);
                        Import(moduleInfo[key],key);
                    }                 
                }
            }
            EditorGUILayout.EndHorizontal();

            if (data.develop == 1)
            {
                DrawReverBoxGUI(Color.white, ImportWindowInfo.IsEN ? "Export the imported module" : "把已经导入的模块导回仓库"
                    , MessageType.Warning, ImportWindowInfo.IsEN ? "Export the imported module" : "反导已经导入的");
            }

            GUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
            if (EditorGUI.EndChangeCheck())
                SaveData();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            if (versionData != null)
            {
                OpenUrl("Gitee", versionData.author.url);
                OpenUrl("作者邮箱:" + versionData.author.email, string.Empty);
                OpenUrl("框架版本:" + versionData.version, string.Empty);
            }
            EditorGUILayout.EndHorizontal();
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

        private void DrawReverBoxGUI(Color color,string message,MessageType Info, string name)
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
        }

        private void DrawBoxGUI(Color color,string message,MessageType ImportWindowInfo,string path,string name,string copyPath = "")
        {         
            GUI.color = color;
            EditorGUILayout.HelpBox(message, ImportWindowInfo);
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
                if (GUILayout.Button(ImportWindowInfo.IsEN ? "Delete Module" : "删除模块", GUILayout.Height(20),data.develop != 1 ? GUILayout.Width(window.position.width / 2.5f) : GUILayout.Width(window.position.width / 4f)))
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

        private void Import(string copyPath,string name)
        {
            if (!Directory.Exists(copyPath))
            {
                Debug.LogError(ImportWindowInfo.IsEN ? string.Format("Path lost! Check the Package root directory and be sure to manually add the framework package.json! path:{0}",copyPath) : string.Format("路径丢失！请检查包根目录以及确保在PackageManager手动添加框架Package.json! 路径:{0}",copyPath));
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
                    var newFile = file.Replace(copyPath,"");                   
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