using Monahrq.DataSets.ViewModels.Crosswalk;
using Monahrq.Infrastructure.Entities.Domain.Wings;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Monahrq.Infrastructure.Entities.Events;
using Monahrq.Infrastructure.Extensions;

namespace Monahrq.DataSets.Model
{
    /// <summary>
    /// The dataset full wizard element mapping model.
    /// </summary>
    /// <seealso cref="Monahrq.DataSets.Model.IElementMappingModel" />
    public abstract class ElementMappingModel: IElementMappingModel
    {
        /// <summary>
        /// Guards the type of the value on.
        /// </summary>
        /// <param name="val">The value.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static object GuardValueOnType(object val, Type type)
        {
            var attemptedValue = (val ?? string.Empty).ToString();
            if (type.IsEnum)
            {
                var converter = TypeDescriptor.GetConverter(typeof(int));
                if (converter.IsValid(attemptedValue))
                {
                    val = converter.ConvertFromString(attemptedValue);
                }
                else
                {
                    converter = TypeDescriptor.GetConverter(type);
                    if (converter.IsValid(attemptedValue))
                    {
                        return converter.ConvertFromString(attemptedValue);
                    }
                }
                try
                {
                    return Enum.ToObject(type, val);
                }
                catch
                {
                    return null;
                }
            }
            else
            {
                if ((type == typeof (Int32) || type == typeof (Int64) || type == typeof (Decimal)) &&
                    !attemptedValue.IsNumeric())
                    return null;

                var converter = TypeDescriptor.GetConverter(type);
                if (converter.IsValid(attemptedValue))
                    return converter.ConvertFromString(attemptedValue);
            }
            return val;
        }

        /// <summary>
        /// Gets the element.
        /// </summary>
        /// <value>
        /// The element.
        /// </value>
        public Element Element
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets the property.
        /// </summary>
        /// <value>
        /// The property.
        /// </value>
        public PropertyInfo Property
        {
            get;
            protected set;
        }

        /// <summary>
        /// Parses the value error.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="ex">The ex.</param>
        /// <returns></returns>
        public static string ParseValueError(object value, Exception ex)
        {
            return ParseValueError(value, ex.Message);
        }

        /// <summary>
        /// Parses the value error.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public static string ParseValueError(object value, string message)
        {
            var temp = value == null || value == DBNull.Value ? "<<NULL>>"
                 : value.ToString();
            var valueStr = string.IsNullOrEmpty(temp) ? "<<EMPTY>>" : temp;
            return string.Format("Value: {0}{1}{2}", valueStr, Environment.NewLine, message);
        }

        /// <summary>
        /// Occurs when [value invalid].
        /// </summary>
        public event EventHandler<ExtendedEventArgs<ElementMappingValueException>> ValueInvalid = delegate { };
        /// <summary>
        /// Occurs when [value evaluated].
        /// </summary>
        public event EventHandler<ExtendedEventArgs<ElementMappingValueEvaluated>> ValueEvaluated = delegate { };
        /// <summary>
        /// Called when [value invalid].
        /// </summary>
        /// <param name="ex">The ex.</param>
        protected virtual void OnValueInvalid(ElementMappingValueException ex)
        {
            ValueInvalid(this, new ExtendedEventArgs<ElementMappingValueException>(ex));
        }

        /// <summary>
        /// Called when [value evaluated].
        /// </summary>
        /// <param name="elementMappingValueEvaluated">The element mapping value evaluated.</param>
        protected virtual void OnValueEvaluated(ElementMappingValueEvaluated elementMappingValueEvaluated)
        {
            ValueEvaluated(this, new ExtendedEventArgs<ElementMappingValueEvaluated>(elementMappingValueEvaluated));
        }
    }

    /// <summary>
    /// The generic mapping model.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="Monahrq.DataSets.Model.IElementMappingModel" />
    public class ElementMappingModel<T> : ElementMappingModel
    {
        /// <summary>
        /// Gets or sets the scope value values.
        /// </summary>
        /// <value>
        /// The scope value values.
        /// </value>
        private Hashtable ScopeValueValues { get; set; }

        /// <summary>
        /// Gets or sets the type of the lazy property.
        /// </summary>
        /// <value>
        /// The type of the lazy property.
        /// </value>
        Lazy<Type> LazyPropertyType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the type of the property.
        /// </summary>
        /// <value>
        /// The type of the property.
        /// </value>
        Type PropertyType
        {
            get
            {
                return LazyPropertyType.Value;
            }
        }

        /// <summary>
        /// Gets or sets the crosswalk mapping.
        /// </summary>
        /// <value>
        /// The crosswalk mapping.
        /// </value>
        private Hashtable CrosswalkMapping { get; set; }

        /// <summary>
        /// The custom crosswalk comparer.
        /// </summary>
        /// <seealso cref="Monahrq.DataSets.Model.IElementMappingModel" />
        class XWalkEqComparer : IEqualityComparer
        {
            /// <summary>
            /// Gets or sets the instance.
            /// </summary>
            /// <value>
            /// The instance.
            /// </value>
            public static XWalkEqComparer Instance { get; private set; }
            /// <summary>
            /// Initializes the <see cref="XWalkEqComparer"/> class.
            /// </summary>
            static XWalkEqComparer()
            {
                Instance = new XWalkEqComparer();
            }

            /// <summary>
            /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
            /// </summary>
            /// <param name="x">The <see cref="System.Object" /> to compare with this instance.</param>
            /// <param name="y">The y.</param>
            /// <returns>
            ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
            /// </returns>
            bool IEqualityComparer.Equals(object x, object y)
            {
                var objX = x ?? CrosswalkScopeBin.NullKey;
                var objY = y ?? CrosswalkScopeBin.NullKey;
                return objX.Equals(objY);
            }

            /// <summary>
            /// Returns a hash code for this instance.
            /// </summary>
            /// <param name="obj">The object.</param>
            /// <returns>
            /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
            /// </returns>
            public int GetHashCode(object obj)
            {
                return obj == null ? CrosswalkScopeBin.NullKey.GetHashCode()
                    : obj.GetHashCode();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ElementMappingModel{T}"/> class.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="xwalkMapping">The xwalk mapping.</param>
        /// <exception cref="ArgumentOutOfRangeException">Element</exception>
        public ElementMappingModel(Element element, IEnumerable<ICrosswalkViewModel> xwalkMapping)
        {
            Element = element;
            CrosswalkMapping = new Hashtable(XWalkEqComparer.Instance);
            foreach (var xwalk in xwalkMapping ?? Enumerable.Empty<ICrosswalkViewModel>())
            {
                var source = xwalk.Crosswalk.SourceValue ?? CrosswalkScopeBin.NullKey;
                var scope = xwalk.Crosswalk.ScopeValue.Value;
                CrosswalkMapping.Add(source, scope);
            }
            try
            {
                Property = typeof(T).GetProperty(element.Name);
                if (Property == null)
                    throw new ArgumentOutOfRangeException("Element", string.Format("Element {0} not found.", element.Name));
                else
                    InitProperty();
            }
            catch (Exception ex)
            {
                var newEx = new ElementMappingValueException(Element, ex);
                OnValueInvalid(newEx);
            }
        }

        //public static IDictionary<string, Type> PropertyTypes = new Dictionary<string, Type>();

        /// <summary>
        /// Initializes the property.
        /// </summary>
        private void InitProperty()
        {
            LazyPropertyType = new Lazy<Type>(() =>
            {
                //if (PropertyTypes.ContainsKey(Property.PropertyType.Name))
                //    return PropertyTypes[Property.PropertyType.Name];

                    //var temp = Property.PropertyType;

                if (!Property.PropertyType.IsGenericType)
                {
                    //if (!PropertyTypes.ContainsKey(temp.Name))
                    //    PropertyTypes.Add(temp.Name, temp);
                    return Property.PropertyType;
                }

                var nullable = typeof(Nullable<>).MakeGenericType(Property.PropertyType.GetGenericArguments()[0]);
                if (nullable.IsAssignableFrom(Property.PropertyType))
                {
                    //if(!PropertyTypes.ContainsKey(temp.Name))
                    //    PropertyTypes.Add(temp.Name, nullable.GetGenericArguments()[0]);

                    return nullable.GetGenericArguments()[0];
                }
                    

                return null;
            });
        }

        // find the property with the same name as element.name, and set (via reflection) that property value = given member value, or set Error
        /// <summary>
        /// Applies the value.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="value">The value.</param>
        public void ApplyValue(object target, object value)
        {
            if (Property == null)
                return;

            var xWalkValue = CrosswalkMapping.Count == 0 ? value
                  : CrosswalkMapping[value ?? CrosswalkScopeBin.NullKey];
            SetValue(target, xWalkValue);
        }

        /// <summary>
        /// Sets the nullable.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="value">The value.</param>
        private void SetNullable(object target, object value)
        {
            var val = value;
            if (val != null)
            {
                var type = Property.PropertyType.GenericTypeArguments[0];
                val = GuardValueOnType(val, type);
            }
            Property.SetValue(target, val);
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="value">The value.</param>
        private void SetValue(object target, object value)
        {
            try
            {
                var val = value == DBNull.Value ? null : value;
                if (!PropertyType.IsClass && (typeof(System.Nullable<>).MakeGenericType(PropertyType) == Property.PropertyType))
                {
                    SetNullable(target, val);
                }
                else
                {
                    val = val == null ? null
                    : GuardValueOnType(val, Property.PropertyType);
                    Property.SetValue(target, val);
                    OnValueEvaluated(new ElementMappingValueEvaluated(value, val));
                }
            }
            catch (ArgumentException ex)
            {
                var msg = string.Format("Unable to set {0} to value: {1}. The input record cannot be processed with this error.", Element.Name, value);
                var temp = new ElementMappingValueException(Element, msg, ex);
                OnValueInvalid(temp);
                OnValueEvaluated(new ElementMappingValueEvaluated(value, temp));
            }
            catch (Exception ex)
            {
                var msg = ParseValueError(value, ex);
                var temp = new ElementMappingValueException(Element, msg, ex);
                OnValueInvalid(temp);
                OnValueEvaluated(new ElementMappingValueEvaluated(value, temp));
            }
        }
    }

    /// <summary>
    /// The element mapping value evaluated entity.
    /// </summary>
    public class ElementMappingValueEvaluated
    {
        /// <summary>
        /// Gets the source.
        /// </summary>
        /// <value>
        /// The source.
        /// </value>
        public object Source { get; private set; }
        /// <summary>
        /// Gets the translation.
        /// </summary>
        /// <value>
        /// The translation.
        /// </value>
        public object Translation { get; private set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="ElementMappingValueEvaluated"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="translation">The translation.</param>
        public ElementMappingValueEvaluated(object source, object translation)
        {
            Source = source;
            Translation = translation;
        }
    }

    /// <summary>
    /// The element mapping value custom exception.
    /// </summary>
    /// <seealso cref="System.ArgumentException" />
    public class ElementMappingValueException : ArgumentException
    {
        /// <summary>
        /// Gets the element.
        /// </summary>
        /// <value>
        /// The element.
        /// </value>
        public Element Element { get; private set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="ElementMappingValueException"/> class.
        /// </summary>
        /// <param name="element">The element.</param>
        public ElementMappingValueException(Element element)
            : this(element, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ElementMappingValueException"/> class.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="ex">The ex.</param>
        public ElementMappingValueException(Element element, Exception ex)
            : base("Mapping value failed." + (ex != null ?" See inner exception.": string.Empty) , element.Name, ex)
        {
            Element = element;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ElementMappingValueException"/> class.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="msg">The MSG.</param>
        /// <param name="ex">The ex.</param>
        public ElementMappingValueException(Element element, string msg, Exception ex)
            :base(msg, "value", ex)
        {
            Element = element;
        }
    }

    /// <summary>
    /// The elementat mapper factory
    /// </summary>
    public class ElementMapperFactory
    {
        /// <summary>
        /// Gets or sets the element.
        /// </summary>
        /// <value>
        /// The element.
        /// </value>
        Element Element { get; set; }
        /// <summary>
        /// Gets or sets the type of the target.
        /// </summary>
        /// <value>
        /// The type of the target.
        /// </value>
        Type TargetType { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ElementMapperFactory"/> class.
        /// </summary>
        /// <param name="element">The element.</param>
        public ElementMapperFactory(Element element)
        {
            Element = element;
            TargetType = Type.GetType(element.Owner.ClrType);
        }

        /// <summary>
        /// Creates the specified crosswalks.
        /// </summary>
        /// <param name="crosswalks">The crosswalks.</param>
        /// <returns></returns>
        public IElementMappingModel Create(IEnumerable<ICrosswalkViewModel> crosswalks)
        {
            var xwalkType = typeof(IEnumerable<ICrosswalkViewModel>);
            var mapperType = typeof(ElementMappingModel<>).MakeGenericType(TargetType);
            var ctor = mapperType.GetConstructor(new []{typeof(Element), xwalkType});
            return ctor.Invoke(new object[] { Element, crosswalks }) as IElementMappingModel;
        }
    }
}