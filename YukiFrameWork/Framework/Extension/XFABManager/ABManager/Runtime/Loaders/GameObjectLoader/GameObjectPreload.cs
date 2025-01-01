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
        ExecuteMultipleAsyncOperation<LoadAssetRequest> operation = new ExecuteMultipleAsyncOperation<LoadAssetRequest>(30);

        while (preLoading)
        {
            while (operation.CanAdd() && needPreLoadAssets.Count > 0) 
            {
                PreloadInfo info = needPreLoadAssets.Dequeue();
                
                LoadAssetRequest request = AssetBundleManager.LoadAssetAsync<GameObject>(info.projectName, info.assetName);
                if (request == null)
                {
                    Debug.LogWarningFormat("资源 {0}/{1} 预加载失败!", info.projectName, info.assetName);
                    continue;
                }
                request.AddCompleteEvent((r) =>
                {
                    GameObject prefab = r.asset as GameObject;
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
                            Debug.LogFormat("资源预加载成功:{0} asset:{1}",info.projectName,info.assetName);
#endif 
                        }
                    }
                });

                operation.Add(request);
            }


            if (operation.IsDone() && needPreLoadAssets.Count == 0) 
            {
                // 说明全部加载完成 直接跳出循环即可
                preLoading = false;
                yield break;
            }
            
            yield return null;
        }

    }

}
