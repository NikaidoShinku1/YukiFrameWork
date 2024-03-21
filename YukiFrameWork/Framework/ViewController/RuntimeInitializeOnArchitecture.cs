///=====================================================
/// - FileName:      RuntimeArchitecture.cs
/// - NameSpace:     YukiFrameWork
/// - Created:       Yuki
/// - Email:         1274672030@qq.com
/// - Description:   控制器基类
/// - Creation Time: 2024/1/14 17:16:18
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================

using System;
namespace YukiFrameWork
{
    /// <summary>
    /// 运行时自动初始化架构(继承ViewController时)
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class RuntimeInitializeOnArchitecture : Attribute
    {
        private readonly Type type;
        private readonly bool isGeneric = false;

        public Type ArchitectureType => type;
        public bool IsGeneric => isGeneric;   

        /// <summary>
        /// 特性标记
        /// </summary>
        /// <param name="type">架构类型</param>
        /// <param name="IsOnly">是否是全局唯一的架构</param>
        public RuntimeInitializeOnArchitecture(Type type,bool IsGeneric = false)
        {
            this.type = type;
            this.isGeneric = IsGeneric;
        }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class SerializationArchitectureAttribute : Attribute 
    {

    }  
}