using System;
using System.Linq;
using System.Reflection;
using Monahrq.Infrastructure.Entities.Domain.Wings;

namespace Monahrq.Infrastructure.Core.Attributes
{
    /// <summary>
    /// Describes an enumeration type used in a Wing Target
    /// </summary>
    [AttributeUsage(AttributeTargets.Enum)]
    public class WingScopeAttribute : NamedDescriptionAttribute
    {
        /// <summary>
        /// The CLR <see cref="Type"/> that this <see cref="WingScopeAttribute"/> describes
        /// </summary>
        public Type ClrType { get; set; }

        public WingScopeAttribute(string name, Type clrType, string description)
            : base(name, description)
        {
            this.Name = name;
            this.ClrType = clrType;
        }

        public WingScopeAttribute(string name, Type clrType)
            : this(name, clrType, name)
        {
        }

        public Scope CreateScope(Target target, Type type)
        {
            if (!type.IsEnum) throw new ArgumentException(@"Type is not an Enum", "type");
            var valueAttrs = type.GetFields()
                                 .Select(fld => fld.GetCustomAttribute<WingScopeValueAttribute>())
                                 .Where(attr => attr != null)
                                 .ToList();
            var result = new Scope(target, this.Name, type);
            valueAttrs.ForEach(v => v.CreateScopeValue(result, v.Name));
            result.Description = this.Description;
            return result;
        }
    }
}