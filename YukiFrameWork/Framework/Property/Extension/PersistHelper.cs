using System.IO;
using YukiFrameWork.Extension;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;
namespace YukiFrameWork
{

    public class ScriptHelper
    {
        private static Dictionary<string, SequencePoint> cache;
        public static Dictionary<string, SequencePoint> Cache
        {
            get
            {
                if (cache == null)
                    cache = PersistHelper.Deserialize<Dictionary<string, SequencePoint>>("sourceCodeTextPoint.json");
                return cache;
            }
        }
    }

    public class SequencePoint
    {
        public string FilePath;
        public int StartLine;

        public SequencePoint() { }

        public SequencePoint(string filePath, int startLine)
        {
            StartLine = startLine;
            FilePath = filePath;
        }
    }
    /// <summary>
    /// 持久化数据记录帮助类
    /// </summary>
    public class PersistHelper
    {
        public static bool Exists(string name)
        {
            var path = GetPath();
            if (!Directory.Exists(path))
                return false;
            var file = path + name;
            if (!File.Exists(file))
                return false;
            return true;
        }

        public static T Deserialize<T>(string name) where T : class, new()
        {
            return Deserialize(name, new T());
        }

        public static T Deserialize<T>(string name, T defaultValue = null) where T : class
        {
            var path = GetPath();
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            var file = path + name;
            if (!File.Exists(file))
                return defaultValue;
            var jsonStr = File.ReadAllText(file);
            var t = SerializationTool.DeserializedObject<T>(jsonStr, new JsonSerializerSettings() { ObjectCreationHandling = ObjectCreationHandling.Replace });
            if (t == null)
                return defaultValue;
            return t;
        }

        public static void Serialize<T>(T obj, string name)
        {
            var path = GetPath();
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            var file = path + name;
            var jsonStr = JsonConvert.SerializeObject(obj, Formatting.Indented);
            File.WriteAllText(file, jsonStr);
        }

        private static string GetPath()
        {

#if UNITY_EDITOR
            var path = "ProjectSettings/yukiframework/";
#else
            var path = BasePath + "/ProjectSettings/yukiframework/";
#endif
            return path;
        }

        /// <summary>
        /// 项目的持久路径, 网络需要处理文件时的目录
        /// </summary>
        public static string BasePath => Application.persistentDataPath;
            


        public static string GetFilePath(string name)
        {
            var path = GetPath();
            var file = path + name;
            return file;
        }
    }
}