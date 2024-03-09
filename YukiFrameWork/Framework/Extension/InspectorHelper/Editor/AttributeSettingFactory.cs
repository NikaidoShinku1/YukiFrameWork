///=====================================================
/// - FileName:      AttributeSettingFactory.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/3/3 20:44:16
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using System.Reflection;
using System.Linq;
#if UNITY_EDITOR
namespace YukiFrameWork
{
	public static class AttributeSettingFactory
    {
		public static void CreateAllSettingAttribute
            (this MemberInfo member,out LabelAttribute label
            ,out GUIColorAttribute color,out EnableEnumValueIfAttribute[] enableEnumValueIfAttribute
            ,out DisableEnumValueIfAttribute[] disable,out EnableIfAttribute[] enableIf,out DisableIfAttribute[] disableIf
            ,out HelperBoxAttribute helperBox,out GUIGroupAttribute group
            ,out DisplayTextureAttribute displayTexture
            ,out PropertyRangeAttribute propertyRange,out ArrayLabelAttribute arrayLabel,out RangeAttribute defaultRange
            ,out RuntimeDisabledGroupAttribute runtimeDisabledGroup
            ,out EditorDisabledGroupAttribute editorDisabledGroup
            ,out ListDrawerSettingAttribute listDrawerSetting,out BoolanPopupAttribute boolanPopup)
		{
            label = member.GetCustomAttribute<LabelAttribute>(true);
            color = member.GetCustomAttribute<GUIColorAttribute>(true);
            enableEnumValueIfAttribute = member.GetCustomAttributes<EnableEnumValueIfAttribute>(true).ToArray();
            disable = member.GetCustomAttributes<DisableEnumValueIfAttribute>(true).ToArray();
            enableIf = member.GetCustomAttributes<EnableIfAttribute>(true).ToArray();
            disableIf = member.GetCustomAttributes<DisableIfAttribute>(true).ToArray();
            helperBox = member.GetCustomAttribute<HelperBoxAttribute>(true);
            group = member.GetCustomAttribute<GUIGroupAttribute>(true);
            displayTexture = member.GetCustomAttribute<DisplayTextureAttribute>(true);
           
            propertyRange = member.GetCustomAttribute<PropertyRangeAttribute>(true);
            arrayLabel = member.GetCustomAttribute<ArrayLabelAttribute>(true);
            defaultRange = member.GetCustomAttribute<RangeAttribute>(true);
            runtimeDisabledGroup = member.GetCustomAttribute<RuntimeDisabledGroupAttribute>(true);
            editorDisabledGroup = member.GetCustomAttribute<EditorDisabledGroupAttribute>(true);
            listDrawerSetting = member.GetCustomAttribute<ListDrawerSettingAttribute>(true);
            boolanPopup = member.GetCustomAttribute<BoolanPopupAttribute>(true);
        }
	}
}
#endif