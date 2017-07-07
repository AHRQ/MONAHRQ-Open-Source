using Monahrq.Infrastructure.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Monahrq.Infrastructure.Data.Extensibility.Configuration
{
    /// <summary>
    /// The persistence manager interface/contract.
    /// </summary>
    public interface IPersistenceManager
    {
        /// <summary>
        /// Asserts the database.
        /// </summary>
        void AssertDb();
        /// <summary>
        /// Resets the database.
        /// </summary>
        void ResetDb();
    }

    /// <summary>
    /// The Sql Persistence Manager
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.Data.Extensibility.Configuration.IPersistenceManager" />
    [Export(typeof(IPersistenceManager))]
    public class SqlPersistenceManager : IPersistenceManager
    {
        /// <summary>
        /// Gets or sets the configuration service.
        /// </summary>
        /// <value>
        /// The configuration service.
        /// </value>
        [Import]
        IConfigurationService ConfigurationService { get; set; }

        /// <summary>
        /// Asserts the database.
        /// </summary>
        public void AssertDb()
        {
            var builder = new SqlConnectionStringBuilder(ConfigurationService.ConnectionSettings.ConnectionString);
            var masterBuilder = new SqlConnectionStringBuilder(builder.ConnectionString);
            masterBuilder.InitialCatalog = string.Empty;
            using (var conn = new SqlConnection(masterBuilder.ConnectionString))
            {
                conn.Open();
                var dt = conn.GetSchema("Databases");
                var dbName = dt.Rows.OfType<DataRow>()
                            .Select(dr => dr[0].ToString())
                            .FirstOrDefault(db => string.Equals(db, builder.InitialCatalog, StringComparison.OrdinalIgnoreCase));
                if (string.IsNullOrEmpty(dbName))
                {
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = string.Format("CREATE DATABASE [{0}];", builder.InitialCatalog);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        /// <summary>
        /// Resets the database.
        /// </summary>
        public void ResetDb()
        {
            var builder = new SqlConnectionStringBuilder(ConfigurationService.ConnectionSettings.ConnectionString);
            var masterBuilder = new SqlConnectionStringBuilder(builder.ConnectionString);
            masterBuilder.InitialCatalog = string.Empty;

            #region drop commands
            var sql = new List<string>
            {
                string.Format("ALTER DATABASE [{0}] ", builder.InitialCatalog),
                "SET OFFLINE ",
                "WITH ROLLBACK IMMEDIATE",
                "GO",
                string.Format("DROP DATABASE [{0}] ", builder.InitialCatalog),
                "GO"
            };
            #endregion drop commands

            using (var conn = new SqlConnection(masterBuilder.ConnectionString))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = string.Join(Environment.NewLine, sql);
                    cmd.ExecuteNonQuery();
                }
            }

            AssertDb();
        }
 
    }
}
