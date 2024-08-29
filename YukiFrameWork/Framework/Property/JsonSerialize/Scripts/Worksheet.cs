using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.IO;

namespace YukiFrameWork.JsonInspector
{
	[System.Serializable]
	public class Worksheet : ScriptableObject
	{
		#region Properties
		[SerializeField]
		private DataFile _file;
		public DataFile file
		{
			get { return _file; }
			private set { _file = value; }
		}

		new public string name { get; private set; }
		#endregion

		#region Tab Properties
		[SerializeField]
		private Rect _tab;
		public Rect tab
		{
			get { return _tab; }
			set { _tab = value; }
		}

		private static int tabWidth
		{
			get
			{
				return (int)Mathf.Min((JsonSerializeEditor.instance.worksheetPos.width - tabSpacing * (JsonSerializeEditor.instance.worksheetCount - 1)) / JsonSerializeEditor.instance.worksheetCount,
									  tabWidthMax);
			}
		}

		public static int tabHeight = 20;

		private static int tabWidthMax = 80;

		private static int tabSpacing = 3;
		#endregion

		#region State Properties
		private int index
		{
			get { return JsonSerializeEditor.instance.worksheets.IndexOf(this); }
		}

		public bool isChanged
		{
			get { return file != null && file.isChanged; }
		}
		#endregion

		#region Styling Properties
		private static GUIStyle _buttonStyle;
		private static GUIStyle buttonStyle
		{
			get
			{
				if (_buttonStyle == null)
				{
					_buttonStyle = new GUIStyle(GUI.skin.box);
					_buttonStyle.padding.bottom = 8;
					_buttonStyle.alignment = TextAnchor.MiddleLeft;
					_buttonStyle.active = new GUIStyleState();					
					_buttonStyle.fontSize = 11;
					_buttonStyle.richText = true;
					_buttonStyle.wordWrap = false;
					_buttonStyle.normal.background = Resources.Load<Texture2D>("Images/box");
				}
				return _buttonStyle;
			}
		}
		#endregion

		#region Unity Methods
		public void OnEnable()
		{
			hideFlags = HideFlags.HideAndDontSave;
			if (JsonSerializeEditor.instance != null)
			{
				tab = new Rect(JsonSerializeEditor.instance.worksheetPos.x + index * (tabWidth + tabSpacing),
							   JsonSerializeEditor.instance.worksheetPos.y,
							   tabWidth,
							   tabHeight * (4f / 3f));
			}
		}
		#endregion

		#region File Methods
		public Worksheet HasFile(DataFile file)
		{
			this.file = file;
			return this;
		}

		public Worksheet Save(FileInfo info = null)
		{
			if (file is JsonFile)
			{
				(file as JsonFile).Save(info);
			}
			if (info != null)
				name = Path.GetFileNameWithoutExtension(info.FullName);
			return this;
		}

		public Worksheet Open(FileInfo info,int hashCode)
		{
			file = CreateInstance<JsonFile>()
				.Load(info);
			this.textInstanceId = hashCode;
			return this
				.HasName(Path.GetFileNameWithoutExtension(info.FullName));
		}
		#endregion

		#region Name Methods
		public Worksheet HasName(string name)
		{
			this.name = name;
			return this;
		}
		#endregion

		#region Tab Methods
		public void SnapTabPosition()
		{
			Rect pos = tab;
			pos.x = JsonSerializeEditor.instance.worksheetPos.x;
			if (index != 0)
				pos.x += index * (tabWidth + tabSpacing);
			pos.width = tabWidth;
			tab = pos;
		}
		internal int textInstanceId;
        public void DragTabPosition(float delta)
		{
			Rect pos = tab;
			pos.x += delta;
			pos.x = Mathf.Clamp(pos.x, JsonSerializeEditor.instance.worksheetPos.x, JsonSerializeEditor.instance.worksheetPos.xMax - tab.width);
			tab = pos;
			if (index > 0)
			{
				Rect tabPrev = JsonSerializeEditor.instance.worksheets[index - 1].tab;
				if (tab.center.x < tabPrev.xMax)
				{
					Utility.Swap<Worksheet>(JsonSerializeEditor.instance.worksheets, index, index - 1);
					JsonSerializeEditor.instance.worksheets[index + 1].SnapTabPosition();
				}
			}
			if (index + 1 < JsonSerializeEditor.instance.worksheetCount)
			{
				Rect tabPrev = JsonSerializeEditor.instance.worksheets[index + 1].tab;
				if (tab.center.x > tabPrev.xMin)
				{
					Utility.Swap<Worksheet>(JsonSerializeEditor.instance.worksheets, index, index + 1);
					JsonSerializeEditor.instance.worksheets[index - 1].SnapTabPosition();
				}
			}
		}
		#endregion

		#region Rendering Methods
		public void Render(Rect position, bool active)
		{
			RenderTab(active);
			if (active)
				RenderSheet(position);
		}

		private void RenderTab(bool isActive)
		{
			GUI.backgroundColor = isActive ? new Color(96 / 255f, 125 / 255f, 139 / 255f) : new Color(69 / 255f, 90 / 255f, 100 / 255f);
			GUI.Label(tab,
					  string.Format("<b><color=#212121ff>{0}</color></b>", name),
					  buttonStyle);			
			GUI.backgroundColor = Color.white;
			if (tab.Contains(Event.current.mousePosition))
			{
				if (Event.current.type == EventType.MouseDown && Event.current.button == 1)
				{
					Debug.Log("啊");
					var menu = new GenericMenu();
					menu.AddItem(new GUIContent("Close"), false, () =>
					{
						JsonSerializeEditor.instance.OnCloseClicked(this);
					});
					menu.ShowAsContext();
					Event.current.Use();
				}
			}
        }

		private void RenderSheet(Rect position)
		{
			position.yMin += tabHeight;
			GUI.backgroundColor = new Color(207 / 255f, 216 / 255f, 220 / 255f);    // light primary color
			GUI.Box(position, "");
			GUI.backgroundColor = Color.white;
			GUILayout.BeginArea(position);
			if (file != null)
				file.Render();
			GUILayout.EndArea();
		}
		#endregion
	}

}
#endif