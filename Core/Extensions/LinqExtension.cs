using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Extensions
{
    internal static class LinqExtension
    {
        public static IEnumerable<TSource> DistinctBy<TSource>(this IEnumerable<TSource> source, params Func<TSource, object>[] keySelectors)
        {
            // initialize the table
            var seenKeysTable = keySelectors.ToDictionary(x => x, x => new HashSet<object>());

            // loop through each element in source
            foreach (var element in source)
            {
                // initialize the flag to true
                var flag = true;

                // loop through each keySelector a
                foreach (var (keySelector, hashSet) in seenKeysTable)
                {                    
                    // if all conditions are true
                    flag = flag && hashSet.Add(keySelector(element));
                }

                // if no duplicate key was added to table, then yield the list element
                if (flag)
                {
                    yield return element;
                }
            }
        }
    }
}