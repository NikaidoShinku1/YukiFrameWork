using YukiFrameWork;

����Դ:

|CoroutineTokenSource |˵��|
|------|-----|
|static Create(Component component)|��������Դ����Ҫ����һ���������������|
|Running()|����������|
|Pause()|��������ͣ|
|Cancel()|��ֹ����ִ��(���󶨵Ķ�������Ҳ�����)|
|CancelAfter(float time)|�ӳ���ֹ����ִ��|
|bool IsCoroutineCompleted|���첽Э���Ƿ��Ѿ����/��ֹ|
|bool  IsCanceled|�������Ƿ��������ֹ����,������ӳ���ֹ��������bool����ǰ����true|

����:

|CoroutineToken|˵��|
|---|---|
|TokenStates States|��ǰToken��״̬|
|EasyEvent< TokenStates > statesChaned|��Token״̬�л�ʱ�Ļص�ע��|

```csharp
public class TestScripts : MonoBehaviour
{
    async void Start()
    {
        CoroutineTokenSource source = CoroutineTokenSource.Create(this);

        //��Token���Զ����ϸ�������������ڡ����ö������٣��Ὣ�����첽��ֹ��
        await new WaitForSeconds(2f).Token(source.Token);

        //��ͣ�첽
        source.Pause();

        //�ָ�(����ͣ���߸����ƽ���ִ�к����)
        source.Running();

        //�������;
        source.Cancel();
    }
}
```