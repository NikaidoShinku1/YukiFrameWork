
using System.Collections;
using UnityEngine;

namespace XFABManager
{
    public class LoadAssetBundleDependRequest : CustomAsyncOperation<LoadAssetBundleDependRequest>
    {


        public static LoadAssetBundleDependRequest LoadAssetBundle(string projectName, string bundleName, string dependence, string suffix)
        {
            string key = string.Format("LoadAssetBundleDependRequest:{0}{1}{2}{3}", projectName, bundleName,dependence,suffix);
            return AssetBundleManager.ExecuteOnlyOnceAtATime<LoadAssetBundleDependRequest>(key, () =>
            {
                LoadAssetBundleDependRequest request = new LoadAssetBundleDependRequest();
                CoroutineStarter.Start(request.Load(projectName, bundleName,dependence,suffix));
                return request;
            });
        }


        internal IEnumerator Load(string projectName,string bundleName,string dependence,string suffix) 
        {
            if (AssetBundleManager.IsLoadedAssetBundle(projectName, dependence))
            {
                AssetBundleManager.AddDependencesBundles(AssetBundleManager.AssetBundles[projectName][dependence], projectName, bundleName, dependence);
                Completed();
                yield break;
            }
            string bundlePath = AssetBundleManager.GetAssetBundlePath(projectName, dependence, suffix);
            AssetBundleCreateRequest request = AssetBundleManager.LoadAssetBundleFromFilePathAsync(bundlePath, projectName);

            //yield return request;
            while (request != null && !request.isDone)
            {
                yield return null; 
            }

            if (request != null && request.assetBundle != null)
            {
                // 加载成功
                AssetBundleManager.AssetBundles[projectName].Add(dependence, request.assetBundle);
                // 添加依赖信息
                AssetBundleManager.AddDependencesBundles(request.assetBundle, projectName, bundleName, dependence);
                // 完成异步操作
                Completed();
            }
            else
            {
                if (File.Exists(bundlePath))
                    Completed(string.Format("AssetBundle:{0}加载失败,请检测文件是否损坏或密钥是否填写正确!", bundlePath));
                else
                    Completed(string.Format("AssetBundle:{0}加载失败!", bundlePath));
            }
        }


    }

}

