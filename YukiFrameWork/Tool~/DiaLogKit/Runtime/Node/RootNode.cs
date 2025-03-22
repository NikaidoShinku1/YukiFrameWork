
using System;

namespace YukiFrameWork.DiaLogue
{
    [AttributeUsage(AttributeTargets.Class,AllowMultiple = false)]
    public abstract class DiaLogueNodeAttribute : Attribute
    { 
    }
    /// <summary>
    /// 设置对话的根节点,设置该特性必须直接继承Node类
    /// </summary>
    public sealed class RootNodeAttribute : DiaLogueNodeAttribute
    {       
        public RootNodeAttribute()
        {
            
        }
    }
    /// <summary>
    /// 设置对话的分支节点，标记后节点属于分支节点
    /// </summary>
    public sealed class CompositeNodeAttribute : DiaLogueNodeAttribute
    {
        public CompositeNodeAttribute()
        { 

        }
    }

    /// <summary>
    /// 设置对话的随机节点，标记后节点属于随机节点
    /// </summary>
    public sealed class RandomNodeAttribute : DiaLogueNodeAttribute
    {
        public RandomNodeAttribute() 
        {

        }
    }

    /// <summary>
    /// 单节点标记，标记该特性后节点的类型为默认单分支节点。
    /// </summary>
    public sealed class SingleNodeAttribute : DiaLogueNodeAttribute
    {
        public SingleNodeAttribute() { }
    }

    [Obsolete("已过时的特性，对话系统采用Excel转换配置，该特性不再使用，如果有不需要序列化的字段，标记ExcelIgnore即可")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class DeSerializedNodeFieldAttribute : Attribute
    { 

    }
}
