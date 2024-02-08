#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace YukiFrameWork.UI
{
    public static class UIGenericEditor 
    {
        [MenuItem("GameObject/YukiFrameWork/Create BasePanel")]
        public static void GenericBasePanel()
        {         
            GameObject g = Selection.activeGameObject;
            Canvas canvas = g?.GetComponent<Canvas>();
            if (canvas == null)
            {
                var obj = new GameObject("Canvas");
                
#if UNITY_2021_1_OR_NEWER
                Undo.RegisterCreatedObjectUndo(obj, "Create Canvas");

                canvas = Undo.AddComponent<Canvas>(obj);
                Undo.AddComponent<CanvasScaler>(obj);
                Undo.AddComponent<GraphicRaycaster>(obj);
#else
                canvas = obj.AddComponent<Canvas>();
                obj.AddComponent<CanvasScaler>();
                obj.AddComponent<GraphicRaycaster>();
#endif
            }

            EventSystem system = Object.FindObjectOfType<EventSystem>();

            if (system == null)
            {
                GameObject obj = new GameObject("EventSystem");
#if UNITY_2021_1_OR_NEWER
                Undo.RegisterCreatedObjectUndo(obj, "Create EventSystem");
                Undo.AddComponent<EventSystem>(obj);
                Undo.AddComponent<StandaloneInputModule>(obj);
#else
                obj.AddComponent<EventSystem>();
                obj.AddComponent<StandaloneInputModule>();
#endif
            }

            GameObject panel = new GameObject("BasePanel");
#if UNITY_2021_1_OR_NEWER
            Undo.RegisterCreatedObjectUndo(panel, "Create Panel");
            RectTransform transform = Undo.AddComponent<RectTransform>(panel);
#else 
            RectTransform transform = panel.AddComponent<RectTransform>();
#endif
            transform.SetRectTransformInfo(canvas);
#if UNITY_2021_1_OR_NEWER
            Undo.AddComponent<BasePanel>(panel);
            var image = Undo.AddComponent<Image>(panel);
#else
            panel.AddComponent<BasePanel>();
            var image = panel.AddComponent<Image>();

            EditorUtility.SetDirty(panel);
            AssetDatabase.SaveAssets();
#endif
            
            image.color = new Color(image.color.r, image.color.g, image.color.b, 0.4f);
        }       
    }
}
#endif
