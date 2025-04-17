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
           
        [LabelText("全局命名空间：")]
        public string nameSpace = "YukiFrameWork.Example";
       
        [HideInInspector]
        public bool IsParent;
        [InfoBox("项目(架构)脚本所依赖的程序集定义(非必要不更改):",InfoMessageType.Warning,IconColor = "red")]
        [LabelText("默认程序集：")]
        public string assembly = "Assembly-CSharp";
        [LabelText("程序集依赖项(有多个Assembly时可以使用)")]
        public string[] assemblies = new string[0];           
        [HideInInspector]
        public ScriptableObject excelConvertConfig;

        [HideInInspector]
        public int excelInstanceId;
        [HideInInspector]
        public string excelDataPath;
        [HideInInspector]
        public string excelTempPath;
#if UNITY_EDITOR
        private LocalScriptGenerator generator = new LocalScriptGenerator();
#endif
#if UNITY_EDITOR
        [Button("打开高级生成窗口", ButtonHeight = 35), PropertySpace]
        private void OpenExpertWindow()
        {
            ExpertCodeConfigWindow.OpenWindow();
        }
#endif
#if UNITY_EDITOR

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

            ExpertCodeConfig config = Resources.Load<ExpertCodeConfig>(nameof(ExpertCodeConfig));

            if (!config)
            {
                if (!System.IO.Directory.Exists("Assets/Resources"))
                {
                    System.IO.Directory.CreateDirectory("Assets/Resources");
                    AssetDatabase.Refresh();

                }
                config = YukiAssetDataBase.CreateScriptableAsset<ExpertCodeConfig>(nameof(ExpertCodeConfig), "Assets/Resources/" + nameof(ExpertCodeConfig) + ".asset");
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
