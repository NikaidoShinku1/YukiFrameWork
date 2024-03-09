using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using XFABManager;

public class ReadBuildInProjectBuildInfoRequest : CustomAsyncOperation<ReadBuildInProjectBuildInfoRequest>
{
    // Fix编码

    public string Content { get; private set; } = string.Empty;

    public ProjectBuildInfo ProjectBuildInfo { get; private set; } = null;

    public IEnumerator ReadBuildInProjectBuildInfo(string projectName)
    {

        string project_build_info = XFABTools.BuildInDataPath(projectName, XFABConst.project_build_info);

        Content = string.Empty;

        if (XFABTools.StreamingAssetsReadable() || Application.isEditor)
        {
            try
            {
                Content = File.ReadAllText(project_build_info);
            }
            catch (Exception e)
            {
                Completed(string.Format("文件读取失败:{0} error:{1}", project_build_info, e.ToString()));
                yield break;
            }
        }
        else 
        {  
            UnityWebRequest requestBuildInfo = UnityWebRequest.Get(project_build_info);
            yield return requestBuildInfo.SendWebRequest();
            if (string.IsNullOrEmpty(requestBuildInfo.error))
                Content = requestBuildInfo.downloadHandler.text;
            else
            {
                Completed(string.Format("读取文件失败:{0} error:{1}", project_build_info, requestBuildInfo.error));
                yield break;
            }
        }
         
        try
        {
            ProjectBuildInfo = JsonConvert.DeserializeObject<ProjectBuildInfo>(Content);
        }
        catch (Exception e)
        {
            Completed(string.Format("json 解析失败:{0} error:{1}", Content, e.ToString()));
            yield break;
        }


        Completed();

    }

    public static ReadBuildInProjectBuildInfoRequest BuildInProjectBuildInfo(string projectName) 
    { 
        string key = string.Format("BuildInProjectBuildInfo:{0}", projectName); 
        return AssetBundleManager.ExecuteOnlyOnceAtATime<ReadBuildInProjectBuildInfoRequest>(key, () =>
        {
            ReadBuildInProjectBuildInfoRequest request = new ReadBuildInProjectBuildInfoRequest();
            CoroutineStarter.Start(request.ReadBuildInProjectBuildInfo(projectName));
            return request;
        }); 
    }

}
