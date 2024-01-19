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
namespace YukiFrameWork
{
    public enum Log
    {
        I,
        W,
        E,
        D
    }
    [ClassAPI("控制台拓展")]
    [GUIDancePath("YukiFrameWork/Framework/LogKit")]
    public class LogKit
    {
        public static bool LogEnabled { get; set; } = true;

        internal static ILogger customlogger => Debug.unityLogger;       

        public static void I(object message)
        {
            if (!LogEnabled) return;

            customlogger.Log(LogType.Log,message);
        }

        public static void W(object message)
        {
            if (!LogEnabled) return;
            customlogger.Log(LogType.Warning,message);
        }

        public static void E(object message)
        {
            if (!LogEnabled) return;

            customlogger.Log(LogType.Error, message);
        }

        public static void D(object message)
        {
            if (!LogEnabled) return;
            customlogger.Log(LogType.Log,"DEBUG: " + message);
        }

        public static Exception Exception(object message)
        {
            if (!LogEnabled) return null;
            Exception ex = new Exception(message.ToString());
            return ex;
        }      
    }

    [ClassAPI("日志工具拓展")]
    public static class LogKitExtension
    {
        public static ILoggerExtension LogInfo<T>(this T core, Log log = Log.I)
        {
            return LogInfo(core, null, log);
        }

        public static ILoggerExtension LogInfo<T>(this T core,Action<T> LogCall, Log log = Log.I)
        {
            switch (log)
            {
                case Log.I:
                    LogKit.I(core);
                    break;
                case Log.W:
                    LogKit.W(core);
                    break;
                case Log.E:
                    LogKit.E(core);
                    break;
                case Log.D:
                    LogKit.D(core);
                    break;
            }
            LogCall?.Invoke(core);
            return new CustomLogger(LogKit.customlogger.logHandler,core);
        }

        public static void TryExecutionEvent(this Exception exception, Action onEvent)
        {
            try
            {
                onEvent?.Invoke();
            }
            catch
            {
                throw exception;
            }
        }
    }

  
}