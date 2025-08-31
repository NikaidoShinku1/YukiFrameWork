对话系套件:DiaLogKit

命名空间: using YukiFrameWork.DiaLogue;

在Assets文件夹下右键YukiFrameWork/NodeTree下创建一个对话树的根节点文件：

![输入图片说明](Texture/1.png)

编辑器如图所示,设置对应的标识，用于启动对话树时使用，

![输入图片说明](Texture/2.png)

点击Edit Graph打开编辑器窗口如下:

![输入图片说明](Texture/3.png)

可以为节点添加不同的自定义类型(字符串)并设置在编辑器时的节点颜色。

任何对话树第一个创建的节点是根节点，不可修改也不可设置类型

示例配置如下:

![输入图片说明](Texture/4.png)

需要创建一个继承DiaLogController基类的对话控制器类，API如下:

|DiaLogController API|对话控制器API|
|--|--|
|int CurrentNodeId { get; }|当前节点的id|
|INode CurrentNode { get; }|当前节点|
|DiaLogState DiaLogState { get; }|当前控制状态(待机、运行、完成)|
|string Key { get; }|对话配置的唯一标识|
|NodeTreeBase NodeTree { get; }|配置本体|
|event Action(DiaLogController, object[]) onStart|当控制启动时与生命周期同步调用的开始事件|
|event Action(DiaLogController,INode,INode) onMove;|当对话发生改变时触发的事件|
|event Action(DiaLogController) onCompleted|当对话完成时的事件|
|---|---|
|生命周期说明||
|void OnUpdate()|可重写的Update方法|
|void OnFixedUpdate()||
|void OnLateUpdate()||
|void OnStart(params object[] param)|当对话控制开始，可自定义传入参数|
|void OnCompleted()|当对话完成|
|bool FindRootNode(out INode node)|查找根节点|
|INode TryFindNode(int id)|不会抛出异常的查找节点|
|INode FindNode(int id)|查找节点|

构建自定义的控制器并实现自己的逻辑
``` csharp
public class CustomDiaLogController : DiaLogController
{
       protected override void OnCompleted()
       {
             
       }

       protected override void OnStart(params object[] param)
       {
            
       }

       protected override void OnMove(INode lastNode, INode nextNode)
       {
        
       }
}
```

|对话管理套件API说明|DiaLogKit static API|
|---|---|
|void Init(string projectName)|初始化方法(使用内置加载器)|
|void Init(IDiaLogLoader loader)|继承对话配置加载接口自定义初始化|
|void Release(string key)|通过标识释放控制器与配置|
|DiaLogController GetDiaLogController(string key)|根据标识获取控制器|
|DiaLogController GetOrCreateController(string path)|获取或加载控制器|
|T GetOrCreateController< T >(string key,IEnumerable< INode > nodes)|当不使用配置且能够自定义节点数据结构，可加载控制器|
|DiaLogController GetOrCreateController(string key,Type type,IEnumerable< INode > nodes)|如上重载|
|DiaLogController GetOrCreateController(NodeTreeBase nodeTree)|直接传递配置加载或者获取控制器|
|void GetOrCreateControllerAsync(string path,Action< DiaLogController > completed)|异步加载控制器|
|void Start(this DiaLogController controller,int id,params object[] param)|无视根节点，传递指定的标识启动控制器|
|void Start(this DiaLogController controller, INode node, params object[] param)|无视根节点，传递指定的节点启动控制器|
|void Start(this DiaLogController controller, params object[] param)|通过根节点启动控制器|
|bool MoveRandom(this DiaLogController controller, out string failedTip)|以当前节点的连接节点中随机推进一个，可输出日志|
|bool MoveRandom(this DiaLogController controller)|以当前节点的连接节点中随机推进一个|
|bool MoveNext(this DiaLogController controller, out string failedTip)|以当前节点的连接节点的第一个进行推进，可输出日志|
|bool MoveNext(this DiaLogController controller)|以当前节点的连接节点的第一个进行推进|
|bool MoveNext(this DiaLogController controller, int linkId)|指定当前节点的连接节点进行推进|
|bool MoveNext(this DiaLogController controller, int linkId,out string failedTip)|指定当前节点的连接节点进行推进,可输出日志|
|bool Move(this DiaLogController controller,int id)|无视连接，强制切换节点|
|bool Move(this DiaLogController controller, int id,out string failedTip)|无视连接，强制切换节点,可输出日志|




