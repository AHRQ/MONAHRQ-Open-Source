using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.ServiceLocation;
using Monahrq.Infrastructure.Configuration;
using Monahrq.Infrastructure.Entities.Core;
using Monahrq.Infrastructure.Entities.Core.Import;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Entities.Events;
using Monahrq.Infrastructure.Utility;
using Monahrq.Sdk.Utilities;
using NHibernate;

namespace Monahrq.Infrastructure.BaseDataLoader
{
    /// <summary>
    /// The abstract/base entity importer class.
    /// </summary>
    /// <typeparam name="TStrategy">The type of the strategy.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <seealso cref="Monahrq.Infrastructure.Entities.Core.Import.IDataLoader" />
    public abstract class EntityImporter<TStrategy, TEntity, TKey> : IDataLoader
        where TStrategy : IEntityDataReaderStrategy<TEntity, TKey>, new()
        where TEntity : Entity<TKey>
    {
        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <value>
        /// The logger.
        /// </value>
        protected ILogWriter Logger { get; private set; }
        /// <summary>
        /// Gets the session factory.
        /// </summary>
        /// <value>
        /// The session factory.
        /// </value>
        protected ISessionFactory SessionFactory { get; private set; }
        /// <summary>
        /// Gets the session provider.
        /// </summary>
        /// <value>
        /// The session provider.
        /// </value>
        protected IDomainSessionFactoryProvider SessionProvider { get; private set; }
        /// <summary>
        /// Gets the entity strategy.
        /// </summary>
        /// <value>
        /// The entity strategy.
        /// </value>
        protected IEntityDataReaderStrategy<TEntity, TKey> EntityStrategy { get; private set; }
        /// <summary>
        /// Gets the configuration service.
        /// </summary>
        /// <value>
        /// The configuration service.
        /// </value>
        protected IConfigurationService ConfigurationService { get; private set; }
        /// <summary>
        /// Gets the versioning strategy.
        /// </summary>
        /// <value>
        /// The versioning strategy.
        /// </value>
        protected BaseDataVersionStrategy VersioningStrategy { get; private set; }

        /// <summary>
        /// Gets or sets the event aggregator.
        /// </summary>
        /// <value>
        /// The event aggregator.
        /// </value>
        protected IEventAggregator EventAggregator { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityImporter{TStrategy, TEntity, TKey}"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="sessionFactoryProvider">The session factory provider.</param>
        /// <param name="dataproviders">The dataproviders.</param>
        /// <param name="configurationService">The configuration service.</param>
        protected EntityImporter(ILogWriter logger
            , [Import(RequiredCreationPolicy = CreationPolicy.NonShared)] 
                IDomainSessionFactoryProvider sessionFactoryProvider
            , IEnumerable<IDataReaderDictionary> dataproviders
            , IConfigurationService configurationService)
        {
            Logger = logger;
            SessionFactory = sessionFactoryProvider.SessionFactory;
            SessionProvider = sessionFactoryProvider;
            FirstImport = false;
            EntityStrategy = new TStrategy();
            var max = dataproviders.Max(d => d.VersionAttribute.Version);
            DataProvider = dataproviders.FirstOrDefault(d => d.VersionAttribute.Version == max);
            ConfigurationService = configurationService;

            EventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>();

            OnFeedback += (sender, args) =>
            {
                if (!IsBackground)
                    EventAggregator.GetEvent<MessageUpdateEvent>()
                        .Publish(new MessageUpdateEvent {Message = args.Data.ToString()});
                else
                    EventAggregator.GetEvent<UiMessageUpdateEventForeGround>()
                        .Publish(new UiMessageUpdateEventForeGround() { Message = args.Data.ToString() });
            };
        }

        //private BaseDataVersionStrategy GetVersioningStrategy()
        //{
        //    var entityType = typeof (TEntity);

        //    var attributes = entityType.GetCustomAttributes(typeof(BaseDataImportAttribute), true) as BaseDataImportAttribute[];

        //    if (attributes == null || !attributes.Any())
        //        return null; // or throw exception

        //    return attributes[0].CreateVersionStrategy<TEntity>();
        //}

        /// <summary>
        /// Loads the data.
        /// </summary>
        public virtual void LoadData()
        {
            try
            {
                ProvideFeedback(string.Format("Loading {0}", Inflector.Pluralize(Inflector.Titleize2(typeof(TEntity).Name))));

                string tableName = typeof(TEntity).EntityTableName();

                using (var session = this.SessionProvider.SessionFactory.OpenStatelessSession())
                {
                    session.CreateSQLQuery(EnableDisableNonClusteredIndexes(tableName, true)) //ALL
                        .ExecuteUpdate();
                }

                using (var session = this.SessionProvider.SessionFactory.OpenStatelessSession())
                {
                    EntityStrategy.CurrentSession = session;
                    using (var reader = DataProvider[Reader.Name])
                    {
                        if (!reader.AlreadyImported())
                        {
                            using (var bulkImporter = new BulkInsert<TEntity, TKey>(session.Connection))
                            {
                                bulkImporter.ConnectionRequested += (o, e) =>
                                {
                                    e.Data = session.Connection as SqlConnection;
                                };
                                bulkImporter.Prepare();
                                bulkImporter.BatchSize = BatchSize; ;
                                while (reader.Read())
                                {
                                    var temp = EntityStrategy.LoadFromReader(reader);
                                    bulkImporter.Insert(temp);
                                }
                            }
                        }
                    }
                }

                Task.Factory.StartNew(() =>
                {
                    using (var session = this.SessionProvider.SessionFactory.OpenStatelessSession())
                    {
                        session.CreateSQLQuery(EnableDisableNonClusteredIndexes(tableName)) //ALL
                            .SetTimeout(600)
                            .ExecuteUpdate();
                    }
                }, TaskCreationOptions.LongRunning);
            }
            catch (Exception ex)
            {
                Logger.Write(ex, $"Error importing data for entity type {typeof(TEntity).Name} using strategy {typeof(TStrategy).Name}");
            }
        }

        /// <summary>
        /// Loads the SQL bulk insert.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="formatFile">The format file.</param>
        /// <param name="dataFiles">The data files.</param>
        protected void LoadSqlBulkInsert(string tableName, string formatFile, params string[] dataFiles)
        {
            try
            {
                // Verify format file exists.
                if (!File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", formatFile)))
                {
                    Logger.Write(string.Format("Format file \"{0}\" missing from resources directory.", formatFile));
                    return;
                }

                using (var session = SessionProvider.SessionFactory.OpenStatelessSession())
                {
                    var con = session.Connection;

                    foreach (var dataFile in dataFiles)
                    {
                        // Verify data file exists.
                        if (!File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", dataFile)))
                        {
                            Logger.Write(string.Format("Import file \"{0}\" missing from resources directory.", dataFile));
                        }
                        else
                        {
                            ProvideFeedback(string.Format("Importing file {0}", dataFile));

                            using (var cmd = con.CreateCommand())
                            {
                                cmd.CommandText = "BULK INSERT " + tableName + " FROM '" +
                                                  Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", dataFile) +
                                                  "' WITH (FIRSTROW = 2, FORMATFILE = '" +
                                                  Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", formatFile) + "')";
                                cmd.CommandTimeout = (int)6000; 
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Write(ex, "Error performing bulk insert of data for entity {0}", typeof(TEntity).Name);
            }
        }

        /// <summary>
        /// Provides the feedback.
        /// </summary>
        /// <param name="msg">The MSG.</param>
        protected void ProvideFeedback(string msg)
        {
            OnFeedback(this, new ExtendedEventArgs<string>(msg));
        }

        /// <summary>
        /// Gets the data provider.
        /// </summary>
        /// <value>
        /// The data provider.
        /// </value>
        public IDataReaderDictionary DataProvider { get; private set; }
        /// <summary>
        /// Gets the reader.
        /// </summary>
        /// <value>
        /// The reader.
        /// </value>
        public abstract ReaderDefinition Reader { get; }
        /// <summary>
        /// Occurs when [on feedback].
        /// </summary>
        public event EventHandler<ExtendedEventArgs<string>> OnFeedback = delegate { };
        /// <summary>
        /// Gets or sets a value indicating whether [first import].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [first import]; otherwise, <c>false</c>.
        /// </value>
        public bool FirstImport { get; protected set; }

        /// <summary>
        /// Gets a value indicating whether this instance is background.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is background; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsBackground
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the size of the batch.
        /// </summary>
        /// <value>
        /// The size of the batch.
        /// </value>
        public virtual int BatchSize
        {
            get { return ConfigurationService.MonahrqSettings.BatchSize; }
        }

        #region Index Manipulation

        /// <summary>
        /// Enables the table indexes.
        /// </summary>
        /// <param name="tableNames">The table names.</param>
        protected void EnableTableIndexes(params string[] tableNames)
        {
            EnableDisableTableIndexes(false, tableNames);
        }

        /// <summary>
        /// Disables the table indexes.
        /// </summary>
        /// <param name="tableNames">The table names.</param>
        protected void DisableTableIndexes(params string[] tableNames)
        {
            EnableDisableTableIndexes(true, tableNames);
        }

        /// <summary>
        /// Enables the disable table indexes.
        /// </summary>
        /// <param name="disable">if set to <c>true</c> [disable].</param>
        /// <param name="tableNames">The table names.</param>
        private void EnableDisableTableIndexes(bool disable, params string[] tableNames)
        {
            using (var session = SessionProvider.SessionFactory.OpenStatelessSession())
            {
                foreach (var tableName in tableNames)
                {
                    session.CreateSQLQuery(EnableDisableNonClusteredIndexes(tableName, disable)) //ALL
                        .SetTimeout(600)
                        .ExecuteUpdate();
                }
            }
        }

        /// <summary>
        /// Enables the disable non clustered indexes.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="disable">if set to <c>true</c> [disable].</param>
        /// <returns></returns>
        private string EnableDisableNonClusteredIndexes(string tableName, bool disable = false)
        {
            var sqlStatement = new StringBuilder();

            sqlStatement.AppendLine("DECLARE @sql AS VARCHAR(MAX)='';" + System.Environment.NewLine);
            sqlStatement.Append("SELECT	 @sql = @sql + 'ALTER INDEX ' + sys.indexes.Name + ' ON " + tableName + " " + (disable ? "DISABLE" : "REBUILD") + "; ");

            if (!disable)
            {
                sqlStatement.Append(" ALTER INDEX ' + sys.indexes.Name + ' ON " + tableName + " REORGANIZE;");
            }
            
            sqlStatement.Append("' + CHAR(13) + CHAR(10)");
            sqlStatement.AppendLine();

            sqlStatement.AppendLine("FROM	 sys.indexes" + System.Environment.NewLine);
            sqlStatement.AppendLine("JOIN    sys.objects ON sys.indexes.object_id = sys.objects.object_id");
            sqlStatement.AppendLine("WHERE sys.indexes.type_desc = 'NONCLUSTERED'");
            sqlStatement.AppendLine("  AND sys.objects.type_desc = 'USER_TABLE'");
            sqlStatement.AppendLine("  AND sys.objects.Name = '" + tableName + "';");

            sqlStatement.AppendLine();
            sqlStatement.AppendLine("exec(@sql);");

            return sqlStatement.ToString();
        }
        #endregion Index Manipulation
    }
}