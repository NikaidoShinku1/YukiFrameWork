﻿using UnityEngine;
using Sirenix.OdinInspector;
using System.IO;
using System.Text;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;



#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;

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

    [Serializable]
    public class AnimationConvertInfo
    {
        public string folderPath;
        public string showViewPath;
        public YDictionary<string,Info> info_Values = new YDictionary<string, Info>();
        [Serializable]
        public class Info
        {
            public bool loop;            
            public float cycleOffset;           
            public FrameInfo[] frameInfos = new FrameInfo[0];
            [Serializable]
            public class FrameInfo
            {
                public string guid;
                public float frame;
            }
        }
    }

    [Serializable]
    public class MultipleAnimationConvertInfo
    {
        [HideInInspector]
        public Texture2D texture;
        [HideInInspector]
        public List<Sprite> sprites = new List<Sprite>();
        [LabelText("AnimationClip预数据")]
        [InfoBox("作为图集的Texture2D有着许多不确定性，一个图集也许可以制作多个AnimationClip，可在此进行添加")]
        public MultipleInfo[] multipleInfos;
        [Serializable]
        public class MultipleInfo
        {
            [LabelText("AnimationClip名称")]
            public string clipName;
            [LabelText("帧信息")]
            [ListDrawerSettings(CustomAddFunction = nameof(Add))]
            public List<MultipleFrameInfo> multipleFrameInfos = new List<MultipleFrameInfo>();

            void Add()
            {
                var multipleFrameInfo = new MultipleFrameInfo();
                multipleFrameInfo.frame = multipleFrameInfos.Count;
                multipleFrameInfos.Add(multipleFrameInfo);
            }
            [Serializable]
            public class MultipleFrameInfo
            {
                [LabelText("当前帧")]
                public float frame;
                [LabelText("当前选择的Sprite")]
#if UNITY_EDITOR
                [ValueDropdown(nameof(GetAllSprites))]
#endif
                public Sprite sprite;

#if UNITY_EDITOR
                private IEnumerable GetAllSprites()
                    => Resources.Load<FrameworkConfigInfo>(nameof(FrameworkConfigInfo)).multipleAnimationConvertInfo.sprites;
#endif
            }
            public bool loop;
            public float cycleOffset;
        }
    }

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
        [HideInInspector,SerializeField]
        public AnimationConvertInfo AnimationConvertInfo;
        [SerializeField,ReadOnly]
        public MultipleAnimationConvertInfo multipleAnimationConvertInfo;
#if UNITY_EDITOR
        private LocalScriptGenerator generator = new LocalScriptGenerator();
        [ReadOnly]
        public string version;
#endif
#if UNITY_EDITOR
        [Button("打开高级生成窗口", ButtonHeight = 35), PropertySpace]
        private void OpenExpertWindow()
        {
            ExpertCodeConfigWindow.OpenWindow();
        }
#endif
#if UNITY_EDITOR
        static ImportSettingWindow.VersionData versionData;
       // [InitializeOnLoadMethod]
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
            EditorCoroutineTool.StartCoroutine(EditorDelay(1), () => 
            {
                TextAsset versionText = AssetDatabase.LoadAssetAtPath<TextAsset>(ImportSettingWindow.packagePath + "/package.json");
                if (versionData == null)
                    versionData = SerializationTool.DeserializedObject<ImportSettingWindow.VersionData>(versionText.text);
                if (info.version != versionData.version)
                {
                    info.version = versionData.version;
                    VersionInfoWindow.Open();
                    EditorUtility.SetDirty(info);
                    AssetDatabase.Refresh();
                }
            });
          
        }

        [DisableEnumeratorWarning]
        private static IEnumerator EditorDelay(float time)
        {
            double start = EditorApplication.timeSinceStartup;

            yield return new WaitUntil(() => EditorApplication.timeSinceStartup - start > time);

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
