using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEngine;

namespace YukiFramework.AddressableExtension.Editor
{
    /// <summary>
    /// 兼容旧菜单入口，推荐使用可视化规则器窗口。
    /// </summary>
    public static class ResConditionerTool
    {
        [MenuItem("Assets/Create/YukiFrameWork/创建Addressable ResConditioner")]
        private static void CreateFilterLegacy()
        {
            if (!AddressableAssetSettingsDefaultObject.SettingsExists)
            {
                Debug.LogError("未找到 AddressableAssetSettings，请先初始化 Addressables。");
                return;
            }

            ResourcesConditionerEditorWindow.ShowWindow();
            Debug.Log("已打开 Resources Conditioner 规则器窗口。请使用「扫描 Addressables」与「生成代码」完成配置。");
        }
    }
}
