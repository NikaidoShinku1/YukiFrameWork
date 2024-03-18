///=====================================================
/// - FileName:      AudioConfig.cs
/// - NameSpace:     YukiFrameWork.Audio
/// - Created:       Yuki
/// - Email:         Yuki@qq.com
/// - Description:   这是一个框架工具创建的脚本
/// - Creation Time: 2023年12月16日 14:16:02
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================

using UnityEngine;
using System;

namespace YukiFrameWork.Audio
{
    public class AudioConfig
    {
        /// <summary>
        /// 声音加载中继类的管理池，如需要使用自定义如Resources则重载初始化方法AudioKit.Init();
        /// 加载前在项目中播放音频前手动初始化，默认使用框架默认ABManager资源加载套件加载！
        /// </summary>
        public AudioLoaderPools LoaderPools { get; set; }                    
    }   
}