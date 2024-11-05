
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

        public static string GuideInfo => !IsEN ? "通用引导模块，可以实现简单的引导系统(暂时不完善，不推荐使用)" : "Universal boot module can realize simple boot system";

        public static string SaveToolInfo => !IsEN ? "框架存档工具，可用于数据的保存以及持久化" : "Framework SaveTool, which can be used for data preservation and persistence";

        public static string MissionInfo => !IsEN ? "框架任务系统，基础通用式任务系统，拥有一键式分组管理以及生命周期，自定义加载，可自由拓展" : "Frame Mission Kit, basic universal Mission system, with one-click group management and life cycle, custom loading, can be freely expanded";

        public static string StateMechineInfo => !IsEN ? "(旧)状态机模块,可以实现对于状态的管理等操作,编辑器可视化(已停止更新,除非项目已经使用旧状态机开发，否则推荐使用新状态机StateManager模块!)" : "State machine module, can be implemented for state management and other operations, or basic action game design, editor visualization(Updates have stopped, and unless the project has been developed using the old state machine, the new StateManager module is recommended!)";

        public static string StateManagerInfo => !IsEN ? "全新状态机模块，在旧状态机之上，全面支持对于动作状态的设计，可完美兼容实现基础动作游戏设计，编辑器可视化" : "The new state machine module, on top of the old state machine, fully supports the design of the action state, which can be perfectly compatible with the basic action game design and editor visualization";

        public static string NavMeshPlusInfo => !IsEN ? "Unity 导航网格plus集成，该插件可以让NavMeshAgent支持2d TileMap的使用(限2021以上使用)" : "Unity Navigation Grid plus integration, which enables NavMeshAgent to support the use of 2d TileMap";

        public static string IOCInfo => !IsEN ? "IOC容器,可以完整实现控制反转的结构思想,DI注入等操作" : "IOC container can realize the structure idea of inversion of control,DI injection and other operations";

        public static string DiaLogInfo => !IsEN ? "对话系统，具有完整编辑器拓展可支持本地化配置的对话系统。可用于视觉小说等需要对话的项目(适合2021以上版本使用)" : "Dialog system, a dialog system with a full editor extension to support localized configuration. It can be used for projects that require dialogue, such as visual novels((Suitable for 2021 and above))";

        public static string BuffKitInfo => !IsEN ? "Buff系统，基于MVC打造的基础通用式Buff系统，拥有一键式UI同步以及完整的生命周期管理" : "Buff system, a basic universal Buff system based on MVC, with one-click UI synchronization and complete lifecycle management";

        public static string UIInfo => !IsEN ? "UI框架,可分层式对UI统一管理" : "UI framework, can be hierarchical UI unified management ";

        public static string AudioInfo => !IsEN ? "AudioKit声音管理条件,可以对声音进行集成式管理" : "AudioKit sound management conditions for integrated sound management";

        public static string KnapsackInfo => !IsEN ? "背包系统,基础通用式背包系统,拥有一键式分组管理以及独立生命周期，自定义加载以及Excel配表,可自由拓展" : "Backpack system, basic universal backpack system, with one-click group management and independent life cycle, custom loading and Excel table, ";

        public static string SkillInfo => !IsEN ? "纯数据技能系统，基于MVC打造的基础通用式技能系统。(测试阶段暂没有文档)" : "Pure data skill system, based on MVC to build a basic universal skill system. (Test phase)";
        #endregion

        public static string ImportAllModuleInfo => !IsEN ? "一键导入所有模块" : "Import all modules";

        public static string ReImportAllModuleInfo => !IsEN ? "重新导入已经导入的模块" : "Re-import modules that have already been imported";

        public static string ImportClickInfo => !IsEN ? "反导已经导入的模块" : "Import already imported modules";
        //public string 

    }
}
