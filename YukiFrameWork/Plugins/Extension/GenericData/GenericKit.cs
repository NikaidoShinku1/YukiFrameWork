using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YukiFrameWork.Extension
{
    public class GenericKit
    {
        public static string ScriptsInfoChange(string info,GenericDataBase data,string description = "")
        {
            info = info.Replace("YukiFrameWork.Project", data.ScriptNamespace);
            info = info.Replace("Yuki@qq.com", data.CreateEmail);
            info = info.Replace("xxxx年x月xx日 xx:xx:xx", data.SystemNowTime);
            info = info.Replace("#SCRIPTNAME#", data.ScriptName);

            if (!string.IsNullOrEmpty(description))
                info = info.Replace("这是一个框架工具创建的脚本", description);
            return info;
        }
    }
}