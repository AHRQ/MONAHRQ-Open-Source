using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Monahrq.SysTray.trayNotification
{
    /// <summary>
    /// class for SysTrayDbHelper
    /// </summary>
    public class SysTrayDbHelper
    {
        //private readonly IConfigurationService _configService;
        /// <summary>
        /// The connection string
        /// </summary>
        private readonly string _connectionString;

        /// <summary>
        /// Initializes a new instance of the <see cref="SysTrayDbHelper"/> class.
        /// </summary>
        public SysTrayDbHelper()
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="SysTrayDbHelper"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        public SysTrayDbHelper(string connectionString) : this()
        {
            _connectionString = connectionString;
           // Log(_connectionString);
        }

        /// <summary>
        /// Executes the reader.
        /// </summary>
        /// <param name="sqlStatement">The SQL statement.</param>
        /// <param name="paramters">The paramters.</param>
        /// <returns></returns>
        public IEnumerable<IDataRecord> ExecuteReader(string sqlStatement, IDictionary<string, object> paramters = null)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand(sqlStatement, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.CommandTimeout = 10000;
                    command.Connection.Open();

                    if (paramters != null && paramters.Any())
                    {
                        foreach (var p in paramters.ToList())
                        {
                            var paramName = p.Key.StartsWith("@") ? p.Key : "@" + p.Key;

                            if (!command.Parameters.Contains(paramName))
                                command.Parameters.Add(new SqlParameter(paramName, p.Value));
                        }
                    }

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                            yield return reader;
                    }

                    command.Connection.Close();
                }
            }
        }


        /// <summary>
        /// Executes the scalar.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sqlStatement">The SQL statement.</param>
        /// <param name="paramters">The paramters.</param>
        /// <returns></returns>
        public T ExecuteScalar<T> (string sqlStatement, IDictionary<string, object> paramters = null)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand(sqlStatement, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.CommandTimeout = 10000;
                    command.Connection.Open();

                    if (paramters != null && paramters.Any())
                    {
                        foreach (var p in paramters.ToList())
                        {
                            var paramName = p.Key.StartsWith("@") ? p.Key : "@" + p.Key;

                            if (!command.Parameters.Contains(paramName))
                                command.Parameters.Add(new SqlParameter(paramName, p.Value));
                        }
                    }

                    return (T)command.ExecuteScalar();

                }
            }
        }

        /// <summary>
        /// Executes the data table.
        /// </summary>
        /// <param name="sqlStatement">The SQL statement.</param>
        /// <param name="paramters">The paramters.</param>
        /// <returns></returns>
        public DataTable ExecuteDataTable(string sqlStatement, IDictionary<string, object> paramters = null)
        {
            var dtResult = new DataTable();

            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand(sqlStatement, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.CommandTimeout = 100000;
                    command.Connection.Open();
                    
                    if (paramters != null && paramters.Any())
                    {
                        foreach (var p in paramters.ToList())
                        {
                            var paramName = p.Key.StartsWith("@") ? p.Key : "@" + p.Key;

                            if (!command.Parameters.Contains(paramName))
                                command.Parameters.Add(new SqlParameter(paramName, p.Value));
                        }
                    }

                    using (var adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(dtResult);
                    }
                    
                }
                connection.Close();
            }

            return dtResult;
        }

        /// <summary>
        /// Executes the non query.
        /// </summary>
        /// <param name="sqlStatement">The SQL statement.</param>
        /// <param name="paramters">The paramters.</param>
        /// <returns></returns>
        public bool ExecuteNonQuery(string sqlStatement, IDictionary<string, object> paramters = null)
        {
            bool returnValue;
            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand(sqlStatement, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.CommandTimeout = 100000;
                    command.Connection.Open();

                    if (paramters != null && paramters.Any())
                    {
                        foreach (var p in paramters.ToList())
                        {
                            var paramName = p.Key.StartsWith("@") ? p.Key : "@" + p.Key;

                            if (!command.Parameters.Contains(paramName))
                                command.Parameters.Add(new SqlParameter(paramName, p.Value));
                        }
                    }

                    returnValue = command.ExecuteNonQuery() > 0;
                }
                connection.Close();
            }
            return returnValue;
        }

        //private static void Log(string message)
        //{
        //    using (var file = new StreamWriter(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Monahrq.SysTray.log")))
        //    {
        //        file.WriteLine(message);
        //        file.Close();
        //    }
        //}
    }
}
