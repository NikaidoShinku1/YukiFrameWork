using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using UnityEngine.Networking;

namespace XFABManager
{

    /// <summary>
    /// 下载文件的请求 ExecuteMultipleAsyncOperation CustomYieldInstruction
    /// </summary>
    public class DownloadFileRequest : CustomAsyncOperation<DownloadFileRequest>
    {

        private const long GB = 1073741824;

        private const string tempSuffix = ".tempFile";

        private const int DOWNLOAD_FILE_TIME_OUT = 10; // 下载文件的超时时间（当下载速度为0超过20s，认为这个文件下载超时）

        private string file_url;
        private string localfile;
        private string tempfile;                // 下载的临时文件路径

        private UnityWebRequest downloadFile;

        internal Coroutine runing_coroutine;

        /// <summary>
        /// 下载速度，单位:字节/秒
        /// </summary>
        public long Speed
        {
            get
            {
                if (downloadFile != null && downloadFile.downloadHandler != null)
                {
                    DownloadHandlerFileRange range = downloadFile.downloadHandler as DownloadHandlerFileRange;
                    if (range != null) return range.Speed;
                }
                return 0;
            }
        }

        /// <summary>
        /// 已经下载的文件大小，单位:字节
        /// </summary>
        public long DownloadedSize { get; private set; }

        /// <summary>
        /// 当前下载的文件总大小，单位:字节
        /// </summary>
        public long AllSize { get; private set; }

        /// <summary>
        /// 当前正在下载的文件地址
        /// </summary>
        public string Url => file_url;

        private DownloadFileRequest(string file_url, string localfile)
        {
            this.file_url = file_url;
            this.localfile = localfile;
            tempfile = string.Format("{0}{1}", localfile, tempSuffix);

        }


#if UNITY_EDITOR
        private void PlayModeStateChanged(PlayModeStateChange obj)
        {
            if (obj == PlayModeStateChange.ExitingPlayMode)
                Abort();
        }
#endif

        private IEnumerator Download()
        {

#if UNITY_EDITOR
            EditorApplication.playModeStateChanged -= PlayModeStateChanged;
            EditorApplication.playModeStateChanged += PlayModeStateChanged;
#endif


            string downloadError = string.Empty;

            // 请求文件大小
            long length = 0;
            RemoteFileInfoRequest file_info = RemoteFileInfoRequest.RequestInfo(file_url);
            yield return file_info;
            if (string.IsNullOrEmpty(file_info.error))
                length = file_info.Length;

            for (int i = 0; i < 3; i++)
            {
                downloadFile = UnityWebRequest.Get(this.file_url);
                downloadFile.disposeDownloadHandlerOnDispose = true;
                downloadFile.disposeCertificateHandlerOnDispose = true;
                DownloadHandlerFileRange downloadHandler = null;

                if (Application.platform != RuntimePlatform.WebGLPlayer)
                {
                    try
                    {
                        // 如果文件大于1g ，采用10mb缓冲区 , 理想情况下网速大概能达到200 ~ 300 mb /s 足够使用了 (100g 10分钟不到就能下载完毕) 
                        if (length <= GB)
                            downloadHandler = new DownloadHandlerFileRange(tempfile, downloadFile);
                        else
                            downloadHandler = new DownloadHandlerFileRange(tempfile, downloadFile, 1024 * 1024 * 10);
                    }
                    catch (System.Exception e)
                    {
                        Completed(e.Message);
                        yield break;
                    }


                    downloadFile.downloadHandler = downloadHandler;
                }

                UnityWebRequestAsyncOperation asyncOperation = null;

                try
                {
                    asyncOperation = downloadFile.SendWebRequest();
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


                float timer = 0;

                while (!asyncOperation.isDone)
                {
                    yield return null;

                    if (progress < downloadFile.downloadProgress)
                        progress = downloadFile.downloadProgress;

                    // 计算网速
                    if (downloadHandler != null)
                    {
                        DownloadedSize = downloadHandler.DownloadedSize;
                        AllSize = downloadHandler.FileSize;
                    }
                    else
                    {
                        DownloadedSize = (long)downloadFile.downloadedBytes;
                        AllSize = length;
                    }

                    // 添加超时逻辑
                    if (Speed == 0) // 当前下载速度为0 开始计时
                    {
                        timer += Time.deltaTime;
                        if (timer >= DOWNLOAD_FILE_TIME_OUT)
                        {
                            break; // 超时了 直接完成 跳出循环
                        }
                    }
                    else
                        timer = 0;
                }

                downloadHandler?.Close();

                downloadError = string.Empty;

                if (timer >= DOWNLOAD_FILE_TIME_OUT)
                {
                    downloadError = string.Format("下载文件超时:{0}", file_url);
                }

                if (downloadFile != null)
                {
                    // 如果是416 说明本地文件已经下载完成
                    if (!string.IsNullOrEmpty(downloadFile.error) && downloadFile.responseCode != 416)
                    {
                        downloadError = string.Format("url:{0} error:{1}", file_url, downloadFile.error);
                    }
                }

                if (!string.IsNullOrEmpty(downloadError))
                {
                    // 说明出错了 可以重新尝试下载
                    ClearDownload();
                    yield return new WaitForSeconds(1); // 一秒之后重试
                }
                else
                    break; // 成功了 跳出循环 下载完成

            }

            // 如果downloadError 为空 说明下载成功了
            if (string.IsNullOrEmpty(downloadError))
            {
                if (Application.platform != RuntimePlatform.WebGLPlayer)
                {
                    try
                    {
                        // 说明已经下载完成 但是没有把临时文件转成正式文件
                        if (File.Exists(localfile))
                            File.Delete(localfile);

                        File.Move(tempfile, localfile);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
                else
                {
                    // Webgl平台不能用文件系统 只能保存字节数据 可以考虑使用 UnityWebRequestAssetBundle.GetAssetBundle 这个来优化 TODO
                    File.WriteAllBytes(localfile, downloadFile.downloadHandler.data);
                }
            }

            // 下载完成
            Completed(downloadError);
        }
        protected override void OnCompleted()
        {
            base.OnCompleted();

#if UNITY_EDITOR
            EditorApplication.playModeStateChanged -= PlayModeStateChanged;
#endif

            ClearDownload(); // 清空下载数据
        }

        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="file_url">需要下载的文件链接</param>
        /// <param name="localfile">下载之后存放在本地的路径</param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static DownloadFileRequest Download(string file_url, string localfile)
        {
            if (string.IsNullOrEmpty(file_url))
                return null;

            string key = string.Format("DownloadFileRequest:{0}", file_url);
            return AssetBundleManager.ExecuteOnlyOnceAtATime<DownloadFileRequest>(key, () => {
                DownloadFileRequest request = new DownloadFileRequest(file_url, localfile);
                request.runing_coroutine = CoroutineStarter.Start(request.Download());
                return request;
            });

        }

        /// <summary>
        /// 中断下载
        /// </summary> 
        protected override void OnAbort()
        {
            base.OnAbort();

            // 中断下载
            AbortDownload();
        }


        private void AbortDownload()
        {

#if UNITY_EDITOR
            EditorApplication.playModeStateChanged -= PlayModeStateChanged;
#endif

            if (runing_coroutine != null)
            {
                CoroutineStarter.Stop(runing_coroutine);
                runing_coroutine = null;
            }

            ClearDownload();
        }


        private void ClearDownload()
        {

            if (downloadFile == null) return;  // 如果为空直接return 

            if (downloadFile.downloadHandler != null)
            {
                DownloadHandlerFileRange range = downloadFile.downloadHandler as DownloadHandlerFileRange;
                if (range != null) range.Close();
            }

            try
            {
                // 如果正在下载 就中断掉
                if (!downloadFile.isDone)
                    downloadFile.Abort();
                downloadFile.Dispose();
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                downloadFile = null;
            }
        }

    }

}

