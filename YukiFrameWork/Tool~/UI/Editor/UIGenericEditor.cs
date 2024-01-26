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
            "创建框架生成BasePanel面板".LogInfo();

            GameObject g = Selection.activeGameObject;
            Canvas canvas = g?.GetComponent<Canvas>();
            if(canvas == null)
                canvas = new GameObject("Canvas").AddComponent<Canvas>();

            EventSystem system = Object.FindObjectOfType<EventSystem>();

            if (system == null)
            {
                system = new GameObject("EventSystem").AddComponent<EventSystem>();
                system.gameObject.AddComponent<StandaloneInputModule>();
            }

            RectTransform transform = new GameObject("BasePanel").AddComponent<RectTransform>();
            transform.SetRectTransformInfo(canvas);
            transform.gameObject.AddComponent<BasePanel>();
            var image = transform.gameObject.AddComponent<Image>();
            image.color = new Color(image.color.r, image.color.g, image.color.b, 0.4f);
        }       
    }
}
#endif
