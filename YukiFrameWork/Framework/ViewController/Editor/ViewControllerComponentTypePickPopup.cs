#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using YukiFrameWork.Extension;

namespace YukiFrameWork
{
    internal class ViewControllerComponentTypePickPopup : PopupWindowContent
    {
        private const float WindowWidth = 340f;
        private const float WindowHeight = 420f;
        private const float Padding = 10f;
        private const float TitleHeight = 22f;
        private const float AccentLineHeight = 2f;
        private const float SearchHeight = 20f;
        private const float RowHeight = 28f;
        private const float RowSpacing = 2f;
        private const float VerticalScrollBarReserve = 16f;
        private const float IconSize = 16f;
        private const float IconLeft = 6f;
        private const float IconTextGap = 4f;
        private const float CountBadgeWidth = 46f;
        private const float CountBadgeRightMargin = 8f;

        private readonly List<ScopedComponentType> _allTypes;
        private readonly Action<ScopedComponentType> _onSelected;
        private readonly SearchField _searchField = new();
        private string _searchText = string.Empty;
        private Vector2 _scroll;
        private List<ScopedComponentType> _filteredTypes;
        public static void Show(Rect activatorRect, ComponentTypeScopeContext context, Action<ScopedComponentType> onSelected)
        {
            var types = ViewControllerComponentTypeScope.Collect(context);
            PopupWindow.Show(activatorRect, new ViewControllerComponentTypePickPopup(types, onSelected));
        }

        private ViewControllerComponentTypePickPopup(List<ScopedComponentType> types, Action<ScopedComponentType> onSelected)
        {
            _allTypes = types ?? new List<ScopedComponentType>();
            _onSelected = onSelected;
            _filteredTypes = _allTypes;
        }

        public override Vector2 GetWindowSize() => new Vector2(WindowWidth, WindowHeight);

        public override void OnGUI(Rect rect)
        {
            ViewControllerFieldBinderStyles.Ensure();

            DrawPanelBackground(rect);

            var innerX = rect.x + Padding;
            var innerWidth = rect.width - Padding * 2;
            var y = rect.y + Padding;

            y = DrawTitle(innerX, innerWidth, y);
            y += 6f;
            y = DrawSearchField(innerX, innerWidth, y);
            y += 8f;

            var scrollHeight = rect.yMax - Padding - y;
            var scrollRect = new Rect(innerX, y, innerWidth, scrollHeight);

            if (_filteredTypes.Count == 0)
            {
                DrawEmptyHint(scrollRect);
                return;
            }

            DrawTypeList(scrollRect);
        }

        private static void DrawPanelBackground(Rect rect)
        {
            ViewControllerFieldBinderStyles.DrawRect(rect, ViewControllerFieldBinderStyles.PanelBg);
            ViewControllerFieldBinderStyles.DrawBorder(rect, ViewControllerFieldBinderStyles.BorderColor);
        }

        private float DrawTitle(float x, float width, float y)
        {
            var titleRect = new Rect(x, y, width, TitleHeight);
            GUI.Label(titleRect, FrameWorkConfigData.AutoBindTypePopupTitle, ViewControllerFieldBinderStyles.PopupTitle);

            var accentRect = new Rect(x, y + TitleHeight, width, AccentLineHeight);
            ViewControllerFieldBinderStyles.DrawRect(accentRect, ViewControllerFieldBinderStyles.AccentColor);
            return y + TitleHeight + AccentLineHeight;
        }

        private float DrawSearchField(float x, float width, float y)
        {
            var searchRect = new Rect(x, y, width, SearchHeight);
            var newSearch = _searchField.OnGUI(searchRect, _searchText);
            if (!string.Equals(newSearch, _searchText, StringComparison.Ordinal))
            {
                _searchText = newSearch;
                ApplyFilter();
                editorWindow?.Repaint();
            }

            return y + SearchHeight;
        }

        private void DrawEmptyHint(Rect area)
        {
            var hintRect = new Rect(area.x, area.y + 24, area.width, 48);
            GUI.Label(hintRect, FrameWorkConfigData.AutoBindNoTypesInScope, ViewControllerFieldBinderStyles.EmptyHint);
        }

        private static float GetListContentWidth(float viewportWidth)
            => Mathf.Max(0f, viewportWidth - VerticalScrollBarReserve);

        private void DrawTypeList(Rect scrollViewport)
        {
            var contentWidth = GetListContentWidth(scrollViewport.width);
            var contentHeight = _filteredTypes.Count * (RowHeight + RowSpacing);
            var viewRect = new Rect(0, 0, contentWidth, contentHeight);

            _scroll = GUI.BeginScrollView(scrollViewport, _scroll, viewRect, alwaysShowHorizontal: false, alwaysShowVertical: true);

            // BeginScrollView 内 mousePosition 为滚动内容区本地坐标，勿再手动换算 scroll 偏移
            for (var i = 0; i < _filteredTypes.Count; i++)
            {
                var rowRect = GetRowRect(i, viewRect.width);
                if (DrawTypeRow(rowRect, i, _filteredTypes[i]))
                {
                    _onSelected?.Invoke(_filteredTypes[i]);
                    editorWindow?.Close();
                }
            }

            GUI.EndScrollView();

            if (Event.current.type == EventType.MouseMove)
                editorWindow?.Repaint();
        }

        private static Rect GetRowRect(int index, float width)
            => new Rect(0, index * (RowHeight + RowSpacing), width, RowHeight);

        private bool DrawTypeRow(Rect rowRect, int index, ScopedComponentType entry)
        {
            var e = Event.current;
            var isHover = rowRect.Contains(e.mousePosition);
            var isOdd = index % 2 == 1;

            if (e.type == EventType.Repaint)
            {
                var bg = isHover
                    ? ViewControllerFieldBinderStyles.PopupHoverBg
                    : isOdd ? ViewControllerFieldBinderStyles.RowOddBg : ViewControllerFieldBinderStyles.RowEvenBg;
                ViewControllerFieldBinderStyles.DrawRect(rowRect, bg);

                if (isHover)
                    ViewControllerFieldBinderStyles.DrawBorder(rowRect, ViewControllerFieldBinderStyles.AccentColor, 1f);

                var iconContent = EditorGUIUtility.ObjectContent(null, entry.Type);
                var iconRect = new Rect(rowRect.x + IconLeft, rowRect.y + (rowRect.height - IconSize) * 0.5f, IconSize, IconSize);
                if (iconContent.image != null)
                    GUI.DrawTexture(iconRect, iconContent.image, ScaleMode.ScaleToFit);

                var badgeRect = new Rect(
                    rowRect.xMax - CountBadgeWidth - CountBadgeRightMargin,
                    rowRect.y + 5,
                    CountBadgeWidth,
                    18);
                var nameLeft = rowRect.x + IconLeft + IconSize + IconTextGap;
                var nameWidth = Mathf.Max(0f, badgeRect.x - nameLeft - 4f);
                var nameRect = new Rect(nameLeft, rowRect.y, nameWidth, rowRect.height);
                var displayName = TruncateWithEllipsis(
                    entry.DisplayName,
                    ViewControllerFieldBinderStyles.PopupTypeName,
                    nameWidth);
                GUI.Label(nameRect, displayName, ViewControllerFieldBinderStyles.PopupTypeName);
                var badgeBg = EditorGUIUtility.isProSkin
                    ? new Color(0.95f, 0.78f, 0.25f, 0.15f)
                    : new Color(0.95f, 0.78f, 0.25f, 0.25f);
                ViewControllerFieldBinderStyles.DrawRect(badgeRect, badgeBg);
                ViewControllerFieldBinderStyles.DrawBorder(badgeRect, ViewControllerFieldBinderStyles.AccentColor, 1f);
                var countText = string.Format(FrameWorkConfigData.AutoBindScopeCountLabel, entry.InstanceCount);
                GUI.Label(badgeRect, countText, ViewControllerFieldBinderStyles.PopupCountBadge);
            }

            EditorGUIUtility.AddCursorRect(rowRect, MouseCursor.Link);

            if (e.type == EventType.MouseDown && e.button == 0 && rowRect.Contains(e.mousePosition))
            {
                e.Use();
                return true;
            }

            return false;
        }

        private static string TruncateWithEllipsis(string text, GUIStyle style, float maxWidth)
        {
            if (string.IsNullOrEmpty(text) || maxWidth <= 1f) return string.Empty;
            if (style.CalcSize(new GUIContent(text)).x <= maxWidth) return text;

            const string ellipsis = "…";
            for (var len = text.Length - 1; len > 0; len--)
            {
                var candidate = text.Substring(0, len) + ellipsis;
                if (style.CalcSize(new GUIContent(candidate)).x <= maxWidth)
                    return candidate;
            }

            return ellipsis;
        }

        private void ApplyFilter()
        {
            if (string.IsNullOrWhiteSpace(_searchText))
            {
                _filteredTypes = _allTypes;
                return;
            }

            _filteredTypes = _allTypes
                .Where(t => t.DisplayName.IndexOf(_searchText, StringComparison.OrdinalIgnoreCase) >= 0
                            || (t.AssemblyQualifiedName != null &&
                                t.AssemblyQualifiedName.IndexOf(_searchText, StringComparison.OrdinalIgnoreCase) >= 0))
                .ToList();
        }
    }
}
#endif
