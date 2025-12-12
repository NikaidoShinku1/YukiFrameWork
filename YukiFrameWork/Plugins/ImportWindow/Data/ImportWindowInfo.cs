
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
        public static string GetModuleInfo(string key)
        {
            if (key == "ActionKit")
                return ActionKitInfo;
            else if (key == "Bezier")
                return BezierInfo;
            else if (key == "SaveTool")
                return SaveToolInfo;
            else if (key == "StateManager")
                return StateManagerInfo;
            else if (key == "IOCContainer")
                return IOCInfo;
            else if (key == "DiaLogKit")
                return DiaLogInfo;
            else if (key == "BuffKit")
                return BuffKitInfo;
            else if (key == "SkillKit")
                return SkillInfo;
            else if (key == "UI")
                return UIInfo;
            else if (key == "Audio")
                return AudioInfo;
            else if (key == "ItemKit")
                return KnapsackInfo;
            else if (key == "NavMeshPlus")
                return NavMeshPlusInfo;
            else if (key == "MissionKit")
                return MissionInfo;
            else if (key == "BehaviourTree")
                return BehaviourTreeInfo;
            else if (key == "Hyclr")
                return ImportHyCLR;
            else if (key == "StateMachine")
                return StateMachineInfo;
            else if (key == "EntitiesExtension")
                return EntitiesInfo;
            else if (key == "InputSystemExtension")
                return InputKitExtension;
            else if (key == "UINavigation")
                return UINAvigation;
            else if (key == "Localization")
                return LocalizationInfo;
            else if (key == "EquipmentKit")
                return EquipmentKitInfo;
            return default;
        }

        public static string UINAvigation => "UIKit拓展输入导航(该包依赖Unity新输入系统InputSystem、框架的InputSystemExtension拓展模块、与UIKit模块)";

        public static string EntitiesInfo => !IsEN ? "Unity Entities拓展模块。该模块拓展Unity ECS与Mono的交互桥梁。在原生Scene即可完美发挥ECS。支持热更新" +
            "\n拓展方式1：以<color=cyan>GameObject</color>为主导形式实体。除渲染外，保留<color=cyan>GameObject</color>原生所有的使用方式，直接应用主场景使用该模块可自定义对于<color=cyan>GameObject</color>的渲染。" +
            "\n拓展方式1相对于原生Mono，在渲染个体转换与ECS代码高速优化的基础上，使用Mono可享受三倍提升，实现以万计数同屏。" +
            "\n\n拓展方式2：同时支持以封装<color=cyan>EntitiesGraphics</color>渲染的方式。在原生<color=yellow>Scene</color>下，代替<color=yellow>SubScene</color>以纯粹实体渲染以发挥出ECS全部的效能(非<color=cyan>GameObject</color>个体转换) 可实现百万同屏。\n\n<color=yellow>注意:你的Unity版本必须是支持dots的</color>" : "Unity Entities extension module. This module expands the interaction bridge between Unity ECS and Mono. The dominant formal entity is GameObject. There is no need to create a new ECS SubScene; the main scene can be directly applied for use. Design humanized while optimizing performance.";

        public static string SerializationInfo => !IsEN ? "框架序列化工具,可以将类转换成Json,Xml,Bytes文件流,集成ExcelToJson转换插件" : "Framework serialization tool, can convert classes to Json,Xml,Bytes file streams, integrated ExcelToJson conversion plug-in";

        public static string ActionKitInfo => !IsEN ? "ActionKit动作时序套件,可以完成例如定时、队列、事件循环、强化Update、等待帧回调等强化功能实现" : "ActionKit Action timing suite, you can complete such as timing, queue, event loop, enhanced Update, wait for frame callback and other enhanced functions";

        public static string BezierInfo => !IsEN ? "贝塞尔曲线计算公式管理类" : "Bessel curve calculation formula management class";

        public static string EquipmentKitInfo => "框架通用式装备系统,可定义装备的配置与生命周期";

        public static string LocalizationInfo => !IsEN ? "框架本地化多语言套件" : "Framework LocalizationKit";

        public static string InputKitExtension => !IsEN ? "框架输入系统集成拓展(该模块完全依赖InputSystem新输入系统)" : "Framework input system integration Extension (This module relies entirely on the InputSystem new input system. And after importing the module, UIKit automatically has a new expanded UI navigation function!)";

        public static string GuideInfo => !IsEN ? "通用引导模块，可以实现简单的引导系统(暂时不完善，不推荐使用)" : "Universal boot module can realize simple boot system";

        public static string SaveToolInfo => !IsEN ? "框架存档工具，可用于数据的保存以及持久化" : "Framework SaveTool, which can be used for data preservation and persistence";

        public static string MissionInfo => !IsEN ? "框架任务系统，基础通用式任务系统，拥有一键式分组管理以及生命周期，自定义加载，可自由拓展" : "Frame Mission Kit, basic universal Mission system, with one-click group management and life cycle, custom loading, can be freely expanded";

        public static string BehaviourTreeInfo => !IsEN ? "框架行为树模块，可以实现对复杂AI的设计，复合节点的打断以及Json序列化" : "The behavior tree module can realize the design of complex AI, the interruption of composite nodes and the Json serialization";

        public static string StateMachineInfo => !IsEN ? "纯粹有限状态机模块,可以实现对于状态的管理等操作,简单好上手。编辑器可视化" : "Pure finite state machine module, can achieve state management and other operations, simple and easy to get started. Editor visualization";

        public static string StateManagerInfo => !IsEN ? "仅为基础动作游戏设计打造的状态机(非必要则选择StateMachine模块)，编辑器可视化" : "A state machine built for basic action game design only (select the StateMachine module if not necessary), visualized by the editor";

        public static string NavMeshPlusInfo => !IsEN ? "Unity 导航网格plus集成，该插件可以让NavMeshAgent支持2d TileMap的使用(限2021以上使用)" : "Unity Navigation Grid plus integration, which enables NavMeshAgent to support the use of 2d TileMap";

        public static string IOCInfo => !IsEN ? "IOC容器,可以完整实现控制反转的结构思想,DI注入等操作" : "IOC container can realize the structure idea of inversion of control,DI injection and other operations";

        public static string DiaLogInfo => !IsEN ? "对话系统，具有完整编辑器拓展可支持本地化配置的对话系统。可用于视觉小说等需要对话的项目(适合2021以上版本使用)" : "Dialog system, a dialog system with a full editor extension to support localized configuration. It can be used for projects that require dialogue, such as visual novels((Suitable for 2021 and above))";

        public static string BuffKitInfo => !IsEN ? "Buff系统，基于MVC打造的基础通用式Buff系统，拥有一键式UI同步以及完整的生命周期管理" : "Buff system, a basic universal Buff system based on MVC, with one-click UI synchronization and complete lifecycle management";

        public static string UIInfo => !IsEN ? "UI框架,可分层式对UI统一管理" : "UI framework, can be hierarchical UI unified management ";

        public static string AudioInfo => !IsEN ? "AudioKit声音管理条件,可以对声音进行集成式管理" : "AudioKit sound management conditions for integrated sound management";

        public static string KnapsackInfo => !IsEN ? "背包系统,基础通用式背包系统,拥有一键式分组管理以及独立生命周期，自定义加载以及Excel配表,可自由拓展" : "Backpack system, basic universal backpack system, with one-click group management and independent life cycle, custom loading and Excel table, ";

        public static string SkillInfo => !IsEN ? "纯数据技能系统，基于MVC打造的基础通用式技能系统。" : "Pure data skill system, based on MVC to build a basic universal skill system.";
        #endregion

        public static string ImportAllModuleInfo => !IsEN ? "一键导入所有模块" : "Import all modules";

        public static string ReImportAllModuleInfo => !IsEN ? "重新导入已经导入的模块" : "Re-import modules that have already been imported";

        public static string ImportClickInfo => !IsEN ? "反导已经导入的模块" : "Import already imported modules";

        public static string ImportHyCLR => !IsEN ? "导入HYCLR热更新模块" : "Import HYCLR modules";

        //public string 

    }
}
