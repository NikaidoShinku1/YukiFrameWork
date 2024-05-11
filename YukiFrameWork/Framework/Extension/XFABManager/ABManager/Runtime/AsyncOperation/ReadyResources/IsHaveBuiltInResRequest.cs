using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace XFABManager
{

    public class IsHaveBuiltInResRequest : CustomAsyncOperation<IsHaveBuiltInResRequest>
    {

        //public BuildInResType buildInResType { get; private set; }
        public bool isHave { get; private set; }

        internal IEnumerator IsHaveBuiltInRes(string projectName)
        {
            yield return null;
            string project_build_info = string.Format("{0}{1}", XFABTools.BuildInDataPath(projectName), XFABConst.project_build_info);

            if (XFABTools.StreamingAssetsReadable() || Application.isEditor)
            {
                isHave = File.Exists(project_build_info);
            }
            else 
            {
                UnityWebRequest requestFiles = UnityWebRequest.Get(project_build_info);
                yield return requestFiles.SendWebRequest();
                isHave = string.IsNullOrEmpty(requestFiles.error);
            }

            Completed();
        }

    }


}


