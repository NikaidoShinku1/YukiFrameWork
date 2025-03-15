YukiFrameWork ��������״̬��: ����ʹ�ø�״̬����ɶ�״̬�Ŀ��ơ��Լ������߼�����ơ�

�����ռ�: using YukiFrameWork.Machine;

�ڳ������½����󣬲�Ϊ�������StateManager�ű�����:

![1](Texture/1.png)

��ͼ��ʾ����ҪΪStateManager������RuntimeStateMachineCore״̬�������ļ�

��Assets�ļ����£��Ҽ�Create/YukiFrameWork/Yuki����״̬���½�һ��״̬����������:

![4](Texture/4.png) ˫�������ô�״̬�����ڣ�

![2](Texture/2.png)

���״̬�����ϻ���ʾ��ǰ��״̬�����ã���Runtime�У�ѡ�д���StateManager����Ķ��󣬵�����ӵ�и���״̬������ʱ����һ����������ʾ��

��״̬�������Ҽ��½�״̬��ͼ��ʾ:

![3](Texture/3.png)

״̬�ǳ��򵥣�ֻ���������Ӧ�����״̬�ű����ܣ���������Դ����������״̬�ű���Ҳ�������Ϸ�ͼ�����õ�Inspector�н��д���(�Ǹ���������������ȷ)��

�����Ĵ����д򿪲����б�ʾ����ͼ:

![3](Texture/5.png)

Ϊ����״̬֮����ӹ���!


![3](Texture/6.png)

���ڹ��ɵ������жϣ���һ�鼯��ȫ������ʱ��״̬����й��ɣ�Ҳ������Ӷ�������飬���и���������ʱ����һ�������������򶼻��������ɡ�

״̬�ű����:StateBehaviour ״̬�඼��Ҫ�̳������������: StateBahviour�����ܼܹ�������������Controller�㼶��

``` csharp

using YukiFrameWork.Machine;

public class IdleState : StateBehaviour
{
    //ToDo
}

```

|StateBehaviourAPI|״ִ̬�л���API˵��|
|---|---|
|Property API|����API|
|StateMachine StateMachine { get; }|��ȡ��״̬���ڵ�״̬��|
|Transform transform { get; }|��ȡ��״̬����������Ϸ�����Transform|
|StateBase CurrentStateInfo { get; }|���״̬����Ϣ(���౾��)|
|Type Type { get; }|���״̬�����ʵ����(����GetType��)|
|--|--|
|Mathod API|����API|
|void OnInit()|��״̬��ʼ��(������ĳ�ʼ��ʱ��ѡ��ҹ�)|
|void OnEnter()|��״̬����|
|void OnExit()|��״̬�˳�|
|void Update()|��״̬����|
|void FixedUpdate|��״̬�������|
|void LateUpdate|��״̬���ڸ���|

��������RuntimeStateMachineCore�࣬������ʱ�ᱻת��ΪStateMachineCore�ࡣ��Ϊ״̬�����ϱ�����ʹ�á�����������������е�״̬��

|RuntimeStateMachineCore API|״̬�����ϱ���API˵��|
|---|---|
|Property API|����API|
|bool IsActive { get; }|���״̬�����ϱ����Ƿ񼤻�|
|StateManager StateManager { get; }|����������ڵ�StateManager|
|Action onStateMachineStarted|�������������ʱ�����Ļص�|
|---|---|
|Method API|����API˵��|
|void Start()|��������״̬������|
|StateBase GetCurrentStateInfo()|��ȡ��ǰ�������е�״̬��Ϣ(��������»ᾫȷ����״̬����״̬��Ϣ)|
|StateBase GetCurrentMachineStateInfo(string name)|��ȡָ��״̬���µ�����״̬��Ϣ|
|void Cancel()|�ر����״̬������|
|StateParameterData GetStateParameterData(string parameterName)|�������ƻ�ȡ��ָ���Ĳ�����Ϣ|
|StateMachine GetRuntimeMachine(string machineName)|����״̬�����ƻ�ȡ��ָ����״̬����Ĭ�ϲ��״̬������Ϊ:"Base Layer"|
|---|---|
|Parameter API|��������API|
|void SetBool(string name, bool value)|--|
|void SetBool(int nameToHash,bool value)|--|
|void SetFloat(string name,float value)|--|
|void SetFloat(int nameToHash,float value)|--|
|void SetInt(string name,int value)|--|
|void SetInt(int nameToHash,int value)|--|
|void SetTrigger(string name)|--|
|void SetTrigger(int nameToHash)|--|
|void ResetTrigger(string name)|����Trigger|
|void ResetTrigger(int nameToHash)|����|
|bool GetBool(string name)|--|
|bool GetBool(int nameToHash)|--|
|float GetFloat(string name)|--|
|float GetFloat(int nameToHash)|--|
|int GetInt(string name)|--|
|int GetInt(int nameToHash)|--|
|bool GetTrigger(string name)|--|
|bool GetTrigger(int nameToHash)|--|

Tip��������������API ���ƻᱻת��ΪHash������StateManager.HashToString����ת����

��StateMachineCore�ᱣ�����е�״̬��(Ĭ�ϲ��Լ����е���״̬��)StateMachine��

|StateMachine API|״̬��API˵��|
|--|--|
|Property API|����API˵��|
|StateMachine Parent { get; }|�������״̬�����������֮�����ĸ�״̬����BaseLayer���״̬��Parent�㶨Ϊ��|
|StateMachineCore StateMachineCore { get; }|״̬�����ϱ���|
|StateBase GetState(string name)|����״̬���Ʋ��ҵ�ָ����״̬����|
|StateMachine GetChildMachine(string machineName)|�������״̬����ָ������״̬��|
|StateBase CurrentState { get; }|��ǰ���״̬��������״̬|
|StateMachine CurrentChildMachine { get; }|���״̬�����Ϊ״̬�������ֵ��Ϊ��|
|StateBase DefaultState { get; }|���״̬����Ĭ��״̬|
|StateBase GetCurrentStateInfo()|��CurrentState������״̬�������ͬ��CurrentState���ԣ�����ȷ��λ����״̬���ľ���״̬|
|StateTransition CurrentTransition { get; }|��ǰ���״̬���ɹ��л�״̬����������|

