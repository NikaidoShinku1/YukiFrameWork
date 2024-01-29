using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace YukiFrameWork.XFABManager
{
    public class GetFileFromServerRequest : CustomAsyncOperation<GetFileFromServerRequest>
    {

        private string url;
        
        //private string _text;
        public string text
        {
            get; protected set;
        }

        //private string _request_url;

        public string request_url {
            get; protected set;
        }

        internal GetFileFromServerRequest(string url) {
            this.url = url;
            if (string.IsNullOrEmpty(url))
            {
                throw new Exception("url is null ! 请配置后重试!");
            }
        }

        internal IEnumerator GetFileFromServer(string projectName, string version, string fileName)
        {
            request_url = XFABTools.ServerPath(url, projectName, version, fileName);
            // 获取内容
            UnityWebRequest request = UnityWebRequest.Get(request_url);
            request.timeout = 5; // 5s超时
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
            if (request.result != UnityWebRequest.Result.Success)
#else 
            if (request.isNetworkError || request.isHttpError)
#endif
            {
                error = request.error;
                if ( request.responseCode == 404 ) 
                {
                    Debug.LogErrorFormat("网络路径:{0} 不存在!请检查 url 填写是否正确!",request_url);
                }
            }
            else {
                text = request.downloadHandler.text;
            }
            isCompleted = true;
        }
    }
}


