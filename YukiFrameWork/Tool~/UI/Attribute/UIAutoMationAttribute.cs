///=====================================================
/// - FileName:      UIAutoMationAttribute.cs
/// - NameSpace:     YukiFrameWork.UI
/// - Created:       Yuki
/// - Email:         Yuki@qq.com
/// - Description:   UI框架注入参数特性
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================

using System;
namespace YukiFrameWork.UI
{
    /// <summary>
    /// 该特性仅在使用UI框架时启用，可以自动注入面板下派生自组件的字段(属性) 注意：可自动注入的参数应为自身组件或者子物体组件 | 
    /// 该特性与IOC模块中的Inject在细节上不同之外功能是一致的
    /// 二者会同时生效所以不可以重复标记
    /// </summary>
    [AttributeUsage(AttributeTargets.Property
        | AttributeTargets.Field)]
    public class UIAutoMationAttribute : Attribute
    {
        private string _name;
        public string Name => _name;

        /// <summary>
        /// 根据GameObject的名称进行查找
        /// </summary>
        /// <param name="name">GameObject的名称</param>
        public UIAutoMationAttribute(string name)
           => _name = name;

        public UIAutoMationAttribute()
            => _name = string.Empty;
    }
}