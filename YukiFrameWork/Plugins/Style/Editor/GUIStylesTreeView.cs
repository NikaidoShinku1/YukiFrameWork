///=====================================================
/// - FileName:      GUIStylesTreeView.cs
/// - NameSpace:     YukiFrameWork.UI
/// - Created:       Yuki
/// - Email:         1274672030@qq.com
/// - Description:   这是一个框架工具创建的脚本
/// - Creation Time: 2024/1/16 16:25:06
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================

#if UNITY_EDITOR
using UnityEngine;
using System;
using UnityEditor.IMGUI.Controls;
using UnityEditor;
using System.Collections.Generic;

namespace YukiFrameWork.Extension
{
    public class GUIStylesTreeView : StyleTreeView<GUIStyle>
    {
        private StyleTreeViewItem<GUIStyle> selectedItem;

        private Vector2 infoSrollPos;

        private int leftBtnIndex;

        private GUIContent leftBtnContent;

        private GUIContent textColorContent;

        private SearchField searchField;

        #region rect
        private Rect viewAreaRect;
        private Rect infoAreaRect;
        private Rect searchAreaRect;
        #endregion

        public GUIStylesTreeView(TreeViewState state, MultiColumnHeader mHeader) : base(state, mHeader)
        {
        }

        protected override void CellGUI(Rect rowRect, StyleTreeViewItem<GUIStyle> item, int index)
        {
            switch (index)
            {
                case 0:
                    GUI.Label(rowRect, item.item.name);
                    break;
                case 1:
                    rowRect.y += 5;
                    GUIStyle style = item.item;
                    GUIStyle current = new GUIStyle(style);
                    float scale = 20 / current.fixedHeight;
                    current.fixedHeight *= scale;
                    current.fixedWidth *= scale;
                    GUI.Box(rowRect, "", current);
                    break;
                case 2:
                    rowRect.height = 20;
                    rowRect.y += 5;
                    if (GUI.Button(rowRect, "Copy")) SerlizationInfo(item.item.name);
                    break;
                default:
                    break;
            }
        }

        protected override void OnInit()
        {
            base.OnInit();

            rowHeight = 30;
            showBorder = true;
            showAlternatingRowBackgrounds = true;
            useScrollView = true;
            leftBtnContent = new GUIContent();
            leftBtnContent.image = EditorGUIUtility.IconContent("Audio Mixer").image;
            textColorContent = new GUIContent("TextColor");
            searchField = new SearchField();
        }

        protected override bool DoesItemMatchSearch(TreeViewItem item, string search)
        {
            StyleTreeViewItem<GUIStyle> mItem = item as StyleTreeViewItem<GUIStyle>;
            return mItem.item.name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public override void OnGUI(Rect rect)
        {
            searchAreaRect = new Rect(rect.x + rect.width * 0.5f, rect.y, rect.width * 0.5f, 20);
            searchString = searchField.OnGUI(searchAreaRect,searchString);
            viewAreaRect = new Rect(rect.x, rect.y + 20, rect.width, rect.height * 0.7f);
            infoAreaRect = new Rect(rect.x, rect.y + viewAreaRect.height + searchAreaRect.height, rect.width, rect.height - searchAreaRect.height);
            base.OnGUI(viewAreaRect);
            OnInfoGUI(infoAreaRect);
        }

        private void OnInfoGUI(Rect rect)
        {
            Rect leftRect = new Rect(infoAreaRect.x, infoAreaRect.y, 80, infoAreaRect.height);
            GUILayout.BeginArea(leftRect,"","grey_border");
            OnLeftInfoGUI();
            GUILayout.EndArea();
            Rect rightRect = new Rect(infoAreaRect.x + leftRect.width,infoAreaRect.y,rect.width - leftRect.width,infoAreaRect.height);
            GUILayout.BeginArea(rightRect,"","grey_border");
            OnRightInfoGUI();
            GUILayout.EndArea();
        }

        private void OnLeftInfoGUI()
        {
            if (leftBtnIndex == 0)
                EditorGUILayout.BeginHorizontal("InsertionMarker");
            else EditorGUILayout.BeginHorizontal();

            leftBtnContent.text = "详情";

            if (GUILayout.Button(leftBtnContent, EditorStyles.label))
            {
                leftBtnIndex = 0;
            }
            EditorGUILayout.EndHorizontal();

            if (leftBtnIndex == 1) EditorGUILayout.BeginHorizontal("InsertionMarker");
            else EditorGUILayout.BeginHorizontal();
            leftBtnContent.text = "预览";

            if (GUILayout.Button(leftBtnContent, EditorStyles.label))
            {
                leftBtnIndex = 1;
            }
            EditorGUILayout.EndHorizontal();
        }

        private void OnRightInfoGUI()
        {
            if (selectedItem == null) 
            {
                GUIUtility.ExitGUI();
                return;
            }

            if (leftBtnIndex == 0)
            {
                infoSrollPos = EditorGUILayout.BeginScrollView(infoSrollPos);
                EditorGUILayout.LabelField($"Name: {selectedItem.item.name}");
                EditorGUILayout.LabelField($"Alignmmnt: {selectedItem.item.alignment}");
                EditorGUILayout.LabelField($"Font: {selectedItem.item.font?.name}");
                EditorGUILayout.LabelField($"FontSize: {selectedItem.item.fontSize}");
                EditorGUILayout.ColorField(textColorContent, selectedItem.item.normal.textColor, false, false, false);
                EditorGUILayout.LabelField($"ContentOffset: {selectedItem.item.contentOffset}");
                EditorGUILayout.LabelField($"FixedHeight: {selectedItem.item.fixedHeight}");
                EditorGUILayout.LabelField($"FixedWidth: {selectedItem.item.fixedWidth}");
                EditorGUILayout.LabelField($"LinHeight: {selectedItem.item.lineHeight}");
                EditorGUILayout.EndScrollView();
            }
            else
            {
                EditorGUILayout.LabelField("",selectedItem.item);
            }
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            infoSrollPos = default;
            int id = selectedIds[0];

            var item = FindStyleTreeViewItem(id);
            selectedItem = item;
        }
    }
}
#endif