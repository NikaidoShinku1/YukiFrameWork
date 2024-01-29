using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using YukiFrameWork.XFABManager;

public class FileMD5Request : CustomAsyncOperation<FileMD5Request>
{

    public string md5 { get;private set; }
    
    private const int md5_max_file_size = 1024 * 1024 * 1;         // 如果大于50mb 是为大文件

    private const int file_stream_buffer_min = 1024 * 1024;         // 文件流缓冲区最小值 1 MB

    private const int file_stream_buffer_max = 1024 * 1024 * 20;    // 文件流缓冲区最大值 20MB


    private const int md5_cache_file_count = 1000;                  // 缓存1000个 文件的md5值 如果超出则清空缓存


    //经过考虑 缓存文件md5值得功能 不太需要,
    //因为检测更新 或者 验证文件 都需要拿到文件最新的md5值 计算是必不可少的
    //所以没有缓存的必要
    //private static Dictionary<string, string> md5_caches = new Dictionary<string, string>();


    public IEnumerator CaculateMD5(string filePath) {

        using (FileStream inputStream = File.OpenRead(filePath))
        {
            if (inputStream.Length < md5_max_file_size)
            {
                md5 = XFABTools.md5file(inputStream);
            }
            else
            {
                MD5CryptoServiceProvider hashAlgorithm = new MD5CryptoServiceProvider();
                byte[] buffer = GetFileStreamBuffer((int)inputStream.Length);
                var output = new byte[buffer.Length];

                int timerFrame = 0;

                while (inputStream.Position < inputStream.Length)
                {
                    int count = inputStream.Read(buffer, 0, buffer.Length);
                    if (inputStream.Position < inputStream.Length)
                        //分块计算哈希值
                        hashAlgorithm.TransformBlock(buffer, 0, buffer.Length, output, 0);
                    else
                        hashAlgorithm.TransformFinalBlock(buffer, 0, count);

                    if (IsWaitForFrame(ref timerFrame))
                    {
                        yield return null;
                    }
                }
                // 有文件 判断md5值 需不需要更新
                md5 = CaculateProviderMd5(hashAlgorithm);
            }
        }


        yield return null;  // 防止 completed 没有触发
        Completed();
    }

    private byte[] GetFileStreamBuffer(int length) {
        int bufferSize = (length / 200);// 缓冲区大小，10MB                                      //if (bufferSize <= 0) { bufferSize = 1; }
        bufferSize = (int)Mathf.Clamp(bufferSize, file_stream_buffer_min, file_stream_buffer_max);  // 缓冲区 范围 1mb - 20mb
        return new byte[bufferSize];
    }

    private string CaculateProviderMd5(MD5CryptoServiceProvider provider) {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < provider.Hash.Length; i++)
            sb.Append(provider.Hash[i].ToString("x2"));
        return sb.ToString();
    }

    private bool IsWaitForFrame(ref int timerFrame) {
        timerFrame++;

        if (timerFrame >= 15) {
            timerFrame = 0;
            return true;
        }

        return false;
    }

}
