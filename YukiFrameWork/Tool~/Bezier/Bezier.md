������������չ���ܽ���:

��ܼ����˶Ա��������ߵ�һЩ��װ:

BezierUtility:���������߹�ʽ����(Ŀǰ���֧������)

API:

	//�Ա��������߹�ʽ��װ��API
	- Vector3 BezierIntepolate(Vector3 p0, Vector3 p1, float t);// B(t) = (1-t)P0 +tP1 Mathf.Clamp(t,0,1)
	- Vector3 BezierIntepolate(Vector3 p0, Vector3 p1,Vecor3 p2, float t)// B(t) = (1-t)(1-t)p0 + 2t(1-t)P1 + t*t*P2 Mathf.Clamp(t,0,1)
	- Vector3 BezierIntepolate(Vector3 p0, Vector3 p1,Vecor3 p2,Vector3 p3, float t)// B(t) = (1-t)(1-t)(1-t)P0 + 3P1 * t *(1-t)*(1-t) + 3P2 * t * t * (1-t)+P3* t * t * t Mathf.Clamp(t,0,1)

	//ʹ��Unity�ṩ��Vector3.Lerp�Թ�ʽ���з�װ
	- Vector3 BezierLerp(Vector3 p0, Vector3 p1, float t);
	- Vector3 BezierLerp(Vector3 p0, Vector3 p1,Vecor3 p2, float t)
	- Vector3 BezierLerp(Vector3 p0, Vector3 p1,Vecor3 p2,Vector3 p3, float t)

	//��������±���������·�����е�·����,tΪ·��������
	- List<Vector3> GetBezierList(Vector3 p0, Vector3 p1, float t);//�����tΪ��������
	- List<Vector3> GetBezierList(Vector3 p0, Vector3 p1, Vector3 p2, float t);
	- List<Vector3> GetBezierList(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t);

ʹ��ʾ����
```

using YukiFrameWork;
public class TestScripts : MonoBehaviour
{
	public Transform p1;
	public Transform p2;
	public Transform p3;
	public List<Vector3> paths = new List();
	void Start()
	{
		//����չʾ���׵�ʹ�÷�ʽ:

		List<Vector3> list = BezierUtility.GetBezierList(p1.position,p2.position,p3.position,50);

		paths = list;
		for(int i = 0;i<paths.Count;i++)
		{
			float t = i / (float)paths.Count;

			//������·���ϵĵ�
			Vector3 points = BezierIntepolate(p1.position,p2.position,p3.position,t);
		}

		
	}
}
```

������չ: ���Ŵ����ı����������ƶ�:
API: //transform��չ����,������APIĬ�ϳ�ʼλ�þ���transform��λ�á�BezierRuntimeMode����ѡ���ƶ����ڵĺ��� --- Update/FixedUpdate/LateUpdate,Ĭ��50������������

	- BezierCore BezierTowards(this Transform transform, Vector3 end, float currentSpeed, BezierRuntimeMode mode = BezierRuntimeMode.OnFixedUpdate, float pointCount = 50);
	- BezierCore BezierTowards(this Transform transform, Vector3 controlPoint1, Vector3 end, float currentSpeed, BezierRuntimeMode mode = BezierRuntimeMode.OnFixedUpdate, float pointCount = 50)
	- BezierCore BezierTowards(this Transform transform, Vector3 controlPoint1, Vector3 controlPoint2, Vector3 end, float currentSpeed, BezierRuntimeMode mode = BezierRuntimeMode.OnFixedUpdate, float pointCount = 50)

```

using YukiFrameWork;
public class TestScripts : MonoBehaviour
{	
	public Transform control;
	public Transform target;
	
	void Start()
	{
		//����ʹ�ö��׵��ƶ�����(���ƶ�����Ҫ�ⲿд��Update��ȥ����ÿ֡����)
		transform.BezierTowards(control.position,target.position,10);
			
	}
}
```

�༭����չʹ��:Ϊ�������BezierVisualTool�ű���ͼ:

![����ͼƬ˵��](Texture/Tool.png)

��ͼ�п���������������������׵ı���������,�����л�ģʽ��ѡ��ʹ��Vector������Transform,֧����Scene��ͼ�еĿ��ӻ�(��Ϸ���ڲ��ɼ�)

���ú���չAPI����:

```
using YukiFrameWork;
public class TestScripts : MonoBehaviour
{	
	public BezierVisualTool tool;	
	
	void Start()
	{
		//���ݿ��ӻ��ű����ö���,ֻ��Ҫ����tool�Լ��ٶȼ���
		transform.BezierTowards(tool,10);
			
	}
}
```

֧�ִ����ڲ��Զ���ı�������������(���BezierVisualTool��ʹ��):

	- һ�׽ӿ�: IFirstOrderBezierCurve
	- ���׽ӿ�: ISecondOrderBezierCurve
	- ���׽ӿ�: IThirdOrderBezierCurve

ʹ��ʾ��:

```
using YukiFrameWork;
public class CustomBezierConfig : ISecondOrderBezierCurve
{	
	 [SerializeField]
     private Transform control;
     public Vector3 GetControlPointAtSecondOrder()
     {
		return control.position;
     }

     public Vector3 GetEndPoint()
     {
        return Vector3.one;
     }

     public int GetPathLangth()
     {
        return 30;
     }

     public Vector3 GetStartPoint()
     {
		return new Vector3(10, 15, 35);
     }     
}

public class TestScripts : MonoBehaviour
{	
	public BezierVisualTool tool;	
	
	void Start()
	{
		//�����Զ��������
		tool.SetSecondOrderBezier(new CustomBezierConfig());
		transform.BezierTowards(tool,10);
			
	}
}
```






	