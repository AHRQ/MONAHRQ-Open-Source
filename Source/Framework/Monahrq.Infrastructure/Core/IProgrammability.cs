using System.Reflection;

namespace Monahrq.Infrastructure.Entities.Core
{
    /// <summary>
    /// The programmability interface.
    /// </summary>
    public interface IProgrammability
    {
        /// <summary>
        /// Applies this instance.
        /// </summary>
        void Apply();
        /// <summary>
        /// Gets the version attribute.
        /// </summary>
        /// <value>
        /// The version attribute.
        /// </value>
        VersionedComponentExportAttribute VersionAttribute { get; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.Entities.Core.IProgrammability" />
    public abstract class ProgrammabilityBase : IProgrammability
    {
        /// <summary>
        /// Applies this instance.
        /// </summary>
        public abstract void Apply();

        /// <summary>
        /// Gets the version attribute.
        /// </summary>
        /// <value>
        /// The version attribute.
        /// </value>
        public virtual VersionedComponentExportAttribute VersionAttribute
        {
            get
            {
                return GetType().GetCustomAttribute<VersionedComponentExportAttribute>();
            }
        }
    }
}
