using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GameServer.Extensions;

namespace GameServer;

public static class ReflectionUtils
{
    public static IEnumerable<Type> FindTypesByAttribute<T>()
        where T : Attribute
    {
        return from t in Assembly.GetExecutingAssembly().GetTypes()
               where t.GetAttribute<T>() != null
               select t;
    }

    public static IEnumerable<MethodInfo> FindMethodsByAttribute<T>(Type type)
        where T : Attribute
    {
        return from m in type.GetMethods()
               where m.GetAttribute<T>() != null
               select m;
    }

    public static IEnumerable<MethodInfo> FindMethodsByAttribute<T>()
        where T : Attribute
    {
        return from t in Assembly.GetExecutingAssembly().GetTypes()
               from m in t.GetMethods()
               where m.GetAttribute<T>() != null
               select m;
    }
}