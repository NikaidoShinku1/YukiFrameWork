///=====================================================
/// - FileName:      ConvertPrefabTool.cs
/// - NameSpace:     Parkour
/// - Description:   框架自定ViewController
/// - Creation Time: 2026/4/21 11:23:50
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
namespace YukiFrameWork
{
	public sealed class ConvertPrefabTool : EditorWindow
	{
		private const float SectionSpacing = 8f;
		private const float ContentPadding = 10f;
		private const float LabelWidth = 72f;
		private const float PreviewSize = 56f;
		private const float PreviewGap = 8f;

		private GameObject newGameObject;
		private bool keepOldName = true;
		private bool keepActiveState = true;
		private bool copyLocalTransform = true;

		private GUIStyle _headerTitleStyle;
		private GUIStyle _headerSubtitleStyle;
		private GUIStyle _sectionTitleStyle;
		private GUIStyle _statusLabelStyle;
		private GUIStyle _primaryButtonStyle;
		private bool _stylesInitialized;

		[MenuItem("YukiFrameWork/LocalWindow/Prefab转换工具")]
		private static void OpenWindow()
		{
			var window = GetWindow<ConvertPrefabTool>("Prefab转换工具");
			window.minSize = new Vector2(440f, 360f);
			window.Show();
		}

		private void InitStyles()
		{
			if (_stylesInitialized)
				return;

			var isProSkin = EditorGUIUtility.isProSkin;

			_headerTitleStyle = new GUIStyle(EditorStyles.boldLabel)
			{
				fontSize = 15,
				alignment = TextAnchor.MiddleLeft,
				normal = { textColor = Color.white },
				padding = new RectOffset(14, 0, 0, 0),
			};

			_headerSubtitleStyle = new GUIStyle(EditorStyles.miniLabel)
			{
				fontSize = 11,
				alignment = TextAnchor.MiddleLeft,
				normal = { textColor = new Color(1f, 1f, 1f, 0.78f) },
				padding = new RectOffset(14, 0, 0, 0),
			};

			_sectionTitleStyle = new GUIStyle(EditorStyles.boldLabel)
			{
				fontSize = 12,
				margin = new RectOffset(0, 0, 0, 4),
			};

			_statusLabelStyle = new GUIStyle(EditorStyles.label)
			{
				fontSize = 12,
				alignment = TextAnchor.MiddleLeft,
				fontStyle = FontStyle.Bold,
			};

			_primaryButtonStyle = new GUIStyle(GUI.skin.button)
			{
				fontSize = 13,
				fontStyle = FontStyle.Bold,
				fixedHeight = 36f,
				normal = { textColor = isProSkin ? Color.white : new Color(0.12f, 0.12f, 0.12f) },
			};

			_stylesInitialized = true;
		}

		private void OnGUI()
		{
			InitStyles();

			var oldLabelWidth = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = LabelWidth;

			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(ContentPadding);
			EditorGUILayout.BeginVertical();
			DrawHeaderBanner();
			EditorGUILayout.Space(SectionSpacing);

			DrawTargetSection();
			EditorGUILayout.Space(SectionSpacing);

			DrawOptionsSection();
			EditorGUILayout.Space(SectionSpacing);

			DrawStatusSection();
			EditorGUILayout.Space(SectionSpacing + 2f);

			DrawActionButton();
			EditorGUILayout.EndVertical();
			GUILayout.Space(ContentPadding);
			EditorGUILayout.EndHorizontal();

			EditorGUIUtility.labelWidth = oldLabelWidth;
		}

		private void DrawHeaderBanner()
		{
			var headerRect = EditorGUILayout.GetControlRect(false, 58f);
			var headerColor = EditorGUIUtility.isProSkin
				? new Color(0.16f, 0.38f, 0.58f)
				: new Color(0.24f, 0.52f, 0.78f);

			EditorGUI.DrawRect(headerRect, headerColor);

			var titleRect = new Rect(headerRect.x, headerRect.y + 10f, headerRect.width, 22f);
			var subtitleRect = new Rect(headerRect.x, headerRect.y + 32f, headerRect.width, 18f);
			GUI.Label(titleRect, "Prefab 批量转换", _headerTitleStyle);
			GUI.Label(subtitleRect, "将 Hierarchy 选中对象替换为指定预制体", _headerSubtitleStyle);
		}

		private void DrawTargetSection()
		{
			DrawSection("替换目标", () =>
			{
				EditorGUILayout.BeginHorizontal(GUILayout.MinHeight(PreviewSize));

				EditorGUILayout.BeginVertical(GUILayout.Width(PreviewSize), GUILayout.Height(PreviewSize));
				GUILayout.FlexibleSpace();
				var previewRect = GUILayoutUtility.GetRect(PreviewSize, PreviewSize, GUILayout.Width(PreviewSize), GUILayout.Height(PreviewSize));
				DrawTargetPreview(previewRect);
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndVertical();

				GUILayout.Space(PreviewGap);

				EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
				newGameObject = (GameObject)EditorGUILayout.ObjectField("预制体", newGameObject, typeof(GameObject), false);

				if (newGameObject == null)
					DrawFieldHint("请拖入或选择要替换成的 Prefab / GameObject");
				else
				{
					var assetPath = AssetDatabase.GetAssetPath(newGameObject);
					DrawFieldHint(string.IsNullOrEmpty(assetPath) ? "场景对象" : assetPath);
				}

				EditorGUILayout.EndVertical();
				EditorGUILayout.EndHorizontal();
			});
		}

		private static void DrawFieldHint(string text)
		{
			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(LabelWidth);
			EditorGUILayout.LabelField(text, EditorStyles.miniLabel);
			EditorGUILayout.EndHorizontal();
		}

		private void DrawTargetPreview(Rect rect)
		{
			var bgColor = EditorGUIUtility.isProSkin
				? new Color(0.18f, 0.18f, 0.18f)
				: new Color(0.82f, 0.82f, 0.82f);
			EditorGUI.DrawRect(rect, bgColor);

			if (newGameObject == null)
			{
				var icon = EditorGUIUtility.IconContent("d_Prefab Icon").image;
				if (icon != null)
				{
					var iconRect = new Rect(
						rect.x + (rect.width - 28f) * 0.5f,
						rect.y + (rect.height - 28f) * 0.5f,
						28f,
						28f);
					GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit, true);
				}

				return;
			}

			var preview = AssetPreview.GetAssetPreview(newGameObject) ?? AssetPreview.GetMiniThumbnail(newGameObject);
			if (preview != null)
				GUI.DrawTexture(rect, preview, ScaleMode.ScaleToFit, true);

			if (UnityEngine.Event.current.type == EventType.MouseDown && rect.Contains(UnityEngine.Event.current.mousePosition))
			{
				Selection.activeObject = newGameObject;
				EditorGUIUtility.PingObject(newGameObject);
				UnityEngine.Event.current.Use();
			}
		}

		private void DrawOptionsSection()
		{
			DrawSection("转换选项", () =>
			{
				keepOldName = EditorGUILayout.ToggleLeft("沿用旧对象名称", keepOldName);
				keepActiveState = EditorGUILayout.ToggleLeft("沿用旧激活状态", keepActiveState);
				copyLocalTransform = EditorGUILayout.ToggleLeft("复制本地变换 (Position / Rotation / Scale)", copyLocalTransform);
			});
		}

		private void DrawStatusSection()
		{
			var selectedCount = Selection.gameObjects.Length;
			var canReplace = newGameObject != null && selectedCount > 0;

			EditorGUILayout.BeginVertical(EditorStyles.helpBox);
			EditorGUILayout.LabelField("当前状态", _sectionTitleStyle);

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Hierarchy 选中数量", GUILayout.Width(LabelWidth));

			var countColor = selectedCount > 0
				? (EditorGUIUtility.isProSkin ? new Color(0.55f, 0.85f, 0.55f) : new Color(0.1f, 0.45f, 0.1f))
				: (EditorGUIUtility.isProSkin ? new Color(0.85f, 0.55f, 0.55f) : new Color(0.65f, 0.15f, 0.15f));

			var previousColor = GUI.color;
			GUI.color = countColor;
			EditorGUILayout.LabelField(selectedCount.ToString(), _statusLabelStyle);
			GUI.color = previousColor;

			EditorGUILayout.EndHorizontal();

			EditorGUILayout.Space(4f);
			EditorGUILayout.HelpBox(
				canReplace
					? $"准备就绪：将把 {selectedCount} 个对象替换为「{newGameObject.name}」。"
					: selectedCount == 0
						? "请先在 Hierarchy 中选中需要被替换的对象。"
						: "请先指定替换目标预制体。",
				canReplace ? MessageType.None : MessageType.Info);
			EditorGUILayout.EndVertical();
		}

		private void DrawActionButton()
		{
			var selectedCount = Selection.gameObjects.Length;
			var canReplace = newGameObject != null && selectedCount > 0;

			EditorGUI.BeginDisabledGroup(!canReplace);

			var buttonColor = EditorGUIUtility.isProSkin
				? new Color(0.28f, 0.62f, 0.36f)
				: new Color(0.35f, 0.72f, 0.42f);

			var previousBgColor = GUI.backgroundColor;
			GUI.backgroundColor = canReplace ? buttonColor : previousBgColor;

			if (GUILayout.Button(canReplace ? $"批量替换 ({selectedCount})" : "批量替换当前选中对象", _primaryButtonStyle))
				ReplaceSelectedObjects();

			GUI.backgroundColor = previousBgColor;
			EditorGUI.EndDisabledGroup();
		}

		private static void DrawSection(string title, System.Action drawContent)
		{
			EditorGUILayout.BeginVertical(EditorStyles.helpBox);
			EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
			EditorGUILayout.Space(2f);
			drawContent?.Invoke();
			EditorGUILayout.EndVertical();
		}

		private void ReplaceSelectedObjects()
		{
			if (newGameObject == null)
			{
				Debug.LogWarning("[ConvertPrefabTool] 请先指定替换目标。");
				return;
			}

			var selected = Selection.gameObjects;
			if (selected == null || selected.Length == 0)
			{
				Debug.LogWarning("[ConvertPrefabTool] 请先在层级中选中要被替换的对象。");
				return;
			}

			Undo.IncrementCurrentGroup();
			Undo.SetCurrentGroupName("Batch Replace Selected Objects");
			var undoGroup = Undo.GetCurrentGroup();

			int replacedCount = 0;
			for (int i = 0; i < selected.Length; i++)
			{
				var oldObj = selected[i];
				if (oldObj == null)
					continue;

				var parent = oldObj.transform.parent;
				var siblingIndex = oldObj.transform.GetSiblingIndex();
				var localPos = oldObj.transform.localPosition;
				var localRot = oldObj.transform.localRotation;
				var localScale = oldObj.transform.localScale;
				var oldName = oldObj.name;
				var oldActive = oldObj.activeSelf;

				var newObj = parent != null ? Instantiate(newGameObject, parent, false) : Instantiate(newGameObject);
				Undo.RegisterCreatedObjectUndo(newObj, "Create replacement object");
				newObj.transform.SetSiblingIndex(siblingIndex);

				if (copyLocalTransform)
				{
					newObj.transform.localPosition = localPos;
					newObj.transform.localRotation = localRot;
					newObj.transform.localScale = localScale;
				}

				if (keepOldName)
					newObj.name = oldName;
				else
				{
					newObj.name = newObj.name.Replace("(Clone)", "");
				}

				if (keepActiveState)
					newObj.SetActive(oldActive);

				Undo.DestroyObjectImmediate(oldObj);
				replacedCount++;
			}

			Undo.CollapseUndoOperations(undoGroup);

			ShowNotification(new GUIContent($"[ConvertPrefabTool] 已替换 {replacedCount} 个对象"));
			Debug.Log($"[ConvertPrefabTool] 已替换 {replacedCount} 个对象。");
		}
	}
}
#endif
