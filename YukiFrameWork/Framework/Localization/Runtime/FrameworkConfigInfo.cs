using UnityEngine;
using Sirenix.OdinInspector;
namespace YukiFrameWork
{
    [HideMonoScript]
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
        [LabelText("运行时的默认语言:"), PropertySpace(6), ShowIf(nameof(SelectIndex), 1)]
        public Language defaultLanguage;
        [LabelText("本地配置"),PropertySpace, ShowIf(nameof(SelectIndex), 1)]
        public YDictionary<string, LocalizationConfigBase> dependConfigs = new YDictionary<string, LocalizationConfigBase>();   
        [HideInInspector]
        public int SelectIndex = 0;
    }
}