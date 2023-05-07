using System;
using System.Collections.Generic;

namespace MultiServerManager.Utils.Extensions
{
    internal static class IEnumerableExt
    {
        public static void ForEach<T>(this IEnumerable<T> values, Action<T> action)
        {
            foreach (var v in values)
            {
                action(v);
            }
        }
    }
}
