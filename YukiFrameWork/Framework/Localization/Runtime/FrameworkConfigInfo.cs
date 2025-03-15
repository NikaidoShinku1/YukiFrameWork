using UnityEngine;
using Sirenix.OdinInspector;
using System.IO;
using System.Text;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using System;
using UnityEngine.Tilemaps;




#if UNITY_EDITOR
using UnityEditor;
#endif
using YukiFrameWork.Extension;
namespace YukiFrameWork
{
#if UNITY_EDITOR
    public class LocalScriptGenerator : ICodeGenerator
    {
        public StringBuilder BuildFile(params object[] arg)
        {
            string scriptName = (string)arg[0];
            string nameSpace = (string)arg[1];
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("///=====================================================");
            builder.AppendLine("/// - FileName:      " + scriptName + ".cs");
            builder.AppendLine("/// - NameSpace:     " + nameSpace);
            builder.AppendLine("/// - Description:   通过本地的代码生成器创建的脚本");
            builder.AppendLine("/// - Creation Time: " + System.DateTime.Now.ToString());
            builder.AppendLine("/// -  (C) Copyright 2008 - 2025");
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
            return builder;
        }
    }
#endif
    [HideMonoScript]
    public class FrameworkConfigInfo : ScriptableObject
    {
        public static FrameworkConfigInfo GetFrameworkConfig()
            => Resources.Load<FrameworkConfigInfo>(nameof(FrameworkConfigInfo));
        enum Mode
        {
            [LabelText("脚本生成设置")]
            Tool,
            [LabelText("本地化配置")]
            Script,          
            [LabelText("程序集设置")]
            Assembly,          

        }
        internal enum ReLoadProject
        {
            [LabelText("重启游戏")]
            Game,
            [LabelText("重新进入当前场景")]
            Scene,
        }
        [LabelText("是否开启视图快捷显示"),SerializeField]
        internal bool IsShowHerarchy = true;
        [LabelText("是否开启编译重载"),SerializeField]
        [InfoBox("开启后可在代码编译后执行运行时的指定操作。")]
        internal bool IsShowReLoadProject = false;

        [LabelText("选择模式:"),SerializeField,ShowIf(nameof(IsShowReLoadProject))]
        internal ReLoadProject project;
        [SerializeField, EnumToggleButtons]
        Mode mode;
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
        public ScriptableObject excelConvertConfig;

        [HideInInspector]
        public int excelInstanceId;
        [HideInInspector]
        public string excelDataPath;
        [HideInInspector]
        public string excelTempPath;

        [HideInInspector]
        public int excelHeader;
      
        public int SelectIndex => (int)mode;
#if UNITY_EDITOR
        private LocalScriptGenerator generator = new LocalScriptGenerator();       
#endif
#if UNITY_EDITOR
        [Button("生成脚本",ButtonHeight = 35),PropertySpace, ShowIf(nameof(SelectIndex), 0)]
        private void Generic()
        {
            if (string.IsNullOrEmpty(genericPath))
            {
                Debug.LogError((FrameWorkConfigData.IsEN ? "Cannot create script because path is empty!" : "路径为空无法创建脚本!"));
                return;
            }
#if UNITY_EDITOR
            StringBuilder builder = generator.BuildFile(scriptName,nameSpace);

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

        [Button("打开高级生成窗口", ButtonHeight = 35), PropertySpace, ShowIf(nameof(SelectIndex), 0)]
        private void OpenExpertWindow()
        {
            ExpertCodeConfigWindow.OpenWindow();
        }
#endif
#endif
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
        internal static void CreateConfig()
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

        [UnityEditor.Callbacks.DidReloadScripts(0)]
        static async void ReLoad()
        {
            FrameworkConfigInfo info = Resources.Load<FrameworkConfigInfo>(nameof(FrameworkConfigInfo));
            if (!info) return;
            if (!info.IsShowReLoadProject) return;
            if (!EditorApplication.isPlaying) return;
            switch (info.project)
            {
                case ReLoadProject.Game:
                    EditorApplication.isPlaying = false;
                    while (Time.frameCount > 1)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(0.1f));

                        if (Time.frameCount <= 1)
                        {
                            EditorApplication.isPlaying = true;
                            break;
                        }
                    }
                  
                    break;
                case ReLoadProject.Scene:
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                    break;
                default:
                    break;
            }
        }
#endif
    }
}
