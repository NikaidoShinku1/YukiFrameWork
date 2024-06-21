using YukiFrameWork;

令牌源:

|CoroutineTokenSource |说明|
|------|-----|
|static Create(Component component)|创建令牌源，需要传递一个组件绑定生命周期|
|Running()|将令牌启动|
|Pause()|将令牌暂停|
|Cancel()|终止令牌执行(当绑定的对象被销毁也会调用)|
|CancelAfter(float time)|延迟终止令牌执行|
|bool IsCoroutineCompleted|该异步协程是否已经完成/终止|
|bool  IsCanceled|该令牌是否调用了终止方法,如果是延迟终止方法，该bool会提前返回true|

令牌:

|CoroutineToken|说明|
|---|---|
|TokenStates States|当前Token的状态|
|EasyEvent< TokenStates > statesChaned|当Token状态切换时的回调注册|

```csharp
public class TestScripts : MonoBehaviour
{
    async void Start()
    {
        CoroutineTokenSource source = CoroutineTokenSource.Create(this);

        //绑定Token，自动绑定上该组件的生命周期。当该对象销毁，会将整个异步终止。
        await new WaitForSeconds(2f).Token(source.Token);

        //暂停异步
        source.Pause();

        //恢复(仅暂停或者该令牌结束执行后可用)
        source.Running();

        //打断运行;
        source.Cancel();
    }
}
```