using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.Logging;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Entities.Events;
using Monahrq.Infrastructure.Utility;
using System;
using System.Data;
using System.IO;
using System.Text;
using Monahrq.Sdk.Utilities;

namespace Monahrq.Infrastructure.BaseDataLoader
{
    /// <summary>
    /// The abstract base data importer.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <seealso cref="Monahrq.Infrastructure.BaseDataLoader.IBaseDataImporter{TEntity, TKey}" />
    /// <seealso cref="System.ComponentModel.Composition.IPartImportsSatisfiedNotification" />
    public abstract class BaseDataImporter<TEntity, TKey> : IBaseDataImporter<TEntity, TKey>, IPartImportsSatisfiedNotification
        where TEntity : Entity<TKey>, new()
    {
        /// <summary>
        /// Gets or sets the data provider.
        /// </summary>
        /// <value>
        /// The data provider.
        /// </value>
        [Import]
        public IDomainSessionFactoryProvider DataProvider { get; set; }
        /// <summary>
        /// Gets or sets the logger.
        /// </summary>
        /// <value>
        /// The logger.
        /// </value>
        [Import(LogNames.Session)]
        public ILogWriter Logger { get; set; }

        /// <summary>
        /// Gets the loader priority.
        /// </summary>
        /// <value>
        /// The loader priority.
        /// </value>
        public virtual int LoaderPriority { get { return 10; } }
        /// <summary>
        /// Gets or sets the version strategy.
        /// </summary>
        /// <value>
        /// The version strategy.
        /// </value>
        public BaseDataVersionStrategy VersionStrategy { get; set; }
        /// <summary>
        /// Gets the name of the database table.
        /// </summary>
        /// <value>
        /// The name of the database table.
        /// </value>
        public virtual string DatabaseTableName { get { return typeof(TEntity).EntityTableName(); } }
        /// <summary>
        /// Pres the load data.
        /// </summary>
        public virtual void PreLoadData() { }
        /// <summary>
        /// Loads the data.
        /// </summary>
        public abstract void LoadData();
        /// <summary>
        /// Posts the load data.
        /// </summary>
        public virtual void PostLoadData() { }
        /// <summary>
        /// Gets the loader description.
        /// </summary>
        /// <value>
        /// The loader description.
        /// </value>
        public virtual string LoaderDescription { get { return Inflector.Pluralize(Inflector.Titleize2(typeof(TEntity).Name)); } }
        /// <summary>
        /// Gets a value indicating whether [turn off indexes during impport].
        /// </summary>
        /// <value>
        /// <c>true</c> if [turn off indexes during impport]; otherwise, <c>false</c>.
        /// </value>
        public virtual bool TurnOffIndexesDuringImpport { get { return true; } }
        /// <summary>
        /// Gets a value indicating whether [allow disable indexes during import].
        /// </summary>
        /// <value>
        /// <c>true</c> if [allow disable indexes during import]; otherwise, <c>false</c>.
        /// </value>
        public virtual bool AllowDisableIndexesDuringImport { get { return true; } }
        /// <summary>
        /// Gets a value indicating whether [allow enable indexes during import].
        /// </summary>
        /// <value>
        /// <c>true</c> if [allow enable indexes during import]; otherwise, <c>false</c>.
        /// </value>
        public virtual bool AllowEnableIndexesDuringImport { get { return true; } }

        /// <summary>
        /// Loads from reader.
        /// </summary>
        /// <param name="rdr">The RDR.</param>
        /// <returns></returns>
        public virtual TEntity LoadFromReader(IDataReader rdr) { return null; }

        /// <summary>
        /// Occurs when [on feedback].
        /// </summary>
        public event EventHandler<ExtendedEventArgs<string>> OnFeedback = delegate { };
        /// <summary>
        /// Provides the feedback.
        /// </summary>
        /// <param name="msg">The MSG.</param>
        protected void ProvideFeedback(string msg) { OnFeedback(this, new ExtendedEventArgs<string>(msg)); }

        /// <summary>
        /// The base data dir
        /// </summary>
        protected string baseDataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "BaseData");
        /// <summary>
        /// The table name
        /// </summary>
        protected string tableName = typeof(TEntity).EntityTableName();
        /// <summary>
        /// The table index is off
        /// </summary>
        private bool tableIndexIsOff = false;
        /// <summary>
        /// Called when a part's imports have been satisfied and it is safe to use.
        /// </summary>
        public virtual void OnImportsSatisfied() { }

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
            using (var session = DataProvider.SessionFactory.OpenStatelessSession())
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
