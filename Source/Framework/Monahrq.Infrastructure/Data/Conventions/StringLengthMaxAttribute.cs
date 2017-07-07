using System.ComponentModel.DataAnnotations;

namespace Monahrq.Infrastructure.Data.Conventions
{
    /// <summary>
    /// The custom FluentNHibernate string length attribute.
    /// </summary>
    /// <seealso cref="System.ComponentModel.DataAnnotations.StringLengthAttribute" />
    public class StringLengthMaxAttribute : StringLengthAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StringLengthMaxAttribute"/> class.
        /// </summary>
        public StringLengthMaxAttribute()
            : base(10000)
        {
            // 10000 is an arbitrary number large enough to be in the nvarchar(max) range 
        }
    }
}
