using System;
using System.Reflection;

namespace Monahrq.Sdk.Modules.Wings
{
    /// <summary>
    /// The custom wing exception.
    /// </summary>
    /// <seealso cref="System.Exception" />
    public abstract class WingException : Exception { }

    /// <summary>
    /// The custom wing target element attribute exception.
    /// </summary>
    /// <seealso cref="Monahrq.Sdk.Modules.Wings.WingException" />
    public class WingTargetElementAttributeException : WingException
    {
        /// <summary>
        /// Gets the element property.
        /// </summary>
        /// <value>
        /// The element property.
        /// </value>
        public PropertyInfo ElementProperty { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WingTargetElementAttributeException"/> class.
        /// </summary>
        /// <param name="prop">The property.</param>
        public WingTargetElementAttributeException(PropertyInfo prop)
        {
            ElementProperty = prop;
        }

        /// <summary>
        /// Gets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public override string Message
        {
            get
            {
                return "Target element property not properly attributed";
            }
        }
    }

    /// <summary>
    /// The custom wing target attribute exception.
    /// </summary>
    /// <seealso cref="Monahrq.Sdk.Modules.Wings.WingException" />
    public class WingTargetAttributeException : WingException
    {
        /// <summary>
        /// Gets the type of the target.
        /// </summary>
        /// <value>
        /// The type of the target.
        /// </value>
        public Type TargetType { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WingTargetAttributeException"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        public WingTargetAttributeException(Type type)
        {
            this.TargetType = type;
        }
        /// <summary>
        /// Gets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public override string Message
        {
            get
            {
                return "Target class not properly attributed";
            }
        }
    }

    /// <summary>
    /// The invalid wing assembly custom exception.
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class InvalidWingAssemblyException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidWingAssemblyException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public InvalidWingAssemblyException(string message) : base(message) { }
    }

    /// <summary>
    /// The invalid wing module custom exception.
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class InvalidWingModuleException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidWingModuleException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public InvalidWingModuleException(string message) : base(message) { }
    }
}
