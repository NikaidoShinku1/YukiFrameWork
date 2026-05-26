#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using YukiFrameWork.Extension;

namespace YukiFrameWork
{
    public enum ViewControllerBindPickMode
    {
        Scene = 0,
        Hierarchy = 1,
        Prefab = 2
    }

    internal static class ViewControllerFieldBinderPicker
    {
        private static readonly string[] ModeLabelsCn = { "场景", "层级", "预制体" };
        private static readonly string[] ModeLabelsEn = { "Scene", "Hierarchy", "Prefab" };

        public static string[] GetModeLabels()
            => FrameWorkConfigData.IsEN ? ModeLabelsEn : ModeLabelsCn;

        public static ViewControllerBindPickMode GetPickMode()
        {
            var index = PlayerPrefs.GetInt("ViewControllerBindPickMode", (int)ViewControllerBindPickMode.Scene);
            return Enum.IsDefined(typeof(ViewControllerBindPickMode), index)
                ? (ViewControllerBindPickMode)index
                : ViewControllerBindPickMode.Scene;
        }

        public static void SetPickMode(ViewControllerBindPickMode mode)
            => PlayerPrefs.SetInt("ViewControllerBindPickMode", (int)mode);

        public static bool PassesPickMode(GameObject go, ViewControllerBindPickMode mode, Transform hierarchyRoot)
        {
            if (go == null) return false;

            return mode switch
            {
                ViewControllerBindPickMode.Hierarchy => IsUnderRoot(go.transform, hierarchyRoot),
                ViewControllerBindPickMode.Prefab => IsPrefabObject(go),
                _ => go.scene.IsValid() && go.scene.isLoaded
            };
        }

        public static bool IsPrefabObject(GameObject go)
        {
            if (go == null) return false;
            if (PrefabUtility.IsPartOfPrefabAsset(go)) return true;
            return PrefabUtility.IsPartOfPrefabInstance(go);
        }

        public static bool IsUnderRoot(Transform target, Transform root)
        {
            if (target == null || root == null) return false;
            var current = target;
            while (current != null)
            {
                if (current == root) return true;
                current = current.parent;
            }
            return false;
        }

        public static string GetDisplayPath(Transform target, Transform hierarchyRoot)
        {
            if (target == null) return string.Empty;

            var segments = new List<string>();
            var current = target;
            while (current != null)
            {
                segments.Add(current.name);
                current = current.parent;
            }

            segments.Reverse();
            return string.Join("/", segments);
        }

        public static void ShowObjectMenu(Transform hierarchyRoot, ViewControllerBindPickMode mode, Action<GameObject> onSelected)
        {
            var menu = new GenericMenu();
            var count = 0;

            if (mode == ViewControllerBindPickMode.Hierarchy)
            {
                CollectObjectEntries(hierarchyRoot, hierarchyRoot, menu, mode, onSelected, ref count);
            }
            else
            {
                var scene = SceneManager.GetActiveScene();
                if (!scene.IsValid())
                {
                    menu.AddDisabledItem(new GUIContent(FrameWorkConfigData.NoSceneObjects));
                    menu.ShowAsContext();
                    return;
                }

                foreach (var rootGo in scene.GetRootGameObjects())
                {
                    if (!PassesPickMode(rootGo, mode, hierarchyRoot)) continue;
                    CollectObjectEntries(rootGo.transform, hierarchyRoot, menu, mode, onSelected, ref count);
                }
            }

            if (mode == ViewControllerBindPickMode.Prefab)
            {
                menu.AddSeparator(string.Empty);
                menu.AddItem(new GUIContent(FrameWorkConfigData.SelectProjectPrefabBtn), false, () =>
                {
                    ViewControllerPrefabPickWindow.Show(onSelected);
                });
            }

            if (count == 0)
                menu.AddDisabledItem(new GUIContent(GetEmptyHint(mode)));

            menu.ShowAsContext();
        }

        /// <summary>仅列出已绑定对象自身的组件（不含子物体）。</summary>
        public static void ShowBoundObjectComponentMenu(GameObject boundObject, Action<int> onTypeIndexSelected)
        {
            var menu = new GenericMenu();
            if (boundObject == null)
            {
                menu.AddDisabledItem(new GUIContent(FrameWorkConfigData.SelectObjectFirstForComponent));
                menu.ShowAsContext();
                return;
            }

            menu.AddItem(new GUIContent("GameObject"), false, () => onTypeIndexSelected(0));

            var components = boundObject.GetComponents<Component>();
            for (var i = 0; i < components.Length; i++)
            {
                var comp = components[i];
                if (comp == null) continue;
                var typeIndex = i + 1;
                var typeName = comp.GetType().Name;
                menu.AddItem(new GUIContent(typeName), false, () => onTypeIndexSelected(typeIndex));
            }

            menu.ShowAsContext();
        }

        private static string GetEmptyHint(ViewControllerBindPickMode mode)
        {
            return mode switch
            {
                ViewControllerBindPickMode.Prefab => FrameWorkConfigData.NoPrefabObjects,
                ViewControllerBindPickMode.Hierarchy => FrameWorkConfigData.NoHierarchyObjects,
                _ => FrameWorkConfigData.NoSceneObjects
            };
        }

        private static void CollectObjectEntries(
            Transform current,
            Transform hierarchyRoot,
            GenericMenu menu,
            ViewControllerBindPickMode mode,
            Action<GameObject> onSelected,
            ref int count)
        {
            var go = current.gameObject;
            if (PassesPickMode(go, mode, hierarchyRoot))
            {
                var path = GetDisplayPath(current, hierarchyRoot);
                menu.AddItem(new GUIContent(path), false, () => onSelected(go));
                count++;
            }

            for (var i = 0; i < current.childCount; i++)
                CollectObjectEntries(current.GetChild(i), hierarchyRoot, menu, mode, onSelected, ref count);
        }

    }
}
#endif
