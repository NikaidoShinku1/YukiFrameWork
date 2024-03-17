///=====================================================
/// - FileName:      MethodButtonAttribute.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/3/1 20:06:17
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
namespace YukiFrameWork
{
    [AttributeUsage(AttributeTargets.Method)]
	public sealed class MethodButtonAttribute : PropertyAttribute
	{
        private string label;
        private float height;
        private float width;
        public string Label => label;
        public float Height => height;
        public float Width => width;       
        public object[] Args { get; set; }
        public MethodButtonAttribute(string label,float width,float height,params object[] args)
        {
            this.label = label;
            this.height = height;
            this.width = width;
            this.Args = args;
        }

        public MethodButtonAttribute(float width,float height,params object[] args)
        {
            this.label = string.Empty;
            this.height = height;
            this.width = width;
            this.Args = args;
        }

        public MethodButtonAttribute(string label,params object[] args)
        {
            this.label = label;
            this.height = 20;
            this.Args = args;
            this.width =  -1;
        }

        public MethodButtonAttribute(params object[] args)
        {
            this.label = string.Empty;
            this.height = 20;
            this.Args = args;
            this.width =  -1;
        }
    }
}
