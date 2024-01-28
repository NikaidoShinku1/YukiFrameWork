using System;
using UnityEngine;
namespace YukiFrameWork.Knapsack
{  
    public enum LoadType
    {
        ABManager,
        Resources
    }

    [Serializable]
    public class ItemConfigData : ScriptableObject
    {
        [HideInInspector]
        [SerializeField] private string projectName;

        [HideInInspector]
        [SerializeField] private string projectPath;
      
        [SerializeField] private LoadType loadType = LoadType.ABManager;

        [HideInInspector]
        [SerializeField] private ItemUI handItemPrefab;     

        public string ProjectName
        {
            get => projectName;
            set => projectName = value;
        }      

        public LoadType LoadType
        {
            get => loadType;
            set => loadType = value;
        }

        public string ProjectPath
        {
            get => projectPath;
            set => projectPath = value;
        }

        public ItemUI HandItemPrefab
        {
            get => handItemPrefab;
            set => handItemPrefab = value;
        }
    }
}
