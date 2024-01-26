using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YukiFrameWork.Extension
{
    [Serializable]
    public class GenericScriptDataInfo
    {
        public static bool IsEN
        {
            get => PlayerPrefs.GetInt("ViewControllerIsEN") == 1;
            set => PlayerPrefs.SetInt("ViewControllerIsEN", value ? 1 : 0);
        }

        public static string TitleTip => !IsEN ? "脚本生成设置:" : "Scripts Generation Settings:";

        public static string Email => !IsEN ? "邮箱:" : "Email:";

        public static string NameSpace => !IsEN ? "命名空间:" : "NameSpace:";

        public static string Name => !IsEN ? "脚本名称:" : "Script Name:";

        public static string Path => !IsEN ? "生成路径:" : "Generate Path:";

        public static string SelectScriptBtn => !IsEN ? "选择脚本" : "Select Scripts";

        public static string OpenScriptBtn => !IsEN ? "打开脚本" : "Open Scripts";

        public static string GenerateScriptBtn => !IsEN ? "生成脚本" : "Generate Scripts";

        public static string OpenPartialScriptBtn => !IsEN ? "打开分写脚本" : "Open the life cycle script";   
       
        public static string AddEventInfo => !IsEN ? "添加事件可视化注册器" : "Add an event visual registry";

        public static string EventAudioMationInfo => !IsEN ? "事件自动化注册,在自动化注册架构前会自动处理\nTip:如不采用自动化注册架构请改用Awake注册" : "Automated registration of events, processed automatically before automated registration schema \nTip: Use Awake registration instead of automated registration schema";

        public static string EventAwakeInfo => !IsEN ? "事件在Awake生命周期注册" : "Events are registered in the Awake lifecycle";

        public static string AutoInfo => !IsEN ? "创建时架构自动化:" : "Architecture Automation at Creation time:";

        public static string AssemblyInfo => !IsEN ? "架构是否存在于自定义程序集:" : "Does the schema exist in the custom assembly:";

        public static string BindExtensionInfo => !IsEN ? "字段绑定拓展:" : "Field binding extension:";

        public static string DragObjectInfo => !IsEN ? "将对象拖入这个区间:" : "Drag the object into this interval:";
    }
}