using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YukiFrameWork
{
    public class LocalGenericScriptInfo : ScriptableObject
    {
        public string scriptName;
        public string nameSpace = "YukiFrameWork.Example";
        public string genericPath = "Assets/Scripts";

        public bool IsParent;
        public string parentName = "MonoBehaviour";

        public string assembly = "Assembly-CSharp";
    }
}