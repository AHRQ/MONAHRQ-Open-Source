using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Monahrq.Websites.Model
{/***
  * 
  * string desc = myEnumVariable.GetAttributeOfType<DescriptionAttribute>().Description
  * 
  */
    public static class EnumAttributeExtension
    {
        public static IEnumerable<Enum> GetFlags(this Enum input)
        {
            return Enum.GetValues(input.GetType()).Cast<Enum>().Where(value => input.HasFlag(value));
        }


        // GetBasicAttributes the Name value of the Display attribute if the   
        // enum has one, otherwise use the value converted to title case.  
        public static string GetDescription<TEnum>(this TEnum value)
            where TEnum : struct, IConvertible
        {
            var attr = value.GetAttributeOfType<TEnum, DescriptionAttribute>();
            return attr == null ? value.ToString() : attr.Description;
        }

		public static T GetValueFromDescription<T>(this string description)
		{
			var type = typeof(T);
			if (!type.IsEnum) throw new InvalidOperationException();
			foreach (var field in type.GetFields())
			{
				var attribute = Attribute.GetCustomAttribute(field,
					typeof(DescriptionAttribute)) as DescriptionAttribute;
				if (attribute != null)
				{
					if (attribute.Description == description)
						return (T)field.GetValue(null);
				}
				else
				{
					if (field.Name == description)
						return (T)field.GetValue(null);
				}
			}
			throw new ArgumentException("Not found.", "description");
			// or return default(T);
		}

		/// <summary>
		/// Gets an attribute on an enum field value
		/// </summary>
		/// <typeparam name="T">The type of the attribute you want to retrieve</typeparam>
		/// <param name="value">The enum value</param>
		/// <returns>The attribute of type T that exists on the enum value</returns>
		private static T GetAttributeOfType<TEnum, T>(this TEnum value)
            where TEnum : struct, IConvertible
            where T : Attribute
        {

            return value.GetType()
                        .GetMember(value.ToString())
                        .First()
                        .GetCustomAttributes(false)
                        .OfType<T>()
                        .LastOrDefault();
        }
       
       
        public static bool Has<T>(this System.Enum type, T value)
        {
            try
            {
                return (((int)(object)type & (int)(object)value) == (int)(object)value);
            }
            catch
            {
                return false;
            }
        }
        public static bool Is<T>(this System.Enum type, T value)
        {
            try
            {
                return (int)(object)type == (int)(object)value;
            }
            catch
            {
                return false;
            }
        }
        public static T Add<T>(this System.Enum type, T value)
        {
            try
            {
                return (T)(object)(((int)(object)type | (int)(object)value));
            }
            catch (Exception ex)
            {
                throw new ArgumentException(
                    string.Format(
                        "Could not append value from enumerated type '{0}'.",
                        typeof(T).Name
                        ), ex);
            }
        }
        public static T Remove<T>(this System.Enum type, T value)
        {
            try
            {
                return (T)(object)(((int)(object)type & ~(int)(object)value));
            }
            catch (Exception ex)
            {
                throw new ArgumentException(
                    string.Format(
                        "Could not remove value from enumerated type '{0}'.",
                        typeof(T).Name
                        ), ex);
            }
        }

        /*
         * SomeType value = SomeType.Grapes;
            bool isGrapes = value.Is(SomeType.Grapes); //true
            bool hasGrapes = value.Has(SomeType.Grapes); //true

            value = value.Add(SomeType.Oranges);
            value = value.Add(SomeType.Apples);
            value = value.Remove(SomeType.Grapes);

            bool hasOranges = value.Has(SomeType.Oranges); //true
            bool isApples = value.Is(SomeType.Apples); //false
            bool hasGrapes = value.Has(SomeType.Grapes); //false
         */
    }
}
