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
    
    [AttributeUsage(AttributeTargets.Property
        | AttributeTargets.Field)]
    [Obsolete("过时的UI模块自动化特性，该特性已经被VFindChildComponentByName取代!")]
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