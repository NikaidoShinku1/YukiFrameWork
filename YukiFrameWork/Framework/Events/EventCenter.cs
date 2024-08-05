using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using YukiFrameWork.Extension;

namespace YukiFrameWork.Events
{  

    [Serializable]
    internal class EventCenter
    {   
        [LabelText("事件标识:")]
        public string eventName; 
           
        [InfoBox("事件的注销进行生命周期绑定")]
        public UnRegisterType unRegisterType;
     
        [LabelText("可视化附加事件")]
        public UnityEvent<IEventArgs> onEvent = new UnityEvent<IEventArgs>();   
        
    }     
}
