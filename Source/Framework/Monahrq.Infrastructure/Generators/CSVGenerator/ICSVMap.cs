namespace Monahrq.Infrastructure.Generators
{
    /// <summary>
    /// The csv map interface.
    /// </summary>
    interface ICSVMap {
        /// <summary>
        /// Transforms the specified property name.
        /// </summary>
        /// <param name="PropertyName">Name of the property.</param>
        /// <returns></returns>
        object Transform(string PropertyName);
    }
}
