using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using YukiFrameWork.Extension;

namespace YukiFrameWork.Events
{
    internal enum EventType
    {
        
        String,
        Enum
    }    
    [ClassAPI("运行时事件注册")]
    [GUIDancePath("YukiFrameWork/Framework/Events")]
    [HideMonoScript]
    public class RuntimeEventCenter : MonoBehaviour
    {     
        internal ViewController m_controller => GetComponent<ViewController>();

        [InfoBox("在绑定的ViewController脚本中可编辑器添加由字符串做标识的参数类型为IEventArg的事件注册器 注册器不会与特性标记的事件共享生命周期")]
        [SerializeField,LabelText("事件设置中心"),ListDrawerSettings()]
        internal List<EventCenter> centers = new List<EventCenter>();
    }   
}
