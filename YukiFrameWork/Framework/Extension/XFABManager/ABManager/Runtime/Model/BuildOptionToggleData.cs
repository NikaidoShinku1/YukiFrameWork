#if UNITY_EDITOR 

using UnityEditor;
using UnityEngine;

namespace XFABManager
{
    [System.Serializable]
    public class BuildOptionToggleData
    {

        public string title;
        public string tooltip;
        public bool isOn;
        public GUIContent content { get; set; }

        public BuildAssetBundleOptions option;

        public BuildOptionToggleData(string title, string tooltip, bool isOn, BuildAssetBundleOptions option)
        {
            this.title = title;
            this.tooltip = tooltip;
            this.isOn = isOn;
            content = new GUIContent(this.title, this.tooltip);
            this.option = option;
        }

    }
}

#endif