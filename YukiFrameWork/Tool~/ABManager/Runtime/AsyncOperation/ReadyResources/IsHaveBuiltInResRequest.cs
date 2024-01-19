using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace YukiFrameWork.ABManager
{

    public class IsHaveBuiltInResRequest : CustomAsyncOperation<IsHaveBuiltInResRequest>
    {

        //public BuildInResType buildInResType { get; private set; }
        public bool isHave { get; private set; }

        internal IEnumerator IsHaveBuiltInRes(string projectName)
        {
            yield return null;
            string project_build_info = string.Format("{0}{1}", ABTools.BuildInDataPath(projectName), ABConst.project_build_info);
#if UNITY_ANDROID && !UNITY_EDITOR
            UnityWebRequest requestFiles = UnityWebRequest.Get(project_build_info);
            yield return requestFiles.SendWebRequest();
            isHave = string.IsNullOrEmpty(requestFiles.error);
#else
            isHave = File.Exists(project_build_info);
#endif
            Completed();
        }

    }


}


