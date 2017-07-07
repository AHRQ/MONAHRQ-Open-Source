using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Monahrq.Sdk.ViewModels
{
    /// <summary>
    /// Borrowed from Jounce.Framework.ViewModel ref:codeplex
    ///     Host for a dynamically built type
    /// </summary>
    /// <remarks>
    /// If you are going to use the type in a list, you should override the
    /// equality and hash code methods
    /// </remarks>
    public class CustomType : ICustomTypeProvider
    {
        /// <summary>
        /// Indexer to properties
        /// </summary>
        /// <param name="propertyName">The name of the property</param>
        /// <returns>The value of the property</returns>
        public object this[string propertyName]
        {
            get { return GetPropertyValue(propertyName); }
            set { SetPropertyValue(propertyName, value); }
        }

        /// <summary>
        /// Type helper
        /// </summary>
        private readonly CustomTypeHelper<CustomType> _helper = new CustomTypeHelper<CustomType>();

        /// <summary>
        /// Add a property 
        /// </summary>
        /// <param name="name">The name of the property to add</param>
        public static void AddProperty(String name)
        {
            CustomTypeHelper<CustomType>.AddProperty(name);
        }

        /// <summary>
        /// Generic property add
        /// </summary>
        /// <typeparam name="T">The type to add</typeparam>
        /// <param name="name">The name of the property</param>
        public static void AddProperty<T>(string name)
        {
            AddProperty(name, typeof(T));
        }

        /// <summary>
        /// Add a property with a list of attributes
        /// </summary>
        /// <param name="name">The name of the property</param>
        /// <param name="attributes">Property attributes</param>
        public static void AddProperty<T>(string name, List<Attribute> attributes)
        {
            AddProperty(name, typeof(T), attributes);
        }

        /// <summary>
        /// Add a property with a specific type
        /// </summary>
        /// <param name="name">The name of the property</param>
        /// <param name="propertyType">The type of the property</param>
        public static void AddProperty(String name, Type propertyType)
        {
            CustomTypeHelper<CustomType>.AddProperty(name, propertyType);
        }

        /// <summary>
        /// Add a property with a list of attributes
        /// </summary>
        /// <param name="name">The name of the property</param>
        /// <param name="propertyType">The type of the property</param>
        /// <param name="attributes">Property attributes</param>
        public static void AddProperty(String name, Type propertyType, List<Attribute> attributes)
        {
            CustomTypeHelper<CustomType>.AddProperty(name, propertyType, attributes);
        }

        /// <summary>
        /// Typed value set
        /// </summary>
        /// <typeparam name="T">The type of the property</typeparam>
        /// <param name="propertyName">The name of the property</param>
        /// <param name="value">The value</param>
        /// <remarks>
        /// Use with type of <see cref="Object"/> for a non-generic add
        /// </remarks>
        public void SetPropertyValue<T>(string propertyName, T value)
        {
            _helper.SetPropertyValue(propertyName, value);
        }

        /// <summary>
        /// Get the value of the property
        /// </summary>
        /// <param name="propertyName">The name of the property</param>
        /// <returns>The value of the property</returns>
        public object GetPropertyValue(string propertyName)
        {
            return _helper.GetPropertyValue(propertyName);
        }

        /// <summary>
        /// Strong typed value fetch
        /// </summary>
        /// <typeparam name="T">The type of the property</typeparam>
        /// <param name="propertyName">The name of the property</param>
        /// <returns>The value of the property</returns>
        public T GetPropertyValue<T>(string propertyName)
        {
            return (T)_helper.GetPropertyValue(propertyName);
        }

        /// <summary>
        /// Get all properties
        /// </summary>
        /// <returns>The list of the properties</returns>
        public PropertyInfo[] GetProperties()
        {
            return _helper.GetProperties();
        }

        /// <summary>
        /// The type of the dynamic object
        /// </summary>
        /// <returns>The type</returns>
        public Type GetCustomType()
        {
            return _helper.GetCustomType();
        }
    }
}
