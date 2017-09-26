using System;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Practices.Prism.Logging;
using Monahrq.Infrastructure.Domain;
using Monahrq.Infrastructure.Entities.Domain;

namespace Monahrq.Infrastructure.BaseDataLoader
{
    /// <summary>
    /// The abstract base data enumeration importer. Imports base data that's based off of custom enumerations.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TEnum">The type of the enum.</typeparam>
    /// <seealso cref="Monahrq.Infrastructure.BaseDataLoader.BaseDataImporter{TEntity, TKey}" />
    public abstract class BaseDataEnumImporter<TEntity, TKey, TEnum> : BaseDataImporter<TEntity, TKey>
        where TEntity : Entity<TKey>, new()
    {
        /// <summary>
        /// Gets the entity.
        /// </summary>
        /// <param name="val">The value.</param>
        /// <returns></returns>
        public abstract TEntity GetEntity(object val);

        /// <summary>
        /// Loads the data.
        /// </summary>
        public override void LoadData()
        {
            try
            {
                if (!VersionStrategy.IsLoaded() && VersionStrategy.IsNewest())
                {
                    Logger.Information($"Processing base data update for type {typeof(TEntity).Name}");

                    var rows = 0;
                    // start transaction
                    using (var session = DataProvider.SessionFactory.OpenStatelessSession())
                    {
                        // Always turncate for enum types
                        using (var cmd = session.Connection.CreateCommand())
                        {
                            if (cmd.Connection.State == ConnectionState.Closed)
                                cmd.Connection.Open();
                            cmd.CommandTimeout = 900;
                            cmd.CommandText = string.Format("TRUNCATE TABLE {0}", tableName);
                            cmd.ExecuteNonQuery();
                        }

                        // Loop through values in the enum.
                        using (var bulkImporter = new BulkInsert<TEntity, TKey>(session.Connection))
                        {
                            bulkImporter.ConnectionRequested += (o, e) =>
                            {
                                e.Data = session.Connection as SqlConnection;
                            };
                            bulkImporter.Prepare();
                            bulkImporter.BatchSize = 1000; 

                            Array values = Enum.GetValues(typeof(TEnum));
                            foreach (var val in values)
                            {
                                var temp = GetEntity(val);
                                if (temp != null)
                                {
                                    bulkImporter.Insert(temp);
                                    rows++;
                                }
                            }
                        }
                    }

                    SchemaVersion version;
                    using (var session = DataProvider.SessionFactory.OpenSession())
                    {
                        version = VersionStrategy.GetVersion(session);
                        session.SaveOrUpdate(version);
                        session.Flush();
                    }

                    Logger.Information($"Base data update completed for type {typeof(TEntity).Name}: {rows} rows inserted or updated; now at schema version {version}");
                }
            }
            catch (Exception ex)
            {
                Logger.Write(ex, "Error importing {0} enumeration values for data type {1}", typeof(TEnum).Name,
                    typeof(TEntity).Name);
            }
        }
    }
}
