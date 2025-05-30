#### 框架更新日志

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

V1.39.1 框架新增Excel-So转换工具。为MissionKit，ItemKit，BuffKit，SkillKit，DiaLogKit完成Excel转换适配

V1.38.5 框架新增BindablePropertyStruct属性绑定类，BindableProperty的少gc消耗版本，仅为结构体打造

V1.38.4 新增UI背景分辨率适组件BackgroundAdaptation 边界适配分辨率组件BorderAdaptation，修复ItemKit OnSlotSelect方法委托不正确的问题

V1.38.3 优化ItemKit，修复读取存档还存在上一个背包数据，添加部分API

V1.38.2 UiKit新增全层级判断面板是否开启API，新增Type的几个获取的拓展方法，ActionKit新增Lerp01方法

V1.38.1 框架新增红点系统。在主页更新文档示例链接。优化细节

V1.37.9 优化编辑器窗口细节。导入窗口新增未公开ECS包导入界面。同步XFABManager更新

V1.37.8 UIKit细节优化。

V1.37.7 AudioKit新增判断播放层级是否处于闲置状态的API。优化部分细节。

V1.37.6 ActionKit新增Lerp插值方法

V1.37.5 修复XFABManager通过ab包加载资源数据不正确的问题。优化GameObjectLoader预加载。优化架构初始化异步执行。修复对象池套件在池子满的情况下无法取出池内对象的问题。

V1.37.4 统一UI模块释放操作。只需要调用UIKit.Release方法即可

V1.37.3 修复UI模块调用UIManager.Instance.Dispose方法后重新初始化层级丢失的问题

V1.37.2 优化AudioKit资源池释放管理。优化UIKit的释放面板方法。新增BindableProperty拓展方法，修复架构初始化时机不正确的问题。

V1.37.1 框架资源加载器新增UnLoad释放方法。同步更新所有的模块。

V1.36.1 架构为Model/System层新增IAsync_InitModule接口，这两个层级继承该接口时，在准备架构时可以自动进行异步的初始化行为。优化细节

V1.35.7 新增集合拓展方法RandomEnumerable与RandomChoose方法，MonoHelper类新增ApplicationQuit_AddListener方法。优化细节

V1.35.6 修复UIKit自带的拖拽功能会强制令鼠标位置为面板正中心的问题。

V1.35.5 修复状态机模块切换自身动作时的异常问题。修正Animator作为状态机动作组件时，偶现鬼畜问题。

V1.35.4 优化任务系统，移除过时API。示例包问题修复。

V1.35.3 UIKit新增OnPreInit方法。作为面板的预初始化方法。会在OnInit之前调用。通过OpenPanel打开可以传递参数!

V1.35.2 优化拓展框架标准容器类Container

V1.35.1 优化动作状态机模块。该版本开始移除Hybridclr，经考虑，该插件应与项目需求本身为准。与框架互不冲突也不依赖，应自行导入。

V1.34.1 同步更新XFABManager，Hybridclr，修复ActionKit在回收时没有清理子节点的问题。OdinInspector的导入改为UnityPackage包

V1.33.5 修复ActionKit中Update无法手动打断的问题。修复任务系统在刷新UI时的执行顺序问题。优化细节。

V1.33.4 优化任务系统。

V1.33.3 修复行为树模块编辑器在Unity6显示异常的问题。

V1.33.2 AudioKit模块新增AudioVolumeListener组件，可在Inspector监听音量的变更事件。

V1.33.1优化行为树模块，修复打断导致状态异常的Bug，新增行为树模块文档，对话系统优化，运行时使用完全clone出来的配置进行处理。

V1.32.1 重构框架的导入窗口。单独分离不与基本设置窗口一起。同步更新XFABManager，改变华佗的导入方式为内置。内部进行华佗的更新周期同步，不需要自己手动导入git。

V1.31.1 (beta测试版)框架新增行为树模块，高级代码生成设置新增So的路径生成。同步封装属性到接口。优化部分代码细节

V1.30.5 优化对话系统细节。

V1.30.4 优化任务系统的部分细节。优化代码生成细节。修复XFABManager资源加载时，派生类不能统一加载基类的问题。

V1.30.3 高级代码生成新增字段/方法注释，为预览视图优化，贴合真实编译样式

V1.30.2 为高级代码生成新增文件夹分离功能。优化部分代码细节

V1.30.1 框架新增高级代码生成设置。更完善的代码生成，规则生成以及接口处理。可配置字段方法一键式生成，优化细节

V1.29.3 为ViewController新增文件夹分离生成功能。可以在创建脚本时自动创建一个文件夹单独管理脚本。UnLoad组件新增回调

V1.29.2 公开架构初始化接口。优化部分代码细节，Odin插件新增Odin Validator。

V1.29.1 框架新增任务系统。同步更新XFABManager，优化细节并支持GameObjectLoader预加载池。

V1.28.15 优化背包系统，支持物品的插入与通过下标清空物品。优化细节，框架提供YMonoBehaviour基类支持可视化Awake注册。新增UIColor组件可以注册对Color的修改事件。

V1.28.14 对话系统自带的UIDiaLog组件优化使用，仅作为自动对话以及打字机注册使用。优化部分细节。

V1.28.13 优化对话系统，支持导入Json配表时，通过连接id

V1.28.12 背包系统优化，物品类型改为字符串判断，编辑器选择优化。

V1.28.11 AudioKit模块为AudioInfo添加3dSoundSetting设置，新增音效层级的暂停与恢复。修复状态机模块中，当多重继承时，不重写FixedUpdate与LateUpdate不执行的问题。

V1.28.10 Buff模块-->BuffHandler新增获取BuffController的方法，对话系统优化，对话树的运行状态数据移动到控制器中

V1.28.9 修复状态机模块，默认状态的id不为0时导致的状态执行错误

V1.28.8 架构拓展容器模块，可进行控制反转。自动化对MonoBehaviour生命周期管理，详情可查看架构文档中的容器文档跳转。

V1.28.7 解锁BasePanel与ViewController组件基类的代码绑定功能，现在可以提前进行代码绑定，在点击生成脚本的按钮后 自动进行绑定

V1.28.6 修复AudioKit在切换场景时，因对象丢失导致访问AudioSource异常的问题，新增Vector3的拓展方法 SetX等单个向量修改的API。优化细节

V1.28.5 更新不同的模块引导需要的编译器警告，优化部分代码细节，

V1.28.4 优化代码细节，修复编译时事件注入进程偶现阻塞的问题

V1.28.3 动作状态机支持Animator的混合树。优化代码细节

V1.28.2 新增协程的警告提示，修复在抽象类也会弹出层级警告的问题。

V1.28.1 重构事件系统，从该版本开始全面全自动注入，细节通过官网查看消息广播模块的文档，架构事件与事件系统整合，现在没有区别。新增部分情况下的代码警告与提示。优化代码体验以及规范。优化细节。

V1.27.1 新增NavMeshPlus插件作为工具使用，可以在配置窗口导入。

V1.26.4 优化AudioKit，新增AudioClip的直接使用API，新增列表的排序拓展，不仅限于List才能进行Sort的调用。修复UIKit调用GetPanel无法找到Top层的面板以及新增遍历全层级找到面板的API

V1.26.3 优化部分代码细节

V1.26.2 优化SkillKit与BuffKit，移除对UI的适配，添加新的委托进行外部的逻辑注册。更新BuffKit的文档，AudioKit中AudioInfo组件新增Enable时播放

V1.26.1 框架新增RandomKit类，拓展封装UnityEngine的Random类作为随机套件使用。优化细节，配表自动反序列化修复数组导致异常的问题

V1.25.8 修复全局对象池如果预加载的对象的构造函数是私有化导致异常的问题。优化SkillKit与BuffKit的编辑器性能

V1.25.7 优化全局对象池，新增池子信息查询以及手动清理的API拓展，文档更新，可以拿到指定池子的活跃信息。

V1.25.6 优化ItemKit，BuffKit与SkillKit，将模块配表Plus全集中于本地配置窗口中，优化编辑器使用更加快捷方便。

V1.25.5 新增Model层自动反序列化特性，架构文档更新可查看，优化细节。

V1.25.4 优化配表的构建逻辑，使用更人性化，支持在架构中通过TableConfig访问配表数据，修复2021以下版本快捷编辑器拓展会异常的问题。新增真实堆栈跟踪，架构准备以及LogKit.Exception都可以使用。

V1.25.3 优化状态机模块，添加可修改动画方法，优化动作脚本的粒子特效池使用，优化状态机生成脚本的路径持久化，优化可视化场景选择，运行时不可修改

V1.25.2 框架新增快捷视图拓展。

V1.25.1 优化架构生命周期流程，现在架构的初始化要比自动化注册更早，可以提前处理一些设置。添加隐藏拓展层级Config层，更新架构文档

其余均为不稳定版功能的修正，不以浏览。