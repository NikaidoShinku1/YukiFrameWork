using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using YukiFrameWork.ABManager;

public class GetProjectVersionDefault : MonoBehaviour ,IGetProjectVersion{

    //public string url;

    private string error;
    private bool is_done;
    private string version;
    public string Error()
    {
        return error;
    }

    public bool isDone()
    {
        return is_done;
    }

    public string Result()
    {
        return version;
    }

    void IGetProjectVersion.GetProjectVersion(string projectName)
    {
        StartCoroutine(GetVersion(projectName));
    }


    private IEnumerator GetVersion(string projectName) {

        is_done = false;
        error = string.Empty;
        version = string.Empty;
        yield return null;

        string url = AssetBundleManager.GetProfile(projectName).url;

        string version_url = string.Format("{0}/versions/{1}/version.txt", url, projectName);


        UnityWebRequest www = UnityWebRequest.Get(version_url);
        www.timeout = 5;
        UnityWebRequestAsyncOperation operation = null;
        try
        {
            operation = www.SendWebRequest();
        }
        catch (System.InvalidOperationException e)
        {
            is_done = true;
            error = string.Format("{0},Non-secure network connections disabled in Player Settings!", e.Message);
            yield break;
        }
        catch (System.Exception e) {
            is_done = true;
            error = e.Message;
            yield break;
        }
         
        yield return operation;

        is_done = true;

#if UNITY_2020_1_OR_NEWER
            if (www.result != UnityWebRequest.Result.Success)
#else 
            if (www.isNetworkError || www.isHttpError)
#endif
        {
            error = www.error;

            if (www.responseCode == 404) {
                Debug.LogErrorFormat("网络路径:{0} 不存在! 请检查地址填写是否正确!", version_url);
            }

            yield break;
        }
        version = www.downloadHandler.text.Trim();
    }

}
