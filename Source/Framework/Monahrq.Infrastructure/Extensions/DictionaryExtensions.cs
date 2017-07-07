using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monahrq.Infrastructure.Extensions
{
    public static class DictionaryExtensions
    {
        /// <summary>
        /// returns dictionary with each item separated by 'delimiter'. Key/Value separated by 'separator'
        /// </summary>
        /// <param name="items">Dictionary to stringify</param>
        /// <param name="delimiter">item delimiter</param>
        /// <param name="seperator">Key/Value seperator</param>
        /// <returns>string</returns>
        public static string ToString(this IEnumerable<KeyValuePair<string,string>> items, string delimiter, string seperator)
        {
            return string.Join(delimiter, 
                items
                .Select( 
                    kvp=> string.Format("{0}{1}{2}", kvp.Key, seperator,kvp.Value)));
        }
        /// <summary>
        /// returns dictionary with each item on a new line. Key Value separated by ': '
        /// </summary>
        /// <param name="items">Dictionary to stringify</param>
        /// <returns>string</returns>
        public static string ToString(this IEnumerable<KeyValuePair<string, string>> items)
        {
            return items.ToString(Environment.NewLine, ": ");
        }

		
        public static bool SetIfNotExist<K,V>(this IDictionary<K,V> items, K key, V value=default(V))
		{
			if (items.ContainsKey(key)) return false;
			items[key] = value;
			return true;
		}
		public static bool SetIfNotExist<K, V>(this IDictionary<K, V> items, K key, Func<V> valueFunc)
		{
			if (items.ContainsKey(key)) return false;
			items[key] = valueFunc();
			return true;
		}

		public static V GetValueOrDefault<K,V>(this IDictionary<K,V> items, K key,V defaultV = default(V))
		{
			if (items.ContainsKey(key)) return items[key];
			return defaultV;
		}
    }
}
