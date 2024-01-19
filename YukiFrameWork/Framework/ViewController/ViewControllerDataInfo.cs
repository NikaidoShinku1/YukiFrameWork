using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YukiFrameWork.Extension
{
    [Serializable]
    public class ViewControllerDataInfo
    {
        public bool IsEN = false;

        public string TitleTip => !IsEN ? "脚本生成设置:" : "Scripts Generation Settings:";

        public string Email => !IsEN ? "邮箱:" : "Email:";

        public string NameSpace => !IsEN ? "命名空间:" : "NameSpace:";

        public string Name => !IsEN ? "脚本名称:" : "Script Name:";

        public string Path => !IsEN ? "生成路径:" : "Generate Path:";

        public string SelectScriptBtn => !IsEN ? "选择脚本" : "Select Scripts";

        public string OpenScriptBtn => !IsEN ? "打开脚本" : "Open Scripts";

        public string GenerateScriptBtn => !IsEN ? "生成脚本" : "Generate Scripts";

        public string PrefabGenerationPath => !IsEN ? "Prefab 生成路径:" : "Prefab Generate Path:";

        public string DescriptionInfo => !IsEN ? "将文件夹拖入这个区间" : "Drag the folder into this interval";

        public string CreatePrefabInfo => !IsEN ? "创建Prefab" : "Create Prefab";

        public string Update_PrefabInfo => !IsEN ? "更新Prefab" : "Update Prefab";

        public string AddEventInfo => !IsEN ? "添加事件可视化注册器" : "Add an event visual registry";


    }
}