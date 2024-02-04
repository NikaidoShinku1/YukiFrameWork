using System;
using System.Reflection;

namespace YukiFrameWork.Extension
{
    [AttributeUsage(AttributeTargets.Field 
        | AttributeTargets.Property
        ,AllowMultiple = true)]
    [Obsolete("已不再使用该特性初始化容器")]
    public class InjectionContainerAttribute : Attribute
    {
        private FieldInfo fieldInfo;
        public FieldInfo FieldInfo => fieldInfo;
        public InjectionContainerAttribute() 
        {
            Type type = typeof(ContainerBuilder);

            foreach (var info in type.GetFields
                (BindingFlags.NonPublic 
                | BindingFlags.Instance))
            {
                foreach (var attribute in info.GetCustomAttributes())
                {
                    if (attribute.GetType().Equals(typeof(InjectionBuilderAttribute)))
                    {
                        fieldInfo = info;
                        break;
                    }
                }
            }
        }
    }
}