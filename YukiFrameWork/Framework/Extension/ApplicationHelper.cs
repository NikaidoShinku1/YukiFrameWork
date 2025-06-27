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
        public static void Quit()
        {           
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;

#else
             Application.Quit();
#endif
        }
        private static FrameworkConfigInfo config;
#if UNITY_EDITOR
        [InitializeOnLoadMethod]
        static void Init()
        {
            config = Resources.Load<FrameworkConfigInfo>(nameof(FrameworkConfigInfo));
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
                if (!config.IsShowHerarchy) return;
             
                GameObject current = (GameObject)EditorUtility.InstanceIDToObject(id);             
                if (current == null)
                {                    
                    return;
                }
        
                //float tagWidth = 100;
                //float layerWidth = 120;
                float activeWidth = 20;
               // float buttonWidth = 60;
                EditorGUI.BeginChangeCheck();

                GUIStyle style = null;
                int order = 0;

                foreach (var item in current.GetComponents<Component>())
                {
                    DrawRectIcon(item.GetType(), rect, current, Color.black, ref order, ref style);
                }

                /*if (current.GetComponent<Camera>())               
                    DrawRectIcon<Camera>(rect, current, Color.cyan, ref order, ref style) ;

                if (current.GetComponent<AudioListener>())
                    DrawRectIcon<AudioListener>(rect, current, Color.yellow, ref order, ref style);

                if (current.GetComponent<Canvas>())                
                    DrawRectIcon<Canvas>(rect, current, Color.yellow, ref order, ref style);*/

                Event e = Event.current;

                
                
          
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

        private static void DrawRectIcon(Type type,Rect selectionRect, GameObject go, Color textColor, ref int order, ref GUIStyle style)
        {
            if (go.GetComponent(type))
            {
                order += 1;
                var rect = GetRect(selectionRect, order);
                DrawIcon(rect,type);
            }
        }

        private static Rect GetRect(Rect selectionRect, int index)
        {
            Rect rect = new Rect(selectionRect);
            rect.x += rect.width - 20 - (20 * index);
            rect.width = 18;
            return rect;
        }

        private static void DrawIcon(Rect rect,Type type)
        {
           
            var icon = EditorGUIUtility.ObjectContent(null, type).image;
            if (!type.Assembly.FullName.StartsWith("UnityEngine") && !type.Assembly.FullName.StartsWith("UnityEditor") && type.IsSubclassOf(typeof(MonoBehaviour)))
                GUI.Label(rect, EditorGUIUtility.IconContent("cs Script Icon"));         
            GUI.Label(rect, icon);     
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
