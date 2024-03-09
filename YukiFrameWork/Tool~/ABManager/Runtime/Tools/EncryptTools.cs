using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace XFABManager
{
    public class EncryptTools
    {

        /// <summary>
        /// DES加密
        /// </summary>
        /// <param name="desModel"></param>
        /// <returns></returns>
        public static byte[] Encrypt(byte[] data, string key)
        {
            //使用8位密钥
            byte[] key8 = new byte[8];
            //如果我们的密钥不是8为，则自动补全到8位
            byte[] byteKey = Encoding.UTF8.GetBytes(key.PadRight(key8.Length));
            //复制密钥
            Array.Copy(byteKey, key8, key8.Length);

            //使用8位向量
            byte[] iv8 = new byte[8];
            //如果我们的向量不是8为，则自动补全到8位
            byte[] byteIv = Encoding.UTF8.GetBytes(string.Empty.PadRight(iv8.Length));
            //复制向量
            Array.Copy(byteIv, iv8, iv8.Length);

            // 创建加密对象
            DESCryptoServiceProvider desProvider = new DESCryptoServiceProvider();
            desProvider.Mode = CipherMode.CBC;
            desProvider.Padding = PaddingMode.PKCS7;
            desProvider.Key = key8;
            desProvider.IV = iv8;
            byte[] result = null;
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream EncryptStream = new CryptoStream(ms, desProvider.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        EncryptStream.Write(data, 0, data.Length);
                        EncryptStream.FlushFinalBlock();
                        result = ms.ToArray();
                    }
                }
            }
            catch { }
            return result;
        }

        /// <summary>
        /// DES解密
        /// </summary>
        /// <param name="desModel"></param>
        /// <returns></returns>
        public static byte[] Decrypt(byte[] data, string key)
        {
            //使用8位密钥
            byte[] key8 = new byte[8];
            //如果我们的密钥不是8为，则自动补全到8位
            byte[] byteKey = Encoding.UTF8.GetBytes(key.PadRight(key8.Length));
            //复制密钥
            Array.Copy(byteKey, key8, key8.Length);

            //使用8位向量
            byte[] iv8 = new byte[8];
            //如果我们的向量不是8为，则自动补全到8位
            byte[] byteIv = Encoding.UTF8.GetBytes(string.Empty.PadRight(iv8.Length));
            //复制向量
            Array.Copy(byteIv, iv8, iv8.Length);

            // 创建解密对象
            DESCryptoServiceProvider desProvider = new DESCryptoServiceProvider();
            desProvider.Mode = CipherMode.CBC;
            desProvider.Padding = PaddingMode.PKCS7;
            desProvider.Key = key8;
            desProvider.IV = iv8;
            byte[] result = null;
            try
            {
                using (MemoryStream ms = new MemoryStream(data))
                {
                    using (CryptoStream DecryptStream = new CryptoStream(ms, desProvider.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        using (MemoryStream msResult = new MemoryStream())
                        {
                            byte[] temp = new byte[1024 * 1024];
                            int len = 0;
                            while ((len = DecryptStream.Read(temp, 0, temp.Length)) > 0)
                            {
                                msResult.Write(temp, 0, len);
                            }

                            result = msResult.ToArray();
                        }
                    }
                }
            }
            catch { }
            return result;
        }

        /// <summary>
        /// DES加密字符串
        /// </summary>
        /// <param name="data"></param>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        /// <returns></returns>
        public static string Encrypt(string data, string key)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(data);
            byte[] result = Encrypt(bytes, key);
            if (result == null)
            {
                return "";
            }
            return Convert.ToBase64String(result);
        }

        /// <summary>
        /// DES解密字符串
        /// </summary>
        /// <param name="data"></param>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        /// <returns></returns>
        public static string Decrypt(string data, string key)
        {
            byte[] bytes = Convert.FromBase64String(data);
            byte[] result = Decrypt(bytes, key);
            if (result == null)
            {
                return "";
            }
            return Encoding.UTF8.GetString(result);
        }

    }
}