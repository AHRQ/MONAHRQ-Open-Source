using Monahrq.Infrastructure.Core.Attributes;
namespace Monahrq.Sdk.Modules.Wings
{
    /// <summary>
    /// The wing module interface.
    /// </summary>
    public interface IWingModule
    {
        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        string Description { get; }
        /// <summary>
        /// Gets the unique identifier.
        /// </summary>
        /// <value>
        /// The unique identifier.
        /// </value>
        string Guid { get; }
    }

    /// <summary>
    /// The targeted wing module interface.
    /// </summary>
    /// <seealso cref="Monahrq.Sdk.Modules.Wings.IWingModule" />
    public interface ITargetedModuleBase : IWingModule
    {
        /// <summary>
        /// Gets the target attribute.
        /// </summary>
        /// <value>
        /// The target attribute.
        /// </value>
        WingTargetAttribute TargetAttribute { get; }
    }
}
