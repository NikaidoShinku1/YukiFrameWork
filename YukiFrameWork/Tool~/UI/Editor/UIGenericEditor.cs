#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace YukiFrameWork.UI
{
    public static class UIGenericEditor 
    {
        [MenuItem("GameObject/YukiFrameWork/Add Framework UIRoot")]
        public static void GenericBasePanel()
        {
            string sceneName = SceneManager.GetActiveScene().name;

            GameObject uiRoot = SceneManager.GetActiveScene().FindRootGameObject("UIRoot");

            if (uiRoot == null)
            {
                uiRoot = Resources.Load<GameObject>("UIRoot");
                var o = PrefabUtility.InstantiatePrefab(uiRoot);
                Undo.RegisterCreatedObjectUndo(o, "Add UIRoot");
                EditorUtility.SetDirty(uiRoot);
                AssetDatabase.SaveAssets();
            }
            else
            {
#if UNITY_EDITOR || YukiFrameWork_DEBUGFULL
                LogKit.W("场景已经存在UiRoot");
#endif
            }
        }
    }
}
#endif
