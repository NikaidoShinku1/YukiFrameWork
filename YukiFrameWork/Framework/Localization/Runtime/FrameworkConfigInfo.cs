using UnityEngine;
using Sirenix.OdinInspector;
namespace YukiFrameWork
{
    public class FrameworkConfigInfo : ScriptableObject
    {
        [ReadOnly, SerializeField]
        public string scriptName;
        [ReadOnly, SerializeField]
        public string nameSpace = "YukiFrameWork.Example";
        [ReadOnly, SerializeField]
        public string genericPath = "Assets/Scripts";
        [ReadOnly, SerializeField]
        public bool IsParent;
        [ReadOnly, SerializeField]
        public string parentName = "MonoBehaviour";
        [ReadOnly, SerializeField]
        public string assembly = "Assembly-CSharp";
        [ReadOnly, SerializeField]
        public string[] assemblies = new string[0];
        [ReadOnly, SerializeField]
        public LocalizationConfigBase configBases;
        [ReadOnly, SerializeField]
        public YDictionary<int, LocalizationConfigBase> dependConfigs = new YDictionary<int, LocalizationConfigBase>();
        [ReadOnly, SerializeField]
        public Language defaultLanguage;
    }
}