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
                yield return new WaitForEndOfFrame();
                Completed(string.Format("项目名不能为空!"));
                yield break;
            }

            // 构建检测结果
            result = new CheckUpdateResult(projectName);
            // 根据更新模式 检测本地资源 判断本地是否 有资源 是否有内置资源 给出更新方案
            yield return CoroutineStarter.Start(CheckLocalRes(projectName));

            // 只有当检查的结果是更新或者下载时才需要做后面的网络检测，如果不是监测到这里就结束了
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
            yield return CoroutineStarter.Start(CompareWithLocal(projectName, projectBuildInfo.bundleInfos));
            // 获取更新内容
            result.message = projectBuildInfo.update_message;
            result.updateDate = projectBuildInfo.update_date;

            Completed();
        }

        /// <summary>
        /// 与本地的文件比较 判断是否需要更新
        /// </summary>
        /// <param name="projectName">资源模块名</param>
        /// <param name="serverBundleInfos">服务端bundle列表信息</param>
        private IEnumerator CompareWithLocal(string projectName, BundleInfo[] serverBundleInfos)
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
                        isNeedUpdate = true;

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
            Debug.LogFormat("对比本地文件Md5耗时:{0}/秒",stopwatch.ElapsedMilliseconds/1000.0f);
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
                    if (!string.IsNullOrEmpty(request_build_in_info.error)) {
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

                        if (need_update_files.Count == 0 || result.updateSize == 0) {
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
        }


        /// <summary>
        /// 检测本地资源
        /// </summary>
        private IEnumerator CheckLocalRes(string projectName)
        {

            // 如果是编辑器  并且 从 Assets 加载资源 
            // 这种情况下 是 不需要AssetBundle的 
#if UNITY_EDITOR
            if (AssetBundleManager.GetProfile(projectName).loadMode == LoadMode.Assets)
            {
                result.updateType = UpdateType.DontNeedUpdate;
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
                Debug.LogFormat("非更新模式释放本地文件md5对比耗时:{0}/秒", stopwatch.ElapsedMilliseconds/1000.0f);
#endif

            }

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

