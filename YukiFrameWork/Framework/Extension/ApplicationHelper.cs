///=====================================================
/// - FileName:      ApplicationHelper.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/4/7 19:26:08
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;


#if UNITY_EDITOR
using UnityEngine.SceneManagement;
using UnityEditor;
#endif
namespace YukiFrameWork.Extension
{
	public static class ApplicationHelper
	{
        public static GUIContent[] ToContents(this IEnumerable<string> enumerable)
        {
            return enumerable.Select(x => new GUIContent(x)).ToArray();
        }
		public static bool GetRuntimeOrEditor() => Application.isPlaying;

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
        static void Init()
        {
            EditorApplication.hierarchyWindowItemOnGUI = (int id, Rect rect) =>
            {                
                OnDefaultHierarchyGUI(id, rect);              
            };
        }

        private static void OnDefaultHierarchyGUI(int id, Rect rect)
        {
            //默认场景视图下的层级

            try
            {
                if (!FrameworkConfigInfo.IsShowHerarchy) return;
             
                GameObject current = (GameObject)EditorUtility.InstanceIDToObject(id);             
                if (current == null)
                {                    
                    return;
                }
        
                float tagWidth = 100;
                float layerWidth = 120;
                float activeWidth = 20;
                float buttonWidth = 60;
                EditorGUI.BeginChangeCheck();

                ISerializedFieldInfo info = current.GetComponentInParent<ISerializedFieldInfo>();

                if (info == null)
                {

                }
                else
                {                
                    Rect btnRect = new Rect(rect);
                    btnRect.x = (btnRect.xMax - layerWidth - tagWidth - activeWidth - 10 - buttonWidth);
                    btnRect.width = buttonWidth;

                    if (GUI.Button(btnRect, "Bind"))
                    {
                        string[] names = current.GetComponents<Component>().Select(x => x.GetType().FullName).ToArray();

                        List<string> targets = new List<string>();
                        targets.Add(typeof(GameObject).FullName);
                        targets.AddRange(names);
                        var bind = current.GetOrAddComponent<YukiBind>();
                        EditorUtility.DisplayCustomMenu(btnRect, targets.ToContents(), -1, (data, contents, selectIndex) =>
                        {
                            Selection.activeObject = current;
                            EditorApplication.delayCall += () =>
                            {
                                bind._fields.fieldLevelIndex = 0;
                                bind._fields.fieldTypeIndex = bind._fields.Components.IndexOf(contents[selectIndex]);
                            };
                        }, null);

                    }

                }

                if (!current.CompareTag("Untagged"))
                {
                    Rect tagRect = new Rect(rect);

                    tagRect.x = (tagRect.xMax - layerWidth -  tagWidth - activeWidth);
                    tagRect.width = tagWidth;
                    GUI.Label(tagRect, current.tag);
                }              
                if (current.layer != 0)
                {
                    Rect layerRect = new Rect(rect);

                    layerRect.x = (layerRect.xMax - layerWidth - activeWidth);
                    layerRect.width = layerWidth;
                    GUI.color = Color.cyan;                  
                    GUI.Label(layerRect, string.Format("[{0}]", LayerMask.LayerToName(current.layer)));

                    GUI.color = Color.white;

                }
             
              
                Rect activeRect = new Rect(rect);
                activeRect.x = (activeRect.xMax - activeWidth);
                activeRect.width = 18;
                current.SetActive(EditorGUI.Toggle(activeRect, current.activeSelf));
                if (EditorGUI.EndChangeCheck())
                {
                    EditorUtility.SetDirty(current);
                    AssetDatabase.SaveAssets();
                }
                
            }
            catch { }
        }


        private static void OnPrefabHierarchyGUI(int id, Rect rect,GameObject prefab)
        {
            try
            {
                ISerializedFieldInfo info = prefab.GetComponent<ISerializedFieldInfo>();

                if (info == null)
                {
                    OnDefaultHierarchyGUI(id, rect);
                    return;
                }
                GameObject current = (GameObject)EditorUtility.InstanceIDToObject(id);
                if (current == null) return;

                var data = info.Find(x => x.target == current);

                if (data == null)
                {
                    YukiBind bind = current.GetComponent<YukiBind>();
                    if(bind)
                    data = bind._fields;
                }              

                if (data == null) return;
                EditorGUI.BeginChangeCheck();
                string level = data.fieldLevel[data.fieldLevelIndex];
                string type = data.Components[data.fieldTypeIndex];
                
                var bindRect = new Rect(rect);
                bindRect.x = rect.x + rect.width - 350;

                GUI.color = Color.green;
                GUI.Label(bindRect, "Bind");
                GUI.color = Color.white;
                GUI.Label(new Rect(rect) { x = rect.x + rect.width - 300}, string.Format("{0} {1}", level, type));

                Rect activeRect = new Rect(rect);
                activeRect.x = activeRect.xMax - 20;
                activeRect.width = 18;
                current.SetActive(EditorGUI.Toggle(activeRect, current.activeSelf));
                if (EditorGUI.EndChangeCheck())
                {
                    EditorUtility.SetDirty(current);
                    AssetDatabase.SaveAssets();
                }
            }
            catch {  }
            //预制体预览修改视图下的层级
        }
#endif
    }
}
