框架通用Buff系统(代码设计以MVC的形式)：

命名空间：YukiFrameWork.Buffer;

在Assets文件夹下右键创建BuffDataBase配置:

![输入图片说明](Texture/1.png)

![输入图片说明](Texture/2.png)

配置如图所示，最上方生成Buff代码可以设置后生成派生自Buff类的Buff代码，而后就可以在生成类型这里选择，图中AttackBuff示例为:

``` csharp
public class AttackBuff : Buff
{
	//ToDo
}
```
双击配置打开配置窗口如下:
![输入图片说明](Texture/3.png)
在左侧新建配置即可。

|Buff Data API|Buff的默认配置数据|
|--|--|
|string Key { get; set; }|唯一标识|
|string Name { get; set; }|Buff名称|
|string Description { get; set; }|Buff介绍|
|BuffMode BuffMode { get; set; }|Buff的添加模式|
|float Duration { get; set; }|Buff的持续时间，当该参数小于0时默认为无限时间|
|Sprite Icon { get; set; }|Buff图标|
|List< IEffect > EffectDatas { get; }|Buff的所有效果|
|string BuffControllerType { get; set; }|Buff的绑定控制器类型|

对于任意的一个Buff，可以添加多个需要的效果：

|IEffect 效果接口|Interface API|
|--|--|
|string Key { get; set; }|效果的唯一标识(唯一性仅适用于同一Buff下)|
|string Name { get; set; }|效果名称|
|string Type { get; set; }|效果的类型(可查询使用)|
|string Description { get; set; }|效果的介绍|
|IReadOnlyDictionary<string,EffectParam> EffectParams { get; }|效果可用的参数|

对于框架默认配表的Buff基类，有默认可使用的效果类型NormalEffect类。如需要自定义可重写EffectData属性如下：

``` csharp
public class AttackBuff : Buff
{
	public override List<IEffect> EffectDatas => base.EffectDatas;//重写自己定义的继承IEffect的效果类
}
```

|可添加Buff的执行者接口|Interface API|
|--|--|
|IBuffExecutor|执行者接口|
|bool OnAddBuffCondition()|外部Buff添加条件，默认为True，与Controller的添加条件区别在于，该方法控制所有的buff添加判断，如果外部添加条件设置为False,则无法添加任何Buff|
|void OnAdd(BuffController controller)|添加Buff并成功后触发|
|void OnRemove(BuffController controller)|移除Buff后触发|

对于需要Buff的对象，都应该继承这个接口！代表该对象可以拥有Buff。


``` csharp
public class TestScripts : MonoBehaviour,IBuffExecutor
{
    
    public bool OnAddBuffCondition() => true;

    public void OnAdd(BuffController controller)
    {
        
    }

    public void OnRemove(BuffController controller)
    {
        
    }
}
```

所有Buff均由BuffKit统一进行管理!

|BuffKit static API|BuffKit API说明|
|---|---|
|void Init(string projectName)|BuffKit初始化加载器方法|
|void Init(IBuffLoader buffLoader)|初始化重载(直接传递Loader)|
|void AddBuffData(IBuff buff)|添加一个Buff数据(适合在不使用框架默认的so配表自定义使用)|
|void RemoveBuffData(string key)|通过标识移除一个Buff配置，该API不会影响正在运行中的Buff|
|void RemoveBuffData(IBuff buff)|通过Buff对比移除一个Buff配置,该API不会影响正在运行中的Buff|
|void LoadBuffDataBase(string name)|加载配表|
|void LoadBuffDataBase(BuffDataBase buffDataBase)|直接通过传递配表加载|
|IEnumerator LoadBuffDataBaseAsync(string name)|异步加载配表|
|List< IEffect > GetEffects(IBuffExecutor player)|获取执行者目前正在执行的所有Buff下的所有效果|
|void GetEffectsNonAlloc< T >(IBuffExecutor player, List< T > results) where T : IEffect||
|List< IEffect > GetEffects(IBuffExecutor player,string key)|获取指定效果标识的所有效果(如果有多个Buff具有相同标识的效果可用)|
|void GetEffectsNonAlloc(IBuffExecutor player, List< IEffect > results,string key)|上述重载，但需要自行传递集合|
|List< IEffect > GetEffectsByType(IBuffExecutor player, string type)|根据效果的类型获取到指定的效果集合|
|void GetEffectsNonAllocByType(IBuffExecutor player, List< IEffect > results, string type) |如上重载，但需要自行传递集合|
|IBuff GetBuffData(string key)|获取指定的Buff数据|
|BuffController GetBuffController(this IBuffExecutor Player,string buffKey)|查询某个角色下的Buff控制器|
|List<BuffController> GetBuffControllers(this IBuffExecutor Player)|查询某个角色下所有的Buff控制器|
|BuffController AddBuff(IBuffExecutor player,string buffKey,params object[] param)|为执行者添加Buff，返回Buff控制器|
|BuffController AddBuff(IBuffExecutor player, IBuff buff, params object[] param)|如上重载|
|BuffController AddBuff(IBuffExecutor player, string buffKey, float duration, params object[] param)|如上重载，可以动态设置持续时间，以添加Buff传递的时间为准|
|BuffController AddBuff(IBuffExecutor player, IBuff buff, float duration, params object[] param)|如上重载|
|void RemoveBuff(IBuffExecutor player)|移除指定执行者所有的Buff|
|void RemoveBuff(IBuffExecutor player, BuffController controller)|移除指定Buff|
|void RemoveBuff(IBuffExecutor player, IList< BuffController > controllers)|移除一组Buff|
|void RemoveBuff< T >(IBuffExecutor player) where T : BuffController|移除指定类型的Buff|
|void RemoveBuff(IBuffExecutor player, string key)|根据Buff标识移除|
|bool IsContainEffect(IBuffExecutor player, string effect_key)|查询执行者正在执行的Buff有没有指定的效果|
|bool IsContainBuff(IBuffExecutor player, string buffKey)|查询执行者是否有指定的Buff正在运行|
|bool IsContainBuff(IBuffExecutor player, string buffKey,out BuffController controller)|查询执行者是否有指定的Buff正在运行,可返回值||

创建Buff生命周期控制器BuffController的派生基类

示例如下:

``` csharp

	public class CustomBuffController : BuffController
	{
		protected override void OnAdd(params object[] param)
        {
           
        }

        protected override void OnUpdate()
        {
            
        }

        protected override void OnRemove()
        {
            
           
        }
	}

```

BuffController专门处理Buff逻辑以及生命周期。

|BuffController API|Buff控制器API|
|---|---|
|IBuffExecutor Player { get; }|绑定的执行者对象|
|IBuff Buff { get; }|绑定的Buff配置|
|float ExistedTime { get; }|当前Buff已运行的时间|
|virtual float Duration { get;set; }|可重写的持续时间属性。如果Buff是无限时间的，则默认该属性为-1|
|float Progress { get; }|当前Buff的进度，如果Buff是无限时间的，则返回-1 否则返回进度(0-1)|
|--|--|
|可重写方法一览|Level:protected|
|virtual bool OnAddBuffCondition()|可重写的是否能添加Buff的条件设置，默认为True，当该方法返回False时该Buff不能被任何对象添加|
|virtual void OnAdd(params object[] param) { }|当Buff添加|
|virtual void OnUpdate() { }|当Buff更新|
|virtual void OnFixedUpdate()|当Buff间隔|
|virtual void OnLateUpdate|当Buff晚于更新|
|virtual void OnRemove()|当Buff被移除|

基本使用如下:

``` csharp
    public class TestScripts : MonoBehaviour,IBuffExecutor
    {
        public BuffDataBase dataBase;
       
        void Start()
        {
           
            BuffKit有多种方式进行对Buff配置的初始化：

             //当需要加载时，BuffKit依赖框架XFABManager模块，传入模块名：

            BuffKit.InitLoader(projectName:"");

            也可以自定义加载器如下:
            BuffKit.InitLoader(new BuffResourcesLoader());

            //当已经持有dataBase时候可以调用该方法直接使用
            BuffKit.LoadBuffDataBase(dataBase);         
        }

        //外部的添加条件方法
        bool IBuffExecutor.OnAddBuffCondition() => true;

        public void OnAdd(BuffController controller)
        {
        
        }

        public void OnRemove(BuffController controller)
        {
        
        }
    }

    ///自定义加载器
    public class BuffResourcesLoader : IBuffLoader
    {
        public TItem Load<TItem>(string path) where TItem : BuffDataBase
        {
            return Resources.Load<TItem>(path);
        }

        public async void LoadAsync<TItem>(string path, Action<TItem> callBack) where TItem : BuffDataBase
        {
            //异步加载出值：框架有一套完整的异步流协程工具，使用标准的异步语法糖进行异步的编写，详情请查阅框架拓展工具中的协程拓展。
            TItem dataBase = await Resources.LoadAsync<TItem>(path) as TItem;
            callBack?.Invoke(dataBase);
        }
    }
```

