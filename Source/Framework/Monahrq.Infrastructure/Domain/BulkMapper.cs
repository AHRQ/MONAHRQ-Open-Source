using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using Monahrq.Infrastructure.Entities.Domain.Wings;

namespace Monahrq.Infrastructure.Entities.Domain
{

    public interface IBulkMapper
    {
        IEnumerable<object> InstanceValues(object instance);
        Func<object, object> this[string fieldName] { get; }
    }

    public class BulkMapper<T> : IBulkMapper
        where T : class
    {

        public IEnumerable<object> InstanceValues(object instance)
        {
            return Columns.Select(col => this[col.ColumnName]((T)instance)).ToList();
        }

        DataColumn[] Columns { get; set; }

        protected T Instance { get; private set; }

        protected IDictionary<string, Func<T, object>> Lookup { get; private set; }

        public BulkMapper(DataTable dt, T instance = default(T), Target target = null)
        {
            Lookup = new Dictionary<string, Func<T, object>>();
            Columns = dt.Columns.OfType<DataColumn>().OrderBy(col => col.Ordinal).ToArray();
            Instance = instance;
            Prepare();
        }

        void Prepare()
        {

            foreach (var col in Columns)
            {
                Lookup[col.ColumnName] = t =>
                    {
                        object result = null;
                        return result;
                    };
            }

            ApplyElementMappingLookup();

            ApplyTypeSpecificColumnNameLookup();
        }

        protected virtual void ApplyTypeSpecificColumnNameLookup()
        {
        }

        protected virtual string LookupPropertyName(PropertyInfo pi)
        {
             return pi.Name;
        }

        private void ApplyElementMappingLookup()
        {

            //Func<PropertyInfo, bool> PropertyFilter = LookupPropertyFilter;

            Func<PropertyInfo, object, object> temp = (pi, o) => pi.GetValue(o);

            var lookupList = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public).OfType<PropertyInfo>()
                .Select(
                    pi =>
                        {
                            var xlatePropName = LookupPropertyName(pi); 

                            //var isNullableEnum = pi.PropertyType.IsGenericType &&
                            //                     pi.PropertyType.GetGenericTypeDefinition() == typeof (Nullable<>) &&
                            //                     Nullable.GetUnderlyingType(pi.PropertyType) != null && Nullable.GetUnderlyingType(pi.PropertyType).IsEnum;

                            //if (pi.PropertyType.IsEnum || isNullableEnum)
                            //{
                            //    return new Tuple<string, Func<T, object>>(xlatePropName, o => EnumExtensions2.GetEnumFieldDescription(pi.GetValue(o)));
                            //}
                            return new Tuple<string, Func<T, object>>(xlatePropName, o => pi.GetValue(o));
                        })
                .ToList();
            lookupList.ForEach((kvp) =>
                    {
                        Lookup[kvp.Item1] = kvp.Item2;
                    });
        }

        public Func<T, object> this[string fieldName]
        {
            get
            {
                return Lookup[fieldName];
            }
        }

        Func<object, object> IBulkMapper.this[string fieldName]
        {
            get
            {
                Func<object, object> temp = input => this[fieldName]((T)input);

                return temp;
            }
        }


    }

    public static class EnumExtensions2
    {
        /// <summary>
        /// Return the "Description" attribute text for an enum item, or the given default string if the attribute doesn't exist.
        /// </summary>
        /// <example>EnumExtensions.GetEnumFieldDescription(myEnum.myField, myEnum.myField.ToString())</example>
        /// <see cref="http://www.codeproject.com/Articles/13821/Adding-Descriptions-to-your-Enumerations"/>
        public static string GetEnumFieldDescription(object enumValue)
        {
            if (enumValue == null || enumValue == string.Empty) return null;

            FieldInfo fi = enumValue.GetType().GetField(enumValue.ToString());

            if (null != fi)
            {
                object[] attrs = fi.GetCustomAttributes(typeof(DescriptionAttribute), true).ToArray();

                return attrs.Length > 0 && !string.IsNullOrEmpty(((DescriptionAttribute)attrs[0]).Description) 
                                ? ((DescriptionAttribute) attrs[0]).Description 
                                : enumValue.ToString();
            }

            return null;
        }
    }
}
