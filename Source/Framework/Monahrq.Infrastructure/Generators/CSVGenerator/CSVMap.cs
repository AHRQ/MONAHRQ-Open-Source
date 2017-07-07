using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Monahrq.Infrastructure.Generators
{
    /// <summary>
    /// The csv map attribute. This attribute is utilized by placing on the object properties that should be included
    /// inthe csv file generation / export.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Property)]
    public class CSVMap : System.Attribute
    {
        #region mapping properties
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public FieldType Type { get; set; }
        /// <summary>
        /// The ordinal
        /// </summary>
        private int _ordinal = int.MinValue;

        /// <summary>
        /// Gets or sets the ordinal.
        /// </summary>
        /// <value>
        /// The ordinal.
        /// </value>
        public int Ordinal
        {
            get { return _ordinal; }
            set { _ordinal = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="CSVMap"/> is transform.
        /// </summary>
        /// <value>
        ///   <c>true</c> if transform; otherwise, <c>false</c>.
        /// </value>
        public bool Transform { get; set; }

        /// <summary>
        /// Gets the name of the member.
        /// </summary>
        /// <value>
        /// The name of the member.
        /// </value>
        public string MemberName { get; private set; }
        /// <summary>
        /// Gets the type of the member.
        /// </summary>
        /// <value>
        /// The type of the member.
        /// </value>
        public Type MemberType { get; private set; }
        /// <summary>
        /// Gets the member information.
        /// </summary>
        /// <value>
        /// The member information.
        /// </value>
        public PropertyInfo MemberInfo { get; private set; }

        #endregion

        /// <summary>
        /// Gets the map.
        /// </summary>
        /// <param name="modelType">Type of the model.</param>
        /// <returns></returns>
        public static Dictionary<int?, CSVMap> GetMap(Type modelType)
        {
            int maxOrdinal = 0;
            var props = modelType.GetProperties();
            var map = new Dictionary<int?, CSVMap>();
            foreach (var m in props)
            {
                var attr = (m.GetCustomAttributes(typeof(CSVMap), true).FirstOrDefault());
                if (attr == null) continue;
                var mapAttr = (CSVMap)attr;
                mapAttr.MemberName = m.Name;
                mapAttr.Name = mapAttr.Name ?? mapAttr.MemberName;
                mapAttr.MemberType = m.PropertyType;
                //if order not specififed dump it at the end of the fields
                maxOrdinal++;
                if (mapAttr.Ordinal == int.MinValue)
                {
                    if (map.Count == 0) mapAttr.Ordinal = maxOrdinal;
                    else mapAttr.Ordinal = Math.Max(maxOrdinal, map.Keys.Max(k => k.Value) + 1);
                }

                mapAttr.MemberInfo = m.DeclaringType.GetProperty(m.Name);
                map.Add(mapAttr.Ordinal, mapAttr);
            }
            return map;
        }
    }
    /// <summary>
    /// The csv field type.
    /// </summary>
    public enum FieldType
    {
        /// <summary>
        /// The not set
        /// </summary>
        NotSet,
        /// <summary>
        /// The alphanumeric
        /// </summary>
        Alphanumeric,
        /// <summary>
        /// The numeric
        /// </summary>
        Numeric
    }
}
