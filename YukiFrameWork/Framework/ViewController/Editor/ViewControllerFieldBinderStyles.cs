#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace YukiFrameWork
{
    internal static class ViewControllerFieldBinderStyles
    {
        private static bool _initialized;

        public static GUIStyle Panel;
        public static GUIStyle HeaderBar;
        public static GUIStyle HeaderLabel;
        public static GUIStyle RowEven;
        public static GUIStyle RowOdd;
        public static GUIStyle IndexLabel;
        public static GUIStyle PickerButton;
        public static GUIStyle PickerButtonEmpty;
        public static GUIStyle FieldInput;
        public static GUIStyle RemoveButton;
        public static GUIStyle FooterButton;
        public static GUIStyle FooterButtonPrimary;
        public static GUIStyle FoldoutTitle;
        public static GUIStyle CountBadge;
        public static GUIStyle EmptyHint;
        public static GUIStyle ModeBarLabel;
        public static GUIStyle ModeToggle;
        public static GUIStyle ModeToggleOn;
        public static GUIStyle PopupTitle;
        public static GUIStyle PopupTypeName;
        public static GUIStyle PopupCountBadge;

        public static Color AccentColor => new Color(0.95f, 0.78f, 0.25f);
        public static Color PopupHoverBg => EditorGUIUtility.isProSkin
            ? new Color(0.32f, 0.32f, 0.32f)
            : new Color(0.82f, 0.86f, 0.92f);
        public static Color ModeBarBg => EditorGUIUtility.isProSkin
            ? new Color(0.16f, 0.16f, 0.16f)
            : new Color(0.88f, 0.88f, 0.88f);
        public static Color PanelBg => EditorGUIUtility.isProSkin
            ? new Color(0.18f, 0.18f, 0.18f)
            : new Color(0.92f, 0.92f, 0.92f);
        public static Color RowEvenBg => EditorGUIUtility.isProSkin
            ? new Color(0.22f, 0.22f, 0.22f)
            : new Color(0.96f, 0.96f, 0.96f);
        public static Color RowOddBg => EditorGUIUtility.isProSkin
            ? new Color(0.19f, 0.19f, 0.19f)
            : new Color(0.93f, 0.93f, 0.93f);
        public static Color BorderColor => EditorGUIUtility.isProSkin
            ? new Color(0.08f, 0.08f, 0.08f, 0.9f)
            : new Color(0.55f, 0.55f, 0.55f, 0.6f);

        public static void Ensure()
        {
            if (_initialized) return;
            _initialized = true;

            Panel = new GUIStyle
            {
                padding = new RectOffset(8, 8, 8, 8),
                margin = new RectOffset(0, 0, 4, 4)
            };

            HeaderBar = new GUIStyle
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                padding = new RectOffset(10, 6, 6, 6),
                margin = new RectOffset(0, 0, 0, 2)
            };
            HeaderBar.normal.textColor = EditorGUIUtility.isProSkin
                ? new Color(0.85f, 0.85f, 0.85f)
                : new Color(0.25f, 0.25f, 0.25f);

            HeaderLabel = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleLeft,
                fontStyle = FontStyle.Bold,
                padding = new RectOffset(2, 2, 0, 0)
            };
            HeaderLabel.normal.textColor = EditorGUIUtility.isProSkin
                ? new Color(0.65f, 0.65f, 0.65f)
                : new Color(0.4f, 0.4f, 0.4f);

            RowEven = new GUIStyle { padding = new RectOffset(4, 4, 4, 4), margin = new RectOffset(0, 0, 1, 1) };
            RowOdd = new GUIStyle(RowEven);

            IndexLabel = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                fontSize = 10
            };
            IndexLabel.normal.textColor = EditorGUIUtility.isProSkin
                ? new Color(0.55f, 0.55f, 0.55f)
                : new Color(0.45f, 0.45f, 0.45f);

            PickerButton = new GUIStyle(EditorStyles.popup)
            {
                alignment = TextAnchor.MiddleLeft,
                fixedHeight = 22,
                fontSize = 11,
                padding = new RectOffset(8, 18, 2, 2)
            };

            PickerButtonEmpty = new GUIStyle(PickerButton);
            PickerButtonEmpty.normal.textColor = EditorGUIUtility.isProSkin
                ? new Color(0.5f, 0.5f, 0.5f)
                : new Color(0.55f, 0.55f, 0.55f);
            PickerButtonEmpty.fontStyle = FontStyle.Italic;

            FieldInput = new GUIStyle(EditorStyles.textField)
            {
                fixedHeight = 22,
                fontSize = 11,
                padding = new RectOffset(8, 6, 3, 3)
            };

            RemoveButton = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleCenter,
                fixedHeight = 22,
                fixedWidth = 26,
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                padding = new RectOffset(0, 0, 0, 1)
            };
            RemoveButton.normal.textColor = EditorGUIUtility.isProSkin
                ? new Color(0.85f, 0.45f, 0.45f)
                : new Color(0.75f, 0.2f, 0.2f);

            FooterButton = new GUIStyle(GUI.skin.button)
            {
                fixedHeight = 28,
                fontSize = 12,
                fontStyle = FontStyle.Normal,
                padding = new RectOffset(14, 14, 4, 4),
                margin = new RectOffset(4, 4, 0, 0)
            };

            FooterButtonPrimary = new GUIStyle(FooterButton)
            {
                fontStyle = FontStyle.Bold
            };
            FooterButtonPrimary.normal.textColor = AccentColor;

            FoldoutTitle = new GUIStyle(EditorStyles.foldout)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                padding = new RectOffset(14, 4, 0, 0),
                margin = new RectOffset(0, 0, 0, 0)
            };
            FoldoutTitle.normal.textColor = AccentColor;
            FoldoutTitle.onNormal.textColor = AccentColor;
            FoldoutTitle.focused.textColor = AccentColor;
            FoldoutTitle.onFocused.textColor = AccentColor;
            FoldoutTitle.active.textColor = AccentColor;
            FoldoutTitle.onActive.textColor = AccentColor;

            CountBadge = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 10,
                fontStyle = FontStyle.Normal,
                padding = new RectOffset(4, 4, 1, 1)
            };
            CountBadge.normal.textColor = EditorGUIUtility.isProSkin
                ? new Color(0.82f, 0.82f, 0.82f)
                : new Color(0.35f, 0.35f, 0.35f);

            ModeBarLabel = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleLeft,
                fontStyle = FontStyle.Bold,
                fontSize = 11
            };
            ModeBarLabel.normal.textColor = EditorGUIUtility.isProSkin
                ? new Color(0.7f, 0.7f, 0.7f)
                : new Color(0.35f, 0.35f, 0.35f);

            ModeToggle = new GUIStyle(EditorStyles.miniButton)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 11,
                fixedHeight = 18,
                padding = new RectOffset(6, 6, 2, 2)
            };

            ModeToggleOn = new GUIStyle(ModeToggle)
            {
                fontStyle = FontStyle.Bold
            };
            ModeToggleOn.normal.textColor = AccentColor;
            ModeToggleOn.normal.background = ModeToggle.active.background;

            EmptyHint = new GUIStyle(EditorStyles.centeredGreyMiniLabel)
            {
                fontSize = 11,
                fontStyle = FontStyle.Italic,
                padding = new RectOffset(0, 0, 12, 12)
            };

            PopupTitle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(2, 2, 2, 2)
            };
            PopupTitle.normal.textColor = AccentColor;

            PopupTypeName = new GUIStyle(EditorStyles.label)
            {
                fontSize = 11,
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(0, 0, 0, 0),
                clipping = TextClipping.Clip,
                wordWrap = false
            };

            PopupCountBadge = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 10,
                fontStyle = FontStyle.Bold,
                padding = new RectOffset(6, 6, 2, 2),
                fixedHeight = 18
            };
            PopupCountBadge.normal.textColor = EditorGUIUtility.isProSkin
                ? new Color(0.75f, 0.75f, 0.75f)
                : new Color(0.35f, 0.35f, 0.35f);
        }

        public static void DrawRect(Rect rect, Color color)
        {
            EditorGUI.DrawRect(rect, color);
        }

        public static void DrawBorder(Rect rect, Color color, float thickness = 1f)
        {
            DrawRect(new Rect(rect.x, rect.y, rect.width, thickness), color);
            DrawRect(new Rect(rect.x, rect.yMax - thickness, rect.width, thickness), color);
            DrawRect(new Rect(rect.x, rect.y, thickness, rect.height), color);
            DrawRect(new Rect(rect.xMax - thickness, rect.y, thickness, rect.height), color);
        }

        public static Color GetLevelColor(int levelIndex)
        {
            return levelIndex switch
            {
                0 => EditorGUIUtility.isProSkin ? new Color(0.55f, 0.55f, 0.55f) : new Color(0.45f, 0.45f, 0.45f),
                1 => new Color(0.45f, 0.72f, 0.95f),
                2 => new Color(0.95f, 0.68f, 0.35f),
                _ => new Color(0.55f, 0.88f, 0.55f)
            };
        }
    }
}
#endif
