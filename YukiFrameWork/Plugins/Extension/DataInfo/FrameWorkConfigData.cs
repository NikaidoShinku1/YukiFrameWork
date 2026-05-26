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

        public static string ViewControllerParent => !IsEN ? "派生自:" : "Derive From:";

        public static string OpenPartialScriptBtn => !IsEN ? "打开分写脚本" : "Open the life cycle script";   
       
        public static string AddEventInfo => !IsEN ? "添加事件可视化注册器" : "Add an event visual registry";

        public static string EventAudioMationInfo => !IsEN ? "事件自动化注册,在自动化注册架构前会自动处理\nTip:如不采用自动化注册架构请改用Awake注册" : "Automated registration of events, processed automatically before automated registration schema \nTip: Use Awake registration instead of automated registration schema";

        public static string EventAwakeInfo => !IsEN ? "事件在Awake生命周期注册" : "Events are registered in the Awake lifecycle";

        public static string AutoInfo => !IsEN ? "创建时架构自动化:" : "Architecture Automation at Creation time:";

        public static string AssemblyInfo => !IsEN ? "项目(架构)脚本所依赖的程序集定义(非必要不更改):" : "Assembly definitions that scripts depend on:";

        public static string AssemblyDependInfo => !IsEN ? "程序集依赖项(有多个Assembly时可以使用):" : "Assembly dependencies (you can use them if you have multiple assemblies):";

        public static string BindExtensionInfo => !IsEN ? "字段绑定" : "Field Binding";

        public static string DragObjectInfo => !IsEN ? "将对象拖入这个区间:" : "Drag the object into this interval:";

        public static string FieldNameHeader => !IsEN ? "字段名" : "Name";

        public static string FieldLevelHeader => !IsEN ? "级别" : "Level";

        public static string FieldObjectHeader => !IsEN ? "对象" : "Object";

        public static string FieldComponentHeader => !IsEN ? "组件" : "Component";

        public static string SelectObjectBtn => !IsEN ? "选择对象..." : "Select Object...";

        public static string SelectComponentBtn => !IsEN ? "选择组件..." : "Select Component...";

        public static string SelectObjectFirstForComponent => !IsEN ? "请先选择绑定对象" : "Select a bound object first";

        public static string AddFieldBindingBtn => !IsEN ? "添加绑定" : "Add Binding";

        public static string GenerateBindingCodeBtn => !IsEN ? "生成代码" : "Generate Code";

        public static string NoHierarchyObjects => !IsEN ? "层级下没有可用对象" : "No objects under hierarchy";

        public static string NoSceneObjects => !IsEN ? "场景中没有可用对象" : "No objects in scene";

        public static string NoPrefabObjects => !IsEN ? "没有可用的预制体对象" : "No prefab objects available";

        public static string ObjectPickModeLabel => !IsEN ? "对象模式" : "Object Mode";

        public static string SelectProjectPrefabBtn => !IsEN ? "从项目选择预制体..." : "Pick Prefab Asset...";

        public static string PrefabPickHint => !IsEN
            ? "Only prefab assets under the project Assets folder can be selected."
            : "仅可选择项目 Assets 目录下的预制体资源。";

        public static string PrefabAssetLabel => !IsEN ? "预制体资源" : "Prefab Asset";

        public static string ConfirmBtn => !IsEN ? "确定" : "OK";

        public static string CancelBtn => !IsEN ? "取消" : "Cancel";

        public static string BindingCountLabel => !IsEN ? "items" : "项";

        public static string AutoBindSectionLabel => !IsEN ? "自动绑定（按组件类型）" : "Auto Bind (By Component Type)";

        public static string AutoBindTypeLabel => !IsEN ? "组件类型" : "Component Type";

        public static string AutoBindAddTypeBtn => !IsEN ? "添加类型" : "Add Type";

        public static string AutoBindBuildBtn => !IsEN ? "自动构建绑定" : "Auto Build Bindings";

        public static string AutoBindTypeHint => !IsEN
            ? "组件类型从当前对象模式的作用域内识别：场景=当前场景全部对象；层级=挂载对象自身及子物体；预制体=指定预制体根及子物体。点击类型按钮从弹窗选择。"
            : "Component types are collected from the active scope: Scene = active scene; Hierarchy = self and children; Prefab = selected prefab root and children. Click a type button to pick from the popup.";

        public static string AutoBindNoTypes => !IsEN ? "请先添加至少一个组件类型。" : "Add at least one component type first.";

        public static string AutoBindAddedCount => !IsEN ? "已添加 {0} 条绑定" : "Added {0} binding(s)";

        public static string AutoBindSelectTypeBtn => !IsEN ? "选择组件类型..." : "Select Component Type...";

        public static string AutoBindTypePopupTitle => !IsEN ? "选择组件类型" : "Select Component Type";

        public static string AutoBindTypeSearchPlaceholder => !IsEN ? "搜索类型..." : "Search types...";

        public static string AutoBindNoTypesInScope => !IsEN ? "当前作用域内没有可用组件类型" : "No component types in current scope";

        public static string AutoBindPrefabRequired => !IsEN ? "预制体模式下请先指定预制体资源。" : "Assign a prefab asset in Prefab mode first.";

        public static string AutoBindScopeCountLabel => !IsEN ? "×{0}" : "×{0}";

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