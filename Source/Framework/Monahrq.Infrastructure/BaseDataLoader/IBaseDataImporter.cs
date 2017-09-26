using System.Data;
using Monahrq.Infrastructure.Entities.Domain;

namespace Monahrq.Infrastructure.BaseDataLoader
{
    /// <summary>
    /// The genric base data importer imterface.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <seealso cref="Monahrq.Infrastructure.BaseDataLoader.IBasedataImporter" />
    public interface IBaseDataImporter<TEntity, TKey> : IBasedataImporter
        where TEntity : class, IEntity<TKey>
    {
        /// <summary>
        /// Loads from reader.
        /// </summary>
        /// <param name="rdr">The RDR.</param>
        /// <returns></returns>
        TEntity LoadFromReader(IDataReader rdr);
    }

    /// <summary>
    /// The base data importer imterface.
    /// </summary>
    public interface IBasedataImporter
    {
        /// <summary>
        /// Gets the loader priority.
        /// </summary>
        /// <value>
        /// The loader priority.
        /// </value>
        int LoaderPriority { get; }   // NOTE: Defaults to 10. Override to a lower number for load order.
        /// <summary>
        /// Gets or sets the version strategy.
        /// </summary>
        /// <value>
        /// The version strategy.
        /// </value>
        BaseDataVersionStrategy VersionStrategy { get; set; }
        /// <summary>
        /// Gets the name of the database table.
        /// </summary>
        /// <value>
        /// The name of the database table.
        /// </value>
        string DatabaseTableName { get; }
        /// <summary>
        /// Pres the load data.
        /// </summary>
        void PreLoadData();
        /// <summary>
        /// Loads the data.
        /// </summary>
        void LoadData();
        /// <summary>
        /// Posts the load data.
        /// </summary>
        void PostLoadData();
        /// <summary>
        /// Gets the loader description.
        /// </summary>
        /// <value>
        /// The loader description.
        /// </value>
        string LoaderDescription { get; }   // NOTE: Defaults to the entity name titlized and pluralized. Override for a better display name.
        /// <summary>
        /// Gets a value indicating whether [turn off indexes during impport].
        /// </summary>
        /// <value>
        /// <c>true</c> if [turn off indexes during impport]; otherwise, <c>false</c>.
        /// </value>
        bool TurnOffIndexesDuringImpport { get; }
    }

    /// <summary>
    /// The base data import strategy type enumeration.
    /// </summary>
    public enum BaseDataImportStrategyType
    {
        /// <summary>
        /// Deletes all old data before importing the newest data
        /// </summary>
        Replace,
        /// <summary>
        /// Appends new data to existing data
        /// </summary>
        Append
    }
}