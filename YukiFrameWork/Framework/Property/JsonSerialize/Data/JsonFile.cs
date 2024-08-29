using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using YukiFrameWork.Extension;

namespace YukiFrameWork.JsonInspector
{
	[System.Serializable]
	public class JsonFile : DataFile
	{
		#region Properties
		[SerializeField]
		new private JObject _data;
		new public JObject data
		{
			get
			{
				if (_data == null)
				{
					_data = new JObject();
				}
				return (JObject)_data;
			}
			set { _data = value; }
		}
		#endregion

#if UNITY_EDITOR
		#region Editor Fields
		private int tier = 0;

		//[SerializeField]
		private Vector2 scrollPos;

		[SerializeField]
		private List<bool> toggles = new List<bool>();

		[SerializeField]
		private string _serializedString;

		private int collectionIndex;

		//[System.NonSerialized]
		private Rect workspacePos = new Rect();
		#endregion

		#region Editor Properties
		public override bool isChanged
		{
			get
			{
				bool result = false;
				if (info == null)
					result = true;
				else if (data != null &&
						 !string.Equals(data.ToString(), CreateInstance<JsonFile>().Load(info).data.ToString()))
					result = true;
				return result;
			}
		}
		#endregion

		#region Popup Properties
		private GUIContent[] _dataTypeOptions;
		private GUIContent[] dataTypeOptions
		{
			get
			{
				if (_dataTypeOptions == null)
				{
					_dataTypeOptions = new GUIContent[] {
					new GUIContent(JTokenType.None.ToString(), Resources.Load<Texture2D>("Images/add_icon"), "Add a new data element."),
					new GUIContent(JTokenType.Object.ToString(), Resources.Load<Texture2D>("Images/object_icon"), "Change this object to a different type."),
					new GUIContent(JTokenType.Array.ToString(), Resources.Load<Texture2D>("Images/array_icon"), "Change this array to a different type."),
					new GUIContent(JTokenType.Integer.ToString(), Resources.Load<Texture2D>("Images/integer_icon"), "Change this integer to a different type."),
					new GUIContent(JTokenType.Float.ToString(), Resources.Load<Texture2D>("Images/float_icon"), "Change this float to a different type."),
					new GUIContent(JTokenType.String.ToString(), Resources.Load<Texture2D>("Images/string_icon"), "Change this string to a different type."),
					new GUIContent(JTokenType.Boolean.ToString(), Resources.Load<Texture2D>("Images/boolean_icon"), "Change this boolean to a different type.")
				};
				}
				return _dataTypeOptions;
			}
		}
		#endregion

		#region Icon Properties
		private GUIContent _keyIcon;
		private GUIContent keyIcon
		{
			get
			{
				if (_keyIcon == null)
				{
					_keyIcon = new GUIContent(Resources.Load<Texture2D>("Images/key_icon"), "Name of this data element.");
				}
				return _keyIcon;
			}
		}

		private GUIContent _valueIcon;
		private GUIContent valueIcon
		{
			get
			{
				if (_valueIcon == null)
				{
					_valueIcon = new GUIContent(Resources.Load<Texture2D>("Images/value_icon"), "Value of this data element.");
				}
				return _valueIcon;
			}
		}

		private GUIContent _downArrowIcon;
		private GUIContent downArrowIcon
		{
			get
			{
				if (_downArrowIcon == null)
					_downArrowIcon = new GUIContent(Resources.Load<Texture2D>("Images/down_arrow_icon"), "Show data elements.");
				return _downArrowIcon;
			}
		}

		private GUIContent _rightArrowIcon;
		private GUIContent rightArrowIcon
		{
			get
			{
				if (_rightArrowIcon == null)
					_rightArrowIcon = new GUIContent(Resources.Load<Texture2D>("Images/right_arrow_icon"), "Hide data elements.");
				return _rightArrowIcon;
			}
		}
		#endregion

		#region Color Properties
		private Color darkPrimaryColor
		{
			get { return Color.gray; }
		}

		private Color primaryColor
		{
			get { return new Color(96 / 255f, 125 / 255f, 139 / 255f); }
		}

		private Color lightPrimaryColor
		{
			get { return Color.white; }
		}

		private Color accentColor
		{
			get { return Color.red; }
		}


		private Color secondaryTextColor
		{
			get { return Color.yellow; }
		}

		private Color hoverColor
		{
			get
			{
				return Color.Lerp(lightPrimaryColor, accentColor, Mathf.Sin((float)(EditorApplication.timeSinceStartup % Mathf.PI * 6)) * 0.5f + 0.5f);
			}
		}
		#endregion

		#region Size Properties
		private int unitSize
		{
			get { return 32; }
		}

		private int fieldWidth
		{
			get { return unitSize * 5; }
		}

		private int bufferSize
		{
			get { return 8; }
		}
		#endregion

		#region Style Properties
		private GUIStyle _spanStyle;
		private GUIStyle spanStyle
		{
			get
			{
				if (_spanStyle == null)
				{
					_spanStyle = new GUIStyle(GUI.skin.label);
					_spanStyle.margin = new RectOffset(1, 1, 1, 0);
					_spanStyle.normal.background = Texture2D.whiteTexture;
					_spanStyle.normal.textColor = Color.white;
				}
				return _spanStyle;
			}
		}

		private GUIStyle _dataTypeDropdownStyle;
		private GUIStyle dataTypeDropdownStyle
		{
			get
			{
				if (_dataTypeDropdownStyle == null)
				{
					_dataTypeDropdownStyle = new GUIStyle(GUI.skin.button);
					_dataTypeDropdownStyle.imagePosition = ImagePosition.ImageOnly;
					_dataTypeDropdownStyle.padding = new RectOffset(0, 0, 0, 0);
				}
				return _dataTypeDropdownStyle;
			}
		}

		private GUIStyle _fieldStyle;
		private GUIStyle fieldStyle
		{
			get
			{
				if (_fieldStyle == null)
				{
					_fieldStyle = new GUIStyle(GUI.skin.textField);
					_fieldStyle.fontSize = 12;
					_fieldStyle.richText = true;
				}
				return _fieldStyle;
			}
		}
		#endregion
#endif

		#region Load Methods
		new public JsonFile Load(FileInfo info)
		{
			this.info = info;
			if (info != null &&
				File.Exists(info.FullName))
			{
				this.data = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(info.FullName));
			}
			return this;
		}
		#endregion

		#region Save Methods
		new public JsonFile Save(FileInfo info = null)
		{
			if (info != null)   // Save as
				this.info = info;
			if (this.info != null)
			{   // Save
				using (FileStream stream = File.Create(this.info.FullName)) { }
				File.WriteAllText(this.info.FullName, SerializationTool.SerializedObject(data));
			}
			else
				Debug.LogError("Missing file information. Save failed.");
			return this;
		}
		#endregion

#if UNITY_EDITOR
		#region Serialization Methods
		public override void OnBeforeSerialize()
		{
			if (_serializedString == null)
			{
				JObject obj = new JObject();
				obj.Add("data", data);
				if (info != null)
				{
					obj.Add("info", JObject.FromObject(info));
				}
				obj.Add("scrollX", scrollPos.x);
				obj.Add("scrollY", scrollPos.y);
				if (toggles.Count > 0)
					obj.Add("toggles", JsonConvert.SerializeObject(toggles));
				_serializedString = JsonConvert.SerializeObject(obj);
			}
		}

		public override void OnAfterDeserialize()
		{
			if (_serializedString != null)
			{
				JObject obj = (JObject)JsonConvert.DeserializeObject(_serializedString);
				this.data = (JObject)obj["data"];
				if (obj["info"] != null)
					this.info = (FileInfo)obj["info"].ToObject(typeof(FileInfo));
				if (obj["scrollX"] != null)
					this.scrollPos = new Vector2((float)obj["scrollX"], (float)obj["scrollY"]);
				if (obj["toggles"] != null)
				{
					this.toggles = JsonConvert.DeserializeObject<List<bool>>(obj["toggles"].ToString());
				}
				_serializedString = null;
			}
		}
		#endregion

		#region Render Methods
		public override void Render()
		{
			scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
			tier = 0;
			collectionIndex = 0;
			JToken val = data;
			RenderElement(ref val,
						  false);
			EditorGUILayout.EndScrollView();
		}

		private void RenderElement(ref JToken token, bool isTypeInteractive = true)
		{
			Rect spanPos, iconPos;
			spanPos = iconPos = RenderSpan(GetSpanHeight(token));
			RenderTypeIcon(ref iconPos, ref token, isTypeInteractive);
			if (token != null)
			{
				RenderName(iconPos, ref token);
				if (IsCollection(token))
				{
					if (toggles.Count > collectionIndex)
					{
						RenderFieldIcon(iconPos,
										toggles[collectionIndex] ? downArrowIcon : rightArrowIcon,
										false);
					}
					RenderSpanToggle(spanPos);
					RenderCollection(token);
				}
				else
				{
					JToken value = (token is JProperty) ? (token as JProperty).Value : token;
					if (value.Parent != null)
					{
						RenderValue(iconPos, (JValue)value);
					}
				}
			}
		}

		private Rect RenderSpan(int height)
		{
			Rect pos = new Rect();
			float contentWidth = Mathf.Max(EditorGUIUtility.currentViewWidth - bufferSize,
										   workspacePos.width - 1);
			if (true)
			{
				pos = GUILayoutUtility.GetRect(new GUIContent(""),
											   spanStyle,
											   GUILayout.Height(height + bufferSize * 3),
											   GUILayout.Width(contentWidth));
				GUI.backgroundColor = (tier & (1 << 0)) > 0 ? darkPrimaryColor : primaryColor;
				GUI.Box(pos, "", spanStyle);
				GUI.backgroundColor = Color.white;
			}
			return pos;
		}

		private void RenderSpanToggle(Rect pos)
		{
			if (toggles.Count <= collectionIndex)
			{
				toggles.Add(false);
			}
			if (collectionIndex < toggles.Count)
			{
				toggles[collectionIndex] = GUI.Toggle(pos, toggles[collectionIndex], "", GUIStyle.none);
			}
		}

		private void RenderTypeIcon(ref Rect pos, ref JToken value, bool interactive = true)
		{
			// If the value is a property get the represented value
			JToken dataVal = value.Type.Equals(JTokenType.Property) ? (value as JProperty).Value : value;

			// Get the icon position
			pos.x += bufferSize + (unitSize + bufferSize) * tier;
			pos.y += 1.5f * bufferSize;
			pos.width = pos.height = unitSize;

			// Render the field
			GUI.backgroundColor = Color.clear;
			int selection = GetDataTypeSelection(dataVal.Type);
			if (interactive)
			{
				GUI.contentColor = pos.Contains(Event.current.mousePosition) ? hoverColor : lightPrimaryColor;
				int selectionPrev = selection;
				selection = EditorGUI.Popup(pos, selection, dataTypeOptions, dataTypeDropdownStyle);
				if (!selection.Equals(selectionPrev))
				{
					if (value is JProperty)
					{
						value = new JProperty((value as JProperty).Name, GetSelectionPrimitive(selection));
					}
					else
						value = GetSelectionPrimitive(selection);
				}
			}
			else
			{
				GUI.contentColor = lightPrimaryColor;
				EditorGUI.LabelField(pos, new GUIContent(dataTypeOptions[selection].image, "Your root data object."), dataTypeDropdownStyle);
			}
			GUI.backgroundColor = Color.white;
			GUI.contentColor = Color.white;

			// Update the workspace bounds
			SetWorkspaceBounds(pos);
		}

		private Rect RenderTypeIcon(Rect pos, ref int selection, bool interactive = true)
		{
			pos.x += bufferSize + (unitSize + bufferSize) * tier;
			pos.y += 1.5f * bufferSize;
			pos.width = unitSize;
			pos.height = unitSize;
			GUI.backgroundColor = Color.clear;
			if (interactive)
			{
				GUI.contentColor = pos.Contains(Event.current.mousePosition) ? hoverColor : lightPrimaryColor;
				selection = EditorGUI.Popup(pos, selection, dataTypeOptions, dataTypeDropdownStyle);
			}
			else
			{
				GUI.contentColor = lightPrimaryColor;
				EditorGUI.LabelField(pos, new GUIContent(dataTypeOptions[selection].image, "Your root data object."), dataTypeDropdownStyle);
			}
			GUI.backgroundColor = Color.white;
			GUI.contentColor = Color.white;
			SetWorkspace(pos);
			return pos;
		}

		private int GetDataTypeSelection(JTokenType type)
		{
			int result = -1;
			for (int i = 0; i < dataTypeOptions.Length; i++)
			{
				if (dataTypeOptions[i].text.Equals(type.ToString()))
				{
					result = i;
					break;
				}
			}
			return result;
		}

		private void RenderCollection(JToken token)
		{
			if (collectionIndex < toggles.Count &&
				toggles[collectionIndex])
			{
				tier += 1;
				JToken value = token.Type.Equals(JTokenType.Property) ? (token as JProperty).Value : token;
				// Render existing collection
				List<JToken> collection = value.Children<JToken>().ToList();
				for (int i = 0; i < collection.Count; i++)
				{
					if (collection[i] is JContainer)
					{
						collectionIndex += 1;
					}
					JTokenType prevType = (collection[i] is JProperty) ? (collection[i] as JProperty).Value.Type : collection[i].Type;
					JToken subToken = collection[i];
					RenderElement(ref subToken);
					if (subToken is JProperty)
					{
						if (prevType != (subToken as JProperty).Value.Type)
						{
							RemoveToggles(collection[i]);
							collection[i] = subToken;
						}
						else if (!collection[i].Equals(subToken))
						{
							if (!IsCollectionKeyInUse(collection, (subToken as JProperty).Name))
							{
								collection[i] = subToken;
							}
							else
							{
								Debug.LogWarning("Properties must have unique names. Try again with another name.");
							}
						}
						else if ((subToken as JProperty).Value.Type.Equals(JTokenType.Null) ||
								   (subToken as JProperty).Value.Type.Equals(JTokenType.None))
						{
							RemoveToggles(collection[i]);
							collection.RemoveAt(i);
							i -= 1;
						}
					}
					else
					{
						if (!collection[i].Equals(subToken))
						{
							RemoveToggles(collection[i]);
							if (subToken == null)
							{
								collection.RemoveAt(i);
								i -= 1;
							}
							else
								collection[i] = subToken;
						}
					}
				}

				// Render add to collection button
				JToken addValue = RenderAddData();
				if (addValue != null)
				{
					if (value.Type.Equals(JTokenType.Object))
					{
						collection.Add(new JProperty(System.Guid.NewGuid().ToString(), addValue));
					}
					else if (value.Type.Equals(JTokenType.Array))
						collection.Add(addValue);
				}

				// Replace the collection
				try
				{
					(value as JContainer).ReplaceAll(collection);
				}
				catch (System.Exception e)
				{
					Debug.Log(e);
				}
				tier -= 1;
			}
			else
			{
				IterateCollectionIndex(token);
			}
		}

		private void IterateCollectionIndex(JToken token)
		{
			JToken value = token.Type.Equals(JTokenType.Property) ? (token as JProperty).Value : token;
			List<JToken> collection = value.Children<JToken>().ToList();
			for (int i = 0; i < collection.Count; i++)
			{
				JToken subValue = collection[i].Type.Equals(JTokenType.Property) ? (collection[i] as JProperty).Value : collection[i];
				if (subValue is JContainer)
				{
					collectionIndex += 1;
					IterateCollectionIndex(subValue);
				}
			}
		}

		private bool IsCollectionKeyInUse(List<JToken> collection, string key)
		{
			bool result = false;
			foreach (JProperty value in collection)
			{
				if (value.Name.Equals(key))
				{
					result = true;
				}
			}
			return result;
		}

		private void RemoveToggles(JToken token)
		{
			JToken data = (token is JProperty) ? (token as JProperty).Value : token;
			if (data is JContainer)
			{
				toggles.RemoveAt(collectionIndex);
				foreach (var childToken in data.Children().ToList())
				{
					RemoveToggles(childToken);
				}
			}
		}

		private Rect RenderValue(Rect pos, JValue value)
		{
			// Render data icon
			Rect iconPos = RenderFieldIcon(pos, valueIcon, false);

			// Get data field position
			Rect dataPos = iconPos;
			dataPos.x = dataPos.xMax + bufferSize;
			dataPos.width = fieldWidth;

			// Render data field
			GUI.backgroundColor = lightPrimaryColor;
			switch (value.Type)
			{
				case JTokenType.None:
				case JTokenType.Null:
					break;
				case JTokenType.Integer:
					int nInt = (int)value;
					nInt = EditorGUI.IntField(dataPos, nInt, fieldStyle);
					if (nInt != (int)value)
					{
						value.Value = nInt;
					}
					break;
				case JTokenType.Float:
					float nFloat = (float)value;
					nFloat = EditorGUI.FloatField(dataPos, nFloat, fieldStyle);
					if (nFloat != (int)value)
					{
						value.Value = nFloat;
					}
					break;
				case JTokenType.String:
					string nString = (string)value;
					int numLines = nString.Split('\n').Length;
					dataPos.height = fieldStyle.lineHeight * numLines + (iconPos.height - fieldStyle.lineHeight);
					nString = GUI.TextArea(dataPos, nString, fieldStyle);
					if (nString != (string)value)
					{
						value.Value = nString;
					}
					break;
				case JTokenType.Boolean:
					bool nBool = (bool)value;
					nBool = EditorGUI.Toggle(dataPos, nBool);
					if (nBool != (bool)value)
					{
						value.Value = nBool;
					}
					break;
				default:
					Debug.Log("Implement " + value.Type);
					break;
			}
			GUI.backgroundColor = Color.white;

			// Update workspace bounds
			dataPos.xMax += bufferSize;
			SetWorkspace(dataPos);
			return dataPos;
		}

		private void RenderName(Rect pos, ref JToken value)
		{
			string name = "root";
			if (value != null)
			{
				if (value.Type.Equals(JTokenType.Property))
				{
					name = (value as JProperty).Name;
					name = RenderName(pos, name);
					if (!(value as JProperty).Name.Equals(name))
					{
						try
						{
							value = new JProperty(name, (value as JProperty).Value);
						}
						catch (System.Exception e)
						{
							Debug.Log(e);
						}
					}
				}
				else
				{
					if (value.Parent != null &&
					   value.Parent.Type.Equals(JTokenType.Array))
						name = (value.Parent as JArray).IndexOf(value).ToString();
					RenderName(pos, name);
				}
			}
		}

		private string RenderName(Rect pos, ref JProperty value)
		{
			string newName = RenderName(pos, value.Name);
			if (!value.Name.Equals(newName))
			{
				try
				{
					value = new JProperty(newName, value.Value);
				}
				catch (System.Exception e)
				{
					Debug.Log(e);
				}
			}
			return newName;
		}

		private string RenderName(Rect pos, string name)
		{
			Rect iconPos = RenderFieldIcon(pos, keyIcon, true);
			GUI.backgroundColor = lightPrimaryColor;
			Rect namePos = iconPos;
			namePos.x = namePos.xMax + bufferSize;
			namePos.width = fieldWidth;
			GUI.contentColor = secondaryTextColor;
#if UNITY_5_3_OR_NEWER
			name = EditorGUI.DelayedTextField(namePos, name, fieldStyle);
#else
		name = EditorGUI.TextField(namePos, name, fieldStyle);
#endif
			GUI.contentColor = Color.white;
			GUI.backgroundColor = Color.white;
			namePos.xMax += bufferSize;
			SetWorkspace(namePos);
			return name;
		}

		private JToken RenderAddData()
		{
			Rect pos = RenderSpan(unitSize);
			int typeSelection = GetDataTypeSelection(JTokenType.None);
			Rect iconPos = RenderTypeIcon(pos, ref typeSelection);
			SetWorkspace(iconPos);
			JToken result = GetSelectionPrimitive(typeSelection);
			if (result is JContainer)
			{
				int newIndex = collectionIndex + 1;
				if (toggles.Count <= newIndex)
				{
					toggles.Add(false);
				}
				else
				{
					toggles.Insert(newIndex, false);
				}
			}
			return result;
		}

		private Rect RenderFieldIcon(Rect pos, GUIContent content, bool isUpper)
		{
			GUI.color = lightPrimaryColor;
			Rect result = pos;
			result.x = result.xMax + bufferSize;
			result.width = result.height = pos.height / 2f;
			result.y = result.center.y;
			float yOffset = (result.height + bufferSize) * 0.5f;
			result.y += isUpper ? -yOffset : yOffset;
			EditorGUI.LabelField(result, content);
			GUI.color = Color.white;
			return result;
		}

		private JToken GetSelectionPrimitive(int typeSelection)
		{
			JToken result = null;
			switch (typeSelection)
			{
				case 1:
					result = new JObject();
					break;
				case 2:
					result = new JArray();
					break;
				case 3:
					result = new int();
					break;
				case 4:
					result = new float();
					break;
				case 5:
					result = "";
					break;
				case 6:
					result = new bool();
					break;
				default:
					break;
			}
			return result;
		}

		private bool IsCollection(JToken token)
		{
			token = token.Type.Equals(JTokenType.Property) ? (token as JProperty).Value : token;
			return token.Type.Equals(JTokenType.Object) || token.Type.Equals(JTokenType.Array);
		}

		private void SetWorkspace(Rect elementPos)
		{
			if (elementPos.xMax > workspacePos.xMax)
				workspacePos.xMax = elementPos.xMax;
			if (elementPos.yMax > workspacePos.yMax)
				workspacePos.yMax = elementPos.yMax;
		}

		private void SetWorkspaceBounds(Rect elementPos)
		{
			if (elementPos.xMax > workspacePos.xMax)
				workspacePos.xMax = elementPos.xMax;
			if (elementPos.yMax > workspacePos.yMax)
				workspacePos.yMax = elementPos.yMax;
		}

		private int GetSpanHeight(JToken token)
		{
			int result = unitSize;
			token = token is JProperty ? (token as JProperty).Value : token;
			if (token != null &&
				token.Type.Equals(JTokenType.String))
			{
				int numLines = ((string)token).Split('\n').Length;
				result = (int)((unitSize / 2f) + (fieldStyle.lineHeight * numLines + ((unitSize / 2f) - fieldStyle.lineHeight)));
			}
			return result;
		}
		#endregion
#endif
	}
}