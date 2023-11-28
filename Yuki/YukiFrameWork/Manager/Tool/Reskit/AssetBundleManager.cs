using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using YukiFrameWork.Pools;
using YukiFrameWork;
public class AssetBundleManager : Singleton<AssetBundleManager>
{
    //资源关系依赖配置表，可以根据Crc来锁定资源
    protected Dictionary<uint, ResourcesItem> resourcesItemDic = new Dictionary<uint, ResourcesItem>();

    //储存已经加载的ab包，根据crc来判定
    protected Dictionary<uint, AssetBundleItem> assetBundleItemDic = new Dictionary<uint, AssetBundleItem>();

    public Dictionary<uint, AssetBundleItem> AssetBundleItemDic => assetBundleItemDic;

    public Dictionary<uint, ResourcesItem> ResourcesItemDic => resourcesItemDic;


    /// <summary>
    /// 根据路径Crc加载中间类ResourcesItem
    /// </summary>
    /// <param name="crc"></param>
    /// <returns></returns>
    public ResourcesItem LoadResourceAssetBundle(uint crc)
    {
        if (!resourcesItemDic.TryGetValue(crc, out var item))
        {
            Debug.LogError($"LoadResouecesAssetBundle error: can not find crc {crc} in AssetBundleConfig");
            return item;
        }

        if (item.AssetBundle != null)
            return item;

        item.AssetBundle = LoadAssetBundle(item.AssetBundleName);

        if (item.DependAssetBundle != null)
        {
            for (int i = 0; i < item.DependAssetBundle.Count; i++)
            {
                LoadAssetBundle(item.DependAssetBundle[i]);
            }
        }
        return item;
    }

    public ResourcesItem FindResourcesItem(uint crc)
    {
        resourcesItemDic.TryGetValue(crc, out var item);
        return item;
    }

    private AssetBundle LoadAssetBundle(string assetBundleName)
    {
        AssetBundleItem item = null;
        uint crc = CRC32.GetCRC32(assetBundleName);

        if (!assetBundleItemDic.TryGetValue(crc, out item))
        {
            AssetBundle assetBundle = null;
            string path = Application.streamingAssetsPath + @"\" + assetBundleName;
            if (File.Exists(path))
            {
                assetBundle = AssetBundle.LoadFromFile(path);
            }

            if (assetBundle == null)
            {
                Debug.LogError("Load AssetBundle Error: " + path);
            }

            item = AssetBundleItem.Get();
            item.AssetBundle = assetBundle;
            item.RefCount++;
            assetBundleItemDic.Add(crc, item);
            return assetBundle;
        }
        else item.RefCount++;
        return item.AssetBundle;
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    /// <param name="item"></param>
    public void ReleaseAsset(ResourcesItem item)
    {
        if (item == null) return;

        if (item.DependAssetBundle != null && item.DependAssetBundle.Count > 0)
        {
            for (int i = 0; i < item.DependAssetBundle.Count; i++)
            {
                UnLoadAssetBundle(item.DependAssetBundle[i]);
            }
        }
        UnLoadAssetBundle(item.AssetBundleName);
    }

    public void UnLoadAssetBundle(string name)
    {
        AssetBundleItem item = null;
        uint crc = CRC32.GetCRC32(name);

        if (assetBundleItemDic.TryGetValue(crc, out item))
        {
            item.RefCount--;
            if (item.RefCount <= 0 && item.AssetBundle != null)
            {
                item.AssetBundle.Unload(true);
                item.Release();
                assetBundleItemDic.Remove(crc);
            }
        }
    }
}

public class AssetBundleItem
{
    public AssetBundle AssetBundle { get; set; } = null;
    public int RefCount { get; set; } = 0;

    private static SimpleObjectPools<AssetBundleItem> simpleObjectPools
        = new YukiFrameWork.Pools.SimpleObjectPools<AssetBundleItem>(() => 
        {
            return new AssetBundleItem();
        },ab => ab.Reset(),100);

    public void Release() => simpleObjectPools.Release(this);

    public static AssetBundleItem Get() => simpleObjectPools.Get();

    public void Reset()
    {
        AssetBundle = null;
        RefCount = 0;
    }
}

public class ResourcesItem
{
    //资源路径的Crc
    public uint Crc { get; set; } = 0;

    //资源的文件名称
    public string AssetName { get; set; } = string.Empty;

    //资源的所在ab包名
    public string AssetBundleName { get; set; } = string.Empty;

    //该资源所依赖的ab包
    public List<string> DependAssetBundle { get; set; } = null;

    //资源加载完的ab包
    public AssetBundle AssetBundle { get; set; } = null;

    public Object Obj { get; set; } = null;

    public int UID { get; set; } = 0;

    // 资源最后使用的时间   
    public float LastUseTime { get; set; } = 0f;

    // 引用计数   
    protected int refCount = 0;

    //是否在跳转场景的时候清除这个物品类
    public bool Clear { get; set; } = true;

    public int RefCount
    {
        get => refCount;
        set
        {
            refCount = value;
            if(refCount < 0)
            {
                Debug.LogError("refCount < 0" + refCount + "," + (Obj != null ? Obj.name : "Obj is Null"));
            }
        }
    }
}
