using System;

namespace Monahrq.Infrastructure.Core.Attributes
{
    /// <summary>
    /// Describes a Wing assembly
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    public class WingAssemblyAttribute : NamedDescriptionAttribute
    {
        /// <summary>
        /// Unique <see cref="Guid"/> that will be used to refer to this Wing assembly throughout MONAHRQ
        /// </summary>
        public Guid Id { get; private set; }

        public WingAssemblyAttribute(string id, string name, string description)
            : base(name, description)
        {
            this.Id = Guid.Parse(id);
            this.Name = name;
        }
    }
}