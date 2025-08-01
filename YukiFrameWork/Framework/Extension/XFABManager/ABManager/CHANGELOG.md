## version 1.18.2 更新内容

1. 修复无法创建资源模块的问题!


## version 1.18.1 更新内容

1. 优化部分编辑器代码!
2. ImageLoader 添加 color 属性!

## version 1.18.0 更新内容

1. 优化创建资源模块编辑器界面！
2. 删除依赖项目功能，这个功能完全没有必要!

## version 1.17.9 更新内容

1. 把同步加载场景的方法标记为过时, Unity提供的加载场景的方法在编辑器模式和运行时模式表现不一致，不推荐使用，建议使用异步加载代替!

## version 1.17.8 更新内容

1. 添加属性 GameObjectLoader.CloseTime, 可通过该属性设置对象池不再使用后多长时间会被关闭!
2. 添加属性 GameObjectLoader.DetectionInterval, 可通过该属性设置对象池检测时间间隔!

## version 1.17.7 更新内容

1. 添加属性 GameObjectLoader.DestroyTime, 可通过该属性设置游戏物体回收后多长时间销毁！

## version 1.17.6 更新内容

1. 修复编辑器模式 LoadAllAssets 资源加载失败的问题!

## version 1.17.5 更新内容

1. 优化编辑器拖拽列表!

## version 1.17.4 更新内容

1. 添加异步请求中断方法!

## version 1.17.3 更新内容

1. 添加导出资源包的功能!

## version 1.17.2 更新内容

1. 优化GameObjectLoader异步加载逻辑!

## version 1.17.1 更新内容

1. 删除测试日志!

## version 1.17.0 更新内容

1. 优化编辑器资源加载逻辑!

## version 1.16.9 更新内容

1. 优化编辑器AssetBundle窗口资源移除逻辑!
2. 添加获取本地资源版本号的方法!
3. 优化ImageLoader代码!

## version 1.16.8 更新内容

1. 优化编辑器窗口AssetBundle列表拖拽警告输出!

## version 1.16.7 更新内容

1. 优化编辑器窗口AssetBundle列表拖拽效果!

## version 1.16.6 更新内容

1. 优化编辑器可拖拽的TreeView的数据刷新逻辑!

## version 1.16.5 更新内容

1. 优化编辑器界面,AssetBundle列表拖拽逻辑!

## version 1.16.4 更新内容

1. 优化编辑器界面,AssetBundle列表拖拽逻辑!

## version 1.16.3 更新内容

1. 优化编辑器GUI,添加AssetBundle列表拖拽调整顺序的功能!

## version 1.16.2 更新内容

1. 调整预加载资源的方式为同步!

## version 1.16.1 更新内容

1. 优化AssetBundle加载逻辑!
2. 优化资源检测更新代码!

## version 1.16.0 更新内容

1. 优化AssetBundle加载逻辑!
2. 优化 GameObjectLoader 函数重载!
3. 优化资源检测逻辑!

## version 1.15.9 更新内容

1. 修复同时下载多个大文件时导致的文件损坏!
2. 删除 GameObjectLoader 加载游戏物体默认修改位置和旋转的代码，提供对应的重载函数!

## version 1.15.8 更新内容

1. 修复检测资源更新报错!

## version 1.15.7 更新内容

1. 优化 GameObjectLoader 回收资源逻辑!

## version 1.15.6 更新内容

1. 优化 GameObjectLoader 加载资源 和 预加载资源逻辑!

## version 1.15.5 更新内容

1. 优化资源加载失败的原因处理!

## version 1.15.4 更新内容

1. 优化 GameObjectLoader 回收 Transform 的方法!

## version 1.15.3 更新内容

1. 优化协程启动器!

## version 1.15.2 更新内容

1. 优化复制文件夹的方法!

## version 1.15.1 更新内容

1. 优化本地资源管理!

## version 1.15.0 更新内容

1. 优化编辑器加载资源逻辑!

## version 1.14.9 更新内容

1. 完善 ImageLoader !

## version 1.14.8 更新内容

1. 完善 ImageLoader !

## version 1.14.7 更新内容

1. 调整GameObjectLoader销毁的超时时间

## version 1.14.6 更新内容

1. 优化资源加载 
2. 完善GameObjectLoader.UnLoad 方法

## version 1.14.5 更新内容

1. 优化 GameObjectLoader 加载 和 卸载 对象的方法!


## version 1.14.4 更新内容

1. 优化 GameObjectLoader 加载对象逻辑, 加载出来的对象自动放入当前活跃的场景!

## version 1.14.3 更新内容

1.  删除无用的命名空间引用!

## version 1.14.2 更新内容

1.  添加Unload脚本事件!

## version 1.14.1 更新内容

1.  添加GameObjectLoader预加载资源功能!

## version 1.14.0 更新内容

1. 删除部分类型加载资源的警告提示!

## version 1.13.9 更新内容

1. 修复 MacOS 系统编辑器模式创建项目报错!
2. 优化 MacOS 系统编辑器模式修改bundle名称 显示不清晰!

## version 1.13.8 更新内容

1. 优化异步计算md5逻辑!

## version 1.13.7 更新内容

1. 修复GameObjectLoader异步加载资源没有进度的问题!

## version 1.13.6 更新内容

1. 优化GameObjectLoader加载逻辑!

## version 1.13.5 更新内容

1. 优化场景进度!

## version 1.13.4 更新内容

1. 优化AssetBundle异步加载速度!

## version 1.13.3 更新内容

1. 优化GameObjectLoader加载逻辑!

## version 1.13.2 更新内容

1. 删除 Texture 和 Texture2D 类型资源加载的警告提示!

## version 1.13.1 更新内容

1. 添加脚本Unload，可用于回收通过GameObjectLoader加载的游戏物体！

## version 1.13.0 更新内容

1. 优化 EditorApplicationTool 逻辑!

## version 1.12.9 更新内容

1. 修复ImageLoader在WebGL平台加载网络图片失败!

## version 1.12.8 更新内容

1. 优化GameObjectLoader.UnLoad方法!

## version 1.12.7 更新内容

1. 添加方法CustomAsyncOperation.RemoveAllCompleteEvent!

## version 1.12.6 更新内容

1. 优化拓展方法IsDestroy!

## version 1.12.5 更新内容

1. 调整协程启动器初始化选项

## version 1.12.4 更新内容

1. 修复编辑器模式下,关闭重新编译程序集的选项时,导致的协程启动器无法正常工作的问题!

## version 1.12.3 更新内容

1. 优化异步加载场景回调,删除进度渐变逻辑!

## version 1.12.2 更新内容

1. 优化ImageLoader加载大图片卡顿的问题!

## version 1.12.1 更新内容

1. 修复编辑器模式同步加载场景 AssetBundleManager.LoadScene失败的问题!

## version 1.12.0 更新内容

1. 优化场景资源卸载
2. 优化异步加载场景逻辑

## version 1.11.9 更新内容

1. 优化计算依赖资源逻辑，添加单个AssetBundle资源数量判断，防止包内资源数量过多!
2. 优化异步加载场景逻辑，解决卡顿和加载进度不平滑的问题!

## version 1.11.8 更新内容

1. 优化编辑器样式显示!


## version 1.11.7 更新内容

1. 优化 GameObjectLoader 代码!

## version 1.11.6 更新内容

1. 优化 AssetBundle 文件名称，防止不同模块出现相同名称AssetBundle的文件!
2. 优化Sprites文件打包名称映射!

## version 1.11.4 更新内容

1. 优化 ImageLoader , 添加 Refresh 和 Clear 方法!

## version 1.11.3 更新内容

1. 优化 ImageLoader 的 Other 的设置方式! 


## version 1.11.2 更新内容

1. 优化加载子资源方法 


## version 1.11.1 更新内容

1. 添加加载子资源的方法 

## version 1.11.0 更新内容

1. 修复DownloadedSize计算错误的问题!

## version 1.10.9 更新内容

1. 修复资源类型获取失败导致的报错

## version 1.10.8 更新内容

1. 优化关闭对象池的方法

## version 1.10.7 更新内容

1. 修复重复加载场景 导致部分材质丢失的问题
2. 优化编辑器资源列表界面显示效果
3. 优化资源重复时的错误提示

## version 1.10.6 更新内容

1. 优化GameObjectLoader从对象池中加载出的游戏物体会默认放到DontDestroyOnLoad场景的问题!

## version 1.10.5 更新内容

1. 修复guid重复的问题!

## version 1.10.4 更新内容

1. 优化 GameObjectLoader 资源卸载逻辑!

## version 1.10.3 更新内容

1. 兼容OpenHarmony平台 

## version 1.10.2 更新内容

1. 兼容WebGL平台
2. 修改Build文件夹名称，防止被Git忽略!

## version 1.10.1 更新内容

1. 修复 RuntimeAnimatorController 类型的资源加载失败!

## version 1.10.0 更新内容

1. 修改 CheckUpdateResult默认值!
2. 优化CustomAsyncOperation代码!

## version 1.9.9 更新内容

1. 修复重复拖拽资源到AssetBundle列表添加失败的问题!

## version 1.9.8 更新内容

1. 修复ImageLoader的Network模式下移动端图片加载失败的问题!

## version 1.9.7 更新内容

1. 修复在打包时编辑器内(AssetBundle为Multiple模式)的资源会丢失的问题.

## version 1.9.6 更新内容

1. 修复GameObjectLoader在编辑器模式下报错

## version 1.9.5 更新内容

1. 优化 CoroutineStarter 协程启动


## version 1.9.4 更新内容

1. 修改ReadyResRequest.ExecutionType默认值为None

## version 1.9.3 更新内容

1. 优化Eidtor界面Group中的bundle(One Or Multiple)选项显示不全的问题! 
2. 添加ImageLoader同步或异步加载的选项!
3. 修改GameObjectLoader的方法为静态方法,GameObjectLoader.Instance已过时!

## version 1.9.2 更新内容

1.添加CustomAsyncOperation移除完成事件回调的方法!

## version 1.9.1 更新内容

1.优化CustomAsyncOperation完成时的事件回调,修改注册方法,在注册时进行判断当前请求是否已经完成,
  如果已经完成,则直接触发该回调!

## version 1.9.0 更新内容

1.优化ImageLoader脚本

## version 1.8.9 更新内容

1.优化AssetBundleManager.ExecuteOnlyOnceAtATime 异步任务正在执行的判断逻辑

## version 1.8.8 更新内容 

1. 优化编辑器模式资源加载

## version 1.8.7 更新内容 

1. 修复ImageLoader加载AssetBundle中的资源报错

## version 1.8.6 更新内容 

1. 优化编辑器bundle列表下拉按钮的显示区域

## version 1.8.5 更新内容 

1. 添加ReadyRes的重载函数
2. 删除GameObjectLoader资源加载的日志

## version 1.8.4 更新内容 

1. 优化编辑器模式协程启动
2. 删除查找资源依赖的功能

## version 1.8.3 更新内容 

1. 修复编辑器模式加载资源报错

## version 1.8.2 更新内容 

1. GameObjectLoader添加异步加载功能

## version 1.8.1 更新内容 

1. 优化删除项目配置文件时的提示

## version 1.8.0 更新内容 

1. 添加 GameObjectLoader 功能
   [详情参考](Documentation~/ClassApi/GameObjectLoader.md)
2. 添加 ImageLoader 功能
   [详情参考](Documentation~/ClassApi/ImageLoader.md)

## version 1.7.9 更新内容 

1. 修改菜单选项路径 XFABManager/Project -> Window/XFKT/XFABManager/Project
2. 修改菜单选项路径 XFABManager/About -> Window/XFKT/XFABManager/About

## version 1.7.8 更新内容 

1. 修改插件安装方式为通过 Unity PackageManager 安装！

## version 1.7.7 更新内容 

1. 修复编辑器模式下 同一个文件夹中有相同文件名 但类型不同 的文件时 ， 导致的资源加载失败的问题!

## version 1.7.6 更新内容 

1. 优化代码,兼容一些特殊情况,防止异常报错

## version 1.7.5 更新内容 

1. 优化AssetBundle异步加载,防止同时异步加载同一个AssetBundle报错!

## version 1.7.4 更新内容 

1. 优化资源打包

## version 1.7.3 更新内容 

1. 添加弱网情况下下载资源的超时逻辑(防止弱网下载卡住)

## version 1.7.2 更新内容 

1. 修复异步加载资源没有进度的bug

## version 1.7.1 更新内容 

1. 优化打包方式 添加自动计算依赖资源 并 打包的逻辑
2. 添加自动打包图集的功能
3. 修复异步加载资源进度为0的功能
4. 添加资源名 与 bundle 名的映射 ，加载资源可以不用传入bundleName参数 
5. 优化资源下载速度, 添加多个文件同时下载的逻辑 
6. 优化资源释放速度


## version 1.7.0 更新内容 

1. 修复编辑器编译报错
2. 优化依赖包的卸载 

## version 1.6.9 更新内容 

1. 添加压缩包下载完成后的校验

## version 1.6.8 更新内容 

1. 优化编辑器模式加载资源卡顿  
2. 优化检测资源更新逻辑,删除计算本地文件md5的操作, 提升效率
3. 优化内置资源逻辑,如果需要更新的资源在内置资源中,会优先使用内置资源,避免重复下载

## version 1.6.7 更新内容 

1. 优化下载文件功能
2. 优化资源列表显示

## version 1.6.6 更新内容 

1. 优化计算冗余资源的效率

## version 1.6.5 更新内容 

1. 修复整理资源功能导致的图片丢失的bug

## version 1.6.4 更新内容 

1. 优化计算冗余资源时间过长 和 卡顿的问题
2. 关闭查看AssetBundle的依赖信息(会产生卡顿,后面会进行优化)

## version 1.6.3 更新内容 

1. 隐藏更新日期配置,改为自动添加当前日期
2. 优化资源时过滤以shader 结尾的文件 ( 发现 shader 单独打包后会丢失,效果无法正常展示 ) 
3. 完善优化资源的判断
4. 优化资源页面 点击资源可对应在编辑器中选中
5. 添加查找资源引用 和 被引用的功能 (方便优化资源)
6. 添加整理资源功能 
7. 添加图集打包功能


## version 1.6.2 更新内容 

1. 修复在下载资源时 频繁断开网络导致报错 的问题

## version 1.6.1 更新内容 

1. 添加更新日期的配置

## version 1.6.0 更新内容 

1. 优化 SetProjectUpdateUrl 方法 (如果在调用时没有初始化 会先进行初始化)
2. 优化下载文件的逻辑，添加断点续传逻辑

## version 1.5.9 更新内容 

1. 添加 设置某一个项目更新地址的方法 AssetBundleManager.SetProjectUpdateUrl(url,projectName) *注:如果 projectName 是空 会把所有项目的更新地址设置为 url !

## version 1.5.8 更新内容 

1. 修复 修改 bundle_group 名称 子节点丢失的 bug

## version 1.5.7 更新内容 

1. 添加AssetBundle列表分组功能!

## version 1.5.6 更新内容 

1. 修复编辑器 Assets 模式下 卸载单个AssetBundle出错!

## version 1.5.5 更新内容

1. 修复初始化异常 

## version 1.5.4 更新内容

1. 添加属性 AssetBundleManager.isInited , 用来判断 AssetBundleManager 是否初始化! 
	*注:一般用不到，如果是 准备资源(AssetBundleManager.ReadyRes) 或 释放资源(AssetBundleManager.ExtralRes) \n 都会自动调用初始化的方法，如果是自己自定义资源准备流程，需要主动调用AssetBundleManager.InitializeAsync() 来进行初始化!

## version 1.5.3 更新内容

1. 修复文件夹不能同名的bug

## version 1.5.2 更新内容

1. 修复 AssetBundleManager 同步加载场景 传入 LoadSceneMode 无效的bug!


## version 1.5.1 更新内容

1. 修复异步计算md5值出错的bug!

## version 1.5.0 更新内容

1. 修复拖拽文件夹到AssetBundle列表,名称没有转为小写的bug!

## version 1.4.9 更新内容

1. 修复资源列表中 文件夹 和 文件同名时 会同时被选中的bug！

## version 1.4.8 更新内容

1. 取消打包过滤 .assets 文件.

## version 1.4.7 更新内容

1. 修复更新资源报错!

## version 1.4.6 更新内容

1. 优化 XFABTools.CaCaculateMD5(string filePath) 计算md5的方法

## version 1.4.5 更新内容

1. 添加API文档

## version 1.4.4 更新内容

1. 优化自定义版本获取

## version 1.4.3 更新内容

1. 修复默认获取版本号出错


## version 1.4.2 更新内容

1. 优化 异步获取文件 md5 值的方法

## version 1.4.1 更新内容

1. 优化资源检测( *注:修改资源检测逻辑，增加资源版本的判断,如果版本一致则认为不需要更新,以此来优化检测速度,所以如果资源变动,请务必修改版本号,否则可能会更新不到! )

## version 1.4.0 更新内容

1. 优化自定义版本获取
2. 优化 AssetBundleManager.DownloadOneAssetBundle 方法 
3. 优化异步加载资源的方法

## version 1.3.9 更新内容

1. 优化添加 AssetBundle 时检测其他AssetBundle的资源 
2. 优化AssetBundle的下载方式

## version 1.3.8 更新内容

1. 优化版本获取 
2. 优化 DownloadOneAssetBundle 方法

## version 1.3.7 更新内容

1. 修复当打开Project窗口时重启Unity报错

## version 1.3.6 更新内容

1. 添加一键打包的取消和选中全部的功能
2. 添加下载文件完成之后 验证的功能
3. 优化加载AssetBundle的方法
4. 修复资源名改变时 资源列表没有同步
5. 添加同时打开多个Project窗口功能
6. 添加 bundle 多选功能 
7. 添加 资源文件拖拽到bundle列表功能 自动添加bundle功能


## version 1.3.5 更新内容

1. 添加方法 AssetBundleManager.GetProjectCacheSize(string projectName)  获取某一个资源模块 在本地的所有资源大小
2. 添加方法 AssetBundleManager.ClearProjectCache(string projectName)  清空某一个模块在本地的所有资源文件

## version 1.3.4 更新内容

1. 放弃内置zip功能 

## version 1.3.3 更新内容

1. 修复IOS手机文件复制失败 
2. 修改获取依赖包失败

## version 1.3.2 更新内容

1. 修复编辑器模式 资源文件路径为空时 打包失败的情况!

## version 1.3.1 更新内容

1. 修复在修改配置时，配置文件没有发生改变的情况

## version 1.3.0 更新内容

1. 修复 Assets 模式资源加载出错
2. 修复不同模块有相同名称的AssetBundle时 加载出错 
   *此项修复修改了AssetBundle的加载，导致不能加载之前版本打包的AssetBundle , 需要重新打包AssetBundle
   
## version 1.2.9 更新内容

1. 优化 Assets 模式加载文件

## version 1.2.8 更新内容

1. 修复同时复制一个文件导致的报错

## version 1.2.7 更新内容

1. 优化异步获取文件md5值的方法

## version 1.2.6 更新内容

1. 优化AssetBundle文件过大导致的卡顿问题

## version 1.2.5 更新内容

1. 优化文件复制方法，添加复制失败的处理

## version 1.2.4 更新内容

1. 添加各个方法参数是否为空的判断
2. 修复 Assets 模式 LoadAllAssets() 加载不到资源的bug

## version 1.2.3 更新内容

1. 修复CheckResUpdate 在 Mac 编辑器模式下 获取配置失败
2. 优化DownloadOneAssetBundle方法

## version 1.2.2 更新内容

1. 优化方法 AssetBundleManager.DownloadOneAssetBundle()

## version 1.2.1 更新内容

1. 添加ab包名称不能含有空格 等特殊字符的验证
2. 修改下载文件失败的尝试次数为3次
3. 优化 DownloadFileRequest.DownlaodFile() 方法


## version 1.2.0 更新内容

1. 修复 字段没有使用的警告
2. 修复 当Projects窗口 打开时 编辑脚本 不显示的问题

## version 1.1.0 更新内容

1. 优化 默认获取项目版本 url  的获取方式 

2. 修复 选择 配置组 界面没有刷新的问题  

3. 修复 DownloadOneAssetBundle 下载依赖包失败的问题 

4. 优化 DownloadOneAssetBundle 添加 Assets 模式判断 

5. 优化 DownloadOneAssetBundle 添加下载速度 字段 ，添加当前更新模式字段 

## version 1.0.9 更新内容

1. 一键打包 可以选择项目  
2. 修改教程视频地址  
3. 添加项目以列表的形式显示
4. 优化释放资源的方法

## version 1.0.8 更新内容 

1. 优化 加载 AssetBundle 的方法 ，自动转换 AssetBundle 名称为小写 

2. 修复 异步加载资源 在 编辑器 Assets 模式不触发完成的回调 问题 

3. 优化异步回调，添加结果参数 

4. 添加编辑器 AssetBundle 的搜索功能 ，

5. 添加编辑器 AssetBundle的数量显示

## version 1.0.7 更新内容 

1. 添加 自定义服务端文件路径的接口 (视频介绍:https://www.bilibili.com/video/BV1uX4y1w7M9?p=6)

## version 1.0.6 更新内容 

1. 添加 PackageAll 提示 

2. 准备资源 ，更新下载资源 添加当前网速字段 CurrentSpeed / CurrentSpeedFormatStr (视频介绍:https://www.bilibili.com/video/BV1uX4y1w7M9?p=5)

3. 修复 异步操作出错 进度是仍是 0 的问题 

4. 优化  LoadAllAssetBundles  在 Editor 模式下的适配 

5. 优化方法 AssetBundleManager.IsLoadedAssetBundle(), 如果是编辑器模式 并且从 Assets 加载资源  ，一直返回true 

6. 优化 配置界面 点击空白 取消选中 

7. 优化 AssetList 列表 文字重叠问题

## version 1.0.5 更新内容

1. 修复 AB 输出路径错误 

2. 添加 构建完成 的 回调  (视频介绍:https://www.bilibili.com/video/BV1uX4y1w7M9?p=1)

3. 修复 重复调用下载 会下载多次的问题

4. 公开 判断 AssetBundle 是否已经加载的方法   AssetBundleManager.IsLoadedAssetBundle  (视频介绍:https://www.bilibili.com/video/BV1uX4y1w7M9?p=2)
 
5. 添加 多个配置功能 , 可以针对某一个模块进行配置 (视频介绍:https://www.bilibili.com/video/BV1uX4y1w7M9?p=3)

6. 修复 LoadAllAssetBundles 报错


## version 1.0.4 更新内容
删除文档


## version 1.0.3 更新内容
添加 world 说明文档

## version 1.0.2 更新内容
修复 UnityWebRequest 相关字段过时 警告

## version 1.0.1 更新内容
添加 readme.txt 
添加 Examples 案例 
修复 Editor 模式下加载资源报空的 bug 
