using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Monahrq.Sdk.ViewModels
{
    /// <summary>
    /// //borrowed from Jounce  aka Wintellect the guy who is working on Time Machine. I like him :) 
    ///  Helper for dynamic, bindable types
    /// </summary>
    /// <typeparam name="T">The type</typeparam>
    /// <remarks>
    /// This code is based on the blog post by Microsoft Silverlight team member Alexandra Rusina at:
    /// http://blogs.msdn.com/b/silverlight_sdk/archive/2011/04/25/binding-to-dynamic-properties-with-icustomtypeprovider-silverlight-5-beta.aspx
    /// </remarks>
    public class CustomTypeHelper<T> : ICustomTypeProvider, INotifyPropertyChanged
    {
        private static readonly List<CustomPropertyInfoHelper> _customProperties = new List<CustomPropertyInfoHelper>();
        private readonly Dictionary<string, object> _customPropertyValues;
        private CustomTypeDelegate _ctype;

        /// <summary>
        ///     Property changed event
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public CustomTypeHelper()
        {
            _customPropertyValues = new Dictionary<string, object>();
            foreach (var property in GetCustomType().GetProperties())
            {
                _customPropertyValues.Add(property.Name, null);
            }
        }

        /// <summary>
        /// Add a new string property to the dynamic type
        /// </summary>
        /// <param name="name">The name of the property</param>
        public static void AddProperty(string name)
        {
            if (!CheckIfNameExists(name))
                _customProperties.Add(new CustomPropertyInfoHelper(name, typeof(String)));
        }

        /// <summary>
        /// Add a new property of a specific type
        /// </summary>
        /// <param name="name">The name of the property</param>
        /// <param name="propertyType">The type of the property</param>
        public static void AddProperty(string name, Type propertyType)
        {
            if (!CheckIfNameExists(name))
                _customProperties.Add(new CustomPropertyInfoHelper(name, propertyType));
        }

        /// <summary>
        /// Add a new property with a list of attributes
        /// </summary>
        /// <param name="name">The name of the property</param>
        /// <param name="propertyType">The type of the property</param>
        /// <param name="attributes">The attributes associated with the property</param>
        public static void AddProperty(string name, Type propertyType, List<Attribute> attributes)
        {
            if (!CheckIfNameExists(name))
                _customProperties.Add(new CustomPropertyInfoHelper(name, propertyType, attributes));
        }

        private static bool CheckIfNameExists(string name)
        {
            if ((from p in _customProperties select p.Name).Contains(name) ||
                (from p in typeof(T).GetProperties() select p.Name).Contains(name))
                throw new Exception("The property with this name already exists: " + name);
            return false;
        }

        /// <summary>
        /// Set the value of the property
        /// </summary>
        /// <param name="propertyName">The name of the property</param>
        /// <param name="value">The value of the property</param>
        /// <exception cref="Exception">Exceptions thrown if the property is not valid or incorrect type</exception>
        public void SetPropertyValue(string propertyName, object value)
        {
            var propertyInfo =
                (from prop in _customProperties where prop.Name == propertyName select prop).FirstOrDefault();

            if (!_customPropertyValues.ContainsKey(propertyName))
                throw new Exception("There is no property " + propertyName);

            if (propertyInfo == null)
            {
                throw new Exception("There is no property " + propertyName);
            }

            if (!ValidateValueType(value, propertyInfo.Type))
                throw new Exception("Value is of the wrong type or null for a non-nullable type.");

            if (_customPropertyValues[propertyName] != value)
            {
                _customPropertyValues[propertyName] = value;
                NotifyPropertyChanged(propertyName);
            }
        }

        private static bool ValidateValueType(object value, Type type)
        {
            if (value == null)
                // Non-value types can be assigned null.
                if (!type.IsValueType)
                    return true;
                else
                    // Check if the type if a Nullable type.
                    return (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>));
            return type.IsInstanceOfType(value);
        }

        /// <summary>
        /// Get the value of a property
        /// </summary>
        /// <param name="propertyName">The name of the property</param>
        /// <returns>The value of the property</returns>
        /// <exception cref="Exception">Exception if the name is incorrect</exception>
        public object GetPropertyValue(string propertyName)
        {
            if (_customPropertyValues.ContainsKey(propertyName))
                return _customPropertyValues[propertyName];
            throw new Exception("There is no property " + propertyName);
        }

        /// <summary>
        /// Get the list of all properties
        /// </summary>
        /// <returns>The list of properties</returns>
        public PropertyInfo[] GetProperties()
        {
            return GetCustomType().GetProperties();
        }

        /// <summary>
        /// Get the type for the property
        /// </summary>
        /// <returns>The custom type</returns>
        public Type GetCustomType()
        {
            return _ctype ?? (_ctype = new CustomTypeDelegate(typeof(T)));
        }

        private class CustomTypeDelegate : Type
        {
            private readonly Type _baseType;

            public CustomTypeDelegate(Type delegatingType)
            {
                _baseType = delegatingType;
            }

            public override Assembly Assembly
            {
                get { return _baseType.Assembly; }
            }

            public override string AssemblyQualifiedName
            {
                get { return _baseType.AssemblyQualifiedName; }
            }

            public override Type BaseType
            {
                get { return _baseType.BaseType; }
            }

            public override string FullName
            {
                get { return _baseType.FullName; }
            }

            public override Guid GUID
            {
                get { return _baseType.GUID; }
            }

            protected override TypeAttributes GetAttributeFlagsImpl()
            {
                throw new NotImplementedException();
            }

            protected override ConstructorInfo GetConstructorImpl(BindingFlags bindingAttr, Binder binder,
                                                                  CallingConventions callConvention, Type[] types,
                                                                  ParameterModifier[] modifiers)
            {
                throw new NotImplementedException();
            }

            public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr)
            {
                return _baseType.GetConstructors(bindingAttr);
            }

            public override Type GetElementType()
            {
                return _baseType.GetElementType();
            }

            public override EventInfo GetEvent(string name, BindingFlags bindingAttr)
            {
                return _baseType.GetEvent(name, bindingAttr);
            }

            public override EventInfo[] GetEvents(BindingFlags bindingAttr)
            {
                return _baseType.GetEvents(bindingAttr);
            }

            public override FieldInfo GetField(string name, BindingFlags bindingAttr)
            {
                return _baseType.GetField(name, bindingAttr);
            }

            public override FieldInfo[] GetFields(BindingFlags bindingAttr)
            {
                return _baseType.GetFields(bindingAttr);
            }

            public override Type GetInterface(string name, bool ignoreCase)
            {
                return _baseType.GetInterface(name, ignoreCase);
            }

            public override Type[] GetInterfaces()
            {
                return _baseType.GetInterfaces();
            }

            public override MemberInfo[] GetMembers(BindingFlags bindingAttr)
            {
                return _baseType.GetMembers(bindingAttr);
            }

            protected override MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder,
                                                        CallingConventions callConvention, Type[] types,
                                                        ParameterModifier[] modifiers)
            {
                throw new NotImplementedException();
            }

            public override MethodInfo[] GetMethods(BindingFlags bindingAttr)
            {
                return _baseType.GetMethods(bindingAttr);
            }

            public override Type GetNestedType(string name, BindingFlags bindingAttr)
            {
                return _baseType.GetNestedType(name, bindingAttr);
            }

            public override Type[] GetNestedTypes(BindingFlags bindingAttr)
            {
                return _baseType.GetNestedTypes(bindingAttr);
            }

            public override PropertyInfo[] GetProperties(BindingFlags bindingAttr)
            {
                var clrProperties = _baseType.GetProperties(bindingAttr);

                if (_customProperties == null)
                {
                    return null;
                }

                return clrProperties != null

                           ? clrProperties.Concat(_customProperties).ToArray()
                    // ReSharper disable CoVariantArrayConversion
                           : _customProperties.ToArray();
                // ReSharper restore CoVariantArrayConversion
            }

            protected override PropertyInfo GetPropertyImpl(string name, BindingFlags bindingAttr, Binder binder,
                                                            Type returnType, Type[] types, ParameterModifier[] modifiers)
            {
                // Look for the CLR property with this name first.
                var propertyInfo =
                    (from prop in GetProperties(bindingAttr) where prop.Name == name select prop).FirstOrDefault() ??
                    (from prop in _customProperties where prop.Name == name select prop).FirstOrDefault();
                return propertyInfo;
            }

            protected override bool HasElementTypeImpl()
            {
                throw new NotImplementedException();
            }

            public override object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target,
                                                object[] args, ParameterModifier[] modifiers,
                                                System.Globalization.CultureInfo culture, string[] namedParameters)
            {
                return _baseType.InvokeMember(name, invokeAttr, binder, target, args, modifiers, culture,
                                              namedParameters);
            }

            protected override bool IsArrayImpl()
            {
                throw new NotImplementedException();
            }

            protected override bool IsByRefImpl()
            {
                throw new NotImplementedException();
            }

            protected override bool IsCOMObjectImpl()
            {
                throw new NotImplementedException();
            }

            protected override bool IsPointerImpl()
            {
                throw new NotImplementedException();
            }

            protected override bool IsPrimitiveImpl()
            {
                return _baseType.IsPrimitive;
            }

            public override Module Module
            {
                get { return _baseType.Module; }
            }

            public override string Namespace
            {
                get { return _baseType.Namespace; }
            }

            public override Type UnderlyingSystemType
            {
                get { return _baseType.UnderlyingSystemType; }
            }

            public override object[] GetCustomAttributes(Type attributeType, bool inherit)
            {
                return _baseType.GetCustomAttributes(attributeType, inherit);
            }

            public override object[] GetCustomAttributes(bool inherit)
            {
                return _baseType.GetCustomAttributes(inherit);
            }

            public override bool IsDefined(Type attributeType, bool inherit)
            {
                return _baseType.IsDefined(attributeType, inherit);
            }

            public override string Name
            {
                get { return _baseType.Name; }
            }
        }

        // Custom implementation of the PropertyInfo
        private class CustomPropertyInfoHelper : PropertyInfo
        {
            private readonly string _name;
            public readonly Type Type;
            private readonly List<Attribute> _attributes = new List<Attribute>();

            public CustomPropertyInfoHelper(string name, Type type)
            {
                _name = name;
                Type = type;
            }

            public CustomPropertyInfoHelper(string name, Type type, List<Attribute> attributes)
            {
                _name = name;
                Type = type;
                _attributes = attributes;
            }

            public override PropertyAttributes Attributes
            {
                get { throw new NotImplementedException(); }
            }

            public override bool CanRead
            {
                get { return true; }
            }

            public override bool CanWrite
            {
                get { return true; }
            }

            public override MethodInfo[] GetAccessors(bool nonPublic)
            {
                throw new NotImplementedException();
            }

            public override MethodInfo GetGetMethod(bool nonPublic)
            {
                throw new NotImplementedException();
            }

            public override ParameterInfo[] GetIndexParameters()
            {
                throw new NotImplementedException();
            }

            public override MethodInfo GetSetMethod(bool nonPublic)
            {
                throw new NotImplementedException();
            }

            // Returns the value from the dictionary stored in the Customer's instance.
            public override object GetValue(object obj, BindingFlags invokeAttr, Binder binder, object[] index,
                                            System.Globalization.CultureInfo culture)
            {
                // ReSharper disable CoVariantArrayConversion
                return obj.GetType().GetMethod("GetPropertyValue").Invoke(obj, new[] { _name });
                // ReSharper restore CoVariantArrayConversion
            }

            public override Type PropertyType
            {
                get { return Type; }
            }

            // Sets the value in the dictionary stored in the Customer's instance.
            public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder,
                                          object[] index, System.Globalization.CultureInfo culture)
            {
                obj.GetType().GetMethod("SetPropertyValue").Invoke(obj, new[] { _name, value });
            }

            public override Type DeclaringType
            {
                get { throw new NotImplementedException(); }
            }

            public override object[] GetCustomAttributes(Type attributeType, bool inherit)
            {
                var attrs = from a in _attributes where a.GetType() == attributeType select a;
                // ReSharper disable CoVariantArrayConversion
                return attrs.ToArray();
                // ReSharper restore CoVariantArrayConversion
            }

            public override object[] GetCustomAttributes(bool inherit)
            {
                // ReSharper disable CoVariantArrayConversion
                return _attributes.ToArray();
                // ReSharper restore CoVariantArrayConversion
            }

            public override bool IsDefined(Type attributeType, bool inherit)
            {
                throw new NotImplementedException();
            }

            public override string Name
            {
                get { return _name; }
            }

            public override Type ReflectedType
            {
                get { throw new NotImplementedException(); }
            }
        }
    }
}
