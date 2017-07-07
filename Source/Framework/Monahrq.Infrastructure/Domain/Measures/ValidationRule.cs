using System;
namespace Monahrq.Infrastructure.Entities.Domain.Measures.Fields
{
    [Serializable]
    public class ValidationRule : OwnedEntity<Measure, int, int>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationRule"/> class.
        /// </summary>
        protected ValidationRule()
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationRule"/> class.
        /// </summary>
        /// <param name="field">The field.</param>
        public ValidationRule(Measure field) : base(field) 
        {}

        public virtual string PropertyName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [is required].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is required]; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsRequired { get; set; }
        /// <summary>
        /// Gets or sets the maximum length.
        /// </summary>
        /// <value>
        /// The maximum length.
        /// </value>
        public virtual int MaxLength { get; set; }
    }
}
