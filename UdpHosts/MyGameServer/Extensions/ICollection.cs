using System.Collections.Generic;

namespace MyGameServer.Extensions
{
    public static class ICollectionExtensions
    {
        public static void AddAll<T>(this ICollection<T> t, IEnumerable<T> other)
        {
            foreach (var o in other)
            {
                t.Add(o);
            }
        }
    }
}