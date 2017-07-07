using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Monahrq.Resources
{
    /// <summary>
    /// Class to execute the finalize script in the database
    /// </summary>
    class FinalizeScript
    {
        /// <summary>
        /// The connection
        /// </summary>
        private IDbConnection _connection;

        /// <summary>
        /// Initializes a new instance of the <see cref="FinalizeScript"/> class.
        /// </summary>
        /// <param name="dbConnection">The database connection.</param>
        public FinalizeScript(IDbConnection dbConnection)
        {
            // TODO: Complete member initialization
            _connection = dbConnection;
        }


        /// <summary>
        /// Executes this database command.
        /// </summary>
        public void Execute()
        {
            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = TheScript;
                cmd.CommandTimeout = 3000;
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Gets the script.
        /// </summary>
        /// <value>
        /// The script.
        /// </value>
        public string TheScript 
        {
            get
            {
                var str = Assembly.GetExecutingAssembly().GetManifestResourceStream(this.GetType(), "finalizedb.sql");
                return new StreamReader(str).ReadToEnd();
            }
        }
    }
}
