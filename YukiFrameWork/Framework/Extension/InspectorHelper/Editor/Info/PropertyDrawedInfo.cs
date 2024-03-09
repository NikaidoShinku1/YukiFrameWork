///=====================================================
/// - FileName:      PropertyMemberDrawedInfo.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/3/1 23:03:45
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using UnityEngine;
using System;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;

namespace YukiFrameWork
{
    [Serializable]
    public abstract class PropertyDrawedInfo
    {
        public GUIColorAttribute GUIColor { get; protected set; }
        public EnableEnumValueIfAttribute[] EnableEnumValueIf { get; protected set; }
        public DisableEnumValueIfAttribute[] DisableEnumValueIf { get; protected set; }
        public EnableIfAttribute[] EnableIf { get; protected set; }
        public DisableIfAttribute[] DisableIf { get; protected set; }
        public LabelAttribute Label { get; protected set; }
        public PropertyRangeAttribute PropertyRange { get; protected set; }
        public Type ItemType { get; set; }
        public Object target { get; protected set; }
        public SerializedProperty Property { get; protected set; }
        public HelperBoxAttribute HelperBox { get; protected set; }
        public RuntimeDisabledGroupAttribute RuntimeDisabledGroup { get; protected set; }
        public EditorDisabledGroupAttribute EditorDisabledGroup { get; protected set; }
        public DisplayTextureAttribute DisplayTexture { get; protected set; }
        public ArrayLabelAttribute ArrayLabel { get; protected set; }
        public ListDrawerSettingAttribute ListDrawerSetting { get; protected set; }
        public BoolanPopupAttribute BoolanPopup { get; protected set; }
        protected GUIContent Content { get; set; }
        public PropertyDrawedInfo(Object target,SerializedProperty property,BoolanPopupAttribute boolanPopup,ArrayLabelAttribute arrayLabel,ListDrawerSettingAttribute listDrawerSetting,DisplayTextureAttribute displayTexture,PropertyRangeAttribute propertyRange,LabelAttribute label,GUIColorAttribute color,EnableEnumValueIfAttribute[] enableEnumValueIfAttribute
            ,DisableEnumValueIfAttribute[] disableEnumValueIfAttribute,EnableIfAttribute[] enableIf
            ,DisableIfAttribute[] disableIf,HelperBoxAttribute helperBox,RuntimeDisabledGroupAttribute runtimeDisabledGroup,EditorDisabledGroupAttribute editorDisabledGroup)
		{
			GUIColor = color;
            this.target = target;
            this.BoolanPopup = boolanPopup;
            this.ArrayLabel = arrayLabel;
            this.ListDrawerSetting = listDrawerSetting;
            this.DisplayTexture = displayTexture;
			this.EnableEnumValueIf = enableEnumValueIfAttribute;
			this.DisableEnumValueIf = disableEnumValueIfAttribute;
            this.PropertyRange = propertyRange;
			this.EnableIf = enableIf;
			this.DisableIf = disableIf;
            this.Label = label;
            this.Property = property;
            this.HelperBox = helperBox;
            this.RuntimeDisabledGroup = runtimeDisabledGroup;
            this.EditorDisabledGroup = editorDisabledGroup;
            string name = Label == null ? Property.displayName : Label.Label;
            if (Label == null) name = char.ToUpper(name[0]) + name.Substring(1);
            Content = new GUIContent(name);
        }

        public PropertyDrawedInfo() { }

        public abstract void OnGUI();

        public virtual void DrawHelperBox(bool rectValue = false,Rect rect = new Rect())
        {
            if (HelperBox == null) return;

            if (HelperBox != null)
            {
                switch (HelperBox.Message)
                {
                    case Message.Info:
                        if (!rectValue)
                            EditorGUILayout.HelpBox(HelperBox.Info, MessageType.Info);
                        else EditorGUI.HelpBox(rect, HelperBox.Info, MessageType.Info);
                        break;
                    case Message.Warning:
                        if (!rectValue)
                            EditorGUILayout.HelpBox(HelperBox.Info, MessageType.Warning);
                        else EditorGUI.HelpBox(rect, HelperBox.Info, MessageType.Warning);
                        break;
                    case Message.Error:
                        if (!rectValue)
                            EditorGUILayout.HelpBox(HelperBox.Info, MessageType.Error);
                        else EditorGUI.HelpBox(rect, HelperBox.Info, MessageType.Error);
                        break;
                }
            }
        }

        public virtual bool DrawConditionIf(Type type,Object target)
        {
            bool IsDraw = true;

            if (EnableEnumValueIf != null || EnableIf != null)
            {
                bool enumValue = false;
                bool defaultValue = false;
                if (EnableEnumValueIf.Length > 0)
                {
                    for (int i = 0; i < EnableEnumValueIf.Length; i++)
                    {
                        var obj = GlobalReflectionSystem.GetValue(target.GetType(), target, EnableEnumValueIf[i].Name);
                        enumValue = ((obj != null && obj.ToString() == EnableEnumValueIf[i].Enum.ToString()));
                        if (enumValue) break;
                    }
                }
                else enumValue = true;

                if (EnableIf.Length > 0)
                {
                    for (int i = 0; i < EnableIf.Length; i++)
                    {
                        object obj = GlobalReflectionSystem.GetValue(target.GetType(), target, EnableIf[i].ValueName);
                        if (obj == null) defaultValue = false;
                        else
                        {
                            bool value = (bool)obj;
                            defaultValue = value;
                            if (defaultValue) break;
                        }
                    }
                }
                else defaultValue = true;

                IsDraw = defaultValue && enumValue;
            }

            if ((DisableEnumValueIf != null && DisableEnumValueIf.Length > 0) || (DisableIf != null && DisableIf.Length > 0))
            {
                bool enumValue = false;
                bool defaultValue = false;

                if (DisableEnumValueIf.Length > 0)
                {
                    for (int i = 0; i < DisableEnumValueIf.Length; i++)
                    {
                        Enum e = (Enum)GlobalReflectionSystem.GetValue(target.GetType(), target, DisableEnumValueIf[i].Name);

                        enumValue = (e != null && e.ToString() == DisableEnumValueIf[i].Enum.ToString());
                        if (enumValue) break;
                    }
                }
                else enumValue = true;

                if (DisableIf.Length > 0)
                {
                    for (int i = 0; i < DisableIf.Length; i++)
                    {
                        object obj = GlobalReflectionSystem.GetValue(target.GetType(), target, DisableIf[i].ValueName);
                        if (obj == null) defaultValue = false;
                        else
                        {
                            bool value = (bool)obj;
                            defaultValue = value;
                            if (defaultValue) break;
                        }
                    }
                }
                else defaultValue = true;
                IsDraw = !(defaultValue && enumValue);
            }          

            return IsDraw;
        }

        
    }	    
}
#endif