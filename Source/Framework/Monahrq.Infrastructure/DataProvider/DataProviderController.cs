using System;
using System.Collections.Generic;
using System.Data.Common;
using Microsoft.Practices.ServiceLocation;
using System.Data;
using Monahrq.Infrastructure;

namespace Monahrq.Sdk.DataProvider
{
    public interface IDataProviderController
    {
        DbProviderFactory ProviderFactory { get; }
        string Name { get; }
        DbConnectionStringBuilder CreateConnectionStringBuilder();
        DataTable SelectTable(string connectionString, string tableName, int nrows = int.MaxValue);
    }

    public interface IDataProviderController<T> : IDataProviderController
        where T : DbProviderFactory
    {
    }

    public class DataProviderController<T> : IDataProviderController<T>
        where T : DbProviderFactory
    {
        private ILogWriter Logger
        {
            get;
            set;
        }

        public DataProviderController()
        {
            Initialize();
        }

        private void Initialize()
        {
            ProviderFactory = this.GetProviderFactory<T>();
            Logger = ServiceLocator.Current.GetInstance<ILogWriter>(LogNames.Session);
        }

        public T ProviderFactory
        {
            get;
            private set;
        }

        DbProviderFactory IDataProviderController.ProviderFactory
        {
            get { return this.ProviderFactory; }
        }

        public string Name
        {
            get
            {
                return this.GetExportAttribute().ControllerName;
            }
        }

        public DbConnectionStringBuilder CreateConnectionStringBuilder()
        {
            return this.GetExportAttribute().CreateConnectionStringBuilder();
        }

        public DataTable SelectTable(string connectionString, string tableName, int nrows = int.MaxValue)
        {
            using (var conn = ProviderFactory.CreateConnection())
            {
                conn.ConnectionString = connectionString;
                conn.Open();

                var result = new DataTable();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = string.Format("select * from [{0}]", tableName);

                    using (var da = ProviderFactory.CreateDataAdapter())
                    {
                        da.SelectCommand = cmd;
                        da.FillSchema(result, SchemaType.Source);
                    }

                    int n = 0;
                    var items = Array.CreateInstance(typeof(object), result.Columns.Count) as object[];

                    try
                    {
                        using (var rdr = cmd.ExecuteReader())
                        {
                            while (n < nrows && rdr.Read())
                            {
                                rdr.GetValues(items);
                                result.Rows.Add(items);
                                n++;
                            }
                        }
                    }
                    catch (DbException ex)
                    {
                        Logger.Write(ex, System.Diagnostics.TraceEventType.Error,
                            new Dictionary<string, object>()
                            {
                                { "Action", string.Format("Read Table [{0}] failed on data provider: {1}", tableName, Name) },
                                {"sql", cmd.CommandText},
                                {"connectionString", connectionString}, 
                                {"tableName", tableName},
                                {"nRows", nrows}
                            },
                            "Error selecting data from table {0}", tableName);

                        // BUG: this throw is not caught in (some) wizard steps. One cause of the exceptions has been fixed, so to test that
                        // this throw gets caught now, we need to temporarily hard-code divide-by-zero inside the try block above.
                        throw new DataProviderControllerException(
                            string.Format("Read Table [{0}] failed on data provider: {1}", tableName, Name)
                            , ex);
                    }
                }

                return result;
            }
        }
    }

    /// <summary>
    /// OleDbDataProviderController
    /// </summary>
    public class OleDbDataProviderController : DataProviderController<System.Data.OleDb.OleDbFactory>
    {
    }

    public class DataProviderControllerException : Exception
    {
        public DataProviderControllerException(string message, Exception ex)
            : base(message, ex)
        {
        }
    }
}
