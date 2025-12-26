///=====================================================
/// - FileName:      DynamicViewController.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   高级定制脚本生成
/// - Creation Time: 12/26/2025 2:56:55 PM
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.IO;
namespace YukiFrameWork
{
    public abstract class DynamicViewController : ViewController
    {       
        protected override void Awake()
        {
            DynamicValue.Inject(this);
            base.Awake();
        }



#if UNITY_EDITOR
        
        
        public override bool OnInspectorGUI()
        {
            if (GUILayout.Button("打开示例场景", GUILayout.Height(50)))
            {
                string[] guids = AssetDatabase.FindAssets("t:Scene");
                for (int i = 0; i < guids.Length; i++)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[i]);

                    string sceneName = Path.GetFileNameWithoutExtension(path);

                    string[] dis = path.Split('/');                    
                    if (sceneName == "DynamicViewControllerExampleScene")
                    {
                        UnityEditor.SceneManagement.EditorSceneManager.OpenScene(path);
                    }
                }
                
            }

            return true;
        }
#endif
    }
}
