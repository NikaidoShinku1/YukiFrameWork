using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YukiFrameWork.ABManager
{
    public interface IServerFilePath
    {
        string ServerPath(string url, string projectName, string version, string fileName);
    }

}

