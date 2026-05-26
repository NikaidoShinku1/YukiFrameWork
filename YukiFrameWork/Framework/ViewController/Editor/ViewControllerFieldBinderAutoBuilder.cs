#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using YukiFrameWork.Extension;

namespace YukiFrameWork
{
    internal static class ViewControllerFieldBinderAutoBuilder
    {
        public static int Build(
            ISerializedFieldInfo info,
            Component target,
            Transform root,
            ViewControllerBindPickMode mode,
            IList<string> componentTypeFullNames,
            GameObject prefabAsset = null)
        {
            if (info == null || target == null || root == null || componentTypeFullNames == null)
                return 0;

            if (mode == ViewControllerBindPickMode.Prefab && prefabAsset == null)
                return 0;

            var types = ResolveTypes(componentTypeFullNames);
            if (types.Count == 0) return 0;

            var existingNames = new HashSet<string>(
                info.GetSerializeFields().Select(f => f.fieldName).Where(n => !string.IsNullOrEmpty(n)),
                StringComparer.Ordinal);

            var added = 0;
            Undo.RecordObject(target, "Auto Build Field Bindings");

            foreach (var type in types)
            {
                var matches = CollectMatches(root, mode, type, prefabAsset);
                foreach (var (go, typeIndex) in matches)
                {
                    if (IsAlreadyBound(info, go, typeIndex)) continue;

                    var fieldName = CreateUniqueFieldName(existingNames, type, go);
                    var data = new SerializeFieldData
                    {
                        target = go,
                        fieldTypeIndex = typeIndex,
                        fieldName = fieldName,
                        fieldLevelIndex = 0
                    };
                    info.AddFieldData(data);
                    existingNames.Add(fieldName);
                    added++;
                }
            }

            if (added > 0)
            {
                if (PrefabUtility.IsPartOfPrefabInstance(target))
                    PrefabUtility.RecordPrefabInstancePropertyModifications(target);
                EditorUtility.SetDirty(target);
            }

            return added;
        }

        private static List<Type> ResolveTypes(IList<string> typeFullNames)
        {
            var result = new List<Type>();
            foreach (var fullName in typeFullNames)
            {
                if (string.IsNullOrWhiteSpace(fullName)) continue;
                var type = Type.GetType(fullName);
                if (type == null)
                {
                    foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        type = assembly.GetType(fullName);
                        if (type != null) break;
                    }
                }

                if (type != null && typeof(Component).IsAssignableFrom(type))
                    result.Add(type);
            }

            return result;
        }

        private static List<(GameObject go, int typeIndex)> CollectMatches(
            Transform root,
            ViewControllerBindPickMode mode,
            Type componentType,
            GameObject prefabAsset)
        {
            var results = new List<(GameObject, int)>();
            if (mode == ViewControllerBindPickMode.Prefab)
            {
                if (prefabAsset == null) return results;
                var scanRoot = prefabAsset.transform;
                CollectMatchesRecursive(scanRoot, scanRoot, ViewControllerBindPickMode.Hierarchy, componentType, results);
                return results;
            }

            if (mode == ViewControllerBindPickMode.Hierarchy)
            {
                CollectMatchesRecursive(root, root, mode, componentType, results);
                return results;
            }

            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid()) return results;

            foreach (var rootGo in scene.GetRootGameObjects())
            {
                if (!ViewControllerFieldBinderPicker.PassesPickMode(rootGo, mode, root)) continue;
                CollectMatchesRecursive(rootGo.transform, root, mode, componentType, results);
            }

            return results;
        }

        private static void CollectMatchesRecursive(
            Transform current,
            Transform hierarchyRoot,
            ViewControllerBindPickMode mode,
            Type componentType,
            List<(GameObject go, int typeIndex)> results)
        {
            var go = current.gameObject;
            if (ViewControllerFieldBinderPicker.PassesPickMode(go, mode, hierarchyRoot))
            {
                if (componentType == typeof(GameObject))
                    results.Add((go, 0));
                else
                {
                    var component = go.GetComponent(componentType);
                    if (component != null)
                        results.Add((go, FindComponentTypeIndex(go, componentType)));
                }
            }

            for (var i = 0; i < current.childCount; i++)
                CollectMatchesRecursive(current.GetChild(i), hierarchyRoot, mode, componentType, results);
        }

        private static int FindComponentTypeIndex(GameObject go, Type componentType)
        {
            if (componentType == typeof(GameObject))
                return 0;

            var components = go.GetComponents<Component>();
            for (var i = 0; i < components.Length; i++)
            {
                var comp = components[i];
                if (comp != null && componentType.IsAssignableFrom(comp.GetType()))
                    return i + 1;
            }

            return 0;
        }

        private static bool IsAlreadyBound(ISerializedFieldInfo info, GameObject go, int typeIndex)
        {
            foreach (var field in info.GetSerializeFields())
            {
                if (field.target == null) continue;
                var boundGo = field.target is GameObject g ? g : (field.target as Component)?.gameObject;
                if (boundGo == go && field.fieldTypeIndex == typeIndex)
                    return true;
            }

            return false;
        }

        private static string CreateUniqueFieldName(HashSet<string> existingNames, Type componentType, GameObject go)
        {
            var shortName = char.ToLowerInvariant(componentType.Name[0]) + componentType.Name[1..];
            var candidate = SanitizeFieldName($"{shortName}_{go.name}");
            if (!existingNames.Contains(candidate))
                return candidate;

            var suffix = 1;
            while (existingNames.Contains($"{candidate}{suffix}"))
                suffix++;
            return $"{candidate}{suffix}";
        }

        private static string SanitizeFieldName(string name)
        {
            if (string.IsNullOrEmpty(name)) return "field";
            var cleaned = name.Replace(" ", string.Empty);
            if (char.IsDigit(cleaned[0]))
                cleaned = "_" + cleaned;
            return cleaned;
        }
    }
}
#endif
