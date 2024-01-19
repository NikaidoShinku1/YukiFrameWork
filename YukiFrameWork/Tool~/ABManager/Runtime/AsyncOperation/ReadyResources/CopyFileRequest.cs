using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;


namespace YukiFrameWork.ABManager
{
    public class CopyFileRequest : CustomAsyncOperation<CopyFileRequest>
    {
        /// <summary>
        /// 从 app 内置目录复制文件
        /// </summary>
        /// <param name="source"></param>
        /// <param name="des"></param>
        /// <returns></returns>
        public IEnumerator Copy(string source, string des)
        {

            yield return new WaitForEndOfFrame();

            string path = Path.GetDirectoryName(des);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            if (File.Exists(des)) File.Delete(des);

#if UNITY_STANDALONE_OSX || UNITY_IOS || UNITY_EDITOR_OSX
            UnityWebRequest copyFile = UnityWebRequest.Get(string.Format("file://{0}", source));
#else
            UnityWebRequest copyFile = UnityWebRequest.Get(source);
#endif
            DownloadHandlerFile handlerFile = new DownloadHandlerFile(des);
            copyFile.downloadHandler = handlerFile;
            yield return copyFile.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
            if (copyFile.result != UnityWebRequest.Result.Success)
#else
            if ( !string.IsNullOrEmpty(copyFile.error) )
#endif
            {
                Completed(copyFile.error);
                yield break;
            }

            Completed();
        }
    }

}

