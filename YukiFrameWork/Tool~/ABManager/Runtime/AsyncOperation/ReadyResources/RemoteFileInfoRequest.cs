using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace YukiFrameWork.ABManager
{

    public enum RemoteFileExistType 
    {
        /// <summary>
        /// 存在
        /// </summary>
        Exist,
        /// <summary>
        /// 不存在
        /// </summary>
        UnExist,
        /// <summary>
        /// 未知
        /// </summary>
        Unknown
    }

    /// <summary>
    /// 获取网络文件信息(比如:文件大小，最后的修改时间，是否存在 等)
    /// </summary>
    public class RemoteFileInfoRequest : CustomAsyncOperation<RemoteFileInfoRequest>
    {

        /// <summary>
        /// 网络文件存在类型
        /// </summary>
        public RemoteFileExistType ExistType { get; private set; }

        /// <summary>
        /// 网络文件的大小（单位:字节）
        /// </summary>
        public long Length { get; private set; }

        /// <summary>
        /// 网络文件路径
        /// </summary>
        public string Url { get; private set; }

        /// <summary>
        /// 网络文件类型
        /// </summary>
        public string Type { get; private set; }

        /// <summary>
        /// 最后一次的修改时间
        /// </summary>
        public string LastModified{get;private set;} // Last-Modified

        /// <summary>
        /// 当前网络文件的所有信息
        /// </summary>
        public Dictionary<string, string> AllInfo { get;private set; }

        private RemoteFileInfoRequest(string url) {
            this.Url = url;
        }


        private IEnumerator Request() 
        {
            UnityWebRequest request = null;
            for (int i = 0; i < 3; i++)
            {
                request = UnityWebRequest.Head(Url);
                request.timeout = 5;
                request.disposeCertificateHandlerOnDispose = true;
                request.disposeDownloadHandlerOnDispose = true;

                UnityWebRequestAsyncOperation asyncOperation = null;
                try
                {
                    asyncOperation = request.SendWebRequest();
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
#if UNITY_2020_1_OR_NEWER
                if (request.result == UnityWebRequest.Result.Success)
#else
                if (string.IsNullOrEmpty(request.error))
#endif
                {
                    break;
                }
                else 
                {
                    yield return new WaitForSeconds(0.1f); // 如果出错 1s 后重试
                }
            }

#if UNITY_2020_1_OR_NEWER
            if (request.result == UnityWebRequest.Result.Success)
#else
            if (string.IsNullOrEmpty(request.error))
#endif
            {
                ExistType = RemoteFileExistType.Exist ;
                AllInfo = request.GetResponseHeaders();
                Type = request.GetResponseHeader("Content-Type");
                LastModified = request.GetResponseHeader("Last-Modified");

                try
                {
                    Length = long.Parse(request.GetResponseHeader("Content-Length"));
                }
                catch (System.Exception)
                {
                }
                
                Completed();
            }
            else 
            {
                ExistType = request.responseCode == 404 ? RemoteFileExistType.UnExist : RemoteFileExistType.Unknown ;

                AllInfo = new Dictionary<string, string>(); // 空的字典
                Completed(string.Format("url:{0} error:{1}",Url ,request.error));
            }
        }

        /// <summary>
        /// 请求网络文件信息
        /// </summary>
        /// <param name="file_url">网络文件路径</param>
        public static RemoteFileInfoRequest RequestInfo(string file_url) 
        { 
            string key = string.Format("RemoteFileInfoRequest:{0}", file_url);
            return AssetBundleManager.ExecuteOnlyOnceAtATime<RemoteFileInfoRequest>(key, () => {
                RemoteFileInfoRequest request = new RemoteFileInfoRequest(file_url);
                CoroutineStarter.Start(request.Request());
                return request;
            });
        }
    }
}
