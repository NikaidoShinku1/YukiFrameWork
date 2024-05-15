///=====================================================
/// - FileName:      LogConfig.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/5/15 17:15:15
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using UnityEngine;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif
namespace YukiFrameWork
{
    public enum FolderType
    {
        [LabelText("Application.persistentDataPath")]
        persistentDataPath,
        [LabelText("Application.dataPath")]
        dataPath,
        [LabelText("Application.streamingAssetsPath")]
        streamingAssetsPath,
        [LabelText("Application.temporaryCachePath")]
        temporaryCachePath,
        [LabelText("自定义存档路径")]
        custom
    }
    public class LogConfig : ScriptableObject
	{
        internal const string LOGFULLCONDITION = "Yuki_DEBUGFULL";
        internal const string LOGINFOCONDITION = "Yuki_DEBUGINFO";
        internal const string LOGWARNINGCONDITION = "Yuki_DEBUGWARNING";
        internal const string LOGERRORCONDITION = "Yuki_DEBUGWARNING";

        [LabelText("是否完成日志初始化"),ReadOnly]
        public bool IsInitialization;

        /// <summary>
        /// 是否开启日志
        /// </summary>
        [SerializeField,InfoBox("是否开启日志")]
        internal bool LogEnabled  = true;

        [SerializeField, InfoBox("本日志系统输出都会带上该标记以示区分")]
        internal string prefix = ">>>>";
        /// <summary>
        /// 是否允许将日志写入文件，默认开启
        /// </summary>
        [SerializeField,InfoBox("是否允许将日志写入文件，默认开启")]
        internal bool LogSaving = true;   

        [LabelText("文件夹方式选择:"), SerializeField, BoxGroup("文件路径设置"),ShowIf(nameof(LogSaving))]
        public FolderType folderType = FolderType.persistentDataPath;
        [LabelText("保存的文件路径:"), BoxGroup("文件路径设置"),FolderPath(AbsolutePath = true), ShowIf(nameof(IsCustom))]
        public string saveFolder;
        private bool IsCustom => folderType == FolderType.custom && LogSaving;
        [SerializeField,LabelText("允许最大生成的文件数量"), BoxGroup("文件路径设置"), ShowIf(nameof(LogSaving))]
        internal int fileCount = 20;

        [ShowInInspector]
        public string saveDirPath
        {
            get
            {
                string folderName = "/Logs";
                switch (folderType)
                {
                    case FolderType.persistentDataPath:
                        return Application.persistentDataPath + folderName;
                    case FolderType.dataPath:
                        return Application.dataPath + folderName;
                    case FolderType.streamingAssetsPath:
                        return Application.streamingAssetsPath + folderName;
                    case FolderType.temporaryCachePath:
                        return Application.temporaryCachePath + folderName;
                    case FolderType.custom:
                        return saveFolder + folderName;
                }

                return default;
            }
        }
#if UNITY_EDITOR
        [OnInspectorGUI]
        void OnInspectorGUI()
        {
            GUILayout.Space(15);

            EditorGUILayout.HelpBox("下方是框架日志套件中，所有级别的程序集宏定义,可以手动在Edit/ProjectSetting中添加对应宏定义\n没有宏定义的情况下是日志是不会触发以及调用的，导入框架时默认注入" + LogKit.LOGFULLCONDITION + "定义", MessageType.Info);

            EditorGUILayout.TextField("All Level:", LogKit.LOGFULLCONDITION);
            EditorGUILayout.Space();

            EditorGUILayout.TextField("Info:",LogKit.LOGINFOCONDITION);
            EditorGUILayout.TextField("Warning:", LogKit.LOGWARNINGCONDITION);
            EditorGUILayout.TextField("Error", LogKit.LOGERRORCONDITION);
         
        }
#endif
    }
}
