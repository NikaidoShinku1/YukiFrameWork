using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic; 
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace XFABManager
{

    /// <summary>
    /// 检测资源 不包含依赖项目
    /// </summary>
    public class CheckResUpdateRequest : CustomAsyncOperation<CheckResUpdateRequest>
    {
        //private CheckUpdateResult _result;
        public CheckUpdateResult result { get; private set; }

#if XFABMANAGER_LOG_OPEN_TESTING
        System.Diagnostics.Stopwatch stopwatch = null;
#endif

        internal IEnumerator CheckResUpdate(string projectName)
        {
             
#if XFABMANAGER_LOG_OPEN_TESTING
            Debug.LogFormat("CheckResUpdate:{0}",projectName);
            stopwatch = System.Diagnostics.Stopwatch.StartNew();
            stopwatch.Restart();
#endif

            if (string.IsNullOrEmpty(projectName))
            { 
                Completed(string.Format("项目名不能为空!"));
                yield break;
            }

            // 构建检测结果
            result = new CheckUpdateResult(projectName);
            // 根据更新模式 检测本地资源 判断本地是否 有资源 是否有内置资源 给出更新方案  

            CheckLocalResRequest checkLocalResRequest = CheckLocalResRequest.Check(projectName, result);
            yield return checkLocalResRequest;
            if (!string.IsNullOrEmpty(checkLocalResRequest.error))
            {
                Completed(checkLocalResRequest.error);
                yield break;
            }

            // 只有当检查的结果是更新或者下载时才需要做后面的网络检测，如果不是检测到这里就结束了
            if (result.updateType != UpdateType.Update && result.updateType != UpdateType.Download)
            {
                Completed();
                yield break;
            }

            // 获取项目版本
            GetProjectVersionRequest requestVersion = null;

            // 请求项目版本，这里有三次机会，如果三次都失败了，才认为请求版本出错
            for (int i = 0; i < 3; i++)
            {
                // 获取项目版本
                requestVersion = AssetBundleManager.GetProjectVersion(projectName);
                yield return requestVersion;

                if (string.IsNullOrEmpty(requestVersion.error)) break; // 请求成功，没有出错，跳出循环

                yield return new WaitForSeconds(0.1f); // 如果请求出错，等待0.1s再请求下一次
            }

            // 请求出错
            if (!string.IsNullOrEmpty(requestVersion.error))
            {
                // 检测出错
                result.updateType = UpdateType.Error;
                Completed(requestVersion.error);
                yield break;
            }
             
            result.version = requestVersion.version;
            // 检测是否有压缩文件
            if (result.updateType == UpdateType.Download && Application.platform != RuntimePlatform.WebGLPlayer)
            {
                // 判断服务端是否有.zip文件  
                string zipPath = XFABTools.ServerPath(AssetBundleManager.GetProfile(projectName).url, projectName, requestVersion.version, string.Format("{0}.zip", projectName));
                RemoteFileInfoRequest zip_file_info = RemoteFileInfoRequest.RequestInfo(zipPath);
                yield return zip_file_info;
                 
#if XFABMANAGER_LOG_OPEN_TESTING
                Debug.LogFormat("CheckResUpdate CheckServerZip :{0}", zip_file_info.ExistType);
#endif

                if (string.IsNullOrEmpty(zip_file_info.error) && zip_file_info.ExistType == RemoteFileExistType.Exist)
                {
                    result.updateType = UpdateType.DownloadZip;
                    result.updateSize = zip_file_info.Length;
                    Completed();
                    yield break;
                }
            }
            // 获取服务器的文件列表信息
            ProjectBuildInfo projectBuildInfo = null;

            GetFileFromServerRequest requestFile = AssetBundleManager.GetFileFromServer(projectName, requestVersion.version, XFABConst.project_build_info);
            yield return requestFile;
             
#if XFABMANAGER_LOG_OPEN_TESTING
            Debug.LogFormat("CheckResUpdate ServerProjectBuildInfo :{0}", requestFile.text);
#endif

            if (!string.IsNullOrEmpty(requestFile.error))
            {
                result.updateType = UpdateType.Error;
                result.message = error;
                Completed(string.Format("获取项目{0}文件列表出错:{1}", projectName, requestFile.error));
                yield break;
            }
            else
            {
                try
                {
                    projectBuildInfo = JsonConvert.DeserializeObject<ProjectBuildInfo>(requestFile.text);
                }
                catch (Exception e)
                {
                    Completed(string.Format("json 解析失败:{0} error:{1}", requestFile.text, e.ToString()));
                    yield break;
                }
            }

            result.check_build_info = projectBuildInfo;

            // 保存后缀信息
            LocalAssetBundleInfoManager.Instance.SaveSuffix(result.projectName, projectBuildInfo.suffix);

            // 如果版本号 不相同 获取到服务端文件列表 与本地进行比较 找出需要更新的文件 
            yield return CompareLocalResRequest.Compare(projectName, projectBuildInfo.bundleInfos, result); // 这个不存在出错 除非抛异常

            // 获取更新内容
            result.message = projectBuildInfo.update_message;
            result.updateDate = projectBuildInfo.update_date;

            Completed();
        }

         

        protected override void OnCompleted()
        {
            base.OnCompleted();
            if (result.updateType != UpdateType.DownloadZip && result.updateType != UpdateType.Error && result.updateType != UpdateType.Download)
            {
                // 在解压中的时候杀掉进程可能会导致压缩文件没有删除，
                // 所以在这里检测以下，如果有压缩文件就把它删掉，在特定的情况下
                string localfile = XFABTools.LocalResPath(result.projectName, string.Format("{0}{1}", result.projectName, ".zip"));
                if (File.Exists(localfile)) File.Delete(localfile);
            }
             
#if XFABMANAGER_LOG_OPEN_TESTING
            stopwatch.Stop();
            Debug.LogFormat("CheckResUpdate 检测结果:{0} 检测耗时:{1}/秒", result,stopwatch.ElapsedMilliseconds/1000.0f);
#endif

        }

    }


    /// <summary>
    /// 更新类型 
    /// </summary>
    public enum UpdateType
    {
        /// <summary>
        /// 更新资源
        /// </summary>
        Update,
        /// <summary>
        /// 下载资源
        /// </summary>
        Download,

        /// <summary>
        /// 下载压缩文件
        /// </summary>
        DownloadZip,

        /// <summary>
        /// 释放内置资源
        /// </summary>
        ExtractLocal,

        /// <summary>
        /// 释放内置压缩资源
        /// </summary>
        [Obsolete("删除释放压缩资源选项!", true)]
        ExtractLocalZip,

        /// <summary>
        /// 不需要更新
        /// </summary>
        DontNeedUpdate,
        /// <summary>
        /// 更新出错
        /// </summary>
        Error

    }





}

