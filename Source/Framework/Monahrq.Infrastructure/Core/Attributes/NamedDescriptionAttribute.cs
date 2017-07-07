using System.ComponentModel;

namespace Monahrq.Infrastructure.Core.Attributes
{
    /// <summary>
    /// Specifies a name and description for a type member
    /// </summary>
    public class NamedDescriptionAttribute : DescriptionAttribute
    {
        /// <summary>
        /// Gets the name stored in this attribute
        /// </summary>
        public string Name { get; protected set; }

        public NamedDescriptionAttribute(string name, string description)
            : base(description)
        {
            Name = name;
        }
    }
}
