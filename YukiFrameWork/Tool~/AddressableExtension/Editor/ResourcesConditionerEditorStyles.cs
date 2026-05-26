using UnityEditor;
using UnityEngine;

namespace YukiFramework.AddressableExtension.Editor
{
    internal static class ResourcesConditionerEditorStyles
    {
        private static bool initialized;
        private static GUIStyle sectionTitle;
        private static GUIStyle sectionBox;
        private static GUIStyle previewBox;
        private static GUIStyle statLabel;
        private static GUIStyle statValue;
        private static GUIStyle listHeader;
        private static GUIStyle listHeaderCentered;
        private static GUIStyle listCount;

        public static float LabelWidth { get; private set; } = 120f;
        public static float ContentPadding { get; private set; } = 10f;

        public static GUIStyle SectionTitle => Init(ref sectionTitle, () =>
        {
            var s = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 12,
                margin = new RectOffset(4, 4, 6, 4)
            };
            return s;
        });

        public static GUIStyle SectionBox => Init(ref sectionBox, () =>
        {
            var s = new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(8, 8, 8, 8),
                margin = new RectOffset(4, 4, 2, 6)
            };
            return s;
        });

        public static GUIStyle PreviewBox => Init(ref previewBox, () =>
        {
            var s = new GUIStyle(EditorStyles.textField)
            {
                wordWrap = true,
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(8, 8, 6, 6),
                fontStyle = FontStyle.Bold
            };
            s.normal.textColor = EditorGUIUtility.isProSkin
                ? new Color(0.75f, 0.9f, 1f)
                : new Color(0.1f, 0.35f, 0.55f);
            return s;
        });

        public static GUIStyle StatLabel => Init(ref statLabel, () =>
            new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.MiddleLeft });

        public static GUIStyle StatValue => Init(ref statValue, () =>
        {
            var s = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleRight };
            s.fontSize = 13;
            return s;
        });

        public static GUIStyle ListHeader => Init(ref listHeader, () =>
        {
            var s = new GUIStyle(EditorStyles.miniLabel)
            {
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };
            s.normal.textColor = EditorGUIUtility.isProSkin
                ? new Color(0.75f, 0.75f, 0.75f)
                : new Color(0.35f, 0.35f, 0.35f);
            return s;
        });

        public static GUIStyle ListHeaderCentered => Init(ref listHeaderCentered, () =>
        {
            var s = new GUIStyle(ListHeader) { alignment = TextAnchor.MiddleCenter };
            return s;
        });

        public static GUIStyle ListCount => Init(ref listCount, () =>
            new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.MiddleRight });

        public static Color ZebraColor => EditorGUIUtility.isProSkin
            ? new Color(1f, 1f, 1f, 0.03f)
            : new Color(0f, 0f, 0f, 0.04f);

        public static Color SelectedRowColor => EditorGUIUtility.isProSkin
            ? new Color(0.24f, 0.48f, 0.9f, 0.25f)
            : new Color(0.2f, 0.45f, 0.9f, 0.18f);

        private static GUIStyle Init(ref GUIStyle field, System.Func<GUIStyle> factory)
        {
            if (!initialized)
                initialized = true;
            if (field == null)
                field = factory();
            return field;
        }

        public static void DrawToolbarDivider()
        {
            var rect = GUILayoutUtility.GetRect(1f, 18f, GUILayout.Width(1f));
            rect.y += 2f;
            rect.height -= 4f;
            EditorGUI.DrawRect(rect, EditorGUIUtility.isProSkin
                ? new Color(1f, 1f, 1f, 0.12f)
                : new Color(0f, 0f, 0f, 0.18f));
        }
    }
}
