///=====================================================
/// - FileName:      DrawingUtility.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/3/19 20:34:12
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
using System.Collections.Generic;
using UnityEditorInternal;
using System.Reflection;
using System.Linq;

namespace YukiFrameWork
{
	public static class DrawingUtility
	{	
		public static void SetReoderableList(InfoData item, ReorderableList reorderableList, LabelAttribute label,ListDrawerSettingAttribute listDrawerSetting,object target,Type type)
		{
			var displayNames = new List<string>();
			reorderableList.drawHeaderCallback = rect =>
			{
                var newRect = new Rect(rect.xMax - 60, rect.y, 60, rect.height);
                EditorGUI.LabelField(rect, string.Format("{0}: {1}", PropertyUtility.GetLabel(item, label).text, item.property.arraySize), EditorStyles.label);
                if (GUI.Button(newRect, "Clear", "minibutton"))
                {
                    reorderableList.serializedProperty.arraySize = 0;
                }       
			};
			var propertyRange = item.member.GetCustomAttribute<PropertyRangeAttribute>(true);
			var range = item.member.GetCustomAttribute<RangeAttribute>(true);
			reorderableList.drawElementCallback = (rect, index, b, c) =>
			{			         
				if (item.property.isArray)
				{
                    var value = item.property.GetArrayElementAtIndex(index);
                    if (listDrawerSetting.IsAutoOnly)
						EditorGUI.BeginDisabledGroup(listDrawerSetting.IsReadOnly);
					else
					{
						object obj = GlobalReflectionSystem.GetValue(type, target, listDrawerSetting.ValueName);

						if (obj is bool v)
						{
							EditorGUI.BeginDisabledGroup(listDrawerSetting.IsReversal ? !v : v);
						}
						else EditorGUI.BeginDisabledGroup(false);
					}
					if (value.propertyType == SerializedPropertyType.Enum)
					{
                        Type type = (item.member as FieldInfo).FieldType;
                        Type itemType = null;
						if (type.IsArray)
							itemType = type.GetElementType();
						else if (type.IsGenericType)
							itemType = type.GetGenericArguments()[0];		
                        displayNames.Clear();
                        foreach (var enumName in value.enumNames)
                        {
                            var enumType = itemType.GetField(enumName);
                            var labelAttributes = enumType?.GetCustomAttributes<LabelAttribute>(false).ToArray();

                            displayNames.Add(labelAttributes.Length > 0 ? labelAttributes[0].Label : enumName);
                        }

                        int indexed = value.enumValueIndex;

                        indexed = EditorGUI.Popup(rect,value.displayName, indexed, displayNames.ToArray());

                        if (indexed != value.enumValueIndex)
                        {
                            value.enumValueIndex = indexed;

                        }
                    }
					else if ((value.propertyType == SerializedPropertyType.Integer || value.propertyType == SerializedPropertyType.Float)
					&& (range != null || propertyRange != null))
					{
						float minValue = range != null ? range.min : propertyRange.MinValue;
						float maxValue = propertyRange != null ? propertyRange.MaxValue : range.max;
						if (value.propertyType == SerializedPropertyType.Float)
						{
							EditorGUI.Slider(rect, value, minValue, maxValue);
						}
						else
							EditorGUI.IntSlider(rect, value, (int)minValue, (int)maxValue);
					}
					else
					{
						EditorGUI.PropertyField(rect, value, true);
					}
					EditorGUI.EndDisabledGroup();
				}
				else
				{
                    string warning = typeof(ListDrawerSettingAttribute).Name + " can be used only on arrays or lists";
                    EditorGUILayout.HelpBox(warning, MessageType.Warning);
                    Debug.LogWarning(warning, PropertyUtility.GetTargetObject(item.property));

                    EditorGUILayout.PropertyField(item.property, true);
                }
			};

			reorderableList.elementHeightCallback = index =>
			{
				if (item.property.isArray && item.property.arraySize > 0)
				{
					var value = item.property.GetArrayElementAtIndex(index);
					return EditorGUI.GetPropertyHeight(value, true);
				}
				return 20;
			};			
		}	
		public static void PropertyField(InfoData item,LabelAttribute label,Dictionary<string,ReorderableList> listPairs)
		{
			if (listPairs.ContainsKey(item.member.Name))
			{
				listPairs[item.member.Name].DoLayoutList();
			}			
			else
			{
				EditorGUILayout.PropertyField(item.property, PropertyUtility.GetLabel(item, label), true);
			}
        }

		public static void PropertyFieldInBoolValue(InfoData item,LabelAttribute label, BoolanPopupAttribute boolanPopup)
		{
            int indexed = item.property.boolValue ? 1 : 0;
			string[] values = new string[] { boolanPopup.FalseValue, boolanPopup.TrueValue };
            indexed = EditorGUILayout.Popup(PropertyUtility.GetLabel(item, label), indexed, values);         
            item.property.boolValue = indexed == 1;
        }

		public static void PropertyFieldInEnum(InfoData item,LabelAttribute label,List<string> displayNames)
		{
			Type fieldType = (item.member as FieldInfo).FieldType;
			displayNames.Clear();		
			foreach (var enumName in item.property.enumNames)
			{
				var enumType = fieldType.GetField(enumName);
				var labelAttributes = enumType.GetCustomAttributes(typeof(LabelAttribute),false).ToArray();			
				displayNames.Add(labelAttributes.Length > 0 ? (labelAttributes[0] as LabelAttribute).Label : enumName);
			}
			int indexed = item.property.enumValueIndex;

			indexed = EditorGUILayout.Popup(PropertyUtility.GetLabel(item,label),indexed, displayNames.ToArray());

			if (indexed != item.property.enumValueIndex)
			{
				item.property.enumValueIndex = indexed;
            }
        }

		public static void PropertyFieldInSlider(InfoData item,LabelAttribute label, RangeAttribute range, PropertyRangeAttribute propertyRange)
		{
			float maxValue = 0;
			float minValue = 0;

			if (range != null)
			{
				minValue = range.min;
				maxValue = range.max;
			}
			else if (propertyRange != null)
			{
				minValue = propertyRange.MinValue; 
				maxValue = propertyRange.MaxValue;
			}

			if (item.property.propertyType == SerializedPropertyType.Integer)
			{
				EditorGUILayout.IntSlider(item.property, (int)minValue, (int)maxValue, PropertyUtility.GetLabel(item, label));
			}
			else if (item.property.propertyType == SerializedPropertyType.Float)
			{
				EditorGUILayout.Slider(item.property, minValue, maxValue, PropertyUtility.GetLabel(item, label));
			}
		}

		public static void PropertyFieldInHelperBox(HelperBoxAttribute helperBox)
		{
			switch (helperBox.Message)
			{
				case Message.Info:
                    EditorGUILayout.HelpBox(helperBox.Info, MessageType.Info);
                    break;
				case Message.Warning:
                    EditorGUILayout.HelpBox(helperBox.Info, MessageType.Warning);
                    break;
				case Message.Error:
                    EditorGUILayout.HelpBox(helperBox.Info, MessageType.Error);
                    break;
				default:
					break;
			}
			
		}

		public static void PropertyFieldInTexture(DisplayTextureAttribute DisplayTexture,InfoData item,LabelAttribute label)
		{	
            item.property.objectReferenceValue = EditorGUILayout.ObjectField(PropertyUtility.GetLabel(item,label),item.property.objectReferenceValue, typeof(Texture), false,GUILayout.Height(DisplayTexture.Height));        
        }

		public static void PropertyMethodInfo() { }
	}
}
#endif