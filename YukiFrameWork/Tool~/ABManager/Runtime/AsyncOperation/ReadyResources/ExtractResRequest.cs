using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace YukiFrameWork.XFABManager
{
    public class ExtractResRequest : CustomAsyncOperation<ExtractResRequest>
    {
          
        private Queue<BundleInfo> all_need_extract = new Queue<BundleInfo>();  // 所有需要释放的文件  
        private List<BundleInfo> extractedFiles = new List<BundleInfo>(); // 已经释放完成的文件
        
        private int all_count;

        internal IEnumerator ExtractRes(string projectName) {

            CheckResUpdateRequest checkUpdate = AssetBundleManager.CheckResUpdate(projectName);
            yield return checkUpdate;

            if ( !string.IsNullOrEmpty(checkUpdate.error) ) {
                Completed(checkUpdate.error);
                yield break;
            }

            yield return CoroutineStarter.Start(ExtractRes(checkUpdate.result));

        }


        internal IEnumerator ExtractRes(CheckUpdateResult result)
        {

            if ( result.updateType == UpdateType.DontNeedUpdate)
            {
                Completed();
                yield break;
            }

            // 判断是否有内置资源
            IsHaveBuiltInResRequest isHaveBuiltInRes = AssetBundleManager.IsHaveBuiltInRes(result.projectName);
            yield return isHaveBuiltInRes;
            if (!isHaveBuiltInRes.isHave)
            {
                Completed(string.Format("释放资源失败!未查询到内置资源{0}!", result.projectName));
                yield break;
            }

            // 释放压缩资源
            string buildInRes = null;
            string localRes = null;


            switch (result.updateType)
            {
                case UpdateType.ExtractLocal:
                    string project_build_info = XFABTools.BuildInDataPath(result.projectName, XFABConst.project_build_info);
                    string content = string.Empty;

#if UNITY_ANDROID && !UNITY_EDITOR
                    UnityWebRequest requestBuildInfo = UnityWebRequest.Get(project_build_info);
                    yield return requestBuildInfo.SendWebRequest();
                    if (string.IsNullOrEmpty(requestBuildInfo.error))
                        content = requestBuildInfo.downloadHandler.text;
                    else 
                    {
                        Completed(string.Format("读取文件失败:{0} error:{1}", project_build_info, requestBuildInfo.error));
                        yield break;
                    }
#else
                    try
                    {
                        content = File.ReadAllText(project_build_info);
                    }
                    catch (Exception e)
                    {
                        Completed(string.Format("文件读取失败:{0} error:{1}", project_build_info, e.ToString()));
                        yield break;
                    }
#endif
                    ProjectBuildInfo buildInfo = null;
                    try
                    {
                        buildInfo = JsonConvert.DeserializeObject<ProjectBuildInfo>(content);
                    }
                    catch (Exception e)
                    {
                        Completed(string.Format("json 解析失败:{0} error:{1}", content, e.ToString()));
                        yield break;
                    }

                    // 保存后缀
                    LocalAssetBundleInfoManager.Instance.SaveSuffix(result.projectName, buildInfo.suffix);

                    all_need_extract.Clear();

                    foreach (var item in result.need_update_bundles)
                    {
                        all_need_extract.Enqueue(item);
                    }

                    all_count = all_need_extract.Count;

                    ExecuteMultipleAsyncOperation<CopyFileRequest> copy_file_operation = new ExecuteMultipleAsyncOperation<CopyFileRequest>(30);

                    while (all_need_extract.Count > 0)
                    {
                        yield return null;

                        while (all_need_extract.Count > 0 && copy_file_operation.CanAdd()) 
                        {
                            BundleInfo bundleInfo = all_need_extract.Dequeue();

                            buildInRes = XFABTools.BuildInDataPath(result.projectName, bundleInfo.bundleName);
                            localRes = XFABTools.LocalResPath(result.projectName, bundleInfo.bundleName);

                            // 复制之前先把旧的文件删掉
                            if (File.Exists(localRes)) File.Delete(localRes);
                            // 在开始准备复制文件之前 先清空旧的md5的信息
                            LocalAssetBundleInfoManager.Instance.DeleteAssetBundleMd5(result.projectName, bundleInfo.bundleName);

                            // 如果StreamingAssets目录能读，则不需要复制文件，直接更新配置即可
                            if (XFABTools.StreamingAssetsReadable()) 
                            {  
                                LocalAssetBundleInfoManager.Instance.SetAssetBundleMd5(result.projectName, bundleInfo.bundleName, bundleInfo.md5);
                                continue;
                            }
                             
                            CopyFileRequest copy_buildin_request = FileTools.Copy(buildInRes, localRes);

                            copy_buildin_request.AddCompleteEvent((request) =>
                            {
                                if (string.IsNullOrEmpty(request.error))
                                {
                                    // 文件释放完成
                                    extractedFiles.Add(bundleInfo);
                                    // 更新本地配置
                                    LocalAssetBundleInfoManager.Instance.SetAssetBundleMd5(result.projectName, bundleInfo.bundleName, bundleInfo.md5);
                                }
                            });
                             
                            copy_file_operation.Add(copy_buildin_request);
                        }
                        progress = (float)(extractedFiles.Count ) / all_count;

                        if (!string.IsNullOrEmpty(copy_file_operation.Error())) // 如果正在下载中的文件全都下载出错了 跳出循环
                            break;
                    }

                    while (!copy_file_operation.IsDone())
                        yield return null;

                    // 检测文件是否下载出错
                    if (!string.IsNullOrEmpty(copy_file_operation.Error()))
                    {
                        Completed(copy_file_operation.Error());
                        yield break;
                    }

                    break;
                default: 
                    Completed(string.Format("释放资源出错,未知类型{0}", result.updateType));
                    yield break;
            }
            Completed();
        }

    }

}

