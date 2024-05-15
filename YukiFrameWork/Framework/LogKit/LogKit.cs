///=====================================================
/// - FileName:      LogKit.cs
/// - NameSpace:     YukiFrameWork
/// - Created:       Yuki
/// - Email:         1274672030@qq.com
/// - Description:   控制台Debug拓展类
/// - Creation Time: 2023/12/31 17:29:15
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================

using UnityEngine;
using System;
using YukiFrameWork.Extension;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using Debug = UnityEngine.Debug;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Threading.Tasks;
using System.Linq;
namespace YukiFrameWork
{    
    [ClassAPI("控制台拓展")]
    [GUIDancePath("YukiFrameWork/Framework/LogKit")]
    public class LogKit
    {
        internal const string LOGFULLCONDITION = "YukiFrameWork_DEBUGFULL";
        internal const string LOGINFOCONDITION = "YukiFrameWork_DEBUGINFO";
        internal const string LOGWARNINGCONDITION = "YukiFrameWork_DEBUGWARNING";
        internal const string LOGERRORCONDITION = "YukiFrameWork_DEBUGERROR";       
        private static LogConfig config;

        private static StreamWriter mLogInfoWriter;
        private static Task writeFileTask = null;    

        public static bool LogEnabled
        {
            get => config.LogEnabled;
            set => config.LogEnabled = value;
        }

        public static bool LogSaving
        {
            get => config.LogSaving;
            set => config.LogSaving = value;
        }

        private static string mLogFileName;
        private static bool IsWriting;

        private static Queue<string> allLogInfos = new Queue<string>();

        static void InitLogFileName()
        {
            mLogFileName = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
            mLogFileName = mLogFileName.Replace("-", "_");
            mLogFileName = mLogFileName + ".log";
        }

        [RuntimeInitializeOnLoadMethod]
        static void Init()
        {          
            config = Resources.Load<LogConfig>(nameof(LogConfig));

            if (config == null) return;
            if (config.saveDirPath.IsNullOrEmpty()) return;

            Task.Run(() =>
            {
                string[] files = Directory.GetFiles(config.saveDirPath).Where(x => !x.EndsWith(".meta")).ToArray();
                Array.Reverse(files);
                for (int i = 0; i < files.Length; i++)
                {
                    try
                    {                      
                        DateTime dateTime = File.GetCreationTime(files[i]);

                        if ((DateTime.Now - dateTime).Days > 3 || i + 1 > config.fileCount)
                        {
                            File.Delete(files[i]);
                        }
                    }
                    catch (Exception)
                    {

                    }
                }
            });
        }

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
        static void EditorInit()
        {          
            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);

            config = Resources.Load<LogConfig>(nameof(LogConfig));
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<LogConfig>();
                string path = "Assets/Resources";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                    AssetDatabase.Refresh();
                }
                AssetDatabase.CreateAsset(config, $"{path}/{nameof(LogConfig)}.asset");
                AssetDatabase.Refresh();
            }
            if (!config.IsInitialization)
            {
                config.IsInitialization = true;
                if (defines.IsNullOrEmpty())
                    defines += LOGFULLCONDITION + ";";
                else defines += $";{LOGFULLCONDITION};";
                PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, defines);
            }        
        }
#endif
        [Conditional(LOGINFOCONDITION),Conditional(LOGFULLCONDITION)]
        public static void I(object message,params object[] args)
        {
            if (!config.LogEnabled) return;
            string msg = $"{config.prefix} Info:{message}";

            if (args == null || args.Length == 0)
                Debug.Log(msg);
            else
                Debug.LogFormat(msg, args);

            allLogInfos.Enqueue(msg);
            LogToFile();
        }
        [Conditional(LOGFULLCONDITION),Conditional(LOGWARNINGCONDITION)]
        public static void W(object message,params object[] args)
        {
            if (!config.LogEnabled) return;
            string msg = $"{config.prefix} Warning:{message}";

            if (args == null || args.Length == 0)
                Debug.LogWarning(msg);
            else
                Debug.LogWarningFormat(msg, args);

            allLogInfos.Enqueue(msg);
            LogToFile();
        }
        [Conditional(LOGFULLCONDITION), Conditional(LOGERRORCONDITION)]
        public static void E(object message,params object[] args)
        {
            if (!config.LogEnabled) return;

            string msg = $"{config.prefix} Error:{message}";

            if (args == null || args.Length == 0)
                Debug.LogError(msg);
            else
                Debug.LogErrorFormat(msg, args);

            allLogInfos.Enqueue(msg);
            LogToFile();
        }    
      
        public static void Exception(Exception e)
        {
            Debug.LogException(e);
        }

        public static void Exception(object message)
        {
            Exception(new System.Exception(message.ToString()));
        }

        private static void OnLogByUnity(string condition, string stackTrace, LogType type)
        {
            if(type == LogType.Log || condition.StartsWith(config.prefix))
            {
                return;
            }
            var str = type == LogType.Warning ? "[W]" : "[E]" + GetSystemNowTime() + condition + "\n" + stackTrace;                 
            LogToFile();

        } 

        private static string GetSystemNowTime()
        {
            return DateTime.Now.ToString("HH:mm:ss.fff") + " "; ;
        }

        /// <summary>
        /// 将日志写入到文件中
        /// </summary>
        /// <param name="message"></param>
        /// <param name="EnableStack"></param>
        private static void LogToFile()
        {
            if (!config.LogSaving || IsWriting)
                return;
            IsWriting = true;
            if (!Application.isPlaying)
                mLogFileName = "EditorLogTip.log";
            else if (mLogFileName.IsNullOrEmpty())
                InitLogFileName();

            writeFileTask = Task.Run(() =>
            {                  
                if (string.IsNullOrEmpty(config.saveDirPath))
                {                  
                    return;
                }
                string path = config.saveDirPath + "/" + mLogFileName;                
                try
                {
                    if (!Directory.Exists(config.saveDirPath))
                    {
                        Directory.CreateDirectory(config.saveDirPath);
                    }                   
                    mLogInfoWriter = File.AppendText(path);                                

                    while (allLogInfos.Count > 0)
                    {
                        string t = allLogInfos.Dequeue();
                        mLogInfoWriter.WriteLine(t);
                        mLogInfoWriter.Flush();
                    }
                    mLogInfoWriter.Close();

                }
                catch (Exception)
                {
                   
                }
                IsWriting = false;
                writeFileTask = null;
                mLogInfoWriter = null;
            });
        }   

    }
    
    [ClassAPI("日志工具拓展")]
    public static class LogKitExtension
    {
        public static void LogInfo<T>(this T core,params object[] args)
        {
            LogKit.I(core,args);
        }
        
        public static void LogError<T>(this T core,params object[] args)
        {
            LogKit.E(core,args);
        }

        public static void LogWarning<T>(this T core,params object[] args)
        {
            LogKit.W(core,args);
        }

        public static void LogException<T>(this Exception core)
        {
            LogKit.Exception(core);
        }

        public static void LogException<T>(this object core)
        {
            LogKit.Exception(core);
        }
    }

  
}