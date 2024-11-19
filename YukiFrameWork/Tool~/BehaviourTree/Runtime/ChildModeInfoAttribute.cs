using Sirenix.OdinInspector;
using System;

/// <summary>
/// 子节点容量
/// </summary>
public enum ChildMode
{
    /// <summary>
    /// 无
    /// </summary>
    None,
        
    /// <summary>
    /// 一个
    /// </summary>
    Single,
        
    /// <summary>
    /// 多个
    /// </summary>
    Multiple,
}

/// <summary>
/// 子节点容量信息特性
/// </summary>
public class ChildModeInfoAttribute : Attribute
{
    public ChildMode ChildMode { get; set; }
}
