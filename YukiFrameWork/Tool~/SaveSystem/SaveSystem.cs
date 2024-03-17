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
using Newtonsoft.Json;
using System.Linq;
using System.Text;

#if UNITY_EDITOR
using UnityEditor;
#endif
namespace YukiFrameWork
{
    [Serializable]
    public class SaveInfo
    {
        public int saveID;

        [NonSerialized]
        private DateTime lastDateTime;

        [JsonIgnore]
        public DateTime LastDateTime
        {
            get
            {
                if (lastDateTime == default(DateTime))
                {
                    DateTime.TryParse(lastDateTimeString, out lastDateTime);
                }
                return lastDateTime;
            }
        }

        [SerializeField]
        [JsonProperty]
        private string lastDateTimeString;
        public SaveInfo(int saveID, DateTime dateTime)
        {
            this.saveID = saveID;
            this.lastDateTime = dateTime;
            lastDateTimeString = lastDateTime.ToString();
        }

        public void Update_LastTime(DateTime dateTime)
        {
            lastDateTime = dateTime;
            lastDateTimeString = lastDateTime.ToString();
        }
    }
    public class SaveSystem : AbstractSystem
    {      
        [Serializable]
        public class SaveSystemData
        {
            public int currentID;
            public List<SaveInfo> infos = new List<SaveInfo>();         
        }

        private static SaveSystemData systemData;

        private const string SAVE_FOLDERNAME = "SaveData";
     
        private string saveDirPath;
        private string systemFilePath;       
        private static bool isInited = false;
        private const string SAVE_PATH = "FrameWorkSavePath";
        private static Dictionary<int, Dictionary<string, object>> cacheDict = new Dictionary<int, Dictionary<string, object>>();

        public override void Init()
        {
            if (isInited) return;
            string persistentDataPath = PlayerPrefs.GetString(SAVE_PATH);
            if (string.IsNullOrEmpty(persistentDataPath))
            {
                persistentDataPath = Application.persistentDataPath;
                PlayerPrefs.SetString(SAVE_PATH, persistentDataPath);
            }
            saveDirPath = persistentDataPath + @"/" + SAVE_FOLDERNAME;
            systemFilePath = saveDirPath + @"/" + "LocalSystemData" + GetSuffix(SerializationType.Json);
            Debug.Log($"存档系统初始化完成,存档保存的路径----：{saveDirPath}");
            CheckAndCreateFolder();
            InitSystemDefaultData();
            isInited = true;
        }

        /// <summary>
        /// 设置新的保存地址
        /// </summary>
        /// <param name="newFolderPath">新的文件夹地址</param>
        public bool SetNewSaveFolder(string newFolderPath)
        {
            if (newFolderPath.EndsWith("/") || newFolderPath.EndsWith(@"\"))
            {
                Debug.LogWarning(@"存档路径变更失败!需要输入的是文件夹路径，最后一个文件夹不需要带上/或者\!");
                return false;
            }
            PlayerPrefs.SetString(SAVE_PATH, newFolderPath);
            saveDirPath = newFolderPath + @"/" + SAVE_FOLDERNAME;
            systemFilePath = saveDirPath + @"/" + "LocalSystemData" + GetSuffix(SerializationType.Json);
            CheckAndCreateFolder();
            InitSystemDefaultData();
            Debug.Log($"存档路径变更，重新初始化存档系统!存档保存的路径----：{saveDirPath}");
            return true;
        }

        /// <summary>
        /// 获得所有的存档
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="order"></param>
        /// <param name="isDescending"></param>
        /// <returns></returns>
        public List<SaveInfo> GetAllSaveInfos<T>(Func<SaveInfo,T> order,bool isDescending = false)
        {
            return isDescending ? systemData.infos.OrderByDescending(order).ToList() : systemData.infos.OrderBy(order).ToList();
        }

        /// <summary>
        /// 根据时间来获取所有存档，更新时间越新则排序在越前面
        /// </summary>
        /// <returns>返回所有的存档信息</returns>
        public List<SaveInfo> GetAllSaveInfoByUpdateTime()
        {
            List<SaveInfo> SaveInfos = new List<SaveInfo>(systemData.infos.Count);
            for (int i = 0; i < systemData.infos.Count; i++)
            {
                SaveInfos.Add(systemData.infos[i]);
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
        private void InitSystemDefaultData()
        {
            systemData = LoadSystemData();            
        }
       
        private void Update_SystemData()
        {
            string info = AssemblyHelper.SerializedObject(systemData);

            Save_File(info, systemFilePath);
        }

        /// <summary>
        /// 创建一个存档信息
        /// </summary>
        /// <returns></returns>
        public SaveInfo CreateSaveInfo()
        {
            SaveInfo SaveInfo = new SaveInfo(systemData.currentID,DateTime.Now);
            systemData.infos.Add(SaveInfo);
            systemData.currentID++;
            Update_SystemData();
            return SaveInfo;
        }

        /// <summary>
        /// 通过id获得一个存档信息类
        /// </summary>
        /// <param name="saveID"></param>
        /// <returns></returns>
        public SaveInfo GetSaveInfo(int saveID)
        {
            return systemData.infos.Find(x => x.saveID == saveID);
        }

        /// <summary>
        /// 获得相同id的存档信息类
        /// </summary>
        /// <param name="SaveInfo">存档信息</param>
        /// <returns></returns>
        public SaveInfo GetSaveInfo(SaveInfo SaveInfo)
        {
            return GetSaveInfo(SaveInfo.saveID);
        }

        /// <summary>
        /// 删除指定id的存档信息
        /// </summary>
        /// <param name="saveID">保存id</param>
        public void DeleteSaveInfo(int saveID)
        {
            string path = GetSavePath(saveID, false);
            if (!string.IsNullOrEmpty(path))
            {
                Directory.Delete(path);
#if UNITY_EDITOR
                AssetDatabase.Refresh();
#endif
            }

            systemData.infos.Remove(GetSaveInfo(saveID));
            Update_SystemData();
        }

        private SaveSystemData LoadSystemData()
        {
            CheckAndCreateFolder();         
            SaveSystemData systemData = null;
            if (File.Exists(systemFilePath))
            {
                systemData = AssemblyHelper.DeserializedObject<SaveSystemData>(File.ReadAllText(systemFilePath));               
            }
            if (systemData == null)
            {
                systemData = new SaveSystemData();
                Update_SystemData();
            }
            return systemData;
        }

        /// <summary>
        /// 删除所有的存档信息
        /// </summary>
        public void DeleteAllSaveInfo()
        {
            if (Directory.Exists(saveDirPath))
            {
                Directory.Delete(saveDirPath, true);
#if UNITY_EDITOR
                AssetDatabase.Refresh();
#endif
            }
            InitSystemDefaultData();
            CheckAndCreateFolder();
        }

        /// <summary>
        /// 删除包括缓存对象在内所有的信息
        /// </summary>
        public void DeleteAll()
        {
            cacheDict.Clear();
            DeleteAllSaveInfo();
        }

        /// <summary>
        /// 删除某一个缓存的对象
        /// </summary>
        /// <param name="saveID">保存的id</param>
        /// <param name="fileName">包括后缀在内的文件名称</param>
        public void RemoveCache(int saveID, string fileName)
        {
            cacheDict[saveID].Remove(fileName);
        }

        /// <summary>
        /// 删除指定保存id下所有缓存的对象
        /// </summary>
        /// <param name="saveID">保存的id</param>
        public void RemoveCache(int saveID)
        {
            cacheDict.Remove(saveID);
        }

        #region Saving....
        /// <summary>
        /// 将对象保存为Json文件(文件名称默认为类名称)
        /// </summary>
        /// <param name="saveObject">对象</param>
        /// <param name="saveID">保存的id</param>
        public void SaveObjectToJson(object saveObject, int saveID = 0)
            => SaveObjectToJson(saveObject, saveObject.GetType().Name, saveID);

        /// <summary>
        /// 将对象保存为Json文件(文件名称默认为类名称)
        /// </summary>
        /// <param name="saveObject">对象</param>
        /// <param name="info">存档信息</param>
        public void SaveObjectToJson(object saveObject, SaveInfo info)
            => SaveObjectToJson(saveObject, saveObject.GetType().Name, info.saveID);

        /// <summary>
        /// 将对象保存为Json文件
        /// </summary>
        /// <param name="saveObject">对象</param>
        /// <param name="fileName">文件名称</param>
        /// <param name="saveID">保存的id</param>
        public void SaveObjectToJson(object saveObject, string fileName,int saveID = 0)
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
        public void SaveObjectToJson(object saveObject, string fileName, SaveInfo info)
            => SaveObjectToJson(saveObject, fileName, info.saveID);

        /// <summary>
        /// 将对象保存为Xml文件(文件名称默认为类名称)
        /// </summary>
        /// <param name="saveObject">对象</param>
        /// <param name="saveID">保存的id</param>
        public void SaveObjectToXml(object saveObject, int saveID = 0)
            => SaveObjectToXml(saveObject, saveObject.GetType().Name, saveID);

        /// <summary>
        /// 将对象保存为Xml文件(文件名称默认为类名称)
        /// </summary>
        /// <param name="saveObject">对象</param>
        /// <param name="info">存档信息</param>
        public void SaveObjectToXml(object saveObject, SaveInfo info)
            => SaveObjectToXml(saveObject, saveObject.GetType().Name, info.saveID);

        /// <summary>
        /// 将对象保存为Xml文件
        /// </summary>
        /// <param name="saveObject">对象</param>
        /// <param name="fileName">文件名称</param>
        /// <param name="info">存档信息</param>
        public void SaveObjectToXml(object saveObject, string fileName, SaveInfo info)
            => SaveObjectToXml(saveObject, fileName, info.saveID);

        /// <summary>
        /// 将对象保存为Xml文件
        /// </summary>
        /// <param name="saveObject">对象</param>
        /// <param name="fileName">文件名称</param>
        /// <param name="saveID">保存的id</param>
        public void SaveObjectToXml(object saveObject, string fileName,int saveID = 0)
        {
            CheckAndCreateFolder();
            SaveExecute(saveObject, fileName, saveID, SerializationType.Xml);
        }
        /// <summary>
        /// 将对象保存为Bytes文件(文件名称默认为类名称)
        /// </summary>
        /// <param name="saveObject">对象</param>
        /// <param name="saveID">保存的id</param>
        public void SaveObjectToBytes(object saveObject, int saveID = 0)
            => SaveObjectToBytes(saveObject, saveObject.GetType().Name, saveID);

        /// <summary>
        /// 将对象保存为Bytes文件(文件名称默认为类名称)
        /// </summary>
        /// <param name="saveObject">对象</param>
        /// <param name="info">存档信息</param>
        public void SaveObjectToBytes(object saveObject, SaveInfo info)
            => SaveObjectToBytes(saveObject, saveObject.GetType().Name, info.saveID);

        /// <summary>
        /// 将对象保存为Bytes文件
        /// </summary>
        /// <param name="saveObject">对象</param>
        /// <param name="fileName">文件名称</param>
        /// <param name="info">存档信息</param>
        public void SaveObjectToBytes(object saveObject, string fileName, SaveInfo info)
           => SaveObjectToBytes(saveObject, fileName, info.saveID);

        /// <summary>
        /// 将对象保存为Bytes文件
        /// </summary>
        /// <param name="saveObject">对象</param>
        /// <param name="fileName">文件名称</param>
        /// <param name="saveID">保存的id</param>
        public void SaveObjectToBytes(object saveObject, string fileName, int saveID = 0)
        {
            CheckAndCreateFolder();
            SaveExecute(saveObject, fileName, saveID, SerializationType.Bytes);
        }

        private void SaveExecute(object saveObject, string fileName, int saveID, SerializationType serialization)
        {
            string info = string.Empty;
            switch (serialization)
            {
                case SerializationType.Json:
                    info = AssemblyHelper.SerializedObject(saveObject);
                    break;
                case SerializationType.Xml:
                    info = AssemblyHelper.XmlSerializedObject(saveObject);
                    break;
                case SerializationType.Bytes:
                    info = Encoding.UTF8.GetString(AssemblyHelper.ByteSerializedObject(saveObject));
                    break;              
            }

            string folder = GetSavePath(saveID);     

            string targetPath = $@"{folder}/{GetSaveName(serialization,fileName)}";

            Save_File(info, targetPath);

            GetSaveInfo(saveID).Update_LastTime(DateTime.Now);

            Update_SystemData();

            SetCache(saveID, fileName, saveObject);
        }
        #endregion

        private void Save_File(string info,string path)
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

        private void SetCache(int saveID,string fileName,object saveObject)
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

        private T GetCache<T>(int saveID,string fileName) where T : class
        {
            if (cacheDict.ContainsKey(saveID))
            {
                cacheDict[saveID].TryGetValue(fileName, out var obj);
                return obj as T;
            }
            return null;
        }

        private string GetSuffix(SerializationType type)
        {
            return type switch
            {
                SerializationType.Json => ".json",
                SerializationType.Xml => ".xml",
                SerializationType.Bytes => ".bytes",
                _ => string.Empty
            };
        }

        private string GetSaveName(SerializationType type, string fileName)
        {
            return $"{fileName}{GetSuffix(type)}";
        }

        private string GetSavePath(int saveID, bool autoGeneric = true)
        {
            if (GetSaveInfo(saveID) == null)
                throw LogKit.Exception($"YukiFrameWork---当前存档没有被创建，存档id为{saveID}");

            string folder = $@"{saveDirPath}/{saveID}";

            if (Directory.Exists(folder))
                return folder;
            else if(autoGeneric)
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
        public T LoadObjectFromJson<T>(string fileName,int saveID = 0) where T : class
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
        public T LoadObjectFromJson<T>(string fileName, SaveInfo info) where T : class
        {
            return LoadObjectFromJson<T>(fileName, info.saveID);
        }

        /// <summary>
        /// 通过Json文件读取存档
        /// </summary>
        /// <typeparam name="T">返回的类型</typeparam>
        /// <param name="saveID">保存的id</param>
        /// <returns>返回一个类型为T的对象</returns>
        public T LoadObjectFromJson<T>(int saveID = 0) where T : class
        {
            return LoadObjectFromJson<T>(typeof(T).Name,saveID);
        }

        /// <summary>
        /// 通过Json文件读取存档
        /// </summary>
        /// <typeparam name="T">返回的类型</typeparam>     
        /// <param name="info">存档信息</param>
        /// <returns>返回一个类型为T的对象</returns>
        public T LoadObjectFromJson<T>(SaveInfo info) where T : class
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
        public T LoadObjectFromXml<T>(string fileName, int saveID = 0) where T : class
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
        public T LoadObjectFromXml<T>(string fileName, SaveInfo info) where T : class
        {
            return LoadObjectFromXml<T>(fileName, info.saveID);
        }

        /// <summary>
        /// 通过Xml文件读取存档
        /// </summary>
        /// <typeparam name="T">返回的类型</typeparam>
        /// <param name="saveID">保存的id</param>
        /// <returns>返回一个类型为T的对象</returns>
        public T LoadObjectFromXml<T>(int saveID = 0) where T : class
        {
            return LoadObjectFromXml<T>(typeof(T).Name,saveID);
        }

        /// <summary>
        /// 通过Xml文件读取存档
        /// </summary>
        /// <typeparam name="T">返回的类型</typeparam>     
        /// <param name="info">存档信息</param>
        /// <returns>返回一个类型为T的对象</returns>
        public T LoadObjectFromXml<T>(SaveInfo info) where T : class
        {
            return LoadObjectFromXml<T>(typeof(T).Name, info.saveID);
        }

        /// <summary>
        /// 通过bytes文件读取存档
        /// </summary>
        /// <typeparam name="T">返回的类型</typeparam>
        /// <param name="fileName">文件名称</param>
        /// <param name="saveID">保存的id</param>
        /// <returns>返回一个类型为T的对象</returns>
        public T LoadObjectFromBytes<T>(string fileName, int saveID = 0) where T : class
        {
            return Loading<T>(fileName,saveID,SerializationType.Bytes);
        }
        /// <summary>
        /// 通过bytes文件读取存档
        /// </summary>
        /// <typeparam name="T">返回的类型</typeparam>
        /// <param name="fileName">文件名称</param>
        /// <param name="info">存档信息</param>
        /// <returns>返回一个类型为T的对象</returns>
        public T LoadObjectFromBytes<T>(string fileName, SaveInfo info) where T : class
        {
            return LoadObjectFromBytes<T>(fileName, info.saveID);
        }
        /// <summary>
        /// 通过bytes文件读取存档
        /// </summary>
        /// <typeparam name="T">返回的类型</typeparam>
        /// <param name="saveID">保存的id</param>
        /// <returns>返回一个类型为T的对象</returns>
        public T LoadObjectFromBytes<T>(int saveID = 0) where T : class
        {
            return LoadObjectFromBytes<T>(typeof(T).Name,saveID);
        }

        /// <summary>
        /// 通过bytes文件读取存档
        /// </summary>
        /// <typeparam name="T">返回的类型</typeparam>     
        /// <param name="info">存档信息</param>
        /// <returns>返回一个类型为T的对象</returns>
        public T LoadObjectFromBytes<T>(SaveInfo info) where T : class
        {
            return LoadObjectFromBytes<T>(typeof(T).Name, info.saveID);
        }

        private T Loading<T>(string fileName,int saveID,SerializationType type) where T : class
        {
            string name = GetSaveName(type, fileName);
            T obj = GetCache<T>(saveID, name);

            if (obj == null)
            {
                string folder = GetSavePath(saveID, false);
                if (string.IsNullOrEmpty(folder))
                {
                    throw LogKit.Exception("无法读取存档数据，请检查文件夹是否正确或者是否已经保存了存档!");
                }
                string targetPath = $@"{folder}/{name}";
                if (!File.Exists(targetPath))
                {
                    throw LogKit.Exception("该文件夹下存档不存在!路径:" + targetPath);
                }
                string info = File.ReadAllText(targetPath);
                switch (type)
                {
                    case SerializationType.Json:
                        obj = AssemblyHelper.DeserializedObject<T>(info);
                        break;
                    case SerializationType.Xml:
                        obj = AssemblyHelper.XmlDeserializedObject<T>(info);
                        break;
                    case SerializationType.Bytes:
                        byte[] bytes = Encoding.UTF8.GetBytes(info);
                        obj = AssemblyHelper.ByteDeserializedObject<T>(bytes);
                        break;                
                }
            }

            return obj;
        }
        #endregion

        private void CheckAndCreateFolder()
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
