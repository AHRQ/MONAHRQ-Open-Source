namespace Monahrq.Sdk.Services.Contracts
{
    /// <summary>
    /// A <see cref="IDatasetWing"/> provides information about an entity such as it's Measures and Topics, Import procedure, and UI
    /// </summary>
    public interface IDatasetWing 
    {
        /// <summary>
        /// A short name for this <see cref="IDatasetWing"/>
        /// </summary>
        string Name { get; }

        /// <summary>
        /// A longer description for this <see cref="IDatasetWing"/>
        /// </summary>
        string Description { get; }
    }

    /// <summary>
    /// A <see cref="IDatasetWing"/> provides information about an entity such as it's Measures and Topics, Import procedure, and UI
    /// </summary>
    /// <typeparam name="T">The entity type described by this <see cref="IDatasetWing{T}"/></typeparam>
    public interface IDatasetWing<T> : IDatasetWing
    { }
}