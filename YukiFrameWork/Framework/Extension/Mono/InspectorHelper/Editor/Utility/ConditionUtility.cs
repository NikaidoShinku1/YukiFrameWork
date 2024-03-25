///=====================================================
/// - FileName:      ConditionUtility.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/3/19 20:18:34
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
namespace YukiFrameWork
{
	public static class ConditionUtility
	{
        public static bool DrawConditionIf(EnableEnumValueIfAttribute[] EnableEnumValueIf
            ,EnableIfAttribute[] EnableIf, DisableEnumValueIfAttribute[] DisableEnumValueIf
            , DisableIfAttribute[] DisableIf
            , Type type, object target)
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
                        var obj = GlobalReflectionSystem.GetValue(type, target, EnableEnumValueIf[i].Name);
                        enumValue = ((obj != null && obj.ToString() == EnableEnumValueIf[i].Enum.ToString()));
                        if (enumValue) break;
                    }
                }
                else enumValue = true;

                if (EnableIf.Length > 0)
                {
                    for (int i = 0; i < EnableIf.Length; i++)
                    {
                        object obj = GlobalReflectionSystem.GetValue(type, target, EnableIf[i].ValueName);
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
                        object e = GlobalReflectionSystem.GetValue(type, target, DisableEnumValueIf[i].Name);

                        enumValue = (e != null && e.ToString() == DisableEnumValueIf[i].Enum.ToString());
                        if (enumValue) break;
                    }
                }
                else enumValue = true;

                if (DisableIf.Length > 0)
                {
                    for (int i = 0; i < DisableIf.Length; i++)
                    {
                        object obj = GlobalReflectionSystem.GetValue(type, target, DisableIf[i].ValueName);
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

        public static bool DisableGroupLifeCycle(RuntimeDisabledGroupAttribute runtimeDisabledGroup, EditorDisabledGroupAttribute editorDisabledGroup)
        {
            if (runtimeDisabledGroup != null && editorDisabledGroup != null)
            {
                return true;
            }

            if(runtimeDisabledGroup != null)
                return Application.isPlaying;
            else if(editorDisabledGroup != null)
                return !Application.isPlaying;

            return false;

        }

        public static bool DisableGroupInValue(Type type,object target,DisableGroupEnumValueIfAttribute disableGroupEnumValueIf, DisableGroupIfAttribute disableGroupIf)
        {
            if (disableGroupEnumValueIf == null && disableGroupIf == null)
                return false;

            if (disableGroupIf != null)
            {
                var obj = GlobalReflectionSystem.GetValue(type, target, disableGroupIf.ValueName);

                if (obj is bool value)
                {
                    return value;
                }   
            }
            else if (disableGroupEnumValueIf != null)
            {
                var obj = GlobalReflectionSystem.GetValue(type, target, disableGroupEnumValueIf.Name);

                if (obj != null && obj.ToString() == disableGroupEnumValueIf.Enum.ToString())
                    return true;  
            }
            return false;
        }
    }
}
