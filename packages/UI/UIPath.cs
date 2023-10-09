using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YukiFrameWork.UI
{
    [System.Serializable]
    public class UIPath
    {
        public string UIPanelPath;

        public UIPath() { }

        public UIPath(string UIPanelPath)
        {
            this.UIPanelPath = UIPanelPath;
        }
    }
}