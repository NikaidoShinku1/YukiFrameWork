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
using System.IO;
using System.Linq;
using System.Collections;

namespace YukiFrameWork.Res
{
    public class ResourcesManager
    {
        private readonly Dictionary<string,ResourceRequest> resDict = new Dictionary<string,ResourceRequest>();
        private readonly Dictionary<string, Object[]> resallDict = new Dictionary<string, Object[]>();     
        public  T Load<T>(string path) where T : Object
        {
            if (resDict.TryGetValue(path, out var item))
            {
                var obj = item.asset as T;
                if(obj == null)obj = (item.asset as GameObject).GetComponent<T>();
                return obj;
            }
            var newItem = Resources.Load<T>(path);
            if (newItem == null)
            {
                Debug.LogError($"路径错误请重试，当前你输入的路径是{path}");
                return null;
            }
            return Resources.Load<T>(path);
        }

        public List<T> LoadAll<T>(string path) where T : Object
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
                return newObjs;
            }
            var newItems = Resources.LoadAll<T>(path);
            if (newItems == null)
            {
                Debug.LogError($"路径错误请重试，当前你输入的路径是{path}");
                return null;
            }
            return new List<T>(newItems);
        }

        public ResourceRequest LoadAsync(string path)
        {
            var item = Resources.LoadAsync(path);
            if (item == null)
            {
                Debug.LogError($"路径错误请重试，当前你输入的路径是{path}");
                return null;
            }
            if (!resDict.ContainsKey(path))
            {
                resDict.Add(path, item);    
            }
            return item;
        }

        public IEnumerator LoadAllAsync<T>(string path) where T : Object
        {
            List<T> newObjs = new List<T>();

            var items = Resources.LoadAll<T>(path);
            if (items == null)
            {
                Debug.LogError($"路径错误请重试，当前你输入的路径是{path}");
                yield break;
            }
            foreach (var item in items)
            {
                yield return null;
                T obj = item as T;
                if(obj == null)obj = (item as GameObject).GetComponent<T>();             
                newObjs.Add(obj);
            }
            resallDict.Add(path, items);            
        }

        public void UnLoadResourcesAssets()
        {
            resallDict.Clear();
            resDict.Clear();
        }

       
    }
}