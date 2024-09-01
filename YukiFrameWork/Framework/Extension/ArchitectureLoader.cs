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
namespace YukiFrameWork
{
    public enum ArchitecTableLoadType
    {       
#if UNITY_EDITOR
        Editor,
#endif
        Resources,
        XFABManager
    }

    public sealed class ArchitectureTableConfig
    {
        private ArchitectureTable architectureTable;
        public ArchitectureTableConfig(ArchitectureTable architectureTable)
        {
            this.architectureTable = architectureTable;
            architectureTable.Init();
        }
        public IDictionary<string, UnityEngine.Object> Configs => architectureTable.Table;

        internal IDictionary<string, Type> Datas => architectureTable.Table_Data;

        /// <summary>
        /// 获取文件配置，当配置为Json或Xml等Unity可识别TextAssets时，可使用该API获取对应的文本内容
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public string GetConfigByFile(string path)
        {
            if (architectureTable.Table.TryGetValue(path, out var value))
            {
                if (!value)
                    throw new Exception($"路径{path}存在，但匹配的配表丢失，请重试! value is Null");

                if (value is TextAsset textAsset)
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
                if (!value)
                    throw new Exception($"路径{path}存在，但匹配的配表丢失，请重试! value is Null");
                return value;
            }
            throw new Exception("丢失指定路径/名称的资源配置，无法取得对应Config Path:" + path);
        }
    }

    [ClassAPI("架构配表收集")]
    public class ArchitectureTable
    {
        private ArchitecTableLoadType loadType;
        private event Func<string, Type, UnityEngine.Object> ResLoader;
        /// <summary>
        /// 配表收集
        /// <para>Key:收集的资源名称/路径</para>
        /// <para>Value:收集的资源类型</para>
        /// </summary>
        private Dictionary<string, Type> table;

        private Dictionary<string, UnityEngine.Object> tableRes_Dicts = new Dictionary<string, UnityEngine.Object>();

        internal string projectName;

        internal IDictionary<string, UnityEngine.Object> Table => tableRes_Dicts;

        internal IDictionary<string, Type> Table_Data => table;

        public ArchitectureTable(Dictionary<string, Type> table, ArchitecTableLoadType tableLoadType = ArchitecTableLoadType.XFABManager)
        {
            this.table = table;
            this.loadType = tableLoadType;
        }
        public ArchitectureTable(Dictionary<string, Type> table, Func<string, Type, UnityEngine.Object> ResLoader)
        {
            this.table = table;
            this.ResLoader = ResLoader;
        }


        internal void Init()
        {
            if (table == null)
                throw new Exception("配表丢失");
           
            foreach (var item in table)
            {
                Type type = item.Value;
                UnityEngine.Object current = null;
                if (this.ResLoader != null)
                    current = this.ResLoader.Invoke(item.Key, type);
                else
                {
                    switch (loadType)
                    {
#if UNITY_EDITOR
                        case ArchitecTableLoadType.Editor:
                            current = UnityEditor.AssetDatabase.LoadAssetAtPath(item.Key, type);
                            break;
#endif
                        case ArchitecTableLoadType.Resources:
                            current = UnityEngine.Resources.Load(item.Key, type);
                            break;
                        case ArchitecTableLoadType.XFABManager:
                            current = AssetBundleManager.LoadAsset(projectName, item.Key, type);
                            break;
                    }
                }
                tableRes_Dicts.Add(item.Key, current);
            }
        }
    }
}
