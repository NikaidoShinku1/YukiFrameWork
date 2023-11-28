using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR

namespace YukiFrameWork.Res.Editors
{
    [CreateAssetMenu(fileName = "ABConfig", menuName = "CreateABConfig")]
    public class ABConfig : ScriptableObject
    {
        [field: SerializeField] 
        [field:Header("预制体的文件夹路径(以Prefab为包名打包ab包)")]
        public List<string> AllPrefabPath { get; private set; } = new List<string>();

        [field:Header("默认自带配置表打包路径(如有变动请自行修改)")]      
        [field: SerializeField]
        [field: Header("AB包包名以及存放路径(指定路径(文件夹)打对应ab包)")]
        public List<ABPackageFile> ABPackageFiles { get; private set; } = new List<ABPackageFile>()
        {
            new ABPackageFile
            {
                abName = "assetbundleconfig",
                abPath = "Assets/GameData/Data/ABDepend"
            }
        };
    }
    [System.Serializable]
    public struct ABPackageFile
    {
        public string abName;

        public string abPath;
    }
}
#endif