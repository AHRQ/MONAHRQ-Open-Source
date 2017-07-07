using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Reflection;
using Monahrq.Sdk.Types;

namespace Monahrq.Infrastructure.Extensions
{
    /// <summary>
    /// Extensions for the enum class.
    /// </summary>
    public static class EnumExtensions
    {

        public static List<string> GetEnumStringValues<T>() where T : struct
        {
            Type enumType = typeof (T);

            // Can't use type constraints on value types, so have to do check like this
            if (enumType.BaseType != typeof (Enum))
                throw new ArgumentException("T must be of type System.Enum");

            return Enum.GetValues(enumType).Cast<Object>().Select(x => x.ToString()).ToList();
        }

        public static T GetEnumValueFromString<T>(string value) where T : struct
        {
            var enumValue = (T) Enum.Parse(typeof (T), value);
            return enumValue;
        }

        public static T GetEnum<T>(this string value) where T : struct
        {
            return GetEnumValueFromString<T>(value);
        }

        public static Dictionary<int, string> ToDictionary(this Type enumType)
        {
            return Enum.GetValues(enumType)
                .Cast<object>()
                .ToDictionary(k => (int) k, v => ((Enum) v).GetDescription());
        }

        public static object GetValueFromDescription(this string description, Type enumType)
        {
            var type = enumType;
            if (!type.IsEnum) throw new InvalidOperationException();
            foreach (var field in type.GetFields())
            {
                var attribute = Attribute.GetCustomAttribute(field,
                    typeof (DescriptionAttribute)) as DescriptionAttribute;
                if (attribute != null)
                {
                    if (attribute.Description == description)
                        return field.GetValue(null);
                }
                else
                {
                    if (field.Name == description)
                        return field.GetValue(null);
                }
            }
            throw new ArgumentException(@"Not found.", "description");
            // or return default(T);
        }

        public static T GetValueFromDescription<T>(this string description)
        {
            return (T) GetValueFromDescription(description, typeof (T));
        }

        /// <summary>
        /// Gets the description of an enumeration.
        /// </summary>
        /// <param name="me">The enumeration.</param>
        /// <returns>The value of the [Description] attribute for the enum, or the name of
        /// the enum value if there isn't one.</returns>
        public static string GetDescription(this Enum me)
        {
            //  Get the enum type.
            var enumType = me.GetType();

            //  Get the description attribute.
            var descriptionAttribute = enumType.GetField(me.ToString())
                .GetCustomAttributes(typeof (DescriptionAttribute), false)
                .FirstOrDefault() as DescriptionAttribute;

            //  Get the description (if there is one) or the name of the enum otherwise.
            return descriptionAttribute != null
                ? descriptionAttribute.Description
                : me.ToString();
        }

        public static List<string> GetEnumDescriptions<T>() where T : struct
        {
            var enumType = typeof (T);
            return Enum.GetValues(enumType).Cast<object>().Select(x => ((Enum) x).GetDescription()).ToList();
        }

        /// <summary>
        /// options:
        ///		- {0} - Enum as String
        ///		- {1} - Enum Description.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="format"></param>
        /// <returns></returns>
        public static List<string> GetEnumFormattedStrings<T>(string format) where T : struct
        {
            var enumType = typeof (T);
            return Enum.GetValues(enumType).Cast<object>().Select(x =>
                String.Format(
                    format,
                    x.ToString(),
                    ((Enum) x).GetDescription())).ToList();
        }

        /// <summary>
        /// options:
        ///		- EnumName - Enum as String
        ///		- EnumDescription - Enum Description.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<dynamic> GetEnumDynamicDTOs<T>() where T : struct
        {
            var enumType = typeof (T);
            return Enum.GetValues(enumType).Cast<object>().Select(x =>
                new
                {
                    EnumName = x.ToString(),
                    EnumDescription = ((Enum) x).GetDescription()
                }.ToExpandoObject() as dynamic).ToList();
        }

        public static List<SelectListItem> GetEnumSelectListItems<T>() where T : struct
        {
           // var result = new List<string>();
            var enumType = typeof (T);
            return Enum.GetValues(enumType).Cast<object>().Select(x =>
                new SelectListItem
                {
                    Value = x.ToString(),
                    Text = ((Enum) x).GetDescription()
                }).ToList();
        }

        /// <summary>
        /// Return the "Description" attribute text for an enum item, or the given default string if the attribute doesn't exist.
        /// </summary>
        /// <example>EnumExtensions.GetEnumFieldDescription(myEnum.myField, myEnum.myField.ToString())</example>
        /// <see cref="http://www.codeproject.com/Articles/13821/Adding-Descriptions-to-your-Enumerations"/>
        public static string GetEnumFieldDescription(object enumValue, string defaultDesc = "")
        {
            FieldInfo fi = enumValue.GetType().GetField(enumValue.ToString());

            if (null != fi)
            {
                object[] attrs = fi.GetCustomAttributes(typeof (DescriptionAttribute), true).ToArray();
                if (attrs.Length > 0)
                    return ((DescriptionAttribute) attrs[0]).Description;
            }

            return defaultDesc;
        }

        public static T GetEnumValueFromDescription<T>(string val) where T : struct
        {
            var enumType = typeof (T);
            var firstOrDefault = Enum.GetValues(enumType)
                .Cast<object>().FirstOrDefault(x => ((Enum) x).GetDescription().ToLower().Equals(val.ToLower()));
            return firstOrDefault == null ? default(T) : (T)firstOrDefault;
        }

        private static uint AsUint(object item)
        {
            var temp = Convert.ChangeType(item, typeof (uint));
            return (uint) temp;
        }

        public static T AllFlags<T>()
        {
            var values = Enum.GetValues(typeof (T)).OfType<object>().Select(AsUint).ToArray();
            uint result = 0;
            values.ToList().ForEach(value => result |= value);
            return (T) Enum.ToObject(typeof (T), result);
        }


        private static TExpected GetAttributeValue<T, TExpected>(this Enum enumeration, Func<T, TExpected> expression,
            Func<Enum, TExpected> notFound = null)
            where T : Attribute
        {
            T attribute =
                enumeration.GetType().GetMember(enumeration.ToString())[0].GetCustomAttributes(typeof (T), false)
                    .Cast<T>()
                    .SingleOrDefault();
            if (attribute == null)
                return notFound == null ? default(TExpected) : notFound(enumeration);
            return expression(attribute);
        }

        public static IEnumerable<Tuple<Enum, TExpected>> GetAttributeValues<T, TExpected>(this Enum enumeration,
            Func<T, TExpected> expression, Func<Enum, TExpected> notFound = null)
            where T : Attribute
        {
            var components = enumeration.Components();
            return components.Select(component => Tuple.Create(component, component.GetAttributeValue(expression, notFound)));
        }

        public static IEnumerable<Enum> Components(this Enum enumeration)
        {
            var flags = Enum.GetValues(enumeration.GetType()).OfType<Enum>().ToList();
            return flags.Where(enumeration.HasFlag);
        }

        public static string GetEnumDescription(this Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes =
                (DescriptionAttribute[]) fi.GetCustomAttributes(
                    typeof (DescriptionAttribute),
                    false);

            return attributes.Length > 0 
                            ? attributes[0].Description 
                            : value.ToString();
        }
    }

    #region do Not remove. I will flush out to make all enum extension methods work off this functionality in the Future - Jason
    ///// <summary>
    ///// Utility class used to translate enums to user friendly names
    ///// </summary>
    ///// <typeparam name="T"></typeparam>
    //public class EnumTranslator<T>
    //{
    //    /// <summary>
    //    /// Parses the specified value.
    //    /// </summary>
    //    /// <param name="value">The value.</param>
    //    /// <returns></returns>
    //    public static T Parse(string value)
    //    {
    //        var enumType = typeof(T);
    //        foreach (var fi in enumType.GetFields())
    //        {
    //            if (fi.Name.ToLower() == value.ToLower())
    //                return (T)fi.GetValue(null);

    //            var fieldAttributes = fi.GetCustomAttributes(typeof(DescriptionAttribute), false).ToArray();

    //            foreach (DescriptionAttribute attr in fieldAttributes)
    //            {
    //                if (attr.Description.EqualsIgnoreCase(value))
    //                    return (T)fi.GetValue(null);
    //            }
    //        }

    //        throw new InvalidCastException(string.Format("Can't convert {0} to {1}", value,
    //                                            enumType));
    //    }

    //    /// <summary>
    //    /// Parses the specified value.
    //    /// </summary>
    //    /// <param name="value">The value.</param>
    //    /// <returns></returns>
    //    public static T ParseFromExtendedDescription(string value)
    //    {
    //        var enumType = typeof(T);
    //        foreach (var fi in enumType.GetFields())
    //        {
    //            if (fi.Name.ToLower() == value.ToLower())
    //                return (T)fi.GetValue(null);

    //            var fieldAttributes = fi.GetCustomAttributes(typeof(ExtendedDescriptionAttribute), false).ToArray();

    //            foreach (ExtendedDescriptionAttribute attr in fieldAttributes)
    //            {
    //                if (attr.Description.EqualsIgnoreCase(value))
    //                    return (T)fi.GetValue(null);
    //            }
    //        }

    //        throw new InvalidCastException(string.Format("Can't convert {0} to {1}", value,
    //                                            enumType));
    //    }

    //    /// <summary>
    //    /// Parses the specified value.
    //    /// </summary>
    //    /// <param name="value">The value.</param>
    //    /// <returns></returns>
    //    public static T ParseFromDescription(string value)
    //    {
    //        var enumType = typeof(T);
    //        foreach (var fi in enumType.GetFields())
    //        {
    //            if (fi.Name.ToLower() == value.ToLower())
    //                return (T) fi.GetValue(null);

    //            var fieldAttributes = fi.GetCustomAttributes(typeof (DescriptionAttribute), false);

    //            foreach (DescriptionAttribute attr in fieldAttributes)
    //            {
    //                if (attr.Description.EqualsIgnoreCase(value))
    //                    return (T) fi.GetValue(null);
    //            }
    //        }

    //        throw new InvalidCastException(string.Format("Can't convert {0} to {1}", value, enumType));
    //    }

    //    /// <summary>
    //    /// Uses the enum description attribute to generate the string translation of the enum value.
    //    /// </summary>
    //    /// <param name="enumValue">The enum value.</param>
    //    /// <returns></returns>
    //    public static string ToString(T enumValue)
    //    {
    //        var enumType = typeof(T);
    //        var fi = enumType.GetField(enumValue.ToString());

    //        //Get the Description attribute that has been applied to this enum
    //        var fieldAttributes = fi.GetCustomAttributes(typeof(DescriptionAttribute), false).ToArray();

    //        if (fieldAttributes.Length > 0)
    //        {
    //            var descAttr = fieldAttributes[0] as DescriptionAttribute;
    //            if (descAttr != null)
    //                return descAttr.Description;

    //        }

    //        //Enum does not have Description attribute so we return default string representation.
    //        return enumValue.ToString();
    //    }

    //    /// <summary>
    //    /// Uses the enum description attribute to generate the string translation of the enum value.
    //    /// </summary>
    //    /// <param name="enumValue">The enum value.</param>
    //    /// <returns></returns>
    //    public static string GetExtendedDescription(T enumValue)
    //    {
    //        var enumType = typeof(T);
    //        var fi = enumType.GetField(enumValue.ToString());

    //        //Get the Description attribute that has been applied to this enum
    //        var fieldAttributes = fi.GetCustomAttributes(typeof(ExtendedDescriptionAttribute), false).ToArray();

    //        if (fieldAttributes.Length > 0)
    //        {
    //            var descAttr = fieldAttributes[0] as ExtendedDescriptionAttribute;
    //            if (descAttr != null)
    //                return descAttr.Description;

    //        }

    //        //Enum does not have Description attribute so we empty string.
    //        return string.Empty;
    //    }

    //    /// <summary>
    //    /// Returns an array of strings that represent the values of the enumerator
    //    /// </summary>
    //    /// <returns></returns>
    //    public static string[] ToArray()
    //    {
    //        return ToList().ToArray();
    //    }

    //    /// <summary>
    //    /// Returns a list of strings that represent the extended description
    //    /// values of the enumerator
    //    /// </summary>
    //    /// <returns></returns>
    //    public static List<string> ToExtendedDescriptionList()
    //    {
    //        var enumValues = new List<string>();
    //        var enumType = typeof(T);
    //        foreach (FieldInfo fi in enumType.GetFields())
    //        {
    //            if (fi.IsSpecialName == false)
    //            {
    //                //Get the Description attribute that has been applied to this enum
    //                var fieldAttributes = fi.GetCustomAttributes(typeof(ExtendedDescriptionAttribute), false);
    //                if (fieldAttributes.Length > 0)
    //                {
    //                    var descAttr = fieldAttributes[0] as ExtendedDescriptionAttribute;
    //                    if (descAttr != null)
    //                        enumValues.Add(descAttr.Description);

    //                }
    //                else
    //                {
    //                    //Enum does not have Description attribute so we return default string representation.
    //                    var enumValue = (T)fi.GetValue(null);
    //                    enumValues.Add(enumValue.ToString());
    //                }
    //            }
    //        }

    //        return enumValues;
    //    }

    //    /// <summary>
    //    /// Returns a list of strings that represent the values of the enumerator
    //    /// </summary>
    //    /// <returns></returns>
    //    public static List<string> ToList()
    //    {
    //        var enumValues = new List<string>();
    //        var enumType = typeof(T);
    //        foreach (var fi in enumType.GetFields())
    //        {
    //            if (fi.IsSpecialName == false)
    //            {
    //                //Get the Description attribute that has been applied to this enum
    //                var fieldAttributes = fi.GetCustomAttributes(typeof(DescriptionAttribute), false).ToArray();
    //                if (fieldAttributes.Length > 0)
    //                {
    //                    var descAttr = fieldAttributes[0] as DescriptionAttribute;
    //                    if (descAttr != null)
    //                        enumValues.Add(descAttr.Description);

    //                }
    //                else
    //                {
    //                    //Enum does not have Description attribute so we return default string representation.
    //                    var enumValue = (T)fi.GetValue(null);
    //                    enumValues.Add(enumValue.ToString());
    //                }
    //            }
    //        }

    //        return enumValues;
    //    }

    //    /// <summary>
    //    /// Returns a dictionary that maps enums to their descriptions.
    //    /// </summary>
    //    /// <returns></returns>
    //    public static Dictionary<T, string> ToDictionary()
    //    {
    //        var enumDictionary = new Dictionary<T, string>();

    //        var enumType = typeof(T);
    //        foreach (var fi in enumType.GetFields())
    //        {
    //            if (fi.IsSpecialName == false)
    //            {
    //                var enumValue = (T)fi.GetValue(null);

    //                //Get the Description attribute that has been applied to this enum
    //                var fieldAttributes = fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
    //                if (fieldAttributes.Length > 0)
    //                {
    //                    var descAttr = fieldAttributes[0] as DescriptionAttribute;
    //                    if (descAttr != null)
    //                        enumDictionary.Add(enumValue, descAttr.Description);
    //                }
    //                else
    //                {
    //                    //Enum does not have Description attribute so we return default string representation.
    //                    enumDictionary.Add(enumValue, enumValue.ToString());
    //                }
    //            }
    //        }

    //        return enumDictionary;
    //    }

    //    /// <summary>
    //    /// Returns binding list of integer description object.
    //    /// </summary>
    //    /// <returns></returns>
    //    public static BindingList<IntegerDescriptionObject> ToBindingList()
    //    {
    //        var enumBindingList = new BindingList<IntegerDescriptionObject>();

    //        var enumType = typeof(T);
    //        foreach (var fi in enumType.GetFields())
    //        {
    //            if (fi.IsSpecialName == false)
    //            {
    //                var enumValue = (T)fi.GetValue(null);

    //                //Get the Description attribute that has been applied to this enum
    //                var fieldAttributes = fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
    //                if (fieldAttributes.Length > 0)
    //                {
    //                    var descAttr = fieldAttributes[0] as DescriptionAttribute;
    //                    if (descAttr != null)
    //                        enumBindingList.Add(new IntegerDescriptionObject(Convert.ToInt32(enumValue), descAttr.Description));
    //                }
    //                else
    //                {
    //                    //Enum does not have Description attribute so we return default string representation.
    //                    enumBindingList.Add(new IntegerDescriptionObject(Convert.ToInt32(enumValue), enumValue.ToString()));
    //                }
    //            }
    //        }

    //        return enumBindingList;
    //    }

    //    /// <summary>
    //    /// Returns the extended description binding list.
    //    /// </summary>
    //    /// <returns></returns>
    //    public static BindingList<IntegerDescriptionObject> ToExtendedBindingList()
    //    {
    //        var enumBindingList = new BindingList<IntegerDescriptionObject>();

    //        var enumType = typeof(T);
    //        foreach (var fi in enumType.GetFields())
    //        {
    //            if (fi.IsSpecialName == false)
    //            {
    //                var enumValue = (T)fi.GetValue(null);

    //                //Get the Description attribute that has been applied to this enum
    //                var fieldAttributes = fi.GetCustomAttributes(typeof(ExtendedDescriptionAttribute), false).ToArray();
    //                if (fieldAttributes.Length > 0)
    //                {
    //                    var descAttr = fieldAttributes[0] as ExtendedDescriptionAttribute;
    //                    if (descAttr != null)
    //                        enumBindingList.Add(new IntegerDescriptionObject(Convert.ToInt32(enumValue), descAttr.Description));
    //                }
    //                else
    //                {
    //                    //Enum does not have Description attribute so we return default string representation.
    //                    enumBindingList.Add(new IntegerDescriptionObject(Convert.ToInt32(enumValue), enumValue.ToString()));
    //                }
    //            }
    //        }

    //        return enumBindingList;
    //    }

    //    /// <summary>
    //    /// Returns a list that contains all of the enum types.  This list can be enumerated
    //    /// against to get each enum value.
    //    /// </summary>
    //    /// <returns></returns>
    //    public static T[] GetKeys()
    //    {
    //        var enumType = typeof(T);

    //        return (from fi in enumType.GetFields() 
    //                where fi.IsSpecialName == false 
    //                select (T) fi.GetValue(null)).ToArray();
    //    }
    //}

    ///// <summary>
    ///// Integer description object
    ///// </summary>
    //public class IntegerDescriptionObject
    //{
    //    /// <summary>
    //    /// Initializes a new instance of the <see cref="IntegerDescriptionObject"/> class.
    //    /// </summary>
    //    /// <param name="enumInteger">The enum integer.</param>
    //    /// <param name="enumDescription">The enum description.</param>
    //    public IntegerDescriptionObject(int enumInteger, string enumDescription)
    //    {
    //        EnumDescription = enumDescription;
    //        EnumInteger = enumInteger;
    //    }

    //    /// <summary>
    //    /// Gets or sets the enum integer.
    //    /// </summary>
    //    /// <value>The enum integer.</value>
    //    public int EnumInteger { get; private set; }

    //    /// <summary>
    //    /// Gets or sets the enum description.
    //    /// </summary>
    //    /// <value>The enum description.</value>
    //    public string EnumDescription { get; private set; }
    //}

    ///// <summary>
    ///// This attribute is used to decorate enum values with their string representation.
    ///// </summary>
    //[AttributeUsage(AttributeTargets.Field)]
    //public class ExtendedDescriptionAttribute : Attribute
    //{
    //    /// <summary>
    //    /// Description of the enum.
    //    /// </summary>
    //    public string Description { get; private set; }

    //    /// <summary>
    //    /// Initializes a new instance of the <see cref="ExtendedDescriptionAttribute"/> class.
    //    /// </summary>
    //    /// <param name="description">The description.</param>
    //    public ExtendedDescriptionAttribute(string description)
    //    {
    //        Description = description;
    //    }
    //}
    #endregion
}
