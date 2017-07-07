using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monahrq.Sdk.Extensibility.Utility
{
    public class Hash
    {
        private long _hash;

        public string Value
        {
            get
            {
                return _hash.ToString("x", CultureInfo.InvariantCulture);
            }
        }

        public override string ToString()
        {
            return Value;
        }

        public void AddString(string value)
        {
            if (string.IsNullOrEmpty(value))
                return;

            _hash += GetStringHashCode(value);
        }

        public void AddStringInvariant(string value)
        {
            if (string.IsNullOrEmpty(value))
                return;

            AddString(value.ToLowerInvariant());
        }

        public void AddTypeReference(Type type)
        {
            AddString(type.AssemblyQualifiedName);
            AddString(type.FullName);
        }

        public void AddDateTime(DateTime dateTime)
        {
            _hash += dateTime.ToBinary();
        }

        /// <summary>
        /// We need a custom string hash code function, because .NET string.GetHashCode()
        /// function is not guaranteed to be constant across multiple executions.
        /// </summary>
        private static long GetStringHashCode(string s)
        {
            unchecked
            {
                long result = 352654597L;
                foreach (var ch in s)
                {
                    long h = ch.GetHashCode();
                    result = result + (h << 27) + h;
                }
                return result;
            }
        }
    }
}
