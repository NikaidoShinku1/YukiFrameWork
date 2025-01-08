using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XFABManager
{

    /// <summary>
    /// 根据更新模式 检测本地资源 判断本地是否 有资源 是否有内置资源 给出更新方案
    /// </summary>
    public class CheckLocalResRequest : CustomAsyncOperation<CheckLocalResRequest>
    {

        /// <summary>
        /// 根据更新模式 检测本地资源 判断本地是否 有资源 是否有内置资源 给出更新方案
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name=""></param>
        public static CheckLocalResRequest Check(string projectName, CheckUpdateResult result) 
        {
            CheckLocalResRequest request = new CheckLocalResRequest();
            CoroutineStarter.Start(request.CheckLocalRes(projectName, result));
            return request; 
        }

        /// <summary>
        /// 检测本地资源
        /// </summary>
        private IEnumerator CheckLocalRes(string projectName, CheckUpdateResult result)
        { 
            // 如果是编辑器  并且 从 Assets 加载资源 
            // 这种情况下 是 不需要AssetBundle的 
#if UNITY_EDITOR
            if (AssetBundleManager.GetProfile(projectName).loadMode == LoadMode.Assets)
            {
                result.updateType = UpdateType.DontNeedUpdate;
                Completed();
                yield break;
            }
#endif
            // 是否为更新模式
            bool isUpdate = AssetBundleManager.GetProfile(projectName).updateModel == UpdateMode.UPDATE;
            // 判断是否有内置资源
            IsHaveBuiltInResRequest isHaveBuiltIn = AssetBundleManager.IsHaveBuiltInRes(projectName);
            yield return isHaveBuiltIn;
            
#if XFABMANAGER_LOG_OPEN_TESTING
            Debug.LogFormat("CheckResUpdate CheckLocalRes isUpdate:{0} isHaveBuiltIn:{1} IsHaveResOnLocal:{2} StreamingAssetsReadable:{3}", isUpdate, isHaveBuiltIn.isHave, AssetBundleManager.IsHaveResOnLocal(projectName), XFABTools.StreamingAssetsReadable());
#endif

            if (isUpdate)
            {
                // 更新模式
                if (AssetBundleManager.IsHaveResOnLocal(projectName))
                {
                    // 数据目录有资源
                    result.updateType = UpdateType.Update;
                }
                else
                {
                    // 数据目录没资源 
                    result.updateType = isHaveBuiltIn.isHave ? UpdateType.Update : UpdateType.Download; // Update和Download的区别是 Download会去检测服务端有没有.zip的文件
                }
            }
            else
            {
                // 非更新模式 
                // 判断有没有内置资源 如果没有内置资源则认为缺失资源(非更新模式必须要有内置的资源)
                if (!isHaveBuiltIn.isHave)
                {
                    result.message = string.Format("缺少{0}资源！", projectName);
                    result.updateType = UpdateType.Error; // 非更新模式又没有内置资源，此时认为资源缺失!Error
                    Completed(result.message);
                    yield break;
                }

                // 判断内置目录是否可读
                if (XFABTools.StreamingAssetsReadable())
                {
                    // 非更新模式下内置目录可读，可以直接使用内置目录的资源，不需要做任何处理
                    result.updateType = UpdateType.DontNeedUpdate;
                    Completed();
                    yield break;
                }

                // 读取内置文件的信息
                ReadBuildInProjectBuildInfoRequest request_build_in_info = ReadBuildInProjectBuildInfoRequest.BuildInProjectBuildInfo(projectName);
                yield return request_build_in_info;
                if (!string.IsNullOrEmpty(request_build_in_info.error))
                {
                    Completed(request_build_in_info.error);
                    yield break;
                }
                ProjectBuildInfo buildInfo = request_build_in_info.ProjectBuildInfo;

#if XFABMANAGER_LOG_OPEN_TESTING
                System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
                stopwatch.Restart();
#endif


                bool isNeedExtract = false;

                // 需要释放的文件
                List<BundleInfo> need_update_bundles = new List<BundleInfo>();

                int count = 0;

                foreach (var item in buildInfo.bundleInfos)
                {
                    if (string.IsNullOrEmpty(item.bundleName))
                        continue;

                    string local_md5 = LocalAssetBundleInfoManager.Instance.GetAssetBundleMd5(projectName, item.bundleName);
                    if (!item.md5.Equals(local_md5))
                    {
                        isNeedExtract = true;
                        need_update_bundles.Add(item);
                    }

                    // 每对比200个文件等一帧，防止卡顿
                    count++;
                    if (count >= 200)
                    {
                        yield return null;
                        count = 0;
                    }

                }

                // 需要更新的文件
                result.need_update_bundles = need_update_bundles.ToArray();
                result.updateType = isNeedExtract ? UpdateType.ExtractLocal : UpdateType.DontNeedUpdate;

#if XFABMANAGER_LOG_OPEN_TESTING
                stopwatch.Stop();
                Debug.LogFormat("非更新模式释放本地文件md5对比耗时:{0}/秒", stopwatch.ElapsedMilliseconds / 1000.0f);
#endif 
            }

            // 完成请求
            Completed();

        }

    }

}

