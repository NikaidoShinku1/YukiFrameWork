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
namespace YukiFrameWork.Extension
{
    public class ImportSettingWindow : EditorWindow
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

        public const string importPath = "Packages/com.yuki.yukiframework/Plugins/ImportWindow/Data/ImportPath.json";
        private const string packagePath = "Packages/com.yuki.yukiframework";
        public class Data
        {
            public string path;
            public int develop = 0;
            public bool isEN;
        } 

        [MenuItem("YukiFrameWork/Import Window",false,-1000)]
        static void ShowWindow()
        {
            var window = GetWindow<ImportSettingWindow>("YukiFrameWork");
            window.Show();
        }      
        private void OnEnable()
        {
            LoadData();        
        }      
        private void OnDisable()
        {
            SaveData();
           
        }
        /// <summary>
        /// Key:Name  Value:Path
        /// </summary>
        private readonly Dictionary<string, string> moduleInfo = new Dictionary<string, string>()
        {
            ["ActionKit"] = packagePath + "/Tool~/ActionKit",
            ["Bezier"] = packagePath + "/Tool~/Bezier",
            ["StateMechine"] = packagePath + "/Tool~/StateMechine",
            ["IOCContainer"] = packagePath + "/Tool~/IOCContainer",
            ["ABManager"] = packagePath + "/Tool~/ABManager",
            ["UI"] = packagePath + "/Tool~/UI",
            ["Audio"] = packagePath + "/Tool~/Audio",
            ["Knapsack"] = packagePath + "/Tool~/Knapsack",
            ["DoTween"] = packagePath + "/Tool~/DoTween",
            ["UniRx"] = packagePath + "/Tool~/UniRx",
            ["UniTask"] = packagePath + "/Tool~/UniTask",
            ["Serialization"] = packagePath + "/Tool~/SerializationTool"
        };

        void LoadData()
        {
            TextAsset text = AssetDatabase.LoadAssetAtPath<TextAsset>(importPath);
            data = JsonUtility.FromJson<Data>(text.text);
            ImportWindowInfo.IsEN = data.isEN;

            TextAsset versionText = AssetDatabase.LoadAssetAtPath<TextAsset>(packagePath + "/package.json");

            versionData = AssemblyHelper.DeserializeObject<VersionData>(versionText.text); 
           
            
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
            EditorGUILayout.EndHorizontal();           
            EditorGUILayout.Space();
            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginVertical("GroupBox");
            scrollPosition = GUILayout.BeginScrollView(scrollPosition,false,true);

            DrawBoxGUI(Color.white, ImportWindowInfo.SerializationInfo
                , MessageType.Info, string.Format("{0}/Serialization", data.path), "Serialization", packagePath + "/Tool~/Serialization");

            DrawBoxGUI(Color.white, ImportWindowInfo.ActionKitInfo
                , MessageType.Info, string.Format("{0}/ActionKit", data.path), "ActionKit", packagePath + "/Tool~/ActionKit");

            DrawBoxGUI(Color.white, ImportWindowInfo.BezierInfo
            , MessageType.Info, string.Format("{0}/Bezier", data.path), "Bezier", packagePath + "/Tool~/Bezier");

            DrawBoxGUI(Color.white, ImportWindowInfo.StateMechineInfo
           , MessageType.Info, string.Format("{0}/StateMechine", data.path), "StateMechine", packagePath + "/Tool~/StateMechine");

            DrawBoxGUI(Color.white, ImportWindowInfo.IOCInfo
            , MessageType.Info, string.Format("{0}/IOCContainer", data.path), "IOCContainer", packagePath + "/Tool~/IOCContainer");

            DrawBoxGUI(Color.white, ImportWindowInfo.ABManagerInfo
          , MessageType.Info, string.Format("{0}/ABManager", data.path), "ABManager", packagePath + "/Tool~/ABManager");

            DrawBoxGUI(Color.white, ImportWindowInfo.UIInfo
           , MessageType.Info, string.Format("{0}/UI",data.path), "UI", packagePath + "/Tool~/UI");

            DrawBoxGUI(Color.white, ImportWindowInfo.AudioInfo
           , MessageType.Info, string.Format("{0}/Audio", data.path), "Audio", packagePath + "/Tool~/Audio");

            DrawBoxGUI(Color.white, ImportWindowInfo.KnapsackInfo
            , MessageType.Info, string.Format("{0}/Knapsack", data.path), "Knapsack", packagePath + "/Tool~/Knapsack");

            DrawBoxGUI(Color.white, ImportWindowInfo.DoTweenInfo
                , MessageType.Info, string.Format("{0}/DoTween", data.path), "DoTween", packagePath + "/Tool~/DoTween");

            DrawBoxGUI(Color.white, ImportWindowInfo.UniRxInfo
                , MessageType.Info, string.Format("{0}/UniRx",  data.path), "UniRx", packagePath + "/Tool~/UniRx");

            DrawBoxGUI(Color.white, ImportWindowInfo.UniTaskInfo
                , MessageType.Info, string.Format("{0}/UniTask", data.path), "UniTask", packagePath + "/Tool~/UniTask");

            EditorGUILayout.HelpBox(ImportWindowInfo.ImportAllModuleInfo, MessageType.Warning);
            EditorGUILayout.BeginHorizontal();
            GUI.color = Color.white;
            if (GUILayout.Button(ImportWindowInfo.ImportAllModuleInfo, GUILayout.Height(20)))
            {
                Import(packagePath + "/Tool~/Serialization", "Serialization");
                Import(packagePath + "/Tool~/ActionKit", "ActionKit");
                Import(packagePath + "/Tool~/Bezier", "Bezier");
                Import(packagePath + "/Tool~/StateMechine", "StateMechine");
                Import(packagePath + "/Tool~/IOCContainer", "IOCContainer");
                Import(packagePath + "/Tool~/ABManager", "ABManager");
                Import(packagePath + "/Tool~/UI", "UI");
                Import(packagePath + "/Tool~/Audio", "Audio");
                Import(packagePath + "/Tool~/Knapsack", "Knapsack");
                Import(packagePath + "/Tool~/DoTween", "DoTween");
                Import(packagePath + "/Tool~/UniRx", "UniRx");
                Import(packagePath + "/Tool~/UniTask", "UniTask");
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.HelpBox(ImportWindowInfo.ReImportAllModuleInfo, MessageType.Warning);
            EditorGUILayout.BeginHorizontal();
            GUI.color = Color.white;
            if (GUILayout.Button(ImportWindowInfo.ReImportAllModuleInfo, GUILayout.Height(20)))
            {
                foreach (var key in moduleInfo.Keys)
                {
                    string path = string.Format("{0}/YukiFrameWork/", Application.dataPath);
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
                    , MessageType.Warning, ImportWindowInfo.IsEN ? "Export the imported module" : "反导已经导入的模块");
            }

            GUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
            if (EditorGUI.EndChangeCheck())
                SaveData();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            OpenUrl("Gitee", versionData.author.url);
            OpenUrl("作者邮箱:"+versionData.author.email, string.Empty);
            OpenUrl("框架版本:" + versionData.version,string.Empty);
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
            DrawButtonGUI(path, name,copyPath);
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
                if (GUILayout.Button(ImportWindowInfo.IsEN ? "Delete Module" : "删除模块", GUILayout.Height(20),data.develop != 1 ? GUILayout.Width(position.width / 2) : GUILayout.Width(position.width / 3)))
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
                Debug.LogError(ImportWindowInfo.IsEN ? "Path lost! Check the Package root directory and be sure to manually add the framework package.json!" : "路径丢失！请检查包根目录以及确保在PackageManager手动添加框架Package.json!");
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