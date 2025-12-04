using UnityEngine.Networking;
using System.IO;
using System;
using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace XFABManager
{

    public class DownloadHandlerFileRange : DownloadHandlerScript
    {

        #region 私有字段
        private string Path;//文件保存的路径
        private FileStream FileStream;
        private UnityWebRequest unityWebRequest;
        private long LocalFileSize = 0;//本地已经下载的文件的大小
        private long TotalFileSize = 0;//文件的总大小
        private long CurFileSize = 0;//当前的文件大小
        private Dictionary<long, long> number_of_bytes_downloaded_per_second = new Dictionary<long, long>();
        private List<long> temp_list = new List<long>();
        #endregion

        /// <summary>
        /// 文件正式开始下载事件,此事件触发以后即可获取到文件的总大小
        /// </summary>
        public event System.Action StartDownloadEvent;

        /// <summary>
        /// ReceiveData()之后调用的回调，参数是本地文件已经下载的总大小
        /// </summary>
        public event System.Action<ulong> DownloadedSizeUpdateEvent;

        #region 属性
        /// <summary>
        /// 下载速度,单位:Byte/s 保留两位小数
        /// </summary>
        public long Speed
        {
            // 下载速度就是一秒钟下载了多少数据，当前这一秒还没有过完，所以只能知道上一秒下了多少，
            // 所以优先使用上一秒下载数据的总量作为下载速度
            get
            {
                long cur_time = DateTimeTools.DateTimeToTimestamp(DateTime.Now, false);

                // 判断是否拥有上一秒的数据，如果有则返回上一秒种一共下载了多少数据，这个值为下载速度
                if (number_of_bytes_downloaded_per_second.ContainsKey(cur_time - 1))
                    return number_of_bytes_downloaded_per_second[cur_time - 1];
                // 如果没有上一秒的数据，则判断是否拥有当前这一秒的数据，如果有则返回
                if (number_of_bytes_downloaded_per_second.ContainsKey(cur_time))
                    return number_of_bytes_downloaded_per_second[cur_time];
                return 0;
            }
        }

        /// <summary>
        /// 文件的总大小
        /// </summary>
        public long FileSize
        {
            get
            {
                return TotalFileSize;
            }
        }

        /// <summary>
        /// 下载进度[0,1]
        /// </summary>
        public float DownloadProgress
        {
            get
            {
                return GetProgress();
            }
        }

        /// <summary>
        /// 已经下载的文件大小
        /// </summary>
        public long DownloadedSize => CurFileSize;

        #endregion

        #region 公共方法
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="path">文件保存的路径</param>
        /// <param name="request">UnityWebRequest对象,用来获文件大小,设置断点续传的请求头信息</param>
        public DownloadHandlerFileRange(string path, UnityWebRequest request) : base()
        {
            Init(path, request);
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="path">文件保存的路径</param>
        /// <param name="request">UnityWebRequest对象,用来获文件大小,设置断点续传的请求头信息</param>
        /// <param name="bufferLength">下载缓冲区大小</param>
        public DownloadHandlerFileRange(string path, UnityWebRequest request, long bufferLength) : base(new byte[bufferLength])
        {
            if (bufferLength <= 0) throw new Exception("bufferLength必须大于0!");
            Init(path, request);
        }


        private void Init(string path, UnityWebRequest request)
        {
            Path = path;
            string directory = System.IO.Path.GetDirectoryName(Path);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            try
            {
                FileStream = new FileStream(Path, FileMode.Append, FileAccess.Write);
            }
            catch (FileNotFoundException)
            {
                System.IO.File.Create(Path).Close(); // 创建文件
                FileStream = new FileStream(Path, FileMode.Append, FileAccess.Write);
            }
            catch (IOException e)
            {
                throw e;
            }

            unityWebRequest = request;
            if (File.Exists(path))
            {
                LocalFileSize = new System.IO.FileInfo(path).Length;
            }
            CurFileSize = LocalFileSize;

            unityWebRequest.SetRequestHeader("Range", "bytes=" + LocalFileSize + "-");
        }
        #endregion


        public void Close()
        {
            if (FileStream == null) return;

            try
            {
                FileStream.Close();
                FileStream.Dispose();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                FileStream = null;
            }
        }


        #region 私有继承的方法

        /// <summary>
        /// 调用UnityWebRequest.downloadProgress属性时,将会调用该方法,用于返回下载进度
        /// </summary>
        /// <returns></returns>
        protected override float GetProgress()
        {
            return TotalFileSize == 0 ? 0 : ((float)CurFileSize) / TotalFileSize;
        }

        //Note:当下载的文件数据大于2G时,该int类型的参数将会数据溢出,所以先自己通过响应头来获取长度,获取不到再使用参数的方式
        protected override void ReceiveContentLengthHeader(ulong contentLength)
        {
            string contentLengthStr = unityWebRequest.GetResponseHeader("Content-Length");

            if (!string.IsNullOrEmpty(contentLengthStr))
            {
                try
                {
                    TotalFileSize = long.Parse(contentLengthStr);
                }
                catch (Exception)
                {
                    TotalFileSize = (long)contentLength;
                }
            }
            else
            {
                TotalFileSize = (long)contentLength;
            }
            //这里拿到的下载大小是待下载的文件大小,需要加上本地已下载文件的大小才等于总大小
            TotalFileSize += LocalFileSize;
            StartDownloadEvent?.Invoke();
        }

        protected override bool ReceiveData(byte[] data, int dataLength)
        {
            if (data == null || dataLength == 0 || unityWebRequest.responseCode > 400)
            {
                return false;
            }

            if (FileStream == null) return false;

            try
            {

                FileStream.Write(data, 0, dataLength);
                FileStream.Flush();
                CurFileSize += dataLength;
                DownloadedSizeUpdateEvent?.Invoke((ulong)CurFileSize);
            }
            catch (Exception)
            {
                return false;
            }
            // 记录某一秒钟下载的字节数量
            // 当前的时间 
            temp_list.Clear();
            long cur_time = DateTimeTools.DateTimeToTimestamp(DateTime.Now, false);
            if (number_of_bytes_downloaded_per_second.ContainsKey(cur_time))
                number_of_bytes_downloaded_per_second[cur_time] += dataLength;
            else
                number_of_bytes_downloaded_per_second.Add(cur_time, dataLength);


            // 清除旧的数据
            foreach (var item in number_of_bytes_downloaded_per_second.Keys)
                if (cur_time - item > 2)
                    temp_list.Add(item);

            foreach (var item in temp_list)
                number_of_bytes_downloaded_per_second.Remove(item);

            temp_list.Clear();

            return true;
        }


        #endregion


    }

}