using System.Reflection;

///=====================================================
/// - FileName:      InjectBuilderExtension.cs
/// - NameSpace:     YukiFrameWork.IOC
/// - Description:   Container拓展
/// - Creation Time: 2024/2/22 12:44:05
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
namespace YukiFrameWork.IOC
{
	public static class InjectBuilderExtension
	{
        public static bool Destroy<T>(this IReleaseContainer release,string name)
        {
            return release.Destroy(typeof(T), name);
        }

        public static bool Destroy<T>(this IReleaseContainer release)
        {
            return release.Destroy(typeof(T));
        }

        public static T Resolve<T>(this IDefaultContainer d) where T : class
        {
            return d.Resolve(typeof(T)) as T;
        }
        public static T Resolve<T>(this IDefaultContainer d,string name) where T : class
        {
            return d.Resolve(typeof(T),name) as T;
        }
       
        public static IEntryPoint ResolveEntry<T>(this IDefaultContainer d)
        {
            return d.ResolveEntry( typeof(T),typeof(T).Name);
        }

        public static IEntryPoint ResolveEntry<T>(this IDefaultContainer d,string name)
        {
            return d.ResolveEntry(typeof(T), name);
        }

        public static IEntryPoint Inject<T>(this IEntryPoint point, T value)
        {
            return Inject<T>(point,value,string.Empty);
        }

        public static IEntryPoint Inject<T>(this IEntryPoint point,T value,string labelName)
        {
            System.Type valueType = point.Value.GetType();

            if (valueType == null) throw new System.Exception("获取类型失败请重新查找，EntryPoint:" + point.Name);

            foreach (var member in valueType.GetMembers(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.SetProperty))
            {               
                DynamicValueAttribute attribute = member.GetCustomAttribute<DynamicValueAttribute>();

                if (attribute == null) continue;
                SetPointValue<T>(point, member, attribute, labelName, value);
            }

            return point;
        }

        private static void SetPointValue<T>(IEntryPoint point, MemberInfo member, DynamicValueAttribute attribute,string labelName, T value)
        {
            bool IsLabelNullOrEmpty = (string.IsNullOrEmpty(attribute.LabelName) || string.IsNullOrEmpty(labelName));

            if (member is FieldInfo fieldInfo)
            {
                if (IsLabelNullOrEmpty && fieldInfo.FieldType.Equals(typeof(T)))
                    fieldInfo.SetValue(point.Value, value);
                else if (attribute.LabelName == labelName)
                    fieldInfo.SetValue(point.Value, value);
            }
            else if (member is PropertyInfo propertyInfo)
            {
                if (IsLabelNullOrEmpty && propertyInfo.PropertyType.Equals(typeof(T)))
                    propertyInfo.SetValue(point.Value, value);
                else if (attribute.LabelName == labelName && propertyInfo.PropertyType.Equals(typeof(T)))
                    propertyInfo.SetValue(point.Value, value);
            }
        }

       
    }
}
