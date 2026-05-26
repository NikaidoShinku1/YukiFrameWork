#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace YukiFrameWork.Events.Editor
{
    internal enum EventAnalysisSafetyFilter
    {
        All,
        Safe,
        Risk
    }

    internal enum EventAnalysisToolbarActionKind
    {
        Neutral,
        Primary,
        Danger
    }

    internal static class EventAnalysisStyles
    {
        public static readonly Color RiskText = new Color(1f, 0.45f, 0.38f);
        public static readonly Color FilterGroupBg = new Color(0.18f, 0.18f, 0.18f, 1f);
        public static readonly Color FilterSelectedBg = new Color(0.28f, 0.52f, 0.38f, 1f);
        public static readonly Color FilterSelectedText = new Color(0.95f, 0.98f, 0.95f, 1f);

        private static readonly Color NeutralBg = new Color(0.28f, 0.28f, 0.28f, 1f);
        private static readonly Color NeutralBgHover = new Color(0.36f, 0.36f, 0.36f, 1f);
        private static readonly Color PrimaryBg = new Color(0.2f, 0.48f, 0.34f, 1f);
        private static readonly Color PrimaryBgHover = new Color(0.26f, 0.58f, 0.4f, 1f);
        private static readonly Color DangerBg = new Color(0.52f, 0.24f, 0.24f, 1f);
        private static readonly Color DangerBgHover = new Color(0.62f, 0.3f, 0.3f, 1f);
        private static readonly Color ActionText = new Color(0.92f, 0.92f, 0.92f, 1f);

        public static Button CreateActionButton(
            string text,
            Action onClick,
            EventAnalysisToolbarActionKind kind,
            string iconName = null)
        {
            var button = new Button(onClick);
            button.text = string.Empty;
            button.style.flexShrink = 0;
            button.style.flexGrow = 0;

            var content = new VisualElement();
            content.style.flexDirection = FlexDirection.Row;
            content.style.alignItems = Align.Center;
            content.pickingMode = PickingMode.Ignore;

            if (!string.IsNullOrEmpty(iconName))
            {
                var icon = EditorGUIUtility.IconContent(iconName);
                if (icon?.image is Texture2D texture)
                {
                    var image = new Image { image = texture };
                    image.style.width = 14;
                    image.style.height = 14;
                    image.style.marginRight = 4;
                    image.pickingMode = PickingMode.Ignore;
                    content.Add(image);
                }
            }

            var label = new Label(text);
            label.pickingMode = PickingMode.Ignore;
            label.style.color = ActionText;
            label.style.unityFontStyleAndWeight = FontStyle.Normal;
            content.Add(label);
            button.Add(content);

            var builtInLabel = button.Q<Label>();
            if (builtInLabel != null && builtInLabel != label)
                builtInLabel.style.display = DisplayStyle.None;

            ApplyToolbarActionButton(button, kind);
            return button;
        }

        public static void ApplyToolbarActionButton(Button button, EventAnalysisToolbarActionKind kind)
        {
            if (button == null)
                return;

            var (normalBg, hoverBg) = kind switch
            {
                EventAnalysisToolbarActionKind.Primary => (PrimaryBg, PrimaryBgHover),
                EventAnalysisToolbarActionKind.Danger => (DangerBg, DangerBgHover),
                _ => (NeutralBg, NeutralBgHover)
            };

            button.style.flexShrink = 0;
            button.style.flexGrow = 0;
            button.style.marginLeft = 2;
            button.style.marginRight = 2;
            button.style.marginTop = 2;
            button.style.marginBottom = 2;
            button.style.paddingLeft = 8;
            button.style.paddingRight = 10;
            button.style.paddingTop = 2;
            button.style.paddingBottom = 2;
            button.style.borderTopLeftRadius = 4;
            button.style.borderTopRightRadius = 4;
            button.style.borderBottomLeftRadius = 4;
            button.style.borderBottomRightRadius = 4;
            button.style.borderTopWidth = 0;
            button.style.borderBottomWidth = 0;
            button.style.borderLeftWidth = 0;
            button.style.borderRightWidth = 0;
            button.style.backgroundColor = normalBg;
            button.style.color = ActionText;
            button.style.unityFontStyleAndWeight = FontStyle.Normal;

            button.RegisterCallback<MouseEnterEvent>(_ => button.style.backgroundColor = hoverBg);
            button.RegisterCallback<MouseLeaveEvent>(_ => button.style.backgroundColor = normalBg);
        }

        public static VisualElement CreateToolbarRow()
        {
            var toolbar = new VisualElement();
            toolbar.style.flexDirection = FlexDirection.Row;
            toolbar.style.alignItems = Align.Center;
            toolbar.style.flexShrink = 0;
            toolbar.style.minHeight = 28;
            toolbar.style.paddingLeft = 4;
            toolbar.style.paddingRight = 4;
            toolbar.style.paddingTop = 2;
            toolbar.style.paddingBottom = 2;
            toolbar.style.backgroundColor = new Color(0.22f, 0.22f, 0.22f, 1f);
            return toolbar;
        }

        public static VisualElement CreateToolbarSeparator()
        {
            var separator = new VisualElement();
            separator.style.flexShrink = 0;
            separator.style.width = 1;
            separator.style.height = 18;
            separator.style.marginLeft = 6;
            separator.style.marginRight = 6;
            separator.style.marginTop = 5;
            separator.style.backgroundColor = new Color(0.45f, 0.45f, 0.45f, 0.55f);
            return separator;
        }

        public static Button CreateFilterButton(string text, Action onClick)
        {
            var button = new Button(onClick) { text = text };
            button.style.flexShrink = 0;
            button.style.flexGrow = 0;
            button.style.marginLeft = 1;
            button.style.marginRight = 1;
            button.style.paddingLeft = 8;
            button.style.paddingRight = 8;
            button.style.paddingTop = 2;
            button.style.paddingBottom = 2;
            button.style.borderTopLeftRadius = 3;
            button.style.borderTopRightRadius = 3;
            button.style.borderBottomLeftRadius = 3;
            button.style.borderBottomRightRadius = 3;
            button.style.borderTopWidth = 0;
            button.style.borderBottomWidth = 0;
            button.style.borderLeftWidth = 0;
            button.style.borderRightWidth = 0;
            return button;
        }

        public static VisualElement CreateFilterGroup(params Button[] buttons)
        {
            var group = new VisualElement();
            group.style.flexDirection = FlexDirection.Row;
            group.style.alignItems = Align.Center;
            group.style.flexShrink = 0;
            group.style.flexGrow = 0;
            group.style.backgroundColor = FilterGroupBg;
            group.style.borderTopLeftRadius = 4;
            group.style.borderTopRightRadius = 4;
            group.style.borderBottomLeftRadius = 4;
            group.style.borderBottomRightRadius = 4;
            group.style.marginLeft = 4;
            group.style.marginRight = 4;
            group.style.marginTop = 2;
            group.style.marginBottom = 2;
            group.style.paddingLeft = 2;
            group.style.paddingRight = 2;
            group.style.paddingTop = 2;
            group.style.paddingBottom = 2;

            foreach (var button in buttons)
                group.Add(button);

            return group;
        }

        public static void ApplyFilterButton(Button button, bool selected)
        {
            if (button == null)
                return;

            button.style.backgroundColor = selected ? FilterSelectedBg : StyleKeyword.Null;
            button.style.color = selected ? FilterSelectedText : ActionText;
            button.style.unityFontStyleAndWeight = selected ? FontStyle.Bold : FontStyle.Normal;
        }

        public static void ApplyRiskText(Label label)
        {
            if (label == null)
                return;

            label.style.color = RiskText;
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
        }

        public static void ClearTextStyle(Label label)
        {
            if (label == null)
                return;

            label.style.color = StyleKeyword.Null;
            label.style.unityFontStyleAndWeight = FontStyle.Normal;
        }
    }
}
#endif
