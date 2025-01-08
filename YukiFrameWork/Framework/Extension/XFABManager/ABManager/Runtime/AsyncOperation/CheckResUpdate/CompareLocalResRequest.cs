using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace XFABManager
{
    /// <summary>
    /// 与本地的文件比较 判断是否需要更新
    /// </summary>
    public class CompareLocalResRequest : CustomAsyncOperation<CompareLocalResRequest>
    {

        public static CompareLocalResRequest Compare(string projectName, BundleInfo[] serverBundleInfos, CheckUpdateResult result) 
        { 
            CompareLocalResRequest compareLocalResRequest = new CompareLocalResRequest();
            CoroutineStarter.Start(compareLocalResRequest.CompareWithLocal(projectName, serverBundleInfos,result)); 
            return compareLocalResRequest;  
        }

        /// <summary>
        /// 与本地的文件比较 判断是否需要更新
        /// </summary>
        /// <param name="projectName">资源模块名</param>
        /// <param name="serverBundleInfos">服务端bundle列表信息</param>
        private IEnumerator CompareWithLocal(string projectName, BundleInfo[] serverBundleInfos, CheckUpdateResult result)
        {
#if XFABMANAGER_LOG_OPEN_TESTING 
            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
            stopwatch.Restart();
#endif

            // 需要更新的文件列表
            List<BundleInfo> need_update_files = new List<BundleInfo>();

            int compare_md5_count = 0;

            for (int i = 0; i < serverBundleInfos.Length; i++)
            {

                if (string.IsNullOrEmpty(serverBundleInfos[i].bundleName))
                    continue;

                bool isNeedUpdate = false;
                // 判断本地是否存在
                string localFile = XFABTools.LocalResPath(projectName, serverBundleInfos[i].bundleName);
                string buildInPath = XFABTools.BuildInDataPath(projectName, serverBundleInfos[i].bundleName);

                // 该文件只要在数据目录或者内置目录其中一个存在就认为本地有该文件（如果内置目录不能读即使有也会返回false）
                if (File.Exists(localFile) || File.Exists(buildInPath))
                {
                    // 判断md5值是否相同 
                    string local_md5 = LocalAssetBundleInfoManager.Instance.GetAssetBundleMd5(projectName, serverBundleInfos[i].bundleName);

                    if (string.IsNullOrEmpty(local_md5) && File.Exists(localFile))
                    {
                        // 计算md5
                        FileMD5Request request_md5 = XFABTools.CaculateFileMD5(localFile);
                        yield return request_md5;
                        if (string.IsNullOrEmpty(request_md5.error))
                        {
                            local_md5 = request_md5.md5;
                            // 保存计算的值
                            LocalAssetBundleInfoManager.Instance.SetAssetBundleMd5(projectName, serverBundleInfos[i].bundleName, local_md5);
                        }
                    }

                    if (!serverBundleInfos[i].md5.Equals(local_md5))
                    {
#if XFABMANAGER_LOG_OPEN_TESTING
                        UnityEngine.Debug.LogFormat("CheckUpdate MD5 校验失败! file:{0} local_md5:{1} server_md5:{2}", localFile, local_md5, serverBundleInfos[i].md5);
#endif

                        isNeedUpdate = true;
                    }

                    // 每比较200个文件的md5就挂起一帧防止卡顿
                    compare_md5_count++;
                    if (compare_md5_count >= 200)
                    {
                        compare_md5_count = 0;
                        yield return null;
                    }
                }
                else
                    // 本地没有 需要更新
                    isNeedUpdate = true;


                if (isNeedUpdate)
                {
                    result.updateSize += serverBundleInfos[i].bundleSize;
                    need_update_files.Add(serverBundleInfos[i]);
                }
            }

#if XFABMANAGER_LOG_OPEN_TESTING
            stopwatch.Stop();
            Debug.LogFormat("对比本地文件Md5耗时:{0}/秒", stopwatch.ElapsedMilliseconds / 1000.0f);
#endif

            // 判断是否需要更新
            if (result.updateSize == 0 || need_update_files.Count == 0)
            {
                result.updateType = UpdateType.DontNeedUpdate;
                // 更新一下本地版本号
                XFABTools.SaveVersion(result.projectName, result.version);
            }
            else
            {
                // 判断是否有内置资源
                IsHaveBuiltInResRequest isHaveBuiltIn = AssetBundleManager.IsHaveBuiltInRes(result.projectName);
                yield return isHaveBuiltIn;
                // 如果有内置资源并且内置目录不可写才需要判断(如果可写内置目录与数据目录是一个，再判断没有意义)
                if (isHaveBuiltIn.isHave && !XFABTools.StreamingAssetsWritable())
                {
                    ReadBuildInProjectBuildInfoRequest request_build_in_info = ReadBuildInProjectBuildInfoRequest.BuildInProjectBuildInfo(result.projectName);
                    yield return request_build_in_info;
                    if (!string.IsNullOrEmpty(request_build_in_info.error))
                    {
                        Completed(request_build_in_info.error);
                        yield break;
                    }
                    ProjectBuildInfo buildInfo = request_build_in_info.ProjectBuildInfo;

                    List<BundleInfo> need_extra = new List<BundleInfo>();
                    foreach (var item in need_update_files)
                    {
                        if (buildInfo.EqualBundleFile(item.bundleName, item.md5))
                        {
                            need_extra.Add(item);
                        }
                    }

                    if (need_extra.Count != 0)
                    {

                        // 把需要更新的资源剔除 ，剔除之后如果还有需要更新的 ， 那就更新 ，如果没了才释放
                        foreach (var item in need_extra)
                        {
                            need_update_files.Remove(item);
                            result.updateSize -= item.bundleSize;
                        }

                        if (need_update_files.Count == 0 || result.updateSize == 0)
                        {
                            //如果需要更新的文件全部都内置了，这个时候只需要释放资源就可以
                            //否则就把没有内置的资源更新下来就可以了
                            result.updateType = UpdateType.ExtractLocal;
                            result.need_update_bundles = need_extra.ToArray();

                            Completed(); 
                            yield break;
                        } 
                    }
                }
            }

            result.need_update_bundles = need_update_files.ToArray();
            Completed();
        }

    }

}

