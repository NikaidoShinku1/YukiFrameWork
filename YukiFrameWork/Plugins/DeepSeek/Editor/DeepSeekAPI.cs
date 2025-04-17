///=====================================================
/// - FileName:      DeepSeekAPI.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   高级定制脚本生成
/// - Creation Time: 2025/4/15 17:19:10
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.Networking;
using System.IO;
using System.Security.Cryptography;
using System.Text;
namespace YukiFrameWork
{
    public static class DeepSeekAPI 
    {
        public static string DeepSeekCode => "[YukiFrameWork DeepSeek 代码生成:] ";
        [Serializable]
        public struct Request
        {
            public string model;
            public RequestMessage[] messages;
        }
        // Struct representing a single message in the request
        [Serializable]
        public struct RequestMessage
        {
            // 发送消息的用户
            public string role;
            // 消息
            public string content;
        }
        // Struct representing the response payload received from the DeepSeek API.
        [Serializable]
        public struct Response
        {
            public string id;
            public ResponseChoice[] choices;
        }

        [Serializable]
        public struct ResponseChoice
        {
            public int index;
            public ResponseMessage message;
        }

        [Serializable]
        public struct ResponseMessage
        {
            public string role;
            public string content;
        }

        // Submits a prompt to the DeepSeek API and returns the generated response content.
        public static string Submit(string prompt)
        {
            // Retrieves the singleton instance of settings for API configuration.
            var settings = ScriptGeneratorSettings.instance;
            // Creates a request object with the specified model and a single user message containing the prompt.
            var request = new Request
            {
                model = settings.model,
                messages = new[] { new RequestMessage() { role = "user", content = prompt } }
            };

            // Serializes the request struct to JSON format for the API request.
            var requestJson = JsonUtility.ToJson(request);

            // Configures the UnityWebRequest based on Unity version for compatibility.
#if UNITY_2022_2_OR_NEWER
            // Uses the newer, simpler Post method signature in Unity 2022.2+.
            using var post = UnityWebRequest.Post(settings.Url, requestJson, "application/json");
#else
        // Uses the older, manual configuration for Unity versions before 2022.2.
        using var post = new UnityWebRequest(settings.Url, "POST");
        post.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(requestJson));
        post.downloadHandler = new DownloadHandlerBuffer();
        post.SetRequestHeader("Content-Type", "application/json");
#endif

            post.timeout = settings.timeout;

            var userEmail = CloudProjectSettings.userName;
            if (string.IsNullOrEmpty(userEmail))
            {
                var m = DeepSeekCode + " You must be signed in to Unity to use this feature.";
                throw new Exception(m);
            }

            var key = Encryption.Decrypt(settings.apiKey, userEmail);
            post.SetRequestHeader("Authorization", "Bearer " + key);

            var webRequest = post.SendWebRequest();

            var time = Time.realtimeSinceStartup;
            var cancel = false;
            var progress = 0f;

            while (!webRequest.isDone && !cancel)
            {
                cancel = EditorUtility.DisplayCancelableProgressBar("AI代码生成", "生成脚本中...",
                                                                    progress += 0.01f);
                System.Threading.Thread.Sleep(100);

                var timeout = settings.useTimeout ? settings.timeout + 1f : float.PositiveInfinity;
                if (Time.realtimeSinceStartup - time > timeout)
                {
                    EditorUtility.ClearProgressBar();
                    throw new TimeoutException($"{DeepSeekCode} Request timed out");
                }
            }

            EditorUtility.ClearProgressBar();

            var responseJson = post.downloadHandler.text;
            if (!string.IsNullOrEmpty(post.error))
            {
                throw new Exception($"{DeepSeekCode}{post.error}");
            }

            if (string.IsNullOrEmpty(responseJson))
            {
                throw new Exception($"{DeepSeekCode}No response received");
            }

            var data = JsonUtility.FromJson<Response>(responseJson);
            if (data.choices == null || data.choices.Length == 0)
            {
                throw new Exception($"{DeepSeekCode}No choices received");
            }

            return data.choices[0].message.content;
        }

        public abstract class Encryption
        {
            public static string Encrypt(string plainText, string email)
            {
                byte[] key, iv;
                GetKeyAndIv(email, out key, out iv);

                using var aes = new AesManaged { Key = key, IV = iv };
                using var encryptor = aes.CreateEncryptor();
                using var memoryStream = new MemoryStream();
                using var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
                using (var streamWriter = new StreamWriter(cryptoStream))
                {
                    streamWriter.Write(plainText);
                }

                return Convert.ToBase64String(memoryStream.ToArray());
            }

            public static string Decrypt(string encryptedText, string email)
            {
                byte[] key, iv;
                GetKeyAndIv(email, out key, out iv);

                using var aes = new AesManaged { Key = key, IV = iv };
                using var decryptor = aes.CreateDecryptor();
                using var memoryStream = new MemoryStream(Convert.FromBase64String(encryptedText));
                using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
                using var streamReader = new StreamReader(cryptoStream);
                return streamReader.ReadToEnd();
            }

            private static void GetKeyAndIv(string email, out byte[] key, out byte[] iv)
            {
                using var sha256 = SHA256.Create();
                var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(email));
                key = new byte[16];
                iv = new byte[16];
                Array.Copy(hash, 0, key, 0, 16);
                Array.Copy(hash, 16, iv, 0, 16);
            }
        }
    }
}
#endif