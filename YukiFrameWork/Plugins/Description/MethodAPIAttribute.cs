///=====================================================
/// - FileName:      MethodAPI.cs
/// - NameSpace:     YukiFrameWork.Extension
/// - Created:       Yuki
/// - Email:         Yuki@qq.com
/// - Description:   注解方法特性
/// - Creation Time: xxxx年x月xx日 xx:xx:xx
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================

using System;
namespace YukiFrameWork.Extension
{   
    [AttributeUsage(AttributeTargets.Method,AllowMultiple = true)]
    public class MethodAPIAttribute : Attribute
    {
        private string methodDescription;

        public string MethodDescription
            => methodDescription;

        public MethodAPIAttribute(string description)
        {
            this.methodDescription = description;
        }
    }
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class ClassAPIAttribute : Attribute
    {
        private string classDescription;

        public string ClassDescription
            => classDescription;

        public ClassAPIAttribute(string classDescription)
        {
            this.classDescription = classDescription;
        }
    }
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class GUIDancePathAttribute : Attribute
    {
        private string classDescription;

        public string ClassDescription
            => classDescription;

        public GUIDancePathAttribute(string classDescription)
        {
            this.classDescription = classDescription;
        }
    }
}