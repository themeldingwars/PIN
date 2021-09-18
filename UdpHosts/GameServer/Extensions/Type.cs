using System;
using System.Linq;

namespace GameServer.Extensions
{
    public static class TypeExtensions
    {
        public static T GetAttribute<T>(this Type type, bool inherit = false)
            where T : Attribute
        {
            return type.GetCustomAttributes(typeof(T), inherit).FirstOrDefault() as T;
        }
    }
}