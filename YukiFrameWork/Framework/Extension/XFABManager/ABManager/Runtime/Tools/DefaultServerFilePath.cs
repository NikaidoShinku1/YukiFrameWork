using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XFABManager
{
    public class DefaultServerFilePath : IServerFilePath
    {
        public string ServerPath(string url, string projectName, string version, string fileName)
        {
            if (url.EndsWith("/")) url = url.TrimEnd('/');
            return string.Format("{0}/{1}/{2}/{3}/{4}", url, projectName, version, XFABTools.GetCurrentPlatformName(), fileName);
        }
    }

}
