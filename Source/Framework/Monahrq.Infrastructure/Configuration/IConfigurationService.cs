using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace Monahrq.Infrastructure.Configuration
{
    /// <summary>
    /// The Monahrq Configuration Service interface / contract.
    /// </summary>
    public interface IConfigurationService
	{
        /// <summary>
        /// Gets the current schema version.
        /// </summary>
        /// <value>
        /// The current schema version.
        /// </value>
        string CurrentSchemaVersion
		{
			get;
		}
        /// <summary>
        /// Gets or sets the connection settings.
        /// </summary>
        /// <value>
        /// The connection settings.
        /// </value>
        ConnectionStringSettings ConnectionSettings
		{
			get;
			set;
		}

        /// <summary>
        /// Gets or sets the win qi connection settings.
        /// </summary>
        /// <value>
        /// The win qi connection settings.
        /// </value>
        ConnectionStringSettings WinQiConnectionSettings { get; set; }

        /// <summary>
        /// Gets or sets the last data folder.
        /// </summary>
        /// <value>
        /// The last data folder.
        /// </value>
        string LastDataFolder
		{
			get;
			set;
		}

        /// <summary>
        /// Gets or sets a value indicating whether [rebuild database].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [rebuild database]; otherwise, <c>false</c>.
        /// </value>
        bool RebuildDatabase
		{
			get;
			set;
		}

        /// <summary>
        /// Gets or sets a value indicating whether [use API for physicians].
        /// </summary>
        /// <value>
        /// <c>true</c> if [use API for physicians]; otherwise, <c>false</c>.
        /// </value>
        bool UseApiForPhysicians
		{
			get;
			set;
		}

        /// <summary>
        /// Gets or sets a value indicating whether [data access components installed].
        /// </summary>
        /// <value>
        /// <c>true</c> if [data access components installed]; otherwise, <c>false</c>.
        /// </value>
        bool DataAccessComponentsInstalled
		{
			get;
			set;
		}
        /// <summary>
        /// Gets or sets the hospital region.
        /// </summary>
        /// <value>
        /// The hospital region.
        /// </value>
        HospitalRegionElement HospitalRegion { get; set; }

        /// <summary>
        /// Saves the specified named connection element.
        /// </summary>
        /// <param name="namedConnectionElement">The named connection element.</param>
        void Save(NamedConnectionElement namedConnectionElement);
        /// <summary>
        /// Saves the specified settings.
        /// </summary>
        /// <param name="settings">The settings.</param>
        void Save(IMonahrqSettings settings);
        /// <summary>
        /// Saves this instance.
        /// </summary>
        void Save();

        /// <summary>
        /// Gets the monahrq settings.
        /// </summary>
        /// <value>
        /// The monahrq settings.
        /// </value>
        IMonahrqSettings MonahrqSettings { get; }

        /// <summary>
        /// Gets the entity provider factory.
        /// </summary>
        /// <value>
        /// The entity provider factory.
        /// </value>
        DbProviderFactory EntityProviderFactory { get; }

        /// <summary>
        /// Gets the default win qi connection string.
        /// </summary>
        /// <value>
        /// The default win qi connection string.
        /// </value>
        ConnectionStringSettings DefaultWinQiConnectionString { get; }

        /// <summary>
        /// Forces the refresh.
        /// </summary>
        void ForceRefresh();

	}


    /// <summary>
    /// The Db Provider Factory Extension Methods class.
    /// </summary>
    public static class DbProviderFactoryExtensions
	{
        /// <summary>
        /// Executes the scalar.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <param name="sql">The SQL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public static object ExecuteScalar(this ConnectionStringSettings settings, IEnumerable<string> sql,
			IEnumerable<KeyValuePair<string, object>> parameters = null)
		{
			return settings.ExecuteScalar(string.Join(Environment.NewLine, sql), parameters);
		}

        /// <summary>
        /// Executes the scalar.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <param name="sql">The SQL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public static object ExecuteScalar(this ConnectionStringSettings settings, StringBuilder sql,
			IEnumerable<KeyValuePair<string, object>> parameters = null)
		{
			return settings.ExecuteScalar(sql.ToString(), parameters);
		}

        /// <summary>
        /// Executes the scalar.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <param name="sql">The SQL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public static object ExecuteScalar(this ConnectionStringSettings settings, string sql,
			IEnumerable<KeyValuePair<string, object>> parameters = null)
		{
			parameters = parameters ?? Enumerable.Empty<KeyValuePair<string, object>>();

			var factory = settings.CreateFactory();

			using (var conn = factory.CreateConnection())
			{
				conn.ConnectionString = settings.ConnectionString;
				conn.Open();
				using (var cmd = conn.CreateCommand())
				{
					cmd.CommandText = sql;
					foreach (var p in parameters)
					{
						var prm = factory.CreateParameter();
						prm.ParameterName = p.Key;
						prm.Value = p.Value;
						cmd.Parameters.Add(prm);

					}
					return cmd.ExecuteScalar();
				}
			}
		}

        /// <summary>
        /// Executes the non query.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <param name="sql">The SQL.</param>
        /// <param name="parameters">The parameters.</param>
        public static void ExecuteNonQuery(this ConnectionStringSettings settings, StringBuilder sql,
			IEnumerable<KeyValuePair<string, object>> parameters = null)
		{
			settings.ExecuteNonQuery(sql.ToString(), parameters);
		}

        /// <summary>
        /// Executes the non query.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <param name="sql">The SQL.</param>
        /// <param name="parameters">The parameters.</param>
        public static void ExecuteNonQuery(this ConnectionStringSettings settings, IEnumerable<string> sql,
			IEnumerable<KeyValuePair<string, object>> parameters = null)
		{
			settings.ExecuteNonQuery(string.Join(Environment.NewLine, sql), parameters);
		}

        /// <summary>
        /// Executes the non query.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <param name="sql">The SQL.</param>
        /// <param name="parameters">The parameters.</param>
        public static void ExecuteNonQuery(this ConnectionStringSettings settings, string sql,
			IEnumerable<KeyValuePair<string, object>> parameters = null)
		{
			parameters = parameters ?? Enumerable.Empty<KeyValuePair<string, object>>();

			var factory = settings.CreateFactory();

			using (var conn = factory.CreateConnection())
			{
				conn.ConnectionString = settings.ConnectionString;
				conn.Open();
				using (var cmd = conn.CreateCommand())
				{
					cmd.CommandTimeout = (int)Math.Round(MonahrqConfiguration.SettingsGroup.MonahrqSettings().LongTimeout.TotalSeconds);
					cmd.CommandText = sql;
					foreach (var p in parameters)
					{
						var prm = factory.CreateParameter();
						prm.ParameterName = p.Key;
						prm.Value = p.Value;
						cmd.Parameters.Add(prm);

					}
					cmd.ExecuteNonQuery();
				}
			}
		}

        /// <summary>
        /// Creates the factory.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns></returns>
        private static DbProviderFactory CreateFactory(this ConnectionStringSettings settings)
		{
			return DbProviderFactories.GetFactory(settings.ProviderName);
		}
	}
}
