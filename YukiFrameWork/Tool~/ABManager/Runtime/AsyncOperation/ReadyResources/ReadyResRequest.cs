using System.Collections;
using System.Collections.Generic; 
using UnityEngine;
namespace YukiFrameWork.XFABManager
{
    /// <summary>
    /// 当前正在处理的操作的类型
    /// </summary>
    public enum ExecutionType 
    { 
        None,

        /// <summary>
        /// 下载文件
        /// </summary>
        Download,
        /// <summary>
        /// 解压文件
        /// </summary>
        Decompression, // 
        /// <summary>
        /// 校验文件
        /// </summary>
        Verify,
        /// <summary>
        /// 释放资源
        /// </summary>
        ExtractLocal
    }

    public class ReadyResRequest : CustomAsyncOperation<ReadyResRequest>
    {

        /// <summary>
        /// 当前正在执行什么操作 比如:下载文件,解压文件,校验文件等
        /// </summary>
        public ExecutionType ExecutionType { get; private set; } = ExecutionType.None;

        /// <summary>
        /// 当前正在 准备的模块的名称
        /// </summary>
        public string CurrentProjectName { get; private set; }

        /// <summary>
        /// 下载速度 单位:字节
        /// </summary>
        public long CurrentSpeed { get; protected set; }
         
        /// <summary>
        /// 已下载的文件大小 单位:字节
        /// </summary>
        public long DownloadedSize { get; private set; }

        /// <summary>
        /// 需要下载的大小 单位:字节
        /// </summary>
        public long NeedDownloadedSize { get; private set; }

        internal IEnumerator ReadyRes(string projectName)
        {

#if XFABMANAGER_LOG_OPEN_TESTING
            Debug.LogFormat("ReadyRes:{0}",projectName);
#endif

            if (string.IsNullOrEmpty(projectName)) {
                yield return new WaitForEndOfFrame();
                Completed(string.Format("项目名不能为空!"));
                yield break;
            } 
            // 检测更新
            CheckResUpdatesRequest checkReq = AssetBundleManager.CheckResUpdates(projectName);
            yield return checkReq;
            if (!string.IsNullOrEmpty(checkReq.error))
            {
                error = string.Format("准备资源失败,检测更新出错:{0}", checkReq.error);
                Completed();
                yield break;
            }

            yield return CoroutineStarter.Start(ReadyRes(checkReq.results));
            //Completed();
        }


        internal IEnumerator ReadyRes(CheckUpdateResult[] results)
        {



            // 具体操作
            for (int i = 0; i < results.Length; i++)
            {
#if XFABMANAGER_LOG_OPEN_TESTING
                Debug.LogFormat("start ready res:{0}", results[i]);
#endif

                //updateType = results[i].updateType;
                CurrentProjectName = results[i].projectName;

                switch (results[i].updateType)
                {
                    case UpdateType.Update:
                    case UpdateType.Download:
                    case UpdateType.DownloadZip:

                        // 更新或者下载资源
                        UpdateOrDownloadResRequest downloadReq = AssetBundleManager.UpdateOrDownloadRes(results[i]);

                        while (!downloadReq.isDone)
                        {
                            yield return null;
                            progress = downloadReq.progress / results.Length * (i + 1);
                            CurrentSpeed = downloadReq.CurrentSpeed; 
                            DownloadedSize = downloadReq.DownloadedSize;
                            NeedDownloadedSize = downloadReq.NeedDownloadedSize;
                            ExecutionType = downloadReq.ExecutionType;
                        }

                        if (!string.IsNullOrEmpty(downloadReq.error))
                        {
                            error = string.Format("准备资源失败,下载出错:{0}", downloadReq.error);
                            Completed();
                            yield break;
                        }

                        break;
                    case UpdateType.ExtractLocal:

                        // 释放资源
                        ExtractResRequest extractReq = AssetBundleManager.ExtractRes(results[i]);
                        ExecutionType = ExecutionType.ExtractLocal;
                        while (!extractReq.isDone)
                        {
                            yield return null;
                            progress = extractReq.progress / results.Length * (i + 1);
                        }

                        if (!string.IsNullOrEmpty(extractReq.error))
                        {
                            error = string.Format("准备资源失败,释放资源出错:{0}", extractReq.error);
                            Completed();
                            yield break;
                        }

                        // 再次调用 准备资源方法 检测是否需要更新资源
                        ReadyResRequest readyUpdate = AssetBundleManager.ReadyRes(results[i].projectName);
                        // 释放完成之后 还需要再检测是否需要更新 
                        while (!readyUpdate.isDone)
                        {
                            yield return null;
                            progress = readyUpdate.progress / results.Length * (i + 1);
                            ExecutionType  = readyUpdate.ExecutionType;
                            CurrentProjectName = readyUpdate.CurrentProjectName;
                            CurrentSpeed = readyUpdate.CurrentSpeed;
                            DownloadedSize = readyUpdate.DownloadedSize;
                            NeedDownloadedSize = readyUpdate.NeedDownloadedSize; 
                        }

                        if ( !string.IsNullOrEmpty(readyUpdate.error) ) {
                            Completed(readyUpdate.error);
                        }
                        break;
                    case UpdateType.Error:
                        // 出错
                        error = string.Format("准备资源失败,{0}", results[i].message);
                        Completed();
                        yield break;
                }
                
            }
            Completed();
        }

    }

}

