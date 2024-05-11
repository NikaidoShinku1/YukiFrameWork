using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace XFABManager
{
    public class UpdateOrDownloadResRequest : CustomAsyncOperation<UpdateOrDownloadResRequest>
    {

        /// <summary>
        /// 下载速度 单位:字节
        /// </summary>
        public long CurrentSpeed { get; protected set; }

        /// <summary>
        /// 已下载的文件大小
        /// </summary>
        public long DownloadedSize { get; private set; }

        /// <summary>
        /// 需要下载的大小
        /// </summary>
        public long NeedDownloadedSize { get; private set; }

        /// <summary>
        /// 当前正在执行什么操作 比如:下载文件,解压文件,校验文件等
        /// </summary>
        public ExecutionType ExecutionType { get;private set; }

        internal IEnumerator UpdateOrDownloadRes(string projectName)
        {

            CheckResUpdateRequest requestUpdate = AssetBundleManager.CheckResUpdate(projectName);
            yield return requestUpdate;

            if (!string.IsNullOrEmpty(requestUpdate.error))
            {
                Completed(requestUpdate.error);
                yield break;
            }
            yield return CoroutineStarter.Start(UpdateOrDownloadRes(requestUpdate.result));
        }

        /// <summary>
        /// 更新或下载资源
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        internal IEnumerator UpdateOrDownloadRes(CheckUpdateResult result)
        {

            NeedDownloadedSize = result.updateSize;

            // 判断是不是下载 压缩包
            if (result.updateType == UpdateType.DownloadZip)
            { 
                string zipUrl = XFABTools.ServerPath(AssetBundleManager.GetProfile(result.projectName).url, result.projectName, result.version, string.Format("{0}.zip", result.projectName));
                string zip_md5_url = XFABTools.ServerPath(AssetBundleManager.GetProfile(result.projectName).url, result.projectName, result.version, string.Format("{0}_md5.txt", result.projectName));
                string localfile = XFABTools.LocalResPath(result.projectName, string.Format("{0}{1}", result.projectName, ".zip"));

                // 如果本地没有对应的zip文件，则下载，如果有，就不下载了，直接进入到对比md5的流程
                if (!File.Exists(localfile))
                {
                    ExecutionType = ExecutionType.Download;
                    // 如果下载压缩包，先把本地资源目录清理一下
                    AssetBundleManager.DeleteAssetBundleFiles(result.projectName);
                    // 开始下载
                    DownloadFileRequest requestZip = DownloadFileRequest.Download(zipUrl, localfile);

                    while (!requestZip.isDone)
                    {
                        yield return null;
                        progress = requestZip.progress;
                        CurrentSpeed = requestZip.Speed;
                        DownloadedSize = requestZip.DownloadedSize;
                    }
                    if (!string.IsNullOrEmpty(requestZip.error))
                    {
                        Completed(requestZip.error);
                        yield break;
                    }
                }

                ExecutionType = ExecutionType.Verify;

                // 请求md5
                UnityWebRequest request_zip_md5 = UnityWebRequest.Get(zip_md5_url);
                request_zip_md5.timeout = 5;

                UnityWebRequestAsyncOperation asyncOperation = null;
                try
                {
                    asyncOperation = request_zip_md5.SendWebRequest();
                }
                catch (System.InvalidOperationException e)
                {
                    Completed(string.Format("{0},Non-secure network connections disabled in Player Settings!", e.Message));
                    yield break;
                }
                catch (System.Exception e)
                {
                    Completed(e.ToString());
                    yield break;
                }

                yield return asyncOperation;

                string md5 = null;

#if UNITY_2020_1_OR_NEWER
                if (request_zip_md5.result != UnityWebRequest.Result.Success)
#else
                if (!string.IsNullOrEmpty(request_zip_md5.error))
#endif
                {
                    Completed(request_zip_md5.error);
                    yield break;
                }
                else
                    md5 = request_zip_md5.downloadHandler.text;



                // 解压之前先校验文件是否正确
                FileMD5Request request_local_zip_md5 = XFABTools.CaculateFileMD5(localfile);
                while (!request_local_zip_md5.isDone)
                {
                    yield return null;
                }

                if (!string.IsNullOrEmpty(request_local_zip_md5.error))
                {
                    Completed(request_local_zip_md5.error);
                    yield break;
                }

                if (!md5.Equals(request_local_zip_md5.md5))
                {
                    // md5 校验失败
                    File.Delete(localfile); // 删除本地的文件
                    Completed("md5校验失败!");
                    yield break;
                }

#if XFABMANAGER_LOG_OPEN_TESTING
                UnityEngine.Debug.LogFormat("压缩包md5信息校验成功:{0}", File.Exists(localfile));
#endif

                ExecutionType = ExecutionType.Decompression;



                Task<bool> unzip = ZipTools.UnZipFileAsync(localfile, XFABTools.DataPath(result.projectName),(p)=>progress = p);
                while (!unzip.IsCompleted)
                {
                    yield return null;
                }

                if (!unzip.Result)
                {
                    // 解压异常 删除压缩包
                    File.Delete(localfile);
                    Completed(string.Format("解压出错:{0}!", localfile));
                    yield break;
                }

                // 同步md5 
                string project_build_info_path = XFABTools.LocalResPath(result.projectName, XFABConst.project_build_info);
                if (File.Exists(project_build_info_path))
                {

                    try
                    {
                        ProjectBuildInfo info = JsonConvert.DeserializeObject<ProjectBuildInfo>(File.ReadAllText(project_build_info_path));
                        // 保存md5信息
                        LocalAssetBundleInfoManager.Instance.SyncBundleList(result.projectName, info.bundleInfos);
                        // 保存后缀的信息
                        LocalAssetBundleInfoManager.Instance.SaveSuffix(result.projectName, info.suffix);
                    }
                    catch (System.Exception e)
                    {
                        Completed(string.Format("解析json失败:{0} error:{1}", project_build_info_path, e.ToString()));
                        yield break;
                    }
                }

                // 解压完成之后 删除压缩包
                if (File.Exists(localfile))
                    File.Delete(localfile);
            }
            else if (result.updateType == UpdateType.Update || result.updateType == UpdateType.Download)
            {
                // 更新下载
                string fileUrl = null;
                string localFile = null;

                List<DownloadObjectInfo> files = new List<DownloadObjectInfo>();

                for (int i = 0; i < result.need_update_bundles.Length; i++)
                {
                    fileUrl = XFABTools.ServerPath(AssetBundleManager.GetProfile(result.projectName).url, result.projectName, result.version, result.need_update_bundles[i].bundleName);// 这个文件名是包含后缀的
                    localFile = XFABTools.LocalResPath(result.projectName, result.need_update_bundles[i].bundleName);

                    files.Add(new DownloadObjectInfo(fileUrl, localFile));
                }

                ExecutionType = ExecutionType.Download;

                DownloadFilesRequest requestFiles = DownloadFilesRequest.DownloadFiles(files);

                List<FileMD5Request> all_verify_res_request = new List<FileMD5Request>(); // 保存所有的校验中的资源

                requestFiles.onFileDownloadCompleted += (info) =>
                {
                    // 校验和更新资源
                    string bundleName = System.IO.Path.GetFileName(info.localfile);
                    BundleInfo bundleInfo = BundleInfo.Empty;
                    foreach (var item in result.need_update_bundles)
                    {
                        if (item.bundleName.Equals(bundleName))
                        {
                            bundleInfo = item;
                            break;
                        }
                    }

                    if (!bundleInfo.Equals(BundleInfo.Empty))
                    {
                        // 计算本地的md5
                        FileMD5Request request = XFABTools.CaculateFileMD5(info.localfile);
                        request.AddCompleteEvent((response) =>
                        {

                            if (response.md5.Equals(bundleInfo.md5))
                            {
                                // 文件校验通过 更新配置
                                LocalAssetBundleInfoManager.Instance.SetAssetBundleMd5(result.projectName, bundleInfo.bundleName, bundleInfo.md5);
                            }
                            else
                            {
                                // 文件校验失败 移除配置
                                LocalAssetBundleInfoManager.Instance.DeleteAssetBundleMd5(result.projectName, bundleInfo.bundleName);
                                // 删除文件
                                if (File.Exists(info.localfile)) File.Delete(info.localfile);
                            }
                        }); 
                        all_verify_res_request.Add(request);
                    }

                };

                while (!requestFiles.isDone)
                {
                    yield return null;
                    progress = requestFiles.progress;
                    CurrentSpeed = requestFiles.Speed;
                    DownloadedSize = requestFiles.DownloadedSize;
                }

                if (!string.IsNullOrEmpty(requestFiles.error))
                {
                    Completed(requestFiles.error);
                    yield break;
                }

                // 等待所有的校验完成
                foreach (var item in all_verify_res_request)
                {
                    while (!item.isDone)
                        yield return null;
                }

                ExecutionType = ExecutionType.Verify;
                // 验证下载的资源是否正确 
                CheckResUpdateRequest requestUpdate = AssetBundleManager.CheckResUpdate(result.projectName);
                yield return requestUpdate;

                // 验证出错
                if (!string.IsNullOrEmpty(requestUpdate.error))
                {
                    Completed(requestUpdate.error);
                    yield break;
                }

                // 在更新或下载时会过滤掉已经内置的资源，所以这里检测需不需要释放
                if (requestUpdate.result.updateType == UpdateType.ExtractLocal)
                {
                    // 释放资源
                    ExecutionType = ExecutionType.ExtractLocal;
                    ExtractResRequest request_extra = AssetBundleManager.ExtractRes(requestUpdate.result);
                    while (!request_extra.isDone)
                    {
                        yield return null;
                        progress = request_extra.progress;
                    }

                    // 出错了
                    if (!string.IsNullOrEmpty(request_extra.error))
                    {
                        Completed(request_extra.error);
                        yield break;
                    }

                    // 再次校验文件
                    ExecutionType = ExecutionType.Verify;
                    // 验证下载的资源是否正确 
                    requestUpdate = AssetBundleManager.CheckResUpdate(result.projectName);
                    yield return requestUpdate;

                    if (string.IsNullOrEmpty(requestUpdate.error) && requestUpdate.result.updateType != UpdateType.DontNeedUpdate)
                    {
                        // 说明资源不匹配 需要重新下载
                        Completed("资源校验失败,请稍后重试!");
                        yield break;
                    }
                }
                else 
                {
                    if (string.IsNullOrEmpty(requestUpdate.error) && requestUpdate.result.updateType != UpdateType.DontNeedUpdate)
                    {
                        // 说明资源不匹配 需要重新下载
                        Completed("资源校验失败,请稍后重试!");
                        yield break;
                    }
                }
                
            }

            // 保存 Version
            XFABTools.SaveVersion(result.projectName, result.version);

            // 同步md5
            if (result.check_build_info != null)
                LocalAssetBundleInfoManager.Instance.SyncBundleList(result.projectName, result.check_build_info.bundleInfos);

            // 下载完成
            Completed();

        }

    }

}

