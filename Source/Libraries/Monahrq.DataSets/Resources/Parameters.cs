using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monahrq.DataSets.Resources
{
    /// <summary>
    /// The parameters utlity class.
    /// </summary>
    public static class Parameters
    {
        /// <summary>
        /// The parameter list
        /// </summary>
        private static Dictionary<int, object> paramList =
            new Dictionary<int, object>();

        /// <summary>
        /// Saves the specified hash.
        /// </summary>
        /// <param name="hash">The hash.</param>
        /// <param name="value">The value.</param>
        public static void save(int hash, object value)
        {
            if (!paramList.ContainsKey(hash))
                paramList.Add(hash, value);
        }

        /// <summary>
        /// Requests the specified hash.
        /// </summary>
        /// <param name="hash">The hash.</param>
        /// <returns></returns>
        public static object request(int hash)
        {
            return ((KeyValuePair<int, object>)paramList.
                        Where(x => x.Key == hash).FirstOrDefault()).Value;
        }
    }
}
