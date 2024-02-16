using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;


namespace YukiFrameWork.XFABManager
{

    // Fix 编码

    public class ImageLoaderRequest : CustomAsyncOperation<ImageLoaderRequest>
    {

        internal Coroutine runing = null;

        private DownloadFileRequest request_file;
        private UnityWebRequest request_local_file;

        private const int MAX_DOWNLOAD_COUNT = 10; // 最多同时下载10个
        private static int currentDownloadCount = 0;
        #region 属性
        public ImageData NetworkImage { get; private set; }

        public Action<float> onProgressChange;

        #endregion

        public IEnumerator RequestImage(ImageModel imageModel ) {

            // 判断该图片是否已经存在
            if (ImageLoaderManager.IsHaveImage(imageModel.Key))
            {
                NetworkImage = ImageLoaderManager.images[imageModel.Key]; 
                NetworkImage.last_time = Time.time; 
                onProgressChange?.Invoke(1);
                Completed();
                yield break;
            }

            onProgressChange?.Invoke(0);

            if (imageModel.type == ImageLoaderType.AssetBundle)
            {
                Texture2D texture = null;
                Sprite sprite = null;

                if (imageModel.load_type == ImageLoadType.ASync)
                {
                    LoadAssetRequest texture_request = AssetBundleManager.LoadAssetAsyncWithoutTips<Texture2D>(imageModel.projectName, imageModel.assetName);
                    while (texture_request != null && !texture_request.isDone)
                    {
                        yield return null;
                        onProgressChange?.Invoke(texture_request.progress);
                    } 
                    LoadAssetRequest sprite_request = AssetBundleManager.LoadAssetAsyncWithoutTips<Sprite>(imageModel.projectName, imageModel.assetName);
                    yield return sprite_request;

                    texture = texture_request?.asset as Texture2D;
                    sprite = sprite_request?.asset as Sprite;
                }
                else 
                {
                    texture = AssetBundleManager.LoadAssetWithoutTips<Texture2D>(imageModel.projectName,imageModel.assetName);
                    sprite = AssetBundleManager.LoadAssetWithoutTips<Sprite>(imageModel.projectName, imageModel.assetName);
                }


                if (texture == null && sprite == null)
                {
                    // 加载失败
                    Completed(string.Format("资源加载失败!projectName:{0} assetName:{1}", imageModel.projectName, imageModel.assetName));
                    yield break;
                }

                NetworkImage = new ImageData();
                NetworkImage.key = imageModel.Key;
                NetworkImage.texture = texture;
                NetworkImage.sprite = sprite;
                NetworkImage.last_time = Time.time;
                NetworkImage.native_path = string.Empty;
                NetworkImage.type = imageModel.type;
                ImageLoaderManager.images.Add(NetworkImage.key, NetworkImage);
                Completed(); 
            }
            else 
            {
                while (currentDownloadCount > MAX_DOWNLOAD_COUNT)
                {
                    yield return null;
                }

                currentDownloadCount++;

                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(imageModel.path);  
                string fileName = XFABTools.md5(fileNameWithoutExtension); // 防止图片名称中包含特殊字符 导致异常
                 
                string path = string.Empty;

                if (imageModel.type == ImageLoaderType.Network)
                {

                    // 把图片下载到本地
                    path = string.Format("{0}/Caches/CacheImages/{1}", Application.temporaryCachePath, fileName);

                    if (!File.Exists(path))
                    {
                        // 本地没有该文件 从服务端下载
                        // 下载文件
                        request_file = DownloadFileRequest.Download(imageModel.path, path);

                        while (!request_file.isDone) {
                            yield return null;
                            onProgressChange.Invoke(request_file.progress * 0.5f); 
                        }

                        //yield return request_file;

                        if (!string.IsNullOrEmpty(request_file.error))
                        {
                            Completed(request_file.error);
                            currentDownloadCount--;
                            yield break;
                        } 
                    }
                }
                else if (imageModel.type == ImageLoaderType.Local)
                {
                    path = imageModel.path;
                    if (!File.Exists(path))
                    {
                        Completed(string.Format("未发现到文件:{0}", path));
                        currentDownloadCount--;
                        yield break;
                    }
                }


                // 从本地读取
                request_local_file = UnityWebRequestTexture.GetTexture(string.Format("file://{0}", path), true);
                request_local_file.disposeUploadHandlerOnDispose = true;
                request_local_file.disposeDownloadHandlerOnDispose = true;
                request_local_file.timeout = 5;

                UnityWebRequestAsyncOperation operation = request_local_file.SendWebRequest();

                while (!operation.isDone) {
                    yield return null;
                    onProgressChange?.Invoke(operation.progress * 0.5f + 0.5f);
                }


                onProgressChange?.Invoke(1);

#if UNITY_2020_1_OR_NEWER
                if (request_local_file.result == UnityWebRequest.Result.Success && string.IsNullOrEmpty(request_local_file.error))
#else
                if (string.IsNullOrEmpty(request_local_file.error))
#endif
                {
                    DownloadHandlerTexture handler = request_local_file.downloadHandler as DownloadHandlerTexture;
                    Texture2D texture2D = handler.texture;
                    texture2D.name = fileName;
                    Sprite sprite = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
                    sprite.name = texture2D.name;

                    NetworkImage = new ImageData();
                    NetworkImage.key = imageModel.Key;
                    NetworkImage.texture = texture2D;
                    NetworkImage.sprite = sprite;
                    //NetworkImage.AddReference(reference_hash_code);
                    NetworkImage.last_time = Time.time;
                    NetworkImage.native_path = path;
                    NetworkImage.type = imageModel.type;
                    ImageLoaderManager.images.Add(NetworkImage.key, NetworkImage); 
                    Completed();
                }
                else
                    Completed($"请求url={imageModel.path}失败，错误{request_local_file.error}");

                currentDownloadCount--;
            }
             

        }


        /// <summary>
        /// 中断请求
        /// </summary>
        public void Abort()
        {
            if (runing == null) return;
            CoroutineStarter.Stop(runing);
            request_file?.Abort();
            request_local_file?.Abort();
            if (currentDownloadCount > 0) currentDownloadCount--;
            Completed("request abort!");
        }

    }

}