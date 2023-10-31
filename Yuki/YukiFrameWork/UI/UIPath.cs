using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YukiFrameWork.Res;

namespace YukiFrameWork.UI
{
    [System.Serializable]
    public class UIPath
    {
        [NonSerialized]
        public Attribution type;      
       
        public string UIPanelPath;
        public string assetBundleName;
        public UIPath() { }

        public UIPath(string UIPanelPath,string assetBundleName)
        {
            this.UIPanelPath = UIPanelPath;
            this.assetBundleName = assetBundleName;           
        }
    }
}