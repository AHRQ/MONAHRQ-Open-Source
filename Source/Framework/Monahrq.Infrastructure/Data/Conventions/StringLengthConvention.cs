using System.ComponentModel.DataAnnotations;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;

namespace Monahrq.Infrastructure.Data.Conventions
{
    /// <summary>
    /// The custom FluentNHibernate string length convention.
    /// </summary>
    /// <seealso cref="FluentNHibernate.Conventions.AttributePropertyConvention{StringLengthAttribute}" />
    public class StringLengthConvention : AttributePropertyConvention<StringLengthAttribute>
    {
        /// <summary>
        /// Apply changes to a property with an attribute matching T.
        /// </summary>
        /// <param name="attribute">Instance of attribute found on property.</param>
        /// <param name="instance">Property with attribute</param>
        protected override void Apply(StringLengthAttribute attribute, IPropertyInstance instance)
        {
            instance.Length(attribute.MaximumLength);
        }
    }
}