using System;
using System.Reflection;

namespace YukiFrameWork.Extension
{
    [AttributeUsage(AttributeTargets.Field 
        | AttributeTargets.Property
        ,AllowMultiple = true)]
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