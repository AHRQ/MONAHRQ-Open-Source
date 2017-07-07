using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.Logging;
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
                            // start transaction
                            // Verify format file exists.
                            if (!File.Exists(Path.Combine(baseDataDir, FormatFile)))
                            {
                                Logger.Log(string.Format("Format file \"{0}\" missing from the base data resources directory.", FormatFile), Category.Exception, Priority.Medium);
                                return;
                            }

                            // Verify data file exists.
                            if (!File.Exists(Path.Combine(baseDataDir, file)))
                            {
                                Logger.Log(string.Format("Import file \"{0}\" missing from the base data resources directory.", file), Category.Exception, Priority.Medium);
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
                                using (var cmd = con.CreateCommand())
                                {
                                    cmd.CommandText = "BULK INSERT " + tableName + " FROM '" +
                                                      Path.Combine(baseDataDir, file) +
                                                      "' WITH (FIRSTROW = 2, FORMATFILE = '" +
                                                      Path.Combine(baseDataDir, FormatFile) + "')";
                                    cmd.CommandTimeout = 6000;
                                    cmd.ExecuteNonQuery();
                                }
                            }

                            using (var session = DataProvider.SessionFactory.OpenSession())
                            {
                                // TODO: Add functionality to update existing version row in all scenarios to avoid multiple entries.- Jason
                                var version = VersionStrategy.GetVersion(session);

                                session.SaveOrUpdate(version);
                                session.Flush();
                            }
                            
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString(), Category.Exception, Priority.High);
            }
            finally
            {
                if (TurnOffIndexesDuringImpport && AllowEnableIndexesDuringImport && tableIndexIsOff)
                    Task.Factory.StartNew(() => { EnableTableIndexes(tableName); }, TaskCreationOptions.LongRunning);
            }
        }
    }
}
