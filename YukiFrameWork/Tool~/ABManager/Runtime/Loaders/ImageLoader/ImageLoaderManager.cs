using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// Fix 编码
namespace YukiFrameWork.ABManager {

    [Serializable]
    public class ImageData {
        public string key;
        public Texture2D texture;
        public Sprite sprite;
        internal List<int> references_hash_code = new List<int>(); // 当前这个image 被哪些游戏物体引用
        internal float last_time; // 最近一次使用这个image的时间
         
        /// <summary>
        /// 本地缓存路径
        /// </summary>
        public string native_path;

        public ImageLoaderType type;

        public void AddReference(int hash_code) { 
            if(references_hash_code.Contains(hash_code)) return;
            references_hash_code.Add(hash_code);
        }

        public void RemoveReference(int hash_code)
        {
            if (!references_hash_code.Contains(hash_code)) return;
            references_hash_code.Remove(hash_code);
        }

    }


    public class ImageLoaderManager
    {
        private const int UNLOAD_TIME_OUT = 5 * 60; // 5分钟 内没人使用就会卸载
        internal static Dictionary<string, ImageData> images = new Dictionary<string, ImageData>();

        private static WaitForSeconds check_recovery_interval = new WaitForSeconds(10); // 检测自动回收的间隔

        private static Coroutine automaticRecoveryCoroutine;

        /// <summary>
        /// 通过ImageLoader请求网络图片在本地的缓存文件夹路径
        /// </summary>
        public static string CachePath => string.Format("{0}/Caches/CacheImages", Application.temporaryCachePath);

        /// <summary>
        /// 当缓存到本地的图片超过多少时间会被删除 (单位:天)
        /// 默认超过 30 天会被删除 最小:1 最大:365
        /// </summary>
        public static int ClearImageCacheOutOfTime {
            get {
                return PlayerPrefs.GetInt("ClearImageCacheOutOfTime", 30);
            }
            set {
                PlayerPrefs.SetInt("ClearImageCacheOutOfTime", Mathf.Clamp(value, 1, 365));
                PlayerPrefs.Save();
            }
        }

        static ImageLoaderManager(){
            automaticRecoveryCoroutine = CoroutineStarter.Start(AutomaticRecovery());
            CoroutineStarter.Start(AutoClearCacheImage());
        }
         
        internal static ImageLoaderRequest LoadImage(ImageModel imageModel, int reference_hash_code)
        {
            if (automaticRecoveryCoroutine == null)
                automaticRecoveryCoroutine = CoroutineStarter.Start(AutomaticRecovery());

            ImageLoaderRequest imageRequest = AssetBundleManager.ExecuteOnlyOnceAtATime<ImageLoaderRequest>(imageModel.Key, () => {
                ImageLoaderRequest request = new ImageLoaderRequest();
                request.runing = CoroutineStarter.Start(request.RequestImage(imageModel));
                return request;
            });

            imageRequest.AddCompleteEvent((r) =>
            {
                if (r.NetworkImage != null)
                {
                    r.NetworkImage.AddReference(reference_hash_code);
                    //Debug.LogFormat("添加 hashcode :{0}",reference_hash_code);
                }
            });
             
            return imageRequest;
        }

        internal static void UnloadImage(ImageModel imageModel, int reference_hash_code)
        {
            if (!IsHaveImage(imageModel.Key)) return;
            images[imageModel.Key].RemoveReference(reference_hash_code);
        }

        internal static void AddNetworkImage(ImageData image)
        {
            if (images.ContainsKey(image.key)) return;
            images.Add(image.key, image);
        }

        internal static bool IsHaveImage(string url) { 
            return images.ContainsKey(url);
        }

        private static void UnloadNetworkImage(ImageData image)
        {  
            if(images.ContainsKey(image.key))
                images.Remove(image.key);
              
            if (image.type == ImageLoaderType.AssetBundle)
            {
                // 卸载资源
                AssetBundleManager.UnloadAsset(image.texture);
                AssetBundleManager.UnloadAsset(image.sprite);
            }
            else 
            { 
                // 如果不是从AssetBundle中加载的,直接销毁即可
                try
                {
                    GameObject.Destroy(image.sprite);
                    GameObject.Destroy(image.texture);
                }
                catch (Exception)
                {
                }
            } 
            
            image.sprite = null;
            image.texture = null;
            image = null;
        }


        private static IEnumerator AutomaticRecovery() {
            List<ImageData> need_unload_images = new List<ImageData>();
            while (true)
            {
                need_unload_images.Clear();
                foreach (ImageData image in images.Values)
                {
                    if (image.references_hash_code.Count == 0 && Time.time - image.last_time >= UNLOAD_TIME_OUT) 
                        need_unload_images.Add(image);
                }

                for (int i = 0; i < need_unload_images.Count; i++)
                {
                    UnloadNetworkImage(need_unload_images[i]);
                    yield return null;
                } 
                yield return check_recovery_interval;
            }
        }


        /// <summary>
        /// 获取本地所有缓存图片的总大小(当ImageLoader从网络加载图片时,会缓存到本地)(单位:字节)
        /// </summary>
        /// <returns></returns>
        public static long GetCacheImageSize() 
        {
            if (!Directory.Exists(CachePath)) return 0;
            return FileTools.GetDirectorySize(CachePath);
        }


        /// <summary>
        /// 清空从网络缓存的图片
        /// </summary>
        public static void ClearCacheImage() 
        {
            try
            {
                // 删除文件夹即可
                if (Directory.Exists(CachePath))
                    Directory.Delete(CachePath, true);
            }
            catch (Exception)
            { 
            }
            
        }

        /// <summary>
        /// 当缓存到本地的图片超过一定的时间时会把它删掉
        /// </summary>
        /// <returns></returns>
        private static IEnumerator AutoClearCacheImage() 
        {
            if (!Directory.Exists(CachePath)) yield break; 
            string[] files = Directory.GetFiles(CachePath);
            if(files == null || files.Length == 0) yield break;

            foreach (var file in files)
            {
                try
                {
                    DateTime time = File.GetLastWriteTime(file);
                    TimeSpan detal = DateTime.Now - time; 
                    if (detal.TotalDays >= ClearImageCacheOutOfTime)
                        File.Delete(file);
                }
                catch (Exception)
                {
                }

                yield return null;
            }

           
        } 
    }

}

