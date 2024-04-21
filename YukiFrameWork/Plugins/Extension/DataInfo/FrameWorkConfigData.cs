using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace YukiFrameWork.Extension
{
    [Serializable]
    public class FrameWorkConfigData
    {
        public static bool IsEN
        {
            get => PlayerPrefs.GetInt("ViewControllerIsEN") == 1;
            set => PlayerPrefs.SetInt("ViewControllerIsEN", value ? 1 : 0);
        }

        public static string TitleTip => !IsEN ? "脚本生成设置:" : "Scripts Generation Settings:";

        public static string Email => !IsEN ? "邮箱:" : "Email:";

        public static string NameSpace => !IsEN ? "命名空间:" : "NameSpace:";

        public static string Name => !IsEN ? "脚本名称:" : "Script Name:";

        public static string Path => !IsEN ? "生成路径:" : "Generate Path:";

        public static string SelectScriptBtn => !IsEN ? "选择脚本" : "Select Scripts";

        public static string OpenScriptBtn => !IsEN ? "打开脚本" : "Open Scripts";

        public static string GenerateScriptBtn => !IsEN ? "生成脚本" : "Generate Scripts";

        public static string OpenPartialScriptBtn => !IsEN ? "打开分写脚本" : "Open the life cycle script";   
       
        public static string AddEventInfo => !IsEN ? "添加事件可视化注册器" : "Add an event visual registry";

        public static string EventAudioMationInfo => !IsEN ? "事件自动化注册,在自动化注册架构前会自动处理\nTip:如不采用自动化注册架构请改用Awake注册" : "Automated registration of events, processed automatically before automated registration schema \nTip: Use Awake registration instead of automated registration schema";

        public static string EventAwakeInfo => !IsEN ? "事件在Awake生命周期注册" : "Events are registered in the Awake lifecycle";

        public static string AutoInfo => !IsEN ? "创建时架构自动化:" : "Architecture Automation at Creation time:";

        public static string AssemblyInfo => !IsEN ? "项目(架构)脚本所依赖的程序集定义(非必要不更改):" : "Assembly definitions that scripts depend on:";

        public static string AssemblyDependInfo => !IsEN ? "程序集依赖项(有多个Assembly时可以使用):" : "Assembly dependencies (you can use them if you have multiple assemblies):";

        public static string BindExtensionInfo => !IsEN ? "字段绑定拓展:" : "Field binding extension:";

        public static string DragObjectInfo => !IsEN ? "将对象拖入这个区间:" : "Drag the object into this interval:";

        public static string RuntimeLocalization => !IsEN ? "本地化配置" : "Runtime localization configuration";

        public static string RuntimeDepandAssembly => !IsEN ? "绑定程序集设置" : "Bind Assembly Settings";

        public static string GenericScriptInfo => !IsEN ? "脚本生成器" : "Script Generator";

        public static string LocalizationInfo => !IsEN ? "添加本地化语言支持:" : "Add localized language support:";

        public static string DefaultHelperInfo => !IsEN ? "默认语言可以被动态修改!" : "The default language can be changed dynamically!";
        public static string RuntimeLocalLanguageInfo => !IsEN ? "运行时的默认语言设置:" : "Default language Settings at runtime:";
        public static string AddDependLocalConfigInfo => !IsEN ? "可以添加多个子配置项(如果有多个配置的情况下)" : "Multiple sub-configuration items can be added (if there are multiple configurations)";
        public static string DependInfo => !IsEN ? "子配置项" : "DependConfig";

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
        static void BindIniter()
        {
            PlayerPrefs.SetInt("BindFoldOut", 1);
        }
#endif

    }
}