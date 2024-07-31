
using System;

namespace YukiFrameWork.DiaLogue
{
    [AttributeUsage(AttributeTargets.Class,AllowMultiple = false)]
    public abstract class DiaLogueNodeAttribute : Attribute
    { 
    }
    /// <summary>
    /// ���öԻ��ĸ��ڵ�,���ø����Ա���ֱ�Ӽ̳�Node��
    /// </summary>
    public sealed class RootNodeAttribute : DiaLogueNodeAttribute
    {       
        public RootNodeAttribute()
        {
            
        }
    }
    /// <summary>
    /// ���öԻ��ķ�֧�ڵ㣬��Ǻ�ڵ����ڷ�֧�ڵ�
    /// </summary>
    public sealed class CompositeNodeAttribute : DiaLogueNodeAttribute
    {
        public CompositeNodeAttribute()
        { 

        }
    }

    /// <summary>
    /// ���öԻ�������ڵ㣬��Ǻ�ڵ���������ڵ�
    /// </summary>
    public sealed class RandomNodeAttribute : DiaLogueNodeAttribute
    {
        public RandomNodeAttribute() 
        {

        }
    }

    /// <summary>
    /// ���ڵ��ǣ���Ǹ����Ժ�ڵ������ΪĬ�ϵ���֧�ڵ㡣
    /// </summary>
    public sealed class SingleNodeAttribute : DiaLogueNodeAttribute
    {
        public SingleNodeAttribute() { }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class DeSerializedNodeFieldAttribute : Attribute
    { 

    }
}
