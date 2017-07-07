using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.Practices.ServiceLocation;
using Monahrq.Infrastructure.Configuration;
using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Entities.Domain.Wings;
using Monahrq.Infrastructure.Entities.Events;
using Monahrq.Infrastructure.Utility;

namespace Monahrq.Infrastructure.BaseDataLoader
{
    /// <summary>
    /// The bulk insert class. Performs bulk insert operations utilizing the <see cref="SqlBulkCopy"/> implementation.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <seealso cref="System.IDisposable" />
    public class BulkInsert<T, TKey> : IDisposable
        where T : Entity<TKey>
    {
        /// <summary>
        /// Gets or sets the items.
        /// </summary>
        /// <value>
        /// The items.
        /// </value>
        List<T> Items { get; set; }
        /// <summary>
        /// Gets or sets the ops logger.
        /// </summary>
        /// <value>
        /// The ops logger.
        /// </value>
        ILogWriter OpsLogger { get; set; }
        /// <summary>
        /// Gets or sets the session logger.
        /// </summary>
        /// <value>
        /// The session logger.
        /// </value>
        ILogWriter SessionLogger { get; set; }
        /// <summary>
        /// Gets or sets the mapper.
        /// </summary>
        /// <value>
        /// The mapper.
        /// </value>
        IBulkMapper Mapper { get; set; }
        /// <summary>
        /// Gets or sets the configuration service.
        /// </summary>
        /// <value>
        /// The configuration service.
        /// </value>
        IConfigurationService ConfigService { get; set; }
        /// <summary>
        /// Occurs when [connection requested].
        /// </summary>
        public event EventHandler<ExtendedEventArgs<SqlConnection>> ConnectionRequested = delegate { };
        /// <summary>
        /// Gets or sets the size of the batch.
        /// </summary>
        /// <value>
        /// The size of the batch.
        /// </value>
        public int BatchSize { get; set; }
        /// <summary>
        /// Gets or sets the timeout.
        /// </summary>
        /// <value>
        /// The timeout.
        /// </value>
        public int Timeout { get; set; }

        /// <summary>
        /// Gets or sets the target.
        /// </summary>
        /// <value>
        /// The target.
        /// </value>
        public Target Target { get; set; }

        /// <summary>
        /// Inserts the specified instance.
        /// </summary>
        /// <param name="instance">The instance.</param>
        public void Insert(T instance)
        {
            if (instance == null) return;

            if (Mapper == null)
            {
                Mapper = instance.CreateBulkInsertMapper(TargetTableSource, instance, Target);
            }

            if (CurrentTargetTable == null)
            {
                CurrentTargetTable = TargetTableSource.Clone();
            }

            //if (instance is ICDCodedTarget)
            {
                instance.CleanBeforeSave();
            }

            var row = CurrentTargetTable.NewRow();
            row.ItemArray = Mapper.InstanceValues(instance).ToArray();

            var datasetRecord = instance as DatasetRecord;
            if (datasetRecord != null && (row[typeof(Dataset).Name + "_Id"] == DBNull.Value || row[typeof(Dataset).Name + "_Id"] == null))
            {
                row[typeof(Dataset).Name + "_Id"] = datasetRecord.Dataset.Id;
            }
            
            CurrentTargetTable.Rows.Add(row);

            if (CurrentTargetTable.Rows.Count == BatchSize)
            {
                Flush();
            }
        }
        /// <summary>
        /// Flushes this instance.
        /// </summary>
        public void Flush()
        {
            if (CurrentTargetTable == null) return;

            try
            {
                //using (var connection = RequestConnection())
                using (var connection = new SqlConnection(ConfigService.ConnectionSettings.ConnectionString))
                {
                    connection.Open();
                    using (var bcp = new SqlBulkCopy(connection, SqlBulkCopyOptions.TableLock, null))
                    //using (var bcp = new SqlBulkCopy(connection))
                    {
                        bcp.BatchSize = CurrentTargetTable.Rows.Count < 1000 ? CurrentTargetTable.Rows.Count : BatchSize; 
                        bcp.BulkCopyTimeout = Timeout;
                        bcp.DestinationTableName = TargetTableName;
                        foreach (var mapping in ColumnMappings.OfType<SqlBulkCopyColumnMapping>())
                        {
                            bcp.ColumnMappings.Add(mapping);
                        }
                        bcp.WriteToServer(CurrentTargetTable);
                    }
                    connection.Close();
                }
            }
            catch (Exception exc)
            {
                OpsLogger.Write(exc);
                throw exc;
            }
            finally
            {
                CurrentTargetTable = TargetTableSource.Clone();
            }
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="BulkInsert{T, TKey}" /> class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="target">The target.</param>
        /// <param name="batchSize">Size of the batch.</param>
        public BulkInsert(IDbConnection connection, Target target = null, int? batchSize = null)
        {
            ConfigService = ServiceLocator.Current.GetInstance<IConfigurationService>();

            OpsLogger = ServiceLocator.Current.GetInstance<ILogWriter>(LogNames.Operations);
            SessionLogger = ServiceLocator.Current.GetInstance<ILogWriter>(LogNames.Session);
            BatchSize = batchSize ?? ConfigService.MonahrqSettings.BatchSize;
            Timeout = (int)Math.Round(ConfigService.MonahrqSettings.LongTimeout.TotalSeconds);

            Target = target;
        }
        /// <summary>
        /// Prepares this instance.
        /// </summary>
        public void Prepare()
        {
            InitSourceTable();
            InitBulkCopy();
        }
        /// <summary>
        /// Requests the connection.
        /// </summary>
        /// <returns></returns>
        private SqlConnection RequestConnection()
        {
            var args = new ExtendedEventArgs<SqlConnection>();
            ConnectionRequested(this, args);
            return args.Data;
        }

        /// <summary>
        /// The disposed
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                if (disposing)
                {
                    try
                    {
                        Flush();

                        //Task.Factory.StartNew(() =>
                        //{
                        //    var configService = ServiceLocator.Current.GetInstance<IConfigurationService>();
                        //    var dbCreator = ServiceLocator.Current.GetInstance<IDatabaseCreator>(CreatorNames.Sql);
                        //    dbCreator.TruncateDbLogFile(configService.ConnectionSettings.ConnectionString);
                        //}, TaskCreationOptions.LongRunning);
                    }
                    finally
                    {
                        try
                        {
                            if (CurrentTargetTable != null)
                            {
                                CurrentTargetTable.Dispose();
                                CurrentTargetTable = null;
                            }
                        }
                        finally
                        {
                            if (TargetTableSource != null)
                            {
                                TargetTableSource.Dispose();
                                TargetTableSource = null;
                            }
                        }
                    }
                }
                _disposed = true;
            }
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="BulkInsert{T, TKey}" /> class.
        /// </summary>
        ~BulkInsert()
        {
            Dispose(false);
        }

        /// <summary>
        /// Gets the name of the target table.
        /// </summary>
        /// <value>
        /// The name of the target table.
        /// </value>
        string TargetTableName
        {
            get { return Target != null && Target.IsCustom ? Target.DbSchemaName : typeof (T).EntityTableName(); }
        }

        /// <summary>
        /// Gets or sets the target table source.
        /// </summary>
        /// <value>
        /// The target table source.
        /// </value>
        DataTable TargetTableSource
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the current target table.
        /// </summary>
        /// <value>
        /// The current target table.
        /// </value>
        DataTable CurrentTargetTable
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the column mappings.
        /// </summary>
        /// <value>
        /// The column mappings.
        /// </value>
        List<SqlBulkCopyColumnMapping> ColumnMappings { get; set; }

        /// <summary>
        /// Initializes the bulk copy.
        /// </summary>
        private void InitBulkCopy()
        {
            ColumnMappings = new List<SqlBulkCopyColumnMapping>();
            SessionLogger.Information(@"Building Bulk Insert Map for Target ""{0}"" into table ""{1}""",
                    typeof(T).FullName, TargetTableName);

            foreach (var col in TargetTableSource.Columns.OfType<DataColumn>())
            {
                if (!col.AutoIncrement)
                {
                    var mapping = new SqlBulkCopyColumnMapping(col.ColumnName, col.ColumnName);
                    ColumnMappings.Add(mapping);
                }
            }

            SessionLogger.Information(@"Bulk Insert Mapped for Target ""{0}"" into table ""{1}""", typeof(T).FullName, TargetTableName);
        }

        /// <summary>
        /// Initializes the source table.
        /// </summary>
        private void InitSourceTable()
        {
            SessionLogger.Information(@"Creating source data table structure for Target ""{0}"" from table ""{1}""", typeof(T).FullName, TargetTableName);
            TargetTableSource = new DataTable();
            var sql = string.Format("SELECT  * from [{0}]", TargetTableName);
            using (var conn = new SqlConnection(ConfigService.ConnectionSettings.ConnectionString))
            {
                conn.Open();
                using (var da = new SqlDataAdapter(sql, conn))
                {
                    da.FillSchema(TargetTableSource, SchemaType.Source);
                }
                conn.Close();
            }
            var idCol = TargetTableSource.Columns["Id"];
            if (idCol != null)
            {
                // if the table has an "Id" column, assume that it's an IDENTITY column and we're not providing values (i.e.: all of our values are 0 and will fail the unique constraint)
                TargetTableSource.PrimaryKey = null;
                idCol.Unique = false;
            }
            SessionLogger.Information(@"Source data table structure created for Target ""{0}"" from table ""{1}""", typeof(T).FullName, TargetTableName);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    static class Helper
    {
        /// <summary>
        /// Determines whether [is].
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <returns>
        ///   <c>true</c> if [is] [the specified source]; otherwise, <c>false</c>.
        /// </returns>
        public static bool Is<T>(this Type source)
        {
            return source.IsSubclassOf(typeof(T));
        }
    }

}
