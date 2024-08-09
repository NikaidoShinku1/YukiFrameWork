using System;

public static class TypeExtend
{
    public static Type[] GetInterfaceGenericTypeArguments(this Type type)
    {
        foreach (var interfaceType in type.GetInterfaces())
        {
            var genericTypeArguments = interfaceType.GetGenericArguments();
            if (genericTypeArguments == null)
                continue;
            if (genericTypeArguments.Length == 0)
                continue;
            return genericTypeArguments;
        }
        throw new Exception("获取接口失败!");
    }

    public static Type GetArrayItemType(this Type type)
    {
        foreach (var interfaceType in type.GetInterfaces())
        {
            var genericTypeArguments = interfaceType.GetGenericArguments();
            if (genericTypeArguments == null)
                continue;
            if (genericTypeArguments.Length == 0)
                continue;
            return genericTypeArguments[0];
        }
        return null;
    }

    public static bool IsInterfaceType(this Type type, params Type[] interfaceTypes)
    {
        foreach (var interfaceType1 in type.GetInterfaces())
        {
            foreach (var interfaceType2 in interfaceTypes)
            {
                if (interfaceType1 == interfaceType2)
                    return true;
            }
        }
        return false;
    }
}