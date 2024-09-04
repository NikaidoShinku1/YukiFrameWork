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
using System.Text;

#if UNITY_2023_1_OR_NEWER && UNITY_EDITOR
using UnityEditor.Build;
#endif

#if UNITY_EDITOR
using UnityEditor;
#endif
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

        private static LogConfig _config;

        private static LogConfig config
        {
            get
            {
                if (_config == null)
                    _config = Resources.Load<LogConfig>(nameof(LogConfig));
                return _config;
            }
        }

        private static StreamWriter mLogInfoWriter;

        public static bool LogEnabled
        {
            get => config.LogEnabled;
            set
            {
                config.LogEnabled = value;
#if UNITY_EDITOR
                config.Save();
#endif
            }
        }

        public static bool LogSaving
        {
            get => config.LogSaving;
            set
            {
                config.LogSaving = value;
#if UNITY_EDITOR
                config.Save();
#endif
            }
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
        static LogKit() 
        {
            Application.logMessageReceivedThreaded += OnLogByUnity;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Init()
        {
            if (config == null) return;
            if (config.saveDirPath.IsNullOrEmpty()) return;

            if (!Directory.Exists(config.saveDirPath)) return;

            CheckFileSize();

            LogKit.I("获取日志文件输出路径:" + config.saveDirPath);
        }

        private static async void CheckFileSize()
        {
            ///防止运行时卡顿，真实时间三秒后执行
            await CoroutineTool.WaitForSecondsRealtime(3f);
            var files = Directory.GetFiles(config.saveDirPath).Where(x => !x.EndsWith(".meta") && !x.StartsWith("EditorLogTip")).ToArray();

            int deleteSize = files.Length - config.fileCount;
            if (deleteSize <= 0) return;  
            try
            {
                for (int i = 0; i < deleteSize; i++)
                {
                    //每一百个文件等待一帧
                    if(i % 100 == 0)                   
                        await CoroutineTool.WaitForFrame();
                    File.Delete(files[i]);
                }
            }
            catch (Exception)
            {

            }

        }

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
        static void EditorInit()
        {
#if UNITY_2023_1_OR_NEWER
            string defines = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.Standalone);
#else
            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);
#endif
            if (config == null)
            {
                _config = ScriptableObject.CreateInstance<LogConfig>();
                string path = "Assets/Resources";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                    AssetDatabase.Refresh();
                }
                AssetDatabase.CreateAsset(_config, $"{path}/{nameof(LogConfig)}.asset");
                AssetDatabase.Refresh();
            }
            if (!_config.IsInitialization)
            {
                _config.IsInitialization = true;
                if (!defines.Contains(LOGFULLCONDITION))
                {
                    if (defines.IsNullOrEmpty())
                        defines += LOGFULLCONDITION + ";";
                    else defines += $";{LOGFULLCONDITION};";
                }
#if UNITY_2023_1_OR_NEWER
                PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.Standalone,defines);
#else
                PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, defines);
                _config.Save();
#endif
            }
        }
#endif
        [Conditional(LOGINFOCONDITION), Conditional(LOGFULLCONDITION)]
        public static void I(object message, params object[] args)
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
        [Conditional(LOGFULLCONDITION), Conditional(LOGWARNINGCONDITION)]
        public static void W(object message, params object[] args)
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
        public static void E(object message, params object[] args)
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

        public static void Exception(Exception ex)
        {
            throw new Exception(string.Format("{0}\n------------> \nReal StackTrace(捕捉时机堆栈坐标) --> \n{1}\n\n------------------\n Default StackTrace(Unity 默认捕捉) ", ex.Message, ex.StackTrace));
        }
       
        static StringBuilder stackBuilder = new StringBuilder();
        private static void OnLogByUnity(string condition, string stackTrace, LogType type)
        {
            // 这里只关心报错信息
            if (type != LogType.Exception) return;

            if (!config.LogSaving) return;
            stackBuilder.Clear();
            stackBuilder.Append("[").Append(DateTime.Now.ToString()).Append("]").Append("[").Append(type.ToString()).Append("]").Append(condition).AppendLine();
            stackBuilder.AppendLine(stackTrace);
            allLogInfos.Enqueue(stackBuilder.ToString());
            LogToFile();
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
            catch (Exception ex)
            {
                Debug.Log(ex.ToString());
            }
            IsWriting = false;
            mLogInfoWriter = null;

        }

    }

    [ClassAPI("日志工具拓展")]
    public static class LogKitExtension
    {
        public static void LogInfo<T>(this T core, params object[] args)
        {
            LogKit.I(core, args);
        }

        public static void LogError<T>(this T core, params object[] args)
        {
            LogKit.E(core, args);
        }

        public static void LogWarning<T>(this T core, params object[] args)
        {
            LogKit.W(core, args);
        }

        public static void LogException<T>(this Exception core)
        {
            LogKit.Exception(core);
        }

    }

}