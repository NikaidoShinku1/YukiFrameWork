
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace XFABManager
{

    public struct DownloadObjectInfo
    {

        public string url;
        public string localfile;
        //internal long length;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="url">需要下载的文件的网络路径</param>
        /// <param name="localfile">保存在本地的路径</param>
        /// <param name="length">需要下载的文件的大小(如果传的值不正确,获取到的下载进度也会不正确!)</param>
        public DownloadObjectInfo(string url, string localfile)
        {
            this.url = url;
            this.localfile = localfile;
            //this.length = 0;
        }

    }

    /// <summary>
    /// 下载多个文件的请求 
    /// 用来替换 DownloadManager
    /// </summary>
    public class DownloadFilesRequest : CustomAsyncOperation<DownloadFilesRequest>
    {

        #region 字段

         
        /// <summary>
        /// 等待下载的文件集合
        /// </summary>
        private Queue<DownloadObjectInfo> waitDownloadObjects = null;

        /// <summary>
        /// 已经下载完成的文件
        /// </summary>
        private List<DownloadObjectInfo> downloadedObjects = null;

        /// <summary>
        /// 需要下载的所有文件的集合
        /// </summary>
        private List<DownloadObjectInfo> downloadObjects = null;


        /// <summary>
        /// 需要下载的文件的大小
        /// </summary>
        private Dictionary<string, long> download_file_length = new Dictionary<string, long>();

        /// <summary>
        /// 某一秒的所有下载文件的请求
        /// </summary>
        private Dictionary<long, List<DownloadFileRequest>> number_of_downloaded_file_request_per_second = new Dictionary<long, List<DownloadFileRequest>>();

        private List<long> temp_list = new List<long>();



        private long all_download_size;
        /// <summary>
        /// 当某个文件下载完成时触发
        /// </summary>
        public System.Action<DownloadObjectInfo> onFileDownloadCompleted;

        internal Coroutine runing_coroutine;

        private ExecuteMultipleAsyncOperation<DownloadFileRequest> multiple_download_operation;

        private List<DownloadFileRequest> downloadFiles = new List<DownloadFileRequest>();

        #endregion


        #region 属性

        /// <summary>
        /// 下载速度，单位:字节/秒
        /// </summary>
        public long Speed { 
            get {
                long speed = 0;

                long cur_time = DateTimeTools.DateTimeToTimestamp(DateTime.Now, false);

                if (number_of_downloaded_file_request_per_second.ContainsKey(cur_time - 1))
                {
                    
                    foreach (var item in number_of_downloaded_file_request_per_second[cur_time - 1]) 
                    {
                        //Debug.LogWarningFormat("上一秒下载的文件:{0} 速度:{1}",item.Url,item.Speed);    
                        speed += item.Speed; 
                    }
                    //Debug.LogFormat("上一秒的下载的数量:{0} cur_time:{1} speed:{2}", number_of_downloaded_file_request_per_second[cur_time - 1].Count, cur_time,speed);
                    return speed;
                }
                if (number_of_downloaded_file_request_per_second.ContainsKey(cur_time))
                {
                    foreach (var item in number_of_downloaded_file_request_per_second[cur_time])
                        speed += item.Speed;
                }

                return speed;
            } 
        }


        /// <summary>
        /// 已经下载的文件大小，单位:字节
        /// </summary>
        public long DownloadedSize
        {
            get
            {
                all_download_size = 0;
                foreach (var item in downloadedObjects)
                {
                    if (download_file_length.ContainsKey(item.url))
                        all_download_size += download_file_length[item.url];
                }

                long size = all_download_size + DownloadingSize;
                return size;
            }

        }

        private long DownloadingSize
        {
            get {
                long downloading_size = 0;

                if (multiple_download_operation != null) 
                {
                    foreach (var item in multiple_download_operation.InExecutionAsyncOperations)
                    {
                        downloading_size += item.DownloadedSize;
                    }
                }
                 
                return downloading_size;
            }
        }

        /// <summary>
        /// 所有需要下载的文件总大小，单位:字节 
        /// </summary>
        public long AllSize { get; private set; }

        #endregion


        #region 方法
         
        private DownloadFilesRequest(List<DownloadObjectInfo> downloadObjects)
        {
            this.downloadObjects = downloadObjects;
            this.waitDownloadObjects = new Queue<DownloadObjectInfo>(downloadObjects.Count);
            this.downloadedObjects = new List<DownloadObjectInfo>(downloadObjects.Count);

            //ResetWaitDownloadObjects();
        }
         
        /// <summary>
        /// 重置等待下载的文件集合
        /// </summary>
        private void ResetWaitDownloadObjects()
        {
            waitDownloadObjects.Clear();
            foreach (var item in this.downloadObjects)
            {
                this.waitDownloadObjects.Enqueue(item);
            }
        }

        internal IEnumerator Download()
        {
            ResetWaitDownloadObjects();
            if (waitDownloadObjects == null || waitDownloadObjects.Count == 0)
            {
                Debug.LogWarning("需要下载的文件为空,请添加后重试!");
                Completed();
                yield break;
            }

            float time = Time.time;

            // 清空文件记录
            download_file_length.Clear();
            ExecuteMultipleAsyncOperation<RemoteFileInfoRequest> multiple_remote_operation = new ExecuteMultipleAsyncOperation<RemoteFileInfoRequest>(30);

            int allCount = waitDownloadObjects.Count;


            // 请求所有文件的大小 
            while (waitDownloadObjects.Count > 0 || !multiple_remote_operation.IsDone())
            {
                // 可以下载
                while (multiple_remote_operation.CanAdd() && waitDownloadObjects.Count > 0)
                {
                    DownloadObjectInfo info = waitDownloadObjects.Dequeue();
                    if (string.IsNullOrEmpty(info.url) || string.IsNullOrEmpty(info.localfile)) continue;
                    RemoteFileInfoRequest request_remote = RemoteFileInfoRequest.RequestInfo(info.url);

                    request_remote.AddCompleteEvent((request) =>
                    {
                        if (string.IsNullOrEmpty(request.error))
                        {
                            // 记录size 
                            if (download_file_length.ContainsKey(info.url))
                                download_file_length[info.url] = request_remote.Length;
                            else
                                download_file_length.Add(info.url, request_remote.Length);
                        }
                    });
                     
                    multiple_remote_operation.Add(request_remote);
                }

                progress = (1 - waitDownloadObjects.Count * 1.0f / allCount) * 0.1f;

                if (!string.IsNullOrEmpty(multiple_remote_operation.Error())) break;
                yield return null;
            }
            // 等待正在下载的其他任务执行完成
            while (!multiple_remote_operation.IsDone())
            {
                yield return null;
            }
            if (!string.IsNullOrEmpty(multiple_remote_operation.Error()))
            {
                Completed(multiple_remote_operation.Error());
                yield break;
            }

            AllSize = 0;
            foreach (var item in downloadObjects)
            {

                if (download_file_length.ContainsKey(item.url))
                {
                    AllSize += download_file_length[item.url];
                }
            }

            // 开始下载文件
            ResetWaitDownloadObjects();
            downloadedObjects.Clear();

            multiple_download_operation = new ExecuteMultipleAsyncOperation<DownloadFileRequest>(10);

            while (waitDownloadObjects.Count > 0 || !multiple_download_operation.IsDone())
            {
                // 可以下载
                while (multiple_download_operation.CanAdd() && waitDownloadObjects.Count > 0)
                {
                    DownloadObjectInfo info = waitDownloadObjects.Dequeue();
                    if (string.IsNullOrEmpty(info.url) || string.IsNullOrEmpty(info.localfile)) continue;
                    DownloadFileRequest download = DownloadFileRequest.Download(info.url, info.localfile);

                    download.AddCompleteEvent((request) =>
                    {
                        if (string.IsNullOrEmpty(download.error))
                        {  
                            // 文件下载完成
                            downloadedObjects.Add(info);
                            onFileDownloadCompleted?.Invoke(info);
                        }
                        downloadFiles.Remove(download);
                    });
                     
                    // 保存正在下载的请求 (如果需要中断，需要用到)
                    downloadFiles.Add(download);
                    multiple_download_operation.Add(download);
                }
                 

                foreach (var download_file in multiple_download_operation.InExecutionAsyncOperations)
                { 

                    long cur_time = DateTimeTools.DateTimeToTimestamp(DateTime.Now, false);
                    if (number_of_downloaded_file_request_per_second.ContainsKey(cur_time))
                    {
                        if (!number_of_downloaded_file_request_per_second[cur_time].Contains(download_file))
                            number_of_downloaded_file_request_per_second[cur_time].Add(download_file);
                    }
                    else
                    {
                        List<DownloadFileRequest> list = new List<DownloadFileRequest>();
                        list.Add(download_file);
                        number_of_downloaded_file_request_per_second.Add(cur_time, list);
                    }

                    // 清理数据
                    temp_list.Clear();

                    foreach (var key in number_of_downloaded_file_request_per_second.Keys)
                        if (cur_time - key > 2) temp_list.Add(key);

                    foreach (var key in temp_list)
                        number_of_downloaded_file_request_per_second.Remove(key);

                    temp_list.Clear();
                }

                if (AllSize != 0) 
                {                    
                    // 先转成float(不会溢出)再相除
                    float p = DownloadedSize * 1.0f / AllSize; // 如果AllSize等于0 会抛异常
                    // 获取所有文件的大小的步骤 占比 0.1
                    p = 0.1f + p * 0.9f;
                    // 防止进度后退(如果下载一个文件，下载了一部分，但是后面失败了，这个时候已下载的字节就会减少，所以进度也会变小)
                    if (progress < p) progress = p;
                }else 
                    progress = 0.0f;
                 
                yield return null;

                if (!string.IsNullOrEmpty(multiple_download_operation.Error())) break; // 有文件下载失败
            }

            // 等待正在下载的其他任务执行完成
            while (!multiple_download_operation.IsDone())
            {
                yield return null;
            }

            // 检测文件是否下载出错
            if (!string.IsNullOrEmpty(multiple_download_operation.Error()))
            {
                Completed(multiple_download_operation.Error());
                yield break;
            }

            Completed();
        }

        [System.Obsolete("该方法已经过时,请使用DownloadFilesRequest.DownloadFiles(List<DownloadObjectInfo> files) 代替!", true)]
        public static DownloadFilesRequest DownloadFiles(Dictionary<string, string> files)
        {
            //DownloadFilesRequest downloadFiles = new DownloadFilesRequest(null);
            ////downloadFiles.AddRange(files);
            //CoroutineStarter.Start(downloadFiles.Download());
            return null;
        }

        public static DownloadFilesRequest DownloadFiles(List<DownloadObjectInfo> files)
        {
            DownloadFilesRequest downloadFiles = new DownloadFilesRequest(files);
            downloadFiles.runing_coroutine = CoroutineStarter.Start(downloadFiles.Download());
            return downloadFiles;
        }
         
        /// <summary>
        /// 中断下载
        /// </summary>
        public void Abort()
        {
            try
            {
                if (runing_coroutine != null)
                    CoroutineStarter.Stop(runing_coroutine);
                foreach (var item in downloadFiles)
                {
                    item.Abort();
                }
            }
            catch (System.Exception)
            {
            }
            finally {
                Completed("request abort!");
            }
        }
         
        protected override void OnCompleted()
        {
            base.OnCompleted();
            number_of_downloaded_file_request_per_second.Clear(); // 清空数据
            downloadFiles.Clear(); 
        }

        #endregion
    }

}

