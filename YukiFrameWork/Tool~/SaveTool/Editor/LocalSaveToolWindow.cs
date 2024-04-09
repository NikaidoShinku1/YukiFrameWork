///=====================================================
/// - FileName:      LocalSaveToolWindow.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/4/1 22:54:06
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using UnityEditor;
namespace YukiFrameWork
{   
    public class LocalSaveToolWindow : OdinEditorWindow
	{
		private SaveToolConfig config;
		[MenuItem("YukiFrameWork/SaveTool",false,-2)]
		public static void OpenWindow()
		{
			var window = GetWindow<LocalSaveToolWindow>();
			window.Show();
			window.titleContent = new GUIContent("存档配置窗口");			
			
		}	

		[BoxGroup,LabelText("文件夹名称:")]
		public string saveFolderName;
        [BoxGroup, LabelText("文件夹方式选择:")]
        public FolderType folderType;
		private bool IsCustom => folderType == FolderType.custom;
        [BoxGroup("文件路径设置"), LabelText("保存的文件路径:"), ShowIf(nameof(IsCustom))]
        public string saveFolder;   

        [BoxGroup, LabelText("当前所有的存档信息:")]
        public List<SaveInfo> infos = new List<SaveInfo>();
        protected override void OnEnable()
        {
			OnAfterDeserialize();
        }

        protected override void OnAfterDeserialize()
        {
            Update_Config();           
            saveFolderName = config.saveFolderName;
			saveFolder = config.saveFolder;
			infos = config.infos;
			folderType = config.folderType;
        }

        protected override void OnBeforeSerialize()
        {
            Update_Config();
            switch (folderType)
            {
                case FolderType.persistentDataPath:
                    saveFolder = Application.persistentDataPath;
                    break;
                case FolderType.dataPath:
                    saveFolder = Application.dataPath;
                    break;
                case FolderType.streamingAssetsPath:
                    saveFolder = Application.streamingAssetsPath;
                    break;
                case FolderType.temporaryCachePath:
                    saveFolder = Application.temporaryCachePath;
                    break;
            }
            config.saveFolder = saveFolder;
			config.infos = infos;
			config.folderType = folderType;			
			config.saveFolderName = saveFolderName;
        }
        private void OnInspectorUpdate()
        {
            Repaint();
        }
        protected override void OnImGUI()
        {
            base.OnImGUI();          
        }
		[Button("定位到指定文件夹"), BoxGroup("文件路径设置"),ShowIf(nameof(IsCustom))]
        private void CheckMouseToPosition()
		{
           saveFolder = EditorUtility.OpenFolderPanel("定位到指定文件夹", string.Empty, string.Empty);
        }

        private void Update_Config()
        {
			if (config == null)
			{
				config = Resources.Load<SaveToolConfig>(nameof(SaveToolConfig));
				if (config == null)
				{
					CreateSaveConfig();
					config = Resources.Load<SaveToolConfig>(nameof(SaveToolConfig));
				}
            }				
        }

      
        [InitializeOnLoadMethod]
		static void CreateSaveConfig()
		{
			var config = Resources.Load<SaveToolConfig>(nameof(SaveToolConfig));

			if (config == null)
			{
				config = ScriptableObject.CreateInstance<SaveToolConfig>();

				string path = "Assets/Resources";
				if (!Directory.Exists(path))
				{
					Directory.CreateDirectory(path);
					AssetDatabase.Refresh();
				}

				AssetDatabase.CreateAsset(config, path + "/" + nameof(SaveToolConfig) + ".asset");
				AssetDatabase.Refresh();
				EditorUtility.SetDirty(config);
				AssetDatabase.SaveAssets();
			}


		}
	}
}
#endif