///=====================================================
/// - FileName:      ItemKitEditor.cs
/// - NameSpace:     YukiFrameWork.Knapsack
/// - Created:       Yuki
/// - Email:         1274672030@qq.com
/// - Description:   这是一个框架工具创建的脚本
/// - Creation Time: 2023/12/26 15:29:58
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================

using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.IO;
namespace YukiFrameWork.Knapsack
{
#if UNITY_EDITOR
    public class ItemKitEditor
    {
        [MenuItem("Assets/YukiFrameWork-ItemKit/创建背包一键式面板预制体",false,-1000)]
        public static void CreateInventoryAssets()
        {
            RectTransform panelRect = new GameObject("KnapsackPanel").AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.gameObject.AddComponent<CanvasGroup>();
            panelRect.gameObject.AddComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            panelRect.gameObject.AddComponent<Inventory>();
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            RectTransform slotConfigRect = new GameObject("SlotPanel").AddComponent<RectTransform>();
            slotConfigRect.transform.SetParent(panelRect);
            var group = slotConfigRect.gameObject.AddComponent<GridLayoutGroup>();
            slotConfigRect.offsetMin = panelRect.offsetMin;
            slotConfigRect.offsetMax = panelRect.offsetMax;
            slotConfigRect.anchorMin = panelRect.anchorMin;
            slotConfigRect.anchorMax = panelRect.anchorMax;

            RectTransform slotRect = new GameObject("Slot").AddComponent<RectTransform>();
            slotRect.gameObject.AddComponent<Slot>();
            slotRect.offsetMin = Vector2.zero;
            slotRect.anchorMin = new Vector2(0.5f, 0.5f);
            slotRect.anchorMax = new Vector2(0.5f, 0.5f);
            slotRect.offsetMin = panelRect.offsetMin;
            slotRect.offsetMax = panelRect.offsetMax;
            slotRect.gameObject.AddComponent<Image>().color = Color.black;
            slotRect.gameObject.AddComponent<Button>();
            slotRect.SetParent(slotConfigRect);
            Vector2 target = new Vector2(group.cellSize.x - (group.cellSize.x / 10), group.cellSize.y - (group.cellSize.y / 10));
            ItemUI itemUI = CreateItemUI(target,true);
            itemUI.transform.SetParent(slotRect);
            
            string targetPath = GetTargetPath(panelRect.gameObject.name);
            Text text = CreateItemUIText();           
            text.GetComponent<RectTransform>().sizeDelta = target;
            text.transform.SetParent(itemUI.transform);          
            PrefabUtility.SaveAsPrefabAsset(panelRect.gameObject, targetPath);
            GameObject.DestroyImmediate(panelRect.gameObject);
            AssetDatabase.Refresh();
        }       

        private static ItemUI CreateItemUI(Vector2 sizeDelta,bool Test = false)
        {
            RectTransform rect = new GameObject(Test? "ItemUI(参考,应在调整好后单独做成Prefab并删除该预制体的此对象)":"ItemUI").AddComponent<RectTransform>();

            var image = rect.gameObject.AddComponent<Image>();
            image.color = Color.white;
            image.raycastTarget = false;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.sizeDelta = sizeDelta;
            return rect.gameObject.AddComponent<ItemUI>();                  
        }

        private static Text CreateItemUIText()
        {
            RectTransform textTransform = new GameObject("Amount").AddComponent<RectTransform>();
            var text = textTransform.gameObject.AddComponent<Text>();         
            text.text = "0";
            textTransform.anchorMin = Vector2.zero;
            textTransform.anchorMax = Vector2.one;
            text.alignment = TextAnchor.LowerRight;
            textTransform.anchoredPosition = Vector2.zero;
            text.raycastTarget = false;
            text.color = Color.black;           
            
            return text;
        }

        private static string GetTargetPath(string name)
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (string.IsNullOrEmpty(path))
                path = "Assets";
            string targetPath = path + "/" + name + ".prefab";
            int i = 1;
            while (File.Exists(targetPath))
            {
                targetPath = path + "/" + name + $" {i} .prefab";
                i++;
            }
            return targetPath;
        }
    }
#endif
}