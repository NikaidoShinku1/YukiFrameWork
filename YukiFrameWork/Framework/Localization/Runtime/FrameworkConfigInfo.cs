using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
namespace YukiFrameWork
{
    public class FrameworkConfigInfo : ScriptableObject
    {
        [ReadOnly]
        public string scriptName;
        [ReadOnly]
        public string nameSpace = "YukiFrameWork.Example";
        [ReadOnly]
        public string genericPath = "Assets/Scripts";
        [ReadOnly]
        public bool IsParent;
        [ReadOnly]
        public string parentName = "MonoBehaviour";
        [ReadOnly]
        public string assembly = "Assembly-CSharp";
        [ReadOnly]
        public string[] assemblies = new string[0];
        [ReadOnly]
        public LocalizationConfigBase configBases;
        [ReadOnly]
        public Language DefaultLanguage;
    }
}