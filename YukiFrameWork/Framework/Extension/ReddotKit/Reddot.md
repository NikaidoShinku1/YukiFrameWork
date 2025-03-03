��ܺ��ϵͳʹ��:ReddotKit

����ʾ����![Reddot1](../Texture/Reddot1.png)

���һ���µĿն��󲢹���Reddot����������Ǻ������Image������Ϊ�ö���������壬��Ϊ��������������������������Ŀ���!


|Reddot Property API|API˵��|
|--|--|
|string ReddotParent|����Ψһ����·��,����Ϊ��|
|string ReddotPath|���·������·��Ϊ��ʱ���ú����Ϊ����������ʹ��|

|Reddot Method API|API˵��|
|-|-|
|void Remove()|�Ƴ����·������|

�����·��Ϊ��ʱ������A�����ParentΪTest��PathΪ�գ�B�����ParentҲ��Test��Path��Ϊ�գ���ʱ��B����ĺ������AҲ������

|ReddotKit API|API˵��|
|---|--|
|IList< string > GetReddotPaths(string parent)|ͨ�������ڵ㣬��ȡĳһ�����еĺ��·��|
|IReadOnlyDictionary< string, IList< string > > ReadOnlyReddotPath|��ȡֻ���ĺ������|
|void AddReddotPath(string parent, string path)|���һ���µĺ��·��|
|void AddReddotPaths(string parent, IList< string > paths)|���һ���µĺ��·��|
|void RemoveReddotPath(string parent)|�Ƴ�ĳһ�����������еĺ��·��|
|void RemoveReddotPath(string parent,string path)|�Ƴ�ĳһ��������ָ���ĺ��·��|
|string ReddotPersistence (Getter)|��ȡ���·���ĳ־û�����(Json�ַ���)|
|void LoadPersistenceReddotPath(string persistence)|����ָ���������Լ������еĺ��·��|
|Action onReddotPathChanged|�����·���ı�(����/����)ʱ�������¼�|

�½��ű�����������������

``` csharp
using YukiFrameWork;
using UnityEngine;
using YukiFrameWork.Events;
public class TestScripts : MonoBehaviour
{
    void Awake()
    {
        //���ָ���ĺ��·��
        ReddotKit.AddReddotPath("Parent","1001");

        //�����¼��Ĵ����������Դ���onReddotPathChanged;����һ���Դ��Ľṹ��ChangeReddotArg������ͨ��EventManager�����¼���ע��

        EventManager.AddListener<ChangeReddotArg>(arg => 
        {
            //��д���Լ����߼�,��onReddotPathChangedͬʱ����
        });
    }
}

```