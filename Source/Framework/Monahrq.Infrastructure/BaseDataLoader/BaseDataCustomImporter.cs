using Monahrq.Infrastructure.Entities.Domain;

namespace Monahrq.Infrastructure.BaseDataLoader
{
    /// <summary>
    /// The abstract base data custom importer class. Override the LoadData() method in the strategy to have it called. 
    /// Everything must be handled in the strategy.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <seealso cref="Monahrq.Infrastructure.BaseDataLoader.BaseDataImporter{TEntity, TKey}" />
    public abstract class BaseDataCustomImporter<TEntity, TKey> : BaseDataImporter<TEntity, TKey>
        where TEntity : Entity<TKey>, new()
    {
        /// <summary>
        /// Loads the data.
        /// </summary>
        public override void LoadData() { }
    }
}
