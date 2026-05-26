#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using YukiFrameWork.Extension;

namespace YukiFrameWork
{
    internal class ViewControllerPrefabPickWindow : EditorWindow
    {
        private static Action<GameObject> _callback;
        private GameObject _prefab;

        public static void Show(Action<GameObject> callback)
        {
            _callback = callback;
            var window = GetWindow<ViewControllerPrefabPickWindow>(true, FrameWorkConfigData.SelectProjectPrefabBtn, true);
            window.minSize = new Vector2(360, 90);
            window.maxSize = new Vector2(520, 90);
            window.ShowUtility();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField(FrameWorkConfigData.PrefabPickHint, EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.Space(4);

            _prefab = (GameObject)EditorGUILayout.ObjectField(
                FrameWorkConfigData.PrefabAssetLabel,
                _prefab,
                typeof(GameObject),
                false);

            EditorGUILayout.Space(8);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(FrameWorkConfigData.CancelBtn, GUILayout.Width(80)))
                CloseWindow(null);

            EditorGUI.BeginDisabledGroup(!IsValidPrefab(_prefab));
            if (GUILayout.Button(FrameWorkConfigData.ConfirmBtn, GUILayout.Width(80)))
                CloseWindow(_prefab);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
        }

        private static bool IsValidPrefab(GameObject go)
        {
            if (go == null) return false;
            return PrefabUtility.IsPartOfPrefabAsset(go);
        }

        private void CloseWindow(GameObject result)
        {
            _callback?.Invoke(result);
            _callback = null;
            Close();
        }

        private void OnDestroy()
        {
            _callback = null;
        }
    }
}
#endif
