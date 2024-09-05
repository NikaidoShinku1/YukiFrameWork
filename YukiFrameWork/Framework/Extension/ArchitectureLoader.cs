///=====================================================
/// - FileName:      ArchitectureLoader.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/9/1 12:11:32
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using XFABManager;
using System.Collections.Generic;
using YukiFrameWork.Extension;
using System.IO;
using System.Collections;
namespace YukiFrameWork
{
    public enum ArchitectureTableLoadType
    {       
#if UNITY_EDITOR
        Editor,
#endif
        Resources,
        XFABManager
    }

    internal class TableInfo
    {
        public ArchitectureTableLoadType loadType;            
        public Type type;
        public UnityEngine.Object obj;
    }

    public sealed class ArchitectureTableConfig
    {
        private ArchitectureTable architectureTable;
        public ArchitectureTableConfig(ArchitectureTable architectureTable)
        {
            this.architectureTable = architectureTable;
            architectureTable.Init();
        }
        /// <summary>
        /// 获取文件配置，当配置为Json或Xml等Unity可识别TextAssets时，可使用该API获取对应的文本内容
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public string GetConfigByFile(string path)
        {
            if (architectureTable.Table.TryGetValue(path, out var value))
            {
                if (value == null || !value.obj)
                    throw new Exception($"路径{path}存在，但匹配的配表丢失，请重试! value is Null");

                if (value.obj is TextAsset textAsset)
                {
                    return textAsset.text;
                }

                throw new Exception("查找的资源不是TextAssets! path:" + path);
            }
            throw new Exception("丢失指定路径/名称的资源配置，无法取得对应Config Path:" + path);
        }

        /// <summary>
        /// 获取配置并自动转换类型，但要注意类型不正确导致异常的情况
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <returns></returns>
        public T GetConfig<T>(string path) where T : UnityEngine.Object
        {
            return GetConfig(path) as T;
        }

        internal bool CheckConfigByFile(string path, out UnityEngine.Object config)
        {
            config = GetConfig(path);
            return typeof(TextAsset).IsAssignableFrom(config.GetType());
        }

        /// <summary>
        /// 获取配置
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public UnityEngine.Object GetConfig(string path)
        {
            if (architectureTable.Table.TryGetValue(path, out var value))
            {
                if (value == null || !value.obj)
                    throw new Exception($"路径{path}存在，但匹配的配表丢失，请重试! value is Null");
                return value.obj;
            }
            throw new Exception("丢失指定路径/名称的资源配置，无法取得对应Config Path:" + path);
        }
    }

    [ClassAPI("架构配表收集")]
    public class ArchitectureTable
    {
        internal string projectName;
        /// <summary>
        /// 全局的加载方式，如果没有为配表信息指定加载方式，则默认使用该属性判断
        /// </summary>
        public ArchitectureTableLoadType LocalLoadType { get; set; } = ArchitectureTableLoadType.XFABManager;
        public Func<string, Type, UnityEngine.Object> ResLoader { get; set; }

        private Dictionary<string,TableInfo> infos = new Dictionary<string, TableInfo>();

        internal IDictionary<string, TableInfo> Table => infos;

        /// <summary>
        /// 添加配表的路径以及类型信息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pathOrName"></param>
        public void Add<T>(string pathOrName) where T : UnityEngine.Object
        {
            Add(pathOrName, typeof(T));
        }   

        /// <summary>
        ///  添加配表的路径以及类型信息
        /// </summary>
        /// <param name="pathOrName"></param>
        /// <param name="type"></param>
        public void Add(string pathOrName, Type type)
        {
            Add(pathOrName, type, LocalLoadType);
        }

        /// <summary>
        ///  添加配表的路径以及类型信息，还有指定加载方式
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pathOrName"></param>
        /// <param name="loadType"></param>
        public void Add<T>(string pathOrName,ArchitectureTableLoadType loadType) where T : UnityEngine.Object
        {
            Add(pathOrName, typeof(T),loadType);
        }

        /// <summary>
        /// 添加配表的路径以及类型信息，还有指定加载方式
        /// </summary>
        /// <param name="pathOrName"></param>
        /// <param name="type"></param>
        /// <param name="loadType"></param>
        /// <exception cref="Exception"></exception>
        public void Add(string pathOrName, Type type,ArchitectureTableLoadType loadType)
        {
            if (!typeof(UnityEngine.Object).IsAssignableFrom(type))
            {
                throw new Exception("Obj is Not UnityEngine.Object");
            }

            infos.Add(pathOrName, new TableInfo()
            {
                type = type,
                loadType = loadType
            });
        }


        internal void Init()
        {
            if (infos == null)
                throw new Exception("配表丢失");

            if (infos.Count == 0) return;
           
            foreach (var item in infos)
            {
                TableInfo info = item.Value;
                Type type = info.type;
                ArchitectureTableLoadType loadType = info.loadType;
                UnityEngine.Object current = null;
                if (this.ResLoader != null)
                    current = this.ResLoader.Invoke(item.Key, type);
                else
                {
                    switch (loadType)
                    {
#if UNITY_EDITOR
                        case ArchitectureTableLoadType.Editor:
                            current = UnityEditor.AssetDatabase.LoadAssetAtPath(item.Key, type);
                            break;
#endif
                        case ArchitectureTableLoadType.Resources:
                            current = UnityEngine.Resources.Load(item.Key, type);
                            break;
                        case ArchitectureTableLoadType.XFABManager:
                            current = AssetBundleManager.LoadAsset(projectName, item.Key, type);
                            break;
                    }
                }
                info.obj = current;
            }
        }
    }
}
