���ͨ��Buffϵͳ(���������MVC����ʽ)��

�����ռ䣺YukiFrameWork.Buffer;

��Assets�ļ������Ҽ�����BuffDataBase����:

![����ͼƬ˵��](Texture/1.png)

![����ͼƬ˵��](Texture/2.png)

������ͼ��ʾ�����Ϸ�����Buff����������ú�����������Buff���Buff���룬����Ϳ�����������������ѡ��ͼ��AttackBuffʾ��Ϊ:

```
public class AttackBuff : Buff
{
	//ToDo
}
```

���������ʹ�ѡ����Ӧ�����ͺ󣬵���·������������ұߵļӺż��ɴ����µ�Buff���á�

![����ͼƬ˵��](Texture/3.png)

�����������ɶ�Buff���ݵ�����,�����ô���1ʱ���Ϳ��Խ���������õ���Json�����Ϊ�ο����ں������ϣ���Զ���Buff���̳�IBuff�ӿ�ʱ���ɲ������

��������ɺ󣬼��ɵ���༭�������ṩ�����ɱ�ʶ���룬�����ṩһ���Ѿ����úú����ɵ�Json�����Լ��������ã���

![����ͼƬ˵��](Texture/4.png)

![����ͼƬ˵��](Texture/5.png)

����ע�⣺��ʶ��̶�����ΪBuffs�����ɵ�·���������ռ����Ϸ�����Buff����һ�¡�

����ʹ��ʾ��������Ϊ�������BuffHandler��������Ǵ������:

```
	void Start()
	{
		BuffHandler handler = gameObject.AddComponent<BuffHandler>();
	}
```

BuffHandlerΪ�ö����Buff�������࣬Buff����ӣ�UIͬ���󶨣��Ƴ���������ִ�С�

BuffHandler API:

	//Buff���ʱ�����Ļص���ֻҪ����AddBufferû�����ʧ�ܶ��ı���Buff��״̬����ͳһ����ø÷��������ҿ����õ�Controller
	- EasyEvent<BuffController> onBuffAddCallBack; //handler.onBuffAddCallBack.RegisterEvent(controller =>{ });

	//Buff�Ƴ�ʱ�����Ļص������ҿ����õ�Controller   
    - EasyEvent<BuffController> onBuffRemoveCallBack;handler.onBuffRemoveCallBack.RegisterEvent(controller =>{ });

	//����UIBuffHandlerGroup�������API��Ӧ�·�����ʹ��UIͬ��ʱ����
	- void SetUIBuffHandlerGroup(UIBuffHandlerGroup handlerGroup)

	//���Buff������һ��Buff�Լ���Ҷ���Player
	- void AddBuffer<T>(IBuff buff,GameObject player) where T : BuffController,new()

	//����Buff�ı�ʶ�Լ���Ҷ���Player
	- void AddBuffer<T>(string BuffKey, GameObject player) where T : BuffController, new()
	
	//���ݱ�ʶɾ��ĳһ���������е�Buff
	- bool RemoveBuffer(string BuffKey)

	//Buffƥ��ɾ��������
	- bool RemoveBuffer(IBuff buff);

	//�õ��������е�ָ����ʶ��Buff����
	- int GetBufferCount(string BuffKey);

	//�õ��������е�Buff��������
	- int GetAllBufferCount()

	//��ֹ�����е�����Buff���÷������ᴥ��BuffController��OnBuffRemove����
	- void StopAllBuffController()

����Buff�������ڿ�����BuffController����������

ʾ������:

```

	public class CustomBuffController : BuffController
	{
		public override void OnBuffAwake()
        {
            Debug.Log("׼�����Buff:" + Buffer.BuffName);
        }

        public override void OnBuffStart()
        {

        }

        public override void OnBuffUpdate()
        {
            Debug.Log("��ǰ��Buff:" + Buffer.BuffName + "����:" + BuffLayer);
        }

        public override void OnBuffFixedUpdate()
        {

        }

        public override void OnBuffRemove()
        {
            Debug.Log("�Ƴ�Buff");
        }

        public override void OnBuffDestroy()
        {
            Debug.Log("Buff��ȫ����");
        }     

        public override bool OnRemoveBuffCondition()
        {
            return base.OnRemoveBuffCondition();
        }

        public override bool OnAddBuffCondition()
        {
            return base.OnAddBuffCondition();
        }      
	}

```

BuffControllerר�Ŵ���Buff�߼��Լ��������ڡ�

BuffController API:

    //BuffController�ľ�̬����ʵ�����������Զ��������ʱ���Ƽ�ʹ�ø÷�������������������ר�ŵĶ���ؽ��������Ż�
    - static T CreateInstance<T>(IBuff buffer, GameObject Player) where T : BuffController, new();

    //Controllerִ���ڼ��Buff
    - IBuff Buffer { get; }

    //BuffKey���Զ��塣
    - string BuffKey => Buffer.BuffKey;

    //UIͬ��ʱ���������Buffer��BuffHandler��UIBuffHandlerGroupʱ�����Զ�ΪController��ӡ�
    - UIBuffer UIBuffer { get; }

    //��ǰBuff�Ĳ���
    - int BuffLayer { get; }

    //��BuffHandler����Buffʱ�󶨵����/����
    - GameObject Player { get; }

    //Buff���õ����ʱ������Զ���
    - float MaxTime => Buffer.BuffTimer;
    
    //��ǰBuffʣ��ʱ��
    - float RemainingTime { get; }

    ///------- ��������API��

    /// <summary>
    /// �ڲ���Buff���������Ĭ��ΪTrue������Ҫ�ڲ��������Buff���߼����߱���ϣ���Լ��ֶ����Ƶ��ӵĲ���ʱ����ʹ��
    /// </summary>
    /// <returns></returns>
    - public virtual bool OnAddBuffCondition() => true;
    
    /// <summary>
    /// �ڲ���Buff�Ƴ�������Ĭ��ΪFalse���������ڲ������Ƴ�Buff���߼�����ʹ�ã����÷����ڷ���Trueʱ����Buff�ᱻ�Ƴ�
    /// </summary>
    /// <returns></returns>
    - public virtual bool OnRemoveBuffCondition() => false;
    
    /// <summary>
    /// ���˿�ͬʱ���ڵ�Buff֮�⣬ͬһBuff�£�������Ӷ��ٲ㣬ֻҪBuff���ڣ���AwakeҲ��ֻ�е�һ�δ�����ʱ����á�
    /// </summary>
    - public abstract void OnBuffAwake();
    
    /// <summary>
    /// ÿһ��Buff�������ߵ��ӵ�ʱ�򶼻����
    /// </summary>
    - public abstract void OnBuffStart();	
    
    - public virtual void OnBuffUpdate() { }
    
    - public virtual void OnBuffFixedUpdate() { }	
    
    /// <summary>
    /// ÿһ��Buff�Ƴ���ʱ��ִ�У����Buff�ǵ����˶����ҿ����˻������٣���ÿ�μ���һ�㶼�����һ�θ÷���
    /// </summary>
    - public abstract void OnBuffRemove();
    
    /// <summary>
    /// ֻ�е���Buff��ȫ����ʱ��ִ�и÷�����
    /// </summary>
    - public virtual void OnBuffDestroy(){ }		

UIͬ����Ϊ���úõ�������UIBuffHandlerGroup�࣬��ͼ��ʾ:

![����ͼƬ˵��](Texture/6.png)

�����Զ����UIBuffer�࣬ʾ������:

```
    public class CustomUIBuffer : UIBuffer
    {
     
        public override void OnBuffDestroy()
        {
            
        }

        public override void OnBuffRemove(int buffLayer)
        {
            
        }

        public override void OnBuffStart(IBuff buff, string buffKey, int buffLayer)
        {

        }

        public override void OnBuffUpdate(float remainingTime)
        {
            
        }

        public override void OnDispose()
        {
            
        }
    }
```

�����ò��ҵ�ĳһ���趨�õ�UI֮������������Ԥ��������UIBuffHandlerGroup�У������ø��ڵ㡣

UIBuffer API��

    //ÿ��Buff��״̬���ɹ��ı������ӳɹ�ʱ���÷���Ҳ�������á�
    - void OnBuffStart(IBuff buff, string buffKey, int buffLayer)��

    //ͬ��ִ�е�Update���·���������UIBuffer��
    - void OnBuffUpdate(float remainingTime)��

    //��Buff���Ƴ�ʱҲ��ͬ�����á�
    - void OnBuffRemove(int buffLayer);

    //��Buff����ȫ����ʱͬ������
    - void OnBuffDestroy();

    //��UIBuffHandlerGroup����һ�׶���ؽ��жԸ�����Ĺ���,OnDispose��Ϊ����ʱ��ִ�еķ�����
    - void OnDispose()

Buff�����׼���BuffKit�࣬ʹ������:

```
    public class TestScripts : MonoBehaviour
    {
        public BuffDataBase dataBase;

        void Start()
        {
            BuffKit�ж��ַ�ʽ���ж�Buff���õĳ�ʼ����

             //����Ҫ����ʱ��BuffKit�������XFABManagerģ�飬����ģ������

            BuffKit.InitLoader(projectName:"");

            Ҳ�����Զ������������:
            BuffKit.InitLoader(new BuffResourcesLoader());

            //���Ѿ�����dataBaseʱ����Ե��ø÷���ֱ��ʹ��
            BuffKit.LoadBuffDataBase(dataBase);         
        }
    }

    ///�Զ��������
    public class BuffResourcesLoader : IBuffLoader
    {
        public BuffDataBase Load(string path)
        {
            return Resources.Load<BuffDataBase>(path);
        }

        public async void LoadAsync(string path, Action<BuffDataBase> callBack)
        {
            //�첽���س�ֵ�������һ���������첽��Э�̹��ߣ�ʹ�ñ�׼���첽�﷨�ǽ����첽�ı�д����������Ŀ����չ�����е�Э����չ��
            BuffDataBase dataBase = await Resources.LoadAsync<BuffDataBase>(path) as BuffDataBase;
            callBack?.Invoke(dataBase);
        }
    }
```

������ʾ������:

```
    public class TestScripts : MonoBehaviour
    {
        public BuffDataBase dataBase;

        private BuffHandler handler;
        void Start()
        {
            BuffKit.LoadBuffDataBase(dataBase);
            handler = this.GetOrAddComponent<BuffHandler>();
            handler.SetUIBuffHandlerGroup(FindObjectOfType<UIBuffHandlerGroup>());


        }

        void Update()
        {
            if(Input.GetMouseButtonDown(0))
            {
                handler.AddBuff(BuffKit.GetBuffByKey("Buff_Attack"),gameObject);
            }
        }
    }
```

BuffKit static API:

    //ʹ��XFABManager����ʱ��������������
    - void InitBuffLoader(string projectName);

    //�����Զ���ļ�����
    - void InitBuffLoader(IBuffLoader loader)��

    //����·�����DataBase
    - void LoadBuffDataBase(string dataBasePath);

    //�첽��ȡ
    - IEnumerator LoadBuffDataBaseAsync(string dataBasePath)��

    //ֱ�Ӵ�������
    - void LoadBuffDataBase(BuffDataBase buffDataBase)��

    //���ݱ�ʶ��ȡBuff
    - IBuff GetBuffByKey(string key)��



