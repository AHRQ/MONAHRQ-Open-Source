using System;
using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records;
using Monahrq.Infrastructure.Entities.Domain.Wings;

namespace Monahrq.Infrastructure.Core.Attributes
{
    /// <summary>
    /// Describes a column in a Wing target (a <see cref="DatasetRecord"/> implementation)
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class WingTargetElementAttribute : NamedDescriptionAttribute
    {
        /// <summary>
        /// Gets a value indicating whether the property described by this attribute is mandatory
        /// </summary>
        public bool IsRequired { get; private set; }

        /// <summary>
        /// Additional description for the property described by this attribute
        /// </summary>
        public string LongDescription { get; private set; }

        //public Type DataType { get; private set; }

        /// <summary>
        /// Defines the column index of the property described by this attribute in data sets and 
        /// </summary>
        public int Ordinal { get; private set; }

        /// <param name="name">The name of the property that this attribute decorates</param>
        public WingTargetElementAttribute(
            string name,
            string description,
            bool isRequired,
            int ordinal)
            : this(name, description, isRequired, ordinal, description)
        {
        }

        /// <param name="name">The name of the property that this attribute decorates</param>
        public WingTargetElementAttribute(
            string name,
            string description,
            bool isRequired,
            int ordinal,
            string longDecription)
            : base(name, description)
        {
            this.Ordinal = ordinal;
            this.IsRequired = isRequired;
            this.LongDescription = longDecription;
        }

        /// <param name="name">The name of the property that this attribute decorates</param>
        public WingTargetElementAttribute(string name, bool isRequired, int ordinal)
            : this(name, name, isRequired, ordinal)
        {
        }

        public Element CreateElement(Target owner)
        {
            var result = new Element(owner, this.Name)
            {
                Description = this.Description,
                LongDescription = this.LongDescription,
                IsRequired = this.IsRequired,
                Ordinal = this.Ordinal
            };
            return result;
        }
    }
}