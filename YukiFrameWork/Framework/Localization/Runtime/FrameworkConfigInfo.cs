using UnityEngine;
using Sirenix.OdinInspector;
using System.IO;
using System.Text;

#if UNITY_EDITOR
using UnityEditor;
#endif
using YukiFrameWork.Extension;
namespace YukiFrameWork
{
    [HideMonoScript]
    public class FrameworkConfigInfo : ScriptableObject
    {
        [ShowIf(nameof(SelectIndex), 0), LabelText("脚本名称：")]
        public string scriptName;          
        [ShowIf(nameof(SelectIndex), 0), LabelText("命名空间：")]
        public string nameSpace = "YukiFrameWork.Example";
        [ShowIf(nameof(SelectIndex), 0), LabelText("生成路径："),FolderPath(AbsolutePath = true)]
        public string genericPath = "Assets/Scripts";
        [HideInInspector]
        public bool IsParent;
        [InfoBox("项目(架构)脚本所依赖的程序集定义(非必要不更改):",InfoMessageType.Warning,IconColor = "red")]
        [ShowIf(nameof(SelectIndex), 2),LabelText("默认程序集：")]
        public string assembly = "Assembly-CSharp";
        [ShowIf(nameof(SelectIndex),2),LabelText("程序集依赖项(有多个Assembly时可以使用)")]
        public string[] assemblies = new string[0];      
        [LabelText("运行时的默认语言:"), PropertySpace(6), ShowIf(nameof(SelectIndex), 1)]
        public Language defaultLanguage;
        [LabelText("本地配置"),PropertySpace, ShowIf(nameof(SelectIndex), 1)]
        public YDictionary<string, LocalizationConfigBase> dependConfigs = new YDictionary<string, LocalizationConfigBase>();

        [HideInInspector]
        public int SelectIndex = 0;

        public FrameworkConfigInfo GetFramework(int index)
        {
            SelectIndex = index;
            return this;
        }
        [Button("生成脚本",ButtonHeight = 35),PropertySpace, ShowIf(nameof(SelectIndex), 0)]
        private void Generic()
        {
            if (string.IsNullOrEmpty(genericPath))
            {
                Debug.LogError((FrameWorkConfigData.IsEN ? "Cannot create script because path is empty!" : "路径为空无法创建脚本!"));
                return;
            }
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("///=====================================================");
            builder.AppendLine("/// - FileName:      " + scriptName + ".cs");
            builder.AppendLine("/// - NameSpace:     " + nameSpace);
            builder.AppendLine("/// - Description:   通过本地的代码生成器创建的脚本");
            builder.AppendLine("/// - Creation Time: " + System.DateTime.Now.ToString());
            builder.AppendLine("/// -  (C) Copyright 2008 - 2024");
            builder.AppendLine("/// -  All Rights Reserved.");
            builder.AppendLine("///=====================================================");

            builder.AppendLine("using YukiFrameWork;");
            builder.AppendLine("using UnityEngine;");
            builder.AppendLine("using System;");
            builder.AppendLine($"namespace {nameSpace}");
            builder.AppendLine("{");
            builder.AppendLine($"\tpublic class {scriptName}");
            builder.AppendLine("\t{");
            builder.AppendLine("");
            builder.AppendLine("\t}");

            builder.AppendLine("}");

            if (!Directory.Exists(genericPath))
            {
                Directory.CreateDirectory(genericPath);
#if UNITY_EDITOR
                AssetDatabase.Refresh();
#endif
            }
            string scriptFilePath = genericPath + @"/" + scriptName + ".cs";
            if (File.Exists(scriptFilePath))
            {
                Debug.LogError((FrameWorkConfigData.IsEN ? $"Scripts already exist in this folder! Path:{scriptFilePath}" : $"脚本已经存在该文件夹! Path:{scriptFilePath}"));
                return;
            }

            using (FileStream fileStream = new FileStream(scriptFilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                StreamWriter streamWriter = new StreamWriter(fileStream, Encoding.UTF8);

                streamWriter.Write(builder);

                streamWriter.Close();

                fileStream.Close();
            }

#if UNITY_EDITOR
            this.Save();
            AssetDatabase.Refresh();
#endif

        }
#if UNITY_EDITOR
        [Button("为配置生成代码"), ShowIf(nameof(SelectIndex), 1)]
        [InfoBox("生成的代码为配置的标识,类名为Localization,请注意:配置标识跟配置内对应文本的标识不能出现一致的情况，否则会出问题")]
        void GenericCode(string codePath = "Assets/Scripts/YukiFrameWork/Code")
        {         
            if (dependConfigs.Count == 0)
            {
                Debug.LogWarning("没有添加配置无法生成代码");
                return;
            }
            CodeCore codeCore = new CodeCore();
            CodeWriter codeWriter = new CodeWriter();
            foreach (var value in dependConfigs)
            {
                string configKey = value.Key;
                codeWriter.CustomCode($"public static string ConfigKey_{configKey} = \"{configKey}\";");
                LocalizationConfigBase localizationConfig = value.Value;
                foreach (var key in localizationConfig.ConfigKeys)
                {
                    codeWriter.CustomCode($"public static string Key_{key} = \"{key}\";");
                }
            }
            codeCore.Descripton("LocalizationKit", nameSpace, "这是本地化套件生成的用于快速调用标识的类，用于标记所有的标识", System.DateTime.Now.ToString());
            codeCore.Using("UnityEngine")
            .Using("System")
            .Using(nameSpace)
            .EmptyLine()
            .CodeSetting(nameSpace, "Localization", string.Empty, codeWriter, false, false, false)
            .builder.CreateFileStream(codePath, "Localization", ".cs");

        }

        [InitializeOnLoadMethod]
        static void CreateConfig()
        {
            FrameworkConfigInfo info = Resources.Load<FrameworkConfigInfo>(nameof(FrameworkConfigInfo));

            if (info == null)
            {
                info = ScriptableObject.CreateInstance<FrameworkConfigInfo>();
                if (!Directory.Exists("Assets/Resources"))
                {
                    Directory.CreateDirectory("Assets/Resources");
                    AssetDatabase.Refresh();
                }
                AssetDatabase.CreateAsset(info, "Assets/Resources/FrameworkConfigInfo.asset");
                AssetDatabase.Refresh();
            }
        }
#endif
    }

}