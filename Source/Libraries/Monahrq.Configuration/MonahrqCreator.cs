using Monahrq.Infrastructure;
using Monahrq.Infrastructure.Configuration;
using Monahrq.Infrastructure.Data;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Sdk.DataProvider;
using Monahrq.Sdk.Events;
using System;
using System.ComponentModel.Composition;
using System.Data.SqlClient;
using System.Windows;

namespace Monahrq.Configuration
{
    [Export(CreatorNames.Monahrq, typeof(IDatabaseCreator)),
     PartCreationPolicy(CreationPolicy.NonShared)]
    public class MonahrqCreator : IDatabaseCreator
    {
        [Import]
        IConfigurationService ConfigService
        {
            get;
            set;
        }

        [Import]
        IDataServicesProvider DataServicesProvider
        {
            get;
            set;
        }

        public void DeleteAndCreate(string connectionString, string schemaVersion)
        { }

        public void TruncateDbLogFile(string connectionString)
        { }

        public event EventHandler<ConnectionStringSuccessEventArgs> Created = delegate { };
        public event EventHandler<ConnectionStringFailedEventArgs> CreateFailed = delegate { };
        public event EventHandler<ConnectionStringSuccessEventArgs> Tested = delegate { };
        public event EventHandler<ConnectionStringFailedEventArgs> TestFailed = delegate { };
        public event EventHandler<ConnectionStringSuccessEventArgs> Deleted = delegate { };
        public event EventHandler<ConnectionStringFailedEventArgs> DeleteFailed = delegate { };
        public event EventHandler<ConnectionStringSuccessEventArgs> DeleteAndCreated = delegate { };
        public event EventHandler<ConnectionStringFailedEventArgs> DeleteAndCreatedFailed = delegate { };
        public event EventHandler<ConnectionStringSuccessEventArgs> UpgradeCompleted = delegate { };
        public event EventHandler<ConnectionStringFailedEventArgs> UpgradeCompletedFailed = delegate { };

        public void Create(string connectionString, string schemaVersion)
        {
        }

        public void Delete(string connectionString)
        {
        }

        public void Test(string connectionString)
        {
            var connstr = ConfigService.ConnectionSettings.ConnectionString;
            if (InitializeDatabase(connstr))
            {
                Tested(this, new ConnectionStringSuccessEventArgs(
                        new SqlConnectionStringBuilder(connstr), "Connection Succeeded"));
            }
            else
            {
                TestFailed(this,
                    new ConnectionStringFailedEventArgs(
                        new SqlConnectionStringBuilder(connstr), new Exception("Connection Failed")));
            }
        }

        /// <summary>
        /// Initializes the database and Prompts the user to get a database connection, if needed.
        /// </summary>
        /// <param name="connectStr">The connect STR.</param>
        private bool InitializeDatabase(string connectStr = null)
        {
            connectStr = string.IsNullOrEmpty(connectStr) ? ConfigService.ConnectionSettings.ConnectionString : connectStr;
            var result = ConnectionStringIsValid(connectStr);
            // TODO: check schema version
            var skipDataConnection = false;
            if (result)
            {
                using (var con = new SqlConnection(connectStr))
                {
                    skipDataConnection = DataServicesProvider.UpgradeDatabase(con);
                }
            }

            if (!result && !skipDataConnection)
            {
                var window = new GetDatabaseConnection();
                window.ShowDialog();
                result = window.DialogResult.GetValueOrDefault();
            }
            return result;
        }

        /// <summary>
        /// Connections the string is valid.
        /// </summary>
        /// <param name="connstr">The connstr.</param>
        /// <returns></returns>
        private bool ConnectionStringIsValid(string connstr)
        {
            bool succeeded = false;

            try
            {
                var creator = new SqlDatabaseCreator();
                creator.TestFailed += delegate
                {
                    succeeded = false;
                };
                creator.Tested += delegate
                {
                    succeeded = true;
                };

                creator.Test(connstr);

                return succeeded;
            }
            catch (Exception)
            {
                return false;
            }
        }


        public void UpgradeDatabase()
        {

        }
    }
}
