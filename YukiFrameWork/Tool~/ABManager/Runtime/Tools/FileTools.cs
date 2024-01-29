using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace YukiFrameWork.XFABManager
{
    public class FileTools
    {

        /// <summary>
        /// 复制一个文件夹(可在编辑器非运行状态下调用)
        /// </summary>
        /// <param name="sourceDir">源文件夹</param>
        /// <param name="destDir">目标文件夹</param>
        /// <param name="progress">进度改变的回调 第一个参数是正在复制的文件名称 第二个是复制的进度</param>
        /// <param excludeSuffix="excludeSuffix">不需要复制的文件的后缀</param>
        public static bool CopyDirectory(string sourceDir, string destDir, Action<string, float> progress,string[] excludeSuffix = null)
        {
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDir))
            {
                Directory.CreateDirectory(destDir);
            }

            if (!Directory.Exists(sourceDir))
            {
                Debug.LogError(string.Format(" 复制失败 ! 源目录{0}不存在! ", sourceDir));
                return false;
            }

            foreach (string folderPath in Directory.GetDirectories(sourceDir, "*", SearchOption.AllDirectories))
            {
                if (!Directory.Exists(folderPath.Replace(sourceDir, destDir)))
                    Directory.CreateDirectory(folderPath.Replace(sourceDir, destDir));
            }
            string[] files = Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories);
            //string filePath = null;
            for (int i = 0; i < files.Length; i++)
            {
                //filePath = files[i];
                if (excludeSuffix!=null && excludeSuffix.Length != 0) {
                    bool isContinue = false;
                    foreach (var item in excludeSuffix)
                    {
                        if (Path.GetExtension(files[i]).Equals(item)) {
                            isContinue = true;
                            Debug.LogFormat("跳过文件:{0} 后缀:{1} 过滤后缀:{2}", files[i], Path.GetExtension(files[i]), item);
                            break;
                        }
                    }
                    if (isContinue) continue; 
                }
                
                var fileDirName = Path.GetDirectoryName(files[i]).Replace("\\", "/");
                var fileName = Path.GetFileName(files[i]);
                string newFilePath = Path.Combine(fileDirName.Replace(sourceDir, destDir), fileName);

                progress?.Invoke(fileName, (float)(i + 1) / (float)files.Length);

                File.Copy(files[i], newFilePath, true);
            }

            return true;
        }

        /// <summary>
        /// 异步复制一个文件(可用于Android平台下的StreamingAssets目录文件复制,不可在编辑器非运行状态下调用)
        /// </summary>
        /// <returns></returns>
        public static CopyFileRequest Copy(string source, string des)
        {
            string key = string.Format("copy:{0}{1}", source, des);
            return AssetBundleManager.ExecuteOnlyOnceAtATime<CopyFileRequest>(key, () => {
                CopyFileRequest request = new CopyFileRequest();
                CoroutineStarter.Start(request.Copy(source, des));
                return request;
            });
        }

        /// <summary>
        /// 获取文件夹大小(可在编辑器非运行状态下调用)
        /// </summary>
        /// <param name="dirPath"></param>
        /// <returns></returns>
        public static long GetDirectorySize(string dirPath)
        {
            DirectoryInfo folderInfo = new DirectoryInfo(dirPath);
            long size = 0;

            // 循环遍历文件夹中的所有文件
            foreach (System.IO.FileInfo file in folderInfo.GetFiles("*", SearchOption.AllDirectories))
            {
                size += file.Length;
            }

            return size;
        }
    }

}

