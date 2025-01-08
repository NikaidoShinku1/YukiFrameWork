using System.Collections;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Runtime.CompilerServices;
using UnityEngine;
using XFABManager;


internal struct PreloadInfo
{ 
    public string projectName;
    public string assetName;
    public bool autoUnload;

    internal PreloadInfo(string projectName,string assetName,bool autoUnload) 
    {
        this.projectName = projectName;
        this.assetName = assetName;
        this.autoUnload = autoUnload;
    }
    
}

internal class GameObjectPreload  
{

    private static bool preLoading = false;

    private static Queue<PreloadInfo> needPreLoadAssets = new Queue<PreloadInfo>();

    /// <summary>
    /// 预加载
    /// </summary>
    /// <param name="projectName">模块名</param>
    /// <param name="assetName">资源名</param>
    internal static void Preload(string projectName, string assetName)
    {
        Preload(projectName, assetName, true);
    }

    /// <summary>
    /// 预加载
    /// </summary>
    /// <param name="projectName">模块名</param>
    /// <param name="assetName">资源名</param>
    /// <param name="autoUnload">是否自动卸载</param>
    internal static void Preload(string projectName, string assetName, bool autoUnload)
    { 
        // 入队列
        needPreLoadAssets.Enqueue(new PreloadInfo(projectName,assetName,autoUnload));

        if (!preLoading)
        {
            preLoading = true; 
            CoroutineStarter.Start(PreLoadAsset());
        }
    }

    /// <summary>
    /// 如果预加载时选择的是不自动卸载，可通过此方法设置为自动卸载
    /// </summary>
    /// <param name="projectName"></param>
    /// <param name="assetName"></param>
    internal static void UnPreload(string projectName, string assetName)
    {
        foreach (var pool in GameObjectLoader.allPools.Values)
        {
            if (string.IsNullOrEmpty(pool.ProjectName) || string.IsNullOrEmpty(pool.AssetName))
                continue;
            if (pool.ProjectName.Equals(projectName) && pool.AssetName.Equals(assetName)) 
            {
                pool.IsAutoUnload = true;
                break;
            }
        }
    }


    internal static bool IsPreloading() {

        return preLoading;
    }
    private static IEnumerator PreLoadAsset()
    {
        while (needPreLoadAssets.Count > 0)
        {
            PreloadInfo info = needPreLoadAssets.Dequeue();

            GameObject prefab = AssetBundleManager.LoadAsset<GameObject>(info.projectName, info.assetName);

            if (prefab)
            {
                // 添加到 GameObjectLoader的对象池中
                GameObjectPool pool = GameObjectLoader.GetOrCreatePool(prefab);
                if (pool != null)
                {
                    pool.ProjectName = info.projectName;
                    pool.AssetName = info.assetName;
                    pool.IsAutoUnload = info.autoUnload;

#if XFABMANAGER_LOG_OPEN_TESTING
                        Debug.LogFormat("资源预加载成功:{0} asset:{1}", info.projectName, info.assetName);
#endif
                }
            }

            yield return null;
        }


        preLoading = false;
    }


}
