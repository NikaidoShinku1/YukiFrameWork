using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace YukiFramework.AddressableExtension.Editor
{
    internal static class AddressablesEditorBridge
    {
        private static Action openAddressablesWindow;

        public static void OpenAddressablesWindow()
        {
            try
            {
                openAddressablesWindow ??= CreateOpenWindowDelegate();
                openAddressablesWindow?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogError("无法打开 Addressables 窗口: " + ex.Message);
            }
        }

        private static Action CreateOpenWindowDelegate()
        {
            var windowType = Type.GetType(
                "UnityEditor.AddressableAssets.GUI.AddressableAssetsWindow, Unity.Addressables.Editor");
            if (windowType == null)
                throw new InvalidOperationException("未找到 AddressableAssetsWindow 类型。");

            var initMethod = windowType.GetMethod("Init", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (initMethod == null)
                throw new InvalidOperationException("未找到 AddressableAssetsWindow.Init 方法。");

            return (Action)Delegate.CreateDelegate(typeof(Action), initMethod);
        }
    }
}
