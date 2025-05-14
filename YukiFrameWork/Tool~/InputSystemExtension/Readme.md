YukiFrameWork InputSystemExtension 新输入系统InputSystem的集成拓展

using YukiFrameWork.InputSystemExtension;

注意：使用该拓展模块之前。必须要导入Unity InputSystem新输入系统包!

|InputKit 全局输入套件类|API说明|
|---|---|
|static Property API|静态属性|
|List< Gamepad > AllGamepads  { get; }|所有的手柄|
|List< Keyboard > AllKeyboards  { get; }|所有的键盘|
|List< Mouse > AllMouses { get; }|所有的鼠标|
|List< Pointer > AllPointers { get; }|所有的Pointer|
|Vector2 MousePosition { get; }|当前鼠标位置|
|---|---|
|static Method API|静态方法|
|bool AnyKeyDown()|是否有任意键按下|
|bool AnyKeyDown(out ButtonControl buttonControl,out InputDeviceType inputType)|是否有任意键按下,返回输出的按钮，以及设备类型(优先检查手柄)|
|bool AnyKeyUp()|是否有任意键抬起|
|bool AnyKeyUp(out ButtonControl buttonControl, out InputDeviceType inputType)|是否有任意键抬起,返回输出的按钮，以及设备类型(优先检查手柄)|
|bool GetKey(Key key)|是否按住某个键盘按键|
|bool GetKeyDown(Key key)|是否按下某个键盘按键|
|bool GetKeyUp(Key key)|是否抬起某个键盘按键|
|bool GetKey(Key key,Keyboard keyboard)|指定键盘是否按住某个按键|
|bool GetKeyDown(Key key, Keyboard keyboard)|指定键盘是否按下某个按键|
|bool GetKeyUp(Key key, Keyboard keyboard) |指定键盘是否抬起某个按键|
|bool GetKey(GamepadButton button)|是否按住某个手柄按键|
|bool GetKeyDown(GamepadButton button)|是否按下某个手柄按键|
|bool GetKeyUp(GamepadButton button)|是否抬起某个手柄按键|
|bool GetKey(GamepadButton button, Gamepad gamepad)|指定手柄是否按住某个按键|
|bool GetKeyDown(GamepadButton button, Gamepad gamepad)|指定手柄是否按下某个按键|
|bool GetKeyUp(GamepadButton button, Gamepad gamepad)|指定手柄是否抬起某个按键|
|bool GetKey(MouseButton button, Mouse mouse)|指定鼠标是否按住某个按键|
|bool GetKeyDown(MouseButton button, Mouse mouse)|指定鼠标是否按下某个按键|
|bool GetKeyUp(MouseButton button, Mouse mouse)|指定鼠标是否抬起某个按键|
|bool GetKey(MouseButton button)|是否按住某个鼠标按键|
|bool GetKeyDown(MouseButton button)|是否按下某个鼠标按键|
|bool GetKeyUp(MouseButton button)|是否抬起某个鼠标按键|
|Vector2 GetStickValue(GamepadButton stick) |获取某个摇杆的值|
|Vector2 GetStickValue(GamepadButton stick,Gamepad gamepad)|获取指定手柄的某个摇杆的值|
|KeyControl GetInputControl(Key key) |获取键盘的某个KeyControl|
|KeyControl GetInputControl(Key key, Keyboard keyboard)|获取指定键盘的某个KeyControl|
|InputControl GetInputControl(GamepadButton button)|获取手柄的InputControl|
|InputControl GetInputControl(GamepadButton button, Gamepad gamepad)|获取指定手柄的InputControl|
|InputControl GetInputControl(MouseButton button)|获取鼠标的InputControl|
|InputControl GetInputControl(MouseButton button, Mouse mouse)|获取指定鼠标的InputControl|
|void GamepadVibrate(Gamepad gamepad,float low,float hight,float time)|让某个手柄震动|
|void GamepadVibrate(float low, float hight, float time)|让当前手柄震动|
|void GamepadStopVibrate()|停止手柄震动|
|void GamepadStopVibrate(Gamepad gamepad)|停止指定手柄的震动|
|--|--|
|拓展方法|API|
|GamepadButton GetGamepadButton(this Gamepad gamepad,ButtonControl buttonControl) |根据ButtonControl获取GamepadButton|

简单示例演示:

``` csharp

public class TestScripts : MonoBehaviour
{
    void Update()
    {
        //比如判断按下了任意按键:

        if(InputKit.AnyKeyDown())
        {
            //ToDo
        }
    }
}

```

模块内部封装了设备类型枚举:

``` csharp    
    public enum InputDeviceType
    {
        [Tooltip("空")]
        None,
        [Tooltip("手柄")]
        Gamepad, 
        [Tooltip("键盘")]
        Keyboard,
        [Tooltip("鼠标")]
        Mouse,
        [Tooltip("触摸")]
        Pointer
    }
```

对于按键控制拓展，框架提供InputKeyControl类:

|InputKeyControl API|按键控制类|
|--|--|
|Construct Method API|构造函数|
|public InputKeyControl(string name, Key defaultKey)|默认使用键盘按键的输入控制|
|public InputKeyControl(string name, MouseButton defaultMouseButton)|默认使用鼠标按键的输入控制|
|public InputKeyControl(string name, GamepadButton defaultGamepadButton)|默认使用手柄的输入控制|
|public InputKeyControl(string name, Key defaultKey, GamepadButton defaultGamepadButton)|默认同时使用键盘和手柄的输入控制|
|public InputKeyControl(string name, MouseButton defaultMouseButton, GamepadButton defaultGamepadButton)|默认同时使用鼠标和手柄的输入控制|
|---|---|
|Property API|属性说明|
|string Name { get;}|按键构造名称("必须保持唯一")|
|Key Key { get; }|键盘按键值|
|MouseButton MouseButton{ get; }|鼠标按键值|
|GamepadButton GamepadButton { get; }|手柄按键值|
|bool EnableGamepad { get; }|是否开启了手柄控制|
|---|---|
|Method API|方法说明|
|void SetInputDevice(Keyboard keyboard)|设置当前使用的键盘设备|
|void SetInputDevice(Mouse mouse)|设置当前使用的鼠标设备|
|void SetInputDevice(Gamepad gamepad)|设置当前使用的手柄设备|
|string GetDisplayName(InputDeviceType inputDeviceType)|获取当前使用的按键的显示名称(根据设备返回)|
|Gamepad GetGamepad()|获取手柄设备|
|Keyboard GetKeyboard()|获取键盘设备|
|Mouse GetMouse()|获取鼠标设备|
|bool GetKeyDown()|判断当前按键是否按下|
|bool GetKey()|判断当前按键是否按住|
|bool GetKeyUp()|判断当前按键是否抬起|
|void Reset()|重置(如果有按键修改则返回默认值)|
|bool IsCanControl(InputDeviceType inputDeviceType)|传递输入类型设备以判断该控制是否能被通过|

鼠标按键值MouseButton:

``` csharp

    public enum MouseButton
    {
        None,
        LeftButton,
        MiddleButton,
        RightButton,
        ForwardButton,
        BackButton,
        Scroll,
        ClickCount
    } 

```

对于InputKeyControl简单示例:

``` csharp

public class TestScripts : MonoBehaviour
{

   // 使用键盘C键控制
   InputKeyControl testControl = new InputKeyControl("testControl",UnityEngine.InputSystem.Key.C);


   private void Update()
   {
       //当按键按下触发
       if (testControl.GetKeyDown())
       { 
           //触发逻辑
       }

       //当按键持续按住触发
       if(testControl.GetKey())
       {
            //
       }

       //当按键抬起触发
       if(testControl.GetKeyUp())
       {
            
       }
   }

}
```

除上述基本使用外，对于InputKeyControl类，可以事件的形式绑定,同时支持InputAction，框架有InputControlGroup 按键控制事件触发分组类:

delegate:
``` csharp
    /// <summary>
    ///  输入回调函数 将按键分配给组或通过传递InputActionMap配置后，注册相对应的事件在每当输入检测符合条件时自动触发
    /// </summary>
    /// <param name="context">当通过ActionMap触发按键时有值</param>
    /// <param name="inputKeyControl">当通过使用框架InputKeyControl触发有值</param>
public delegate void InputCallBackContext(InputAction.CallbackContext context,InputKeyControl inputKeyControl);
```

|InputKeyControlEventTriggerGroup API|按键控制事件触发分组类 API说明|
|---|---|
|static Method API|静态API说明|
|void CreateInputControlGroups(params string[] groupNames)|创建多个按键事件触发分组(分组标识应全局唯一)|
|void ForEach(Action< InputControlGroup > each)|遍历已经加载出来的所有分组|
|InputControlGroup CreateInputControlGroup(string groupName)|创建指定标识作为名称的分组(分组标识应全局唯一)|
|void Release(bool IsClear = false)|释放所有已经创建的分组 IsClear:是否在释放后清空分组|
|void ReleaseGroup(string groupName, bool IsClear = false)|释放指定分组 IsClear:是否在释放后清空分组|
|InputControlGroup GetInputControlGroup(string groupName)|根据标识获取某一个分组|
|InputControlGroup[] CreateInputControlGroupsByInputAction(InputActionMap maps)|通过InputActionAsset的配置取得指定的InputActionMap传递，会传递在ActionMap下所有的InputAction 每一个InputAction构建一个分组，组名为InputAction的名称|
|InputControlGroup CreateInputControlGroupByInputAction(InputAction inputActions)|通过传递指定的InputAction以构建分组,组名为InputAction的名称|
|---|---|
|Property API|属性API说明|
|string GroupName { get; }|分组名称|
|bool IsRunning { get; }|这个分组是否正在运行|
|InputAction InputAction { get; set; }|当前分组所绑定的InputAction|
|event InputCallBackContext onKey|当按住键触发|
|event InputCallBackContext onKeyDown|当按下键触发|
|event InputCallBackContext onKeyUp|当抬起键触发|
|event Action onBindRebind|当对InputAction配置进行按键重映射完成后触发|
|event Action onBindCancel|当对InputAction配置进行按键重映射取消后触发|
|---|---|
|Method API|方法API说明|
|void AutoPerformInteractiveRebinding(int bindingIndex, Action onBindRebound = null,Action onCancel = null)|如果分组设置了InputAction，则可以使用该方法进行对InputAction的自动重改键操作。改键会进行本地化储存。必须保证存在的InputAction名称是唯一的|
|InputActionRebindingExtensions.RebindingOperation PerformIntarctiveRebinding(int bindingIndex,Action onBindRebound = null,Action onCancel = null)|如果分组设置了InputAction，则可以使用该方法进行对InputAction的重改键操作。改键会进行本地化储存。必须保证存在的InputAction名称是唯一的|
|int FindBindingIndex(string displayName)|根据Binding的DisplayName进行BindingIndex的获取操作|
|int FindBindingIndex(InputBinding binding)|根据Binding进行BindingIndex的获取操作|
|void AddKeyControl(InputKeyControl keyControl)|传递按键控制类以添加按键在该分组|
|void AddKeyControls(params InputKeyControl[] keyControls)|添加多个按键|
|void RemoveKeyControl(InputKeyControl keyControl)|传递按键控制以移除相同名称的按键|
|void RemoveKeyControl(string keyName)|根据按键名称移除按键|
|void Enable()|启动这个分组(回调仅启动后可使用)|
|void Disable()|停止这个分组(回调在停止时无法被触发)|
|void Dispose()|释放这个分组|

分组使用简单示例:

``` csharp 

public class TestScripts : MonoBehaviour
{
    public const string W_Key = "Cook_Player_W";
    public const string S_Key = "Cook_Player_S";
    public const string A_Key = "Cook_Player_A";
    public const string D_Key = "Cook_Player_D";
    public const string PlayerInputGroupName = "PlayerInput";
    private InputKeyControl w = new InputKeyControl(W_Key, Key.W);
    private InputKeyControl s = new InputKeyControl(S_Key, Key.S);
    private InputKeyControl a = new InputKeyControl(A_Key, Key.A);
    private InputKeyControl d = new InputKeyControl(D_Key, Key.D);

    
    private InputControlGroup inputKeyControlEventTriggerGroup;
    void Start()
    {
         inputKeyControlEventTriggerGroup = InputControlGroup.CreateInputControlGroup(PlayerInputGroupName);
         inputKeyControlEventTriggerGroup.AddKeyControls(w, a, s, d);    
         
         //添加事件
         inputKeyControlEventTriggerGroup.onKey += Move;

         InputAction inputAction = null;//假设你有一个自己的InputAction配置
         inputKeyControlEventTriggerGroup.InputAction = inputAction;

    }

    void OnEnable()
    {
        //对于分组,并不会在创建后就可以直接进行事件的判断，需要手动开启

        inputKeyControlEventTriggerGroup.Enable();
    }

    void OnDisable()
    {
       
        //如对象失活时不再需要这个分组的运行 调用分组的Disable方法。
         inputKeyControlEventTriggerGroup.Disable();
    }

    private void Move(InputAction.CallbackContext context,InputKeyControl inputKeyControl)
    {
        //在这里填写移动的逻辑

    }



}
```



