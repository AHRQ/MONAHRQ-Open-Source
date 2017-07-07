using Monahrq.Infrastructure.Entities.Data.Strategies;
using Monahrq.Infrastructure.Entities.Domain.Maps;
using Monahrq.Sdk.Utilities;

namespace Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records
{
    /// <summary>
    /// Base type for Fluent NHibernate mapping for dataset record types that inherit from <see cref="DatasetRecord"/>.
    /// Provides mapping for foreign key relationship with <see cref="Dataset"/>.
    /// </summary>
    /// <typeparam name="T">A type that implements <see cref="DatasetRecord"/></typeparam>
    /// <seealso cref="EntityMap{T, Int32, IdentityGeneratedKeyStrategy}" />
    public abstract class DatasetRecordMap<T> : EntityMap<T, int, IdentityGeneratedKeyStrategy>
        where T : DatasetRecord
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatasetRecordMap{T}"/> class and defines the foreign key relationship with <see cref="Dataset"/>
        /// </summary>
        protected DatasetRecordMap()
        {
            References(m => m.Dataset, "Dataset_Id")
                .ForeignKey(string.Format("FK_TARGETS_{0}_DATASETS", Inflector.Pluralize(typeof(T).Name)))
                .Nullable()
                .Cascade.All()
                .Not.LazyLoad();
        }

        /// <summary>
        /// Names the map.
        /// </summary>
        /// <returns>null</returns>
        protected override FluentNHibernate.Mapping.PropertyPart NameMap()
        {
            return null;
        }

        /// <summary>
        /// Gets the name of the table in SQL
        /// </summary>
        public override string EntityTableName
        {
            get
            {
                //todo: should this be using typeof(T).EntityTableName()?
                return "Targets_" + Inflector.Pluralize(typeof(T).Name);
            }
        }
    }
}