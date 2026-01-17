#### 框架更新日志、

V1.61.1 框架新增MissionTree模块，图结构任务树，废弃旧版MissionKit，优化细节，Excel转SO全局配置可支持Sprite导出路径。优化部分细节。同步更新XFABManager

V1.60.2 UIColor优化细节，UINavigation模块优化细节

V1.60.1 新增IDynamicBuilder的方法注入功能，支持DynamicValue特性，新增DynamicRegulation特性支持自定义注入

V1.59.11 优化LocalizationKit的序列化器在初始化时可进行添加。

V1.59.10 更新DynamicValue特性的可视化支持，仅当对应字段可被序列化时生效

V1.59.9 公开DynamicValue的注入方法，StateMachine模块支持动态字段注入

V1.59.8 ItemKit优化细节，EquipmentKit新增检查装备方法

V1.59.7 修复DiaLogKit通过Start调用Move时抛出异常的问题，优化BuffKit细节，EquipmentKit新增API

V1.59.6 修复UINavigation模块中偶现触发事件报空的问题

V1.59.5 UIKit修复不缓存面板打开后在结束运行时出现的已销毁的异常。同时支持动态字段特性

V1.59.4 BuffKit优化细节部分。明确获取效果的API职责

V1.59.3 DynamicValueFromScene特性支持Path与Tag的搜索方式

V1.59.2 框架进一步强化DynamicValue，改注入为IDynamicMonoBehaviour接口，自由性拓展，背包系统新增点击事件

V1.59.1 框架新增DynamicValue特性可自动为字段赋值(仅新增DynamicViewController有效)，优化文档。

V1.58.5 优化SkillKit的同时释放标识配置。

V1.58.4 优化ActionKit细节

V1.58.3 修复BuffKit GetEffectsByType方法内部逻辑问题

V1.58.2 BuffKit将参数移动到效果中。SkillKit优化细节

V1.58.1 优化API兼容Unity6000.2以上版本，框架包新增4.0版本的OdinInspector包，6000.2版本以上都必须用该包。

V1.57.2 修复SaveTool出现打包与编辑器存档读取不统一的问题

V1.57.1 新增事件可视化注册，更新文档，Texture2D转换AnimationClip优化细节，SaveTool重构初始化逻辑

V1.56.3 修复EquipmentKitExcel导入后编辑器没刷新的问题

V1.56.2 修复SkillKit、BuffKit、EquipmentKit可以添加抽象类型的问题

V1.56.1 框架新增EquipmentKit装备系统，优化编辑器细节

V1.55.6 修复StateMachine模块创建脚本引发参数异常的问题

V1.55.5 AudioKit新增AudioVolumeScale控制全层级的音量缩放(总音量值),以及不同层级分组的统一音频缩放,更新文档，优化部分代码细节

V1.55.4 图集转AnimationClip窗口优化，可以进行拖拽指定Sprite到预览图上

V1.55.3 修复StateMachine模块中AutoSwitch不开启也会强制切换没有条件的状态的bug

V1.55.2 修复SkillKit模块中查询技能状态冷却返回Success的bug，优化StateMachine模块细节，优化图集转换AnimationClip配置窗口

V1.55.1 重构对于模式为Multiple的Texture转换AnimationCliip的功能并解除对窗口的屏蔽，现在可以正常使用

补充更新XFABManager

V1.54.1 SkillKit新增配置自定义参数，修复SkillController无法进行动态自定义数据的问题，BuffKit新增自定义配置参数，MissionKit优化细节

V1.53.15 优化ActionKit细节，优化SkillKit与BuffKit可以在配置中打开已绑定的单独的控制器脚本

V1.53.14 UINavigation模块修复面板切换时可能会多次触发自定义按键的问题，StateMachine模块优化切换状态的事件参数，修复生成代码时命名空间无效的问题

V1.53.13 UINavigation模块新增对SelectableEvent组件的自定义按下按键适配

V1.53.12 UINavigation模块的手柄导航拓展适配添加默认键盘导航处理。

V1.53.11 UINavigation模块优化代码细节，新增部分API，UIColor小工具脚本优化

V1.53.10 BindableProperty新增代码模式匹配，BuffKit新增查询BuffController的API
 
V1.53.9 对象池优化细节，作为全局类，优化PoolInfo的调试信息，ItemKit新增物品移除插槽的API，优化细节

V1.53.8 修复BuffKit移除Buff提示无法操作迭代器的bug

V1.53.7 UiKit修复拖拽只能在PC端使用的问题，修复持久化BindableProperty在安卓端偶现失效的问题

V1.53.6 UIKit新增PreLoadPanelAsync、PreLoadPanel、IsPanelExist方法，更新文档

V1.53.5 UIKit新增可视化生命周期事件注册， RandomKit优化细节

V1.53.4 BindablePropertyStruct新增默认构造函数，屏蔽Texture2d转AnimationClip窗口

V1.53.3 新增Toggle的纯净事件监听，新增XFABManager的LoadSceneRequest的await语法糖

V1.53.2 同步更新XFABManager

V1.53.1修复BuffKit与SkillKit中偶现图片添加失败的问题，DiaLogKit重构升级2.0版本，更加自由通用

V1.52.1 SkillKit底层重构进入2.0版本，BuffKit API优化为拓展方法。文档更新

V1.51.2 AudioKit修复当使用Sound层播放音频时，在上一个音频结束播放后下一个音频播放结束时报错的问题，优化部分代码细节，新增API

V1.51.1 BuffKit迎来2.0版本重构，现在更加好用且自由。优化技能系统，添加开始冷却的事件以及改变控制绑定的方式为配置

V1.50.9 StateManager模块全局启动支持传递架构对象。

V1.50.8 同步更新XFABManager

V1.50.7 新增IExcelReSyncScriptableObject反同步Excel转换接口。可处理在导出前执行的逻辑。详情查阅文档。

V1.50.6 修复UIKit中BasePanel勾选可拖拽后当Canvas存在WorldCamera而出现拖拽异常的问题

V1.50.5 同步更新XFABManager，优化部分编辑器窗口细节

V1.50.4 修复InputSystemExtension分组切换InputAction重复注册事件的问题，UGUI部分组件添加纯净事件注册拓展方法。SceneTool新增重载，优化部分代码细节

V1.50.3 修复AudioKit 异步播放失败的bug

V1.50.2 更新AudioGroup注释并优化细节，优化ItemKit细节

V1.50.1 AudioKit更新到2.0版本，更灵活多变的分组。更新AudioKit文档

V1.49.2 BuffKit模块BuffHandler的AddBuffer方法支持返回值。

V1.49.1 框架新增AnimationClip-Sprite转换工具,优化部分代码细节。

V1.48.3 修复在命令中通过架构调用会出现空指针异常的问题

V1.48.2 修复StateMachine会出现状态类型丢失的问题。

V1.48.1 命令系统升级，支持对命令执行的具体周期进行接口自定义，对于命令(对象)本身，支持对命令的撤销功能(新增IUndoCommand接口)，新增命令标签属性Label，可自定义命令的介绍

V1.47.8 修复DIaLogKit在重新获取控制器时出现已经拥有标识而无法获取的问题

V1.47.7 框架启动性能优化，高级代码生成解决生成架构代码少实现方法的问题

V1.47.6 修复StateMachine模块切换状态会出现进入两次的问题，状态新增在退出与进入时可用的上一个状态与下一个状态的属性。API补充拓展

V1.47.5 BuffKit与SkillKit移除控制器接口，直接使用基类Controller即可，使用方式不变。

V1.47.4 修复使用async await时使用Token在结束运行的时候抛出异常的问题，修复DiaLogKit中当uiOption置于root根节点下时会出现销毁丢失的问题。优化部分代码细节

V1.47.3 修复ItemKit在进行持久化读取的时候ui刷新报空的问题。改良SaveTool存档工具的使用，文档更新。

V1.47.2 高级代码生成新增Command的生成

V1.47.1 本地化套件升级为2.0版本，该模块不再内置，转移到导入模块中，如项目需要本地化模块。则在导入窗口导入即可。

V1.46.5 行为树BehaviourTree模块 修复初始化行为节点状态出现空指针异常的问题

V1.46.4 ItemKit新增拖拽回收可选，关闭后当没有拖入插槽时不会自动丢弃。新增UISlot视觉接口IUISlotVisual,可通过该接口自由定制同步的UI组件，如Tmp支持。更新文档

V1.46.3 修复StateMachine模块中 AnyState条件判断成功后会循环进入的问题

V1.46.2 输入系统拓展，为InputAction的动态绑定添加Cancel回调

V1.46.1简单对象池回收一定触发回调。输入拓展更新3.0版本，直接支持InputAction的改键操作，修复ActionKit Lerp无法在Repeat使用的问题  优化部分代码细节。MissionKit支持直接调用完成任务/失败。UI解决编辑器结束运行的抽象丢失报错。

V1.45.4 UI模块新增IUIAnimation动画模式接口可使用。优化部分代码细节

V1.45.3 移除已不再使用的插件，优化部分代码细节

V1.45.2 框架输入系统拓展InputSystemExtension模块新增事件响应分组对InputAction配置的支持。优化部分代码/文档细节 修复ActionKit与UIKit在编辑器模式退出运行时偶现报错的问题

V1.45.1 UIKit新增关闭指定层级下所有面板方法ClosePanelByLevel,优化更新UIKit的文档，移除ClosePanel在查找失败后仍会强制关闭层级顶层面板的功能，新增更新日志窗口、为导入窗口添加版本检测。

V1.44.3 同步更新XFABManager，修复背包套件在调用读取后插槽数量翻倍的问题。

V1.44.2 输入系统拓展模块新增事件分组注册器。将按键归类在一个分组中，可以往该分组注册事件。自动触发

V1.44.1 框架新增新输入系统InputSystem的全局拓展模块。新增依赖UIKit的UINavigation拓展模块。比Selectable组件更加好用的UI全设备导航功能！

V1.43.2 优化编辑器，基础设置仅保存全局命名空间设置，修复DeepSeek生成组件窗口在从未设置apiKey时抛出异常的问题 当误删框架全局配置时。新增LocalConfiguration修复功能。

V1.43.1 框架新增DeepSeek AI代码生成模块。优化红点系统细节。

V1.42.12 修复状态机模块中，当有复数条件的情况下没满足条件也能切换状态的问题。

V1.42.11 补充反射拓展API的文档。修复Excel转So无法导入数据应用在基类字段/属性的问题。

V1.42.10 修复SkillKit不受时间影响的情况下仍计算释放时间的问题。

V1.42.9 ItemKit UISlot组件开放权限，可继承UISlot自定义更新。

V1.42.8 Scene拓展方法新增FindRootGameObjects，当场景存在多个同名对象。可以使用该API，修复Excel转So后直接关闭项目数据没有保存的问题。

V1.42.7 ActionKit支持OnDrawGizmosSelected方法的拓展。ViewController支持OnSceneGUI()方法的重写，重写该方法即不需要自己写Editor拓展类。

V1.42.6 架构Architecture支持OnCompleted方法，在架构准备完成后触发

V1.42.5 修复AudioKit 使用AudioInfo播放音频切换时使用onEndCallBack概率出现卡机的问题。优化全局对象池，更新全局对象池文档。

V1.42.4 优化状态机模块的参数API，新增Button的纯净监听拓展方法(button.AddListenerPure)

V1.42.3 优化LocalizationKit的配置标识方式

V1.42.2 优化SkillKit与BuffKit的接口限制。以自定的形式绘制配表的Icon

V1.42.1 MissionKit、ItemKit、BuffKit、SkillKit 文档更新优化。编辑器全面重做。代码优化。DiaLogKit优化部分细节。

V1.41.1 移除1.25以下更新信息。本地化套件全面升级。优化文档

V1.40.3 修复Excel转So序列化窗口在2020版本因语法糖不支持报错的问题。优化状态机细节。

V1.40.2 原Excel转So仅对自定的序列化类生效。修复在配置中不对Int、float等基本结构类型生效的Bug(如int[])

V1.40.1 移除老版本有限状态机，新增全新纯粹有限状态机。文档已在官网标注。框架工具导入窗口样式优化。打开文档链接更醒目。同步更新XFABManager

其余均为不稳定版功能的修正，不以浏览。