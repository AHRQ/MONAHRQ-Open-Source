using System.Data;
using System.Text;
using Microsoft.Practices.Prism.Logging;
using Monahrq.Infrastructure;
using Monahrq.Infrastructure.Utility.Extensions;
using Monahrq.Sdk.Events;
using System;
using System.ComponentModel.Composition;
using System.Data.SqlClient;

namespace Monahrq.Sdk.DataProvider
{
    [Export(CreatorNames.Sql, typeof(IDatabaseCreator)),
     PartCreationPolicy(CreationPolicy.NonShared)]
    public class SqlDatabaseCreator : IDatabaseCreator
    {
        const string CREATE_SUCCESS_MESSAGE = "Successfully created database.";
        const string TEST_SUCCESS_MESSAGE = "Successfully connected to database.";
        const string DELETE_SUCCESS_MESSAGE = "Database deleted.";

        [Import(LogNames.Session)]
        ILoggerFacade Logger { get; set; }

        // Delete the database
        public void Delete(string connectionString)
        {
            try
            {
                var masterbuilder = new SqlConnectionStringBuilder(connectionString);
                var dbName = masterbuilder.InitialCatalog;
                masterbuilder.InitialCatalog = string.Empty;

                using (var con = new SqlConnection(masterbuilder.ConnectionString))
                {
                    con.Open();
                    using (var cmd = con.CreateCommand())
                    {
                        cmd.CommandText = BuildDeleteDatabaseQuery(dbName);
                        cmd.CommandType = CommandType.Text;

                        cmd.ExecuteNonQuery();
                        Deleted(this, new ConnectionStringSuccessEventArgs(masterbuilder, DELETE_SUCCESS_MESSAGE));
                    }
                }
            }
            catch (Exception ex)
            {
                DeleteFailed(this, new ConnectionStringFailedEventArgs(new SqlConnectionStringBuilder(connectionString), ex));
            }
        }

        public void TruncateDbLogFile(string connectionString)
        {
            try
            {
                var masterbuilder = new SqlConnectionStringBuilder(connectionString);
                var dbName = masterbuilder.InitialCatalog;
                masterbuilder.InitialCatalog = string.Empty;

                var queryBuilder = new StringBuilder();

                queryBuilder.AppendLine("USE [" + dbName + "]");
                //queryBuilder.AppendLine("GO");
                queryBuilder.AppendLine("   ALTER DATABASE [" + dbName + "] SET RECOVERY SIMPLE WITH NO_WAIT; ");
                //queryBuilder.AppendLine("GO");
                queryBuilder.AppendLine("   DBCC SHRINKFILE ('" + dbName + "_log', 1, TRUNCATEONLY); ");
                //queryBuilder.AppendLine("GO");
                queryBuilder.AppendLine("   ALTER DATABASE [" + dbName + "] SET RECOVERY SIMPLE WITH NO_WAIT; ");
                //queryBuilder.AppendLine("GO");


                using (var con = new SqlConnection(masterbuilder.ConnectionString))
                {
                    con.Open();
                    using (var cmd = con.CreateCommand())
                    {
                        cmd.CommandText = queryBuilder.ToString();
                        cmd.CommandType = CommandType.Text;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex.GetBaseException().Message, Category.Exception, Priority.High);
            }
        }

        /// <summary>
        /// Creates the specified connection string.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        public void Create(string connectionString,string schemaVersion)
        {
            try
            {
                var masterbuilder = new SqlConnectionStringBuilder(connectionString);
                var dbName = masterbuilder.InitialCatalog;
                masterbuilder.InitialCatalog = string.Empty;
                //masterbuilder.ApplicationIntent = ApplicationIntent.ReadWrite;
                using (var con = new SqlConnection(masterbuilder.ConnectionString))
                {
                    con.Open();
                    using (var cmd = con.CreateCommand())
                    {
                        cmd.CommandText = BuildCreateDatabaseQuery(dbName);
                        cmd.CommandType = CommandType.Text;
                        //cmd.CommandText = "CREATE DATABASE @dbName";
                        //cmd.Parameters.Add(new SqlParameter("@dbName", dbName));
                        cmd.ExecuteNonQuery();
                        AddSchemaVersion(con, dbName, "Database Schema", schemaVersion);
                        Created(this, new ConnectionStringSuccessEventArgs(masterbuilder, CREATE_SUCCESS_MESSAGE));
                    }
                }
            }
            catch (Exception ex)
            {
                CreateFailed(this, new ConnectionStringFailedEventArgs(new SqlConnectionStringBuilder(connectionString), ex));
            }
        }

        void AddSchemaVersion(SqlConnection con, string dbName, string itemName, string schemaVersion)
        {
            var queryBuilder = new StringBuilder();

            //queryBuilder.AppendLine(string.Format(ReflectionHelper.ReadEmbeddedResourceFile(typeof(ReflectionHelper).Assembly,
            //                              "Monahrq.Infrastructure.Resources.SchemaVersion.SchemaVersion_Create_Update.sql"),
            //                              itemName, schemaVersion));

            queryBuilder.AppendLine("USE [" + dbName + "];");
            queryBuilder.AppendLine("IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'SchemaVersions')");
            queryBuilder.AppendLine("  BEGIN");
            queryBuilder.AppendLine(@"    CREATE TABLE [dbo].[SchemaVersions]([Id] [int] IDENTITY(1,1) NOT NULL, [Name] [nvarchar](255) NOT NULL, [Version] [nvarchar](50) NOT NULL, [ActiveDate] [datetime] NOT NULL DEFAULT (GETDATE())) ON [PRIMARY]; ");
            queryBuilder.AppendLine("  END");
            //queryBuilder.AppendLine("ELSE");
            queryBuilder.AppendLine();
            queryBuilder.AppendLine("INSERT INTO [dbo].[SchemaVersions] ([Name],[Version]) VALUES ('" + itemName + "','" + schemaVersion + "');");
            queryBuilder.AppendLine();

            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = queryBuilder.ToString();
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Deletes the and create.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="schemaVersion">The schema version.</param>
        public void DeleteAndCreate(string connectionString,string schemaVersion)
        {
            try
            {
                var masterbuilder = new SqlConnectionStringBuilder(connectionString);
                var dbName = masterbuilder.InitialCatalog;
                masterbuilder.InitialCatalog = string.Empty;
                masterbuilder.ApplicationIntent = ApplicationIntent.ReadWrite;
                using (var con = new SqlConnection(masterbuilder.ConnectionString))
                {
                    con.Open();
                    using (var cmd = con.CreateCommand())
                    {
                        cmd.CommandText = BuildDropAndCreateTableQuery(dbName);
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandTimeout = 180;
                        cmd.ExecuteNonQuery();
                        AddSchemaVersion(con, dbName, "Database Schema", schemaVersion);
                        DeleteAndCreated(this, new ConnectionStringSuccessEventArgs(masterbuilder, CREATE_SUCCESS_MESSAGE));
                    }
                }
            }
            catch (Exception ex)
            {
                CreateFailed(this, new ConnectionStringFailedEventArgs(new SqlConnectionStringBuilder(connectionString), ex));
            }
        }

        /// <summary>
        /// Upgrades the specified connection string.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="newConnectionString">The new connection string.</param>
        /// <param name="schemaVersion">The schema version.</param>
        public void Upgrade(string connectionString, string newConnectionString, string schemaVersion)
        {
            try
            {
                var masterbuilder = new SqlConnectionStringBuilder(newConnectionString);
                var dbName = masterbuilder.InitialCatalog;
                masterbuilder.InitialCatalog = string.Empty;
                masterbuilder.ApplicationIntent = ApplicationIntent.ReadWrite;
                using (var con = new SqlConnection(masterbuilder.ConnectionString))
                {
                    con.Open();
                    using (var cmd = con.CreateCommand())
                    {
                        cmd.CommandText = BuildDropAndCreateTableQuery(dbName);
                        cmd.CommandType = CommandType.Text;
                        cmd.ExecuteNonQuery();
                        AddSchemaVersion(con, dbName, "Database Schema", schemaVersion);
                        DeleteAndCreated(this, new ConnectionStringSuccessEventArgs(masterbuilder, CREATE_SUCCESS_MESSAGE));
                    }
                }
            }
            catch (Exception ex)
            {
                UpgradeCompletedFailed(this, new ConnectionStringFailedEventArgs(new SqlConnectionStringBuilder(newConnectionString), ex));
            }
        }

        /// <summary>
        /// Builds the create table query.
        /// </summary>
        /// <param name="dbName">Name of the db.</param>
        /// <returns></returns>
        private string BuildCreateDatabaseQuery(string dbName)
        {
            return string.Format(" CREATE DATABASE [{0}]; ALTER DATABASE [{0}] SET RECOVERY SIMPLE;", dbName);
        }

        /// <summary>
        /// Builds the drop and create table query.
        /// </summary>
        /// <param name="dbName">Name of the database.</param>
        /// <returns></returns>
        private string BuildDropAndCreateTableQuery(string dbName)
        {
            var queryBuilder = new StringBuilder();

            queryBuilder.AppendLine(BuildDeleteDatabaseQuery(dbName));
            queryBuilder.AppendLine();
            queryBuilder.AppendLine(BuildCreateDatabaseQuery(dbName));

            return queryBuilder.ToString();
        }

        /// <summary>
        /// Builds the delete database query.
        /// </summary>
        /// <param name="dbName">Name of the database.</param>
        /// <returns></returns>
        private string BuildDeleteDatabaseQuery(string dbName)
        {
            var queryBuilder = new StringBuilder();

            queryBuilder.AppendFormat("IF (db_id('{0}') IS NOT NULL) ", dbName);
            queryBuilder.AppendLine();
            queryBuilder.AppendLine("BEGIN ");
            queryBuilder.AppendFormat("     ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; ", dbName);
            queryBuilder.AppendLine();
            queryBuilder.AppendFormat("     DROP DATABASE [{0}]; ", dbName);
            queryBuilder.AppendLine();
            queryBuilder.AppendLine("END ");

            return queryBuilder.ToString();
        }

        /// <summary>
        /// Tests the specified connection string.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        public void Test(string connectionString)
        {
            var builder = new SqlConnectionStringBuilder(connectionString);
            try
            {
                using (var con = new SqlConnection(builder.ConnectionString))
                {
                    con.Open();
                    Tested(this, new ConnectionStringSuccessEventArgs(builder, TEST_SUCCESS_MESSAGE));
                }
            }
            catch (Exception ex)
            {
                TestFailed(this, new ConnectionStringFailedEventArgs(builder, ex));
            }
        }

        /// <summary>
        /// These are the events for the commands in the HostConnectionView user control
        /// </summary>
        public event EventHandler<ConnectionStringSuccessEventArgs> Created = delegate { };
        /// <summary>
        /// Occurs when [create failed].
        /// </summary>
        public event EventHandler<ConnectionStringFailedEventArgs> CreateFailed = delegate { };
        /// <summary>
        /// Occurs when [tested].
        /// </summary>
        public event EventHandler<ConnectionStringSuccessEventArgs> Tested = delegate { };
        /// <summary>
        /// Occurs when [test failed].
        /// </summary>
        public event EventHandler<ConnectionStringFailedEventArgs> TestFailed = delegate { };
        /// <summary>
        /// Occurs when [deleted].
        /// </summary>
        public event EventHandler<ConnectionStringSuccessEventArgs> Deleted = delegate { };
        /// <summary>
        /// Occurs when [delete failed].
        /// </summary>
        public event EventHandler<ConnectionStringFailedEventArgs> DeleteFailed = delegate { };

        /// <summary>
        /// Occurs when [delete and created].
        /// </summary>
        public event EventHandler<ConnectionStringSuccessEventArgs> DeleteAndCreated = delegate { };
        /// <summary>
        /// Occurs when [delete and created failed].
        /// </summary>
        public event EventHandler<ConnectionStringFailedEventArgs> DeleteAndCreatedFailed = delegate { };
        /// <summary>
        /// Occurs when [upgrade completed].
        /// </summary>
        public event EventHandler<ConnectionStringSuccessEventArgs> UpgradeCompleted = delegate { };
        /// <summary>
        /// Occurs when [upgrade completed failed].
        /// </summary>
        public event EventHandler<ConnectionStringFailedEventArgs> UpgradeCompletedFailed = delegate { };
    }

    #region Unused Code
    //[Export(CreatorNames.Monahrq, typeof(IDatabaseCreator))]
    //public class MonahrqCreator : IDatabaseCreator
    //{
    //    [Import]
    //    IConfigurationService ConfigService
    //    {
    //        get;
    //        set;
    //    }

    //    public event EventHandler<Sdk.Events.ConnectionStringSuccessEventArgs> Created = delegate { };
    //    public event EventHandler<Sdk.Events.ConnectionStringFailedEventArgs> CreateFailed = delegate { };
    //    public event EventHandler<Sdk.Events.ConnectionStringSuccessEventArgs> Tested = delegate { };
    //    public event EventHandler<Sdk.Events.ConnectionStringFailedEventArgs> TestFailed = delegate { };
    //    public event EventHandler<Sdk.Events.ConnectionStringSuccessEventArgs> Deleted = delegate { };
    //    public event EventHandler<Sdk.Events.ConnectionStringFailedEventArgs> DeleteFailed = delegate { };

    //    public void Create(string connectionString)
    //    {
    //    }

    //    public void Delete(string connectionString)
    //    {
    //    }

    //    public void Test(string connectionString)
    //    {
    //        var connstr = ConfigService.ConnectionSettings.ConnectionString;
    //        if (InitializeDatabase(connstr))
    //        {
    //            Tested(this, new ConnectionStringSuccessEventArgs(
    //                    new SqlConnectionStringBuilder(connstr), "Connection Succeeded"));
    //        }
    //        else
    //        {
    //            TestFailed(this,
    //                new Sdk.Events.ConnectionStringFailedEventArgs(
    //                    new SqlConnectionStringBuilder(connstr), new Exception("Connection Failed")));
    //        }
    //    }

    //    /// <summary>
    //    /// Initializes the database and Prompts the user to get a database connection, if needed.
    //    /// </summary>
    //    /// <param name="connectStr">The connect STR.</param>
    //    private bool InitializeDatabase(string connectStr = null)
    //    {
    //        connectStr = string.IsNullOrEmpty(connectStr) ? ConfigService.ConnectionSettings.ConnectionString : connectStr;
    //        var result = ConnectionStringIsValid(connectStr);
    //        if (!result)
    //        {
    //            var window = new GetDatabaseConnection();
    //            window.ShowDialog();
    //            result = window.DialogResult.GetValueOrDefault();
    //        }
    //        return result;
    //    }

    //    /// <summary>
    //    /// Connections the string is valid.
    //    /// </summary>
    //    /// <param name="connstr">The connstr.</param>
    //    /// <returns></returns>
    //    private bool ConnectionStringIsValid(string connstr)
    //    {
    //        bool succeeded = false;
    //        //#if DEBUG
    //        //            return false;
    //        //#else
    //        try
    //        {
    //            var creator = new SqlDatabaseCreator();
    //            creator.TestFailed += delegate
    //            {
    //                succeeded = false;
    //            };
    //            creator.Tested += delegate
    //            {
    //                succeeded = true;
    //            };

    //            creator.Test(connstr);

    //            return succeeded;
    //        }
    //        catch (Exception)
    //        {
    //            return false;
    //        }
    //        //#endif
    //    }
    //}
    #endregion
}
