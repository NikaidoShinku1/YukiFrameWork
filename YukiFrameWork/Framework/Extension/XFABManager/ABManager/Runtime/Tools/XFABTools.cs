using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;


#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

namespace XFABManager
{
    // 工具方法
    public class XFABTools
    {
        /// <summary>
        /// 某个资源模块的数据存放目录 
        /// </summary>
        /// <param name="projectName">资源Project 的 name</param>
        /// <returns></returns>
        public static string DataPath(string projectName)
        {
            // 如果说StreamingAssets目录有写入权限 那数据目录就使用StreamingAsset 
            if (StreamingAssetsWritable()) {
                return string.Format("{0}/{1}/{2}", Application.streamingAssetsPath, projectName, GetCurrentPlatformName());
            }
            return string.Format("{0}/{1}/{2}", Application.persistentDataPath, projectName, GetCurrentPlatformName());
        }


        /// <summary>
        /// 内置数据目录(安装包中的数据的目录) 
        /// </summary>
        /// <param name="projectName">资源Project 的 name</param>
        /// <returns></returns>
        public static string BuildInDataPath(string projectName)
        {
            return string.Format("{0}/{1}/{2}/", Application.streamingAssetsPath, projectName, GetCurrentPlatformName());
        }
        /// <summary>
        /// 内置数据目录(安装包中的数据的目录) 
        /// </summary>
        /// <returns></returns>
        public static string BuildInDataPath(string projectName, string fileName)
        {
            return string.Format("{0}/{1}/{2}/{3}", Application.streamingAssetsPath, projectName, GetCurrentPlatformName(), fileName);
        }

        /// <summary>
        /// 资源的网络路径
        /// </summary>
        /// <param name="url">网络路径</param>
        /// <param name="projectName">项目名</param>
        /// <param name="version">版本</param>
        /// <param name="fileName">文件名</param>
        /// <returns></returns>
        internal static string ServerPath(string url, string projectName, string version, string fileName)
        {
            return AssetBundleManager.ServerFilePath.ServerPath(url, projectName, version, fileName);
            //return string.Format("{0}/{1}/{2}/{3}/{4}", url, projectName, version, GetCurrentPlatformName(), fileName);
        }


        /// <summary>
        /// 缓存到本地的资源文件路径(数据目录的资源,加载资源时从该目录读取AssetBundle文件)
        /// </summary>
        /// <param name="projectName">项目名</param>
        /// <param name="fileName">文件名 需要包含后缀</param>
        /// <returns></returns>
        public static string LocalResPath(string projectName, string fileName)
        {
            return string.Format("{0}/{1}", DataPath(projectName), fileName);
        }

        /// <summary>
        /// 获取当前平台的名称
        /// </summary>
        /// <returns></returns>
        public static string GetCurrentPlatformName()
        {
             
#if UNITY_EDITOR
            return EditorUserBuildSettings.activeBuildTarget.ToString();
#else
            switch (Application.platform)
            {
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.OSXPlayer:
                    return "StandaloneOSX";
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.WindowsEditor:
#if UNITY_64
                    return "StandaloneWindows64";
#else
                    return "StandaloneWindows";
#endif
                case RuntimePlatform.IPhonePlayer:
                    return "iOS";

                case RuntimePlatform.LinuxPlayer:
                case RuntimePlatform.LinuxEditor:
                    return "StandaloneLinux64";

                case RuntimePlatform.WebGLPlayer:
                    return "WebGL";

                case RuntimePlatform.WSAPlayerX86:
                case RuntimePlatform.WSAPlayerX64:
                case RuntimePlatform.WSAPlayerARM:
                    return "WSAPlayer";

                case RuntimePlatform.Android:
                case RuntimePlatform.PS4:
                case RuntimePlatform.XboxOne:
                case RuntimePlatform.tvOS:
                case RuntimePlatform.Switch:
                //case RuntimePlatform.Lumin:
                //case RuntimePlatform.Stadia:
                    return Application.platform.ToString();
            }


            return Application.platform.ToString();
#endif

        }

        /// <summary>
        /// 计算字符串的MD5值
        /// </summary>
        public static string md5(string source)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] data = System.Text.Encoding.UTF8.GetBytes(source);
            byte[] md5Data = md5.ComputeHash(data, 0, data.Length);
            md5.Clear();

            string destString = "";
            for (int i = 0; i < md5Data.Length; i++)
            {
                destString += System.Convert.ToString(md5Data[i], 16).PadLeft(2, '0');
            }
            destString = destString.PadLeft(32, '0');
            return destString;
        }

        internal static void SaveVersion(string projectName,string version) {
            string file_path = LocalResPath(projectName, XFABConst.project_version);
            if (File.Exists(file_path)) File.Delete(file_path);
            string directory = Path.GetDirectoryName(file_path);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            File.WriteAllText(file_path, version);
        }

        /// <summary>
        /// 查询本地某个模块缓存的资源版本号
        /// </summary>
        /// <param name="projectName"></param>
        public static string GetLocalVersion(string projectName) 
        {
            string project_build_info_path = LocalResPath(projectName, XFABConst.project_build_info);
            if (File.Exists(project_build_info_path))
            { 
                try
                {
                    ProjectBuildInfo info = JsonConvert.DeserializeObject<ProjectBuildInfo>(File.ReadAllText(project_build_info_path)); 
                    return info.version;
                }
                catch (Exception)
                { 
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// 计算文件的MD5值
        /// </summary>
        public static string md5file(string file)
        {
            try
            {
                if (Application.platform == RuntimePlatform.WebGLPlayer)
                { 
                    return md5file(File.ReadAllBytes(file));
                }
                else 
                { 
                    FileStream fs = new FileStream(file, FileMode.Open);
                    return md5file(fs);
                }

            }
            catch (Exception ex)
            {
                throw new Exception("md5file() fail, error:" + ex.Message);
            }
        }

        internal static string md5file(FileStream fileStream)
        {

            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] retVal = md5.ComputeHash(fileStream);
            fileStream.Close();

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }
            return sb.ToString();

        }

        internal static string md5file(byte[] bytes)
        { 

            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] retVal = md5.ComputeHash(bytes);
              
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }
            return sb.ToString();

        }

        /// <summary>
        /// 异步计算md5的值,如果文件过大推荐使用此方法
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static FileMD5Request CaculateFileMD5(string path) {

            string key = string.Format("CaculateFileMD5:{0}", path);

            return AssetBundleManager.ExecuteOnlyOnceAtATime<FileMD5Request>(key, () =>
            {
                FileMD5Request request = new FileMD5Request();
                CoroutineStarter.Start(request.CaculateMD5(path));
                return request;
            });
        }

        /// <summary>
        /// 判断一个类是否继承另外一个类(如果传入两个相同的类会返回false)
        /// </summary>
        public static bool IsBaseByClass(Type source, Type target)
        {
            if (source == target) return false;
            while (source.BaseType != null)
            {
                if (source.BaseType == target) return true;
                source = source.BaseType;
            }
            return false;
        }


        /// <summary>
        /// 判断一个类是否实现了某个接口
        /// </summary>
        public static bool IsImpInterface(Type source,Type target) {
            return source.GetInterface(target.FullName) != null;
        }

        /// <summary>
        /// 判断当前的平台的StreamingAssets目录是否有写入的权限
        /// 如果具有写入权限，则数据目录和内置目录为同一个
        /// </summary>
        /// <returns></returns>
        internal static bool StreamingAssetsWritable() 
        {

//#if UNITY_OPENHARMONY
//#endif

#if UNITY_EDITOR

            return false;
#else
            if (Application.platform == RuntimePlatform.Android || 
                Application.platform == RuntimePlatform.IPhonePlayer || 
                Application.platform == RuntimePlatform.WebGLPlayer)
                return false;  // 当前认为 Android , iOS , WebGL 没有写入权限

#if UNITY_OPENHARMONY // 如果是鸿蒙是不能写的
            return false;
#endif

            return true;
#endif
        }

        /// <summary>
        /// 判断当前平台的StreamingAssets目录是否有读取权限 (直接通过 File.Open 读取)
        /// 当资源加载模式处于非更新模式下判断是否能够直接读取StreamingAssets的文件
        /// 如果能直接读取，则不需要释放资源(因为AssetBundle的读取优先从数据目录读，读不到则从内置目录读)
        /// 如果不能直接读取则需要把资源从内置目录复制到数据目录
        /// </summary>
        /// <returns></returns>
        internal static bool StreamingAssetsReadable() 
        {
             

#if UNITY_EDITOR
            return false;
#else

#if UNITY_OPENHARMONY // 如果是鸿蒙 不能直接读取内置目录
            return false;
#endif 
            if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.WebGLPlayer) 
                return false; // 目前发现Android和WebGL平台不能直接读取
            return true;
#endif 
        }


    }

}

