using UnityEngine;
using Sirenix.OdinInspector;
namespace YukiFrameWork
{
    public class FrameworkConfigInfo : ScriptableObject
    {
        [HideInInspector]
        public string scriptName;
        [HideInInspector]
        public string nameSpace = "YukiFrameWork.Example";
        [HideInInspector]
        public string genericPath = "Assets/Scripts";
        [HideInInspector]
        public bool IsParent;
        [HideInInspector]
        public string parentName = "MonoBehaviour";
        [HideInInspector]
        public string assembly = "Assembly-CSharp";
        [ShowIf(nameof(SelectIndex),2)]
        public string[] assemblies = new string[0];
        [SerializeField,LabelText("Local Config:"), ShowIf(nameof(SelectIndex), 1)]
        public LocalizationConfigBase configBases;
        [LabelText("运行时的默认语言:"), PropertySpace(6), ShowIf(nameof(SelectIndex), 1)]
        public Language defaultLanguage;
        [LabelText("子配置项"),PropertySpace, ShowIf(nameof(SelectIndex), 1)]
        public YDictionary<int, LocalizationConfigBase> dependConfigs = new YDictionary<int, LocalizationConfigBase>();   
        [HideInInspector]
        public int SelectIndex = 0;
    }
}