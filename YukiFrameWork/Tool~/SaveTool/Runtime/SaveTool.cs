///=====================================================
/// - FileName:      SaveSystem.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   框架存档系统
/// - Creation Time: 2024/2/19 18:42:35
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using YukiFrameWork.Extension;
using System.Linq;
using System.Text;

#if UNITY_EDITOR
using UnityEditor;
#endif
namespace YukiFrameWork
{
  
    public static class SaveTool
    {
#if UNITY_EDITOR
        [MenuItem("YukiFrameWork/SaveTool/Application.persistentDataPath")]
        static void OpenPersistenDataPath()
        {
            System.Diagnostics.Process.Start("explorer.exe", Application.persistentDataPath.Replace("/", "\\"));
        }

        [MenuItem("YukiFrameWork/SaveTool/Application.dataPath")]
        static void OpenDataPath()
        {
            System.Diagnostics.Process.Start("explorer.exe", Application.dataPath.Replace("/", "\\"));
        }

        [MenuItem("YukiFrameWork/SaveTool/Application.streamingAssetsPath")]
        static void OpenStreamingAssetsPath()
        {
            System.Diagnostics.Process.Start("explorer.exe", Application.streamingAssetsPath.Replace("/", "\\"));
        }

        [MenuItem("YukiFrameWork/SaveTool/Application.temporaryCachePath")]
        static void OpenTemporaryCachePath()
        {
            System.Diagnostics.Process.Start("explorer.exe", Application.temporaryCachePath.Replace("/", "\\"));
        }
#endif
        private static string saveFolderName;
        private static FolderType folderType;
        private static string saveCustomDirName;
        private static Dictionary<int, Dictionary<string, object>> cacheDict = new Dictionary<int, Dictionary<string, object>>();

        private static bool isInited = false;
        private static Dictionary<int, SaveInfo> runtime_saveInfos = new Dictionary<int, SaveInfo>();
        private const string SAVEINFO_ALL_CACHE_INDEXKEY = nameof(SAVEINFO_ALL_CACHE_INDEXKEY);
        public static string saveDirPath
        {
            get
            {
                string fileName = @"/" + saveFolderName;
                switch (folderType)
                {
                    case FolderType.persistentDataPath:
                        return Application.persistentDataPath + fileName;
                    case FolderType.dataPath:
                        return Application.dataPath + fileName;
                    case FolderType.streamingAssetsPath:
                        return Application.streamingAssetsPath + fileName;
                    case FolderType.temporaryCachePath:
                        return Application.temporaryCachePath + fileName;
                    case FolderType.custom:
                        return saveCustomDirName + fileName;
                }

                return default;
            }
        }
        /// <summary>
        /// 当前存档的信息数量
        /// </summary>
        public static int CurrentSaveCount => runtime_saveInfos.Count;
        /// <summary>
        /// 初始化SaveTool。
        /// </summary>
        /// <param name="saveToolConfig"></param>       
        public static void Init(FolderType folderType = FolderType.persistentDataPath, string saveFolderName = "SaveData")
        {
            if (isInited) return;
            isInited = true;
            SaveTool.saveFolderName = saveFolderName;
            SaveTool.folderType = folderType;
            LoadPrefs();
            CheckAndCreateFolder();
        }

        /// <summary>
        /// 自定义文件夹路径初始化 文件夹路径不需要跟"/"符号，框架自动补全
        /// </summary>
        /// <param name="saveCustomDirName"></param>
        /// <param name="saveFolderName"></param>
        /// <param name=""></param>
        public static void Init(string saveCustomDirName, string saveFolderName = "SaveData")
        {
            if (isInited) return;
            isInited = true;
            SaveTool.saveFolderName = saveFolderName;
            SaveTool.folderType = FolderType.custom;
            SaveTool.saveCustomDirName = saveCustomDirName;
            LoadPrefs();
            CheckAndCreateFolder();
        }

        

        private static void SavePrefs()
        {            
            string json = SerializationTool.SerializedObject(runtime_saveInfos.Values);
            json.CreateFileStream(saveDirPath,"SaveInfos",".Json");
            //PlayerPrefs.SetString(SAVEINFO_ALL_CACHE_INDEXKEY, runtime_saveInfos.Count == 0 ? string.Empty : SerializationTool.SerializedObject( runtime_saveInfos.Values));
            //PlayerPrefs.Save();
        }

        private static void LoadPrefs()
        {
            string path = saveDirPath + "/" + "SaveInfos.Json";
            List<SaveInfo> saveInfos = null;
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                if (json.IsNullOrEmpty()) return;
                saveInfos = SerializationTool.DeserializedObject<List<SaveInfo>>(json);
            }
            else saveInfos = new List<SaveInfo>();
            runtime_saveInfos = saveInfos.ToDictionary(x => x.saveID, x => x);
        }

        /// <summary>
        /// 获得所有的存档
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="order"></param>
        /// <param name="isDescending"></param>
        /// <returns></returns>
        public static List<SaveInfo> GetAllSaveInfos<T>(Func<SaveInfo,T> order,bool isDescending = false)
        {
            var items = runtime_saveInfos.Select(x => x.Value);
            return isDescending ? items.OrderByDescending(order).ToList() : items.OrderBy(order).ToList();
        }

        /// <summary>
        /// 根据时间来获取所有存档，更新时间越新则排序在越前面
        /// </summary>
        /// <returns>返回所有的存档信息</returns>
        public static List<SaveInfo> GetAllSaveInfoByUpdateTime()
        {
            List<SaveInfo> SaveInfos = new List<SaveInfo>(runtime_saveInfos.Count);

            foreach (var item in runtime_saveInfos)
            {
                SaveInfos.Add(item.Value);
            }           
            OrderByUpdateTimeComparer orderBy = new OrderByUpdateTimeComparer();
            SaveInfos.Sort(orderBy);
            return SaveInfos;
        }

        private class OrderByUpdateTimeComparer : IComparer<SaveInfo>
        {
            public int Compare(SaveInfo x, SaveInfo y)
            {
                if (x.LastDateTime > y.LastDateTime)
                {
                    return -1;
                }
                else
                {
                    return 1;
                }
            }
        }           
        /// <summary>
        /// 创建一个存档信息,需要为存档指定一个唯一的存档Id!
        /// </summary>
        /// <returns></returns>
        public static SaveInfo CreateSaveInfo(int saveId)
        {
            SaveInfo SaveInfo = new SaveInfo(saveId,DateTime.Now);
            runtime_saveInfos.Add(saveId, SaveInfo);
            SavePrefs();
            return SaveInfo;
        }
        [Obsolete("过时的创建存档信息方法，请调用CreateSaveInfo(int saveId)指定存档的下标")]
        public static SaveInfo CreateSaveInfo()
        {
            throw new Exception("创建存档信息的方法应使用CreateSaveInfo(int saveId)方法!该方法已不再受支持");
        }

        /// <summary>
        /// 通过id获得一个存档信息类
        /// </summary>
        /// <param name="saveID"></param>
        /// <returns></returns>
        public static SaveInfo GetSaveInfo(int saveID)
        {
            runtime_saveInfos.TryGetValue(saveID, out var info);
            return info;
        }

        /// <summary>
        /// 获得相同id的存档信息类
        /// </summary>
        /// <param name="SaveInfo">存档信息</param>
        /// <returns></returns>
        public static SaveInfo GetSaveInfo(SaveInfo SaveInfo)
        {
            return GetSaveInfo(SaveInfo.saveID);
        }

        /// <summary>
        /// 删除指定id的存档信息
        /// </summary>
        /// <param name="saveID">保存id</param>
        public static void DeleteSaveInfo(int saveID)
        {
            string path = GetSavePath(saveID, false);
            if (!string.IsNullOrEmpty(path))
            {
                Directory.Delete(path,true);
#if UNITY_EDITOR
                AssetDatabase.Refresh();
#endif
            }
            runtime_saveInfos.Remove(saveID);
            SavePrefs();
        }     

        /// <summary>
        /// 删除所有的存档信息
        /// </summary>
        public static void DeleteAllSaveInfo()
        {
            if (Directory.Exists(saveDirPath))
            {
                Directory.Delete(saveDirPath, true);
#if UNITY_EDITOR
                AssetDatabase.Refresh();
#endif
            }
            runtime_saveInfos.Clear();
            SavePrefs();
            CheckAndCreateFolder();
        }

        /// <summary>
        /// 删除包括缓存对象在内所有的信息
        /// </summary>
        public static void DeleteAll()
        {
            cacheDict.Clear();
            DeleteAllSaveInfo();
        }

        /// <summary>
        /// 删除某一个缓存的对象
        /// </summary>
        /// <param name="saveID">保存的id</param>
        /// <param name="fileName">包括后缀在内的文件名称</param>
        public static void RemoveCache(int saveID, string fileName)
        {
            cacheDict[saveID].Remove(fileName);
        }

        /// <summary>
        /// 删除指定保存id下所有缓存的对象
        /// </summary>
        /// <param name="saveID">保存的id</param>
        public static void RemoveCache(int saveID)
        {
            cacheDict.Remove(saveID);
        }

        #region Saving....
        /// <summary>
        /// 将对象保存为Json文件(文件名称默认为类名称)
        /// </summary>
        /// <param name="saveObject">对象</param>
        /// <param name="saveID">保存的id</param>
        public static void SaveObjectToJson(object saveObject, int saveID = 0)
            => SaveObjectToJson(saveObject, saveObject.GetType().Name, saveID);

        /// <summary>
        /// 将对象保存为Json文件(文件名称默认为类名称)
        /// </summary>
        /// <param name="saveObject">对象</param>
        /// <param name="info">存档信息</param>
        public static void SaveObjectToJson(object saveObject, SaveInfo info)
            => SaveObjectToJson(saveObject, saveObject.GetType().Name, info.saveID);

        /// <summary>
        /// 将对象保存为Json文件
        /// </summary>
        /// <param name="saveObject">对象</param>
        /// <param name="fileName">文件名称</param>
        /// <param name="saveID">保存的id</param>
        public static void SaveObjectToJson(object saveObject, string fileName,int saveID = 0)
        {
            CheckAndCreateFolder();
            SaveExecute(saveObject, fileName, saveID, SerializationType.Json);
        }

        /// <summary>
        /// 将对象保存为Json文件
        /// </summary>
        /// <param name="saveObject">对象</param>
        /// <param name="fileName">文件名称</param>
        /// <param name="info">存档信息</param>
        public static void SaveObjectToJson(object saveObject, string fileName, SaveInfo info)
            => SaveObjectToJson(saveObject, fileName, info.saveID);

        /// <summary>
        /// 将对象保存为Xml文件(文件名称默认为类名称)
        /// </summary>
        /// <param name="saveObject">对象</param>
        /// <param name="saveID">保存的id</param>
        public static void SaveObjectToXml(object saveObject, int saveID = 0)
            => SaveObjectToXml(saveObject, saveObject.GetType().Name, saveID);

        /// <summary>
        /// 将对象保存为Xml文件(文件名称默认为类名称)
        /// </summary>
        /// <param name="saveObject">对象</param>
        /// <param name="info">存档信息</param>
        public static void SaveObjectToXml(object saveObject, SaveInfo info)
            => SaveObjectToXml(saveObject, saveObject.GetType().Name, info.saveID);

        /// <summary>
        /// 将对象保存为Xml文件
        /// </summary>
        /// <param name="saveObject">对象</param>
        /// <param name="fileName">文件名称</param>
        /// <param name="info">存档信息</param>
        public static void SaveObjectToXml(object saveObject, string fileName, SaveInfo info)
            => SaveObjectToXml(saveObject, fileName, info.saveID);

        /// <summary>
        /// 将对象保存为Xml文件
        /// </summary>
        /// <param name="saveObject">对象</param>
        /// <param name="fileName">文件名称</param>
        /// <param name="saveID">保存的id</param>
        public static void SaveObjectToXml(object saveObject, string fileName,int saveID = 0)
        {
            CheckAndCreateFolder();
            SaveExecute(saveObject, fileName, saveID, SerializationType.Xml);
        }
        private static void SaveExecute(object saveObject, string fileName, int saveID, SerializationType serialization)
        {
            string info = string.Empty;
            switch (serialization)
            {
                case SerializationType.Json:
                    info = SerializationTool.SerializedObject(saveObject);
                    break;
                case SerializationType.Xml:
                    info = SerializationTool.XmlSerializedObject(saveObject);
                    break;           
            }

            string folder = GetSavePath(saveID);     

            string targetPath = $@"{folder}/{GetSaveName(serialization,fileName)}";

            Save_File(info, targetPath);

            GetSaveInfo(saveID).Update_LastTime(DateTime.Now);

            SavePrefs();

            SetCache(saveID, fileName, saveObject);
        }
        #endregion

        private static void Save_File(string info,string path)
        {
            bool IsExist = File.Exists(path);

            if (IsExist)
                File.WriteAllText(path, string.Empty);

            FileMode fileMode = IsExist ? FileMode.Open : FileMode.Create;

            using (FileStream stream = new FileStream(path, fileMode, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                StreamWriter streamWriter = new StreamWriter(stream);
                streamWriter.Write(info);
                streamWriter.Close();
                stream.Close();
            }
#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif
        }

        private static void SetCache(int saveID,string fileName,object saveObject)
        {
            if (cacheDict.ContainsKey(saveID))
            {
                if (cacheDict[saveID].ContainsKey(fileName))
                    cacheDict[saveID][fileName] = saveObject;
                else
                    cacheDict[saveID].Add(fileName, saveObject);
            }
            else
                cacheDict.Add(saveID, new Dictionary<string, object> { { fileName, saveObject } });
        }

        private static T GetCache<T>(int saveID,string fileName) where T : class
        {
            if (cacheDict.ContainsKey(saveID))
            {
                cacheDict[saveID].TryGetValue(fileName, out var obj);
                return obj as T;
            }
            return null;
        }

        private static string GetSuffix(SerializationType type)
        {
            return type switch
            {
                SerializationType.Json => ".json",
                SerializationType.Xml => ".xml",              
                _ => string.Empty
            };
        }

        private static string GetSaveName(SerializationType type, string fileName)
        {
            return $"{fileName}{GetSuffix(type)}";
        }

        private static string GetSavePath(int saveID, bool autoGeneric = true)
        {
            if (GetSaveInfo(saveID) == null)
                throw new Exception($"YukiFrameWork---当前存档没有被创建，存档id为{saveID}");
            string folder = $@"{saveDirPath}/{saveID}";

            if (Directory.Exists(folder))
                return folder;
            else if (autoGeneric)
            {
                Directory.CreateDirectory(folder);
#if UNITY_EDITOR
                AssetDatabase.Refresh();
#endif
                return folder;
            }
            return string.Empty;
        }

        #region Loading....

        /// <summary>
        /// 通过Json文件读取存档
        /// </summary>
        /// <typeparam name="T">返回的类型</typeparam>
        /// <param name="fileName">文件名称</param>
        /// <param name="saveID">保存的id</param>
        /// <returns>返回一个类型为T的对象</returns>
        public static T LoadObjectFromJson<T>(string fileName,int saveID = 0) where T : class
        {
            return Loading<T>(fileName, saveID, SerializationType.Json);
        }

        /// <summary>
        /// 通过Json文件读取存档
        /// </summary>
        /// <typeparam name="T">返回的类型</typeparam>
        /// <param name="fileName">文件名称</param>
        /// <param name="info">存档信息</param>
        /// <returns>返回一个类型为T的对象</returns>
        public static T LoadObjectFromJson<T>(string fileName, SaveInfo info) where T : class
        {
            return LoadObjectFromJson<T>(fileName, info.saveID);
        }

        /// <summary>
        /// 通过Json文件读取存档
        /// </summary>
        /// <typeparam name="T">返回的类型</typeparam>
        /// <param name="saveID">保存的id</param>
        /// <returns>返回一个类型为T的对象</returns>
        public static T LoadObjectFromJson<T>(int saveID = 0) where T : class
        {
            return LoadObjectFromJson<T>(typeof(T).Name,saveID);
        }

        /// <summary>
        /// 通过Json文件读取存档
        /// </summary>
        /// <typeparam name="T">返回的类型</typeparam>     
        /// <param name="info">存档信息</param>
        /// <returns>返回一个类型为T的对象</returns>
        public static T LoadObjectFromJson<T>(SaveInfo info) where T : class
        {
            return LoadObjectFromJson<T>(typeof(T).Name, info.saveID);
        }

        /// <summary>
        /// 通过Xml文件读取存档
        /// </summary>
        /// <typeparam name="T">返回的类型</typeparam>
        /// <param name="fileName">文件名称</param>
        /// <param name="saveID">保存的id</param>
        /// <returns>返回一个类型为T的对象</returns>
        public static T LoadObjectFromXml<T>(string fileName, int saveID = 0) where T : class
        {
            return Loading<T>(fileName, saveID, SerializationType.Xml);
        }
        /// <summary>
        /// 通过Xml文件读取存档
        /// </summary>
        /// <typeparam name="T">返回的类型</typeparam>
        /// <param name="fileName">文件名称</param>
        /// <param name="info">存档信息</param>
        /// <returns>返回一个类型为T的对象</returns>
        public static T LoadObjectFromXml<T>(string fileName, SaveInfo info) where T : class
        {
            return LoadObjectFromXml<T>(fileName, info.saveID);
        }

        /// <summary>
        /// 通过Xml文件读取存档
        /// </summary>
        /// <typeparam name="T">返回的类型</typeparam>
        /// <param name="saveID">保存的id</param>
        /// <returns>返回一个类型为T的对象</returns>
        public static T LoadObjectFromXml<T>(int saveID = 0) where T : class
        {
            return LoadObjectFromXml<T>(typeof(T).Name,saveID);
        }

        /// <summary>
        /// 通过Xml文件读取存档
        /// </summary>
        /// <typeparam name="T">返回的类型</typeparam>     
        /// <param name="info">存档信息</param>
        /// <returns>返回一个类型为T的对象</returns>
        public static T LoadObjectFromXml<T>(SaveInfo info) where T : class
        {
            return LoadObjectFromXml<T>(typeof(T).Name, info.saveID);
        }

        private static T Loading<T>(string fileName,int saveID,SerializationType type) where T : class
        {
            string name = GetSaveName(type, fileName);
            T obj = GetCache<T>(saveID, name);

            if (obj == null)
            {
                string folder = GetSavePath(saveID, false);
                if (string.IsNullOrEmpty(folder))
                {
                    throw new Exception("无法读取存档数据，请检查文件夹是否正确或者存在存档信息但确认是否已经保存了存档数据!");
                }
                string targetPath = $@"{folder}/{name}";
                if (!File.Exists(targetPath))
                {
                    throw new Exception("该文件夹下存档不存在!路径:" + targetPath);
                }
                string info = File.ReadAllText(targetPath);
                switch (type)
                {
                    case SerializationType.Json:
                        obj = SerializationTool.DeserializedObject<T>(info);
                        break;
                    case SerializationType.Xml:
                        obj = SerializationTool.XmlDeserializedObject<T>(info);
                        break;                 
                }
            }

            return obj;
        }
        #endregion

        private static void CheckAndCreateFolder()
        {          
            if (!Directory.Exists(saveDirPath))
            {
                Directory.CreateDirectory(saveDirPath);              
            }           

#if UNITY_EDITOR
            AssetDatabase.Refresh(); 
#endif
        }

    }
}
