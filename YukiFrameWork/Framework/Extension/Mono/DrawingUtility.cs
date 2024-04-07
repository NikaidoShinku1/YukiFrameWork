///=====================================================
/// - FileName:      DrawingUtility.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/3/19 20:34:12
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using UnityEngine;
using System;

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