using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XFABManager
{
    public interface IServerFilePath
    {
        string ServerPath(string url, string projectName, string version, string fileName);
    }

}

