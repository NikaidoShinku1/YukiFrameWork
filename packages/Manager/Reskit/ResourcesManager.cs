///=====================================================
/// - FileName:      ResourcesManager.cs
/// - NameSpace:     YukiFrameWork.Res
/// - Created:       Yuki
/// - Email:         Yuki@qq.com
/// - Description:   ResourcesManager:这是Resources资源管理器
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================

using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.IO;

namespace YukiFrameWork.Res
{
    public class ResourcesManager
    {
        private static readonly Dictionary<string,ResourceRequest> resDict = new Dictionary<string,ResourceRequest>();
        private static readonly Dictionary<string, Object[]> resallDict = new Dictionary<string, Object[]>();     
        public static T Load<T>(string path) where T : Object
        {
            if (resDict.TryGetValue(path, out var item))
            {
                var obj = item.asset as T;
                if(obj == null)obj = (item.asset as GameObject).GetComponent<T>();
                return obj;
            }
            return Resources.Load<T>(path);
        }

        public static List<T> LoadAll<T>(string path) where T : Object
        {
            if (resallDict.TryGetValue(path, out var items))
            {
                var newObjs = new List<T>();
                foreach (var item in items)
                {
                    T obj = item as T;
                    if (obj == null) obj = (item as GameObject).GetComponent<T>();
                    newObjs.Add(obj);
                }
                Debug.Log("资源前置");
                return newObjs;
            }
            return new List<T>(Resources.LoadAll<T>(path));
        }

        public static ResourceRequest LoadAsync(string path)
        {
            var item = Resources.LoadAsync(path);
            if (!resDict.ContainsKey(path))
            {
                resDict.Add(path, item);    
            }
            return item;
        }

        public static async UniTask<List<T>> LoadAllAsync<T>(string path) where T : Object
        {
            List<T> newObjs = new List<T>();

            var items = Resources.LoadAll(path);
            foreach (var item in items)
            {
                await UniTask.Yield();
                T obj = item as T;
                if(obj == null)obj = (item as GameObject).GetComponent<T>();
                newObjs.Add(obj);
            }
            resallDict.Add(path, items);
            return newObjs;
        }

        public static async UniTaskVoid UnLoadResourcesAssets(System.Action CallBack = null)
        {
            await Resources.UnloadUnusedAssets();
            CallBack?.Invoke();
        }
    }
}