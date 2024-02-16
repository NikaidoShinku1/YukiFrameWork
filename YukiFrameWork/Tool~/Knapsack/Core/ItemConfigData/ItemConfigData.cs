using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
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
        [SerializeField,HideInInspector] private string projectName;

        [SerializeField, HideInInspector] private string projectPath;

        [SerializeField, HideInInspector] private LoadType loadType = LoadType.ABManager;

        [SerializeField, HideInInspector] private ItemUI handItemPrefab;

        [HideInInspector]
        public string genericPath = "Assets/Scripts/Knapsack";

        [HideInInspector]
        public string genericName = "CustomData";

        [HideInInspector]
        public List<string> itemDatas = new List<string>();

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

#if UNITY_EDITOR
        public void Save()
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
#endif
    }
}
