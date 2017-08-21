using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.Logging;
using Monahrq.Infrastructure.Domain;
using Monahrq.Infrastructure.Entities.Domain;

namespace Monahrq.Infrastructure.BaseDataLoader
{
    /// <summary>
    /// The base data sql bulk importer. This class utlizes the T-SQL bulk insert implementation
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <seealso cref="Monahrq.Infrastructure.BaseDataLoader.BaseDataImporter{TEntity, TKey}" />
    public abstract class BaseDataSqlBulkImporter<TEntity, TKey> : BaseDataImporter<TEntity, TKey>
            where TEntity : Entity<TKey>, new()
    {
        protected virtual BaseDataImportStrategyType ImportType { get { return BaseDataImportStrategyType.Replace; } }  // NOTE: Override in strategy if append.
        /// <summary>
        /// Gets the fileprefix.
        /// </summary>
        /// <value>
        /// The fileprefix.
        /// </value>
        protected abstract string Fileprefix { get; }
        /// <summary>
        /// Gets the format file.
        /// </summary>
        /// <value>
        /// The format file.
        /// </value>
        protected abstract string FormatFile { get; }

        /// <summary>
        /// Loads the data.
        /// </summary>
        public override void LoadData()
        {
            bool tableIndexIsOff = false;

            try
            {
                // Get list of files matching mask
                // TODO: Throw an error if the path doesn't exist?
                if (Directory.Exists(baseDataDir))
                {
                    var files = Directory.GetFiles(baseDataDir, Fileprefix + "*.csv").ToList();

                    // Turn off indexes.
                    if (files.Any() && TurnOffIndexesDuringImpport && AllowDisableIndexesDuringImport)
                    {
                        DisableTableIndexes();
                        tableIndexIsOff = true;
                    }

                    foreach (var file in files)
                    {
                        VersionStrategy.Filename = file;
                        if (!VersionStrategy.IsLoaded() &&
                            (ImportType == BaseDataImportStrategyType.Append || VersionStrategy.IsNewest()))
                        {
                            Logger.Write($"Processing base data update for type {typeof(TEntity).Name} from file {file}");

                            var rows = 0;
                            // start transaction
                            // Verify format file exists.
                            if (!File.Exists(Path.Combine(baseDataDir, FormatFile)))
                            {
                                Logger.Warning("Format file \"{0}\" missing from the base data resources directory.",
                                    FormatFile);
                                return;
                            }

                            // Verify data file exists.
                            if (!File.Exists(Path.Combine(baseDataDir, file)))
                            {
                                Logger.Warning("Import file \"{0}\" missing from the base data resources directory.",
                                    file);
                                return;
                            }

                            using (var session = DataProvider.SessionFactory.OpenStatelessSession())
                            {
                                // Turn off indexes. // Please do not remove as we may add back. Jason
                                //if (TurnOffIndexesDuringImpport && !tableIndexIsOff)
                                //{
                                //    DisableTableIndexes();
                                //    tableIndexIsOff = true;
                                //}

                                // Turncate the table if it's a replace strategy
                                if (ImportType == BaseDataImportStrategyType.Replace)
                                {
                                    using (var cmd = session.Connection.CreateCommand())
                                    {
                                        if (cmd.Connection.State == ConnectionState.Closed)
                                            cmd.Connection.Open();
                                        cmd.CommandTimeout = 900;
                                        cmd.CommandText = string.Format("TRUNCATE TABLE {0}", tableName);
                                        cmd.ExecuteNonQuery();
                                    }
                                }

                                var con = session.Connection;
                                ProvideFeedback(string.Format("Importing file {0}", file));
                                using (var cmd2 = con.CreateCommand())
                                {
                                    cmd2.CommandText = "BULK INSERT " + tableName + " FROM '" +
                                                      Path.Combine(baseDataDir, file) +
                                                      "' WITH (FIRSTROW = 2, FORMATFILE = '" +
                                                      Path.Combine(baseDataDir, FormatFile) + "')";
                                    cmd2.CommandTimeout = 6000;
                                    rows = cmd2.ExecuteNonQuery();
                                }
                            }

                            SchemaVersion version;
                            using (var session = DataProvider.SessionFactory.OpenSession())
                            {
                                // TODO: Add functionality to update existing version row in all scenarios to avoid multiple entries.- Jason
                                version = VersionStrategy.GetVersion(session);
                                session.SaveOrUpdate(version);
                                session.Flush();
                            }

                            Logger.Information(
                                $"Base data update completed for type {typeof(TEntity).Name}: {rows} rows inserted or updated; now at schema version {version}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Write(ex, "Error loading base data for type {0}", typeof(TEntity).Name);
            }
            finally
            {
                if (TurnOffIndexesDuringImpport && AllowEnableIndexesDuringImport && tableIndexIsOff)
                    Task.Factory.StartNew(() => { EnableTableIndexes(tableName); }, TaskCreationOptions.LongRunning);
            }
        }
    }
}
