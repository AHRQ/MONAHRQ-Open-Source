using System;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.Logging;
using Monahrq.Infrastructure.Domain;
using Monahrq.Infrastructure.Entities.Domain;


namespace Monahrq.Infrastructure.BaseDataLoader
{
    /// <summary>
    /// The abstract base datareader importer class. Imports base data via ado.net datareader and passes of to <see cref="BulkInsert{T, TKey}"/> 
    /// to insert via <see cref="SqlBulkCopy"/> functionality.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <seealso cref="Monahrq.Infrastructure.BaseDataLoader.BaseDataImporter{TEntity, TKey}" />
    public abstract class BaseDataDataReaderImporter<TEntity, TKey> : BaseDataImporter<TEntity, TKey>
        where TEntity : Entity<TKey>, new()
    {
        protected virtual BaseDataImportStrategyType ImportType { get { return BaseDataImportStrategyType.Replace; } }  // NOTE: Override in strategy if append.
        protected abstract string Fileprefix { get; }

        /// <summary>
        /// Pres the process file.
        /// </summary>
        /// <param name="files">The files.</param>
        protected virtual void PreProcessFile(ref string[] files)
        {}

        /// <summary>
        /// Loads the data.
        /// </summary>
        public override void LoadData()
        {
            try
            {
                var tableIndexIsOff = false;
                
                // Get list of files matching mask
                // TODO: Throw an error if the path doesn't exist?
                if (Directory.Exists(baseDataDir))
                {
                    var files = Directory.GetFiles(baseDataDir, Fileprefix + "-*.csv");
                    PreProcessFile(ref files);
                    foreach (var file in files)
                    {
                        VersionStrategy.Filename = file;
                        if (!VersionStrategy.IsLoaded() &&
                            (ImportType == BaseDataImportStrategyType.Append || VersionStrategy.IsNewest()))
                        {
                            // start transaction
                            Logger.Write($"Processing base data update for type {typeof(TEntity).Name} from file {file}");
                            var rows = 0;

                            // Verify data file exists.
                            if (!File.Exists(Path.Combine(baseDataDir, file)))
                            {
                                Logger.Warning("Import file \"{0}\" missing from the base data resources directory.", file);
                                return;
                            }

                            using (var session = DataProvider.SessionFactory.OpenStatelessSession())
                            {
                                // Turn off indexes.
                                if (TurnOffIndexesDuringImpport && !tableIndexIsOff)
                                {
                                    DisableTableIndexes();
                                    tableIndexIsOff = true;
                                }

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

                                var builder = new OleDbConnectionStringBuilder();
                                builder.Provider = "Microsoft.ACE.OLEDB.12.0";
                                builder.DataSource = baseDataDir;
                                builder["Extended Properties"] = "text;HDR=YES;FMT=Delimited";
                                
                                using (var conn = new OleDbConnection(builder.ConnectionString))
                                {
                                    conn.Open();
                                    var sql = string.Format("SELECT * FROM [{0}]", Path.GetFileName(file));
                                    using (var cmd = new OleDbCommand(sql, conn))
                                    {
                                        var reader = cmd.ExecuteReader();
                                        using (var bulkImporter = new BulkInsert<TEntity, TKey>(session.Connection))
                                        {
                                            bulkImporter.ConnectionRequested += (o, e) => e.Data = session.Connection as SqlConnection;
                                            bulkImporter.Prepare();
                                            bulkImporter.BatchSize = 1000;  // TODO: Parameterize?

                                            if (reader == null)
                                            {
                                                Logger.Warning(
                                                    "A problem occurred while trying to read CSV file \"{0}\". Please make sure that the file is properly formated and has data.",
                                                    file);
                                                return;
                                            }

                                            if (reader.HasRows)
                                            {
                                                while (reader.Read())
                                                {
                                                    var temp = LoadFromReader(reader);
                                                    if (temp != null)
                                                    {
                                                        bulkImporter.Insert(temp);
                                                        rows++;
                                                    }
                                                }
                                            }
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

                            // commit transaction
                            Logger.Information(
                                $"Base data update completed for type {typeof(TEntity).Name}: {rows} rows inserted or updated; now at schema version {version}");
                        }
                    }

                    if (TurnOffIndexesDuringImpport && tableIndexIsOff)
                        Task.Factory.StartNew(() => EnableTableIndexes(tableName), TaskCreationOptions.LongRunning);
                }
            }
            catch (Exception ex)
            {
                Logger.Write(ex, "Error loading base data for entity {0}", typeof(TEntity).Name);
            }
        }
    }
}
