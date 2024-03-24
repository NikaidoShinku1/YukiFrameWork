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
using System.Collections;
using YukiFrameWork.Extension;


#if UNITY_EDITOR
using UnityEditor;
using System.Collections.Generic;
using UnityEditorInternal;
using System.Reflection;
using System.Linq;
using Object = UnityEngine.Object;
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

        public static object PropertyField(string name,object obj, Type type)
        {			
            var typeCode = (TypeCode)Type.GetTypeCode(type);
			if (typeCode == TypeCode.Byte)
				obj = (byte)EditorGUILayout.IntField(name, obj != null ? (byte)obj : 0);
			else if (typeCode == TypeCode.SByte)
				obj = (sbyte)EditorGUILayout.IntField(name, obj != null ? (sbyte)obj : 0);
			else if (typeCode == TypeCode.Boolean)
				obj = EditorGUILayout.Toggle(name, obj != null ? (bool)obj : false);
			else if (typeCode == TypeCode.Int16)
				obj = (short)EditorGUILayout.IntField(name, obj != null ? (short)obj : 0);
			else if (typeCode == TypeCode.UInt16)
				obj = (ushort)EditorGUILayout.IntField(name, obj != null ? (ushort)obj : 0);
			else if (typeCode == TypeCode.Char)
				obj = EditorGUILayout.TextField(name, obj != null ? (string)obj : string.Empty).ToCharArray().FirstOrDefault();
			else if (typeCode == TypeCode.Int32)
				obj = EditorGUILayout.IntField(name, obj != null ? (int)obj : 0);
			else if (typeCode == TypeCode.UInt32)
				obj = (uint)EditorGUILayout.IntField(name, obj != null ? (int)obj : 0);
			else if (typeCode == TypeCode.Single)
				obj = EditorGUILayout.FloatField(name, obj != null ? (float)obj : 0);
			else if (typeCode == TypeCode.Int64)
				obj = EditorGUILayout.LongField(name, obj != null ? (long)obj : 0);
			else if (typeCode == TypeCode.UInt64)
				obj = (ulong)EditorGUILayout.LongField(name, obj != null ? (long)obj : 0);
			else if (typeCode == TypeCode.Double)
				obj = EditorGUILayout.DoubleField(name, obj != null ? (double)obj : 0);
			else if (typeCode == TypeCode.String)
				obj = EditorGUILayout.TextField(name, obj != null ? (string)obj : string.Empty);
			else if (type == typeof(Vector2))
				obj = EditorGUILayout.Vector2Field(name, obj != null ? (Vector2)obj : new Vector2());
			else if (type == typeof(Vector3))
				obj = EditorGUILayout.Vector3Field(name, obj != null ? (Vector3)obj : new Vector3());
			else if (type == typeof(Vector4))
				obj = EditorGUILayout.Vector4Field(name, obj != null ? (Vector4)obj : new Vector4());
			else if (type == typeof(Quaternion))
			{
				Quaternion q = obj == null ? new Quaternion() : (Quaternion)obj;
				var value = EditorGUILayout.Vector4Field(name, new Vector4(q.x, q.y, q.z, q.w));
				Quaternion q1 = new Quaternion(value.x, value.y, value.z, value.w);
				obj = q1;
			}
			else if (type == typeof(Rect))
				obj = EditorGUILayout.RectField(name, obj != null ? (Rect)obj : new Rect());
			else if (type == typeof(Color))
				obj = EditorGUILayout.ColorField(name, obj != null ? (Color)obj : new Color());
			else if (type == typeof(Color32))
				obj = EditorGUILayout.ColorField(name, obj != null ? (Color32)obj : new Color32());
			else if (type == typeof(AnimationCurve))
				obj = EditorGUILayout.CurveField(name, obj != null ? (AnimationCurve)obj : new AnimationCurve());
			else if (type.IsSubclassOf(typeof(Object)) | type == typeof(Object))
				obj = EditorGUILayout.ObjectField(name, (Object)obj, type, true);		
                return obj;
        }

		public static object PropertyField(string name, object obj, Type type, Rect rect)
		{
            var typeCode = (TypeCode)Type.GetTypeCode(type);
            if (typeCode == TypeCode.Byte)
                obj = (byte)EditorGUI.IntField(rect,name, obj != null ? (byte)obj : 0);
            else if (typeCode == TypeCode.SByte)
                obj = (sbyte)EditorGUI.IntField(rect,name, obj != null ? (sbyte)obj:0);
            else if (typeCode == TypeCode.Boolean)
                obj = EditorGUI.Toggle(rect,name, obj != null ? (bool)obj:false);
            else if (typeCode == TypeCode.Int16)
                obj = (short)EditorGUI.IntField(rect,name, obj != null ? (short)obj:0);
            else if (typeCode == TypeCode.UInt16)
                obj = (ushort)EditorGUI.IntField(rect,name, obj != null ? (ushort)obj:0);
            else if (typeCode == TypeCode.Char)
                obj = EditorGUI.TextField(rect,name, obj != null ? (string)obj:string.Empty).ToCharArray().FirstOrDefault();
            else if (typeCode == TypeCode.Int32)
                obj = EditorGUI.IntField(rect,name, obj != null ? (int)obj:0);
            else if (typeCode == TypeCode.UInt32)
                obj = (uint)EditorGUI.IntField(rect,name, obj != null ? (int)obj:0);
            else if (typeCode == TypeCode.Single)
                obj = EditorGUI.FloatField(rect,name, obj != null ? (float)obj:0);
            else if (typeCode == TypeCode.Int64)
                obj = EditorGUI.LongField(rect,name, obj != null ? (long)obj:0);
            else if (typeCode == TypeCode.UInt64)
                obj = (ulong)EditorGUI.LongField(rect,name, obj != null ? (long)obj:0);
            else if (typeCode == TypeCode.Double)
                obj = EditorGUI.DoubleField(rect,name, obj != null ? (double)obj:0);
            else if (typeCode == TypeCode.String)
                obj = EditorGUI.TextField(rect,name, obj != null ? (string)obj:string.Empty);
            else if (type == typeof(Vector2))
                obj = EditorGUI.Vector2Field(rect,name, obj != null ? (Vector2)obj:new Vector2());
            else if (type == typeof(Vector3))
                obj = EditorGUI.Vector3Field(rect,name, obj != null ? (Vector3)obj : new Vector3());
            else if (type == typeof(Vector4))
                obj = EditorGUI.Vector4Field(rect,name, obj != null ? (Vector4)obj : new Vector4());
            else if (type == typeof(Quaternion))
            {
                Quaternion q = obj == null ? new Quaternion() : (Quaternion)obj;
                var value = EditorGUI.Vector4Field(rect,name, new Vector4(q.x, q.y, q.z, q.w));
                Quaternion q1 = new Quaternion(value.x, value.y, value.z, value.w);
                obj = q1;
            }
            else if (type == typeof(Rect))
                obj = EditorGUI.RectField(rect,name, obj != null ? (Rect)obj : new Rect());
            else if (type == typeof(Color))
                obj = EditorGUI.ColorField(rect,name, obj != null ? (Color)obj : new Color());
            else if (type == typeof(Color32))
                obj = EditorGUI.ColorField(rect,name, obj != null ? (Color32)obj : new Color32());
            else if (type == typeof(AnimationCurve))
                obj = EditorGUI.CurveField(rect,name, obj != null ? (AnimationCurve)obj:new AnimationCurve());
            else if (type.IsSubclassOf(typeof(Object)) | type == typeof(Object))
                obj = EditorGUI.ObjectField(rect,name, (Object)obj, type, true);
            return obj;
        }

    }
}
#endif