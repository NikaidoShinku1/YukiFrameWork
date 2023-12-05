using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace YukiFrameWork.Res
{
    public enum LoadMode
    {
        同步,
        异步
    }

    public enum ResourceType
    {
        Resources,
        ResKit资源管理套件
    }

    public class ResKit
    {     
        private static AssetBundleConfig assetBundleConfig;

        private static bool isInit = false;
       
        /// <summary>
        /// 保存所有正在使用的资源加载器
        /// </summary>
        private readonly static Dictionary<ulong, ResLoader> loaderDict = new Dictionary<ulong, ResLoader>();

        public static bool Init()
        {        
            if (isInit)
            {
                Debug.LogWarning("ResKit has been initialized and does not need to be performed multiple times!");
                return false;
            }
            string path = Application.streamingAssetsPath + @"\assetbundleconfig";

            AssetBundle configAB = AssetBundle.LoadFromFile(path);

            TextAsset text = configAB.LoadAsset<TextAsset>("AssetBundleConfig");

            if (text == null)
            {
                Debug.LogError("AssetBundleConfig is no Exist!");
                return false;
            }

            MemoryStream ms = new MemoryStream(text.bytes);

            BinaryFormatter bf = new BinaryFormatter();
            AssetBundleConfig bundleConfig = bf.Deserialize(ms) as AssetBundleConfig;
            for (int i = 0; i < bundleConfig.ABList.Count; i++)
            {
                ResourcesItem item = new ResourcesItem();
                item.Crc = bundleConfig.ABList[i].Crc;
                item.AssetName = bundleConfig.ABList[i].AssetsName;
                item.AssetBundleName = bundleConfig.ABList[i].ABName;
                item.DependAssetBundle = bundleConfig.ABList[i].ABDependce;
                AssetBundleManager.Instance.ResourcesItemDic.Add(item.Crc, item);               
            }
            ms.Close();
            assetBundleConfig = bundleConfig;
            isInit = true;
            return true;
        }

        public static ResLoader GetLoader()
        {
            ulong id = 0;
            bool isPossess = loaderDict.ContainsKey(id);
            while (isPossess)
            {
                id++;
                isPossess = loaderDict.ContainsKey(id);
            }           
            var loader = ResLoader.GetLoader(assetBundleConfig.LoadFromAssetBundle,id);
            loaderDict.Add(id, loader);
            return loader;
        }

        public static void Release(ResLoader resLoader)
        {
            if (resLoader == null) return;

            if (loaderDict.ContainsKey(resLoader.LoaderID))
            {
                loaderDict.Remove(resLoader.LoaderID);
                resLoader.Release();
            }
            else
            {
                Debug.LogError("回收失败，当前资源加载器不属于使用序列,加载器id：" + resLoader.LoaderID);
            }
            
        }     
    }
}