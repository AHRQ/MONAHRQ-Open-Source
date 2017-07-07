using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monahrq.Sdk.Extensions
{
    public static class KeyValuePairsExtensions
    {
        public static KeyValuePair<string, object>[] Add(this KeyValuePair<string, object>[] kvp, KeyValuePair<string, object> newKvp)
        {
            if (kvp == null)
            {
                return new KeyValuePair<string, object>[] { newKvp };
            }
            else
            {
                KeyValuePair<string, object>[] outKpv = new KeyValuePair<string, object>[kvp.Length + 1];
                kvp.CopyTo(outKpv, 0);
                outKpv[kvp.Length] = newKvp;
                return outKpv;
            }
        }
    }
}
