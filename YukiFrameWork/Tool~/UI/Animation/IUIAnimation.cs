///=====================================================
/// - FileName:      IUIAnimation.cs
/// - NameSpace:     YukiFrameWork.UI
/// - Description:   高级定制脚本生成
/// - Creation Time: 2025/5/7 17:25:08
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
namespace YukiFrameWork.UI
{
    /// <summary>
    /// UI动画模式。
    /// </summary>
    public interface IUIAnimation 
    {
        /// <summary>
        /// 这个动画模式所绑定的面板
        /// </summary>
        BasePanel Panel { get; set; }
   
        /// <summary>
        /// 当IUIAnimation被赋值时调用
        /// </summary>
        void OnInit();

        /// <summary>
        /// 执行进入动画前执行一次进入
        /// </summary>
        
        void OnEnter(params object[] param);
        /// <summary>
        /// 当UI面板进入(打开时)持续触发的进入动画方法。自由定制。
        /// <para>当进入动画未播放完毕就调用了关闭面板，则会强制退出OnEnterAnimation并开始执行OnExitAnimation</para>
        /// <para>如存在UI动画，则当OnEnterAnimation返回True之前，IsActive不会为True</para>
        /// </summary>      
        /// <returns></returns>
        bool OnEnterAnimation();
        /// <summary>
        /// 当UI面板退出(关闭时)持续触发的退出动画方法。自由定制。
        /// <para>如存在UI动画，则当OnExitAnimation返回True之前，IsActive不会为False</para>
        /// </summary>    
        /// <returns></returns>
        bool OnExitAnimation();

        /// <summary>
        /// 执行退出动画前执行一次退出
        /// </summary>      
        void OnExit();
    }
}
