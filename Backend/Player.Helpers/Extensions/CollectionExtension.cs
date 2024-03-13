using System;
using System.Collections.Generic;

namespace Player.Helpers.Extensions
{
    public static class CollectionExtension
    {
        public static ICollection<T> Randomize<T>(this IEnumerable<T> collection)
        {
            var list = new List<T>(collection);

            var random = new Random();
            
            var n = list.Count;  
            while (n > 1) {  
                n--;
                var k = random.Next(n + 1);
                var value = list[k];
                list[k] = list[n];
                list[n] = value;
            }

            return list;
        }
    }
}