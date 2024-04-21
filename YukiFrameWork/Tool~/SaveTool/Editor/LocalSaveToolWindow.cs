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
		private OdinEditor editor;
		[MenuItem("YukiFrameWork/SaveTool",false,-2)]
		public static void OpenWindow()
		{
			var window = GetWindow<LocalSaveToolWindow>();
			window.Show();
			window.titleContent = new GUIContent("存档配置窗口");			
			
		}	
        protected override void OnEnable()
        {
            Update_Config();
			editor = OdinEditor.CreateEditor(config, typeof(OdinEditor)) as OdinEditor;
            base.OnEnable();	
        }    

        private void OnInspectorUpdate()
        {
            Repaint();
        }
        protected override void OnImGUI()
        {
			editor.DrawDefaultInspector();
            var folderType = config.folderType;
            switch (folderType)
            {
                case FolderType.persistentDataPath:
                    config.saveFolder = Application.persistentDataPath;
                    break;
                case FolderType.dataPath:
                    config.saveFolder = Application.dataPath;
                    break;
                case FolderType.streamingAssetsPath:
                    config.saveFolder = Application.streamingAssetsPath;
                    break;
                case FolderType.temporaryCachePath:
                    config.saveFolder = Application.temporaryCachePath;
                    break;
            }
            base.OnImGUI();          
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