using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XFABManager
{ 
    /// <summary>
    /// 日期工具
    /// </summary>
    public class DateTimeTools {


        /// <summary>
        /// 本时区日期时间转时间戳
        /// </summary>
        /// <param name="datetime">日期</param>
        /// <param name="millisecond">true:毫秒 false:秒 默认:毫秒</param>
        /// <returns>long=Int64</returns>
        public static long DateTimeToTimestamp(DateTime datetime,bool millisecond = true)
        {
            DateTime dd = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            DateTime timeUTC = DateTime.SpecifyKind(datetime, DateTimeKind.Utc);//本地时间转成UTC时间
            TimeSpan ts = (timeUTC - dd);
            return millisecond ? (long)ts.TotalMilliseconds : (long)ts.TotalSeconds;//精确到毫秒
        }

        /// <summary>
        /// 时间戳转本时区日期时间
        /// </summary>
        /// <param name="timeStamp">时间戳</param>
        /// <param name="millisecond">true:毫秒 false:秒 默认:毫秒</param>
        /// <returns></returns>
        public static DateTime TimestampToDateTime(long timeStamp, bool millisecond = true)
        {
            DateTime dd = DateTime.SpecifyKind(new DateTime(1970, 1, 1, 0, 0, 0, 0), DateTimeKind.Local); 
            return millisecond ? dd.AddMilliseconds(timeStamp):dd.AddSeconds(timeStamp);
        }
 


    }
}

