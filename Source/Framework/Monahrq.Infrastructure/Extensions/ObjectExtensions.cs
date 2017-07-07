using System.Linq;
using System.Xml;
using Monahrq.Infrastructure.Utility;
using System.Dynamic;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System;

namespace Monahrq.Infrastructure.Extensions
{
	[Flags]
	public enum EConvertToOptions
	{
		None = 0,
		AvoidNull = 1,
		NoException = 2,
		UseDefault = 2
	}
	public static class ObjectExtensions
	{
		#region Xml Conversion Methods.
		/// <summary>
		/// Removing this from the IXmlSerializable Implementation because engineers keep exposing the SerialzableDictionary object
		/// and that cannot be allowed because nobody else can deserialize.  It is for persistence only so the logic was moved here.
		/// </summary>
		/// <param name="list">The dic.</param>
		/// <returns></returns>
		public static XmlDocument ConvertToXml<T>(this T item, bool removeDeclaration = false) where T : class
		{
			var xmlDocument = XmlHelper.Serialize(item, typeof(T));

			if (removeDeclaration)
			{
				foreach (XmlNode node in xmlDocument.Cast<XmlNode>().Where(node => node.NodeType == XmlNodeType.XmlDeclaration).ToList())
				{
					xmlDocument.RemoveChild(node);
				}
			}

			return xmlDocument;
		}

		/// <summary>
		/// Removing this from the IXmlSerializable Implementation because engineers keep exposing the SerialzableDictionary object
		/// and that cannot be allowed because nobody else can deserialize.  It is for persistence only so the logic was moved here.
		/// </summary>
		/// <param name="doc">The xml document.</param>
		/// <returns></returns>
		public static T ConvertFromXml<T>(this XmlDocument doc) where T : class
		{
			return XmlHelper.Deserialize<T>(doc);
		}

		/// <summary>
		/// Removing this from the IXmlSerializable Implementation because engineers keep exposing the SerialzableDictionary object
		/// and that cannot be allowed because nobody else can deserialize.  It is for persistence only so the logic was moved here.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="xml">The XML string.</param>
		/// <returns></returns>
		public static T ConvertFromXml<T>(this string xml) where T : class
		{
			var xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(xml);
			return XmlHelper.Deserialize<T>(xmlDoc);
		}

		#endregion

		#region Expando Conversion Methods.

		public static ExpandoObject ToExpandoObject(this object _this)
		{
			if (_this == null) return null;
			IDictionary<string, object> expando = new ExpandoObject();

			foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(_this.GetType()))
				expando.Add(property.Name, property.GetValue(_this));

			return expando as ExpandoObject;
		}

		public static dynamic ToDynamic(this object _this)
		{
			return _this.ToExpandoObject();
		}
		#endregion

		#region Ubiqutous Conversion Methods.
		/// <summary>
		/// Converts an object into the specified type.  Uses 'smart' functionality to perform conversion.
		/// Methods accepts a list of 'default' values that will be attempted for conversion in case
		/// conversion fails.
		/// If all conversions fail, an Exception is thrown.
		/// </summary>
		/// <example>	 
		///		enum ECompassDirection { North, East, South, West }
		/// 
		/// 	private static void TestNumberConversion()
		///		{
		///			String valueA = "4.5";
		///			double valueB = valueA.ConvertTo<double>();
		///			Console.WriteLine("Number: {0}", valueB);					// Displays 4.5
		///		}
		///		private static void TestEnumConversion()
		///		{
		///			String valueA = "Southx";
		///			ECompassDirection valueB = valueA.ConvertTo<ECompassDirection>(null,"EES","West",CompassDirection.North);
		///			Console.WriteLine("CompassDirection: {0}", valueB);			// Displays West
		///		}
		///		private static void TestNullableIntConversion()
		///		{
		///			int? valueA = null;
		///			int valueB = valueA.ConvertTo<int>();
		///			Console.WriteLine("int? = null: {0}", valueB);				// Displays 0
		///		
		///			valueA = null;
		///			valueB = valueA.ConvertTo<int>(8);
		///			Console.WriteLine("int? = null default 8: {0}", valueB);	// Displays 8
		///		
		///			valueA = 6;
		///			valueB = valueA.ConvertTo<int>();
		///			Console.WriteLine("int? = 6: {0}", valueB);					// Displays 6
		///		}
		/// </example>

		#region No Options Methods.
		public static T ConvertTo<T>(this object value)
		{
			return ConvertTo<T>(value, CultureInfo.CurrentCulture);
		}
		public static T ConvertTo<T>(this object value, CultureInfo cultureInfo)
		{
			return ConvertTo<T>(value, cultureInfo, default(T));
		}
		public static T ConvertTo<T>(this object value, params object[] defaultValues)
		{
			return ConvertTo<T>(value, CultureInfo.CurrentCulture, defaultValues);
		}
		public static T ConvertTo<T>(this object value, CultureInfo cultureInfo, params object[] defaultValues)
		{

			return ConvertTo<T>(value, CultureInfo.CurrentCulture, EConvertToOptions.None, defaultValues);

		}
		#endregion

		#region Options Methods.

		public static T ConvertTo<T>(this object value, EConvertToOptions options)
		{
			return ConvertTo<T>(value, options, CultureInfo.CurrentCulture);
		}
		public static T ConvertTo<T>(this object value, CultureInfo cultureInfo, EConvertToOptions options)
		{
			return ConvertTo<T>(value, cultureInfo, options, default(T));
		}
		public static T ConvertTo<T>(this object value, EConvertToOptions options, params object[] defaultValues)
		{
			return ConvertTo<T>(value, CultureInfo.CurrentCulture, options, defaultValues);
		}
		public static T ConvertTo<T>(this object value, CultureInfo cultureInfo, EConvertToOptions options, params object[] defaultValues)
		{
			bool isNullSeen = false;

			T result;
			if (InternalConvertTo(value, cultureInfo, out result))
				if ((result == null && !options.HasFlag(EConvertToOptions.AvoidNull)) || result != null)
					return result;
				else if (result == null) isNullSeen = true;

			foreach (var defaultValue in defaultValues)
			{
				if (InternalConvertTo(defaultValue, cultureInfo, out result))
					if ((result == null && !options.HasFlag(EConvertToOptions.AvoidNull)) || result != null)
						return result;
					else if (result == null) isNullSeen = true;
			}

			if (options.HasFlag(EConvertToOptions.NoException))
				return default(T);		//return isNullSeen && options.HasFlag(EConvertToOptions.AvoidNull) ? (T)null as T : default(T);
			else
				throw new Exception("Could not convert to specified type.");
			//	return default(T);
		}
		#endregion

		private static bool InternalConvertTo<T>(object value, CultureInfo cultureInfo, out T result)
		{
			var toType = typeof(T);

			if (value == null)
			{
				if (toType.IsGenericType && toType.GetGenericTypeDefinition() == typeof(Nullable<>) ||
					toType == typeof(string))
				{
					result = default(T);	// null;
					return true;
				}

				result = default(T);
				return false;
			}

			if (toType.IsGenericType &&
				toType.GetGenericTypeDefinition() == typeof(Nullable<>))
			{
				toType = Nullable.GetUnderlyingType(toType); ;
			}


			if (value is string)
			{
				if (toType == typeof(Guid))
				{
					InternalConvertTo<T>(new Guid(Convert.ToString(value, cultureInfo)), cultureInfo, out result);
					return true;
				}
				if (toType.IsEnum)
				{
					try
					{
						result = (T)Enum.Parse(toType, value as string, true);
						return true;
					}
					catch
					{
						result = default(T);
						return false;
					}
				}
				if ((string)value == string.Empty && toType != typeof(string))
				{
					InternalConvertTo<T>(null, cultureInfo, out result);
					return true;
				}
			}
			else
			{
				if (typeof(T) == typeof(string))
				{
					InternalConvertTo<T>(Convert.ToString(value, cultureInfo), cultureInfo, out result);
					return true;
				}
			}

			bool canConvert = toType is IConvertible || (toType.IsValueType && !toType.IsEnum);
			if (canConvert)
			{
				result = (T)Convert.ChangeType(value, toType, cultureInfo);
				return true;
			}
			result = (T)value;
			return true;
		}
		#endregion

		#region QueryType Methods.
		public static bool IsNumeric(this object theValue)
		{
			if (theValue == null) return false;

			long retNum;
			return long.TryParse(theValue.ToString(), NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out retNum);
		}
		#endregion

		#region Property Reflection Methods.

		public static VT GetPropertyValue<OT, VT>(this OT obj, String propertyName)
		{
			var propertyInfo = obj.GetType().GetProperty(propertyName);
			return (VT) Convert.ChangeType(propertyInfo.GetValue(obj), typeof(VT));
		}
		public static Object GetPropertyValue<OT>(this OT obj, String propertyName)
		{
			var propertyInfo = obj.GetType().GetProperty(propertyName);
			return propertyInfo.GetValue(obj);
		}
		public static bool GetPropertyValue<OT,VT>(this OT obj, String propertyName, out Object value)
		{
			value = null;
			var propertyInfo = obj.GetType().GetProperty(propertyName);
			if (propertyInfo == null) return false;
			value = propertyInfo.GetValue(obj);
			return value != null;
		}
		public static VT GetPropertyValue<OT, VT>(this OT obj, String propertyName, Func<Object,VT> converter) //where VT : struct
		{
			var propertyInfo = obj.GetType().GetProperty(propertyName);
			if (propertyInfo == null) return default(VT);
			var valueObj = propertyInfo.GetValue(obj);
			return converter(valueObj);
		}
		public static void SetPropertyValue<OT, VT>(this OT obj, String propertyName, VT value)
		{
			var propertyInfo = obj.GetType().GetProperty(propertyName);
			propertyInfo.SetValue(obj, Convert.ChangeType(value, propertyInfo.PropertyType), null);
		}
		#endregion

	    public static T As<T>(this object item)
	    {
	        if (item == null) return default(T);

            return (T) item;
	    }

		public static Double AsDouble(this object item,double? defaultValue=null)
		{
			try
			{
                if (item == null) return defaultValue ?? 0.0;

				return Double.Parse(item.ToString());
			}
			catch (Exception ex)
			{
				if (defaultValue != null)
					return defaultValue.Value;

				throw ex;
			}
		}
		public static Double? AsNullableDouble(this object item, double? defaultValue=null)
		{
			double value;
			if (item == null) return defaultValue;
			return Double.TryParse(item.ToString(), out value) ? (Double?)value : defaultValue;
		}

        public static Predicate<T> Or<T>(params Predicate<T>[] predicates)
        {
            return delegate (T item)
            {
                foreach (Predicate<T> predicate in predicates)
                {
                    if (predicate(item))
                    {
                        return true;
                    }
                }
                return false;
            };
        }

        public static Predicate<T> And<T>(params Predicate<T>[] predicates)
        {
            return delegate (T item)
            {
                foreach (Predicate<T> predicate in predicates)
                {
                    if (!predicate(item))
                    {
                        return false;
                    }
                }
                return true;
            };
        }
    }
}

