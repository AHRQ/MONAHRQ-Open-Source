using System;
using Monahrq.Infrastructure.Entities.Domain.Wings;

namespace Monahrq.Infrastructure.Core.Attributes
{
    /// <summary>
    /// Describes a value in an enumeration type used in a Wing target
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class WingScopeValueAttribute : NamedDescriptionAttribute
    {
        /// <summary>
        /// The raw value that this <see cref="WingScopeValueAttribute"/> represents
        /// </summary>
        public object Value { get; private set; }

        public WingScopeValueAttribute(string name, object value, string description)
            : base(name, description)
        {
            this.Name = name;
            this.Value = value;
        }

        public WingScopeValueAttribute(string name, object value)
            : this(name, value, name)
        {
        }

        internal ScopeValue CreateScopeValue(Scope scope, string name)
        {
            var result = new ScopeValue(scope, name) {Value = this.Value, Description = this.Description};
            return result;
        }
    }
}