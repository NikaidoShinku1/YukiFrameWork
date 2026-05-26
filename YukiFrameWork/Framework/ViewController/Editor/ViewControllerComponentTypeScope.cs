#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using YukiFrameWork.Extension;

namespace YukiFrameWork
{
    public struct ComponentTypeScopeContext
    {
        public Transform HierarchyRoot;
        public ViewControllerBindPickMode Mode;
        public GameObject PrefabAsset;
    }

    public struct ScopedComponentType
    {
        public Type Type;
        public string DisplayName;
        public string AssemblyQualifiedName;
        public int InstanceCount;
    }

    internal static class ViewControllerComponentTypeScope
    {
        private static readonly HashSet<Type> ExcludedComponentTypes = new()
        {
            typeof(Transform),
            typeof(CanvasRenderer),
        };

        public static List<ScopedComponentType> Collect(ComponentTypeScopeContext context)
        {
            var counts = new Dictionary<Type, int>();
            foreach (var transform in EnumerateScopeTransforms(context))
            {
                if (transform == null) continue;
                var go = transform.gameObject;

                if (!counts.ContainsKey(typeof(GameObject)))
                    counts[typeof(GameObject)] = 0;
                counts[typeof(GameObject)]++;

                foreach (var component in go.GetComponents<Component>())
                {
                    if (component == null) continue;
                    var type = component.GetType();
                    if (!ShouldIncludeType(type)) continue;
                    counts.TryGetValue(type, out var c);
                    counts[type] = c + 1;
                }
            }

            var result = new List<ScopedComponentType>();
            foreach (var pair in counts)
            {
                result.Add(new ScopedComponentType
                {
                    Type = pair.Key,
                    DisplayName = pair.Key == typeof(GameObject) ? "GameObject" : pair.Key.Name,
                    AssemblyQualifiedName = pair.Key.AssemblyQualifiedName,
                    InstanceCount = pair.Value
                });
            }

            result.Sort((a, b) => string.Compare(a.DisplayName, b.DisplayName, StringComparison.Ordinal));
            return result;
        }

        public static string GetDisplayName(string assemblyQualifiedName)
        {
            if (string.IsNullOrWhiteSpace(assemblyQualifiedName)) return string.Empty;
            var type = ResolveType(assemblyQualifiedName);
            if (type == null) return assemblyQualifiedName;
            return type == typeof(GameObject) ? "GameObject" : type.Name;
        }

        public static Type ResolveType(string assemblyQualifiedName)
        {
            if (string.IsNullOrWhiteSpace(assemblyQualifiedName)) return null;
            var type = Type.GetType(assemblyQualifiedName);
            if (type != null) return type;
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = assembly.GetType(assemblyQualifiedName);
                if (type != null) return type;
            }

            return null;
        }

        private static IEnumerable<Transform> EnumerateScopeTransforms(ComponentTypeScopeContext context)
        {
            switch (context.Mode)
            {
                case ViewControllerBindPickMode.Hierarchy:
                    if (context.HierarchyRoot != null)
                        return EnumerateHierarchy(context.HierarchyRoot);
                    break;

                case ViewControllerBindPickMode.Prefab:
                    if (context.PrefabAsset != null)
                        return EnumerateHierarchy(context.PrefabAsset.transform);
                    break;

                default:
                    return EnumerateScene(context.HierarchyRoot);
            }

            return Array.Empty<Transform>();
        }

        private static IEnumerable<Transform> EnumerateHierarchy(Transform root)
        {
            yield return root;
            for (var i = 0; i < root.childCount; i++)
            {
                foreach (var child in EnumerateHierarchy(root.GetChild(i)))
                    yield return child;
            }
        }

        private static IEnumerable<Transform> EnumerateScene(Transform hierarchyRoot)
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid()) yield break;

            foreach (var rootGo in scene.GetRootGameObjects())
            {
                if (!ViewControllerFieldBinderPicker.PassesPickMode(
                        rootGo, ViewControllerBindPickMode.Scene, hierarchyRoot))
                    continue;

                foreach (var t in EnumerateHierarchy(rootGo.transform))
                    yield return t;
            }
        }

        private static bool ShouldIncludeType(Type type)
        {
            if (type == null || !typeof(Component).IsAssignableFrom(type)) return false;
            if (ExcludedComponentTypes.Contains(type)) return false;
            return true;
        }
    }
}
#endif
