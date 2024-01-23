
using System.Collections.Generic;

namespace YukiFrameWork.Extension
{
    public class ImportWindowInfo
    {
        public static bool IsEN = false;

        public static string[] displayedOptions => new string[] { !IsEN ? "使用者" : "User", !IsEN ? "开发者" : "Developer" };    

        public static string DeveloperModeInfo => !IsEN ? "使用模式" : "Usage Mode";

        public static string ImportPath => !IsEN ? "导入路径" : "Import Path";

        public static string SelectPath => !IsEN ? "选择路径" : "Select Path";

        #region Module Info

        public static string SerializationInfo => !IsEN ? "框架序列化工具,可以将类转换成Json,Xml,Bytes文件流,集成ExcelToJson转换插件" : "Framework serialization tool, can convert classes to Json,Xml,Bytes file streams, integrated ExcelToJson conversion plug-in";

        public static string ActionKitInfo => !IsEN ? "ActionKit动作时序套件,可以完成例如定时、队列、事件循环、强化Update、等待帧回调等强化功能实现" : "ActionKit Action timing suite, you can complete such as timing, queue, event loop, enhanced Update, wait for frame callback and other enhanced functions";

        public static string BezierInfo => !IsEN ? "贝塞尔曲线计算公式管理类" : "Bessel curve calculation formula management class";

        public static string StateMechineInfo => !IsEN ? "状态机模块,可以实现对于状态的管理等操作,或者基础动作游戏设计,编辑器可视化" : "State machine module, can be implemented for state management and other operations, or basic action game design, editor visualization";

        public static string IOCInfo => !IsEN ? "IOC容器,可以完整实现控制反转的结构思想,DI注入等操作" : "IOC container can realize the structure idea of inversion of control,DI injection and other operations";

        public static string ABManagerInfo => !IsEN ? "ABManager,框架资源管理模块,提供AssetBundle的可视化管理功能,集成卸载、资源热更新、下载、释放等功能(原作者：弦小风,导入后可查看更多信息!)" : "\"ABManager, framework resource management module, provides visual management functions of AssetBundle, integrating uninstall, resource hot update, download, release and other functions (original author: XianXiaofeng, you can see more information after import!)";

        public static string UIInfo => !IsEN ? "UI框架,可分层式对UI统一管理(导入UI模块之前必须导入框架资源管理工具ABManager!)" : "UI framework, can be hierarchical UI unified management (UI module must be imported before the framework resource management tool ABManager!) ";

        public static string AudioInfo => !IsEN ? "AudioKit声音管理条件,可以对声音进行集成式管理(导入AudioKit模块之前必须导入框架资源管理工具ABManager!)" : "AudioKit sound management conditions for integrated sound management (you must import framework resource management tool ABManager before importing AudioKit modules!) ";

        public static string KnapsackInfo => !IsEN ? "背包系统,基础通用式背包系统,编辑器一键生成预制体,可自由拓展(导入背包系统之前必须导入框架资源管理工具ABManager!)" : "Knapsack system, basic universal backpack system, editor one-click generation of prefabricated, can be freely expanded (before importing the backpack system must import the framework resource management tool ABManager!)";

        public static string DoTweenInfo => !IsEN ? "插件集成：DoTween,一个用于Unity的快速、高效、完全类型安全的面向对象动画引擎" : "Plugin Integration: DoTween, a fast, efficient, fully type-safe object-oriented animation engine for Unity";

        public static string UniTaskInfo => !IsEN ? "插件集成：UniTask,是一种用于异步编程的 C# 库，它扩展了 .NET 中的 Task 和 await/async 模式,Unity轻量级异步编程框架" : "Plug-in Integration: UniTask, a C# library for asynchronous programming that extends the Task and await/async patterns in.NET,Unity Lightweight asynchronous programming framework";

        public static string UniRxInfo => !IsEN ? "插件集成：UniRx,一个响应式框架 (链式编程),Unity编程框架,专注于解决时间上的异步" : "Plugin Integration: UniRx, a responsive framework (chain programming),Unity programming framework, focused on solving asynchronous time";
        #endregion

        public static string ImportAllModuleInfo => !IsEN ? "一键导入所有模块" : "Import all modules with One click";

        public static string ReImportAllModuleInfo => !IsEN ? "重新导入已经导入的模块" : "Re-import modules that have already been imported";

        public static string ImportClickInfo => !IsEN ? "反导已经导入的模块" : "Import already imported modules";
        //public string 

    }
}
