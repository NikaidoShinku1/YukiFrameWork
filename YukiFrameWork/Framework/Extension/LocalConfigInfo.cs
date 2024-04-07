using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YukiFrameWork
{
    public class LocalConfigInfo : ScriptableObject
    {
        public string scriptName;
        public string nameSpace = "YukiFrameWork.Example";
        public string genericPath = "Assets/Scripts";

        public bool IsParent;
        public string parentName = "MonoBehaviour";

        public string assembly = "Assembly-CSharp";

        public string[] assemblies = new string[0];      
        
    }
}