架构容器使用示例:

除基本规则架构外，框架可以使用架构拓展的新容器进行对象的控制反转。

对于注册进容器的实例本身，如果是MonoBehaviour，会自动进行生命周期的管理判断，如果对象已经不合法，会自动注销


规则接口:IGetContainer

``` csharp

public class World : Architecture<World>
{
     public override void OnInit()
     {
         
     }

     ///重写该属性，在这个数组里面填写对应的key，有多少个key就会生成多少个容器，在架构准备完成后会全部生成
     protected override string[] BuildContainers => new string[]
     {
         "自定义的容器标识Key"
     };
}

```

``` csharp

//ViewController已经继承了IGetContainer接口

public class TestScripts : ViewController
{
    public class MyCustomArg : IEventArgs
    { }
   
    async void Start()
    {
        //等待架构准备
        await World.StartUp();

        Container container = this.LoadContainer("自定义的容器标识Key");

        container.Register<MyCustomArg>("你可以为注册的对象添加一个自定义的key，访问的时候通过key来处理，没有key则默认以类型名处理");

        //从容器解析实例
        MyCustomArg arg = container.Resolve<MyCustomArg>();

        //注册MonoBehaviour组件

        container.RegisterComponent<Canvas>(FindAnyObjectByType<Canvas>());

        //解析:
        Canvas myCanvas = container.Resolve<Canvas>();

        //注销对象
        container.UnRegister<MyCustomArg>("你可以添加指定的标识，以具体注销哪一个对象，没有标识会注销这个类型所有的对象");
    }
}

```