�ܹ�����ʹ��ʾ��:

����������ܹ��⣬��ܿ���ʹ�üܹ���չ�����������ж���Ŀ��Ʒ�ת��

����ע���������ʵ�����������MonoBehaviour�����Զ������������ڵĹ����жϣ���������Ѿ����Ϸ������Զ�ע��


����ӿ�:IGetContainer

``` csharp

public class World : Architecture<World>
{
     public override void OnInit()
     {
         
     }

     ///��д�����ԣ����������������д��Ӧ��key���ж��ٸ�key�ͻ����ɶ��ٸ��������ڼܹ�׼����ɺ��ȫ������
     protected override string[] BuildContainers => new string[]
     {
         "�Զ����������ʶKey"
     };
}

```

``` csharp

//ViewController�Ѿ��̳���IGetContainer�ӿ�

public class TestScripts : ViewController
{
    public class MyCustomArg : IEventArgs
    { }
   
    async void Start()
    {
        //�ȴ��ܹ�׼��
        await World.StartUp();

        Container container = this.LoadContainer("�Զ����������ʶKey");

        container.Register<MyCustomArg>("�����Ϊע��Ķ������һ���Զ����key�����ʵ�ʱ��ͨ��key������û��key��Ĭ��������������");

        //����������ʵ��
        MyCustomArg arg = container.Resolve<MyCustomArg>();

        //ע��MonoBehaviour���

        container.RegisterComponent<Canvas>(FindAnyObjectByType<Canvas>());

        //����:
        Canvas myCanvas = container.Resolve<Canvas>();

        //ע������
        container.UnRegister<MyCustomArg>("��������ָ���ı�ʶ���Ծ���ע����һ������û�б�ʶ��ע������������еĶ���");
    }
}

```